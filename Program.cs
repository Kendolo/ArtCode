using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using System.IO;

namespace ArtCode
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            OpenFileDialog open = new OpenFileDialog();
            LoadImage(open);

            Bitmap oldImage = new Bitmap(open.FileName);
            Bitmap newImage = Convert(oldImage);

            SaveImage((Image)newImage);

            //Find marker using newImage
            Scanner(newImage);

            Console.ReadLine();
        }

        static void LoadImage(OpenFileDialog openInMethod)
        {
            openInMethod.Filter = "Image Files(*.jpg; *.jpeg; *.gif; *.bmp)|*.jpg; *.jpeg; *.gif; *.bmp";

            if (openInMethod.ShowDialog() != DialogResult.OK)
            {
                Console.WriteLine("Error");
                Application.Exit();
            }
        }

        static Bitmap Convert(Bitmap oldImage)
        {
            Bitmap newImage = oldImage;
            Color black = Color.Black;
            Color white = Color.White;
            int[] code = new int[oldImage.Width * oldImage.Height];
            int index = 0;

            for (int i = 0; i < oldImage.Height; i++)
            {
                for (int j = 0; j < oldImage.Width; j++)
                {
                    if (oldImage.GetPixel(j, i).GetBrightness() > 0.5f)
                    {
                        newImage.SetPixel(j, i, white);
                        code[index] = 1;
                    }
                    else
                    {
                        newImage.SetPixel(j, i, black);
                        code[index] = 0;
                    }                  
                    index++;
                }
            }

            for (int i = 0; i < code.Length; i++)
            {
                Console.Write(code[i]);
            }

            return newImage;
        }

        static void SaveImage(Image newImage)
        {
            Stream newImageStream = new MemoryStream();
            SaveFileDialog save = new SaveFileDialog();

            save.Filter = "BMP Image | *.bmp";
            save.RestoreDirectory = true;

            if (save.ShowDialog() == DialogResult.OK)
            {
                if ((newImageStream = save.OpenFile()) != null)
                {
                    // Code to write the stream goes here.
                    newImage.Save(newImageStream, System.Drawing.Imaging.ImageFormat.Bmp);
                    newImageStream.Close();
                }
            }
        }
        public static void Scanner(Bitmap imageToBeChecked)
        {
            Console.WriteLine();
            Color currentColor;
            Color previousColor = new Color();
            int colorCounter = 0;

            int[] ratios = new int[5];

            for(int i = 0; i < imageToBeChecked.Height; i++)
            {
                for (int j = 0; j < imageToBeChecked.Width; j++)
                {
                    currentColor = imageToBeChecked.GetPixel(j, i);

                    if(currentColor != previousColor)
                    {
                        previousColor = currentColor;
                        ratios[4] = ratios[3];
                        ratios[3] = ratios[2];
                        ratios[2] = ratios[1];
                        ratios[1] = ratios[0];
                        ratios[0] = colorCounter;
                        colorCounter = 0;
                        //Console.WriteLine();
                        //Console.Write(ratios[6] + " : " + ratios[5] + " : " + ratios[4] + " : " + ratios[3] + " : " + ratios[2] + " : " + ratios[1] + " : " + ratios[0]);

                        if(ratios[4] == 1 && ratios[3] == 1 && ratios[2] == 3 && ratios[1] == 1 && ratios[0] == 1)
                        {
                            Console.WriteLine("Possible marker in line " + (i+1) + " and column " + (j+1) + "!");
                            //Console.Read();
                        }
                    }

                    if(j == 0) //imageToBeChecked.Width - 1)
                    {
                        ratios[0] = 0;
                        ratios[1] = 0;
                        ratios[2] = 0;
                        ratios[3] = 0;
                        ratios[4] = 0;
                    }
                    colorCounter++;
                }
            }
        }
    }
}
