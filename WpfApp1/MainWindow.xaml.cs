using Microsoft.Win32;
using System.IO;
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

namespace SprayPaintApp
{
    public partial class MainWindow : Window
    {
        public enum EditAction
        {
            Pointer, Spray, Eraser
        }
        private EditAction currentAction { get; set; }


        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnLoadFromFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                Uri fileUri = new Uri(openFileDialog.FileName);
                imgCanvas.Source = new BitmapImage(fileUri);
            }
        }

        private void BtnClearCanvas_Click(object sender, RoutedEventArgs e)
        {
            canvas.Children.Clear();
        }

        private void PointerBtn_Click(object sender, RoutedEventArgs e)
        {
            SetEditAction(EditAction.Pointer);
        }

        private void SprayBtn_Click(object sender, RoutedEventArgs e)
        {
            SetEditAction(EditAction.Spray);
        }

        private void EraserBtn_Click(object sender, RoutedEventArgs e)
        {
            SetEditAction(EditAction.Eraser);
        }

        private void SetEditAction(EditAction action)
        {
            PointerBtn.IsChecked = false;
            SprayBtn.IsChecked = false;
            EraserBtn.IsChecked = false;

            currentAction = action;

            switch (action)
            {
                case EditAction.Pointer:
                    PointerBtn.IsChecked = true;
                    break;
                case EditAction.Spray:
                    SprayBtn.IsChecked = true;
                    break;

                case EditAction.Eraser:
                    EraserBtn.IsChecked = true;
                    break;

            }

        }


        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (currentAction == EditAction.Spray)
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    SprayPaint(e.GetPosition(canvas));
                }
            }
        }

        private void SprayPaint(Point position)
        {
            int sprayRadius = 5;
            int sprayDensity = 10;
            Random random = new Random();

            for (int i = 0; i < sprayDensity; i++)
            {
                double angle = random.NextDouble() * 2 * Math.PI;
                double radius = Math.Sqrt(random.NextDouble()) * sprayRadius;

                double x = position.X + radius * Math.Cos(angle);
                double y = position.Y + radius * Math.Sin(angle);

                Ellipse ellipse = new Ellipse
                {
                    Width = 3,
                    Height = 3,
                    Fill = new SolidColorBrush(Colors.Aqua),
                    Margin = new Thickness(x, y, 0, 0)
                };

                canvas.Children.Add(ellipse);
            }
        }


        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Console.WriteLine("Button clicked!");
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Button clicked!");
        }



    }
}