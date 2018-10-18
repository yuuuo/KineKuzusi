using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace KineKuzusi
{
    class Tools
    {
        // 内積計算
        private double DotProduct(Vector a, Vector b)
        {
            return a.X * b.X + a.Y * b.Y;
        }

        //球とパドルの衝突を判定する
        public bool IsCollisionPaddle(Vector p1, Vector p2, Vector center, float radius)
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
        public int IsCollisionBlock(Rectangle block, GameMain.Ball ball)
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

        //配列の一致する要素の数を数える
        public int ElementEqualCount(int key,int[,] vs, int x, int y)
        {
            int count=0;

            for(int i=0; i < x; i++)
            {
                for(int j=0; j < y; j++)
                {
                    if (vs[i, j] == key) count++;
                }
            }

            return count;
        }
    }
}
