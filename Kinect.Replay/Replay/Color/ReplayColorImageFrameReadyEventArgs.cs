using System;

namespace Kinect.Replay.Replay.Color
{
	public class ReplayColorImageFrameReadyEventArgs : EventArgs
	{
		public ReplayColorImageFrame ColorImageFrame { get; set; }
	}
}