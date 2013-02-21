using System;
using Microsoft.Kinect;

namespace Kinect.Replay
{
	public static class Extensions
	{
		public static Skeleton[] GetSkeletons(this SkeletonFrame frame)
		{
			if (frame == null)
				return null;

			var skeletons = new Skeleton[frame.SkeletonArrayLength];
			frame.CopySkeletonDataTo(skeletons);

			return skeletons;
		}
		public static void Raise(this Action proc)
		{
			var p = proc;
			if (p != null) p();
		}

		public static void Raise<T>(this Action<T> proc, T argument)
		{
			var p = proc;
			if (p != null) p(argument);
		}

		public static void Raise<T, U>(this Action<T, U> proc, T t, U u)
		{
			var p = proc;
			if (p != null) p(t, u);
		}

	}
}