using MotionCapture.ViewModels;
using System.Windows.Controls;

namespace MotionCapture.Controls
{
    /// <summary>
    /// Логика взаимодействия для CameraCaptureControl.xaml
    /// </summary>
    public partial class CameraCaptureControl : UserControl
    {
        public CameraCaptureControl(CameraViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
    }
}
