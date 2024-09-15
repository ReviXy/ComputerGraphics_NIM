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


namespace Testcolors
{
    public partial class Form1 : Form
    {
        private Bitmap bitmap;
        public Form1()
        {
            InitializeComponent();
            // Загрузка и отображение изображения
            Image image = Image.FromFile(""); 
            bitmap = new Bitmap(image);

            // Отображение изображения в каналах
            pictureBoxTrue.Image = bitmap; 
            pictureBoxRed.Image = ExtractChannel(bitmap, 0); // Красный канал
            pictureBoxGreen.Image = ExtractChannel(bitmap, 1); // Зеленый канал
            pictureBoxBlue.Image = ExtractChannel(bitmap, 2); // Синий канал

            // Построение гистограмм
            DisplayHistogram(chartRed, GetHistogram(bitmap, 'R'));
            DisplayHistogram(chartGreen, GetHistogram(bitmap, 'G'));
            DisplayHistogram(chartBlue, GetHistogram(bitmap, 'B'));
        }

        private Bitmap ExtractChannel(Bitmap source, int channel)
        {
            Bitmap channelImage = new Bitmap(source.Width, source.Height);

            for (int y = 0; y < source.Height; y++)
            {
                for (int x = 0; x < source.Width; x++)
                {
                    Color pixelColor = source.GetPixel(x, y);
                    Color newColor;

                    switch (channel)
                    {
                        case 0: // Красный канал
                            newColor = Color.FromArgb(pixelColor.R, 0, 0);
                            break;
                        case 1: // Зеленый канал
                            newColor = Color.FromArgb(0, pixelColor.G, 0);
                            break;
                        case 2: // Синий канал
                            newColor = Color.FromArgb(0, 0, pixelColor.B);
                            break;
                        default:
                            newColor = Color.Black;
                            break;
                    }

                    channelImage.SetPixel(x, y, newColor);
                }
            }

            return channelImage;
        }

        private int[] GetHistogram(Bitmap bitmap, char channel)
        {
            int[] histogram = new int[256];

            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    Color pixelColor = bitmap.GetPixel(x, y);
                    int value = 0;

                    switch (channel)
                    {
                        case 'R':
                            value = pixelColor.R;
                            break;
                        case 'G':
                            value = pixelColor.G;
                            break;
                        case 'B':
                            value = pixelColor.B;
                            break;
                    }

                    histogram[value]++;
                }
            }

            return histogram;
        }

        private void DisplayHistogram(Chart chart, int[] histogram)
        {
            chart.Series[0].Points.Clear();
            for (int i = 0; i < histogram.Length; i++)
            {
                chart.Series[0].Points.AddXY(i, histogram[i]);
            }
        }
    }
}
