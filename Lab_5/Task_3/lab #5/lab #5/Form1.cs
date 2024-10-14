using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace lab__5
{
    public partial class Form1 : Form
    {
        private Graphics g;
        private Bitmap bm;
        private List<Point> points = new List<Point>();
        //private List<Point> pointsHelp = new List<Point>();
        private int SelectedPointIndex =-1;
        private int MovingPointIndex = -1;
        private bool isDragging = false;
        private const float PointRadius = 5f;
        public Form1()
        {
            InitializeComponent();
            bm = new Bitmap(850, 600);
            pictureBox1.Image = bm;
            g = Graphics.FromImage(pictureBox1.Image);
        }
        private void Task3Form_Load(object sender, EventArgs e) { }
        private void Clear()
        {
            points.Clear();
            //pointsHelp.Clear();
            g.Clear(pictureBox1.BackColor);
            bm = new Bitmap(850, 600);
            pictureBox1.Image = bm;
            g = Graphics.FromImage(pictureBox1.Image);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Clear();
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            SelectedPointIndex=FindPoint(e.Location);
            if (SelectedPointIndex!=-1)
            {
                if (e.Button==MouseButtons.Left && !isDragging)
                {
                    isDragging = true;
                    MovingPointIndex = SelectedPointIndex;
                }
                else if (e.Button == MouseButtons.Right)
                {
                    RemovePoint(e.Location);
                }
            }
            else if (e.Button == MouseButtons.Left)
            {
                AddPoint(e.Location);
            }
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            //SelectedPointIndex = FindPoint(e.Location);
            if (isDragging && MovingPointIndex!=-1)
            {
                //pointsHelp[SelectedPointIndex] = e.Location;
                points[SelectedPointIndex] = e.Location;
                Redraw();
            }
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            isDragging = false;
            SelectedPointIndex = -1;
            MovingPointIndex = -1;
        }

        private void AddPoint(Point point)
        {
            points.Add(point);
            //pointsHelp.Add(point);
            Redraw();
        }

        private void RemovePoint(Point location)
        {
            SelectedPointIndex = FindPoint(location);
            if (SelectedPointIndex != -1)
            {
                points.RemoveAt(SelectedPointIndex);
                //pointsHelp.RemoveAt(SelectedPointIndex);
                Redraw();
            }
        }

        private int FindPoint(Point location)
        {
            for(int i=0;i<points.Count;i++)
            {
                if (Math.Abs(points[i].X - location.X) < PointRadius && Math.Abs(points[i].Y - location.Y) < PointRadius)
                {
                    return i;
                }
            }
            return -1;
        }

        private void Redraw()
        {
            g.Clear(Color.White);
            DrawBezierCurve();
            DrawPoints();
            pictureBox1.Image = bm;
        }

        private void DrawPoints()
        {
            for (int i = 0; i < points.Count; i++)
            {
                    g.FillRectangle(Brushes.Red, points[i].X - PointRadius, points[i].Y - PointRadius, PointRadius * 2, PointRadius * 2);
            }
        }

        private void DrawBezierCurve()
        {
            if (points.Count < 4) return;

            List<Point> result = new List<Point>();
            float step = 0.01f;

            for (int i = 0; i <= points.Count - 4; i += 3)
            {
                for (float t = 0; t <= 1; t += step)
                {
                    float x = (float)(Math.Pow(1 - t, 3) * points[i].X +
                                       3 * Math.Pow(1 - t, 2) * t * points[i + 1].X +
                                       3 * (1 - t) * Math.Pow(t, 2) * points[i + 2].X +
                                       Math.Pow(t, 3) * points[i + 3].X);

                    float y = (float)(Math.Pow(1 - t, 3) * points[i].Y +
                                       3 * Math.Pow(1 - t, 2) * t * points[i + 1].Y +
                                       3 * (1 - t) * Math.Pow(t, 2) * points[i + 2].Y +
                                       Math.Pow(t, 3) * points[i + 3].Y);

                    result.Add(new Point((int)x, (int)y));
                }
            }

            if (result.Count > 1)
            {
                g.DrawLines(new Pen(Color.Blue), result.ToArray());
            }
        }
    }
}
