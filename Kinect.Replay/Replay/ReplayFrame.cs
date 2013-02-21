using System.IO;

namespace Kinect.Replay.Replay
{
	public abstract class ReplayFrame
	{
		public virtual int FrameNumber { get; protected set; }
		public virtual long TimeStamp { get; protected set; }

		internal abstract void CreateFromReader(BinaryReader reader);
	}
}