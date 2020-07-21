/// <summary>
/// Image resizing algorithms
/// (and a Gaussian blur snnipet class [1])
/// by Henrique Andrade (hjaa@ecomp.poli.br).
/// 
/// The following code was based on a discussion over which interpolation method
/// works best for image resizing. As used by the broadly known Adobe's software
/// Photoshop, this resizing methods are based on their implementation as seen here [2].
/// 
/// The box filtering algorithm mentioned in the Photoshop's parameters was 
/// implemented by me based on this article [3].
/// 
/// The interpolations methods were based over the explanation found in
/// Computerphile's YouTube channel [4]. 
/// 
/// This C# class was implemented for a Image Processing academic project at
/// Escola Politécnica de Pernambuco [5] but it will serve as part of a larger image 
/// editing software personal project. Details will become available on Github [6] 
/// after the evaluation of this project by the class' staff.
/// 
/// [1] The Gaussian blur algorithm was adapted from: 
///     https://medium.com/@RoardiLeone/fast-image-blurring-algorithm-photoshop-level-w-c-code-87516d5cee87
/// [2] Photoshop's parameters:
///     http://entropymine.com/resamplescope/notes/photoshop/
/// [3] Image Down-Scaler Using the Box Filter Algorithm:
///     https://scholarworks.rit.edu/cgi/viewcontent.cgi?article=10864&context=theses
/// [4] Bicubic Interpolation - Computerphile
///     https://www.youtube.com/watch?v=poY_nGzEEWM
/// [5] Escola Politécnica de Pernambuco
///     http://poli.upe.br/
/// [6] Henrique Andrade's Github Page
///     https://github.com/henriquejandrade
/// </summary>

using System;
using System.Drawing;

namespace ImageEditor.Controllers
{
    class Transform
    {
        public enum Interpolations
        {
            NearestNeighbor,
            Bilinear,
            Bicubic,
            BicubicSmoother,
            BicubicSharper
        }

        public static Bitmap Resize(string path, Interpolations interpolation, int width, int height)
        {
            Bitmap original = new Bitmap(path);
            Bitmap resized = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            double wScale = (double)resized.Width / original.Width;
            double hScale = (double)resized.Height / original.Height;

            bool needsScaling = false;

            switch (interpolation)
            {
                case Interpolations.NearestNeighbor:

                    // Applies nearest neighbor sampling straight
                    for (int i = 0; i < resized.Width; i++)
                    {
                        for (int j = 0; j < resized.Height; j++)
                        {
                            resized.SetPixel(i, j, original.GetPixel((int)(i / wScale), (int)(j / hScale)));
                        }
                    }
                    break;

                case Interpolations.Bilinear:
                    if (wScale >= 0.5)
                    {
                        // Bilinear Interpolation
                        resized = BilinearResize(original, wScale, hScale);

                        // If scaling smaller than 1, applies blurring when bilinear is done
                        if (wScale < 1)
                        {
                            resized = Effects.Blur(resized, wScale);
                        }
                    }
                    else
                    {
                        // Box Filtering
                        resized = BoxFilterResize(original, wScale, hScale);
                    }
                    break;

                case Interpolations.Bicubic:
                    // If scaling is smaller than 0.25,
                    // enlarges image using box filtering first
                    needsScaling = wScale < 0.25;

                    // First, applies box filter scaling to incrase it by 4 of its size
                    // Finally, applies bicubic resizing
                    resized = BicubicResize(
                        needsScaling ? BoxFilterResize(original, 4, 4) : original,
                        needsScaling ? 4 * wScale : wScale,
                        needsScaling ? 4 * hScale : hScale);
                    break;

                case Interpolations.BicubicSmoother:
                    // If the scaling is smaller than 0.25, 
                    // enlarges image using box filtering first
                    needsScaling = wScale < 0.25;

                    // First, applies box filter scaling to incrase it by 4 of its size
                    // Then, applies bicubic resizing
                    resized = BicubicResize(
                        needsScaling ? BoxFilterResize(original, 4, 4) : original,
                        needsScaling ? 4 * wScale : wScale,
                        needsScaling ? 4 * hScale : hScale);

                    // Finally, applies blurring
                    resized = Effects.Blur(resized, 1.15);
                    break;

                case Interpolations.BicubicSharper:
                    // If the scaling is smaller than 0.25, 
                    // enlarges image using box filtering first
                    needsScaling = wScale < 0.25;

                    // First, applies box filter scaling to incrase it by 4 of its size
                    // Then, applies bicubic resizing
                    resized = BicubicResize(
                        needsScaling ? BoxFilterResize(original, 4, 4) : original,
                        needsScaling ? 4 * wScale : wScale,
                        needsScaling ? 4 * hScale : hScale);

                    // Finally, applies blurring
                    resized = Effects.Blur(resized, 1.05);
                    break;
            }

            return resized;
        }

