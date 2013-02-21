using System;

namespace Kinect.Replay.Replay.Skeletons
{
	public class ReplayAllFramesReadyEventArgs : EventArgs
	{
		public ReplayAllFrames AllFrames { get; set; }
	}
}