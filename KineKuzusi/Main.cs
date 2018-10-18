using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KineKuzusi
{
    public partial class Main : UserControl
    {
        public Main()
        {
            InitializeComponent();
        }

        private void Draw(object sender, PaintEventArgs e)
        {
            Rectangle rectangle = new Rectangle(0, 0, Width / 2, Height / 2);
            e.Graphics.FillRectangle(new SolidBrush(Color.BlueViolet), rectangle);
        }
    }
}
