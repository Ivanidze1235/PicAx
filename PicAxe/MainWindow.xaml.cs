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
using System.Drawing.Drawing2D;
using System.Collections;
using System.Reflection;

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

            TransformGroup group = new TransformGroup();

            ScaleTransform xform = new ScaleTransform();
            group.Children.Add(xform);

            TranslateTransform tt = new TranslateTransform();
            group.Children.Add(tt);

            mainImage.RenderTransform = group;

            mainImage.MouseWheel += image_MouseWheel;
            mainImage.MouseLeftButtonDown += mainImage_LeftButtonDown;
            mainImage.MouseLeftButtonUp += mainImage_MouseLeftButtonUp;
            mainImage.MouseMove += mainImage_MouseMove;
        }

        string filename;
        public static Bitmap bitmap;
        public static Graphics g;
        
        public bool state = false;

        System.Windows.Point bufferPoint = new System.Windows.Point();
        private System.Windows.Point origin;
        private System.Windows.Point start;

        private void image_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            TransformGroup transformGroup = (TransformGroup)mainImage.RenderTransform;
            ScaleTransform transform = (ScaleTransform)transformGroup.Children[0];

            double zoom = e.Delta > 0 ? .2 : -.2;
            transform.ScaleX += zoom;
            transform.ScaleY += zoom;
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
                backdrop.Source = DrawFilledRectangle(bitmap.Width, bitmap.Height);
            }
        }

        private void New_Click(object sender, RoutedEventArgs e)
        {
            PopupWindow popup = new PopupWindow();
            popup.Owner = this;
            popup.Show();
        }
        public void generate()
        {
            g = Graphics.FromImage(bitmap);
            backdrop.Source = DrawFilledRectangle(bitmap.Width, bitmap.Height);
        }

        public BitmapSource DrawFilledRectangle(int x, int y)
        {
            Bitmap tempBitmap = new Bitmap(x, y);
            using (Graphics graph = Graphics.FromImage(tempBitmap))
            {
                System.Drawing.Rectangle ImageSize = new System.Drawing.Rectangle(0, 0, x, y);
                graph.FillRectangle(System.Drawing.Brushes.White, ImageSize);
            }
            
            BitmapSource i = Imaging.CreateBitmapSourceFromHBitmap(
                               tempBitmap.GetHbitmap(),
                               IntPtr.Zero,
                               Int32Rect.Empty,
                               BitmapSizeOptions.FromEmptyOptions());
                return i;
            
        }

        public BitmapSource DrawEmptyRectangle(int x, int y)
        {
            bitmap = new Bitmap(x, y);
            using (Graphics graph = Graphics.FromImage(bitmap))
            {
                System.Drawing.Rectangle ImageSize = new System.Drawing.Rectangle(0, 0, x, y);
                graph.FillRectangle(System.Drawing.Brushes.Transparent, ImageSize);
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
                FileName.Header = filename;
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
                            encoder.Palette = BitmapPalettes.Halftone256Transparent;
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
            Actions.state = Actions.states.draw;
        }

        private void Drag_Click(object sender, RoutedEventArgs e)
        {
            mainImage.Cursor = Cursors.Hand;
            Actions.state = Actions.states.drag;
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

        private void mainImage_MouseMove(object sender, MouseEventArgs e)
        {
            switch (Actions.state)
            {
                default:
                    break;
                case Actions.states.draw:
                    if (state)
                    {
                        try
                        {
                            var position = Mouse.GetPosition(mainImage);
                            position = PositionToPixel(position);
                            System.Drawing.Pen pen = new System.Drawing.Pen(System.Drawing.Color.Black, 14);

                            pen.EndCap = LineCap.Round;
                            pen.StartCap = LineCap.Round;

                            g.DrawLine(pen, (float)position.X, (float)position.Y, (float)bufferPoint.X, (float)bufferPoint.Y);
                            bufferPoint = position;
                        }
                        catch
                        {
                            MessageBox.Show("Could not draw a square!");
                            throw new Exception("Could not draw a square!");
                        }
                        try
                        {
                            IntPtr hBitmap = bitmap.GetHbitmap();
                            mainImage.Source = Imaging.CreateBitmapSourceFromHBitmap(
                                                                            hBitmap,
                                                                            IntPtr.Zero,
                                                                            Int32Rect.Empty,
                                                                            BitmapSizeOptions.FromEmptyOptions());
                            DeleteObject(hBitmap);
                        }
                        catch
                        {
                            MessageBox.Show("couldn't update displayed image.");
                            throw new Exception("couldn't update displayed image.");
                        }

                    }
                    break;
                case Actions.states.drag:
                    if (!mainImage.IsMouseCaptured) return;

                    var tt = (TranslateTransform)((TransformGroup)mainImage.RenderTransform).Children.First(tr => tr is TranslateTransform);
                    Vector v = start - e.GetPosition(border);
                    tt.X = origin.X - v.X;
                    tt.Y = origin.Y - v.Y;
                    break;
            }
        }

        private void mainImage_LeftButtonDown(object sender, MouseEventArgs e)
        {
            switch (Actions.state)
            {
                case Actions.states.draw:
                    state = true;
                    bufferPoint = PositionToPixel(e.GetPosition(mainImage));
                    mainImage_MouseMove(sender, e);
                    break;
                case Actions.states.drag:
                    mainImage.CaptureMouse();
                    var tt = (TranslateTransform)((TransformGroup)mainImage.RenderTransform).Children.First(tr => tr is TranslateTransform);
                    start = e.GetPosition(border);
                    origin = new System.Windows.Point(tt.X, tt.Y);
                    break;
                case Actions.states.erase:
                    break;
                default:
                    break;
            }
        }

        private void mainImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            switch (Actions.state)
            {
                case Actions.states.draw:
                    state = false;
                    break;
                case Actions.states.drag:
                    mainImage.ReleaseMouseCapture();
                    break;
                case Actions.states.erase:
                    break;
                default:
                    break;
            }
            
        }

        private void mainImage_MouseEnter(object sender, MouseEventArgs e)
        {
            if (bitmap != null)
            {
                g = Graphics.FromImage(bitmap);
                
                
            }
        }

        private void mainImage_MouseLeave(object sender, MouseEventArgs e)
        {
            g = null;
        }

        
    }

    public static class Actions
    {
        //public static int state = 0;
        public enum states
        {
            none,
            draw,
            drag,
            erase,
        }
        
        public static states state = states.none;
    }
}