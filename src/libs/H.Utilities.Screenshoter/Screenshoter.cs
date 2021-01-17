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

        [DllImport("user32", CharSet = CharSet.Unicode)]
        private static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lpRect, MonitorEnumProc callback, int dwData);
        
        private delegate bool MonitorEnumProc(IntPtr hDesktop, IntPtr hdc, ref RECT pRect, IntPtr dwData);

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

            bool Callback(IntPtr hDesktop, IntPtr intPtr, ref RECT rect, IntPtr intPtr1)
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
            }

            EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, Callback, 0);

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

            var window = User32.GetDesktopWindow();
            using var dc = User32.GetWindowDC(window);
            using var toDc = Gdi32.CreateCompatibleDC(dc);
            var hBmp = Gdi32.CreateCompatibleBitmap(dc, displayRectangle.Width, displayRectangle.Height);
            var hOldBmp = Gdi32.SelectObject(toDc, hBmp);

            // ReSharper disable once BitwiseOperatorOnEnumWithoutFlags
            Gdi32.BitBlt(toDc.DangerousGetHandle(), 
                0, 
                0, 
                displayRectangle.Width, 
                displayRectangle.Height, 
                dc.DangerousGetHandle(), 
                displayRectangle.X, 
                displayRectangle.Y, 
                (int)(CopyPixelOperation.CaptureBlt | CopyPixelOperation.SourceCopy));

            var bitmap = Image.FromHbitmap(hBmp);
            Gdi32.SelectObject(toDc, hOldBmp);
            Gdi32.DeleteObject(hBmp);
            Gdi32.DeleteDC(toDc); //?
            User32.ReleaseDC(window, dc.DangerousGetHandle()); //?

            return bitmap;
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
