using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
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
    /// Interaction logic for Loading.xaml
    /// </summary>
    public partial class Loading : UserControl
    {
        public double FPS { get; set; }
        public Brush Inactive { get; set; }
        public Brush Active { get; set; }

        private Timer Timer;
        private int Index;

        public Loading()
        {
            InitializeComponent();
            this.Loaded += Loading_Loaded;
        }

        private void Loading_Loaded(object sender, RoutedEventArgs e)
        {
            (MainGrid.Children[0] as Grid).Background = Inactive;
            (MainGrid.Children[1] as Grid).Background = Inactive;
            (MainGrid.Children[2] as Grid).Background = Inactive;
            (MainGrid.Children[3] as Grid).Background = Inactive;

            MarginAnimation.To = new Thickness(0, 0, 2, 2);
            GridStoryboard.Begin(MainGrid.Children[0] as Grid);

            MarginAnimation.To = new Thickness(2, 0, 0, 2);
            GridStoryboard.Begin(MainGrid.Children[1] as Grid);

            MarginAnimation.To = new Thickness(2, 2, 0, 0);
            GridStoryboard.Begin(MainGrid.Children[2] as Grid);

            MarginAnimation.To = new Thickness(0, 2, 2, 0);
            GridStoryboard.Completed += GridStoryboard_Completed;
            GridStoryboard.Begin(MainGrid.Children[3] as Grid);

            if (FPS <= 0) FPS = 1;

            this.Timer = new Timer(1000.0 / FPS);
            this.Timer.Elapsed += Timer_Elapsed;
        }

        private void GridStoryboard_Completed(object sender, EventArgs e)
        {
            this.Timer.Start();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Index++;

            this.Dispatcher.Invoke(() =>
            {
                if (Index >= MainGrid.Children.Count) Index = 0;

                for (int i = 0; i < MainGrid.Children.Count; i++)
                {
                    (MainGrid.Children[i] as Grid).Background = (i == Index) ? Active : Inactive;
                }
            });
        }
    }
}
