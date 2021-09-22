using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace ArtCode
{
    class Program
    {
        static void Main(string[] args)
        {
            Bitmap image = new Bitmap("D:\\Projects\\GitHub\\ArtCode\\img1.bmp");
            int[,] imageMatrix = new int[image.Width, image.Height];
            int[] code = new int[image.Width * image.Height];
            int index = 0;

            for(int i = 0; i < image.Height; i++)
            {
                for(int j = 0; j < image.Width; j++)
                {
                    imageMatrix[i, j] = Gray2Binary(image.GetPixel(j,i));
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
            Console.Read();
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
