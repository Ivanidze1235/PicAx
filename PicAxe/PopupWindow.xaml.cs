using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PicAxe
{
    /// <summary>
    /// Interaction logic for PopupWindow.xaml
    /// </summary>
    public partial class PopupWindow : Window
    {
        public PopupWindow()
        {
            InitializeComponent();
        }

        private void New_Image(object sender, RoutedEventArgs e)
        {
            var window = this.Owner as MainWindow;
            window.mainImage.Source = window.DrawFilledRectangle(Convert.ToInt32(widthBox.Text), Convert.ToInt32(heightBox.Text));
            this.Hide();
        }
    }
}