using System;
using System.IO;
using Kinect.Replay.Replay.Color;
using Kinect.Replay.Replay.Depth;
using Kinect.Replay.Replay.Skeletons;

namespace Kinect.Replay.Replay
{
	public class ReplayAllFrames : ReplayFrame
	{
		public ReplayColorImageFrame ColorImageFrame { get; set; }
		public ReplayDepthImageFrame DepthImageFrame { get; set; }
		public ReplaySkeletonFrame SkeletonFrame { get; set; }

		public override int FrameNumber
		{
			get { return ColorImageFrame.FrameNumber; }
		}

		public override long TimeStamp
		{
			get { return ColorImageFrame.TimeStamp; }
		}

		internal override void CreateFromReader(BinaryReader reader)
		{
			throw new NotImplementedException();
		}
	}
}