using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;

namespace ArtCode
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            OpenFileDialog open = new OpenFileDialog();
            open.Filter = "Image Files(*.jpg; *.jpeg; *.gif; *.bmp)|*.jpg; *.jpeg; *.gif; *.bmp";

            if (open.ShowDialog() == DialogResult.OK)
            {
                Bitmap image = new Bitmap(open.FileName);

                int[,] imageMatrix = new int[image.Width, image.Height];
                int[] code = new int[image.Width * image.Height];
                int index = 0;

                for (int i = 0; i < image.Height; i++)
                {
                    for (int j = 0; j < image.Width; j++)
                    {
                        imageMatrix[i, j] = Gray2Binary(image.GetPixel(j, i));
                        code[index] = imageMatrix[i, j];
                        index++;
                        Console.Write(imageMatrix[i, j]);
                    }
                    Console.WriteLine();
                }
                Console.WriteLine();

                for (int i = 0; i < code.Length; i++)
                {
                    Console.Write(code[i]);
                }

                Bitmap newImage = new Bitmap(image.Width, image.Height);

                for (int i = 0; i < image.Height; i++)
                {
                    for (int j = 0; j < image.Width; j++)
                    {
                        Color black = Color.Black;
                        Color white = Color.White;

                        if(imageMatrix[i,j] == 0)
                        {
                            newImage.SetPixel(j, i, black);
                        }
                        else
                        {
                            newImage.SetPixel(j, i, white);
                        }
                    }
                }

                Image img = (Image)newImage;
                String newPath = "D:\\Projects\\GitHub\\ArtCode\\Resources\\newImg1.bmp";
                img.Save(newPath);
                System.Diagnostics.Process.Start(newPath);

                Console.Read();
            }
        }

        static int Gray2Binary(Color color)
        {
            if(color.GetBrightness() > 0.5f)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

    }
}
