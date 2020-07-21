/// <summary>
/// Image filtering by intensity and spatial filter algorithms
/// by Henrique Andrade (hjaa@ecomp.poli.br).
/// 
/// The following code is based on "Digital Image Processing, 4th Edition"
/// by Rafael C. Gonzalez and Richard E. Woods, 2018, Pearson.
/// 
/// </summary>

using System;
using System.Drawing;

namespace ImageEditor.Controllers
{
    class Transformations
    {
        public static Bitmap LogCorrection(Bitmap source)
        {
            Bitmap result = new Bitmap(source);

            float c = 255f / MathF.Log(1 + 255);

            for (int i = 0; i < source.Width; i++)
            {
                for (int j = 0; j < source.Height; j++)
                {
                    Color pixel = source.GetPixel(i, j);
                    float logA = MathF.Log(pixel.A + 1);
                    float logR = MathF.Log(pixel.R + 1);
                    float logG = MathF.Log(pixel.G + 1);
                    float logB = MathF.Log(pixel.B + 1);

                    result.SetPixel(i, j, Color.FromArgb(
                        (int)(c * logA),
                        (int)(c * logR),
                        (int)(c * logG),
                        (int)(c * logB)));
                }
            }

            return result;
        }

        public static Bitmap GammaCorrection(Bitmap source, float gamma)
        {
            Bitmap result = new Bitmap(source);

            for (int i = 0; i < source.Width; i++)
            {
                for (int j = 0; j < source.Height; j++)
                {
                    Color pixel = source.GetPixel(i, j);

                    result.SetPixel(i, j, Color.FromArgb(
                        (int)(255f * MathF.Pow(pixel.A / 255f, gamma)),
                        (int)(255f * MathF.Pow(pixel.R / 255f, gamma)),
                        (int)(255f * MathF.Pow(pixel.G / 255f, gamma)),
                        (int)(255f * MathF.Pow(pixel.B / 255f, gamma))
                        ));
                }
            }

            return result;
        }

        public static Bitmap Equalize(Bitmap source)
        {
            Bitmap result = new Bitmap(source);

            long[] distributionA = new long[256];
            long[] distributionR = new long[256];
            long[] distributionG = new long[256];
            long[] distributionB = new long[256];

            for (int i = 0; i < source.Width; i++)
            {
                for (int j = 0; j < source.Height; j++)
                {
                    Color color = source.GetPixel(i, j);
                    distributionA[color.A]++;
                    distributionR[color.R]++;
                    distributionG[color.G]++;
                    distributionB[color.B]++;
                }
            }

            long pixelAmount = source.Width * source.Height;
            double histConst = 255.0 / pixelAmount;
            double[] histSum = new double[4];
            for (int i = 0; i < 256; i++)
            {
                histSum[0] += distributionA[i];
                histSum[1] += distributionR[i];
                histSum[2] += distributionG[i];
                histSum[3] += distributionB[i];

                distributionA[i] = (long)(histSum[0] * histConst);
                distributionR[i] = (long)(histSum[1] * histConst);
                distributionG[i] = (long)(histSum[2] * histConst);
                distributionB[i] = (long)(histSum[3] * histConst);
            }

            for (int i = 0; i < source.Width; i++)
            {
                for (int j = 0; j < source.Height; j++)
                {
                    Color pixel = source.GetPixel(i, j);

                    result.SetPixel(i, j,
                        Color.FromArgb(
                            (int)distributionA[pixel.A],
                            (int)distributionR[pixel.R],
                            (int)distributionG[pixel.G],
                            (int)distributionB[pixel.B]));
                }
            }

            return result;
        }
    }
}
