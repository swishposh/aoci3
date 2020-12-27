using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;

namespace aoci03
{
    public partial class Form1 : Form
    {
        Image<Bgr, byte> srcImg;
        PointF[] pts = new PointF[4];
        int c = 0;
        public Image<Bgr, byte> sourceImage;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();

            var result = dlg.ShowDialog();
            if (result == DialogResult.OK)
            {
                string fileName = dlg.FileName;
                srcImg = new Image<Bgr, byte>(fileName).Resize(640, 480, Inter.Linear);

                imageBox1.Image = srcImg;
            }
        }

        private void imageBox1_Click(object sender, EventArgs e)
        {

        }

        private void imageBox1_MouseClick(object sender, MouseEventArgs e)
        {
            var imgCopy = srcImg.Copy();

            int x = (int)(e.Location.X / imageBox1.ZoomScale);
            int y = (int)(e.Location.Y / imageBox1.ZoomScale);

            pts[c] = new Point(x, y);
            c++;
            if (c >= 4)
                c = 0;

            int radius = 2;
            int thickness = 2;
            var color = new Bgr(Color.Blue).MCvScalar;

            for (int i = 0; i < 4; i++)
                CvInvoke.Circle(imgCopy, new Point((int)pts[i].X, (int)pts[i].Y), radius, color, thickness);

            imageBox1.Image = imgCopy;
        }

        public Image<Bgr, byte> filtr(Image<Bgr, byte> sourceImage, double k)
        {
            Image<Bgr, byte> scaledImg = new Image<Bgr, byte>((int)(sourceImage.Width * k), (int)(sourceImage.Height * k));
            for (int i = 0; i < scaledImg.Width - 1; i++)

                for (int j = 0; j < scaledImg.Height - 1; j++)
                {
                    double I = (i / k);
                    double J = (j / k);

                    double baseI = Math.Floor(I);
                    double baseJ = Math.Floor(J);

                    if (baseI >= sourceImage.Width - 1) continue;
                    if (baseJ >= sourceImage.Height - 1) continue;

                    double rI = I - baseI;
                    double rJ = J - baseJ;

                    double irI = 1 - rI;
                    double irJ = 1 - rJ;

                    Bgr c1 = new Bgr();
                    c1.Blue = sourceImage.Data[(int)baseJ, (int)baseI, 0] * irI + sourceImage.Data[(int)baseJ, (int)(baseI + 1), 0] * rI;
                    c1.Green = sourceImage.Data[(int)baseJ, (int)baseI, 1] * irI + sourceImage.Data[(int)baseJ, (int)(baseI + 1), 1] * rI;
                    c1.Red = sourceImage.Data[(int)baseJ, (int)baseI, 2] * irI + sourceImage.Data[(int)baseJ, (int)(baseI + 1), 2] * rI;

                    Bgr c2 = new Bgr();
                    c2.Blue = sourceImage.Data[(int)(baseJ + 1), (int)baseI, 0] * irI + sourceImage.Data[(int)(baseJ + 1), (int)(baseI + 1), 0] * rI;
                    c2.Green = sourceImage.Data[(int)(baseJ + 1), (int)baseI, 1] * irI + sourceImage.Data[(int)(baseJ + 1), (int)(baseI + 1), 1] * rI;
                    c2.Red = sourceImage.Data[(int)(baseJ + 1), (int)baseI, 2] * irI + sourceImage.Data[(int)(baseJ + 1), (int)(baseI + 1), 2] * rI;

                    Bgr c = new Bgr();
                    c.Blue = c1.Blue * irJ + c2.Blue * rJ;
                    c.Green = c1.Green * irJ + c2.Green * rJ;
                    c.Red = c1.Red * irJ + c2.Red * rJ;

                    scaledImg[j, i] = c;

                }
            return scaledImg;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            double k = double.Parse(textBox1.Text);
            imageBox2.Image = filtr(srcImg, k);
        }

