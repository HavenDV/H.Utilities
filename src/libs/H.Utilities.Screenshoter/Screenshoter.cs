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

        [DllImport("user32.dll", ExactSpelling = true)]
        internal static extern int ReleaseDC(IntPtr hWnd, IntPtr hDc);

        #endregion

        #region DPI

        [DllImport("user32", CharSet = CharSet.Unicode)]
        private static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lpRect, MonitorEnumProc callback, int dwData);
        
        private delegate bool MonitorEnumProc(IntPtr hDesktop, IntPtr hdc, ref Rect pRect, IntPtr dwData);

        [StructLayout(LayoutKind.Sequential)]
        private struct Rect
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern bool GetMonitorInfo(IntPtr hmonitor, [In, Out] MonitorInfoEx info);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 4)]
        internal class MonitorInfoEx
        {
            public int cbSize = Marshal.SizeOf(typeof(MonitorInfoEx));
            public RECT rcMonitor;
            public RECT rcWork;
            public int dwFlags;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public char[] szDevice = new char[32];
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
            MonitorEnumProc callback = (IntPtr hDesktop, IntPtr _, ref Rect _, IntPtr _) =>
            {
                var info = new MonitorInfoEx();
                GetMonitorInfo(hDesktop, info);
                
                var settings = new DEVMODE();
                User32.EnumDisplaySettings(
                    info.szDevice,
                    User32.ENUM_CURRENT_SETTINGS,
                    ref settings);

                var x = settings.dmPosition.x;
                var y = settings.dmPosition.y;
                var width = settings.dmPelsWidth;
                var height = settings.dmPelsHeight;

                left = Math.Min(left, x);
                right = Math.Max(right, x + width);
                top = Math.Min(top, y);
                bottom = Math.Max(bottom, y + height);

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
