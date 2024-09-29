using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        enum Mode
        {
            Idle,
            Create_Polygon,
            Move_Polygon,
            Turn_Around_Point,
            Turn_Around_Center,
            Scaling_Relative_To_Point,
            Scaling_Relative_To_Center,
            Find_Intersection,
            Convexity_Check,
            Position_Relative_To_Edge
        }

        Mode mode = Mode.Idle;
        List<List<Point>> polygons;
        List<Point> points;
        List<Button> modeButtons;

        Color polygonColor = Color.Black;
        Color selectedPolygonColor = Color.Red;

        Bitmap bm;
        Graphics g;

        public Form1()
        {
            InitializeComponent();
            polygons = new List<List<Point>>();
            points = new List<Point>();

            bm = new Bitmap(850, 600);
            pictureBox.Image = bm;
            g = Graphics.FromImage(pictureBox.Image);
            tempPoint = new Point(); // Потому что больше негде
            tempEdge = new List<Point>(); // Потому что больше негде

            polygonSelectDropDown.Items.Clear();
            polygonSelectDropDown.Items.Add("");
            polygonSelectDropDown.SelectedIndex = 0;

            modeButtons = new List<Button>();
            modeButtons.Add(drawPolygonButton);
            modeButtons.Add(movePolygonButton);
            modeButtons.Add(turnAroundPointButton);
            modeButtons.Add(turnAroundCenterButton);
            modeButtons.Add(scaleRelativeToPointButton);
            modeButtons.Add(scaleRelativeToCenterButton);
            modeButtons.Add(findIntersectionButton);
            modeButtons.Add(convexityCheckButton);
            modeButtons.Add(positionRelativeToEdgeButton);

            polygonSelectDropDown.Enabled = false;
            inputTextBox.Enabled = false;
            applyButton.Enabled = false;

            pictureBox.MouseDown += OnMouseDown;
            polygonSelectDropDown.SelectedIndexChanged += RedrawSelectedPolygons;
        }

        void RedrawSelectedPolygons(object sender, EventArgs e)
        {
            DrawPolygons();
        }

        void DrawPolygons()
        {
            bm = new Bitmap(850, 600);
            pictureBox.Image = bm;
            g = Graphics.FromImage(pictureBox.Image);

            for (int j = 0; j < polygons.Count; j++)
            {
                Color color = polygonSelectDropDown.SelectedIndex == j + 1 ? selectedPolygonColor : polygonColor;
                Pen pen = new Pen(color);
                
                List<Point> polygon = polygons[j]; 
                if (polygon.Count == 1) g.DrawRectangle(pen, new Rectangle(polygon[0].X, polygon[0].Y, 1, 1));
                else if (polygon.Count == 2) g.DrawLine(pen, polygon[0].X, polygon[0].Y, polygon[1].X, polygon[1].Y);
                else
                {
                    for (int i = 1; i < polygon.Count; i++)
                        g.DrawLine(pen, polygon[i - 1].X, polygon[i - 1].Y, polygon[i].X, polygon[i].Y);
                    g.DrawLine(pen, polygon[0].X, polygon[0].Y, polygon[polygon.Count - 1].X, polygon[polygon.Count - 1].Y);
                }
            }

            pictureBox.Image = bm;
        }

        int intFlag = 0;
        bool pointFlag = true;
        Point tempPoint;
        List<Point> tempEdge;

        void OnMouseDown(object sender, MouseEventArgs e)
        {
            
            switch (mode)
            {
                case Mode.Create_Polygon:
                    if (e.Button == MouseButtons.Left && !points.Contains(e.Location))
                    {
                        points.Add(e.Location);
                        bm.SetPixel(e.Location.X, e.Location.Y, polygonColor);
                        pictureBox.Image = bm;
                    }
                    else if (e.Button == MouseButtons.Right && points.Count != 0)
                    {
                        polygons.Add(new List<Point>(points));
                        polygonSelectDropDown.Items.Add($"polygon{polygons.Count}");
                        points.Clear();
                        mode = Mode.Idle;
                        clearButton.Enabled = true;
                        SetModeButtons(true);
                        DrawPolygons();
                    }
                    break;

                case Mode.Turn_Around_Point:
                    if (pointFlag)
                    {
                        tempPoint = e.Location;
                        pointFlag = false;

                        polygonSelectDropDown.Enabled = true;
                        inputTextBox.Enabled = true;
                        applyButton.Enabled = true;
                        outputTextBox.Text = "Выберите полигон и угол поворота. После чего нажмите Apply.";
                    }
                    break;

                case Mode.Position_Relative_To_Edge:
                    switch (intFlag)
                    {
                        case 0: 
                            if (!tempEdge.Contains(e.Location))
                            {
                                tempEdge.Add(e.Location);
                                bm.SetPixel(e.Location.X, e.Location.Y, polygonColor);

                                if(tempEdge.Count == 2)
                                {
                                    intFlag = 1;
                                    g.DrawLine(new Pen(selectedPolygonColor), tempEdge[0], tempEdge[1]);
                                    outputTextBox.Text = "Выберите точку.";
                                }
                                pictureBox.Image = bm;
                            }
                            break;
                        case 1:
                            tempPoint = e.Location;
                            bm.SetPixel(e.Location.X, e.Location.Y, selectedPolygonColor);
                            pictureBox.Image = bm;
                            intFlag = 2;

                            Point b = new Point (tempPoint.X - tempEdge[0].X, tempPoint.Y - tempEdge[0].Y);
                            Point a = new Point(tempEdge[1].X - tempEdge[0].X, tempEdge[1].Y - tempEdge[0].Y);
                            float res = b.X * a.Y - b.Y * a.X;
                            string str;
                            if (res > 0) str = "Точка слева от ребра.";
                            else if (res < 0) str = "Точка справа от ребра.";
                            else str = "Точка на ребре.";
                            str += "Нажмите на экран, чтобы продолжить.";
                            outputTextBox.Text = str;

                            break;
                        case 2:
                            mode = Mode.Idle;
                            intFlag = 0;
                            clearButton.Enabled = true;
                            SetModeButtons(true);

                            polygonSelectDropDown.SelectedIndex = 0;
                            inputTextBox.Text = "";
                            outputTextBox.Text = "";

                            DrawPolygons();
                            break;
                        default:break;
                    }
                    break;

                case Mode.Convexity_Check:
                    if (intFlag == 0)
                    {
                        tempPoint = e.Location;
                        bm.SetPixel(e.Location.X, e.Location.Y, selectedPolygonColor);
                        pictureBox.Image = bm;
                        intFlag = 1;

                        polygonSelectDropDown.Enabled = true;
                        applyButton.Enabled = true;
                        outputTextBox.Text = "Выберите полигон. После чего нажмите Apply.";
                    }
                    else if (intFlag == 2)
                    {
                        mode = Mode.Idle;
                        intFlag = 0;
                        clearButton.Enabled = true;
                        SetModeButtons(true);

                        polygonSelectDropDown.SelectedIndex = 0;
                        inputTextBox.Text = "";
                        outputTextBox.Text = "";

                        DrawPolygons();
                    }
                    break;

                case Mode.Find_Intersection:
                    if (intFlag < 4)
                    {
                        tempEdge.Add(e.Location);
                        bm.SetPixel(e.Location.X, e.Location.Y, polygonColor);
                        intFlag++;
                        if (tempEdge.Count % 2 == 0)
                        {
                            
                            g.DrawLine(new Pen(polygonColor), tempEdge[tempEdge.Count - 2], tempEdge[tempEdge.Count - 1]);
                        }
                        pictureBox.Image = bm;
                    }
                    if (intFlag == 4)
                    {
                        Point a = tempEdge[0];
                        Point b = tempEdge[1];
                        Point c = tempEdge[2];
                        Point d = tempEdge[3];
                        Point n = new Point(-(d.Y - c.Y), d.X - c.X);

                        float t = -(float)(n.X * (a.X - c.X) + n.Y * (a.Y - c.Y)) / (n.X * (b.X - a.X) + n.Y * (b.Y - a.Y));

                        if (n.X * (b.X - a.X) + n.Y * (b.Y - a.Y) == 0) { outputTextBox.Text = "Прямые параллельны."; goto qwe; }

                        Point res = new Point(a.X + (int)(t * (b.X - a.X)), a.Y + (int)(t * (b.Y - a.Y)));
                        g.DrawRectangle(new Pen(selectedPolygonColor), res.X, res.Y, 1, 1);
                        pictureBox.Image = bm;

                        if (res.X >= Math.Min(a.X, b.X) && res.X <= Math.Max(a.X, b.X) && res.Y >= Math.Min(a.Y, b.Y) && res.Y <= Math.Max(a.Y, b.Y)
                            && res.X >= Math.Min(c.X, d.X) && res.X <= Math.Max(c.X, d.X) && res.Y >= Math.Min(c.Y, d.Y) && res.Y <= Math.Max(c.Y, d.Y)) 
                            outputTextBox.Text = $"Точка пересечения - ({res.X}, {res.Y}). Нажмите на экран, чтобы продолжить.";
                        else outputTextBox.Text = $"Рёбра не пересекаются.";
                        qwe:
                        intFlag++;
                    }
                    else if (intFlag == 5)
                    {
                        mode = Mode.Idle;
                        intFlag = 0;
                        clearButton.Enabled = true;
                        SetModeButtons(true);

                        polygonSelectDropDown.SelectedIndex = 0;
                        inputTextBox.Text = "";
                        outputTextBox.Text = "";

                        DrawPolygons();
                    }
                    break;
                default: break;
            }
        }

        private void applyButton_Click(object sender, EventArgs e)
        {
            switch (mode)
            {
                case Mode.Move_Polygon:
                    string[] s = inputTextBox.Text.Split(' ');
                    int dx = 0, dy = 0;
                    if (polygonSelectDropDown.SelectedIndex == 0 || s.Length > 2 || !int.TryParse(s[0], out dx) || !int.TryParse(s[1], out dy))
                    {
                        outputTextBox.Text = "Ошибка входных данных";
                        return;
                    }

                    for (int i = 0; i < polygons[polygonSelectDropDown.SelectedIndex - 1].Count; i++)
                        polygons[polygonSelectDropDown.SelectedIndex - 1][i] = Movepoint(polygons[polygonSelectDropDown.SelectedIndex - 1][i], dx, dy);

                    mode = Mode.Idle;
                    clearButton.Enabled = true;
                    SetModeButtons(true);

                    polygonSelectDropDown.SelectedIndex = 0;
                    polygonSelectDropDown.Enabled = false;
                    inputTextBox.Text = "";
                    inputTextBox.Enabled = false;
                    applyButton.Enabled = false;
                    outputTextBox.Text = "";

                    DrawPolygons();
                    break;

                case Mode.Turn_Around_Point:
                    int turn = 0;
                    if (polygonSelectDropDown.SelectedIndex == 0 || !int.TryParse(inputTextBox.Text, out turn))
                    {
                        outputTextBox.Text = "Ошибка входных данных";
                        return;
                    }
                    for (int i = 0; i < polygons[polygonSelectDropDown.SelectedIndex - 1].Count; i++)
                        polygons[polygonSelectDropDown.SelectedIndex - 1][i]=RotatePoint(polygons[polygonSelectDropDown.SelectedIndex - 1][i],tempPoint,turn);
                    mode = Mode.Idle;
                    clearButton.Enabled = true;
                    SetModeButtons(true);

                    polygonSelectDropDown.SelectedIndex = 0;
                    polygonSelectDropDown.Enabled = false;
                    inputTextBox.Text = "";
                    inputTextBox.Enabled = false;
                    applyButton.Enabled = false;
                    outputTextBox.Text = "";

                    DrawPolygons();
                    pointFlag = true;
                break;

                case Mode.Turn_Around_Center:
                    if (polygonSelectDropDown.SelectedIndex == 0 || !int.TryParse(inputTextBox.Text, out turn))
                    {
                        outputTextBox.Text = "Ошибка входных данных";
                        return;
                    }
                    Point tempP = new Point();
                    tempP.X = PolygonCentre(polygons[polygonSelectDropDown.SelectedIndex - 1]).X;
                    tempP.Y = PolygonCentre(polygons[polygonSelectDropDown.SelectedIndex - 1]).Y;
                    for (int i = 0; i < polygons[polygonSelectDropDown.SelectedIndex - 1].Count; i++)
                        polygons[polygonSelectDropDown.SelectedIndex - 1][i] = RotatePoint(polygons[polygonSelectDropDown.SelectedIndex - 1][i], tempP, turn);
                    clearButton.Enabled = true;
                    SetModeButtons(true);

                    polygonSelectDropDown.SelectedIndex = 0;
                    polygonSelectDropDown.Enabled = false;
                    inputTextBox.Text = "";
                    inputTextBox.Enabled = false;
                    applyButton.Enabled = false;
                    outputTextBox.Text = "";

                    outputTextBox.Text = "";

                    DrawPolygons();
                    pointFlag = true;
                    break;

                case Mode.Convexity_Check:
                    if (polygonSelectDropDown.SelectedIndex == 0)
                    {
                        outputTextBox.Text = "Ошибка входных данных";
                        return;
                    }

                    int cnt = 0;
                    List<Point> l = new List<Point>(polygons[polygonSelectDropDown.SelectedIndex - 1]);
                    if (l.Count > 2)
                    {
                        for (int i = 1; i <= l.Count; i++)
                        {
                            Point a = tempPoint;
                            Point b = new Point(tempPoint.X + 1, tempPoint.Y);
                            Point c = l[i - 1];
                            Point d = l[i % l.Count];
                            Point n = new Point(-(d.Y - c.Y), d.X - c.X);
                            float t = -(float)(n.X * (a.X - c.X) + n.Y * (a.Y - c.Y)) / (n.X * (b.X - a.X) + n.Y * (b.Y - a.Y));
                            Point res = new Point(a.X + (int)(t * (b.X - a.X)), a.Y + (int)(t * (b.Y - a.Y)));
                            if (res.X >= Math.Min(c.X, d.X) && res.X <= Math.Max(c.X, d.X) && res.X > a.X) cnt++;

                        }
                    }

                    if (cnt % 2 == 0)
                        outputTextBox.Text = "Точка не лежит внутри полигона. Нажмите на экран, чтобы продолжить.";
                    else
                    {
                        Point b = new Point(tempPoint.X - l[0].X, tempPoint.Y - l[0].Y);
                        Point a = new Point(l[1].X - l[0].X, l[1].Y - l[0].Y);
                        bool sign = b.X * a.Y - b.Y * a.X > 0;

                        bool res = true;

                        for (int i = 1; i <= l.Count; i++)
                        {
                            b = new Point(tempPoint.X - l[i - 1].X, tempPoint.Y - l[i - 1].Y);
                            a = new Point(l[i % l.Count].X - l[i - 1].X, l[i % l.Count].Y - l[i - 1].Y);
                            float t1 = -(b.Y * a.X - b.X * a.Y);
                            if (!(sign && t1 > 0 || !sign && t1 < 0)) { res = false; break; }
                        }

                        if (res) outputTextBox.Text = "Точка лежит внутри выпуклого полигона.";
                        else outputTextBox.Text = "Точка лежит внутри вогнутого полигона.";
                    }

                    polygonSelectDropDown.Enabled = false;
                    applyButton.Enabled = false;

                    intFlag = 2;
                    break;

                default: break;
            }
        }

        private void drawPolygonButton_Click(object sender, EventArgs e)
        {
            mode = Mode.Create_Polygon;
            clearButton.Enabled = false;
            SetModeButtons(false);
        }

        private void movePolygonButton_Click(object sender, EventArgs e)
        {
            if (polygons.Count > 0)
            {
                mode = Mode.Move_Polygon;
                clearButton.Enabled = false;
                SetModeButtons(false);

                polygonSelectDropDown.Enabled = true;
                inputTextBox.Enabled = true;
                applyButton.Enabled = true;
                outputTextBox.Text = "Выберите полигон и введите целые dx и dy через пробел. После чего нажмите Apply.";
            }
            else
            {
                outputTextBox.Text = "Нет полигонов";
            }
        }

        private void turnAroundPointButton_Click(object sender, EventArgs e)
        {
            if (polygons.Count > 0)
            {
                mode = Mode.Turn_Around_Point;
                clearButton.Enabled = false;
                SetModeButtons(false);

                outputTextBox.Text = "Выберите точку, относительно которой будет выполняться поворот.";
            }
            else
            {
                outputTextBox.Text = "Нет полигонов";
            }
        }

        private void turnAroundCenterButton_Click(object sender, EventArgs e)
        {
            if (polygons.Count > 0)
            {
                mode = Mode.Turn_Around_Center;
                clearButton.Enabled = false;
                SetModeButtons(false);
                polygonSelectDropDown.Enabled = true;
                inputTextBox.Enabled = true;
                applyButton.Enabled = true;
                outputTextBox.Text = "Выберите полигон и угол поворота. После чего нажмите Apply";
            }
            else
            {
                outputTextBox.Text = "Нет полигонов";
            }

        }

        private void scaleRelativeToPointButton_Click(object sender, EventArgs e)
        {

        }

        private void scaleRelativeToCenterButton_Click(object sender, EventArgs e)
        {

        }

        private void findIntersectionButton_Click(object sender, EventArgs e)
        {
            tempEdge.Clear();
            mode = Mode.Find_Intersection;
            clearButton.Enabled = false;
            SetModeButtons(false);
            outputTextBox.Text = "Постройте 2 отрезка.";
        }

        int Interpolation(int x0, int y0, int x1, int y1, int x)
        {
            return (int)(y0 + (float)(y1 - y0) * (x - x0) / (x1 - x0));
        }

        private void convexityCheckButton_Click(object sender, EventArgs e)
        {
            if (polygons.Count > 0)
            { 
                mode = Mode.Convexity_Check;
                clearButton.Enabled = false;
                SetModeButtons(false);
                outputTextBox.Text = "Выберите точку.";
            }
            else
            {
                outputTextBox.Text = "Нет полигонов";
            }
        }

        private void positionRelativeToEdgeButton_Click(object sender, EventArgs e)
        {
            mode = Mode.Position_Relative_To_Edge;
            tempEdge.Clear();
            clearButton.Enabled = false;
            SetModeButtons(false);

            outputTextBox.Text = "Постройте ребро 2-мя кликами мыши.";
        }

        void SetModeButtons(bool enabled)
        {
            foreach (Button b in modeButtons)
                b.Enabled = enabled;
        }

        private void Clear()
        {
            polygons.Clear();
            polygonSelectDropDown.Items.Clear();
            polygonSelectDropDown.Items.Add("");
            polygonSelectDropDown.SelectedIndex = 0;
            g.Clear(pictureBox.BackColor);

            bm = new Bitmap(850, 600);
            pictureBox.Image = bm;
            g = Graphics.FromImage(pictureBox.Image);
        }

        private void clearButton_Click(object sender, EventArgs e)
        {
            Clear();
        }
        private int[] Multiplyint(int[][] Matrix, int[] array)
        {
            int[] resultVector = new int[3];
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                    resultVector[i] += Matrix[j][i] * array[j];
            }
            return resultVector;
        }
        private double[] Multiplydouble(double[][] Matrix, int[] array)
        {
            double[] resultVector = new double[3];
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                    resultVector[i] += Matrix[j][i] * array[j];
            }
            return resultVector;
        }
        private Point Movepoint(Point polygonpoint, int dx, int dy)
        {
            int[][] Matrix = new int[3][]
            {
                    new int[3] { 1,   0, 0 },
                    new int[3] { 0,   1, 0 },
                    new int[3] { dx, dy, 1 }
            };
            int[] offsetVector = new int[3] { polygonpoint.X, polygonpoint.Y, 1 };
            int[] resultVector = Multiplyint(Matrix, offsetVector);
            return new Point((int)resultVector[0], (int)resultVector[1]);
        }
        private Point RotatePoint(Point polygonpoint, Point PointofRotate, int rotateAngle)
        {
            double pointA, pointB;
            double angle = (rotateAngle / 180D) * Math.PI;

            pointA = -PointofRotate.X * Math.Cos(angle) + PointofRotate.Y * Math.Sin(angle) + PointofRotate.X;
            pointB = -PointofRotate.X * Math.Sin(angle) - PointofRotate.Y * Math.Cos(angle) + PointofRotate.Y;

            int[] offsetVector = new int[3] { polygonpoint.X, polygonpoint.Y, 1 };
            double[][] Matrix = new double[3][]
            {
                new double[3] {  Math.Cos(angle),   Math.Sin(angle), 0 },
                new double[3] { -Math.Sin(angle),   Math.Cos(angle), 0 },
                new double[3] { pointA, pointB, 1 } 
            };
            double[] resultVector = Multiplydouble(Matrix, offsetVector);
            return new Point((int)resultVector[0], (int)resultVector[1]);
        }
        private Point PolygonCentre(List<Point> polygon)
        {
            int sumX = 0, sumY = 0;
            foreach (Point p in polygon) { sumX += p.X; sumY += p.Y; }
            return new Point(sumX / polygon.Count, sumY / polygon.Count);
        }
    }
}
