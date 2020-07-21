using Emgu.CV;
using Emgu.CV.Structure;
using ImageEditor.Controllers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using static ImageEditor.Controllers.Transform;

namespace ImageEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Dictionary<string, string> Images;
        private Dictionary<string, string> Folders;

        public MainWindow()
        {
            //this.PipelineWorker = new BackgroundWorker();
            //this.PipelineWorker.DoWork += PipelineWorker_DoWork;
            //this.PipelineWorker.RunWorkerCompleted += PipelineWorker_RunWorkerCompleted;

            InitializeComponent();

            this.TextInput.KeyUp += TextInput_KeyUp;

            this.TextInput.Focus();
        }

        private void TextInput_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return || e.Key == Key.Enter)
            {
                string input = TextInput.Text.Trim();
                string[] parts = input.Split(' ');

                try
                {
                    switch (parts[0])
                    {
                        // load imgname path
                        case "load":
                            if (parts.Length >= 3)
                            {
                                if (this.Images == null) this.Images = new Dictionary<string, string>();

                                Images.Add(parts[1], input.Substring(input.IndexOf(parts[0] + " " + parts[1]) + (parts[0] + " " + parts[1]).Length).Trim());

                                ChatBox.AppendText("Image loaded as '" + parts[1] + "'");
                                ChatBox.AppendText(Environment.NewLine);
                            }
                            break;

                        // loadfolder dirname path
                        case "loadfolder":
                            if (parts.Length >= 3)
                            {
                                if (this.Folders == null) this.Folders = new Dictionary<string, string>();

                                Folders.Add(parts[1], input.Substring(input.IndexOf(parts[0] + " " + parts[1]) + (parts[0] + " " + parts[1]).Length).Trim());

                                ChatBox.AppendText("Folder loaded as '" + parts[1] + "'");
                                ChatBox.AppendText(Environment.NewLine);
                            }
                            break;

                        // resize imgname interpolation finalW finalH outputfolder
                        case "resize":
                            if (parts.Length >= 1)
                            {
                                int width = int.Parse(parts[3]);
                                int height = int.Parse(parts[4]);

                                Interpolations interpolation = Interpolations.Bicubic;
                                switch (parts[2].Trim().ToLower())
                                {
                                    case "nn":
                                        interpolation = Interpolations.NearestNeighbor;
                                        break;

                                    case "bl":
                                        interpolation = Interpolations.Bilinear;
                                        break;

                                    case "bc":
                                        interpolation = Interpolations.Bicubic;
                                        break;

                                    case "bcsmoother":
                                        interpolation = Interpolations.BicubicSmoother;
                                        break;

                                    case "bcsharper":
                                        interpolation = Interpolations.BicubicSharper;
                                        break;

                                    default:
                                        ChatBox.AppendText("Error: No interpolation known as " + parts[2] + ".");
                                        ChatBox.AppendText(Environment.NewLine);
                                        return;
                                }

                                Bitmap bitmap = Resize(
                                    Images[parts[1]],
                                    interpolation,
                                    width,
                                    height);

                                Image<Bgr, Byte> image = bitmap.ToImage<Bgr, Byte>();
                                image.Save(Folders[parts[5]] + "/(" + parts[1] + " " + parts[2] + ").png");

                                ChatBox.AppendText(parts[1] + " resized using " + interpolation.ToString() + ".");
                                ChatBox.AppendText(Environment.NewLine);
                            }
                            break;

                        // log imagename outputfolder
                        case "log":
                            if (parts.Length > 2)
                            {
                                Bitmap bitmap = Transformations.LogCorrection(new Bitmap(Images[parts[1]]));
                                bitmap.ToImage<Bgr, Byte>().Save(Folders[parts[2]] + "/" + parts[1] + " " + "log.png");
                                //bitmap.ToImage<Bgr, Byte>().Save("test.png");

                                ChatBox.AppendText("Log transformation applied.");
                                ChatBox.AppendText(Environment.NewLine);
                            }
                            break;

                        // gamma value imagename outputfolder
                        case "gamma":
                            if (parts.Length > 3)
                            {
                                Bitmap bitmap = Transformations.GammaCorrection(new Bitmap(Images[parts[2]]), float.Parse(parts[1]));
                                bitmap.ToImage<Bgr, Byte>().Save(Folders[parts[3]] + "/" + parts[2] + " " + parts[1] + ".png");
                                //bitmap.ToImage<Bgr, Byte>().Save("test.png");

                                ChatBox.AppendText("Gamma transformation applied.");
                                ChatBox.AppendText(Environment.NewLine);
                            }
                            break;

                        // equalize imagename outputfolder
                        case "equalize":
                            if (parts.Length > 2)
                            {
                                Bitmap bitmap = Transformations.Equalize(new Bitmap(Images[parts[1]]));
                                bitmap.ToImage<Bgr, Byte>().Save(Folders[parts[2]] + "/" + parts[1] + " " + "eq.png");

                                ChatBox.AppendText("Equalization applied.");
                                ChatBox.AppendText(Environment.NewLine);
                            }
                            break;

                        // meanfilter imagename size outputfolder
                        case "meanfilter":
                            if (parts.Length > 3)
                            {
                                Bitmap bitmap = Transformations.MeanFilter(new Bitmap(Images[parts[1]]), int.Parse(parts[2]));
                                bitmap.ToImage<Bgr, Byte>().Save(Folders[parts[3]] + "/" + parts[1] + " mean " + parts[2] + ".png");
                                //bitmap.ToImage<Bgr, Byte>().Save("test.png");

                                ChatBox.AppendText("Mean filter applied.");
                                ChatBox.AppendText(Environment.NewLine);
                            }
                            break;

                        // patch width height imagename folder
                        case "patch":
                            if (parts.Length >= 4)
                            {
                                int w = int.Parse(parts[1]);
                                int h = int.Parse(parts[2]);
                                Image<Bgr, Byte> image = new Image<Bgr, byte>(Images[parts[3]]);
                                int wPatches = image.Width / w;
                                int hPatches = image.Height / h;

                                int index = Directory.GetFiles(Folders[parts[4]]).Count(f =>
                                    f.ToLower().EndsWith(".jpg") ||
                                    f.ToLower().EndsWith(".png") ||
                                    f.ToLower().EndsWith(".gif") ||
                                    f.ToLower().EndsWith(".jpeg"));

                                int total = 0;
                                for (int i = 0; i < wPatches; i++)
                                {
                                    for (int j = 0; j < hPatches; j++)
                                    {
                                        Rectangle rectangle = new Rectangle(i * w, j * h, w, h);
                                        image.ROI = rectangle;
                                        image.Save(Folders[parts[4]] + "/(" + index + ").png");

                                        index++;
                                        total++;
                                    }
                                }

                                ChatBox.AppendText((total + 1) + " patches saved.");
                                ChatBox.AppendText(Environment.NewLine);
                            }
                            break;

                        // stitchfolders folder1 folder2 outputfolder
                        case "stitchfolders":
                            if (parts.Length >= 4)
                            {
                                string[] folderA = Directory.GetFiles(Folders[parts[1]]).Where(f =>
                                    f.ToLower().EndsWith(".jpg") ||
                                    f.ToLower().EndsWith(".png") ||
                                    f.ToLower().EndsWith(".gif") ||
                                    f.ToLower().EndsWith(".jpeg")).ToArray();


                                string[] folderB = Directory.GetFiles(Folders[parts[2]]).Where(f =>
                                    f.ToLower().EndsWith(".jpg") ||
                                    f.ToLower().EndsWith(".png") ||
                                    f.ToLower().EndsWith(".gif") ||
                                    f.ToLower().EndsWith(".jpeg")).ToArray();

                                if (folderA.Length == folderB.Length)
                                {
                                    for (int i = 0; i < folderA.Length; i++)
                                    {
                                        Image<Bgr, Byte> imageA = new Image<Bgr, byte>(folderA[i]);
                                        Image<Bgr, Byte> imageB = new Image<Bgr, byte>(folderB[i]);

                                        Image<Bgr, Byte> result = new Image<Bgr, byte>(imageA.Width + imageB.Width, Math.Max(imageA.Height, imageB.Height));

                                        result.ROI = new Rectangle(0, 0, imageA.Width, imageA.Height);
                                        imageA.CopyTo(result);
                                        result.ROI = new Rectangle(imageA.Width, 0, imageB.Width, imageB.Height);
                                        imageB.CopyTo(result);
                                        result.ROI = Rectangle.Empty;

                                        result.Save(Folders[parts[3]] + "/(" + i + ").png");
                                    }

                                    ChatBox.AppendText(folderA.Length + " stitches created");
                                    ChatBox.AppendText(Environment.NewLine);
                                }
                                else
                                {
                                    ChatBox.AppendText("Error: Folders " + Folders[parts[1]] + " and " + Folders[parts[2]] + " do not have the same size");
                                    ChatBox.AppendText(Environment.NewLine);
                                }
                            }
                            break;
                    }
                }
                catch (Exception error)
                {
                    ChatBox.AppendText("Error: " + error.Message);
                    ChatBox.AppendText(Environment.NewLine);
                }

                TextInput.SelectAll();
            }
        }
    }
}
