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
using System.Windows.Markup;


namespace SprayPaintApp
{
    public partial class MainWindow : Window
    {
        //Constants for the file path where data is automatically saved
        private const string AppDataFolder = "finalcover_llc";
        private const string FileName = "$temp$.xaml";

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

        /// <summary>
        /// Initialization
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }


        private void ShowErrorMessage(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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


        /// <summary>
        /// Loads an image from file.
        /// </summary>
        /// <param name="sender">The object that triggered the event.</param>
        /// <param name="e">The event arguments.</param>
        private void BtnLoadFromFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Filter = "PNG Image|*.png|JPEG Image|*.jpg;*.jpeg|Bitmap Image|*.bmp",
                    DefaultExt = ".png" 
                };
                if (openFileDialog.ShowDialog() == true)
                {
                    Uri fileUri = new Uri(openFileDialog.FileName);
                    if (!File.Exists(fileUri.LocalPath))
                    {
                        ShowErrorMessage("Selected file does not exist.");
                        return;
                    }

                    BitmapImage newImage = new BitmapImage(fileUri);
                    CreateImageOnCanvas(newImage);
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error loading image: {ex.Message}");
            }
        }


        /// <summary>
        /// Clears the paintCanvas
        /// </summary>
        /// <param name="sender">The object that triggered the event.</param>
        /// <param name="e">The event arguments.</param>
        private void BtnClearCanvas_Click(object sender, RoutedEventArgs e)
        {
            paintCanvas.Children.Clear();
        }

        /// <summary>
        /// Handles selection of Pointer Button 
        /// </summary>
        /// <param name="sender">The object that triggered the event.</param>
        /// <param name="e">The event arguments.</param>
        private void PointerBtn_Click(object sender, RoutedEventArgs e)
        {
            SetEditAction(EditAction.Pointer);
        }

        /// <summary>
        /// Handles selection of Spray button
        /// </summary>
        /// <param name="sender">The object that triggered the event.</param>
        /// <param name="e">The event arguments.</param>
        private void SprayBtn_Click(object sender, RoutedEventArgs e)
        {
            SetEditAction(EditAction.Spray);
        }

        /// <summary>
        /// Handles the mouse movement on the canvas
        /// </summary>
        /// <param name="sender">The object that triggered the event.</param>
        /// <param name="e">The event arguments.</param>
        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                if (currentAction == EditAction.Spray)
                {
                    if (e.LeftButton == MouseButtonState.Pressed)
                    {
                        SprayPaint(e.GetPosition(paintCanvas));
                    }
                }
                else if (currentAction == EditAction.Eraser)
                {
                    if (e.LeftButton == MouseButtonState.Pressed)
                    {
                        EraseSprayPaint(e.GetPosition(paintCanvas));
                    }
                }
            }
            catch (Exception exception)
            {
                ShowErrorMessage($"Error in mouse move: {exception.Message}");
            }
        }

        /// <summary>
        /// Handles the slider for the size of the spray
        /// </summary>
        /// <param name="sender">The object that triggered the event.</param>
        /// <param name="e">The event arguments.</param>
        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            BrushThickness = spraySize.Value;
            SetEditAction(EditAction.Spray);
        }

        /// <summary>
        /// Handles the color that has been picked to spray
        /// </summary>
        /// <param name="sender">The object that triggered the event.</param>
        /// <param name="e">The event arguments.</param>
        private void SprayClrPickerChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            sprayPaintClr = SprayClrPicker.SelectedColor;
            SetEditAction(EditAction.Spray);
        }


        /// <summary>
        /// Handles selection of Eraser button
        /// </summary>
        /// <param name="sender">The object that triggered the event.</param>
        /// <param name="e">The event arguments.</param>
        private void EraserBtn_Click(object sender, RoutedEventArgs e)
        {
            SetEditAction(EditAction.Eraser);
        }

        /// <summary>
        /// Event handler for the closing event of the main window.
        /// </summary>
        /// <param name="sender">The object that triggered the event.</param>
        /// <param name="e">The event arguments.</param>
        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveAppState();
        }


        /// <summary>
        /// Event handler for the loading event of the main window.
        /// </summary>
        /// <param name="sender">The object that triggered the event.</param>
        /// <param name="e">The event arguments.</param>
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            RestoreAppState();
        }



        /// <summary>
        /// Handles selection of the three actions: Pointer (for moving around the canvas), Spray (for spraying on the canvas), Eraser (for erasing the spray on the canvas)
        /// </summary>
        /// <param name="action">The object that triggered the event.</param>

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


        /// <summary>
        /// Initiates the spray paint action on the canvas at the specified mouse position.
        /// </summary>
        /// <param name="mousePosition">The position of the mouse on the canvas.</param>
        private void SprayPaint(Point mousePosition)
        {
            try
            {
                if (sprayPaintClr == null)
                {
                    ShowErrorMessage("Spray paint color is not set");
                    return;
                }

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
            catch (Exception exception)
            {
                ShowErrorMessage($"Error while Spraying Paint: {exception.Message}");
            }
        }

        /// <summary>
        /// Initiates the erase action on the canvas at the specified mouse position.
        /// </summary>
        /// <param name="mousePosition">The position of the mouse on the canvas.</param>
        private void EraseSprayPaint(Point mousePosition)
        {
            try
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
            catch (ArgumentNullException nullEx)
            {
                ShowErrorMessage($"Error while Erasing Paint: {nullEx.Message}");
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error while Erasing Paint: {ex.Message}");
            }
        }





        /// <summary>
        /// Handles saving of the project. A project is an image with or without spraypaint on it.
        /// </summary>
        /// <param name="sender">The object that triggered the event.</param>
        /// <param name="e">The event arguments.</param>
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "PNG Image|*.png|JPEG Image|*.jpg;*.jpeg|All Files|*.*"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    SaveImage(saveFileDialog.FileName);
                }
            }
            catch (ArgumentNullException nullException)
            {
                ShowErrorMessage($"Error while saving project: {nullException.Message}");
            }
            catch (UnauthorizedAccessException unauthorizedException)
            {
                ShowErrorMessage($"Error while saving project: {unauthorizedException.Message}");
            }
            catch (Exception exception)
            {
                ShowErrorMessage($"Error while saving project: {exception.Message}");
            }
        }

        /// <summary>
        /// Saves the project as an image file. Allowed file types: png, jpeg/jpg
        /// </summary>
        /// <param name="filePath">The path where the image file will be saved.</param>
        private void SaveImage(string filePath)
        {
            try
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
                        throw new NotSupportedException("Unsupported file format.");
                }

                encoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));

                using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                {
                    encoder.Save(fileStream);
                }

                MessageBox.Show($"Saved Successfully to {filePath}");
            }
            catch (UnauthorizedAccessException unauthorizedException)
            {
                ShowErrorMessage($"Error while saving image: {unauthorizedException.Message}");
            }
            catch (NotSupportedException notSupportedException)
            {
                ShowErrorMessage($"Error while saving image: {notSupportedException.Message}");
            }
            catch (Exception exception)
            {
                ShowErrorMessage($"Error while saving image: {exception.Message}");
            }
        }

        /// <summary>
        /// Saves the current state of the canvas to a hidden XAML file for automatically restoring the next time the app is opened
        /// </summary>
        private void SaveAppState()
        {
            
            if (paintCanvas != null)
            {
                string dirPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AppDataFolder);
                string FILENAME = FileName;
                string hiddenFilePath = System.IO.Path.Combine(dirPath, FILENAME);

                try
                {
                    Directory.CreateDirectory(dirPath);
                    using (FileStream fs = new FileStream(hiddenFilePath, FileMode.Create))
                    {
                        XamlWriter.Save(paintCanvas, fs);
                    }
                }
                catch (UnauthorizedAccessException unauthorizedException)
                {
                    ShowErrorMessage($"Error autosaving file: {unauthorizedException.Message}");
                }
                catch (Exception exception)
                {
                    ShowErrorMessage($"Error autosaving file: {exception.Message}");
                }
            }
            
        }



        /// <summary>
        /// Restores the saved application state from the hidden XAML file, if it exists
        /// </summary>
        private void RestoreAppState()
        {
            string dirPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AppDataFolder);
            string FILENAME = FileName;
            string hiddenFilePath = System.IO.Path.Combine(dirPath, FILENAME);
            List<UIElement> restoredElements = new List<UIElement>();

            try
            {
                if (File.Exists(hiddenFilePath))
                {
                    using (FileStream fs = new FileStream(hiddenFilePath, FileMode.Open, FileAccess.Read))
                    {
                        Canvas loadedCanvas = XamlReader.Load(fs) as Canvas;

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
            }
            catch (UnauthorizedAccessException unauthorizedException)
            {
                ShowErrorMessage($"Error autoloading previous state: {unauthorizedException.Message}");
            }
            catch (Exception exception)
            {
                ShowErrorMessage($"Error autoloading previous state: {exception.Message}");
            }
        }



    }

}