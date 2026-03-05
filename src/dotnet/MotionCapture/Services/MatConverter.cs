using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media.Imaging;

namespace MotionCapture.Services;

public static class MatConverter
{
    [DllImport("gdi32.dll")]
    private static extern bool DeleteObject(IntPtr hObject);

    public static BitmapSource ToBitmapSource(Mat mat)
    {
        if (mat == null || mat.IsEmpty)
            return null!;

        using Bitmap bitmap = mat.ToBitmap();

        IntPtr hBitmap = bitmap.GetHbitmap();

        try
        {
            var bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                hBitmap,
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            bitmapSource.Freeze();
            return bitmapSource;
        }
        finally
        {
            DeleteObject(hBitmap);
        }
    }
}