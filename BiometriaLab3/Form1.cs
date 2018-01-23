using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BiometriaLab3
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            Bitmap bitmap = new Bitmap(pictureBox1.Image);
            binarize(bitmap, 15);
        }

        //Everything which is white or black or skin should be white, iris should be black
        private void binarize(Bitmap _bitmap, int tresholdLevel)
        {
            Bitmap bitmap = _bitmap;
            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    var currRgb = bitmap.GetPixel(x, y);
                    var avgRgb = (currRgb.R + currRgb.G + currRgb.B) / 3;
                    var newRgb = Color.FromArgb(currRgb.A, avgRgb, avgRgb, avgRgb);
                    Color rgbToSet = newRgb.R > tresholdLevel || newRgb.G > tresholdLevel || newRgb.B > tresholdLevel ?
                        rgbToSet = Color.FromArgb(255, 255, 255) : rgbToSet = Color.FromArgb(0, 0, 0);
                    bitmap.SetPixel(x, y, rgbToSet);
                }
            }
            pictureBox1.Image = bitmap;
        }

        #region FindEdgesRegion
        private void button2_Click(object sender, EventArgs e)
        {
            var bitmap = new Bitmap(pictureBox1.Image);
            var black = Color.FromArgb(0, 0, 0);
            for (int y = 0; y < bitmap.Height - 1; y++)
            {
                for (int x = 0; x < bitmap.Width - 1; x++)
                {
                    var currRed = bitmap.GetPixel(x, y).R - bitmap.GetPixel(x + 1, y + 1).R;
                    var currGreen = bitmap.GetPixel(x, y).G - bitmap.GetPixel(x + 1, y + 1).G;
                    var currBlue = bitmap.GetPixel(x, y).B - bitmap.GetPixel(x + 1, y + 1).B;
                    var nextRed = bitmap.GetPixel(x + 1, y).R - bitmap.GetPixel(x, y + 1).R;
                    var nextGreen = bitmap.GetPixel(x + 1, y).R - bitmap.GetPixel(x, y + 1).R;
                    var nextBlue = bitmap.GetPixel(x + 1, y).R - bitmap.GetPixel(x, y + 1).R;
                    Color newColor = Color.FromArgb(
                        Math.Abs(currRed) + Math.Abs(nextRed) >= 255 ? 255 : 0,

                        Math.Abs(currGreen) + Math.Abs(nextGreen) >= 255 ? 255 : 0,

                        Math.Abs(currBlue) + Math.Abs(nextBlue) >= 255 ? 255 : 0);
                    bitmap.SetPixel(x, y, newColor);
                }
            }
            System.Diagnostics.Debug.WriteLine("Done");
            pictureBox1.Image = bitmap;
        }
        #endregion
        #region Find pupil and iris
        int Max = 0;
        int radMax = 0;
        int iMax = 0;
        int jMax = 0;
        int irisRadius = 0;
        private void button3_Click(object sender, EventArgs e)
        {
            var bitmap = new Bitmap(pictureBox1.Image);
            var blackColor = Color.FromArgb(0, 0, 0);

            int[,,] coordinateAndRadius3dArray = new int[pictureBox1.Image.Width + 1, pictureBox1.Image.Height + 1, 90];
            for (int i = 0; i < pictureBox1.Image.Width; i++)
            {
                for (int j = 0; j < pictureBox1.Image.Height; j++)
                {
                    if (bitmap.GetPixel(i, j) != blackColor)
                    {
                        for (int radius = 10; radius <= 60; radius++)
                        {
                            for (int angle = 0; angle <= 360; angle++)
                            {
                                int a = (int)(i - (radius * Math.Cos(angle)));
                                int b = (int)(j - (radius * Math.Sin(angle)));
                                if (a > pictureBox1.Image.Width || a < 0 || b >pictureBox1.Image.Height || b < 0)
                                    break;
                                coordinateAndRadius3dArray[a, b, radius]++;
                                if (coordinateAndRadius3dArray[a, b, radius] >= Max)
                                {
                                    Max = coordinateAndRadius3dArray[a, b, radius];
                                    radMax = radius;
                                    iMax = a;
                                    jMax = b;
                                }
                            }
                        }
                    }
                }
            }
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                using (Pen b = new Pen(Color.Red))
                {
                    g.DrawEllipse(b, iMax - radMax, jMax - radMax, radMax * 2, radMax * 2);

                }
            }
            pictureBox1.Image = bitmap;
            bitmap = new Bitmap(BiometriaLab3.Properties.Resources.eye_iris);
            int midX = iMax + radMax / 2;
            int midY = jMax + radMax / 2;
            irisRadius = radMax;
            int x = midX, y = midY;
            while(bitmap.GetPixel(x,y).R <= 140 && bitmap.GetPixel(x,y).G <= 140 && bitmap.GetPixel(x,y).B <= 140 && y < bitmap.Height -1)
            {
                y++;
                irisRadius++;
            }
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                using (Pen b = new Pen(Color.Red))
                {
                    Pen c = new Pen(Color.Green);
                    g.DrawEllipse(c, iMax - radMax, jMax - radMax, radMax * 2, radMax * 2);
                    g.DrawEllipse(b, iMax - irisRadius, jMax - irisRadius, irisRadius * 2, irisRadius * 2);
                }
            }
            pictureBox1.Image = bitmap;
            #endregion
        }

        private void button4_Click(object sender, EventArgs e)
        {
            pictureBox1.Image = BiometriaLab3.Properties.Resources.eye_iris;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            var bitmap = new Bitmap(pictureBox1.Image);
            for(int i = 0; i < bitmap.Width; i++)
            {
                for(int j = 0; j < bitmap.Height; j++)
                {


                    if (Math.Pow((i - iMax), 2) + Math.Pow((j - jMax), 2) >= Math.Pow(irisRadius, 2) || Math.Pow((i - iMax), 2) + Math.Pow((j - jMax), 2) <= Math.Pow(radMax, 2))
                        bitmap.SetPixel(i, j, Color.White);
                }
            }
            for (int i = 0; i < bitmap.Width; i++)
            {
                for (int j = 0; j < bitmap.Height; j++)
                {

                    var color = bitmap.GetPixel(i, j);
                    if(color.R>150)
                        bitmap.SetPixel(i, j, Color.FromArgb(255, 255, 255));
                    if (color.R > 140 && color.G > 140 && color.B > 140)
                        bitmap.SetPixel(i, j, Color.FromArgb(255, 255, 255));
                    if (color.R > color.B + 50 && color.R > color.G + 50)
                        bitmap.SetPixel(i, j, Color.FromArgb(255, 255, 255));
                }
            }
            pictureBox1.Image = bitmap;
        }
    }
}
