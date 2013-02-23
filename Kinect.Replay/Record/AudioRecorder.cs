using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Microsoft.Kinect;

namespace Kinect.Replay.Record
{
	/// <summary>
	/// 
	/// </summary>
    ///<remarks>
    /// This whole thing is a hack right now. Couldn't record from Kinect Audio Stream on my VMWare Fusion VM, 
    /// so recording from default audio source via native API calls right now
    /// </remarks>
	public class AudioRecorder
	{
		public bool IsRunning { get; private set; }
		private KinectSensor kinectSensor;
		private DateTime recordingStartTime;
		private Thread workingThread;
		private MemoryStream contentMemoryStream;

		public void RecordDefaultDeviceAudio()
		{
			IsRunning = true;
			mciSendString("open new Type waveaudio Alias recsound", "", 0, 0);
			mciSendString("record recsound", "", 0, 0);
		}

		public void StopDefaultAudioRecording(string audioFilePath)
		{
			mciSendString(string.Format("save recsound {0}", audioFilePath), "", 0, 0);
			mciSendString("close recsound ", "", 0, 0);
			IsRunning = false;
		}

		[DllImport("winmm.dll", EntryPoint = "mciSendStringA", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
		private static extern int mciSendString(string lpstrCommand, string lpstrReturnString, int uReturnLength, int hwndCallback);

		#region Todo
		public void Record()
		{
			if (IsRunning)
				throw new Exception("AudioRecorder is already Recording");

			IsRunning = true;
			recordingStartTime = DateTime.Now;
			workingThread = new Thread(RecordAudio) { IsBackground = true };
			workingThread.Start();
		}

		void RecordAudio(object o)
		{
			var source = kinectSensor.AudioSource;

			contentMemoryStream = null;
			var buffer = new byte[1024];

			using (var sourceStream = source.Start())
			{
				while (IsRunning)
				{
					if (contentMemoryStream == null)
						contentMemoryStream = new MemoryStream();
					int count;
					while ((count = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
						contentMemoryStream.Write(buffer, 0, count);
				}
			}
		}

		private void SaveAudioToFile(string audioFilePath)
		{
			if (contentMemoryStream == null) return;

			var recordingDuration = DateTime.Now.Subtract(recordingStartTime).TotalSeconds;
			var recordingLength = (int)recordingDuration * 2 * 16000;
			using (var headerMemoryStream = new MemoryStream())
			{
				WriteWavHeader(headerMemoryStream, recordingLength);
				using (var fileStream = new FileStream(audioFilePath, FileMode.Create))
				{
					headerMemoryStream.WriteTo(fileStream);
					contentMemoryStream.WriteTo(fileStream);
					contentMemoryStream.Dispose();
				}
			}
		}

		public void Stop(string audioFilePath)
		{
			IsRunning = false;
			SaveAudioToFile(audioFilePath);
		}

		/// <summary>
		/// A bare bones WAV file header writer
		/// </summary>        
		static void WriteWavHeader(Stream stream, int dataLength)
		{
			//We need to use a memory stream because the BinaryWriter will close the underlying stream when it is closed
			using (var memStream = new MemoryStream(64))
			{
				const int cbFormat = 18; //sizeof(WAVEFORMATEX)
				var format = new WaveFormatEx
				{
					wFormatTag = 1,
					nChannels = 1,
					nSamplesPerSec = 16000,
					nAvgBytesPerSec = 32000,
					nBlockAlign = 2,
					wBitsPerSample = 16,
					cbSize = 0
				};

				using (var bw = new BinaryWriter(memStream))
				{
					//RIFF header
					WriteString(memStream, "RIFF");
					bw.Write(dataLength + cbFormat + 4); //File size - 8
					WriteString(memStream, "WAVE");
					WriteString(memStream, "fmt ");
					bw.Write(cbFormat);

					//WAVEFORMATEX
					bw.Write(format.wFormatTag);
					bw.Write(format.nChannels);
					bw.Write(format.nSamplesPerSec);
					bw.Write(format.nAvgBytesPerSec);
					bw.Write(format.nBlockAlign);
					bw.Write(format.wBitsPerSample);
					bw.Write(format.cbSize);

					//data header
					WriteString(memStream, "data");
					bw.Write(dataLength);
					memStream.WriteTo(stream);
				}
			}
		}

		static void WriteString(Stream stream, string s)
		{
			byte[] bytes = Encoding.ASCII.GetBytes(s);
			stream.Write(bytes, 0, bytes.Length);
		}

		struct WaveFormatEx
		{
			public ushort wFormatTag;
			public ushort nChannels;
			public uint nSamplesPerSec;
			public uint nAvgBytesPerSec;
			public ushort nBlockAlign;
			public ushort wBitsPerSample;
			public ushort cbSize;
		}
		#endregion
	}
}