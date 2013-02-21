using System;

namespace Kinect.Replay.Replay.Depth
{
	public class ReplayDepthImageFrameReadyEventArgs : EventArgs
	{
		public ReplayDepthImageFrame DepthImageFrame { get; set; }
	}
}