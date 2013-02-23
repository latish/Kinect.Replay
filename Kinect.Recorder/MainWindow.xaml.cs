using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Kinect.Replay.Record;
using Kinect.Replay.Replay;
using Kinect.Replay.Replay.Color;
using Kinect.Replay.Replay.Skeletons;
using Microsoft.Kinect;
using Microsoft.Win32;

namespace Kinect.Recorder
{
	public partial class MainWindow : Window, INotifyPropertyChanged
	{
		private const KinectRecordOptions RecordOptions = KinectRecordOptions.Frames | KinectRecordOptions.Audio;
		private KinectSensor _kinectSensor;
		private string _message;
		private WriteableBitmap _imageSource;
		private Skeleton[] _skeletons;
		private Dictionary<JointType, Ellipse> _ellipses;
		private KinectRecorder recorder;
		private KinectReplay replay;
		private bool _isRecording;
		private bool _isReplaying;
		private bool _startedAudio;
		private SoundPlayer _soundPlayer;

		public MainWindow()
		{
			InitializeComponent();
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public string Message
		{
			get { return _message; }
			set
			{
				if (value.Equals(_message)) return;
				_message = value;
				PropertyChanged.Raise(() => Message);
			}
		}

		public WriteableBitmap ImageSource
		{
			get { return _imageSource; }
			set
			{
				if (value.Equals(_imageSource)) return;
				_imageSource = value;
				PropertyChanged.Raise(() => ImageSource);
			}
		}

		public bool IsRecording
		{
			get { return _isRecording; }
			set
			{
				if (value.Equals(_isRecording)) return;
				_isRecording = value;
				PropertyChanged.Raise(() => IsRecording);
			}
		}

		public bool IsReplaying
		{
			get { return _isReplaying; }
			set
			{
				if (value.Equals(_isReplaying)) return;
				_isReplaying = value;
				PropertyChanged.Raise(() => IsReplaying);
			}
		}

		private void MainWindowLoaded(object sender, RoutedEventArgs e)
		{
			try
			{
				KinectSensor.KinectSensors.StatusChanged += KinectSensorsStatusChanged;
				_ellipses = new Dictionary<JointType, Ellipse>();

				_kinectSensor = KinectSensor.KinectSensors.FirstOrDefault(sensor => sensor.Status == KinectStatus.Connected);
				if (_kinectSensor == null)
					Message = "No Kinect found on startup";
				else
					Initialize();
			}
			catch (Exception ex)
			{
				Message = ex.Message;
			}
		}

		void KinectSensorsStatusChanged(object sender, StatusChangedEventArgs e)
		{
			switch (e.Status)
			{
				case KinectStatus.Disconnected:
					if (_kinectSensor == e.Sensor)
					{
						Clean();
						Message = "Kinect disconnected";
					}
					break;
				case KinectStatus.Connected:
					_kinectSensor = e.Sensor;
					Initialize();
					break;
				case KinectStatus.NotPowered:
					Message = "Kinect is not powered";
					Clean();
					break;
				case KinectStatus.NotReady:
					Message = "Kinect is not ready";
					break;
				case KinectStatus.Initializing:
					Message = "Initializing";
					break;
				default:
					Message = string.Concat("Status: ", e.Status);
					break;
			}
		}

		private void Clean()
		{
			if (recorder != null && IsRecording)
				recorder.Stop();
			if (replay != null)
			{
				replay.Stop();
				replay.Dispose();
			}
			if (_kinectSensor == null)
				return;
			if (_kinectSensor.IsRunning)
				_kinectSensor.Stop();
			_kinectSensor.AllFramesReady -= KinectSensorAllFramesReady;
			_kinectSensor.Dispose();
			_kinectSensor = null;
		}

		private void Initialize()
		{
			if (_kinectSensor == null)
				return;
			_kinectSensor.AllFramesReady += KinectSensorAllFramesReady;
			_kinectSensor.ColorStream.Enable();
			_kinectSensor.SkeletonStream.Enable();
			_kinectSensor.DepthStream.Enable();
			_kinectSensor.Start();
			_kinectSensor.AudioSource.Start();
			Message = "Kinect connected";
		}

		void KinectSensorAllFramesReady(object sender, AllFramesReadyEventArgs e)
		{
			if (replay != null && !replay.IsFinished)
				return;

			using (var frame = e.OpenColorImageFrame())
			{
				if (frame != null)
				{
					if (recorder != null)
						recorder.Record(frame);
					UpdateColorFrame(frame);
				}
			}

			using (var frame = e.OpenDepthImageFrame())
			{
				if (frame != null)
				{
					if (recorder != null)
						recorder.Record(frame);
				}
			}

			using (var frame = e.OpenSkeletonFrame())
			{
				if (frame != null)
				{
					if (recorder != null)
						recorder.Record(frame);
					UpdateSkeletons(frame);
				}
			}
		}

		private void UpdateSkeletons(ReplaySkeletonFrame frame)
		{
			_skeletons = frame.Skeletons;
			var trackedSkeleton = _skeletons.FirstOrDefault(s => s.TrackingState == SkeletonTrackingState.Tracked);

			if (trackedSkeleton == null)
				return;

			DrawJoints(trackedSkeleton);
		}

		private void UpdateColorFrame(ReplayColorImageFrame frame)
		{
			var pixelData = new byte[frame.PixelDataLength];
			frame.CopyPixelDataTo(pixelData);
			if (ImageSource == null)
				ImageSource = new WriteableBitmap(frame.Width, frame.Height, 96, 96,
															 PixelFormats.Bgr32, null);

			var stride = frame.Width * PixelFormats.Bgr32.BitsPerPixel / 8;
			ImageSource.WritePixels(new Int32Rect(0, 0, frame.Width, frame.Height), pixelData, stride, 0);
		}

		private void DrawJoints(Skeleton skeleton)
		{
			foreach (var name in Enum.GetNames(typeof(JointType)))
			{
				var jointType = (JointType)Enum.Parse(typeof(JointType), name);
				var coordinateMapper = (_kinectSensor != null && _kinectSensor.Status == KinectStatus.Connected) ? new CoordinateMapper(_kinectSensor) : replay.CoordinateMapper;
				var joint = skeleton.Joints[jointType];

				var skeletonPoint = joint.Position;
				if (joint.TrackingState == JointTrackingState.NotTracked)
					continue;

				var colorPoint = coordinateMapper.MapSkeletonPointToColorPoint(skeletonPoint, ColorImageFormat.RgbResolution640x480Fps30);
				if (!_ellipses.ContainsKey(jointType))
				{
					_ellipses[jointType] = new Ellipse { Width = 20, Height = 20, Fill = Brushes.SandyBrown };
					SkeletonCanvas.Children.Add(_ellipses[jointType]);
				}
				Canvas.SetLeft(_ellipses[jointType], colorPoint.X - _ellipses[jointType].Width / 2);
				Canvas.SetTop(_ellipses[jointType], colorPoint.Y - _ellipses[jointType].Height / 2);
			}
		}

		protected virtual void OnPropertyChanged(string propertyName)
		{
			var handler = PropertyChanged;
			if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
		}

		private void RecordClick(object sender, RoutedEventArgs e)
		{
			if (IsRecording)
			{
				recorder.Stop();
				recorder = null;
				IsRecording = false;
				Message = "";
				return;
			}
			var saveFileDialog = new SaveFileDialog { Title = "Select filename", Filter = "Replay files|*.replay" };
			if (saveFileDialog.ShowDialog() != true) return;

			recorder = new KinectRecorder(RecordOptions, saveFileDialog.FileName, _kinectSensor);
			Message = string.Format("Recording {0}", RecordOptions.ToString());
			recorder.StartAudioRecording();
			IsRecording = true;
		}

		private void ReplayClick(object sender, RoutedEventArgs e)
		{
			if (IsReplaying)
			{
				CleanupReplay();
				Message = "";
				return;
			}
			_startedAudio = false;
			var openFileDialog = new OpenFileDialog { Title = "Select filename", Filter = "Replay files|*.replay" };

			if (openFileDialog.ShowDialog() == true)
			{
				replay = new KinectReplay(openFileDialog.FileName);
                Message = string.Format("Replaying {0}", RecordOptions.ToString());
				replay.AllFramesReady += ReplayAllFramesReady;
				replay.ReplayFinished += CleanupReplay;
				replay.Start();
			}
			IsReplaying = true;
		}

		private void CleanupReplay()
		{
			if (!IsReplaying) return;
			Message = "";
            if(_soundPlayer!=null && _startedAudio)
                _soundPlayer.Stop();
			replay.AllFramesReady -= ReplayAllFramesReady;
			replay.Stop();
			replay.Dispose();
			replay = null;
			IsReplaying = false;
		}

		void ReplayAllFramesReady(ReplayAllFramesReadyEventArgs e)
		{
			if ((replay.Options & KinectRecordOptions.Audio) != 0 && !_startedAudio)
			{
				_soundPlayer = new SoundPlayer(replay.AudioFilePath);
				_soundPlayer.Play();
				_startedAudio = true;
			}

			var colorImageFrame = e.AllFrames.ColorImageFrame;
			if (colorImageFrame != null)
				UpdateColorFrame(colorImageFrame);

			var skeletonFrame = e.AllFrames.SkeletonFrame;
			if (skeletonFrame != null)
				UpdateSkeletons(skeletonFrame);
		}
	}
}