        public static Bitmap BilinearResize(Bitmap original, double wScale, double hScale)
        {
            Bitmap resized = new Bitmap(
                (int)(original.Width * wScale),
                (int)(original.Height * hScale),
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            for (int i = 0; i < resized.Width; i++)
            {
                for (int j = 0; j < resized.Height; j++)
                {
                    float virtualX = ((float)i) / resized.Width * (original.Width - 1);
                    float virtualY = ((float)j) / resized.Height * (original.Height - 1);
                    int realX = (int)virtualX;
                    int realY = (int)virtualY;

                    resized.SetPixel(i, j,
                        BiLerp(
                            original.GetPixel(realX, realY),
                            original.GetPixel(realX + 1, realY),
                            original.GetPixel(realX, realY + 1),
                            original.GetPixel(realX + 1, realY + 1),
                            virtualX - realX,
                            virtualY - realY));
                }
            }

            return resized;
        }

        public static Bitmap BoxFilterResize(Bitmap original, double wScale, double hScale)
        {
            Bitmap resized = new Bitmap(
                (int)(original.Width * wScale),
                (int)(original.Height * hScale),
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            int hWindow = (int)(1 / wScale);
            int vWindow = (int)(1 / hScale);
            int windowSize = hWindow * vWindow;
            for (int i = 0; i < resized.Width; i++)
            {
                for (int j = 0; j < resized.Height; j++)
                {
                    int sumA = 0;
                    int sumR = 0;
                    int sumG = 0;
                    int sumB = 0;

                    int iOriginal = (int)(i * hWindow);
                    int jOriginal = (int)(j * vWindow);
                    for (int h = iOriginal; h < iOriginal + hWindow; h++)
                    {
                        for (int v = jOriginal; v < jOriginal + vWindow; v++)
                        {
                            Color p = original.GetPixel(h, v);
                            sumA += p.A;
                            sumR += p.R;
                            sumG += p.G;
                            sumB += p.B;
                        }
                    }

                    resized.SetPixel(i, j,
                        Color.FromArgb(
                            Math.Clamp((int)(sumA / windowSize), 0, 255),
                            Math.Clamp((int)(sumR / windowSize), 0, 255),
                            Math.Clamp((int)(sumG / windowSize), 0, 255),
                            Math.Clamp((int)(sumB / windowSize), 0, 255)
                        ));
                }
            }

            return resized;
        }

        public static Bitmap BicubicResize(Bitmap original, double wScale, double hScale)
        {
            Bitmap resized = new Bitmap(
                (int)(original.Width * wScale),
                (int)(original.Height * hScale),
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            for (int i = 0; i < resized.Width; i++)
            {
                for (int j = 0; j < resized.Height; j++)
                {
                    float virtualX = ((float)i) / resized.Width * (original.Width - 1);
                    float virtualY = ((float)j) / resized.Height * (original.Height - 1);
                    int realX = (int)virtualX;
                    int realY = (int)virtualY;

                    Color[] points = new Color[]
                    {
                                original.GetPixel(Math.Clamp(realX-1,0,original.Width-1),Math.Clamp( realY-1,0,original.Height-1)),
                                original.GetPixel(realX,Math.Clamp( realY-1,0,original.Height-1)),
                                original.GetPixel(realX+1,Math.Clamp( realY-1,0,original.Height-1)),
                                original.GetPixel(Math.Clamp(realX+2,0,original.Width-1),Math.Clamp( realY-1,0,original.Height-1)),
                                original.GetPixel(Math.Clamp(realX-1,0,original.Width-1),realY),
                                original.GetPixel(realX, realY),
                                original.GetPixel(realX + 1, realY),
                                original.GetPixel(Math.Clamp(realX+2,0,original.Width-1), realY),
                                original.GetPixel(Math.Clamp(realX-1,0,original.Width-1), realY + 1),
                                original.GetPixel(realX, realY + 1),
                                original.GetPixel(realX + 1, realY + 1),
                                original.GetPixel(Math.Clamp(realX+2,0,original.Width-1), realY + 1),
                                original.GetPixel(Math.Clamp(realX-1,0,original.Width-1), Math.Clamp(realY+2,0,original.Height-1)),
                                original.GetPixel(realX, Math.Clamp(realY+2,0,original.Height-1)),
                                original.GetPixel(realX+1, Math.Clamp(realY+2,0,original.Height-1)),
                                original.GetPixel(Math.Clamp(realX+2,0,original.Width-1), Math.Clamp(realY+2,0,original.Height-1))
                    };

                    resized.SetPixel(i, j,
                        BicubicHorizontalInterpolation(points,
                            virtualX - realX,
                            virtualY - realY));
                }
            }

            return resized;
        }

        public static Color Lerp(Color A, Color B, double t)
        {
            return Color.FromArgb(
                (int)(A.A + t * (B.A - A.A)),
                (int)(A.R + t * (B.R - A.R)),
                (int)(A.G + t * (B.G - A.G)),
                (int)(A.B + t * (B.B - A.B))
                );
        }

        public static Color BiLerp(Color A, Color B, Color C, Color D, double tx, double ty)
        {
            return Lerp(Lerp(A, B, tx), Lerp(C, D, tx), ty);
        }

        public static Color Cerp(Color A, Color B, Color C, Color D, double t)
        {
            return Color.FromArgb(
                Math.Clamp((int)(B.A + 0.5 * t * (C.A - A.A + t * (2.0 * A.A - 5.0 * B.A + 4.0 * C.A - D.A + t * (3.0 * (B.A - C.A) + D.A - A.A)))), 0, 255),
                Math.Clamp((int)(B.R + 0.5 * t * (C.R - A.R + t * (2.0 * A.R - 5.0 * B.R + 4.0 * C.R - D.R + t * (3.0 * (B.R - C.R) + D.R - A.R)))), 0, 255),
                Math.Clamp((int)(B.G + 0.5 * t * (C.G - A.G + t * (2.0 * A.G - 5.0 * B.G + 4.0 * C.G - D.G + t * (3.0 * (B.G - C.G) + D.G - A.G)))), 0, 255),
                Math.Clamp((int)(B.B + 0.5 * t * (C.B - A.B + t * (2.0 * A.B - 5.0 * B.B + 4.0 * C.B - D.B + t * (3.0 * (B.B - C.B) + D.B - A.B)))), 0, 255)
                );
        }

        public static Color BicubicHorizontalInterpolation(Color[] p, double tx, double ty)
        {
            return Cerp(
                Cerp(p[0], p[1], p[2], p[3], tx),
                Cerp(p[4], p[5], p[6], p[7], tx),
                Cerp(p[8], p[9], p[10], p[11], tx),
                Cerp(p[12], p[13], p[14], p[15], tx),
                ty);
        }
    }
}

public class Effects
{
    public static Bitmap Blur(Bitmap image, double box)
    {
        int[] boxes = boxesForGaussian(box, 3);
        return FastBoxBlur(FastBoxBlur(FastBoxBlur(image, boxes[0]), boxes[1]), boxes[2]);
    }

    private static int[] boxesForGaussian(double sigma, int n)
    {

        double wIdeal = Math.Sqrt((12 * sigma * sigma / n) + 1);
        double wl = Math.Floor(wIdeal);

        if (wl % 2 == 0) wl--;
        double wu = wl + 2;

        double mIdeal = (12 * sigma * sigma - n * wl * wl - 4 * n * wl - 3 * n) / (-4 * wl - 4);
        double m = Math.Round(mIdeal);

        int[] sizes = new int[n];
        for (int i = 0; i < n; i++)
        {
            if (i < m)
            {
                sizes[i] = (int)wl;
            }
            else
            {
                sizes[i] = (int)wu;
            }
        }
        return sizes;
    }

    private static Bitmap FastBoxBlur(Bitmap img, int radius)
    {

        int kSize = radius;

        if (kSize % 2 == 0) kSize++;
        Bitmap Hblur = (Bitmap)img.Clone();

        float Avg = (float)1 / kSize;

        for (int j = 0; j < img.Height; j++)
        {

            float[] hSum = new float[] { 0f, 0f, 0f, 0f };
            float[] iAvg = new float[] { 0f, 0f, 0f, 0f };

            for (int x = 0; x < kSize; x++)
            {
                Color tmpColor = img.GetPixel(x, j);
                hSum[0] += tmpColor.A;
                hSum[1] += tmpColor.R;
                hSum[2] += tmpColor.G;
                hSum[3] += tmpColor.B;
            }
            iAvg[0] = hSum[0] * Avg;
            iAvg[1] = hSum[1] * Avg;
            iAvg[2] = hSum[2] * Avg;
            iAvg[3] = hSum[3] * Avg;
            for (int i = 0; i < img.Width; i++)
            {
                if ((i - kSize / 2 >= 0) && (i + 1 + kSize / 2 < img.Width))
                {
                    Color tmp_pColor = img.GetPixel(i - kSize / 2, j);
                    hSum[0] -= tmp_pColor.A;
                    hSum[1] -= tmp_pColor.R;
                    hSum[2] -= tmp_pColor.G;
                    hSum[3] -= tmp_pColor.B;
                    Color tmp_nColor = img.GetPixel(i + 1 + kSize / 2, j);
                    hSum[0] += tmp_nColor.A;
                    hSum[1] += tmp_nColor.R;
                    hSum[2] += tmp_nColor.G;
                    hSum[3] += tmp_nColor.B;
                    //
                    iAvg[0] = hSum[0] * Avg;
                    iAvg[1] = hSum[1] * Avg;
                    iAvg[2] = hSum[2] * Avg;
                    iAvg[3] = hSum[3] * Avg;
                }
                Hblur.SetPixel(i, j, Color.FromArgb((int)iAvg[0], (int)iAvg[1], (int)iAvg[2], (int)iAvg[3]));
            }
        }
        Bitmap total = (Bitmap)Hblur.Clone();
        for (int i = 0; i < Hblur.Width; i++)
        {
            float[] tSum = new float[] { 0f, 0f, 0f, 0f };
            float[] iAvg = new float[] { 0f, 0f, 0f, 0f };
            for (int y = 0; y < kSize; y++)
            {
                Color tmpColor = Hblur.GetPixel(i, y);
                tSum[0] += tmpColor.A;
                tSum[1] += tmpColor.R;
                tSum[2] += tmpColor.G;
                tSum[3] += tmpColor.B;
            }
            iAvg[0] = tSum[0] * Avg;
            iAvg[1] = tSum[1] * Avg;
            iAvg[2] = tSum[2] * Avg;
            iAvg[3] = tSum[3] * Avg;

            for (int j = 0; j < Hblur.Height; j++)
            {
                if (j - kSize / 2 >= 0 && j + 1 + kSize / 2 < Hblur.Height)
                {
                    Color tmp_pColor = Hblur.GetPixel(i, j - kSize / 2);
                    tSum[0] -= tmp_pColor.A;
                    tSum[1] -= tmp_pColor.R;
                    tSum[2] -= tmp_pColor.G;
                    tSum[3] -= tmp_pColor.B;
                    Color tmp_nColor = Hblur.GetPixel(i, j + 1 + kSize / 2);
                    tSum[0] += tmp_nColor.A;
                    tSum[1] += tmp_nColor.R;
                    tSum[2] += tmp_nColor.G;
                    tSum[3] += tmp_nColor.B;
                    //
                    iAvg[0] = tSum[0] * Avg;
                    iAvg[1] = tSum[1] * Avg;
                    iAvg[2] = tSum[2] * Avg;
                    iAvg[3] = tSum[3] * Avg;
                }
                total.SetPixel(i, j, Color.FromArgb((int)iAvg[0], (int)iAvg[1], (int)iAvg[2], (int)iAvg[3]));
            }
        }
        return total;
    }
}
