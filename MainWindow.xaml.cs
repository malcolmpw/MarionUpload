using MarionUpload.ViewModels;
using System.Windows;

namespace MarionUpload
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new vmMain();
        }
    }
}
