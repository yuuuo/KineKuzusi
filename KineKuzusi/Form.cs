using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Kinect;
using System.Windows;
using Kinect.Toolbox;
using Rectangle = System.Drawing.Rectangle;

namespace KineKuzusi
{
    class Ball
    {
        public SolidBrush Brush { get; set; }
        public Vector Position { get; set; }
        public Vector Speed { get; set; }
        public int Radius { get; set; }
        public bool IsPenetrated { get; set; }

        public Ball(SolidBrush brush, Vector position, Vector speed, int radius, bool isPenetrated)
        {
            Brush = brush;
            Position = position;
            Speed = speed;
            Radius = radius;
            IsPenetrated = isPenetrated;
        }

        public void ReverseSpeedX()
        {
            Speed = new Vector(Speed.X * -1, Speed.Y);
        }

        public void ReverseSpeedY()
        {
            Speed = new Vector(Speed.X, Speed.Y * -1);
        }
    }

    class Paddle
    {
        public SolidBrush Brush { get; set; }
        public Rectangle Size { get; set; }
        public int MinimumWidth { get; set; }
        public int MaximumWidth { get; set; }

        public Paddle(SolidBrush brush, Rectangle size, int minimum, int maximum)
        {
            Brush = brush;
            Size = size;
            MinimumWidth = minimum;
            MaximumWidth = maximum;
        }

        public void ReSizeWidth(int PaddleWidth)
        {
            Size = new Rectangle(Size.X, Size.Y, PaddleWidth, Size.Height);
        }
    }

    class Blocks
    {
        public Rectangle Size { get; set; }
        public Vector BlockInterval { get; set; }
        public int Row { get; set; }
        public int Column { get; set; }
        public int[,] DurabilityMatrix{ get; set; }

        public Blocks(Rectangle size, Vector blockInterval, int row, int column, int[,] durabilityMatrix)
        {
            Size = size;
            BlockInterval = blockInterval;
            Row = row;
            Column = column;
            DurabilityMatrix= durabilityMatrix;
        }
        public Rectangle BlockPosition(int row, int column)
        {
            int x = Size.X + ( (int)BlockInterval.X + Size.Width) * row;
            int y = Size.Y + ( (int)BlockInterval.Y + Size.Height) * column;
            return  new Rectangle(x, y ,Size.Width, Size.Height);
        }
    }

    public partial class Form : System.Windows.Forms.Form
    {
        //グローバル変数群
        Paddle paddle;
        Ball ball;
        Blocks blocks;

        Rectangle leftWall;
        Rectangle rightWall;

        //List<Rectangle> blocks;
        Timer timer = new Timer();

        SwipeGestureDetector swipeDetector = new SwipeGestureDetector();

        //コンストラクタ
        public Form()
        {
            InitializeComponent();

            //グローバル変数の初期化

            paddle = new Paddle(
                     new SolidBrush(Color.DimGray),
                     new Rectangle(Width / 2, Height * 8 / 10, Width / 8, Height / 20),
                     Width / 7,
                     Width / 3
            );

            ball = new Ball(
                   new SolidBrush(Color.HotPink),
                   new Vector(Width/2 + Width/16, Height * 8/10 - Height/20),
                   new Vector(-Width/50, Height/50),
                   Height/40,
                   false
            );

            blocks = new Blocks(
                     new Rectangle(Width / 8, Height / 20, Width / 10, Height / 20),
                     new Vector(0, 0),
                     6,
                     4,
                     new int[4, 6] { { 3, 2, 3, 2, 3, 2},
                                     { 2, 3, 2, 3, 2, 3},
                                     { 1, 2, 1, 2, 1, 2},
                                     { 2, 1, 2, 1, 2, 1}
                                   }
            );

            leftWall = new Rectangle(0, 0, Width / 18, Height);
            rightWall = new Rectangle(Width - Width/18, 0, Width / 18, Height);
            
            try
            {
                if(KinectSensor.KinectSensors.Count <= 0)
                {
                    throw new Exception("Kinectを接続してください。");
                }

                StartKinect(KinectSensor.KinectSensors[0]);

            }
            catch(Exception ex){
                MessageBox.Show(ex.Message);
                Close();
            }
        }

