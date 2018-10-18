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
using System.IO;

namespace KineKuzusi
{
    public partial class FormMain : Form
    {
        public static GameMain gameMain;
        public static GameOver gameOver;
        private static Panel panel;

        //コンストラクタ
        public FormMain()
        {
            InitializeComponent();
            panel = panel1;
            File.Create(@"Scores.csv");

            CreateGameMain();
        }

        //ゲーム画面を作成し表示する
        private static void CreateGameMain()
        {
            //GameMainの登録
            gameMain = new GameMain();
            gameMain.Disposed += new EventHandler(gameMain_disposed);
            panel.Controls.Add(gameMain);
            gameMain.Dock = DockStyle.Fill;
            gameMain.Visible = true;
        }

        //ゲームオーバー画面を作成し表示する
        private static void CreateGameOver()
        {
            gameOver = new GameOver();
            gameOver.Disposed += new EventHandler(gameOver_disposed);
            panel.Controls.Add(gameOver);
            gameOver.Dock = DockStyle.Fill;
            gameOver.Visible = true;
        }

        //gameMainが破壊された時呼び出される
        private static void gameMain_disposed(object sender, EventArgs e)
        {
            //MessageBox.Show("GameMain was dead! Creating GameOver ...");
            CreateGameOver();
        }

        //gameOverが破壊された時呼び出される
        private static void gameOver_disposed(object sender, EventArgs e)
        {
            //MessageBox.Show("GameOver was dead! Creating GameMain ...");
            CreateGameMain();
        }
    }
}