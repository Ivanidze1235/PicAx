using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Drawing;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Net.Mime.MediaTypeNames;
using System.Windows.Interop;
using System.Windows.Controls.Primitives;
using System.IO;

namespace PicAxe
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        string filename;

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".jpg";
            dlg.Filter = "JPEG Files (*.jpeg)|*.jpeg|PNG Files (*.png)|*.png|JPG Files (*.jpg)|*.jpg|GIF Files (*.gif)|*.gif";

            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                // Open document 
                filename = dlg.FileName;
                FileName.Header = filename;
                mainImage.Source = new BitmapImage(new Uri(filename));
            }
        }

        private void New_Click(object sender, RoutedEventArgs e)
        {
            PopupWindow popup = new PopupWindow();
            popup.Owner = this;
            popup.Show();
        }

        public BitmapSource DrawFilledRectangle(int x, int y)
        {
            Bitmap bmp = new Bitmap(x, y);
            using (Graphics graph = Graphics.FromImage(bmp))
            {
                System.Drawing.Rectangle ImageSize = new System.Drawing.Rectangle(0, 0, x, y);
                graph.FillRectangle(System.Drawing.Brushes.White, ImageSize);
            }
            
            BitmapSource i = Imaging.CreateBitmapSourceFromHBitmap(
                               bmp.GetHbitmap(),
                               IntPtr.Zero,
                               Int32Rect.Empty,
                               BitmapSizeOptions.FromEmptyOptions());
                return i;
            
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            SaveImageToJPEG(mainImage, "C:\\Users\\ivani\\Pictures\\untitled.jpg");
        }

        private void SaveImageToJPEG(System.Windows.Controls.Image ImageToSave, string Location)
        {
            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap((int)ImageToSave.ActualWidth,
                                                                           (int)ImageToSave.ActualHeight,
                                                                           100, 100, PixelFormats.Default);
            renderTargetBitmap.Render(ImageToSave);
            JpegBitmapEncoder jpegBitmapEncoder = new JpegBitmapEncoder();
            jpegBitmapEncoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));
            using (FileStream fileStream = new FileStream(Location, FileMode.Create))
            {
                jpegBitmapEncoder.Save(fileStream);
                fileStream.Flush();
                fileStream.Close();
            }
        }
    }
}
