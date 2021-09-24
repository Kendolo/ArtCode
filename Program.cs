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
    }
}
