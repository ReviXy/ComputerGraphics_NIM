using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace Task1
{
    public partial class Form1 : Form
    {
        private Bitmap bitmap;

        public Form1()
        {
            InitializeComponent();
            Image image = Image.FromFile("8.png");
            bitmap = new Bitmap(image);

            pictureBox1.Image = bitmap;

            int[] hist1 = new int[256];
            int[] hist2 = new int[256];

            Bitmap bitmap1 = new Bitmap(bitmap.Width, bitmap.Height);
            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    Color c = bitmap.GetPixel(x, y);
                    byte y1 = (byte)(0.299f * c.R + 0.587f * c.G + 0.114f * c.B);
                    hist1[y1]++;
                    bitmap1.SetPixel(x, y, Color.FromArgb(y1, y1, y1));
                }
            }

            pictureBox2.Image = bitmap1;

            chart1.Series[0].Points.Clear();
            for (int i = 0; i < hist1.Length; i++)
            {
                chart1.Series[0].Points.AddXY(i, hist1[i]);
            }

            Bitmap bitmap2 = new Bitmap(bitmap.Width, bitmap.Height);
            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    Color c = bitmap.GetPixel(x, y);
                    byte y1 = (byte)(0.2126f * c.R + 0.7152f * c.G + 0.0722f * c.B);
                    hist2[y1]++;
                    bitmap2.SetPixel(x, y, Color.FromArgb(y1, y1, y1));
                }
            }

            pictureBox3.Image = bitmap2;

            chart2.Series[0].Points.Clear();
            for (int i = 0; i < hist2.Length; i++)
            {
                chart2.Series[0].Points.AddXY(i, hist2[i]);
            }

            Bitmap bitmap3 = new Bitmap(bitmap.Width, bitmap.Height);
            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {

                    byte y1 = (byte)Math.Abs(bitmap1.GetPixel(x, y).R - bitmap2.GetPixel(x, y).R);
                    bitmap3.SetPixel(x, y, Color.FromArgb(y1, y1, y1));
                }
            }

            pictureBox4.Image = bitmap3;

            //DisplayHistogram(chartRed, GetHistogram(bitmap, 'R'));
        }

    }
}
