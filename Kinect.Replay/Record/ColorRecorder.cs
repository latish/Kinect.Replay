using System;
using System.IO;
using Microsoft.Kinect;

namespace Kinect.Replay.Record
{
	internal class ColorRecorder
	{
		private DateTime referenceTime;
		private readonly BinaryWriter writer;

		internal ColorRecorder(BinaryWriter writer)
		{
			this.writer = writer;
			referenceTime = DateTime.Now;
		}

		public void Record(ColorImageFrame frame)
		{
			writer.Write((int) FrameType.Color);

			var timeSpan = DateTime.Now.Subtract(referenceTime);
			referenceTime = DateTime.Now;
			writer.Write((long) timeSpan.TotalMilliseconds);
			writer.Write(frame.BytesPerPixel);
			writer.Write((int) frame.Format);
			writer.Write(frame.Width);
			writer.Write(frame.Height);

			writer.Write(frame.FrameNumber);

			writer.Write(frame.PixelDataLength);
			var bytes = new byte[frame.PixelDataLength];
			frame.CopyPixelDataTo(bytes);
			writer.Write(bytes);
		}
	}
}