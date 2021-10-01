using System;
using System.Collections.Generic;

using System.Linq;

using System.Text;
using System.Threading.Tasks;

using System.Drawing;
using System.Drawing.Imaging;

using System.Windows.Forms;

using System.IO;

using System.Numerics;

namespace ArtCode
{
    class Program
    {
        static bool[,] bitMatrix;
        static float[,] greyScaleMatrix;


        static int rows = 0;
        static int columns = 0;
        static int denoiseIterations = 1;
        static int thresholdBoxSize = 50;

        static List<Vector2> markerPositions = new List<Vector2>();

        [STAThread]
        static void Main(string[] args)
        {
            //Create color matrix from .bmp file
            CreateMatrix();


            //BinarizeColorMatrix(threshold);
            //SaveColorMatrix2Bitmap();

            //Retrieve marker position indices
            //GetMarkerPositions();

            //Fill markers                                                      //Image markedSpots = PrintPositions(newImage, markers);
            //Fill(markerPositions);                             
            SaveMatrix();                                                       //<--
        }

        static void CreateMatrix()
        {
            //Load Image
            OpenFileDialog open = new OpenFileDialog();
            open.Filter = "Image Files(*.jpg; *.jpeg; *.gif; *.bmp)|*.jpg; *.jpeg; *.gif; *.bmp";

            if (open.ShowDialog() != DialogResult.OK)
            {
                Console.WriteLine("Open File Dialog Error");
                Application.Exit();
            }

            Bitmap bitmap = new Bitmap(open.FileName);
            
            rows = bitmap.Height;
            columns = bitmap.Width;

            bitMatrix = new bool[rows, columns];


            /*for (int i = 0; i < bitmap.Height; i++)
            {
                for (int j = 0; j < bitmap.Width; j++)
                {
                    if (bitmap.GetPixel(j, i).GetBrightness() > 0.5f) bitMatrix[i, j] = true;
                    else bitMatrix[i, j] = false;
                }
            }*/




            greyScaleMatrix = new float[rows, columns];
            for (int i = 0; i < bitmap.Height; i++)
            {
                for (int j = 0; j < bitmap.Width; j++)
                {
                    greyScaleMatrix[i, j] = bitmap.GetPixel(j, i).GetBrightness();
                }
            }

            for (int d = 0; d < denoiseIterations; d++)
            {
                for (int i = 0; i < bitmap.Height; i++)
                {
                    for (int j = 0; j < bitmap.Width; j++)
                    {
                        if (i == 0 && j == 0) greyScaleMatrix[i, j] = (greyScaleMatrix[i, j] + greyScaleMatrix[i, j + 1] + greyScaleMatrix[i + 1, j] + greyScaleMatrix[i + 1, j + 1]) / 4; //Ecke links-oben
                        else if (i == 0 && j == columns - 1) greyScaleMatrix[i, j] = (greyScaleMatrix[i, j] + greyScaleMatrix[i, j - 1] + greyScaleMatrix[i + 1, j] + greyScaleMatrix[i + 1, j - 1]) / 4; //Ecke rechts-oben
                        else if (i == rows - 1 && j == 0 ) greyScaleMatrix[i, j] = (greyScaleMatrix[i, j] + greyScaleMatrix[i - 1, j] + greyScaleMatrix[i, j + 1] + greyScaleMatrix[i - 1, j + 1]) / 4; //Ecke links-unten
                        else if (i == rows - 1 && j == columns - 1) greyScaleMatrix[i, j] = (greyScaleMatrix[i, j] + greyScaleMatrix[i, j - 1] + greyScaleMatrix[i - 1, j] + greyScaleMatrix[i - 1, j - 1]) / 4; //Ecke rechts-unten
                        else if (i == 0 && j > 0 && j < columns - 1) greyScaleMatrix[i, j] = (greyScaleMatrix[i, j] + greyScaleMatrix[i, j - 1] + greyScaleMatrix[i, j + 1] + greyScaleMatrix[i + 1, j] + greyScaleMatrix[i + 1, j + 1] + greyScaleMatrix[i + 1, j - 1]) / 6; //Kante oben
                        else if (i > 0 && i < rows - 1 && j == columns - 1) greyScaleMatrix[i, j] = (greyScaleMatrix[i, j] + greyScaleMatrix[i - 1, j] + greyScaleMatrix[i, j - 1] + greyScaleMatrix[i + 1, j] + greyScaleMatrix[i - 1, j - 1] + greyScaleMatrix[i + 1, j - 1]) / 6; //Kante rechts
                        else if (i == rows - 1 && j > 0 && j < columns - 1) greyScaleMatrix[i, j] = (greyScaleMatrix[i, j] + greyScaleMatrix[i - 1, j] + greyScaleMatrix[i, j - 1] + greyScaleMatrix[i, j + 1] + greyScaleMatrix[i - 1, j - 1] + greyScaleMatrix[i - 1, j + 1]) / 6; //Kante unten
                        else if (i > 0 && i < rows - 1 && j == 0) greyScaleMatrix[i, j] = (greyScaleMatrix[i, j] + greyScaleMatrix[i - 1, j] + greyScaleMatrix[i, j + 1] + greyScaleMatrix[i + 1, j] + greyScaleMatrix[i - 1, j + 1] + greyScaleMatrix[i + 1, j + 1]) / 6; //Kante links
                        else greyScaleMatrix[i, j] = (greyScaleMatrix[i, j] + greyScaleMatrix[i, j - 1] + greyScaleMatrix[i - 1, j - 1] + greyScaleMatrix[i - 1, j] + greyScaleMatrix[i - 1, j + 1] + greyScaleMatrix[i, j + 1] + greyScaleMatrix[i + 1, j + 1] + greyScaleMatrix[i + 1, j] + greyScaleMatrix[i + 1, j - 1]) / 9; //Kante links
                    }
                }
            }

            bitMatrix = new bool[rows, columns];

            int tBoxesX = 0;
            int tBoxesY = 0;

            if (rows % thresholdBoxSize == 0) tBoxesY = rows / thresholdBoxSize;
            else tBoxesY = rows / thresholdBoxSize + 1;

            if (columns % thresholdBoxSize == 0) tBoxesX = columns / thresholdBoxSize;
            else tBoxesX = columns / thresholdBoxSize + 1;

            float[] tSums = new float[tBoxesX * tBoxesY];
            int[] tDivisors = new int[tBoxesX * tBoxesY];

            int index = 0;

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    index = i / thresholdBoxSize * tBoxesX + j / thresholdBoxSize;
                    tSums[index] += greyScaleMatrix[i, j];
                    tDivisors[index]++;
                }
            }

