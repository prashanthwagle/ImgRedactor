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

        }

        private void BtnClearCanvas_Click(object sender, RoutedEventArgs e)
        {

        }

        private void PointerBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SprayBtn_Click(object sender, RoutedEventArgs e)
        {

        }



        private void EraserBtn_Click(object sender, RoutedEventArgs e)
        {

        }


        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {

        }


        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {

        }



    }
}