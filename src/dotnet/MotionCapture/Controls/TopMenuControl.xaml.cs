using Microsoft.Extensions.DependencyInjection;
using MotionCapture.Core.Interfaces;
using MotionCapture.Services;
using MotionCapture.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.Design;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MotionCapture.Controls
{
    /// <summary>
    /// Логика взаимодействия для TopMenuControl.xaml
    /// </summary>
    public partial class TopMenuControl : UserControl
    {
        public TopMenuControl()
        {
            InitializeComponent();
            var service = ((App)Application.Current).Services.GetRequiredService<ApplicationState>();
            var connectionService = ((App)Application.Current).Services.GetRequiredService<IConnectionStateService>();
            DataContext = new TopMenuViewModel(service, connectionService);
        }
    }
}
