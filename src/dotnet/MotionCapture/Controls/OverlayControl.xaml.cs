using Microsoft.Extensions.DependencyInjection;
using MotionCapture.Core.Interfaces;
using MotionCapture.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace MotionCapture.Controls
{
    /// <summary>
    /// Логика взаимодействия для OverlayControl.xaml
    /// </summary>
    public partial class OverlayControl : UserControl
    {
        public OverlayControl()
        {
            InitializeComponent();
            var service = ((App)Application.Current).Services.GetRequiredService<IConnectionStateService>();
            DataContext = new OverlayViewModel(service);
        }
    }
}
