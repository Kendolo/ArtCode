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

using System.Diagnostics;

namespace ArtCode
{
    class Program
    {
        static bool[,] bitMatrix;
        static float[,] greyScaleMatrix;


        static int rows = 0;
        static int columns = 0;
        static int denoiseIterations = 0;
        static int thresholdBoxSize = 100;

        [STAThread]
        static void Main(string[] args)
        {
            //Create color matrix from .bmp file
            CreateMatrix();

            //Retrieve marker position indices
            FindMarkers();
            
            //Image markedSpots = PrintPositions(newImage, markers);
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

        
        static void FindMarkers()                                       //<--
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            bool currentBit = false;
            bool previousBit = false;

            int bitCounter = 0;
            int markerSize = 0;

            int[] bitCounters = new int[5];
            int[] ratios = new int[5];

            List<Vector2> centroids = new List<Vector2>();
            List<List<Vector2>> centroidGroups = new List<List<Vector2>>();

            for (int i = 0; i < rows; i++)
            {
                Array.Clear(bitCounters, 0, 4);

                for (int j = 0; j < columns; j++)
                {
                    currentBit = bitMatrix[i, j];

                    if (currentBit != previousBit)
                    {
                        previousBit = currentBit;

                        for (int c = bitCounters.Length - 1; c > 0; c--)
                        {
                            bitCounters[c] = bitCounters[c - 1];
                        }
                        bitCounters[0] = bitCounter;
                        bitCounter = 0;

                        for (int r = 0; r < ratios.Length; r++)
                        {
                            ratios[r] = (int)Math.Round((double)bitCounters[r] / bitCounters[0]);
                        }

                        if (ratios.SequenceEqual(new int[5] { 1, 1, 3, 1, 1 }) && currentBit)
                        {
                            markerSize = bitCounters[0];
                            for (int p = 5 * markerSize - 1; p > 2 * markerSize - 1; p--)
                            {
                                centroids.Add(new Vector2(i + 1, j - p));
                            }
                        }
                    }
                    bitCounter++;
                }
            }

            centroidGroups.Add(new List<Vector2>());
            centroidGroups[0].Add(centroids[0]);
            bool passtRein = false;

            for (int i = 1; i < centroids.Count(); i++)
            {
                for (int j = 0; j < centroidGroups.Count(); j++)
                {
                    if (Math.Abs(centroids[i].X - centroidGroups[j][0].X) < 3 * markerSize && Math.Abs(centroids[i].Y - centroidGroups[j][0].Y) < 3 * markerSize)
                    {
                        centroidGroups[j].Add(centroids[i]);
                        passtRein = true;
                    }
                }

                if (!passtRein)
                {
                    centroidGroups.Add(new List<Vector2>());
                    centroidGroups[centroidGroups.Count - 1].Add(centroids[i]);
                }
                passtRein = false;
            }

            Vector2[] newCentroids = new Vector2[centroidGroups.Count()];

            for (int i = 0; i < centroidGroups.Count(); i++)
            {
                float xSum = 0;
                float ySum = 0;
                for (int j = 0; j < centroidGroups[i].Count(); j++)
                {
                    xSum += centroidGroups[i][j].X;
                    ySum += centroidGroups[i][j].Y;
                }
                newCentroids[i] = new Vector2(xSum / centroidGroups[i].Count(), ySum / centroidGroups[i].Count());
            }

            List<int> removeIndices = new List<int>();
            for (int c = 0; c < newCentroids.Length; c++)
            {
                int startX = 0;
                int endX = 0;
                int startY = 0;
                int endY = 0;

                if (newCentroids[c].Y < 6 * markerSize)
                    startY = 0;
                if (newCentroids[c].Y > rows - 6 * markerSize)
                    endY = rows;
                if (newCentroids[c].X < 6 * markerSize)
                    startX = 0;
                if (newCentroids[c].X > columns - 6 * markerSize)
                    endX = columns;
                if (newCentroids[c].Y > 6 * markerSize)
                    startY = (int)(newCentroids[c].Y - 6 * markerSize);
                if (newCentroids[c].Y < rows - 6 * markerSize)
                    endY = (int)(newCentroids[c].Y + 6 * markerSize);
                if (newCentroids[c].X > 6 * markerSize)
                    startX = (int)(newCentroids[c].X - 6 * markerSize);
                if (newCentroids[c].X < columns - 6 * markerSize)
                    endX = (int)(newCentroids[c].X + 6 * markerSize);

                Array.Clear(ratios, 0, 4);
                bool match = false;

                for (int j = startX; j < endX; j++)                   
                {
                    Array.Clear(bitCounters, 0, 4);

                    for (int i = startY; i < endY; i++)
                    {
                        currentBit = bitMatrix[i, j];

                        if (currentBit != previousBit)
                        {
                            previousBit = currentBit;

                            for (int cc = bitCounters.Length - 1; cc > 0; cc--)
                            {
                                bitCounters[cc] = bitCounters[cc - 1];
                            }
                            bitCounters[0] = bitCounter;
                            bitCounter = 0;

                            for (int r = 0; r < ratios.Length; r++)
                            {
                                ratios[r] = (int)Math.Round((double)bitCounters[r] / bitCounters[0]);
                            }

                            if (ratios.SequenceEqual(new int[5] { 1, 1, 3, 1, 1 }) && currentBit)
                            {
                                markerSize = bitCounters[0];
                                for (int p = 5 * markerSize - 1; p > 2 * markerSize - 1; p--)
                                {
                                    centroidGroups[c].Add(new Vector2(j + 1, i - p));
                                }

                                match = true;
                            }
                        }
                        bitCounter++;
                    }
                }
                if (!match)
                    removeIndices.Add(c);
            }

            List<List<Vector2>> newCentroidGroups = new List<List<Vector2>>();

            for (int i = 0; i < centroidGroups.Count(); i++)
            {
                bool remove = false;
                for (int r = 0; r < removeIndices.Count(); r++)
                {
                    if (i == removeIndices[r]) remove = true;
                }

                if (!remove) newCentroidGroups.Add(centroidGroups[i]);
            }

            Vector2[] finalCentroids = new Vector2[newCentroidGroups.Count()];

            for (int i = 0; i < finalCentroids.Length; i++)
            {
                float xSum = 0;
                float ySum = 0;
                for (int j = 0; j < newCentroidGroups[i].Count(); j++)
                {
                    xSum += newCentroidGroups[i][j].X;
                    ySum += newCentroidGroups[i][j].Y;
                }
                finalCentroids[i] = new Vector2((int)Math.Round(xSum / newCentroidGroups[i].Count()), (int)Math.Round(ySum / newCentroidGroups[i].Count()));
            }

            stopwatch.Stop();

            Console.WriteLine("It took " + stopwatch.ElapsedMilliseconds + " milliseconds to find the following " + finalCentroids.Length + " markers in the " + columns + " by " + rows + " image:");
            for (int i = 0; i < finalCentroids.Length; i++)
                Console.WriteLine("final centroids: " + finalCentroids[i]);

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

        static void Fill(List<Vector2> startingPositions)   //funktion muss überarbeitet werden!
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

                    if (bitMatrix[x, y]) //FromArgb(255, 0, 0, 0))
                    {
                        bitMatrix[x, y] = true; 
                        
                        stack.Add(new Vector2(x + 1, y));
                        stack.Add(new Vector2(x - 1, y));
                        stack.Add(new Vector2(x, y + 1));
                        stack.Add(new Vector2(x, y - 1));
                    }
                    stack.RemoveAt(i);
                }
            }
        }
    }
}
