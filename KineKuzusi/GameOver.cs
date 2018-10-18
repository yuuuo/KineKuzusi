using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Kinect.Toolbox;
using System.IO;
using Microsoft.Kinect;


namespace KineKuzusi
{
    public partial class GameOver : UserControl
    {
        int score;
        List<string> stringRanking = new List<string>();
        List<int> intRanking = new List<int>();

        bool once = true;
        Tools tools = new Tools();
        Timer timer = new Timer();
        SwipeGestureDetector swipeDetector = new SwipeGestureDetector();

        public GameOver()
        {
            InitializeComponent();

            stringRanking.AddRange(File.ReadAllText(@"Scores.csv").Split(','));
            stringRanking.RemoveAll(s => s == "");
            intRanking = stringRanking.ConvertAll(s => int.Parse(s));

            score = intRanking[intRanking.Count - 1];
            intRanking.Sort(); intRanking.Reverse();

            StartKinect(KinectSensor.KinectSensors[0]);
            swipeDetector.OnGestureDetected += new Action<string>(swipe_event);
        }

        private void swipe_event(string obj)
        {
            if (once)
            {
                once = false;
                Dispose();
            }
        }

        private void StartKinect(KinectSensor kinect)
        {
            //FormMain.FormMainInstance.SetDesktopBounds(0, 0, 1440, 810);
            //SetBounds(0, 0, 1440, 810);
            Form f = FindForm();
            if (f != null) f.SetDesktopBounds(0, 0, 1440, 810);

            //スケルトンフレームを有効にし、イベントを登録する
            kinect.SkeletonStream.Enable();
            kinect.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(skeleton_update);

            //スワイプジェスチャーのイベント登録
            //swipeDetector.OnGestureDetected += new Action<string>(swipe_event);

            kinect.Start();
        }

        private void skeleton_update(object sender, SkeletonFrameReadyEventArgs e)
        {

            KinectSensor kinect = sender as KinectSensor;

            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    Skeleton[] skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    skeletonFrame.CopySkeletonDataTo(skeletons);
                    foreach (Skeleton skeleton in skeletons)
                    {
                        Joint handLeft = skeleton.Joints[JointType.HandLeft];
                        Joint handRight = skeleton.Joints[JointType.HandRight];

                        if (skeleton.TrackingState == SkeletonTrackingState.Tracked
                            && handRight.TrackingState == JointTrackingState.Tracked)
                        {
                            swipeDetector.Add(handRight.Position, kinect);
                        }
                    }
                }
            }
        }

        private void Draw(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawString("あなたの得点", new Font("HGSSoeiKakupoptai Regular", 70), Brushes.CadetBlue, Width / 5, Height * 1 / 5);
            e.Graphics.DrawString(score.ToString(), new Font("HGSSoeiKakupoptai Regular", 70), Brushes.MediumVioletRed, Width *15/ 50, Height * 2 / 5);


            e.Graphics.DrawString("ランキング", new Font("HGSSoeiKakupoptai Regular", 70), Brushes.CadetBlue, (float)(Width / 1.5), Height/20);
            for (int i = 1; i <= (intRanking.Count < 8 ? intRanking.Count : 8); i++)
            {
                e.Graphics.DrawString(i.ToString()+"位"+" : " + intRanking[i-1].ToString(), new Font("HGSSoeiKakupoptai Regular", 70), Brushes.CadetBlue, (float)(Width/1.5), Height / 20 * (i*2 + 1));
            }
        }
    }
}
