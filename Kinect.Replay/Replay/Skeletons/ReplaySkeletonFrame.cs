using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.Kinect;

namespace Kinect.Replay.Replay.Skeletons
{
	public class ReplaySkeletonFrame : ReplayFrame
	{
		public Tuple<float, float, float, float> FloorClipPlane { get; private set; }
		public Skeleton[] Skeletons { get; private set; }
		public SkeletonTrackingMode TrackingMode { get; set; }

		public ReplaySkeletonFrame(SkeletonFrame frame)
		{
			FloorClipPlane = frame.FloorClipPlane;
			FrameNumber = frame.FrameNumber;
			TimeStamp = frame.Timestamp;
			Skeletons = frame.GetSkeletons();
			TrackingMode = frame.TrackingMode;
		}

		public ReplaySkeletonFrame()
		{
		}

		internal override void CreateFromReader(BinaryReader reader)
		{
			TimeStamp = reader.ReadInt64();
			TrackingMode = (SkeletonTrackingMode)reader.ReadInt32();
			FloorClipPlane = new Tuple<float, float, float, float>(
				reader.ReadSingle(), reader.ReadSingle(),
				reader.ReadSingle(), reader.ReadSingle());

			FrameNumber = reader.ReadInt32();

			var formatter = new BinaryFormatter();
			Skeletons = (Skeleton[])formatter.Deserialize(reader.BaseStream);
		}

		public static implicit operator ReplaySkeletonFrame(SkeletonFrame frame)
		{
			return new ReplaySkeletonFrame(frame);
		}
	}
}