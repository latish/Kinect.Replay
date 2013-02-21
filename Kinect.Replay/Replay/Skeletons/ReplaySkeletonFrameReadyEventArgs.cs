using System;

namespace Kinect.Replay.Replay.Skeletons
{
	public class ReplaySkeletonFrameReadyEventArgs : EventArgs
	{
		public ReplaySkeletonFrame SkeletonFrame { get; set; }
	}
}