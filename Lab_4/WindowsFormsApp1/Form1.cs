using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
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

        bool pointFlag = true;
        Point tempPoint;

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

                    // To do Хачу матрицы
                    for (int i = 0; i < polygons[polygonSelectDropDown.SelectedIndex - 1].Count; i++)
                        polygons[polygonSelectDropDown.SelectedIndex - 1][i] = new Point(polygons[polygonSelectDropDown.SelectedIndex - 1][i].X + dx, polygons[polygonSelectDropDown.SelectedIndex - 1][i].Y + dy);

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
                    float turn = 0;
                    if (polygonSelectDropDown.SelectedIndex == 0 || !float.TryParse(inputTextBox.Text, out turn))
                    {
                        outputTextBox.Text = "Ошибка входных данных";
                        return;
                    }

                    // To do Хачу преобразование
                    // Хачу использование tempPoint

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
            mode = Mode.Move_Polygon;
            clearButton.Enabled = false;
            SetModeButtons(false);

            polygonSelectDropDown.Enabled = true;
            inputTextBox.Enabled = true;
            applyButton.Enabled = true;
            outputTextBox.Text = "Выберите полигон и введите целые dx и dy через пробел. После чего нажмите Apply.";
        }

        private void turnAroundPointButton_Click(object sender, EventArgs e)
        {
            mode = Mode.Turn_Around_Point;
            clearButton.Enabled = false;
            SetModeButtons(false);

            outputTextBox.Text = "Выберите точку, относительно которой будет выполняться поворот.";
        }

        private void turnAroundCenterButton_Click(object sender, EventArgs e)
        {

        }

        private void scaleRelativeToPointButton_Click(object sender, EventArgs e)
        {

        }

        private void scaleRelativeToCenterButton_Click(object sender, EventArgs e)
        {

        }

        private void findIntersectionButton_Click(object sender, EventArgs e)
        {

        }

        private void convexityCheckButton_Click(object sender, EventArgs e)
        {

        }

        private void positionRelativeToEdgeButton_Click(object sender, EventArgs e)
        {

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

        
    }
}
