using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GradientTriangle
{
    public partial class Form1 : Form
    {
        Bitmap bm;
        List<Point> points;
        List<Color> colors;

        public Form1()
        {
            InitializeComponent();

            points = new List<Point>();
            colors = new List<Color>();

            bm = new Bitmap(850, 600);
            pictureBox1.Image = bm;
            Clear();
        }

        void SetColors()
        {
            colors.Clear();

            Random r = new Random();
            HSV hsv = new HSV();
            int offset = r.Next(0, 360);

            hsv.H = offset;
            hsv.S = 1;
            hsv.V = 1;
            colors.Add(HSV_To_RGB(hsv));
            hsv.H += r.Next(80, 120);
            colors.Add(HSV_To_RGB(hsv));
            hsv.H += r.Next(80, 120);
            colors.Add(HSV_To_RGB(hsv));
        }

        void Clear()
        {
            var g = Graphics.FromImage(pictureBox1.Image);
            g.Clear(pictureBox1.BackColor);
            pictureBox1.Image = pictureBox1.Image;
            SetColors();

            pictureBox1.MouseDown += OnMouseDown;
            points.Clear();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Clear();
        }

        void OnMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            if (points.Contains(e.Location)) return;

            points.Add(e.Location);
            bm.SetPixel(e.Location.X, e.Location.Y, Color.Black);

            if (points.Count == 3)
            {
                pictureBox1.MouseDown -= OnMouseDown;

                bm = new Bitmap(850, 600);
                pictureBox1.Image = bm;

                DrawGradient();
            }
            pictureBox1.Image = bm;
        }

        int Interpolation(int x0, int y0, int x1, int y1, int x)
        {
            return (int)(y0 + (float)(y1 - y0) * (x - x0) / (x1 - x0));
        }

        void DrawGradient()
        {
            points.Sort((a, b) => a.Y == b.Y ? 0 : (a.Y < b.Y ? -1 : 1 ));
            
            float inc12, inc13, inc23;

            if (points[0].Y == points[1].Y)
                inc12 = 0;
            else
                inc12 = (float)(points[1].X - points[0].X) / (points[1].Y - points[0].Y);

            if (points[0].Y == points[2].Y)
                inc13 = 0;
            else
                inc13 = (float)(points[2].X - points[0].X) / (points[2].Y - points[0].Y);

            if (points[1].Y == points[2].Y)
                inc23 = 0;
            else
                inc23 = (float)(points[2].X - points[1].X) / (points[2].Y - points[1].Y);

            float x1 = points[0].X;
            float x2 = x1;

            float _inc13 = inc13;

            if (inc13 > inc12)
                (inc13, inc12) = (inc12, inc13);

            int left, right;
            (left, right) = points[1].X < Interpolation(points[0].Y, points[0].X, points[2].Y, points[2].X, points[1].Y) ? (1, 2) : (2, 1);

            for (int i = points[0].Y; i < points[1].Y; i++)
            {
                int cLeftR = Interpolation(points[0].Y, colors[0].R, points[left].Y, colors[left].R, i);
                int cLeftG = Interpolation(points[0].Y, colors[0].G, points[left].Y, colors[left].G, i);
                int cLeftB = Interpolation(points[0].Y, colors[0].B, points[left].Y, colors[left].B, i);

                int cRightR = Interpolation(points[0].Y, colors[0].R, points[right].Y, colors[right].R, i);
                int cRightG = Interpolation(points[0].Y, colors[0].G, points[right].Y, colors[right].G, i);
                int cRightB = Interpolation(points[0].Y, colors[0].B, points[right].Y, colors[right].B, i);
                
                for (int j = (int)x1; j < (int)x2; j++)
                {
                    int R = Interpolation((int)x1, cLeftR, (int)x2, cRightR, j);
                    int G = Interpolation((int)x1, cLeftG, (int)x2, cRightG, j);
                    int B = Interpolation((int)x1, cLeftB, (int)x2, cRightB, j);
                    bm.SetPixel(j, i, Color.FromArgb(R, G, B));
                }
                x1 += inc13;
                x2 += inc12;
            }

            if (points[0].Y == points[1].Y)
            {
                x1 = points[0].X;
                x2 = points[1].X;
            }

           if(_inc13 < inc23) 
                (_inc13, inc23) = (inc23, _inc13);

            (left, right) = Interpolation(points[0].Y, points[0].X, points[2].Y, points[2].X, points[1].Y) < points[1].X ? (0, 1) : (1, 0);

            for (int i = points[1].Y; i < points[2].Y; i++)
            {
                int cLeftR = Interpolation(points[2].Y, colors[2].R, points[left].Y, colors[left].R, i);
                int cLeftG = Interpolation(points[2].Y, colors[2].G, points[left].Y, colors[left].G, i);
                int cLeftB = Interpolation(points[2].Y, colors[2].B, points[left].Y, colors[left].B, i);

                int cRightR = Interpolation(points[2].Y, colors[2].R, points[right].Y, colors[right].R, i);
                int cRightG = Interpolation(points[2].Y, colors[2].G, points[right].Y, colors[right].G, i);
                int cRightB = Interpolation(points[2].Y, colors[2].B, points[right].Y, colors[right].B, i);

                for (int j = (int)x1; j < (int)x2; j++)
                {
                    int R = Interpolation((int)x1, cLeftR, (int)x2, cRightR, j);
                    int G = Interpolation((int)x1, cLeftG, (int)x2, cRightG, j);
                    int B = Interpolation((int)x1, cLeftB, (int)x2, cRightB, j);
                    bm.SetPixel(j, i, Color.FromArgb(R, G, B));
                }
                x1 += _inc13;
                x2 += inc23;
            }

        }

        public struct HSV
        {
            float _h;
            float _s;
            float _v;
            public float H { get { return _h; } set { if (value > 360) value %= 360; while (value < 0) value += 360; _h = value; } }
            public float S { get { return _s; } set { if (value > 1) value = 1; if (value < 0) value = 0; _s = value; } }
            public float V { get { return _v; } set { if (value > 1) value = 1; if (value < 0) value = 0; _v = value; } }
        }

        public static Color HSV_To_RGB(HSV hsv)
        {
            int a = (int)(hsv.H / 60) % 6;
            double f = (hsv.H) / 60 - Math.Floor(hsv.H / 60);
            byte p = (byte)(255 * hsv.V * (1 - hsv.S));
            byte q = (byte)(255 * hsv.V * (1 - f * hsv.S));
            byte t = (byte)(255 * hsv.V * (1 - (1 - f) * hsv.S));

            byte V = (byte)(hsv.V * 255);

            Color res = new Color();
            switch (a)
            {
                case 0: res = Color.FromArgb(V, t, p); break;
                case 1: res = Color.FromArgb(q, V, p); break;
                case 2: res = Color.FromArgb(p, V, t); break;
                case 3: res = Color.FromArgb(p, q, V); break;
                case 4: res = Color.FromArgb(t, p, V); break;
                case 5: res = Color.FromArgb(V, p, q); break;
                default: break;
            }

            return res;
        }
    }
}
