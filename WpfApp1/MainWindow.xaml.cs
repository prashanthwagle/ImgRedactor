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

        //Variable for the Spray Brush Color
        private Color? sprayPaintClr = Colors.Black;


        public MainWindow()
        {
            InitializeComponent();
        }

        private Image CreateImageOnCanvas(BitmapImage bitmapImage)
        {
            Image imgCanvas = new Image();
            imgCanvas.Source = bitmapImage;
            imgCanvas.Stretch = Stretch.Uniform;
            paintCanvas.Children.Clear();
            paintCanvas.Children.Add(imgCanvas);

            Binding widthBinding = new Binding("ActualWidth")
            {
                RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(ScrollViewer), 1)
            };

            Binding heightBinding = new Binding("ActualHeight")
            {
                RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(ScrollViewer), 1)
            };

            imgCanvas.SetBinding(FrameworkElement.WidthProperty, widthBinding);
            imgCanvas.SetBinding(FrameworkElement.HeightProperty, heightBinding);

            return imgCanvas;
        }



        private void BtnLoadFromFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                Uri fileUri = new Uri(openFileDialog.FileName);
                BitmapImage newImage = new BitmapImage(fileUri);
                CreateImageOnCanvas(newImage);
            }
        }

        private void BtnClearCanvas_Click(object sender, RoutedEventArgs e)
        {
            paintCanvas.Children.Clear();
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
                    SprayPaint(e.GetPosition(paintCanvas));
                }
            } else if (currentAction == EditAction.Eraser)
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    EraseSprayPaint(e.GetPosition(paintCanvas));
                }
            }
        }

        private void SprayPaint(Point mousePosition)
        {
            double sprayRadius = BrushThickness;
            int sprayDensity = (int)Math.Ceiling(sprayRadius * 2);
            Random random = new Random();

            for (int i = 0; i < sprayDensity; i++)
            {
                double angle = random.NextDouble() * 2 * Math.PI;
                double radius = Math.Sqrt(random.NextDouble()) * sprayRadius;

                double x = mousePosition.X + radius * Math.Cos(angle);
                double y = mousePosition.Y + radius * Math.Sin(angle);

                Ellipse ellipse = new Ellipse
                {
                    Width = 3,
                    Height = 3,
                    Fill = new SolidColorBrush(sprayPaintClr.Value),
                    Margin = new Thickness(x, y, 0, 0)
                };

                paintCanvas.Children.Add(ellipse);
            }
        }

        private void EraseSprayPaint(Point mousePosition)
        {
            double eraseRadius = 5.0;
            List<UIElement> elementsToRemove = new List<UIElement>();

            foreach (UIElement element in paintCanvas.Children)
            {
                if (element is Ellipse ellipse)
                {
                    Point ellipsePoint = element.TransformToAncestor(paintCanvas).Transform(new Point(0, 0));

                    if (Math.Abs(ellipsePoint.X - mousePosition.X) < eraseRadius && Math.Abs(ellipsePoint.Y - mousePosition.Y) < eraseRadius)
                    {
                        elementsToRemove.Add(ellipse);
                    }
                }
            }

            foreach (UIElement elementToRemove in elementsToRemove)
            {
                paintCanvas.Children.Remove(elementToRemove);
            }
        }



        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            BrushThickness = mySlider.Value;
        }

        private void SprayClrPickerChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            sprayPaintClr = SprayClrPicker.SelectedColor;
            SetEditAction(EditAction.Spray);
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
                (int)paintCanvas.ActualWidth,
                (int)paintCanvas.ActualHeight,
                96,
                96,
                PixelFormats.Default);

            renderTargetBitmap.Render(paintCanvas);

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
            if (paintCanvas != null)
            {
                string dirPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "finalcover_llc");
                string FILENAME = "$temp$.xaml";
                string hiddenFilePath = System.IO.Path.Combine(dirPath, FILENAME);

                try
                {
                    Directory.CreateDirectory(dirPath);
                    using (FileStream fs = new FileStream(hiddenFilePath, FileMode.Create))
                    {
                        XamlWriter.Save(paintCanvas, fs);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error AutoSaving file: {ex.Message}");
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
            string FILENAME = "$temp$.xaml";
            string hiddenFilePath = System.IO.Path.Combine(dirPath, FILENAME);
            List<UIElement> restoredElements = new List<UIElement>();

            try
            {
                using (FileStream fs = new FileStream(hiddenFilePath, FileMode.Open, FileAccess.Read))
                {
                    Canvas loadedCanvas = XamlReader.Load(fs) as Canvas;

                    Console.WriteLine(loadedCanvas.Children);

                    if (loadedCanvas != null)
                    {

                        foreach (UIElement child in loadedCanvas.Children)
                        {
                            restoredElements.Add(child);
                  
                        }

                        foreach (UIElement child in restoredElements)
                        {
                            loadedCanvas.Children.Remove(child);

                            paintCanvas.Children.Add(child);
                        }

                    }
                }
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error Loading file: {ex.Message}");
            }
        }

  

    }

}