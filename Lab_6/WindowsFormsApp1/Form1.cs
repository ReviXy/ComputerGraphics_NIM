using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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


            Icosahedron(ref obj, 50);

            obj.Vertexes = obj.Vertexes.Select(p => TranslatePoint(p, 0, 0, 100)).ToList();

            DrawObject(obj);
        }

        public void DrawObject_Axonometric(Object3D obj)
        {
            List<Point3D> vertexes = obj.Vertexes;

            vertexes = vertexes.Select(p => View(p, camera)).ToList();

            vertexes = vertexes.Select(p => Axonometric(p)).ToList();

            foreach (Point3D p in vertexes)
            {
                g.DrawRectangle(new Pen(Color.Red), p.X - 1, p.Y - 1, 2, 2);
            }

            foreach (Face face in obj.Faces)
            {
                for (int i = 0; i < face.VertexIndexes.Count; i++)
                {
                    Point3D p1 = vertexes[face.VertexIndexes[i]];
                    Point3D p2 = vertexes[face.VertexIndexes[(i + 1) % face.VertexIndexes.Count]];
                    g.DrawLine(new Pen(Color.Black),
                        p1.X,
                        p1.Y,
                        p2.X,
                        p2.Y);

                }
            }
        }

        public void DrawObject_Perspective(Object3D obj)
        {
            List<Point3D> vertexes = obj.Vertexes;

            vertexes = vertexes.Select(p => View(p, camera)).ToList();

            vertexes = vertexes.Select(p => Perspective(p)).ToList();

            foreach (Point3D p in vertexes)
            {
                g.DrawRectangle(new Pen(Color.Red), p.X - 1, p.Y - 1, 2, 2);
            }

            foreach (Face face in obj.Faces)
            {
                for (int i = 0; i < face.VertexIndexes.Count; i++)
                {
                    Point3D p1 = vertexes[face.VertexIndexes[i]];
                    Point3D p2 = vertexes[face.VertexIndexes[(i + 1) % face.VertexIndexes.Count]];
                    g.DrawLine(new Pen(Color.Black),
                        p1.X,
                        p1.Y,
                        p2.X,
                        p2.Y);

                }
            }
        }

        public void DrawObject(Object3D obj)
        {
            List<Point3D> vertexes = obj.Vertexes;

            vertexes = vertexes.Select(p => View(p, camera)).ToList();

            if (perspective) vertexes = vertexes.Select(p => Perspective(p)).ToList();
            else vertexes = vertexes.Select(p => Axonometric(p)).ToList();

            foreach (Point3D p in vertexes)
            {
                g.DrawRectangle(new Pen(Color.Red), p.X - 1, p.Y - 1, 2, 2);
            }

            foreach (Face face in obj.Faces)
            {
                for (int i = 0; i < face.VertexIndexes.Count; i++)
                {
                    Point3D p1 = vertexes[face.VertexIndexes[i]];
                    Point3D p2 = vertexes[face.VertexIndexes[(i + 1) % face.VertexIndexes.Count]];
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
            float c = -pictureBox.Width * 0.8f;

            float[][] PerspectiveMatrix = new float[4][]
            {
                    new float[4] { 1, 0, 0, 0},
                    new float[4] { 0, 1, 0, 0 },
                    new float[4] { 0, 0, 0, -1/c },
                    new float[4] { 0, 0, 0, 1 }
            };

            Point3D temp = MultiplyMatrix(PerspectiveMatrix, p);
            return new Point3D(p.X / temp.W, p.Y / temp.W, 0, p.W);
        }

        public Point3D Axonometric(Point3D p)
        {
            float phi = (float)((40 / 180D) * Math.PI); ;
            float ksi = (float)((90 / 180D) * Math.PI); ;

            float[][] AxonometricMatrix = new float[4][]
            {
                    new float[4] { (float)Math.Cos(ksi), (float)(Math.Sin(phi) * Math.Sin(ksi)), 0, 0},
                    new float[4] { 0, (float)Math.Cos(phi), 0, 0 },
                    new float[4] { (float)Math.Sin(ksi), -(float)(Math.Sin(phi) * Math.Cos(ksi)), 0, 0 },
                    new float[4] { 0, 0, 0, 1 }
            };

            return MultiplyMatrix(AxonometricMatrix, p);
        }

        public void Scale(ref Object3D obj, float mx, float my, float mz)
        {
            Point3D center = new Point3D(0, 0, 0);
            foreach(Point3D p in obj.Vertexes)
                center += p;
            center /= obj.Vertexes.Count;

            obj.Vertexes = obj.Vertexes.Select(p => TranslatePoint(p, -center.X, -center.Y, -center.Z)).ToList();
            obj.Vertexes = obj.Vertexes.Select(p => ScalePoint(p, mx, my, mz)).ToList();
            obj.Vertexes = obj.Vertexes.Select(p => TranslatePoint(p, center.X, center.Y, center.Z)).ToList();
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
                    new float[4] { l*l + cos*(1 - l*l), l*(1 - cos)*m + n * sin, l*(1 - cos)*n - m * sin, 0},
                    new float[4] { l*(1 - cos)*m - n * sin, m*m + cos*(1 - m*m), m*(1 - cos)*n + l*sin, 0 },
                    new float[4] { l*(1 - cos)*n + m * sin, m*(1 - cos)*n - l*sin, n*n + cos*(1 - n*n), 0 },
                    new float[4] { 0, 0, 0, 1 }
            };

            Point3D center = new Point3D(0, 0, 0);
            foreach (Point3D p in obj.Vertexes)
                center += p;
            center /= obj.Vertexes.Count;

            obj.Vertexes = obj.Vertexes.Select(p => MultiplyMatrix(RotateMatrix, p)).ToList();
        }

        public void XRotate(ref Object3D obj, float angle)
        {
            Point3D center = new Point3D(0, 0, 0);
            foreach (Point3D p in obj.Vertexes)
                center += p;
            center /= obj.Vertexes.Count;

            obj.Vertexes = obj.Vertexes.Select(p => TranslatePoint(p, -center.X, -center.Y, -center.Z)).ToList();
            obj.Vertexes = obj.Vertexes.Select(p => XRotatePoint(p, angle)).ToList();
            obj.Vertexes = obj.Vertexes.Select(p => TranslatePoint(p, center.X, center.Y, center.Z)).ToList();
        }

        public void YRotate(ref Object3D obj, float angle)
        {
            Point3D center = new Point3D(0, 0, 0);
            foreach (Point3D p in obj.Vertexes)
                center += p;
            center /= obj.Vertexes.Count;

            obj.Vertexes = obj.Vertexes.Select(p => TranslatePoint(p, -center.X, -center.Y, -center.Z)).ToList();
            obj.Vertexes = obj.Vertexes.Select(p => YRotatePoint(p, angle)).ToList();
            obj.Vertexes = obj.Vertexes.Select(p => TranslatePoint(p, center.X, center.Y, center.Z)).ToList();
        }

        public void ZRotate(ref Object3D obj, float angle)
        {
            Point3D center = new Point3D(0, 0, 0);
            foreach (Point3D p in obj.Vertexes)
                center += p;
            center /= obj.Vertexes.Count;

            obj.Vertexes = obj.Vertexes.Select(p => TranslatePoint(p, -center.X, -center.Y, -center.Z)).ToList();
            obj.Vertexes = obj.Vertexes.Select(p => ZRotatePoint(p, angle)).ToList();
            obj.Vertexes = obj.Vertexes.Select(p => TranslatePoint(p, center.X, center.Y, center.Z)).ToList();
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
                foreach (int i in f.VertexIndexes)
                {
                    sum += temp.Vertexes[i];
                }
                obj.Vertexes.Add(new Point3D(a * sum.X / 3, a * sum.Y / 3, a * sum.Z / 3));
            }

            float k = a / (float)Math.Sqrt(Math.Pow(obj.Vertexes[0].X - obj.Vertexes[1].X, 2) + Math.Pow(obj.Vertexes[0].Y - obj.Vertexes[1].Y, 2) + Math.Pow(obj.Vertexes[0].Z - obj.Vertexes[1].Z, 2));
            for (int i = 0; i < obj.Vertexes.Count; i++)
                obj.Vertexes[i] = obj.Vertexes[i] * k;

            obj.Faces.Add(new Face(0, 1, 2, 3, 4));
            obj.Faces.Add(new Face(5, 6, 7, 8, 9));

            obj.Faces.Add(new Face(0, 1, 12, 11, 10));
            obj.Faces.Add(new Face(1, 2, 14, 13, 12));
            obj.Faces.Add(new Face(2, 3, 16, 15, 14));
            obj.Faces.Add(new Face(3, 4, 18, 17, 16));
            obj.Faces.Add(new Face(4, 0, 10, 19, 18));

            obj.Faces.Add(new Face(5, 6, 13, 12, 11));
            obj.Faces.Add(new Face(6, 7, 15, 14, 13));
            obj.Faces.Add(new Face(7, 8, 17, 16, 15));
            obj.Faces.Add(new Face(8, 9, 19, 18, 17));
            obj.Faces.Add(new Face(9, 5, 11, 10, 19));

        }

        public void Icosahedron(ref Object3D obj, float a)
        {
            float R = a / (2 * (float)Math.Sin(Math.PI / 6));
            float r = R * (float)Math.Cos(Math.PI / 6);

            float step = 360 / 5;

            obj = new Object3D();

            obj.Vertexes.Add(new Point3D(0, 0, R));
            for (float angle = 0; angle < 360; angle += step)
            {
                obj.Vertexes.Add(new Point3D(R * (float)Math.Cos(angle / 180D * Math.PI), R * (float)Math.Sin(angle / 180D * Math.PI), a / 2));
            }

            obj.Vertexes.Add(new Point3D(0, 0, -R));
            for (float angle = step / 2; angle < 360; angle += step)
            {
                obj.Vertexes.Add(new Point3D(R * (float)Math.Cos(angle / 180D * Math.PI), R * (float)Math.Sin(angle / 180D * Math.PI), -a / 2));
            }

            obj.Faces.Add(new Face(0, 1, 2));
            obj.Faces.Add(new Face(0, 2, 3));
            obj.Faces.Add(new Face(0, 3, 4));
            obj.Faces.Add(new Face(0, 4, 5));
            obj.Faces.Add(new Face(0, 5, 1));

            obj.Faces.Add(new Face(6, 7, 8));
            obj.Faces.Add(new Face(6, 8, 9));
            obj.Faces.Add(new Face(6, 9, 10));
            obj.Faces.Add(new Face(6, 10, 11));
            obj.Faces.Add(new Face(6, 11, 7));

            obj.Faces.Add(new Face(1, 2, 7));
            obj.Faces.Add(new Face(7, 8, 2));
            obj.Faces.Add(new Face(2, 3, 8));
            obj.Faces.Add(new Face(8, 9, 3));
            obj.Faces.Add(new Face(3, 4, 9));
            obj.Faces.Add(new Face(9, 10, 4));
            obj.Faces.Add(new Face(4, 5, 10));
            obj.Faces.Add(new Face(10, 11, 5));
            obj.Faces.Add(new Face(5, 1, 11));
            obj.Faces.Add(new Face(11, 7, 1));

        }

        public void Hexahedron(ref Object3D obj, float a)
        {
            float R = a / (2 * (float)Math.Sin(Math.PI / 4));
            float r = R * (float)Math.Cos(Math.PI / 4);

            obj = new Object3D();

            obj.Vertexes.Add(new Point3D(r, r, -r));
            obj.Vertexes.Add(new Point3D(-r, r, -r));
            obj.Vertexes.Add(new Point3D(-r, -r, -r));
            obj.Vertexes.Add(new Point3D(r, -r, -r));
            obj.Vertexes.Add(new Point3D(r, r, r));
            obj.Vertexes.Add(new Point3D(-r, r, r));
            obj.Vertexes.Add(new Point3D(-r, -r, r));
            obj.Vertexes.Add(new Point3D(r, -r, r));

            obj.Faces.Add(new Face(0, 1, 2, 3));
            obj.Faces.Add(new Face(4, 5, 6, 7));
            obj.Faces.Add(new Face(0, 1, 5, 4));
            obj.Faces.Add(new Face(1, 2, 6, 5));
            obj.Faces.Add(new Face(2, 3, 7, 6));
            obj.Faces.Add(new Face(0, 3, 7, 4));
        }

        public void Tetrahedron(ref Object3D obj, float a)
        {
            float R = a / (2 * (float)Math.Sin(Math.PI / 3));
            float r = R * (float)Math.Cos(Math.PI / 3);

            obj = new Object3D();

            obj.Vertexes.Add(new Point3D(0, 0, R));
            obj.Vertexes.Add(new Point3D(R, 0, -r));
            obj.Vertexes.Add(new Point3D(-r, a / 2, -r));
            obj.Vertexes.Add(new Point3D(-r, -a / 2, -r));

            obj.Faces.Add(new Face(0, 1, 2));
            obj.Faces.Add(new Face(0, 2, 3));
            obj.Faces.Add(new Face(0, 1, 3));
            obj.Faces.Add(new Face(1, 2, 3));
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
                obj.Vertexes = obj.Vertexes.Select(p => TranslatePoint(p, dx, dy, dz)).ToList();

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
                if (radioButton3.Checked) obj.Vertexes = obj.Vertexes.Select(p => XRotatePoint(p, angle)).ToList();
                if (radioButton4.Checked) obj.Vertexes = obj.Vertexes.Select(p => YRotatePoint(p, angle)).ToList();
                if (radioButton5.Checked) obj.Vertexes = obj.Vertexes.Select(p => ZRotatePoint(p, angle)).ToList();

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
                obj.Vertexes = obj.Vertexes.Select(p => ScalePoint(p, mx, my, mz)).ToList();

                g.Clear(Color.White);
                DrawObject(obj);
                pictureBox.Refresh();
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (radioButton6.Checked) obj.Vertexes = obj.Vertexes.Select(p => XYMirrorPoint(p)).ToList();
            if (radioButton7.Checked) obj.Vertexes = obj.Vertexes.Select(p => XZMirrorPoint(p)).ToList();
            if (radioButton8.Checked) obj.Vertexes = obj.Vertexes.Select(p => YZMirrorPoint(p)).ToList();

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
        public List<int> VertexIndexes;

        public Face(params int[] indexes)
        {
            VertexIndexes = new List<int>();
            foreach(int i in indexes)
                VertexIndexes.Add(i);
        }
    }

    public class Object3D
    {
        public List<Point3D> Vertexes;
        public List<Face> Faces;

        public Object3D()
        {
            Vertexes = new List<Point3D>();
            Faces = new List<Face>();
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
