using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.Kinect;

namespace Kinect.Replay.Record
{
	internal class SkeletonRecorder
	{
		private DateTime referenceTime;
		private readonly BinaryWriter writer;

		internal SkeletonRecorder(BinaryWriter writer)
		{
			this.writer = writer;
			referenceTime = DateTime.Now;
		}

		public void Record(SkeletonFrame frame)
		{
			writer.Write((int) FrameType.Skeletons);

			var timeSpan = DateTime.Now.Subtract(referenceTime);
			referenceTime = DateTime.Now;
			writer.Write((long) timeSpan.TotalMilliseconds);
			writer.Write((int) frame.TrackingMode);
			writer.Write(frame.FloorClipPlane.Item1);
			writer.Write(frame.FloorClipPlane.Item2);
			writer.Write(frame.FloorClipPlane.Item3);
			writer.Write(frame.FloorClipPlane.Item4);

			writer.Write(frame.FrameNumber);

			var skeletons = frame.GetSkeletons();
			frame.CopySkeletonDataTo(skeletons);

			var formatter = new BinaryFormatter();
			formatter.Serialize(writer.BaseStream, skeletons);
		}
	}
}