using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MidpointDisplacement
{
    public partial class Form1 : Form
    {
        List<Point> points;
        Bitmap bm;
        Graphics g;
        Random rand;

        double r = 0.2;

        public Form1()
        {
            InitializeComponent();

            rand = new Random();
            points = new List<Point>();
            bm = new Bitmap(850, 600);
            pictureBox1.Image = bm;
            g = Graphics.FromImage(pictureBox1.Image);

            pictureBox1.MouseDown += OnMouseDown;
        }

        void MidpointDisplacement(Point p1, Point p2)
        {
            double dist = Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));

            if (dist > 2)
            {
                int h = (p1.Y + p2.Y) / 2 + rand.Next((int)(-r * dist), (int)(r * dist));

                Point newPoint = new Point((p1.X + p2.X) / 2, h);
                
                points.Add(newPoint);
                points.Sort((a, b) => a.X < b.X ? -1 : (a.X > b.X ? 1 : 0));
                Clear();

                for (int i = 0; i < points.Count - 1; i++)
                {
                    g.DrawLine(new Pen(Color.Black, 2), points[i].X, points[i].Y, points[i + 1].X, points[i + 1].Y);
                }

                //Task.Delay(1).Wait();

                //g.DrawRectangle(new Pen(Color.Black), newPoint.X, newPoint.Y, 1, 1);
                //pictureBox1.Image = bm;
                pictureBox1.Update();

                MidpointDisplacement(p1, newPoint);
                MidpointDisplacement(newPoint, p2);
            }

            
        }

        void OnMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && !points.Contains(e.Location))
            {
                if (points.Count >= 2) return;
                points.Add(e.Location);
                bm.SetPixel(e.Location.X, e.Location.Y, Color.Black);
                pictureBox1.Image = bm;
            }
            if (e.Button == MouseButtons.Right && points.Count == 2 && textBox1.Text != "")
            {
                if(double.TryParse(textBox1.Text, out double ar))
                {
                    r = ar;
                    points.Sort((a, b) => a.X < b.X ? -1 : (a.X > b.X ? 1 : 0));
                    MidpointDisplacement(points[0], points[1]);
                }
            }

        }

        private void Clear()
        {
            g.Clear(pictureBox1.BackColor);
            bm = new Bitmap(850, 600);
            pictureBox1.Image = bm;
            g = Graphics.FromImage(pictureBox1.Image);
        }

        private void clearButton_Click(object sender, EventArgs e)
        {
            points.Clear();
            Clear();
        }
    }
}