        public Image<Bgr, byte> sdvig(Image<Bgr, byte> sourceImage, double shift)
        {
            Image<Bgr, byte> shearingImg = new Image<Bgr, byte>((int)(sourceImage.Width + sourceImage.Width * shift) + 1, (int)(sourceImage.Height));
            for (int i = 0; i < sourceImage.Width - 1; i++)
                for (int j = 0; j < sourceImage.Height - 1; j++)
                {
                    int newX = (int)(i + shift * (sourceImage.Height - j));
                    int newY = (int)(j);
                    shearingImg[newY, newX] = sourceImage[j, i];
                }

            return shearingImg;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            imageBox2.Image = sdvig(srcImg, Convert.ToDouble(textBox2.Text));
        }

        public Image<Bgr, byte> povorot(Image<Bgr, byte> sourceImage, double angle)
        {
            Image<Bgr, byte> scaledImage = new Image<Bgr, byte>((int)sourceImage.Width, (int)sourceImage.Height);
            double newX = 0;
            double newY = 0;

            double angleRadians = angle * Math.PI / 180d;

            for (int x = 0; x < scaledImage.Width; x++)
                for (int y = 0; y < scaledImage.Height; y++)
                {
                    newX = (Math.Cos(angleRadians) * (x - scaledImage.Width / 2) - Math.Sin(angleRadians) * (y - scaledImage.Height / 2) + scaledImage.Width / 2);
                    newY = (Math.Sin(angleRadians) * (x - scaledImage.Width / 2) + Math.Cos(angleRadians) * (y - scaledImage.Height / 2) + scaledImage.Height / 2);

                    if (newX >= sourceImage.Width - 1 || newY >= sourceImage.Height - 1) continue;
                    if (newX < 0 || newY < 0) continue;

                    scaledImage[y, x] = sourceImage[(int)newY, (int)newX];
                }
            

            return scaledImage;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            imageBox2.Image = filtr(povorot(srcImg, Convert.ToDouble(textBox3.Text)), 2);
        }


        public Image<Bgr, byte> otrazhenie(Image<Bgr, byte> sourceImage, double qX, double qY)
        {
            Image<Bgr, byte> scaledImage = new Image<Bgr, byte>((int)sourceImage.Width, (int)sourceImage.Height);

            double newX = 0;
            double newY = 0;

            for (int x = 0; x < scaledImage.Width; x++)
                for (int y = 0; y < scaledImage.Height; y++)
                {
                    if (qX == 1 && qY == 1)
                    {
                        newX = x;
                        newY = y;
                    }
                    else if (qX == -1 && qY == -1)
                    {
                        newX = (x * (qX) + sourceImage.Width);
                        newY = (y * (qY) + sourceImage.Height);
                    }
                    else if (qX == 1 && qY == -1)
                    {
                        newX = (x * (qX));
                        newY = (y * (qY) + sourceImage.Height);
                    }
                    else if (qX == -1 && qY == 1)
                    {
                        newX = (x * (qX) + sourceImage.Width);
                        newY = (y * (qY));
                    }

                    if (newX >= sourceImage.Width || newY >= sourceImage.Height) continue;

                    scaledImage[y, x] = sourceImage[(int)newY, (int)newX];
                }

            return scaledImage;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            imageBox2.Image = otrazhenie(srcImg, Convert.ToDouble(textBox4.Text), Convert.ToDouble(textBox5.Text));
        }



        public Image<Bgr, byte> proektsiya(Image<Bgr, byte> copyImage)
        {
            var destPoints = new PointF[]
            {
                new PointF(0, 0),
                new PointF(0, copyImage.Height - 1),
                new PointF(copyImage.Width - 1, copyImage.Height - 1),
                new PointF(copyImage.Width - 1, 0)
            };

            var homographyMatrix = CvInvoke.GetPerspectiveTransform(pts, destPoints);
            var destImage = new Image<Bgr, byte>(copyImage.Size);
            CvInvoke.WarpPerspective(copyImage, destImage, homographyMatrix, destImage.Size);

            return destImage;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            imageBox2.Image = proektsiya(srcImg);
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
