using MarionUpload.ViewModels;
using System.Windows.Controls;

namespace MarionUpload.Views
{    
    public partial class vwLease : UserControl
    {
        public vwLease()
        {
            InitializeComponent();
            DataContext = new vmLease();
        }
    }
}
