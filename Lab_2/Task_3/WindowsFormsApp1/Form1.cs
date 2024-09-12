using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Imaging;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        static string filename = "8.png";
        static ImageFormat format = ImageFormat.Png;

        static Bitmap bm = new Bitmap(filename);

        HSV[,] bm_hsv = new HSV[bm.Width, bm.Height];
        Bitmap im = new Bitmap(bm.Width, bm.Height);

        public Form1()
        {
            InitializeComponent();

            for (int i = 0; i < bm.Height; i++)
                for (int j = 0; j < bm.Width; j++)
                    bm_hsv[j, i] = RGB_To_HSV(bm.GetPixel(j, i));

            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox1.Image = bm;
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            for (int i = 0; i < bm.Height; i++)
            {
                for (int j = 0; j < bm.Width; j++)
                {
                    HSV hsv = bm_hsv[j, i];

                    hsv.H += trackBar1.Value;

                    if (trackBar2.Value > 0) hsv.S = map(trackBar2.Value, 0, 100, hsv.S, 1);
                    else hsv.S = map(trackBar2.Value, -100, 0, 0, hsv.S);

                    if (trackBar3.Value > 0) hsv.V = map(trackBar3.Value, 0, 100, hsv.V, 1);
                    else hsv.V = map(trackBar3.Value, -100, 0, 0, hsv.V);

                    im.SetPixel(j, i, HSV_To_RGB(hsv));
                }
            }

            pictureBox1.Image = im;
        }

        private void save_button_Click(object sender, EventArgs e)
        {
            im.Save("result_" + filename, format);
        }

        public struct HSV
        {
            float _h;
            float _s;
            float _v;
            public float H { get { return _h; } set { if (value > 360) value = value % 360; while (value < 0) value += 360; _h = value; } }
            public float S { get { return _s; } set { if (value > 1) value = 1; if (value < 0) value = 0; _s = value; } }
            public float V { get { return _v; } set { if (value > 1) value = 1; if (value < 0) value = 0; _v = value; } }
        }

        public float map(float value, float min1, float max1, float min2, float max2)
        {
            return ((value - min1) / (max1 - min1)) * (max2-min2) + min2;
        }

        public static HSV RGB_To_HSV(Color c)
        {
            HSV res = new HSV();

            List<float> bytes = new List<float>();
            bytes.Add((float)(c.R) / 255);
            bytes.Add((float)(c.G) / 255);
            bytes.Add((float)(c.B) / 255);

            float max = bytes.Max();
            float min = bytes.Min();

            if (max == min) res.H = 0;
            else if (max == bytes[0])
                    if (c.G >= c.B) res.H = 60 * (bytes[1] - bytes[2]) / (max - min);
                    else res.H = 60 * (bytes[1] - bytes[2]) / (max - min) + 360;
                else
                    if (max == c.G) res.H = 60 * (bytes[2] - bytes[0]) / (max - min) + 120;
                    else res.H = 60 * (bytes[0] - bytes[1]) / (max - min) + 240;
        

            if (max == 0) res.S = 0;
            else res.S = 1 - (min / max);

            res.V = max;


            float asd = c.GetHue();
            float asdad = c.GetSaturation();

            return res;
        }
        
        public static Color HSV_To_RGB(HSV hsv)
        {
            int a = (int)(hsv.H / 60) % 6;
            double f = (hsv.H)/60 - Math.Floor(hsv.H / 60);
            byte p = (byte)(255 * hsv.V * (1 - hsv.S));
            byte q = (byte)(255 * hsv.V * (1 - f* hsv.S));
            byte t = (byte)(255 * hsv.V * (1 - (1-f)* hsv.S));

            byte V = (byte)(hsv.V * 255);

            Color res = new Color();
            switch (a) {
                case 0: res = Color.FromArgb(V, t, p); break;
                case 1: res = Color.FromArgb(q, V, p); break;
                case 2: res = Color.FromArgb(p, V, t); break;
                case 3: res = Color.FromArgb(p, q, V); break;
                case 4: res = Color.FromArgb(t, p, V); break;
                case 5: res = Color.FromArgb(V, p, q); break;
                default:break;
            }

            return res;
        }
    }
}
