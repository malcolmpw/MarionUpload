using System.Windows.Controls;
using MarionUpload.ViewModels;

namespace MarionUpload.Views
{
    /// <summary>
    /// Interaction logic for vwUnit.xaml
    /// </summary>
    public partial class vwLookupUnits : UserControl
    {
        public vwLookupUnits()
        {
            InitializeComponent();
            DataContext = new vmLookupUnits();
        }
    }
}
