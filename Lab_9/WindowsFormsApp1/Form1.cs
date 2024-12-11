using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
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
        Light light;
        int toonShadingColorSteps;

        List<Object3D> objects;

        static string filename = "skull.jpg";
        static Bitmap texture = new Bitmap(filename);

        float[,] ZBuffer;

        public Form1()
        {
            InitializeComponent();

            bm = new Bitmap(pictureBox.Width, pictureBox.Height);
            pictureBox.Image = bm;
            g = Graphics.FromImage(pictureBox.Image);
            g.Clear(Color.White);
            g.TranslateTransform(pictureBox.ClientSize.Width / 2, pictureBox.ClientSize.Height / 2);
            g.ScaleTransform(1, -1);

            ZBuffer = new float[pictureBox.Width, pictureBox.Height];

            objects = new List<Object3D>();

            objectDropDown.Items.Clear();
            objectDropDown.Items.Add("");
            objectDropDown.SelectedIndex = 0;

            camera = new Camera();
            camera.Location = new Point3D(0, 0, 0);
            //camera.ViewVector = new Point3D(0, 0, -1);
            camera.Rotation = new Point3D(0, 0, 0);

            light = new Light();
            light.Location = new Point3D(1000, 0, 0);
            light.Ka = 0.15f;
            light.Kd = 0.8f;
            light.Ks = 0.4f;

            Object3D cube1 = Object3D.Load_obj("cube.obj");
            cube1.color = Color.Blue;
            Triangulate(ref cube1);
            objects.Add(cube1);
            objectDropDown.Items.Add("cube1");

            //Object3D teapot = Object3D.Load_obj("teapot.obj");
            //teapot.color = Color.Red;
            //Triangulate(ref teapot);
            //objects.Add(teapot);
            //objectDropDown.Items.Add("Teapot");

            //Object3D sphere = Object3D.Load_obj("sphere.obj");
            //sphere.color = Color.Red;
            //Triangulate(ref sphere);
            //objects.Add(sphere);
            //objectDropDown.Items.Add("Sphere");

            //___________________________________________________

            //Object3D skull = Object3D.Load_obj("skull.obj");
            //Triangulate(ref skull);
            //objects.Add(skull);
            //objectDropDown.Items.Add("Skull");

            DrawObjects();
        }

        public void Triangulate(ref Object3D obj)
        {
            List<Face> faces = new List<Face>();
            foreach(Face f in obj.Faces)
            {
                if (f.FaceIndices.Count == 3)
                {
                    faces.Add(f);
                    continue;
                }
                
                for (int i = 2; i < f.FaceIndices.Count; i++)
                {
                    Face newf = new Face();
                    newf.FaceIndices.Add(f.FaceIndices[0]);
                    newf.FaceIndices.Add(f.FaceIndices[i - 1]);
                    newf.FaceIndices.Add(f.FaceIndices[i]);
                    faces.Add(newf);
                }

            }
            obj.Faces = faces;
        }

        public void CalculateVertexNormals(ref Object3D obj)
        {
            obj.Normals.Clear();
            for (int i = 1; i <= obj.Vertices.Count; i++)
            {
                List<Point3D> normals = new List<Point3D>();
                foreach(Face f in obj.Faces)
                {
                    if (f.FaceIndices.Where(x => x.VertexIndex == i).Count() != 0)
                    {
                        Point3D v1 = obj.Vertices[f.FaceIndices[1].VertexIndex - 1] - obj.Vertices[f.FaceIndices[0].VertexIndex - 1];
                        Point3D v2 = obj.Vertices[f.FaceIndices[2].VertexIndex - 1] - obj.Vertices[f.FaceIndices[0].VertexIndex - 1];

                        Point3D normal = new Point3D(v1.Y * v2.Z - v1.Z * v2.Y, v1.Z * v2.X - v1.X * v2.Z, v1.X * v2.Y - v1.Y * v2.X);
                        normal.Normalize();

                        normals.Add(normal);
                    }
                }

                Point3D vertexNormal = new Point3D(0, 0, 0);
                foreach (Point3D n in normals) vertexNormal += n;
                vertexNormal /= normals.Count();

                obj.Normals.Add(vertexNormal);
            }
        }

        public void CalculateVertexNormals(ref Object3D obj, List<Point3D> vertices)
        {
            obj.Normals.Clear();
            for (int i = 1; i <= vertices.Count; i++)
            {
                List<Point3D> normals = new List<Point3D>();
                foreach (Face f in obj.Faces)
                {
                    if (f.FaceIndices.Where(x => x.VertexIndex == i).Count() != 0)
                    {
                        Point3D v1 = vertices[f.FaceIndices[1].VertexIndex - 1] - vertices[f.FaceIndices[0].VertexIndex - 1];
                        Point3D v2 = vertices[f.FaceIndices[2].VertexIndex - 1] - vertices[f.FaceIndices[0].VertexIndex - 1];

                        Point3D normal = new Point3D(v1.Y * v2.Z - v1.Z * v2.Y, v1.Z * v2.X - v1.X * v2.Z, v1.X * v2.Y - v1.Y * v2.X);
                        normal.Normalize();

                        normals.Add(normal);
                    }
                }

                Point3D vertexNormal = new Point3D(0, 0, 0);
                foreach (Point3D n in normals) vertexNormal += n;
                vertexNormal /= normals.Count();

                obj.Normals.Add(vertexNormal);
            }
        }

        public void DrawObjects()
        {
            for(int i = 0; i < pictureBox.Width; i++)
                for (int j = 0; j < pictureBox.Height; j++)
                    ZBuffer[i,j] = float.MaxValue;

            foreach(Object3D obj in objects)
            {
                DrawObject(obj);
            }
        }

        public void DrawObject(Object3D obj)
        {
            List<Point3D> vertexes = obj.Vertices;
            List<Point3D> normals = obj.Normals;

            Point3D center = new Point3D(0, 0, 0);
            foreach (Point3D p in obj.Vertices)
                center += p;
            center /= obj.Vertices.Count;

            vertexes = vertexes.Select(p => View(p, camera, center)).ToList();
            light.ViewLocation = View(light.Location, camera, light.Location);

            normals = normals.Select(p => View(p, camera, p)).ToList();

            switch (camera.Projection)
            {
                case Projection.Perspective: vertexes = vertexes.Select(p => Perspective(p)).ToList(); light.ViewLocation = Perspective(light.Location); break;
                case Projection.Axonometric: vertexes = vertexes.Select(p => Axonometric(p)).ToList(); light.ViewLocation = Axonometric(light.Location); break;
                default: break;
            }
            
            g.DrawRectangle(new Pen(Color.Red, 3), light.ViewLocation.X, light.ViewLocation.Y, 2, 2);

            foreach (Face face in obj.Faces)
            {
                Point3D v1 = vertexes[face.FaceIndices[1].VertexIndex - 1] - vertexes[face.FaceIndices[0].VertexIndex - 1];
                Point3D v2 = vertexes[face.FaceIndices[2].VertexIndex - 1] - vertexes[face.FaceIndices[0].VertexIndex - 1];

                Point3D normal = new Point3D(v1.Y * v2.Z - v1.Z * v2.Y, v1.Z * v2.X - v1.X * v2.Z, v1.X * v2.Y - v1.Y * v2.X);
                normal.Normalize();

                if (normal * camera.ViewVector < 0) continue;

                List<Point3D> points = new List<Point3D>();

                // LIVSEY

                //List<Coordinates> textureCoords = new List<Coordinates>();
                //foreach (FaceIndices fi in face.FaceIndices)
                //{
                //    points.Add(vertexes[fi.VertexIndex - 1]);
                //    textureCoords.Add(obj.TextureCoordinates[fi.TextureCoordinateIndex - 1]);
                //}
                //Rasterization_Linear_Texture(points, textureCoords, texture);

                // ___ TEAM FORTRESS 2 ___
                //toonShadingColorSteps = 5;
                //List<Point3D> normals1 = new List<Point3D>();
                //foreach (FaceIndices fi in face.FaceIndices)
                //{
                //    points.Add(vertexes[fi.VertexIndex - 1]);
                //    normals1.Add(normals[fi.NormalIndex - 1]);
                //}
                //Rasterization_TF2(points, normals1, obj.color, light, toonShadingColorSteps);

                // ___ GAWR GURO ___
                List<Color> colors = new List<Color>();
                foreach (FaceIndices fi in face.FaceIndices)
                {
                    Point3D l = light.ViewLocation - vertexes[fi.VertexIndex - 1]; // Light to point
                    l.Normalize();

                    Point3D n = normals[fi.NormalIndex - 1];
                    n.Normalize();

                    float nl = n * l;

                    float D = Clamp(Math.Max(0.0f, light.Kd * nl), 0.0f, 1.0f);

                    int R = (int)Clamp(obj.color.R * (light.Ka + D), 0, 255);
                    int G = (int)Clamp(obj.color.G * (light.Ka + D), 0, 255);
                    int B = (int)Clamp(obj.color.B * (light.Ka + D), 0, 255);

                    colors.Add(Color.FromArgb(R, G, B));
                    points.Add(vertexes[fi.VertexIndex - 1]);
                }
                Rasterization(points, colors);

                //___ FRAME ___
                //for (int i = 0; i < face.FaceIndices.Count; i++)
                //{
                //    Point3D p1 = vertexes[face.FaceIndices[i].VertexIndex - 1];
                //    Point3D p2 = vertexes[face.FaceIndices[(i + 1) % face.FaceIndices.Count].VertexIndex - 1];
                //    g.DrawLine(new Pen(Color.Black),
                //        p1.X,
                //        p1.Y,
                //        p2.X,
                //        p2.Y);
                //}
            }
        }

        int Interpolation(float x0, float y0, float x1, float y1, float x)
        {
            return (int)Math.Round(y0 + (float)(y1 - y0) * (x - x0) / (x1 - x0));
        }

        float Interpolation1(float x0, float y0, float x1, float y1, float x)
        {
            return y0 + (float)(y1 - y0) * (x - x0) / (x1 - x0);
        }

        float Clamp(float x, float min, float max)
        {
            return Math.Min(Math.Max(x, min), max);
        }

        void Rasterization_Linear_Texture(List<Point3D> points, List<Coordinates> textureCoords, Bitmap bm)
        {
            points = points.Select(p => new Point3D((float)Math.Round(p.X), (float)Math.Round(p.Y), p.Z, p.W)).ToList();

            List<(Point3D, Coordinates)> temp = (new List<int> { 0, 1, 2 }).Select(i => (points[i], textureCoords[i])).ToList();

            temp.Sort((a, b) => a.Item1.Y == b.Item1.Y ? 0 : (a.Item1.Y < b.Item1.Y ? -1 : 1));

            points = temp.Select(x => x.Item1).ToList();
            textureCoords = temp.Select(x => x.Item2).ToList();

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

            for (int i = (int)(points[0].Y); i < (int)(points[1].Y); i++)
            {
                float tLeftU = Interpolation1(points[0].Y, textureCoords[0].U, points[left].Y, textureCoords[left].U, i);
                float tLeftV = Interpolation1(points[0].Y, textureCoords[0].V, points[left].Y, textureCoords[left].V, i);

                float tRightU = Interpolation1(points[0].Y, textureCoords[0].U, points[right].Y, textureCoords[right].U, i);
                float tRightV = Interpolation1(points[0].Y, textureCoords[0].V, points[right].Y, textureCoords[right].V, i);

                int zLeft = Interpolation(points[0].Y, points[0].Z, points[left].Y, points[left].Z, i);
                int zRight = Interpolation(points[0].Y, points[0].Z, points[right].Y, points[right].Z, i);

                for (int j = (int)x1; j < (int)x2; j++)
                {
                    float U = Interpolation1((int)x1, tLeftU, (int)x2, tRightU, j);
                    float V = Interpolation1((int)x1, tLeftV, (int)x2, tRightV, j);

                    int z = Interpolation((int)x1, zLeft, (int)x2, zRight, j);
                    if (pictureBox.Width / 2 + j > pictureBox.Width - 1 || pictureBox.Width / 2 + j < 0 || pictureBox.Height / 2 + i > pictureBox.Height - 1 || pictureBox.Height / 2 + i < 0) continue;
                    if (ZBuffer[pictureBox.Width / 2 + j, pictureBox.Height / 2 + i] > z)
                    {
                        ZBuffer[pictureBox.Width / 2 + j, pictureBox.Height / 2 + i] = z;
                        g.DrawRectangle(new Pen(bm.GetPixel((int)((bm.Width) * U), bm.Height - (int)((bm.Height) * V))), j, i, 1, 1);
                    }
                }
                x1 += inc13;
                x2 += inc12;
            }

            if (points[0].Y == points[1].Y)
            {
                x1 = Math.Min(points[0].X, points[1].X);
                x2 = Math.Max(points[0].X, points[1].X);
            }

            if (_inc13 < inc23)
                (_inc13, inc23) = (inc23, _inc13);

            (left, right) = Interpolation(points[0].Y, points[0].X, points[2].Y, points[2].X, points[1].Y) < points[1].X ? (0, 1) : (1, 0);

            for (int i = (int)(points[1].Y); i < (int)(points[2].Y); i++)
            {
                float tLeftU = Interpolation1(points[2].Y, textureCoords[2].U, points[left].Y, textureCoords[left].U, i);
                float tLeftV = Interpolation1(points[2].Y, textureCoords[2].V, points[left].Y, textureCoords[left].V, i);

                float tRightU = Interpolation1(points[2].Y, textureCoords[2].U, points[right].Y, textureCoords[right].U, i);
                float tRightV = Interpolation1(points[2].Y, textureCoords[2].V, points[right].Y, textureCoords[right].V, i);

                int zLeft = Interpolation(points[2].Y, points[2].Z, points[left].Y, points[left].Z, i);
                int zRight = Interpolation(points[2].Y, points[2].Z, points[right].Y, points[right].Z, i);

                for (int j = (int)x1; j < (int)x2; j++)
                {
                    float U = Interpolation1((int)x1, tLeftU, (int)x2, tRightU, j);
                    float V = Interpolation1((int)x1, tLeftV, (int)x2, tRightV, j);

                    int z = Interpolation((int)x1, zLeft, (int)x2, zRight, j);
                    if (pictureBox.Width / 2 + j > pictureBox.Width - 1 || pictureBox.Width / 2 + j < 0 || pictureBox.Height / 2 + i > pictureBox.Height - 1 || pictureBox.Height / 2 + i < 0) continue;
                    if (ZBuffer[pictureBox.Width / 2 + j, pictureBox.Height / 2 + i] > z)
                    {
                        ZBuffer[pictureBox.Width / 2 + j, pictureBox.Height / 2 + i] = z;
                        g.DrawRectangle(new Pen(bm.GetPixel((int)((bm.Width) * U), bm.Height - (int)((bm.Height) * V))), j, i, 1, 1);
                    }
                }
                x1 += _inc13;
                x2 += inc23;
            }

        }

        void Rasterization_TF2(List<Point3D> points, List<Point3D> normals, Color color, Light light, int colorSubdivision)
        {
            points = points.Select(p => new Point3D((float)Math.Round(p.X), (float)Math.Round(p.Y), p.Z, p.W)).ToList();
            List<(Point3D, Point3D)> temp = (new List<int> { 0, 1, 2 }).Select(i => (points[i], normals[i])).ToList();

            temp.Sort((a, b) => a.Item1.Y == b.Item1.Y ? 0 : (a.Item1.Y < b.Item1.Y ? -1 : 1));

            points = temp.Select(x => x.Item1).ToList();
            normals = temp.Select(x => x.Item2).ToList();

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

            for (int i = (int)(points[0].Y); i < (int)(points[1].Y); i++)
            {
                float nLeftX = Interpolation1(points[0].Y, normals[0].X, points[left].Y, normals[left].X, i);
                float nLeftY = Interpolation1(points[0].Y, normals[0].Y, points[left].Y, normals[left].Y, i);
                float nLeftZ = Interpolation1(points[0].Y, normals[0].Z, points[left].Y, normals[left].Z, i);

                float nRightX = Interpolation1(points[0].Y, normals[0].X, points[right].Y, normals[right].X, i);
                float nRightY = Interpolation1(points[0].Y, normals[0].Y, points[right].Y, normals[right].Y, i);
                float nRightZ = Interpolation1(points[0].Y, normals[0].Z, points[right].Y, normals[right].Z, i);

                int zLeft = Interpolation(points[0].Y, points[0].Z, points[left].Y, points[left].Z, i);
                int zRight = Interpolation(points[0].Y, points[0].Z, points[right].Y, points[right].Z, i);

                for (int j = (int)x1; j < (int)x2; j++)
                {
                    float X = Interpolation1((int)x1, nLeftX, (int)x2, nRightX, j);
                    float Y = Interpolation1((int)x1, nLeftY, (int)x2, nRightY, j);
                    float Z = Interpolation1((int)x1, nLeftZ, (int)x2, nRightZ, j);

                    int z = Interpolation((int)x1, zLeft, (int)x2, zRight, j);
                    if (pictureBox.Width / 2 + j > pictureBox.Width - 1 || pictureBox.Width / 2 + j < 0 || pictureBox.Height / 2 + i > pictureBox.Height - 1 || pictureBox.Height / 2 + i < 0) continue;
                    if (ZBuffer[pictureBox.Width / 2 + j, pictureBox.Height / 2 + i] > z)
                    {
                        ZBuffer[pictureBox.Width / 2 + j, pictureBox.Height / 2 + i] = z;

                        Point3D v = new Point3D(-j, -i, -z);
                        v.Normalize();

                        Point3D l = light.ViewLocation - new Point3D(j, i, z); // Light to point
                        l.Normalize();

                        Point3D n = new Point3D(X, Y, Z);
                        n.Normalize();

                        float nl = n * l;
                        Point3D h = n; h *= 2 * nl; h -= l; // h = 2*nl*n - l  
                        h.Normalize();
                        float nh = Math.Max(0.0f, h * v);

                        float D = Clamp(Math.Max(0.0f, light.Kd * nl), 0.0f, 1.0f);
                        float S = Clamp(light.Ks * (float)Math.Pow(nh, 2), 0.0f, 1.0f);
                        if (D == 0) S = 0;

                        int R = (int)Clamp((color.R * (light.Ka + D) + 255f * S), 0, 255);
                        int G = (int)Clamp((color.G * (light.Ka + D) + 255f * S), 0, 255);
                        int B = (int)Clamp((color.B * (light.Ka + D) + 255f * S), 0, 255);

                        int step = (int)((255 - (color.R * light.Ka)) / colorSubdivision);
                        R = Interpolation(color.R * light.Ka, 0, 255, colorSubdivision, R) * step + (int)(color.R * light.Ka);
                        step = (int)((255 - (color.G * light.Ka)) / colorSubdivision);
                        G = Interpolation(color.G * light.Ka, 0, 255, colorSubdivision, G) * step + (int)(color.G * light.Ka);
                        step = (int)((255 - (color.B * light.Ka)) / colorSubdivision);
                        B = Interpolation(color.B * light.Ka, 0, 255, colorSubdivision, B) * step + (int)(color.B * light.Ka);

                        Color newColor = Color.FromArgb(R, G, B);

                        g.DrawRectangle(new Pen(newColor), j, i, 1, 1);
                    }
                }
                x1 += inc13;
                x2 += inc12;
            }

            if (points[0].Y == points[1].Y)
            {
                x1 = Math.Min(points[0].X, points[1].X);
                x2 = Math.Max(points[0].X, points[1].X);
            }

            if (_inc13 < inc23)
                (_inc13, inc23) = (inc23, _inc13);

            (left, right) = Interpolation(points[0].Y, points[0].X, points[2].Y, points[2].X, points[1].Y) < points[1].X ? (0, 1) : (1, 0);

            for (int i = (int)(points[1].Y); i < (int)(points[2].Y); i++)
            {
                float nLeftX = Interpolation1(points[2].Y, normals[2].X, points[left].Y, normals[left].X, i);
                float nLeftY = Interpolation1(points[2].Y, normals[2].Y, points[left].Y, normals[left].Y, i);
                float nLeftZ = Interpolation1(points[2].Y, normals[2].Z, points[left].Y, normals[left].Z, i);

                float nRightX = Interpolation1(points[2].Y, normals[2].X, points[right].Y, normals[right].X, i);
                float nRightY = Interpolation1(points[2].Y, normals[2].Y, points[right].Y, normals[right].Y, i);
                float nRightZ = Interpolation1(points[2].Y, normals[2].Z, points[right].Y, normals[right].Z, i);

                int zLeft = Interpolation(points[2].Y, points[2].Z, points[left].Y, points[left].Z, i);
                int zRight = Interpolation(points[2].Y, points[2].Z, points[right].Y, points[right].Z, i);

                for (int j = (int)x1; j < (int)x2; j++)
                {
                    float X = Interpolation1((int)x1, nLeftX, (int)x2, nRightX, j);
                    float Y = Interpolation1((int)x1, nLeftY, (int)x2, nRightY, j);
                    float Z = Interpolation1((int)x1, nLeftZ, (int)x2, nRightZ, j);

                    int z = Interpolation((int)x1, zLeft, (int)x2, zRight, j);
                    if (pictureBox.Width / 2 + j > pictureBox.Width - 1 || pictureBox.Width / 2 + j < 0 || pictureBox.Height / 2 + i > pictureBox.Height - 1 || pictureBox.Height / 2 + i < 0) continue;
                    if (ZBuffer[pictureBox.Width / 2 + j, pictureBox.Height / 2 + i] > z)
                    {
                        ZBuffer[pictureBox.Width / 2 + j, pictureBox.Height / 2 + i] = z;

                        Point3D v = new Point3D(-j, -i, -z);
                        v.Normalize();

                        Point3D l = light.ViewLocation - new Point3D(j, i, z); // Light to point
                        l.Normalize();

                        Point3D n = new Point3D(X, Y, Z);
                        n.Normalize();

                        float nl = l * n;
                        Point3D h = n; h *= 2 * nl; h -= l;
                        h.Normalize();
                        float nh = Math.Max(0.0f, h * v);

                        float D = Clamp(Math.Max(0.0f, light.Kd * nl), 0.0f, 1.0f);
                        float S = Clamp(light.Ks * (float)Math.Pow(nh, 2), 0.0f, 1.0f);
                        if (D == 0) S = 0;

                        int R = (int)Clamp((color.R * (light.Ka + D) + 255f * S), 0, 255);
                        int G = (int)Clamp((color.G * (light.Ka + D) + 255f * S), 0, 255);
                        int B = (int)Clamp((color.B * (light.Ka + D) + 255f * S), 0, 255);

                        int step = (int)((255 - (color.R * light.Ka)) / colorSubdivision);
                        R = Interpolation(color.R * light.Ka, 0, 255, colorSubdivision, R) * step + (int)(color.R * light.Ka);
                        step = (int)((255 - (color.G * light.Ka)) / colorSubdivision);
                        G = Interpolation(color.G * light.Ka, 0, 255, colorSubdivision, G) * step + (int)(color.G * light.Ka);
                        step = (int)((255 - (color.B * light.Ka)) / colorSubdivision);
                        B = Interpolation(color.B * light.Ka, 0, 255, colorSubdivision, B) * step + (int)(color.B * light.Ka);

                        Color newColor = Color.FromArgb(R, G, B);

                        g.DrawRectangle(new Pen(newColor), j, i, 1, 1);
                    }
                }
                x1 += _inc13;
                x2 += inc23;
            }

        }

        void Rasterization(List<Point3D> points, List<Color> colors)
        {
            points = points.Select(p => new Point3D((float)Math.Round(p.X), (float)Math.Round(p.Y), p.Z, p.W)).ToList();

            List<(Point3D, Color)> temp = (new List<int> { 0, 1, 2 }).Select(i => (points[i], colors[i])).ToList();

            temp.Sort((a, b) => a.Item1.Y == b.Item1.Y ? 0 : (a.Item1.Y < b.Item1.Y ? -1 : 1));

            points = temp.Select(x => x.Item1).ToList();
            colors = temp.Select(x => x.Item2).ToList();

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

            for (int i = (int)(points[0].Y); i < (int)(points[1].Y); i++)
            {
                int cLeftR = Interpolation(points[0].Y, colors[0].R, points[left].Y, colors[left].R, i);
                int cLeftG = Interpolation(points[0].Y, colors[0].G, points[left].Y, colors[left].G, i);
                int cLeftB = Interpolation(points[0].Y, colors[0].B, points[left].Y, colors[left].B, i);

                int cRightR = Interpolation(points[0].Y, colors[0].R, points[right].Y, colors[right].R, i);
                int cRightG = Interpolation(points[0].Y, colors[0].G, points[right].Y, colors[right].G, i);
                int cRightB = Interpolation(points[0].Y, colors[0].B, points[right].Y, colors[right].B, i);

                int zLeft = Interpolation(points[0].Y, points[0].Z, points[left].Y, points[left].Z, i);
                int zRight = Interpolation(points[0].Y, points[0].Z, points[right].Y, points[right].Z, i);

                for (int j = (int)x1; j < (int)x2; j++)
                {
                    int R = Interpolation((int)x1, cLeftR, (int)x2, cRightR, j);
                    int G = Interpolation((int)x1, cLeftG, (int)x2, cRightG, j);
                    int B = Interpolation((int)x1, cLeftB, (int)x2, cRightB, j);

                    int z = Interpolation((int)x1, zLeft, (int)x2, zRight, j);
                    if (pictureBox.Width / 2 + j > pictureBox.Width - 1 || pictureBox.Width / 2 + j < 0 || pictureBox.Height / 2 + i > pictureBox.Height - 1 || pictureBox.Height / 2 + i < 0) continue;
                    if (ZBuffer[pictureBox.Width / 2 + j, pictureBox.Height / 2 + i] > z)
                    {
                        ZBuffer[pictureBox.Width / 2 + j, pictureBox.Height / 2 + i] = z;
                        g.DrawRectangle(new Pen(Color.FromArgb(R, G, B)), j, i, 1, 1);
                    }
                }
                x1 += inc13;
                x2 += inc12;
            }

            if (points[0].Y == points[1].Y)
            {
                x1 = Math.Min(points[0].X, points[1].X);
                x2 = Math.Max(points[0].X, points[1].X);
            }

            if (_inc13 < inc23)
                (_inc13, inc23) = (inc23, _inc13);

            (left, right) = Interpolation(points[0].Y, points[0].X, points[2].Y, points[2].X, points[1].Y) < points[1].X ? (0, 1) : (1, 0);

            for (int i = (int)(points[1].Y); i < (int)(points[2].Y); i++)
            {
                int cLeftR = Interpolation(points[2].Y, colors[2].R, points[left].Y, colors[left].R, i);
                int cLeftG = Interpolation(points[2].Y, colors[2].G, points[left].Y, colors[left].G, i);
                int cLeftB = Interpolation(points[2].Y, colors[2].B, points[left].Y, colors[left].B, i);

                int cRightR = Interpolation(points[2].Y, colors[2].R, points[right].Y, colors[right].R, i);
                int cRightG = Interpolation(points[2].Y, colors[2].G, points[right].Y, colors[right].G, i);
                int cRightB = Interpolation(points[2].Y, colors[2].B, points[right].Y, colors[right].B, i);

                int zLeft = Interpolation(points[2].Y, points[2].Z, points[left].Y, points[left].Z, i);
                int zRight = Interpolation(points[2].Y, points[2].Z, points[right].Y, points[right].Z, i);

                for (int j = (int)x1; j < (int)x2; j++)
                {
                    int R = Interpolation((int)x1, cLeftR, (int)x2, cRightR, j);
                    int G = Interpolation((int)x1, cLeftG, (int)x2, cRightG, j);
                    int B = Interpolation((int)x1, cLeftB, (int)x2, cRightB, j);

                    int z = Interpolation((int)x1, zLeft, (int)x2, zRight, j);
                    if (pictureBox.Width / 2 + j > pictureBox.Width - 1 || pictureBox.Width / 2 + j < 0 || pictureBox.Height / 2 + i > pictureBox.Height - 1 || pictureBox.Height / 2 + i < 0) continue;
                    if (ZBuffer[pictureBox.Width / 2 + j, pictureBox.Height / 2 + i] > z)
                    {
                        ZBuffer[pictureBox.Width / 2 + j, pictureBox.Height / 2 + i] = z;
                        g.DrawRectangle(new Pen(Color.FromArgb(R, G, B)), j, i, 1, 1);
                    }
                }
                x1 += _inc13;
                x2 += inc23;
            }

        }

        public Point3D View(Point3D p, Camera cam, Point3D center)
        {
            p = XRotatePoint(p, -cam.Rotation.X);
            p = YRotatePoint(p, -cam.Rotation.Y);
            p = ZRotatePoint(p, -cam.Rotation.Z);

            Point3D temp = cam.Location;
            temp = XRotatePoint(temp, -cam.Rotation.X);
            temp = YRotatePoint(temp, -cam.Rotation.Y);
            temp = ZRotatePoint(temp, -cam.Rotation.Z);

            p = TranslatePoint(p, -temp.X, -temp.Y, -temp.Z);
            return p;
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
            return new Point3D(p.X / temp.W, p.Y / temp.W, p.Z, p.W);
        }

        public Point3D Axonometric(Point3D p)
        {
            float phi = (float)((0 / 180D) * Math.PI);
            float ksi = (float)((0 / 180D) * Math.PI);

            float[][] AxonometricMatrix = new float[4][]
            {
                    new float[4] { (float)Math.Cos(ksi),    (float)(Math.Sin(phi) * Math.Sin(ksi)),    0, 0 },
                    new float[4] { 0,                       (float)Math.Cos(phi),                      0, 0 },
                    new float[4] { (float)Math.Sin(ksi),    -(float)(Math.Sin(phi) * Math.Cos(ksi)),   1, 0 },
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
            obj.Normals = obj.Normals.Select(p => MultiplyMatrix(RotateMatrix, p)).ToList();
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
            obj.Normals = obj.Normals.Select(p => XRotatePoint(p, angle)).ToList();
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
            obj.Normals = obj.Normals.Select(p => YRotatePoint(p, angle)).ToList();
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
            obj.Normals = obj.Normals.Select(p => ZRotatePoint(p, angle)).ToList();
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

        public static Point3D XRotatePoint(Point3D p, float angle)
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

        public static Point3D YRotatePoint(Point3D p, float angle)
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

        public static Point3D ZRotatePoint(Point3D p, float angle)
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

        public static Point3D MultiplyMatrix(float[][] matrix, Point3D p)
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
            for (int i = 0; i < splits; i++)
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
            for (int j = 0; j < points.Count; j++)
            {
                obj.Faces.Add(new Face(
                    (splits - 1) * points.Count + j + 1,
                    (splits - 1) * points.Count + (j + 1) % points.Count + 1,
                    j + 2,
                    j + 1));
            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked) camera.Projection = Projection.Perspective;
            else if (radioButton2.Checked) camera.Projection = Projection.Axonometric;

            g.Clear(Color.White);
            DrawObjects();
            pictureBox.Refresh();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            float dx, dy, dz;
            if (float.TryParse(textBox1.Text, out dx) && float.TryParse(textBox2.Text, out dy) && float.TryParse(textBox3.Text, out dz) && objectDropDown.SelectedIndex != 0)
            {
                objects[objectDropDown.SelectedIndex - 1].Vertices = objects[objectDropDown.SelectedIndex - 1].Vertices.Select(p => TranslatePoint(p, dx, dy, dz)).ToList();

                g.Clear(Color.White);
                DrawObjects();
                pictureBox.Refresh();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            float angle;
            if (float.TryParse(textBox4.Text, out angle) && objectDropDown.SelectedIndex != 0)
            {
                if (radioButton3.Checked) objects[objectDropDown.SelectedIndex - 1].Vertices = objects[objectDropDown.SelectedIndex - 1].Vertices.Select(p => XRotatePoint(p, angle)).ToList();
                if (radioButton4.Checked) objects[objectDropDown.SelectedIndex - 1].Vertices = objects[objectDropDown.SelectedIndex - 1].Vertices.Select(p => YRotatePoint(p, angle)).ToList();
                if (radioButton5.Checked) objects[objectDropDown.SelectedIndex - 1].Vertices = objects[objectDropDown.SelectedIndex - 1].Vertices.Select(p => ZRotatePoint(p, angle)).ToList();

                if (radioButton3.Checked) objects[objectDropDown.SelectedIndex - 1].Normals = objects[objectDropDown.SelectedIndex - 1].Normals.Select(p => XRotatePoint(p, angle)).ToList();
                if (radioButton4.Checked) objects[objectDropDown.SelectedIndex - 1].Normals = objects[objectDropDown.SelectedIndex - 1].Normals.Select(p => YRotatePoint(p, angle)).ToList();
                if (radioButton5.Checked) objects[objectDropDown.SelectedIndex - 1].Normals = objects[objectDropDown.SelectedIndex - 1].Normals.Select(p => ZRotatePoint(p, angle)).ToList();

                g.Clear(Color.White);
                DrawObjects();
                pictureBox.Refresh();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            float mx, my, mz;
            if (float.TryParse(textBox5.Text, out mx) && float.TryParse(textBox6.Text, out my) && float.TryParse(textBox7.Text, out mz) && objectDropDown.SelectedIndex != 0)
            {
                objects[objectDropDown.SelectedIndex - 1].Vertices = objects[objectDropDown.SelectedIndex - 1].Vertices.Select(p => ScalePoint(p, mx, my, mz)).ToList();

                g.Clear(Color.White);
                DrawObjects();
                pictureBox.Refresh();
            }
        }

        //private void button4_Click(object sender, EventArgs e)
        //{
        //    if (objectDropDown.SelectedIndex != 0)
        //    {
        //        if (radioButton6.Checked) objects[objectDropDown.SelectedIndex - 1].Vertices = objects[objectDropDown.SelectedIndex - 1].Vertices.Select(p => XYMirrorPoint(p)).ToList();
        //        if (radioButton7.Checked) objects[objectDropDown.SelectedIndex - 1].Vertices = objects[objectDropDown.SelectedIndex - 1].Vertices.Select(p => XZMirrorPoint(p)).ToList();
        //        if (radioButton8.Checked) objects[objectDropDown.SelectedIndex - 1].Vertices = objects[objectDropDown.SelectedIndex - 1].Vertices.Select(p => YZMirrorPoint(p)).ToList();

        //        if (radioButton6.Checked) objects[objectDropDown.SelectedIndex - 1].Normals = objects[objectDropDown.SelectedIndex - 1].Normals.Select(p => XYMirrorPoint(p)).ToList();
        //        if (radioButton7.Checked) objects[objectDropDown.SelectedIndex - 1].Normals = objects[objectDropDown.SelectedIndex - 1].Normals.Select(p => XZMirrorPoint(p)).ToList();
        //        if (radioButton8.Checked) objects[objectDropDown.SelectedIndex - 1].Normals = objects[objectDropDown.SelectedIndex - 1].Normals.Select(p => YZMirrorPoint(p)).ToList();

        //        g.Clear(Color.White);
        //        DrawObjects();
        //        pictureBox.Refresh();
        //    }
            
        //}

        private void button5_Click(object sender, EventArgs e)
        {
            float mx, my, mz;
            if (float.TryParse(textBox8.Text, out mx) && float.TryParse(textBox9.Text, out my) && float.TryParse(textBox10.Text, out mz) && objectDropDown.SelectedIndex != 0)
            {
                Object3D temp = objects[objectDropDown.SelectedIndex - 1];
                Scale(ref temp, mx, my, mz);
                objects[objectDropDown.SelectedIndex - 1] = temp;

                g.Clear(Color.White);
                DrawObjects();
                pictureBox.Refresh();
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            float angle;
            if (float.TryParse(textBox11.Text, out angle) && objectDropDown.SelectedIndex != 0)
            {
                Object3D temp = objects[objectDropDown.SelectedIndex - 1];
                if (radioButton9.Checked) XRotate(ref temp, angle);
                if (radioButton10.Checked) YRotate(ref temp, angle);
                if (radioButton11.Checked) ZRotate(ref temp, angle);
                objects[objectDropDown.SelectedIndex - 1] = temp;

                g.Clear(Color.White);
                DrawObjects();
                pictureBox.Refresh();
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            float x1, y1, z1, x2, y2, z2, angle;
            if (float.TryParse(textBox12.Text, out x1) && float.TryParse(textBox13.Text, out y1) && float.TryParse(textBox14.Text, out z1) &&
                float.TryParse(textBox15.Text, out x2) && float.TryParse(textBox16.Text, out y2) && float.TryParse(textBox17.Text, out z2) &&
                float.TryParse(textBox18.Text, out angle) && objectDropDown.SelectedIndex != 0)
            {
                Object3D temp = objects[objectDropDown.SelectedIndex - 1];
                Rotate(ref temp, new Point3D(x1, y1, z1), new Point3D(x2, y2, z2), angle);
                objects[objectDropDown.SelectedIndex - 1] = temp;

                g.Clear(Color.White);
                DrawObjects();
                pictureBox.Refresh();
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            for (float angle = 0; angle <= 360; angle+=10)
            {
                camera.Rotation.Y = angle;
                //camera.Location.Z = (float)Math.Cos((angle / 180D) * Math.PI) * 100;
                //camera.Location.X = (float)Math.Sin((angle / 180D) * Math.PI) * 100;
                g.Clear(Color.White);
                DrawObjects();
                pictureBox.Refresh();
            }
        }

        private void save_button_Click(object sender, EventArgs e)
        {
            if (textBox19.Text.Length != 0)
            {
                StringBuilder sb = new StringBuilder();
                foreach(Point3D v in objects[objectDropDown.SelectedIndex - 1].Vertices)
                {
                    sb.AppendLine($"v {v.X} {v.Y} {v.Z}");
                }

                foreach (Face f in objects[objectDropDown.SelectedIndex - 1].Faces)
                {
                    sb.Append("f");
                    foreach (FaceIndices fi in f.FaceIndices)
                        sb.Append($" {fi.VertexIndex}/{fi.TextureCoordinateIndex}/{fi.NormalIndex}");
                    sb.AppendLine("");
                }

                File.WriteAllText(textBox19.Text + ".obj", sb.ToString());
            }
        }

        public class Camera
        {
            public Point3D Location { get; set; }
            public Point3D Rotation
            {
                get { return _rotation; }
                set
                {
                    Point3D temp = new Point3D(0, 0, -1);
                    temp = XRotatePoint(temp, value.X);
                    temp = YRotatePoint(temp, value.Y);
                    temp = ZRotatePoint(temp, value.Z);

                    ViewVector = temp;
                    _rotation = value;
                }
            }

            private Point3D _rotation;
            public Point3D ViewVector { get; set; }
            public Projection Projection { get; set; }

            public Camera()
            {
                Location = new Point3D(0, 0, 0);
                Rotation = new Point3D(0, 0, 0);
                ViewVector = new Point3D(0, 0, -1);
                Projection = Projection.Perspective;
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

        public void Normalize()
        {
            Point3D normalized = this / (float)Math.Sqrt(Math.Pow(this.X, 2) + Math.Pow(this.Y, 2) + Math.Pow(this.Z, 2));
            X = normalized.X; Y = normalized.Y; Z = normalized.Z;
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
        public List<Point3D> Vertices { get; set; }
        public List<Face> Faces { get; set; }
        public List<Point3D> Normals { get; set; }
        public List<Coordinates> TextureCoordinates { get; set; }
        public List<Coordinates> ParameterSpaceVertices { get; set; }
        public Color color { get; set; }

        public Object3D()
        {
            Vertices = new List<Point3D>();
            Faces = new List<Face>();
            Normals = new List<Point3D>();
            TextureCoordinates = new List<Coordinates>();
            ParameterSpaceVertices = new List<Coordinates>();
            color = Color.FromArgb(35, 35, 35);
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
                else if (s.Substring(0, 2) == "v ")
                {
                    float[] parsed = s.Substring(2, s.Length - 2).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                    res.Vertices.Add(new Point3D(parsed[0], parsed[1], parsed[2], parsed.Length == 3 ? 1 : parsed[3]));
                }
                else if (s.Substring(0, 2) == "f ")
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

            Point3D center = new Point3D(0, 0, 0);
            foreach (Point3D p in res.Vertices)
                center += p;
            center /= res.Vertices.Count;

            res.Vertices = res.Vertices.Select(p => p - center).ToList();

            return res;
        }
    }

    public enum Projection
    {
        Perspective,
        Axonometric
    }

    public class Light
    {
        public Point3D Location { get; set; }
        public Point3D ViewLocation { get; set; }
        public float Ka { get; set; }
        public float Kd { get; set; }
        public float Ks { get; set; }

        public Light()
        {
            Location = new Point3D(0, 0, 0);
            ViewLocation = new Point3D(0, 0, 0);
            Ka = 0.15f;
            Kd = 0.8f;
            Ks = 0.4f;
        }
    }
}
