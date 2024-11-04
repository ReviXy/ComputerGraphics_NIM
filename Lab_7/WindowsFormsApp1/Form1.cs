using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        Bitmap bm;
        Graphics g;
        Camera camera;

        Object3D obj;

        bool perspective = true;

        public Form1()
        {
            InitializeComponent();

            bm = new Bitmap(pictureBox.Width, pictureBox.Height);
            pictureBox.Image = bm;
            g = Graphics.FromImage(pictureBox.Image);
            g.Clear(Color.White);
            g.TranslateTransform(pictureBox.ClientSize.Width / 2, pictureBox.ClientSize.Height / 2);
            g.ScaleTransform(1, -1);

            camera = new Camera();

            //Tetrahedron(ref obj, 50);

            obj = Object3D.Load_obj("test.obj");

            Func<float, float, float> f1 = (x, y) => { float r = x * x + y * y; return (float)(Math.Cos(r) / (r + 1)); };
            Func<float, float, float> f2 = (x, y) => { return (x * x + y * y); };

            //Graph(ref obj, f1, -10, 10, -10, 10, 60);
            //Graph(ref obj, f2, -1, 1, -1, 1, 60);

            //List<Point3D> test = new List<Point3D>{ new Point3D(5,0,0), new Point3D(5, 10, 0), new Point3D(11, 0, 0) };
            //RotationFigure(ref obj, test, Axis.Y, 20);

            DrawObject(obj);
        }

        public void DrawObject(Object3D obj)
        {
            List<Point3D> vertexes = obj.Vertices;

            vertexes = vertexes.Select(p => View(p, camera)).ToList();

            if (perspective) vertexes = vertexes.Select(p => Perspective(p)).ToList();
            else vertexes = vertexes.Select(p => Axonometric(p)).ToList();

            foreach (Point3D p in vertexes)
            {
                g.DrawRectangle(new Pen(Color.Red), p.X - 1, p.Y - 1, 2, 2);
            }

            foreach (Face face in obj.Faces)
            {
                for (int i = 0; i < face.FaceIndices.Count; i++)
                {
                    Point3D p1 = vertexes[face.FaceIndices[i].VertexIndex - 1];
                    Point3D p2 = vertexes[face.FaceIndices[(i + 1) % face.FaceIndices.Count].VertexIndex - 1];
                    g.DrawLine(new Pen(Color.Black),
                        p1.X,
                        p1.Y,
                        p2.X,
                        p2.Y);

                }
            }
        }

        public Point3D View(Point3D p, Camera cam)
        {
            float[][] ViewMatrix = new float[4][]
            {
                    new float[4] { cam.U.X,                                cam.V.X,                               cam.N.X,                              0 },
                    new float[4] { cam.U.Y,                                cam.V.Y,                               cam.N.Y,                              0 },
                    new float[4] { cam.U.Z,                                cam.V.Z,                               cam.N.Z,                              0 },
                    new float[4] { -(cam.U * cam.Location),   -(cam.V * cam.Location),  -(cam.N * cam.Location), 1 }
            };
            return MultiplyMatrix(ViewMatrix, p);
        }

        public Point3D Perspective(Point3D p)
        {
            float c = -pictureBox.Width * 1f;

            float[][] PerspectiveMatrix = new float[4][]
            {
                    new float[4] { 1, 0, 0, 0 },
                    new float[4] { 0, 1, 0, 0 },
                    new float[4] { 0, 0, 0, -1/c },
                    new float[4] { 0, 0, 0, 1 }
            };

            Point3D temp = MultiplyMatrix(PerspectiveMatrix, p);
            return new Point3D(p.X / temp.W, p.Y / temp.W, 0, p.W);
        }

        public Point3D Axonometric(Point3D p)
        {
            float phi = (float)((0 / 180D) * Math.PI);
            float ksi = (float)((0 / 180D) * Math.PI);

            float[][] AxonometricMatrix = new float[4][]
            {
                    new float[4] { (float)Math.Cos(ksi),    (float)(Math.Sin(phi) * Math.Sin(ksi)),    0, 0 },
                    new float[4] { 0,                       (float)Math.Cos(phi),                      0, 0 },
                    new float[4] { (float)Math.Sin(ksi),    -(float)(Math.Sin(phi) * Math.Cos(ksi)),   0, 0 },
                    new float[4] { 0,                       0,                                         0, 1 }
            };

            return MultiplyMatrix(AxonometricMatrix, p);
        }

        public void Scale(ref Object3D obj, float mx, float my, float mz)
        {
            Point3D center = new Point3D(0, 0, 0);
            foreach(Point3D p in obj.Vertices)
                center += p;
            center /= obj.Vertices.Count;

            obj.Vertices = obj.Vertices.Select(p => TranslatePoint(p, -center.X, -center.Y, -center.Z)).ToList();
            obj.Vertices = obj.Vertices.Select(p => ScalePoint(p, mx, my, mz)).ToList();
            obj.Vertices = obj.Vertices.Select(p => TranslatePoint(p, center.X, center.Y, center.Z)).ToList();
        }

        public void Rotate(ref Object3D obj, Point3D a, Point3D b, float angle)
        {
            b -= a;
            b /= (float)Math.Sqrt(Math.Pow(b.X, 2) + Math.Pow(b.Y, 2) + Math.Pow(b.Z, 2));
            float l = b.X;
            float m = b.Y;
            float n = b.Z;

            angle = (float)((angle / 180D) * Math.PI);

            float sin = (float)Math.Sin(angle);
            float cos = (float)Math.Cos(angle);

            float[][] RotateMatrix = new float[4][]
            {
                    new float[4] { l*l + cos*(1 - l*l),     l*(1 - cos)*m + n * sin,    l*(1 - cos)*n - m * sin,    0 },
                    new float[4] { l*(1 - cos)*m - n * sin, m*m + cos*(1 - m*m),        m*(1 - cos)*n + l*sin,      0 },
                    new float[4] { l*(1 - cos)*n + m * sin, m*(1 - cos)*n - l*sin,      n*n + cos*(1 - n*n),        0 },
                    new float[4] { 0,                       0,                          0,                          1 }
            };

            obj.Vertices = obj.Vertices.Select(p => MultiplyMatrix(RotateMatrix, p)).ToList();
        }

        public void XRotate(ref Object3D obj, float angle)
        {
            Point3D center = new Point3D(0, 0, 0);
            foreach (Point3D p in obj.Vertices)
                center += p;
            center /= obj.Vertices.Count;

            obj.Vertices = obj.Vertices.Select(p => TranslatePoint(p, -center.X, -center.Y, -center.Z)).ToList();
            obj.Vertices = obj.Vertices.Select(p => XRotatePoint(p, angle)).ToList();
            obj.Vertices = obj.Vertices.Select(p => TranslatePoint(p, center.X, center.Y, center.Z)).ToList();
        }

        public void YRotate(ref Object3D obj, float angle)
        {
            Point3D center = new Point3D(0, 0, 0);
            foreach (Point3D p in obj.Vertices)
                center += p;
            center /= obj.Vertices.Count;

            obj.Vertices = obj.Vertices.Select(p => TranslatePoint(p, -center.X, -center.Y, -center.Z)).ToList();
            obj.Vertices = obj.Vertices.Select(p => YRotatePoint(p, angle)).ToList();
            obj.Vertices = obj.Vertices.Select(p => TranslatePoint(p, center.X, center.Y, center.Z)).ToList();
        }

        public void ZRotate(ref Object3D obj, float angle)
        {
            Point3D center = new Point3D(0, 0, 0);
            foreach (Point3D p in obj.Vertices)
                center += p;
            center /= obj.Vertices.Count;

            obj.Vertices = obj.Vertices.Select(p => TranslatePoint(p, -center.X, -center.Y, -center.Z)).ToList();
            obj.Vertices = obj.Vertices.Select(p => ZRotatePoint(p, angle)).ToList();
            obj.Vertices = obj.Vertices.Select(p => TranslatePoint(p, center.X, center.Y, center.Z)).ToList();
        }

        public Point3D XYMirrorPoint(Point3D p)
        {
            float[][] XYMirrorMatrix = new float[4][]
            {
                    new float[4] { 1, 0, 0,  0},
                    new float[4] { 0, 1, 0,  0 },
                    new float[4] { 0, 0, -1, 0 },
                    new float[4] { 0, 0, 0,  1 }
            };
            return MultiplyMatrix(XYMirrorMatrix, p);
        }

        public Point3D XZMirrorPoint(Point3D p)
        {
            float[][] XZMirrorMatrix = new float[4][]
            {
                    new float[4] { 1, 0,  0, 0 },
                    new float[4] { 0, -1, 0, 0 },
                    new float[4] { 0, 0,  1, 0 },
                    new float[4] { 0, 0,  0, 1 }
            };
            return MultiplyMatrix(XZMirrorMatrix, p);
        }

        public Point3D YZMirrorPoint(Point3D p)
        {
            float[][] YZMirrorMatrix = new float[4][]
            {
                    new float[4] { -1, 0, 0, 0 },
                    new float[4] { 0,  1, 0, 0 },
                    new float[4] { 0,  0, 1, 0 },
                    new float[4] { 0,  0, 0, 1 }
            };
            return MultiplyMatrix(YZMirrorMatrix, p);
        }

        public Point3D XRotatePoint(Point3D p, float angle)
        {
            angle = (float)((angle / 180D) * Math.PI);
            float[][] XRotationMatrix = new float[4][]
            {
                    new float[4] { 1, 0,                        0,                      0 },
                    new float[4] { 0, (float)Math.Cos(angle),   (float)Math.Sin(angle), 0 },
                    new float[4] { 0, -(float)Math.Sin(angle),  (float)Math.Cos(angle), 0 },
                    new float[4] { 0, 0,                        0,                      1 }
            };

            return MultiplyMatrix(XRotationMatrix, p);
        }

        public Point3D YRotatePoint(Point3D p, float angle)
        {
            angle = (float)((angle / 180D) * Math.PI);
            float[][] XRotationMatrix = new float[4][]
            {
                    new float[4] { (float)Math.Cos(angle),  0, -(float)Math.Sin(angle), 0 },
                    new float[4] { 0,                       1, 0,                       0 },
                    new float[4] { (float)Math.Sin(angle),  0,  (float)Math.Cos(angle), 0 },
                    new float[4] { 0,                       0, 0,                       1 }
            };

            return MultiplyMatrix(XRotationMatrix, p);
        }

        public Point3D ZRotatePoint(Point3D p, float angle)
        {
            angle = (float)((angle / 180D) * Math.PI);
            float[][] XRotationMatrix = new float[4][]
            {
                    new float[4] { (float)Math.Cos(angle),  (float)Math.Sin(angle), 0, 0},
                    new float[4] { -(float)Math.Sin(angle), (float)Math.Cos(angle), 0, 0},
                    new float[4] { 0,                       0,                      1, 0},
                    new float[4] { 0,                       0,                      0, 1}
            };

            return MultiplyMatrix(XRotationMatrix, p);
        }

        public Point3D ScalePoint(Point3D p, float mx, float my, float mz)
        {
            float[][] TranslationMatrix = new float[4][]
            {
                    new float[4] { mx, 0,  0,  0 },
                    new float[4] { 0,  my, 0,  0 },
                    new float[4] { 0,  0,  mz, 0 },
                    new float[4] { 0,  0,  0,  1 }
            };

            return MultiplyMatrix(TranslationMatrix, p);
        }

        public Point3D TranslatePoint(Point3D p, float dx, float dy, float dz)
        {
            float[][] TranslationMatrix = new float[4][]
            {
                    new float[4] { 1,  0,  0,  0 },
                    new float[4] { 0,  1,  0,  0 },
                    new float[4] { 0,  0,  1,  0 },
                    new float[4] { dx, dy, dz, 1 }
            };

            return MultiplyMatrix(TranslationMatrix, p);
        }

        public Point3D MultiplyMatrix(float[][] matrix, Point3D p)
        {
            float[] tempVector = new float[4] { p.X, p.Y, p.Z, p.W};
            float[] resultVector = new float[4];
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                    resultVector[i] += matrix[j][i] * tempVector[j];
            }
            return new Point3D(resultVector[0], resultVector[1], resultVector[2], resultVector[3]);
        }

        public void Dodecahedron(ref Object3D obj, float a)
        {
            Object3D temp = new Object3D();
            Icosahedron(ref temp, 1);

            obj = new Object3D();

            foreach (Face f in temp.Faces)
            {
                Point3D sum = new Point3D(0, 0, 0);
                foreach (FaceIndices i in f.FaceIndices)
                {
                    sum += temp.Vertices[i.VertexIndex];
                }
                obj.Vertices.Add(new Point3D(a * sum.X / 3, a * sum.Y / 3, a * sum.Z / 3));
            }

            float k = a / (float)Math.Sqrt(Math.Pow(obj.Vertices[0].X - obj.Vertices[1].X, 2) + Math.Pow(obj.Vertices[0].Y - obj.Vertices[1].Y, 2) + Math.Pow(obj.Vertices[0].Z - obj.Vertices[1].Z, 2));
            for (int i = 0; i < obj.Vertices.Count; i++)
                obj.Vertices[i] = obj.Vertices[i] * k;

            obj.Faces.Add(new Face(1, 2, 3, 4, 5));
            obj.Faces.Add(new Face(6, 7, 8, 9, 10));

            obj.Faces.Add(new Face(1, 2, 13, 12, 11));
            obj.Faces.Add(new Face(2, 3, 15, 14, 13));
            obj.Faces.Add(new Face(3, 4, 17, 16, 15));
            obj.Faces.Add(new Face(4, 5, 19, 18, 17));
            obj.Faces.Add(new Face(5, 1, 11, 20, 19));

            obj.Faces.Add(new Face(6, 7, 14, 13, 12));
            obj.Faces.Add(new Face(7, 8, 16, 15, 14));
            obj.Faces.Add(new Face(8, 9, 18, 17, 16));
            obj.Faces.Add(new Face(9, 10, 20, 19, 18));
            obj.Faces.Add(new Face(10, 6, 12, 11, 20));

        }

        public void Icosahedron(ref Object3D obj, float a)
        {
            float R = a / (2 * (float)Math.Sin(Math.PI / 6));
            float r = R * (float)Math.Cos(Math.PI / 6);

            float step = 360 / 5;

            obj = new Object3D();

            obj.Vertices.Add(new Point3D(0, 0, R));
            for (float angle = 0; angle < 360; angle += step)
            {
                obj.Vertices.Add(new Point3D(R * (float)Math.Cos(angle / 180D * Math.PI), R * (float)Math.Sin(angle / 180D * Math.PI), a / 2));
            }

            obj.Vertices.Add(new Point3D(0, 0, -R));
            for (float angle = step / 2; angle < 360; angle += step)
            {
                obj.Vertices.Add(new Point3D(R * (float)Math.Cos(angle / 180D * Math.PI), R * (float)Math.Sin(angle / 180D * Math.PI), -a / 2));
            }

            obj.Faces.Add(new Face(1, 2, 3));
            obj.Faces.Add(new Face(1, 3, 4));
            obj.Faces.Add(new Face(1, 4, 5));
            obj.Faces.Add(new Face(1, 5, 6));
            obj.Faces.Add(new Face(1, 6, 2));

            obj.Faces.Add(new Face(7, 8, 9));
            obj.Faces.Add(new Face(7, 9, 10));
            obj.Faces.Add(new Face(7, 10, 11));
            obj.Faces.Add(new Face(7, 11, 12));
            obj.Faces.Add(new Face(7, 12, 8));

            obj.Faces.Add(new Face(2, 3, 8));
            obj.Faces.Add(new Face(8, 9, 3));
            obj.Faces.Add(new Face(3, 4, 9));
            obj.Faces.Add(new Face(9, 10, 4));
            obj.Faces.Add(new Face(4, 5, 10));
            obj.Faces.Add(new Face(10, 11, 5));
            obj.Faces.Add(new Face(5, 6, 11));
            obj.Faces.Add(new Face(11, 12, 6));
            obj.Faces.Add(new Face(6, 2, 12));
            obj.Faces.Add(new Face(12, 8, 2));

        }

        public void Hexahedron(ref Object3D obj, float a)
        {
            float R = a / (2 * (float)Math.Sin(Math.PI / 4));
            float r = R * (float)Math.Cos(Math.PI / 4);

            obj = new Object3D();

            obj.Vertices.Add(new Point3D(r, r, -r));
            obj.Vertices.Add(new Point3D(-r, r, -r));
            obj.Vertices.Add(new Point3D(-r, -r, -r));
            obj.Vertices.Add(new Point3D(r, -r, -r));
            obj.Vertices.Add(new Point3D(r, r, r));
            obj.Vertices.Add(new Point3D(-r, r, r));
            obj.Vertices.Add(new Point3D(-r, -r, r));
            obj.Vertices.Add(new Point3D(r, -r, r));

            obj.Faces.Add(new Face(1, 2, 3, 4));
            obj.Faces.Add(new Face(5, 6, 7, 8));
            obj.Faces.Add(new Face(1, 2, 6, 5));
            obj.Faces.Add(new Face(2, 3, 7, 6));
            obj.Faces.Add(new Face(3, 4, 8, 7));
            obj.Faces.Add(new Face(1, 4, 8, 5));
        }

        public void Tetrahedron(ref Object3D obj, float a)
        {
            float R = a / (2 * (float)Math.Sin(Math.PI / 3));
            float r = R * (float)Math.Cos(Math.PI / 3);

            obj = new Object3D();

            obj.Vertices.Add(new Point3D(0, 0, R));
            obj.Vertices.Add(new Point3D(R, 0, -r));
            obj.Vertices.Add(new Point3D(-r, a / 2, -r));
            obj.Vertices.Add(new Point3D(-r, -a / 2, -r));

            obj.Faces.Add(new Face(1, 2, 3));
            obj.Faces.Add(new Face(1, 3, 4));
            obj.Faces.Add(new Face(1, 2, 4));
            obj.Faces.Add(new Face(2, 3, 4));
        }

        public void Graph(ref Object3D obj, Func<float, float, float> f, float minx, float maxx, float miny, float maxy, int splits)
        {
            obj = new Object3D();

            float stepx = (maxx - minx) / splits;
            float stepy = (maxy - miny) / splits;

            for (int y = 0; y < splits; y++)
            {
                for (int x = 0; x < splits; x++)
                {
                    float x1 = minx + x * stepx;
                    float y1 = miny + y * stepy;

                    obj.Vertices.Add(new Point3D(x1, y1, f(x1, y1)));

                    if (x != 0 && y != 0)
                    {
                        int cur = y * splits + x + 1;
                        obj.Faces.Add(new Face(cur, cur - splits, cur - splits - 1, cur - 1));
                    }
                }
            }
        }

        public void RotationFigure(ref Object3D obj, List<Point3D> points, Axis axis, int splits)
        {
            obj = new Object3D();

            float step = 360 / splits;
            for (int i = 0; i <= splits; i++)
            {
                foreach(Point3D p in points)
                {
                    Point3D newP;
                    switch (axis) {
                        case Axis.X: newP = XRotatePoint(p, i * step); break;
                        case Axis.Y: newP = YRotatePoint(p, i * step); break;
                        case Axis.Z: newP = ZRotatePoint(p, i * step); break;
                        default: newP = new Point3D(0,0,0); break;
                    }
                    obj.Vertices.Add(newP);
                }
                if (i != 0)
                {
                    for (int j = 0; j < points.Count; j++)
                    {
                        obj.Faces.Add(new Face(
                            i * points.Count + j + 1,
                            i * points.Count + (j + 1) % points.Count + 1,
                            (i - 1) * points.Count + (j + 1) % points.Count + 1,
                            (i - 1) * points.Count + j % points.Count + 1));
                    }
                }
            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked) perspective = true;
            if (radioButton2.Checked) perspective = false;

            g.Clear(Color.White);
            DrawObject(obj);
            pictureBox.Refresh();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            float dx, dy, dz;
            if (float.TryParse(textBox1.Text, out dx) && float.TryParse(textBox2.Text, out dy) && float.TryParse(textBox3.Text, out dz))
            {
                obj.Vertices = obj.Vertices.Select(p => TranslatePoint(p, dx, dy, dz)).ToList();

                g.Clear(Color.White);
                DrawObject(obj);
                pictureBox.Refresh();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            float angle;
            if (float.TryParse(textBox4.Text, out angle))
            {
                if (radioButton3.Checked) obj.Vertices = obj.Vertices.Select(p => XRotatePoint(p, angle)).ToList();
                if (radioButton4.Checked) obj.Vertices = obj.Vertices.Select(p => YRotatePoint(p, angle)).ToList();
                if (radioButton5.Checked) obj.Vertices = obj.Vertices.Select(p => ZRotatePoint(p, angle)).ToList();

                g.Clear(Color.White);
                DrawObject(obj);
                pictureBox.Refresh();
            }
            
        }

        private void button3_Click(object sender, EventArgs e)
        {
            float mx, my, mz;
            if (float.TryParse(textBox5.Text, out mx) && float.TryParse(textBox6.Text, out my) && float.TryParse(textBox7.Text, out mz))
            {
                obj.Vertices = obj.Vertices.Select(p => ScalePoint(p, mx, my, mz)).ToList();

                g.Clear(Color.White);
                DrawObject(obj);
                pictureBox.Refresh();
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (radioButton6.Checked) obj.Vertices = obj.Vertices.Select(p => XYMirrorPoint(p)).ToList();
            if (radioButton7.Checked) obj.Vertices = obj.Vertices.Select(p => XZMirrorPoint(p)).ToList();
            if (radioButton8.Checked) obj.Vertices = obj.Vertices.Select(p => YZMirrorPoint(p)).ToList();

            g.Clear(Color.White);
            DrawObject(obj);
            pictureBox.Refresh();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            float mx, my, mz;
            if (float.TryParse(textBox8.Text, out mx) && float.TryParse(textBox9.Text, out my) && float.TryParse(textBox10.Text, out mz))
            {
                Scale(ref obj, mx, my, mz);

                g.Clear(Color.White);
                DrawObject(obj);
                pictureBox.Refresh();
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            float angle;
            if (float.TryParse(textBox11.Text, out angle))
            {
                if (radioButton9.Checked) XRotate(ref obj, angle);
                if (radioButton10.Checked) YRotate(ref obj, angle);
                if (radioButton11.Checked) ZRotate(ref obj, angle);

                g.Clear(Color.White);
                DrawObject(obj);
                pictureBox.Refresh();
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            float x1, y1, z1, x2, y2, z2, angle;
            if (float.TryParse(textBox12.Text, out x1) && float.TryParse(textBox13.Text, out y1) && float.TryParse(textBox14.Text, out z1) && 
                float.TryParse(textBox15.Text, out x2) && float.TryParse(textBox16.Text, out y2) && float.TryParse(textBox17.Text, out z2) && 
                float.TryParse(textBox18.Text, out angle))
            {
                Rotate(ref obj, new Point3D(x1, y1, z1), new Point3D(x2, y2, z2), angle);

                g.Clear(Color.White);
                DrawObject(obj);
                pictureBox.Refresh();
            }
        }

        private void save_button_Click(object sender, EventArgs e)
        {
            if (textBox19.Text.Length != 0)
            {
                StringBuilder sb = new StringBuilder();
                foreach(Point3D v in obj.Vertices)
                {
                    sb.AppendLine($"v {v.X} {v.Y} {v.Z}");
                }

                foreach (Face f in obj.Faces)
                {
                    sb.Append("f");
                    foreach (FaceIndices fi in f.FaceIndices)
                        sb.Append($" {fi.VertexIndex}/{fi.TextureCoordinateIndex}/{fi.NormalIndex}");
                    sb.AppendLine("");
                }

                File.WriteAllText(textBox19.Text + ".obj", sb.ToString());
            }
        }
    }

    public enum Axis
    {
        X, Y, Z
    }

    public class Point3D
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float W { get; set; }

        public Point3D(float x, float y, float z, float w = 1)
        {
            X = x; Y = y; Z = z;
            W = w;
        }

        public static Point3D operator +(Point3D a, Point3D b) => new Point3D(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        public static Point3D operator -(Point3D a, Point3D b) => new Point3D(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        public static Point3D operator /(Point3D a, float b) => new Point3D(a.X / b, a.Y / b, a.Z / b);
        public static Point3D operator *(Point3D a, float b) => new Point3D(a.X * b, a.Y * b, a.Z * b);
        public static float operator *(Point3D a, Point3D b) => a.X * b.X + a.Y * b.Y + a.Z * b.Z;
    }

    public class Face
    {
        public List<FaceIndices> FaceIndices;

        public Face()
        {
            FaceIndices = new List<FaceIndices>();
        }

        public Face(params int[] indexes)
        {
            FaceIndices = new List<FaceIndices>();
            foreach(int i in indexes)
                FaceIndices.Add(new FaceIndices(i));
        }
    }

    public class FaceIndices
    {
        public int VertexIndex { get; set; }
        public int TextureCoordinateIndex { get; set; }
        public int NormalIndex { get; set; }

        public FaceIndices(int v)
        {
            VertexIndex = v;
            TextureCoordinateIndex = v;
            NormalIndex = v;
        }

        public FaceIndices(int v, int vt, int vn)
        {
            VertexIndex = v;
            TextureCoordinateIndex = vt;
            NormalIndex = vn;
        }
    }

    public class Coordinates
    {
        public float U { get; set; }
        public float V { get; set; }
        public float W { get; set; }

        public Coordinates(float u, float v = 0, float w = 0)
        {
            U = u;
            V = v; W = w;
        }
    }

    public class Object3D
    {
        public List<Point3D> Vertices;
        public List<Face> Faces;
        public List<Point3D> Normals;
        public List<Coordinates> TextureCoordinates;
        public List<Coordinates> ParameterSpaceVertices;

        public Object3D()
        {
            Vertices = new List<Point3D>();
            Faces = new List<Face>();
            Normals = new List<Point3D>();
            TextureCoordinates = new List<Coordinates>();
            ParameterSpaceVertices = new List<Coordinates>();
        }

        public static Object3D Load_obj(string fname)
        {
            string[] file = File.ReadAllLines(fname);
            Object3D res = new Object3D();

            foreach (string s in file)
            {
                if (s == "" || s.Substring(0, 1) == "#" || s.Substring(0, 1) == "o" || (s.Length == 1 && s[0] != 'v' && s[0] != 'f')) continue;
                
                if (s.Substring(0, 2) == "vt")
                {
                    float[] parsed = s.Substring(3, s.Length - 3).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                    res.TextureCoordinates.Add(new Coordinates(parsed[0], parsed.Length < 2 ? 0 : parsed[1], parsed.Length < 3 ? 0 : parsed[2]));
                }
                else if (s.Substring(0, 2) == "vn")
                {
                    float[] parsed = s.Substring(3, s.Length - 3).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                    res.Normals.Add(new Point3D(parsed[0], parsed[1], parsed[2]));
                }
                else if (s.Substring(0, 2) == "vp")
                {
                    float[] parsed = s.Substring(3, s.Length - 3).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                    res.ParameterSpaceVertices.Add(new Coordinates(parsed[0], parsed.Length < 2 ? 0 : parsed[1], parsed.Length < 3 ? 0 : parsed[2]));
                }
                else if (s.Substring(0, 1) == "v")
                {
                    float[] parsed = s.Substring(2, s.Length - 2).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                    res.Vertices.Add(new Point3D(parsed[0], parsed[1], parsed[2], parsed.Length == 3 ? 1 : parsed[3]));
                }
                else if (s.Substring(0, 1) == "f")
                {
                    string[] parsed = s.Substring(2, s.Length - 2).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
                    Face f = new Face();
                    foreach (string v in parsed)
                    {
                        string[] vertex = v.Split('/');
                        FaceIndices faceIndices = new FaceIndices(int.Parse(vertex[0]));

                        if (vertex.Length > 1 && vertex[1] != "")
                        {
                            faceIndices.TextureCoordinateIndex = int.Parse(vertex[1]);
                            if (vertex.Length > 2)
                                faceIndices.NormalIndex = int.Parse(vertex[2]);
                        }

                        f.FaceIndices.Add(faceIndices);
                    }
                    res.Faces.Add(f);
                }
                
            }
            return res;
        }
    }

    public class Camera
    {
        public Point3D U { get; set; }
        public Point3D V { get; set; }
        public Point3D N { get; set; }
        public Point3D Location { get; set; }

        public Camera()
        {
            U = new Point3D(1, 0, 0);
            V = new Point3D(0, 1, 0);
            N = new Point3D(0, 0, 1);
            Location = new Point3D(0, 0, 0);
        }
    }

    

}
