using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace H.Utilities
{
    /// <summary>
    /// 
    /// </summary>
    public static class Screenshoter
    {
        #region PInvokes

        [DllImport("gdi32.dll")]
        private static extern bool BitBlt(IntPtr hdcDest, int xDest, int yDest, int
            wDest, int hDest, IntPtr hdcSource, int xSrc, int ySrc, CopyPixelOperation rop);
        [DllImport("gdi32.dll")]
        private static extern IntPtr DeleteDC(IntPtr hDc);
        [DllImport("gdi32.dll")]
        private static extern IntPtr DeleteObject(IntPtr hDc);
        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);
        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleDC(IntPtr hdc);
        [DllImport("gdi32.dll")]
        private static extern IntPtr SelectObject(IntPtr hdc, IntPtr bmp);
        [DllImport("user32.dll")]
        private static extern IntPtr GetDesktopWindow();
        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowDC(IntPtr ptr);

        [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
        internal static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll", ExactSpelling = true)]
        internal static extern int ReleaseDC(IntPtr hWnd, IntPtr hDc);

        internal enum SystemMetric
        {
            VirtualScreenX = 76, // SM_XVIRTUALSCREEN
            VirtualScreenY = 77, // SM_YVIRTUALSCREEN
            VirtualScreenWidth = 78, // CXVIRTUALSCREEN 0x0000004E 
            VirtualScreenHeight = 79, // CYVIRTUALSCREEN 0x0000004F 
        }

        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(SystemMetric metric);

        [DllImport("gdi32.dll")]
        internal static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

        #endregion

        #region DPI

        private static int? _dpi;
        // ReSharper disable once InconsistentNaming
        private static readonly object _dpiLock = new();

        internal static int Dpi {
            get {
                if (_dpi.HasValue)
                {
                    return _dpi.Value;
                }

                lock (_dpiLock)
                {
                    if (_dpi.HasValue)
                    {
                        return _dpi.Value;
                    }

                    var dc = GetDC(IntPtr.Zero);
                    if (dc == IntPtr.Zero)
                    {
                        throw new Win32Exception(Marshal.GetLastWin32Error());
                    }

                    try
                    {
                        _dpi = GetDeviceCaps(dc, 90);

                        return _dpi.Value;
                    }
                    finally
                    {
                        ReleaseDC(IntPtr.Zero, dc);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dpi"></param>
        /// <param name="pixel"></param>
        /// <returns></returns>
        private static double ConvertPixel(int dpi, double pixel)
        {
            return dpi != 0
                ? pixel * 96.0 / dpi
                : pixel;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Returns rectangle of VirtualDisplay. X and Y can be negative.
        /// </summary>
        /// <returns></returns>
        public static Rectangle GetVirtualDisplayRectangle()
        {
            var dpi = Dpi;
            var x = (int)Math.Round(ConvertPixel(dpi, GetSystemMetrics(SystemMetric.VirtualScreenX)));
            var y = (int)Math.Round(ConvertPixel(dpi, GetSystemMetrics(SystemMetric.VirtualScreenY)));
            var width = (int)Math.Round(ConvertPixel(dpi, GetSystemMetrics(SystemMetric.VirtualScreenWidth)));
            var height = (int)Math.Round(ConvertPixel(dpi, GetSystemMetrics(SystemMetric.VirtualScreenHeight)));

            return new Rectangle(x, y, width, height);
        }

        /// <summary>
        /// Creates screenshot of all available screens. <br/>
        /// If <paramref name="rectangle"/> is not null, returns image of this region.
        /// </summary>
        /// <returns></returns>
        public static Bitmap Shot(Rectangle? rectangle = null)
        {
            var displayRectangle = GetVirtualDisplayRectangle();

            var window = GetDesktopWindow();
            var dc = GetWindowDC(window);
            var toDc = CreateCompatibleDC(dc);
            var hBmp = CreateCompatibleBitmap(dc, displayRectangle.Width, displayRectangle.Height);
            var hOldBmp = SelectObject(toDc, hBmp);

            // ReSharper disable once BitwiseOperatorOnEnumWithoutFlags
            BitBlt(toDc, 
                0, 
                0, 
                displayRectangle.Width, 
                displayRectangle.Height, 
                dc, 
                displayRectangle.X, 
                displayRectangle.Y, 
                CopyPixelOperation.CaptureBlt | CopyPixelOperation.SourceCopy);

            var bitmap = Image.FromHbitmap(hBmp);
            SelectObject(toDc, hOldBmp);
            DeleteObject(hBmp);
            DeleteDC(toDc);
            ReleaseDC(window, dc);

            if (rectangle == null)
            {
                return bitmap;
            }

            var fixedRectangle = rectangle.Value;
            fixedRectangle.X -= displayRectangle.X;
            fixedRectangle.Y -= displayRectangle.Y;

            using (bitmap)
            {
                return bitmap.Clone(fixedRectangle, bitmap.PixelFormat);
            }
        }

        /// <summary>
        /// Calls <seealso cref="Task.Run(Action, CancellationToken)"/> for <seealso cref="Shot"/>.
        /// </summary>
        /// <returns></returns>
        public static async Task<Bitmap> ShotAsync(Rectangle? rectangle = null, CancellationToken cancellationToken = default)
        {
            return await Task.Run(() => Shot(rectangle), cancellationToken)
                .ConfigureAwait(false);
        }

        #endregion
    }
}
