using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace Fractal
{
    public partial class Form1 : Form
    {
        struct L_System
        {
            public string InitialAxiom;
            public float InitialAngle;
            public float Angle;
            public Dictionary<char, string> Rules;

            public L_System(string InitialAxiom, float InitialAngle, float Angle, Dictionary<char, string> Rules)
            {
                this.InitialAxiom = InitialAxiom;
                this.InitialAngle = InitialAngle;
                this.Angle = Angle;
                this.Rules = Rules;
            }

        }

        L_System Read_L_System(string filename)
        {
            L_System l_System = new L_System();
            string[] file = File.ReadAllLines(filename);

            string[] s = file[0].Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries);
            l_System.InitialAxiom = s[0];
            l_System.InitialAngle = float.Parse(s[1]);
            l_System.Angle = float.Parse(s[2]);

            Dictionary<char, string> rules = new Dictionary<char, string>();
            for (int i = 1; i < file.Length; i++)
            {
                rules.Add(file[i][0], file[i].Substring(3));
            }
            l_System.Rules = rules;

            return l_System;
        }

        Graphics g;
        Pen pen;

        public Form1()
        {
            InitializeComponent();
            this.Width = 1280;
            this.Height = 720;
            pictureBox1.Size = new Size(1280, 720);
            Bitmap bm = new Bitmap(1280, 720);
            pictureBox1.Image = bm;
            g = Graphics.FromImage(pictureBox1.Image);
            pen = new Pen(Color.Black);

            //L_System l_system = Read_L_System("gosper.txt");
            //int depth = 5;
            //float deviationProportion = 0;
            //List<FractalPoint> fractal = GetFractal(l_system, depth, deviationProportion);
            //DrawFractal(fractal, this.Width, this.Height, 200);

            L_System l_system = Read_L_System("tree.txt");
            int depth = 14;
            float deviationProportion = 1f;
            List<TreeFractalPoint> fractal = GetTreeFractal(l_system, depth, deviationProportion, 1, 20);
            DrawTreeFractal(fractal, this.Width, this.Height, 200, 2, 10, Color.SaddleBrown, Color.LimeGreen);
        }

        struct FractalPoint
        {
            public float X;
            public float Y;
            public bool Flag;

            public FractalPoint(float x, float y, bool flag)
            {
                this.X = x;
                this.Y = y;
                this.Flag = flag;
            }
        }

        struct TreeFractalPoint
        {
            public float X;
            public float Y;
            public int Depth;
            public bool Flag;

            public TreeFractalPoint(float x, float y, int depth, bool flag)
            {
                this.X = x;
                this.Y = y;
                this.Depth = depth;
                this.Flag = flag;
            }
        }

        Random r = new Random();

        void DrawFractal(List<FractalPoint> points, float targetWidth, float targetHeight, float padding)
        {
            float minx = points[0].X, maxx = points[0].X, miny = points[0].Y, maxy = points[0].Y;

            foreach (FractalPoint p in points)
            {
                if (p.X < minx) minx = p.X;
                if (p.Y < miny) miny = p.Y;
                if (p.X > maxx) maxx = p.X;
                if (p.Y > maxy) maxy = p.Y;
            }

            float width = maxx - minx;
            float height = maxy - miny;
            float centexX = (maxx + minx) / 2;
            float centerY = (maxy + miny) / 2;

            float k = Math.Min((targetWidth - padding) / width, (targetHeight - padding) / height);

            for (int i = 0; i < points.Count; i++) {
                points[i] = new FractalPoint((points[i].X * k) + (targetWidth / 2 - (centexX * k)), (points[i].Y * k) + (targetHeight / 2 - (centerY * k)), points[i].Flag);
            }

            for (int i = 0; i < points.Count - 1; i++)
                if (!points[i + 1].Flag)
                    g.DrawLine(pen, points[i].X, points[i].Y, points[i + 1].X, points[i + 1].Y);

        }

        List<FractalPoint> GetFractal(L_System l, int maxDepth, float deviationProportion)
        {
            float length = 10;
            List<FractalPoint> res = new List<FractalPoint>();
            res.Add(new FractalPoint(0, 0, false));

            (float X, float Y) curPoint = (0, 0);
            float curAngle = l.InitialAngle;
            Stack<((float X, float Y) p, float angle)> stateStack = new Stack<((float X, float Y) p, float angle)>();

            string s = GetFractalString(l.InitialAxiom, maxDepth, l.Rules);
            int deviation = (int)(deviationProportion * l.Angle);
            
            foreach (char c in s)
            {
                if (c == '+') curAngle += l.Angle - r.Next(0, deviation);
                else if (c == '-') curAngle -= l.Angle - r.Next(0, deviation);
                else if (c == '[') stateStack.Push((curPoint, curAngle));
                else if (c == ']') { 
                    ((float X, float Y) p, float angle) temp = stateStack.Pop(); 
                    curPoint = temp.p; 
                    curAngle = temp.angle; 
                    res.Add(new FractalPoint(temp.p.X, temp.p.Y, true)); 
                }

                if (char.IsUpper(c))
                {
                    float x = (float)(curPoint.X + length * Math.Cos(curAngle * Math.PI / 180));
                    float y = (float)(curPoint.Y + length * Math.Sin(curAngle * Math.PI / 180));
                    res.Add(new FractalPoint(x, y, false));
                    curPoint = (x, y);
                }
            }

            return res;
        }

        float Interpolation(float x, float min1, float max1, float min2, float max2) => (x - min1) / (max1 - min1) * (max2 - min2) + min2;

        void DrawTreeFractal(List<TreeFractalPoint> points, float targetWidth, float targetHeight, float padding, float minWidth, float maxWidth, Color rootColor, Color leafColor)
        {
            float minx = points[0].X, maxx = points[0].X, miny = points[0].Y, maxy = points[0].Y;
            int maxDepth = 0;

            foreach (TreeFractalPoint p in points)
            {
                if (p.X < minx) minx = p.X;
                if (p.Y < miny) miny = p.Y;
                if (p.X > maxx) maxx = p.X;
                if (p.Y > maxy) maxy = p.Y;
                if (p.Depth > maxDepth) maxDepth = p.Depth;
            }

            float width = maxx - minx;
            float height = maxy - miny;
            float centexX = (maxx + minx) / 2;
            float centerY = (maxy + miny) / 2;

            float k = Math.Min((targetWidth - padding) / width, (targetHeight - padding) / height);

            for (int i = 0; i < points.Count; i++)
            {
                points[i] = new TreeFractalPoint((points[i].X * k) + (targetWidth / 2 - (centexX * k)), (points[i].Y * k) + (targetHeight / 2 - (centerY * k)), points[i].Depth, points[i].Flag);
            }

            for (int i = 0; i < points.Count - 1; i++)
            {
                if (!points[i + 1].Flag)
                {
                    float w = Interpolation(points[i + 1].Depth, 0, maxDepth, maxWidth, minWidth);

                    Byte R = (Byte)Interpolation(points[i + 1].Depth, 0, maxDepth, rootColor.R, leafColor.R);
                    Byte G = (Byte)Interpolation(points[i + 1].Depth, 0, maxDepth, rootColor.G, leafColor.G);
                    Byte B = (Byte)Interpolation(points[i + 1].Depth, 0, maxDepth, rootColor.B, leafColor.B);

                    Pen treePen = new Pen(Color.FromArgb(R, G, B), w);

                    g.DrawLine(treePen, points[i].X, points[i].Y, points[i + 1].X, points[i + 1].Y);
                }
            }
                

        }

        List<TreeFractalPoint> GetTreeFractal(L_System l, int maxDepth, float deviationProportion, float minLength, float maxLength)
        {
            float length = maxLength;
            List<TreeFractalPoint> res = new List<TreeFractalPoint>();
            res.Add(new TreeFractalPoint(0, 0, 0, false));

            (float X, float Y) curPoint = (0, 0);
            float curAngle = l.InitialAngle;
            Stack<((float X, float Y) p, float angle, int depth)> stateStack = new Stack<((float X, float Y) p, float angle, int depth)>();

            string s = GetFractalString(l.InitialAxiom, maxDepth, l.Rules);
            int depth = 0;

            int deviation = (int)(deviationProportion * l.Angle);

            foreach (char c in s)
            {
                if (c == '+') curAngle += l.Angle - r.Next(0, deviation);
                else if (c == '-') curAngle -= l.Angle - r.Next(0, deviation);
                else if (c == '[') stateStack.Push((curPoint, curAngle, depth));
                else if (c == ']')
                {
                    ((float X, float Y) p, float angle, int depth) temp = stateStack.Pop();
                    curPoint = temp.p;
                    curAngle = temp.angle;
                    depth = temp.depth;
                    length = Interpolation(depth, 0, maxDepth, maxLength, minLength);
                    res.Add(new TreeFractalPoint(temp.p.X, temp.p.Y, depth, true));
                }
                else if (c == '@')
                {
                    depth++;
                    length = Interpolation(depth, 0, maxDepth, maxLength, minLength);
                }

                if (char.IsUpper(c))
                {
                    float x = (float)(curPoint.X + length * Math.Cos(curAngle * Math.PI / 180));
                    float y = (float)(curPoint.Y + length * Math.Sin(curAngle * Math.PI / 180));
                    res.Add(new TreeFractalPoint(x, y, depth, false));
                    curPoint = (x, y);
                }
            }

            return res;
        }

        string GetFractalString(string s, int depth, Dictionary<char, string> rules)
        {
            if (depth == 0) return s;

            string res = "";
            foreach(char c in s)
            {
                if (char.IsUpper(c)) res += GetFractalString(rules[c], depth - 1, rules);
                else res += c;
            }
            return res;
        }

    }
}
