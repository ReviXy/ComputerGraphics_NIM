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
        Bitmap bm;
        Graphics g;
        List<Point> points;
        const int MAX_COLOR_VALUE = 255;
        Color penColor = Color.Black;
        Pen pen;
        public Form1()
        {
            InitializeComponent();

            bm = new Bitmap(850, 600);
            pictureBox1.Image = bm;
            Clear();
            
            points = new List<Point>();
            pen = new Pen(penColor);
            g = Graphics.FromImage(bm);
            pictureBox1.Click += pictureBox1_Click_1;

        }
        void pictureBox1_Click_1(object sender, EventArgs e)
        {
            Point point = pictureBox1.PointToClient(Cursor.Position);
            using (Graphics g = pictureBox1.CreateGraphics())
                if (points.Count < 2 && !points.Contains(point))
                {
                    g.FillRectangle(Brushes.Black, point.X, point.Y, 2, 2);
                    points.Add(point);
                }
            if (radioButton1.Checked && (points.Count ==2))
            {
                int x0 = points[points.Count - 2].X;
                int y0 = points[points.Count - 2].Y;
                int x1 = points[points.Count-1].X;
                int y1 = points[points.Count-1].Y;
                Bresenham(x0, y0, x1, y1);
                pictureBox1.Image = bm;
            }
            if (radioButton2.Checked && (points.Count ==2))
            {
                int x0 = points[points.Count - 2].X;
                int y0 = points[points.Count - 2].Y;
                int x1 = points[points.Count-1].X;
                int y1 = points[points.Count-1].Y;
               Wu(x0, y0, x1, y1);
                pictureBox1.Image = bm;
            }
        }
        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                points.Clear();
                //Clear();
                //pictureBox1.Click += pictureBox1_Click_1;
            }
            else if (radioButton2.Checked)
            {
                points.Clear();
                //Clear();
                //pictureBox1.Click += pictureBox1_Click_1;
            }
        }
        void Bresenham(int xstart, int ystart, int xend, int yend)
        {
            if (xstart > xend)
            {
                Swap(ref xstart, ref xend);
                Swap(ref ystart, ref yend);
            }
            int dx = xend - xstart;
            int dy = yend - ystart;
            int xi = xstart;
            int yi = ystart;
            int step = 1;
            int di = 2 * (dy - dx);
            if (dx == 0 || Math.Abs(dy ) > Math.Abs(dx))
            {
                if (dy / (double)dx < 0)
                {
                    xi = xend;
                    step = -1;
                    dy = -dy;
                    int temp = ystart;
                    ystart = yend;
                    yend = temp;
                }
                for (yi = ystart; yi <= yend; yi++)
                {
                    bm.SetPixel(xi, yi, Color.Black);
                    if (di >= 0)
                    {
                        xi += step;
                        di += 2 * (dx - dy);
                    }
                    else
                    {
                        di += 2 * dx;
                    }
                }
            }
            else
            {
                if (dy / (double)dx < 0)
                {
                    step = -1;
                    dy = -dy;
                }
                for (xi = xstart; xi <= xend; xi++)
                {
                    bm.SetPixel(xi, yi, Color.Black);
                    if (di >= 0)
                    {
                        yi += step;
                        di += 2 * (dy - dx);
                    }
                    else
                    {
                        di += 2 * dy;
                    }
                }
            }

        }


        void Wu(int xstart, int ystart, int xend, int yend)
        {
            if (xstart > xend)
            {
                Swap(ref xstart, ref xend);
                Swap(ref ystart, ref yend);
            }

            int dx = xend - xstart;
            int dy = yend - ystart;
            double gradient;
            if (dx == 0)
            {
                gradient = 1;
            }
            else if (dy == 0)
            {
                gradient = 0;
            }
            else
            {
                gradient = dy / (double)dx;
            }

            int step = 1;
            double xi = xstart;
            double yi = ystart;

            if (Math.Abs(gradient) > 1)
            {
                gradient = 1 / gradient;
                if (gradient < 0)
                {
                    xi = xend;
                    step = -1;
                    int temp = ystart;
                    ystart = yend;
                    yend = temp;
                }

                for (yi = ystart; yi <= yend; yi += 1)
                {
                    int help;
                    if (gradient < 0)
                    {
                        help = (int)(MAX_COLOR_VALUE * (xi - (int)xi));
                    }
                    else
                    {
                        help = MAX_COLOR_VALUE - (int)(MAX_COLOR_VALUE * (xi - (int)xi));
                    }
                    bm.SetPixel((int)xi, (int)yi, Color.FromArgb(MAX_COLOR_VALUE - help, MAX_COLOR_VALUE - help, MAX_COLOR_VALUE - help));
                    bm.SetPixel((int)xi + step, (int)yi, Color.FromArgb(help, help, help));
                    xi += gradient;
                }
            }
            else
            {
                if (gradient < 0)
                {
                    step = -1;
                }
                for (xi = xstart; xi <= xend; xi += 1)
                {
                    int help;
                    if (gradient < 0)
                    {
                        help = (int)(MAX_COLOR_VALUE * (yi - (int)yi));
                    }
                    else
                    {
                        help = MAX_COLOR_VALUE - (int)(MAX_COLOR_VALUE * (yi - (int)yi));
                    }
                    bm.SetPixel((int)xi, (int)yi, Color.FromArgb(MAX_COLOR_VALUE - help, MAX_COLOR_VALUE - help, MAX_COLOR_VALUE - help));
                    bm.SetPixel((int)xi, (int)yi + step, Color.FromArgb(help, help, help));
                    yi += gradient;
                }
            }
        }
        void Swap(ref int a, ref int b)
        {
            int temp = a;
            a = b;
            b = temp;
        }


        private void Clear()
        {
            var g = Graphics.FromImage(pictureBox1.Image);
            g.Clear(pictureBox1.BackColor);
            pictureBox1.Image = pictureBox1.Image;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            points.Clear();
            Clear();
        }

    }
}
