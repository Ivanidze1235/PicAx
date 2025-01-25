using System;
using System.Data.EntityClient;
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
using System.Threading;
using System.Runtime.InteropServices;
using System.Management.Instrumentation;
using System.Drawing.Imaging;
using System.Data.Entity.Design;
using System.Data.Objects;

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
        Bitmap bitmap;

        private void image_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScaleTransform st = imageScale;
            
            double zoom = e.Delta > 0 ? .2 : -.2;
            st.ScaleX += zoom;
            st.ScaleY += zoom;
        }
        private void Open_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".jpg";
            dlg.Filter = "JPEG Files (*.jpeg)|*.jpeg|PNG Files (*.png)|*.png|JPG Files (*.jpg)|*.jpg|GIF Files (*.gif)|*.gif|BMP Files (*.bmp)|*.bmp";

            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                // Open document 
                filename = dlg.FileName;
                FileName.Header = filename;
                mainImage.Source = new BitmapImage(new Uri(filename));
                bitmap = BitmapFromSource((BitmapSource)mainImage.Source);
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
            using (Graphics graph = Graphics.FromImage(bitmap))
            {
                System.Drawing.Rectangle ImageSize = new System.Drawing.Rectangle(0, 0, x, y);
                graph.FillRectangle(System.Drawing.Brushes.White, ImageSize);
            }
            
            BitmapSource i = Imaging.CreateBitmapSourceFromHBitmap(
                               bitmap.GetHbitmap(),
                               IntPtr.Zero,
                               Int32Rect.Empty,
                               BitmapSizeOptions.FromEmptyOptions());
                return i;
            
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();

            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".png";
            dlg.FileName = "untitled";
            dlg.Filter = "JPEG Files (*.jpeg)|*.jpeg|PNG Files (*.png)|*.png|JPG Files (*.jpg)|*.jpg|GIF Files (*.gif)|*.gif|BMP Files (*.bmp)|*.bmp";
            dlg.AddExtension = true;
            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                filename = dlg.FileName;

                ImageToFile((BitmapSource)mainImage.Source, filename, dlg.DefaultExt);
            }
        }

        public static void ImageToFile(BitmapSource image, string filePath, string extention)
        {
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                try
                {
                    BitmapEncoder encoder;
                    switch (extention)
                    {
                        default:
                            encoder = new JpegBitmapEncoder();
                            break;

                        case ("png"):
                            encoder = new PngBitmapEncoder();
                            break;

                        case ("bmp"):
                            encoder = new BmpBitmapEncoder();
                            break;

                        case ("jpg"):
                            encoder= new JpegBitmapEncoder();
                            break;

                        case ("gif"):
                            encoder= new GifBitmapEncoder();
                            break;
                    }
                    
                    encoder.Frames.Add(BitmapFrame.Create(image));
                    encoder.Save(fileStream);
                }
                catch
                {
                    throw new FileNotFoundException("No source bitmap found.");
                }
                
            }
        }

        public Bitmap BitmapFromSource(System.Windows.Media.Imaging.BitmapSource bitmapsource)
        {
            //convert image format
            var src = new System.Windows.Media.Imaging.FormatConvertedBitmap();
            src.BeginInit();
            src.Source = bitmapsource;
            src.DestinationFormat = System.Windows.Media.PixelFormats.Bgra32;
            src.EndInit();

            //copy to bitmap
            Bitmap bitmap = new Bitmap(src.PixelWidth, src.PixelHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            var data = bitmap.LockBits(new System.Drawing.Rectangle(System.Drawing.Point.Empty, bitmap.Size), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            src.CopyPixels(System.Windows.Int32Rect.Empty, data.Scan0, data.Height * data.Stride, data.Stride);
            bitmap.UnlockBits(data);
            bitmapsource = null;
            src = null;
            return bitmap;
        }

        public static BitmapSource ConvertToSource(System.Drawing.Bitmap bitmap)
        {
            var bitmapData = bitmap.LockBits(
                new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);

            var bitmapSource = BitmapSource.Create(
                bitmapData.Width, bitmapData.Height,
                bitmap.HorizontalResolution, bitmap.VerticalResolution,
                PixelFormats.Bgr24, null,
                bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);

            bitmap.UnlockBits(bitmapData);
            bitmap = null;

            return bitmapSource;
        }

        private void Brush_Click(object sender, RoutedEventArgs e)
        {
            mainImage.Cursor = Cursors.Cross;
            Actions.state = 1;
        }

        private System.Windows.Point PositionToPixel(System.Windows.Point position)
        {
            double ratio = mainImage.Source.Height / mainImage.ActualHeight;
            position.X *= ratio;
            position.Y *= ratio;

            return position;
        }

        [DllImport("gdi32.dll")]

        public static extern bool DeleteObject(IntPtr hObject);

        private void Draw(object sender, MouseButtonEventArgs e)
        {
            switch (Actions.state)
            {
                default:
                    break;
                case 1:
                    var position = Mouse.GetPosition(mainImage);
                    position = PositionToPixel(position);
                    try
                    {
                        using (Graphics g = Graphics.FromImage(bitmap))
                        {
                            // Draw onto the bitmap here
                            // ....
                            g.FillRectangle(System.Drawing.Brushes.Red, System.Drawing.Rectangle.FromLTRB(
                                (int)position.X - 10,
                                (int)position.Y - 10,
                                (int)position.X + 10,
                                (int)position.Y + 10));
                            g.Dispose();
                        }
                    }
                    catch
                    {
                        MessageBox.Show("Could not draw a square!");
                        throw new Exception("Could not draw a square!");
                    }
                    
                    
                    IntPtr hBitmap = bitmap.GetHbitmap();
                    
                    mainImage.Source = Imaging.CreateBitmapSourceFromHBitmap(
                                                                        hBitmap,
                                                                        IntPtr.Zero,
                                                                        Int32Rect.Empty,
                                                                        BitmapSizeOptions.FromEmptyOptions());

                    try
                    {
                        DeleteObject(hBitmap);
                    }
                    catch
                    {
                        MessageBox.Show("bitmap handle did not get deleted");
                        throw new InvalidOperationException("bitmap handler not deleted");
                    }
                    
                    break;
            }
        }
        
    }



    public static class Actions
    {
        public static int state = 0;
    }
}