            float[] thresholds = new float[tBoxesX * tBoxesY];

            for (int i = 0; i < thresholds.Length; i++)
            {
                thresholds[i] = tSums[i] / tDivisors[i];
            }

            //Create Matrix from image
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    index = i / thresholdBoxSize * tBoxesX + j / thresholdBoxSize;
                    if (greyScaleMatrix[i, j] > thresholds[index]) bitMatrix[i, j] = true;
                    else bitMatrix[i, j] = false;
                }
            }
        }

        static void SaveMatrix()
        {
            Bitmap bitmap = new Bitmap(columns, rows);
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    if (bitMatrix[i, j]) bitmap.SetPixel(j, i, Color.White);
                    else bitmap.SetPixel(j, i, Color.Black);
                }
            }

            Stream stream = new MemoryStream();
            SaveFileDialog save = new SaveFileDialog();

            save.Filter = "BMP Image | *.bmp";
            save.RestoreDirectory = true;

            if (save.ShowDialog() == DialogResult.OK)
            {
                if ((stream = save.OpenFile()) != null)
                {
                    bitmap.Save(stream, ImageFormat.Bmp);
                    stream.Close();
                }
            }
        }

        

        /*static void GetMarkerPositions()                                       //<--
        {
            Color currentColor;
            Color previousColor = new Color();

            int colorCounter = 0;
            int markerSize = 0;

            int[] colorCounters = new int[5];
            int[] ratios = new int[5];

            List<Vector2> positionsHor = new List<Vector2>();
            List<Vector2> positionsVer = new List<Vector2>();
            List<Vector2> matchingPositions = new List<Vector2>();
            List<List<Vector2>> markerPositionGroup = new List<List<Vector2>>();

            for (int t = 0; t < 2; t++)
            {
                int dim1 = 0;
                int dim2 = 0;
                
                for (int i = 0; i < rows * columns; i++)
                {
                    if ((t == 0 && dim1 == rows) || (t == 1 && dim1 == columns))
                    {
                        dim1 = 0;
                        dim2++;

                        Array.Clear(colorCounters, 0, 4);
                    }

                    if (t == 0) currentColor = colorMatrix[dim1, dim2];
                    else currentColor = colorMatrix[dim2, dim1];

                    dim1++;

                    if (currentColor != previousColor)
                    {
                        previousColor = currentColor;

                        for (int c = colorCounters.Length - 1; c > 0; c--)
                        {
                            colorCounters[c] = colorCounters[c - 1];
                        }
                        colorCounters[0] = colorCounter;
                        colorCounter = 0;

                        for (int r = 0; r < ratios.Length; r++)
                        {
                            ratios[r] = (int)Math.Round((double)colorCounters[r] / colorCounters[0]);
                        }

                        if (ratios.SequenceEqual(new int[5] { 1, 1, 3, 1, 1 }) && currentColor == Color.White)
                        {
                            markerSize = colorCounters[0];
                            for (int p = 5 * markerSize - 1; p > 2 * markerSize - 1; p--)
                            {
                                if (t == 0) positionsHor.Add(new Vector2(dim1 + 1, dim2 - p));
                                else positionsVer.Add(new Vector2(dim2 - p, dim1 + 1));
                            }
                        }
                    }
                    colorCounter++;
                }
            }

            for (int i = 0; i < positionsHor.Count(); i++)
            {
                for (int j = 0; j < positionsVer.Count(); j++)
                {
                    if (positionsHor[i] == positionsVer[j])
                    {
                        matchingPositions.Add(positionsHor[i]);
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

            Console.Read();
        }

        public static Image PrintPositions(Bitmap imageToBeMarked, List<Vector2> positions)
        {
            int xPos, yPos;
            Graphics g = Graphics.FromImage(imageToBeMarked);
            g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
            Image square = Bitmap.FromFile("D:\\Projects\\GitHub\\ArtCode\\Resources\\square2.bmp");

            Color dotColor = Color.Turquoise;

            for (int posIndex = 0; posIndex < positions.Count; posIndex++)
            {
                xPos = (int)Math.Round(positions[posIndex].X);
                yPos = (int)Math.Round(positions[posIndex].Y);
                imageToBeMarked.SetPixel(yPos, xPos, dotColor);
                g.DrawImage(square, yPos - 2, xPos - 2);
            }

            return (Image)imageToBeMarked;
        }

        static void Fill(List<Vector2> startingPositions)
        {
            List<Vector2> stack = new List<Vector2>();
            
            for (int i = 0; i < startingPositions.Count; i++)
            {
                stack.Add(startingPositions[i]);
            }

            while (stack.Count > 0)
            {
                for (int i = 0; i < stack.Count; i++)
                {
                    int x = (int)Math.Round(stack[i].X);
                    int y = (int)Math.Round(stack[i].Y);

                    if (colorMatrix[x, y] == Color.Black) //FromArgb(255, 0, 0, 0))
                    {
                        colorMatrix[x, y] = Color.Yellow;
                        
                        stack.Add(new Vector2(x + 1, y));
                        stack.Add(new Vector2(x - 1, y));
                        stack.Add(new Vector2(x, y + 1));
                        stack.Add(new Vector2(x, y - 1));
                    }
                    stack.RemoveAt(i);
                }
            }
        }*/
    }
}
