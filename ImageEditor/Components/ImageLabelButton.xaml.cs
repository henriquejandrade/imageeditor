using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ImageEditor.Components
{
    /// <summary>
    /// Interaction logic for ImageLabelButton.xaml
    /// </summary>
    public partial class ImageLabelButton : Button
    {
        public ImageSource Icon
        {
            get { return (ImageSource)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(ImageSource), typeof(ImageLabelButton));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(ImageLabelButton));

        public Brush HighlightedBackground
        {
            get { return (Brush)GetValue(HighlightedBackgroundProperty); }
            set { SetValue(HighlightedBackgroundProperty, value); }
        }

        public static readonly DependencyProperty HighlightedBackgroundProperty =
            DependencyProperty.Register("HighlightedBackground",
                typeof(Brush),
                typeof(ImageLabelButton),
                new UIPropertyMetadata((Brush)(new BrushConverter()).ConvertFromString("#5785d0")));

        public Brush PressedBackground
        {
            get { return (Brush)GetValue(PressedBackgroundProperty); }
            set { SetValue(PressedBackgroundProperty, value); }
        }

        public static readonly DependencyProperty PressedBackgroundProperty =
            DependencyProperty.Register("PressedBackground", typeof(Brush), typeof(ImageLabelButton));

        public ImageLabelButton()
        {
            InitializeComponent();
        }
    }
}