        //Kinectの動作を開始する
        private void StartKinect(KinectSensor kinect) {

            //
            SetDesktopBounds(0,0,1440, 810);

            //スケルトンフレームを有効にし、イベントを登録する
            kinect.SkeletonStream.Enable();
            kinect.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(skeleton_update);

            //スワイプジェスチャーのイベント登録
            swipeDetector.OnGestureDetected += new Action<string>(swipe_event);


            timer.Interval = 5000;
            timer.Tick += new EventHandler(timer_event);
            
            kinect.Start();
        }

        // 内積計算
        double DotProduct(Vector a, Vector b)
        {
            return a.X * b.X + a.Y * b.Y;
        }

        //球とパドルの衝突を判定する
        bool IsCollisionPaddle(Vector p1, Vector p2, Vector center, float radius)
        {
            Vector lineDir = (p2 - p1);                   
            Vector n = new Vector(lineDir.Y, -lineDir.X); 
            n.Normalize();

            Vector dir1 = center - p1;
            Vector dir2 = center - p2;

            double dist = Math.Abs(DotProduct(dir1, n));
            double a1 = DotProduct(dir1, lineDir);
            double a2 = DotProduct(dir2, lineDir);

            return (a1 * a2 < 0 && dist < radius) ? true : false;
        }

        //球とブロックの衝突を判定する
        int IsCollisionBlock(Rectangle block, Ball ball)
        {
            if (IsCollisionPaddle(new Vector(block.Left, block.Top),
                new Vector(block.Right, block.Top), ball.Position, ball.Radius))
                return 1;

            if (IsCollisionPaddle(new Vector(block.Left, block.Bottom),
                new Vector(block.Right, block.Bottom), ball.Position, ball.Radius))
                return 2;

            if (IsCollisionPaddle(new Vector(block.Right, block.Top),
                new Vector(block.Right, block.Bottom), ball.Position, ball.Radius))
                return 3;

            if (IsCollisionPaddle(new Vector(block.Left, block.Top),
                new Vector(block.Left, block.Bottom), ball.Position, ball.Radius))
                return 4;

            return -1;
        }
        
        //スケルトンが更新される時に呼び出される
        void skeleton_update(object sender, SkeletonFrameReadyEventArgs e) {

            KinectSensor kinect = sender as KinectSensor;

            //ジョイント座標の更新
            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame()) {
                if (skeletonFrame != null) {
                    Skeleton[] skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    skeletonFrame.CopySkeletonDataTo(skeletons);
                    foreach(Skeleton skeleton in skeletons)
                    {
                        Joint handLeft = skeleton.Joints[JointType.HandLeft];
                        Joint handRight = skeleton.Joints[JointType.HandRight];

                        if (skeleton.TrackingState == SkeletonTrackingState.Tracked
                            && handLeft.TrackingState == JointTrackingState.Tracked
                            && handRight.TrackingState == JointTrackingState.Tracked)
                        {

                            swipeDetector.Add(handRight.Position, kinect);

                            //RGB画像の座標への変換
                            ColorImagePoint rightHandPos = KinectSensor.KinectSensors[0].CoordinateMapper.MapSkeletonPointToColorPoint(handRight.Position, KinectSensor.KinectSensors[0].ColorStream.Format);
                            ColorImagePoint leftHandPos = KinectSensor.KinectSensors[0].CoordinateMapper.MapSkeletonPointToColorPoint(handLeft.Position, KinectSensor.KinectSensors[0].ColorStream.Format);

                            paddle.Size = new Rectangle(leftHandPos.X * Width / 640, Height * 8 / 10, (rightHandPos.X - leftHandPos.X) * Width / 640, Height / 20);
                            if ((rightHandPos.X - leftHandPos.X) * Width / 640 < paddle.MinimumWidth) {
                                paddle.ReSizeWidth(paddle.MinimumWidth);
                            } else if ((rightHandPos.X - leftHandPos.X) * Width / 640 > paddle.MaximumWidth) {
                                paddle.ReSizeWidth(paddle.MaximumWidth);
                            }

                            //ボールをウィンドウサイズに合わせる
                            //ball.SetSpeed(Bounds.X / 50, Bounds.Y / 50);
                            ball.Radius = Height / 40;
                            paddle.MinimumWidth = Width / 7;
                            paddle.MaximumWidth = Width / 3;

                            //壁をウィンドウサイズに合わせる
                            leftWall = new Rectangle(0, 0, Width / 18, Height);
                            rightWall = new Rectangle(Width - Width / 18, 0, Width / 18, Height);

                            //ブロックをウィンドウサイズに合わせる
                            blocks.Size = new Rectangle(Width / 8, Height / 20, Width / 10, Height / 20);
                            blocks.BlockInterval = new Vector(Width / 40, Height / 20);

                            //球と壁との衝突を判定する
                            if (ball.Position.X + ball.Radius > rightWall.Left) ball.ReverseSpeedX();
                            if (ball.Position.X - ball.Radius < leftWall.Right) ball.ReverseSpeedX();
                            if (ball.Position.Y > Height) ball.ReverseSpeedY();

                            //球の移動
                            ball.Position += ball.Speed;
                        }
                    }
                }
            }
            
