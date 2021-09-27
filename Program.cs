using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Numerics;

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

            //SaveImage((Image)newImage);

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

            for (int i = 0; i < oldImage.Height; i++)
            {
                for (int j = 0; j < oldImage.Width; j++)
                {
                    if (oldImage.GetPixel(j, i).GetBrightness() > 0.5f)
                    {
                        newImage.SetPixel(j, i, white);
                    }
                    else
                    {
                        newImage.SetPixel(j, i, black);
                    }                  
                }
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

            int[] counters = new int[5];
            int[] ratios = new int[5];
            int markerSize = 0;
            List<Vector2> horizontalPositions = new List<Vector2>();
            List<Vector2> verticalPositions = new List<Vector2>();
            List<Vector2> matchingPositions = new List<Vector2>();
            List<List<Vector2>> markerPositionGroup = new List<List<Vector2>>();
            List<Vector2> markerPositions = new List<Vector2>();

            for (int i = 0; i < imageToBeChecked.Height; i++)
            {
                Array.Clear(counters, 0, 4);

                for (int j = 0; j < imageToBeChecked.Width; j++)
                {
                    currentColor = imageToBeChecked.GetPixel(j, i);

                    if(currentColor != previousColor)
                    {
                        previousColor = currentColor;

                        for (int k = counters.Length - 1 ; k > 0; k--)
                        {
                            counters[k] = counters[k - 1];
                        }
                        counters[0] = colorCounter;
                        colorCounter = 0;

                        for (int r = 0; r < ratios.Length; r++)
                        {
                            ratios[r] = (int)Math.Round((double)counters[r] / counters[0]);
                        }

                        if(ratios.SequenceEqual(new int[5] { 1, 1, 3, 1, 1 }) && currentColor == Color.FromArgb(255, 255, 255, 255))
                        {
                            markerSize = counters[0];
                            for (int p = 5 * markerSize - 1; p > 2 * markerSize - 1; p--)
                            {
                                horizontalPositions.Add(new Vector2(i + 1, j - p));
                            }
                        }
                    }
                    colorCounter++;
                }
            }

            for (int i = 0; i < imageToBeChecked.Width; i++)
            {
                Array.Clear(counters, 0, 4);

                for (int j = 0; j < imageToBeChecked.Height; j++)
                {
                    currentColor = imageToBeChecked.GetPixel(i, j);

                    if (currentColor != previousColor)
                    {
                        previousColor = currentColor;
                        for (int k = counters.Length - 1; k > 0; k--)
                        {
                            counters[k] = counters[k - 1];
                        }
                        counters[0] = colorCounter;
                        colorCounter = 0;

                        for (int r = 0; r < ratios.Length; r++)
                        {
                            ratios[r] = (int)Math.Round((double)counters[r] / counters[0]);
                        }

                        if (ratios.SequenceEqual(new int[5]{1, 1, 3, 1, 1}) && currentColor == Color.FromArgb(255, 255, 255, 255))
                        {
                            markerSize = counters[0];
                            for (int p = 5 * markerSize - 1; p > 2 * markerSize - 1; p--)
                            {
                                verticalPositions.Add(new Vector2(j - p, i + 1));
                            }
                        }
                    }
                    colorCounter++;
                }
            }

            for (int i = 0; i < horizontalPositions.Count(); i++)
            {
                for (int j = 0; j < verticalPositions.Count(); j++)
                {
                    if (horizontalPositions[i] == verticalPositions[j])
                    {
                        matchingPositions.Add(horizontalPositions[i]);
                    }
                }
            }

            markerPositionGroup.Add(new List<Vector2>());
            markerPositionGroup[0].Add(matchingPositions[0]);
            bool passtRein = false;

            for (int i = 1; i < matchingPositions.Count(); i++)
            {
                for (int j = 0; j < markerPositionGroup.Count(); j++)
                {
                    if (Math.Abs(matchingPositions[i].X - markerPositionGroup[j][0].X) < 3 * markerSize && Math.Abs(matchingPositions[i].Y - markerPositionGroup[j][0].Y) < 3 * markerSize)
                    {
                        markerPositionGroup[j].Add(matchingPositions[i]);
                        passtRein = true;
                    }
                }

                if (!passtRein)
                {
                    markerPositionGroup.Add(new List<Vector2>());
                    markerPositionGroup[markerPositionGroup.Count - 1].Add(matchingPositions[i]);
                }
                passtRein = false;
            }

            for (int i = 0; i < markerPositionGroup.Count(); i++)
            {
                float xSum = 0;
                float ySum = 0;
                for (int j = 0; j < markerPositionGroup[i].Count(); j++)
                {
                    xSum += markerPositionGroup[i][j].X;
                    ySum += markerPositionGroup[i][j].Y;
                }
                markerPositions.Add(new Vector2(xSum / markerPositionGroup[i].Count(), ySum / markerPositionGroup[i].Count()));
            }

            for (int i = 0; i < markerPositions.Count(); i++)
            {
                Console.WriteLine("Marker at: " + markerPositions[i]);
            }
        }
    }
}
