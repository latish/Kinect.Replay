using System;
using System.IO;
using System.Linq;
using Microsoft.Kinect;

namespace Kinect.Replay.Record
{
	public class KinectRecorder
	{
		private Stream recordStream;
		private string recordFileName;
		private readonly KinectSensor _sensor;
		private readonly BinaryWriter writer;

		private DateTime previousFlushDate;

		private readonly ColorRecorder colorRecoder;
		private readonly DepthRecorder depthRecorder;
		private readonly SkeletonRecorder skeletonRecorder;
		private readonly AudioRecorder audioRecorder;

		public KinectRecordOptions Options { get; set; }

		public KinectRecorder(KinectRecordOptions options, string targetFileName, KinectSensor sensor)
		{
			Options = options;
			recordFileName = targetFileName;
			_sensor = sensor;
			var stream = File.Create(targetFileName);
			recordStream = stream;
			writer = new BinaryWriter(recordStream);

			writer.Write((int)Options);
			var colorToDepthRelationalParameters = sensor.CoordinateMapper.ColorToDepthRelationalParameters.ToArray();
			writer.Write(colorToDepthRelationalParameters.Length);
			writer.Write(colorToDepthRelationalParameters);

			if ((Options & KinectRecordOptions.Frames) != 0)
			{
				colorRecoder = new ColorRecorder(writer);
				depthRecorder = new DepthRecorder(writer);
				skeletonRecorder = new SkeletonRecorder(writer);
			}

			if ((Options & KinectRecordOptions.Audio) != 0)
				audioRecorder = new AudioRecorder();

			previousFlushDate = DateTime.Now;
		}

		public void Record(SkeletonFrame frame)
		{
			if (skeletonRecorder == null)
				return;
			if (writer == null)
				throw new Exception("This recorder is stopped");

			skeletonRecorder.Record(frame);
			Flush();
		}

		public void Record(ColorImageFrame frame)
		{
			if (colorRecoder == null)
				return;
			if (writer == null)
				throw new Exception("This recorder is stopped");

			colorRecoder.Record(frame);
			Flush();
		}

		public void Record(DepthImageFrame frame)
		{
			if (depthRecorder == null)
				return;
			if (writer == null)
				throw new Exception("This recorder is stopped");

			depthRecorder.Record(frame);
			Flush();
		}

		public void StartAudioRecording()
		{
			if (audioRecorder == null || audioRecorder.IsRunning)
				return;
			audioRecorder.RecordDefaultDeviceAudio();
		}

		private void Flush()
		{
			var now = DateTime.Now;

			if (now.Subtract(previousFlushDate).TotalSeconds > 60)
			{
				previousFlushDate = now;
				writer.Flush();
			}
		}

		public void Stop()
		{
			if (writer == null)
				throw new Exception("This recorder is already stopped");

			if (audioRecorder != null && audioRecorder.IsRunning)
				audioRecorder.StopDefaultAudioRecording(Path.ChangeExtension(recordFileName, ".wav"));

			writer.Close();
			writer.Dispose();

			recordStream.Dispose();
			recordStream = null;
		}

	}
}