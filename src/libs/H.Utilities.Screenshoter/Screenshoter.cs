using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using PInvoke;

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

        #endregion

        #region DPI

        [DllImport("user32")]
        private static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lpRect, MonitorEnumProc callback, int dwData);
        
        private delegate bool MonitorEnumProc(IntPtr hDesktop, IntPtr hdc, ref Rect pRect, int dwData);

        [StructLayout(LayoutKind.Sequential)]
        private struct Rect
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Returns rectangle of VirtualDisplay. X and Y can be negative.
        /// </summary>
        /// <returns>Returns rectangle of VirtualDisplay. X and Y can be negative.</returns>
        public static Rectangle GetVirtualDisplayRectangle()
        {
            var left = 0.0;
            var right = 0.0;
            var top = 0.0;
            var bottom = 0.0;
            MonitorEnumProc callback = (IntPtr hDesktop, IntPtr hdc, ref Rect prect, int d) =>
            {
                SHCore.GetScaleFactorForMonitor(hDesktop, out var scaleFactor)
                    .ThrowOnFailure();
                var scale = 1.0 + 1.25 * ((int)scaleFactor / 100.0 - 1.0);

                left = Math.Min(left, scale * prect.left);
                right = Math.Max(right, prect.left + scale * (prect.right - prect.left));
                top = Math.Min(top, scale * prect.top);
                bottom = Math.Max(bottom, prect.top + scale * (prect.bottom - prect.top));

                return true;
            };

            EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, callback, 0);

            return Rectangle.FromLTRB((int)left, (int)top, (int)right, (int)bottom);
        }

        /// <summary>
        /// Creates screenshot of all available screens. <br/>
        /// If <paramref name="rectangle"/> is not null, returns image of this region.
        /// </summary>
        /// <returns></returns>
        public static Image Shot(Rectangle? rectangle = null)
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
        public static async Task<Image> ShotAsync(
            Rectangle? rectangle = null, 
            CancellationToken cancellationToken = default)
        {
            return await Task.Run(() => Shot(rectangle), cancellationToken)
                .ConfigureAwait(false);
        }

        #endregion
    }
}
