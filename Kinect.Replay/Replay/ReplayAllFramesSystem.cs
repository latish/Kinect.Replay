using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kinect.Replay.Record;
using Kinect.Replay.Replay.Color;
using Kinect.Replay.Replay.Depth;
using Kinect.Replay.Replay.Skeletons;

namespace Kinect.Replay.Replay
{
	internal class ReplayAllFramesSystem 
	{
		protected readonly List<ReplayAllFrames> frames = new List<ReplayAllFrames>();
		private CancellationTokenSource cancellationTokenSource;
		internal event Action<ReplayAllFrames> FrameReady;
		public event Action ReplayFinished;

		public bool IsFinished { get; private set; }

		public void Start()
		{
			Stop();
			IsFinished = false;
			cancellationTokenSource = new CancellationTokenSource();
			var token = cancellationTokenSource.Token;
			Task.Factory.StartNew(() =>
				                      {
					                      foreach (var frame in frames)
					                      {
						                      Thread.Sleep(TimeSpan.FromMilliseconds(frame.TimeStamp));
						                      if (token.IsCancellationRequested)
							                      break;
						                      if (FrameReady != null)
							                      FrameReady(frame);
					                      }
					                      IsFinished = true;
					                      ReplayFinished.Raise();
				                      }, token);
		}

		public void Stop()
		{
			if (cancellationTokenSource == null)
				return;
			cancellationTokenSource.Cancel();
		}

		internal void AddFrames(BinaryReader reader)
		{
            //not the best of approaches - assuming that color frame is the 1st frame followed by depth and skeleton frame
			while (reader.BaseStream.Position != reader.BaseStream.Length)
			{
				var header = (FrameType) reader.ReadInt32();
				switch (header)
				{
					case FrameType.Color:
						var colorFrame = new ReplayColorImageFrame();
						colorFrame.CreateFromReader(reader);
						frames.Add(new ReplayAllFrames {ColorImageFrame = colorFrame});
						break;
					case FrameType.Depth:
						if (frames.Any())
						{
							var depthFrame = new ReplayDepthImageFrame();
							depthFrame.CreateFromReader(reader);
							frames.Last().DepthImageFrame = depthFrame;
						}
						break;
					case FrameType.Skeletons:
						if (frames.Any())
						{
							var skeletonFrame = new ReplaySkeletonFrame();
							skeletonFrame.CreateFromReader(reader);
							frames.Last().SkeletonFrame = skeletonFrame;
						}
						break;
				}
			}
		}
	}
}