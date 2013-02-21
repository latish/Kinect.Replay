using System;
using System.IO;
using Microsoft.Kinect;

namespace Kinect.Replay.Record
{
	internal class DepthRecorder
	{
		private DateTime referenceTime;
		private readonly BinaryWriter writer;

		internal DepthRecorder(BinaryWriter writer)
		{
			this.writer = writer;
			referenceTime = DateTime.Now;
		}

		public void Record(DepthImageFrame frame)
		{
			writer.Write((int) FrameType.Depth);

			var timeSpan = DateTime.Now.Subtract(referenceTime);
			referenceTime = DateTime.Now;
			writer.Write((long) timeSpan.TotalMilliseconds);
			writer.Write(frame.BytesPerPixel);
			writer.Write((int) frame.Format);
			writer.Write(frame.Width);
			writer.Write(frame.Height);

			writer.Write(frame.FrameNumber);

			var shorts = new short[frame.PixelDataLength];
			frame.CopyPixelDataTo(shorts);
			writer.Write(shorts.Length);
			foreach (var s in shorts)
				writer.Write(s);
		}
	}
}