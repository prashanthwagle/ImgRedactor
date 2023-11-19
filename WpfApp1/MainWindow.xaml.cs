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
        //Current Action: Point,Spray,Erase
        public enum EditAction
        {
            Pointer, Spray, Eraser
        }
        private EditAction currentAction { get; set; }


        //Spraybrush Thickness: Default 1, can range from 1-10
        private double _brushThickness = 1;
        public double BrushThickness
        {
            get { return _brushThickness; }
            set { _brushThickness = value; }
        }


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
            double sprayRadius = BrushThickness;
            int sprayDensity = (int)Math.Ceiling(sprayRadius * 2);
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
            BrushThickness = mySlider.Value;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "PNG Image|*.png|JPEG Image|*.jpg;*.jpeg|Bitmap Image|*.bmp|All Files|*.*"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                SaveImage(saveFileDialog.FileName);
            }
        }

        private void SaveImage(string filePath)
        {
            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap(
                (int)canvas.ActualWidth,
                (int)canvas.ActualHeight,
                96,
                96,
                PixelFormats.Default);

            renderTargetBitmap.Render(canvas);

            BitmapEncoder? encoder = null;
            string fileExtension = System.IO.Path.GetExtension(filePath).ToLower();

            switch (fileExtension)
            {
                case ".png":
                    encoder = new PngBitmapEncoder();
                    break;
                case ".jpg":
                case ".jpeg":
                    encoder = new JpegBitmapEncoder();
                    break;
                default:
                    MessageBox.Show("Unsupported file format.");
                    return;
            }

            encoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));

            using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
            {
                encoder.Save(fileStream);
            }

            MessageBox.Show($"Saved Successfully to {filePath}");
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveAppState();
        }

        private void SaveAppState()
        {
            if (canvas != null)
            {
                RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap(
                    (int)canvas.ActualWidth,
                    (int)canvas.ActualHeight,
                    96,
                    96,
                    PixelFormats.Pbgra32);

                renderTargetBitmap.Render(canvas);
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));

                string dirPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "finalcover_llc");
                string FILENAME = "$temp$";
                string hiddenFilePath = System.IO.Path.Combine(dirPath, FILENAME);

                try
                {
                    Directory.CreateDirectory(dirPath);
                    using (FileStream fs = new FileStream(hiddenFilePath, FileMode.Create))
                    {
                        encoder.Save(fs);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            RestoreAppState();
        }

        private void RestoreAppState()
        {
            string dirPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "finalcover_llc");
            string FILENAME = "$temp$";
            string hiddenFilePath = System.IO.Path.Combine(dirPath, FILENAME);

            try
            {
                if (File.Exists(hiddenFilePath))
                {
                    BitmapImage restoredImage = new BitmapImage(new Uri(hiddenFilePath));
                    imgCanvas.Source = restoredImage;
                    canvas.Width = restoredImage.PixelWidth;
                    canvas.Height = restoredImage.PixelHeight;
                    imgCanvas.Width = restoredImage.PixelWidth;
                    imgCanvas.Height = restoredImage.PixelHeight;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

  

    }

}