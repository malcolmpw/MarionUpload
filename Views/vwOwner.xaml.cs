﻿using System.Windows.Controls;
using MarionUpload.ViewModels;

namespace MarionUpload.Views
{
    /// <summary>
    /// Interaction logic for vwOwner.xaml
    /// </summary>
    public partial class vwOwner : UserControl
    {
        public vwOwner()
        {
            InitializeComponent();
            DataContext = new vmOwner();
        }
    }
}
