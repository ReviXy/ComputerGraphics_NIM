using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Drawing.Imaging;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        static string filename = "88.png";
        static Bitmap bmFill = new Bitmap(filename);

        Bitmap bm;
        Graphics g;
        List<Point> points;

        Color penColor = Color.Black;
        Pen pen;
        Color fillColor = Color.Yellow;
        Pen fillPen;

        Color edgeColor = Color.DarkRed;

        public Form1()
        {
            InitializeComponent();

            bm = new Bitmap(850, 600);
            pictureBox1.Image = bm;
            Clear();
            
            points = new List<Point>();
            pen = new Pen(penColor);
            fillPen = new Pen(fillColor);
            g = Graphics.FromImage(bm);

            pictureBox1.MouseDown += OnMouseDown;
            pictureBox1.MouseMove += OnMouseMove;

        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                pictureBox1.MouseDown -= ColorFill;
                pictureBox1.MouseDown -= PictureFill;
                pictureBox1.MouseDown -= SelectEdge;

                pictureBox1.MouseDown += OnMouseDown;
                pictureBox1.MouseMove += OnMouseMove;
            }
            else if (radioButton2.Checked)
            {
                pictureBox1.MouseDown -= OnMouseDown;
                pictureBox1.MouseMove -= OnMouseMove;
                pictureBox1.MouseDown -= PictureFill;
                pictureBox1.MouseDown -= SelectEdge;

                pictureBox1.MouseDown += ColorFill;
            }
            else if (radioButton3.Checked)
            {
                pictureBox1.MouseDown -= OnMouseDown;
                pictureBox1.MouseMove -= OnMouseMove;
                pictureBox1.MouseDown -= ColorFill;
                pictureBox1.MouseDown -= SelectEdge;

                pictureBox1.MouseDown += PictureFill;
            }
            else if (radioButton4.Checked)
            {
                pictureBox1.MouseDown -= OnMouseDown;
                pictureBox1.MouseMove -= OnMouseMove;
                pictureBox1.MouseDown -= ColorFill;
                pictureBox1.MouseDown -= PictureFill;

                pictureBox1.MouseDown += SelectEdge;
            }
        }

        void ColorFill(object sender, MouseEventArgs e)
        {
            ColorFill_(e.Location);
        }

        private bool ColorsEqual(Color c1, Color c2) => c1.R == c2.R && c1.G == c2.G && c1.B == c2.B;

        void ColorFill_(Point p)
        {
            if (p.X >= 0 && p.X < bm.Width && p.Y >= 0 && p.Y < bm.Height && !ColorsEqual(bm.GetPixel(p.X, p.Y), fillColor) && !ColorsEqual(bm.GetPixel(p.X, p.Y), penColor))
            {
                Color oldColor = bm.GetPixel(p.X, p.Y);
                Point left = new Point(p.X, p.Y);
                while (left.X > 0 && ColorsEqual(bm.GetPixel(left.X, left.Y), oldColor)) left.X -= 1;
                Point right = new Point(p.X, p.Y);
                while (right.X < bm.Width - 1 && ColorsEqual(bm.GetPixel(right.X, right.Y), oldColor)) right.X += 1;

                if (left.X == 0) left.X = -1;
                if (right.X == bm.Width - 1) right.X = bm.Width;

                g.DrawLine(fillPen, left.X + 1, left.Y, right.X - 1, right.Y);
                pictureBox1.Image = bm;

                for (int i = left.X + 1; i <= right.X-1; i++)
                    ColorFill_(new Point(i, p.Y + 1));
                for (int i = left.X + 1; i <= right.X-1; i++)
                    ColorFill_(new Point(i, p.Y - 1));
            }
        }

        void PictureFill(object sender, MouseEventArgs e)
        {
            PictureFill_(e.Location, e.Location);
        }
        
        Point GetLocalPoint(Point p, Point origin)
        {
            int x = bmFill.Width / 2 + (p.X - origin.X);
            int y = bmFill.Height / 2 + (p.Y - origin.Y);
            if (x >= bmFill.Width) x %= bmFill.Width;
            if (y >= bmFill.Height) y %= bmFill.Height;
            while (x < 0) x += bmFill.Width;
            while (y < 0) y += bmFill.Height;
            return new Point(x, y);
        }

        void PictureFill_(Point p, Point origin)
        {
            if (p.X >= 0 && p.X < bm.Width && p.Y >= 0 && p.Y < bm.Height && !ColorsEqual(bm.GetPixel(p.X, p.Y), penColor))
            {
                Color oldColor = bm.GetPixel(p.X, p.Y);
                Point temp = GetLocalPoint(new Point(p.X, p.Y), origin);
                if (ColorsEqual(bmFill.GetPixel(temp.X, temp.Y), oldColor)) return; // Типо проверка на уже залитый пиксель

                Point left = new Point(p.X, p.Y);
                while (left.X > 0 && ColorsEqual(bm.GetPixel(left.X, left.Y), oldColor)) left.X -= 1;
                Point right = new Point(p.X, p.Y);
                while (right.X < bm.Width - 1 && ColorsEqual(bm.GetPixel(right.X, right.Y), oldColor)) right.X += 1;

                if (left.X == 0) left.X = -1;
                if (right.X == bm.Width - 1) right.X = bm.Width;

                // Типо заливка линии
                for (int i = left.X + 1; i <= right.X - 1; i++)
                {
                    Point localP = GetLocalPoint(new Point(i, left.Y), origin);
                    bm.SetPixel(i, left.Y, bmFill.GetPixel(localP.X, localP.Y));
                }

                pictureBox1.Image = bm;

                for (int i = left.X + 1; i <= right.X - 1; i++)
                    PictureFill_(new Point(i, p.Y + 1), origin);
                for (int i = left.X + 1; i <= right.X - 1; i++)
                    PictureFill_(new Point(i, p.Y - 1), origin);
            }
        }

        Point GetNeighboor(Point p, int dir)
        {
            Point p1 = new Point(0, 0);
            switch (dir)
            {
                case 0: p1 = new Point(p.X,      p.Y - 1); break;
                case 1: p1 = new Point(p.X + 1,  p.Y - 1); break;
                case 2: p1 = new Point(p.X + 1,  p.Y); break;
                case 3: p1 = new Point(p.X + 1,  p.Y + 1); break;
                case 4: p1 = new Point(p.X,      p.Y + 1); break;
                case 5: p1 = new Point(p.X - 1,  p.Y + 1); break;
                case 6: p1 = new Point(p.X - 1,  p.Y); break;
                case 7: p1 = new Point(p.X - 1,  p.Y -1); break;
                default:break;
            }
            return p1;
        }

        void SelectEdge(object sender, MouseEventArgs e)
        {
            LinkedList<Point> edge = SelectEdge_(e.Location);
            if (edge.Count > 0)
            {
                foreach (Point p in edge)
                    bm.SetPixel(p.X, p.Y, edgeColor);
                pictureBox1.Image = bm;
            }
        }

        LinkedList<Point> SelectEdge_(Point start)
        {
            LinkedList<Point> edge = new LinkedList<Point>();
            edge.AddLast(start);
            Color c = bm.GetPixel(start.X, start.Y);

            Point cur = start;
            int dir = 4;
            
            while (true)
            {
                dir += 2;
                if (dir > 7) dir -= 8;
                Point p;

                for (int i = 0; i < 8; i++)
                {
                    p = GetNeighboor(cur, dir);
                    if (!(p.X >= 0 && p.X < bm.Width && p.Y >= 0 && p.Y < bm.Height)) continue;
                    if (ColorsEqual(bm.GetPixel(p.X, p.Y), c)) goto a;
                    dir--;
                    if (dir < 0) dir += 8;
                }
                return new LinkedList<Point>();
            a:
                if (p.X == start.X && p.Y == start.Y) break;

                edge.AddLast(p);
                cur = p;
            }
            return edge;
        }

        void OnMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            points.Clear();
            points.Add(e.Location);
        }


        void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            points.Add(e.Location);

            if (points.Count < 2)
                return;

            g.DrawLines(pen, points.ToArray());
            pictureBox1.Image = bm;
        }

        private void Clear()
        {
            var g = Graphics.FromImage(pictureBox1.Image);
            g.Clear(pictureBox1.BackColor);
            pictureBox1.Image = pictureBox1.Image;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Clear();
        }
    }
}
