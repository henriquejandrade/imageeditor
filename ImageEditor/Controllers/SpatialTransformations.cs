/// <summary>
/// Image filtering by intensity and spatial filter algorithms
/// by Henrique Andrade (hjaa@ecomp.poli.br).
/// 
/// The following code is based on "Digital Image Processing, 4th Edition"
/// by Rafael C. Gonzalez and Richard E. Woods, 2018, Pearson.
/// 
/// </summary>
/// 
using System;
using System.Drawing;

namespace ImageEditor.Controllers
{
    class SpatialTransformations
    {

        public static Bitmap MeanFilter(Bitmap source, int filterSize)
        {
            float[,] filter = new float[filterSize, filterSize];
            int filterRef = ((int)(filterSize - 1) / 2);

            for (int i = 0; i < filterSize; i++)
            {
                for (int j = 0; j < filterSize; j++)
                {
                    filter[i, j] = 1;
                }
            }

            Bitmap filteredImage = new Bitmap(source);

            /*
             * Iterates image
             */
            for (int i = filterRef; i < source.Width - filterRef; i++)
            {
                for (int j = filterRef; j < source.Height - filterRef; j++)
                {
                    float[] newValues = new float[4];
                    float filterSum = 0;

                    /*
                     * Iterates filter
                     */
                    for (int x = -filterRef; x < filterSize - filterRef; x++)
                    {
                        for (int y = -filterRef; y < filterSize - filterRef; y++)
                        {
                            Color pixel = source.GetPixel(i + x, j + y);
                            newValues[0] += pixel.A * filter[x + filterRef, y + filterRef];
                            newValues[1] += pixel.R * filter[x + filterRef, y + filterRef];
                            newValues[2] += pixel.G * filter[x + filterRef, y + filterRef];
                            newValues[3] += pixel.B * filter[x + filterRef, y + filterRef];

                            filterSum += filter[x + filterRef, y + filterRef];
                        }
                    }

                    newValues[0] = MathF.Abs(newValues[0] / filterSum);
                    newValues[1] = MathF.Abs(newValues[1] / filterSum);
                    newValues[2] = MathF.Abs(newValues[2] / filterSum);
                    newValues[3] = MathF.Abs(newValues[3] / filterSum);

                    newValues[0] = newValues[0] > 255 ? 255 : newValues[0];
                    newValues[1] = newValues[1] > 255 ? 255 : newValues[1];
                    newValues[2] = newValues[2] > 255 ? 255 : newValues[2];
                    newValues[3] = newValues[3] > 255 ? 255 : newValues[3];

                    filteredImage.SetPixel(i, j, Color.FromArgb((int)newValues[0], (int)newValues[1], (int)newValues[2], (int)newValues[3]));
                }
            }

            return filteredImage;
        }
    }
}