            //球とパドルの当たり判定
            if (IsCollisionPaddle(new Vector(paddle.Size.X, paddle.Size.Y), new Vector(paddle.Size.X + paddle.Size.Width, paddle.Size.Y), ball.Position, ball.Radius))
            {
                if (paddle.Size.Width < Width / 5) {
                    timer.Enabled = true;
                    ball.Brush = new SolidBrush(Color.Purple);
                    ball.IsPenetrated = true;
                }
                ball.ReverseSpeedY();
            }

            //球とプロックの当たり判定
            for (int i = 0; i < blocks.Column; i++)
            {
                for (int j = 0; j < blocks.Row; j++)
                {
                    if (blocks.DurabilityMatrix[i,j] != 0) {
                        int collision = IsCollisionBlock(blocks.BlockPosition(j, i), ball);
                        if (collision == 1 || collision == 2)
                        {
                            if (!ball.IsPenetrated) ball.ReverseSpeedY();
                            blocks.DurabilityMatrix[i, j]--;
                        }
                        else if (collision == 3 || collision == 4)
                        {
                            if (!ball.IsPenetrated) ball.ReverseSpeedX();
                            blocks.DurabilityMatrix[i, j]--;
                        }
                    }
                }
            }

            //パドルの特殊処理
            if (paddle.Size.Width < Width/5)
                paddle.Brush = new SolidBrush(Color.LightGreen);
            else
                paddle.Brush = new SolidBrush(Color.DimGray);

            //Drawの再描画
            Invalidate();
        }

        //スワイプジェスチャーの検出
        void swipe_event(string obj)
        {
            MessageBox.Show(obj);
        }

        //タイマーが経過した
        void timer_event(object sender, EventArgs e)
        {
            ball.Brush = new SolidBrush(Color.HotPink);
            ball.IsPenetrated = false;
            timer.Enabled = false;
        }

        //描画関数
        private void Draw(object sender, PaintEventArgs e)
        {

            //球の描画
            float px = (float)ball.Position.X - ball.Radius;
            float py = (float)ball.Position.Y - ball.Radius;
            e.Graphics.FillEllipse(ball.Brush, px, py, ball.Radius * 2, ball.Radius * 2);

            //パドルの描画
            e.Graphics.FillRectangle(paddle.Brush, paddle.Size);

            //壁の配置
            e.Graphics.FillRectangle(new SolidBrush(Color.LightGray), leftWall);
            e.Graphics.FillRectangle(new SolidBrush(Color.LightGray), rightWall);

            //ブロックの描画
            for (int i = 0; i < blocks.Column; i++)
            {
                for (int j = 0; j < blocks.Row; j++)
                {
                    if(blocks.DurabilityMatrix[i,j] >= 3)
                        e.Graphics.FillRectangle(new SolidBrush(Color.DarkBlue), blocks.BlockPosition(j, i));
                    else if (blocks.DurabilityMatrix[i, j] == 2)
                        e.Graphics.FillRectangle(new SolidBrush(Color.Blue), blocks.BlockPosition(j, i));
                    else if (blocks.DurabilityMatrix[i, j] == 1)
                        e.Graphics.FillRectangle(new SolidBrush(Color.LightBlue), blocks.BlockPosition(j, i));
                }
            }

        }
    }
}