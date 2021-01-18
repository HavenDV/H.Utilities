using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using H.Utilities.Extensions;
using PInvoke;

namespace H.Utilities
{
    /// <summary>
    /// 
    /// </summary>
    public static class Screenshoter
    {
        #region PInvokes

        [DllImport("user32", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool EnumDisplayMonitors(
            nint hdc,
            nint lpRect, 
            MonitorEnumProc callback,
            nint dwData);
        
        private delegate bool MonitorEnumProc(
            nint hDesktop,
            nint hdc, 
            ref RECT pRect,
            nint dwData);

        [DllImport("user32", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern bool GetMonitorInfo(
            nint hmonitor, 
            ref MonitorInfoEx info);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 4)]
        internal struct MonitorInfoEx
        {
            public uint cbSize;// = (uint)Marshal.SizeOf(typeof(MonitorInfoEx));
            public RECT rcMonitor;
            public RECT rcWork;
            public uint dwFlags;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public char[] szDevice;// = new char[32];
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Returns rectangle of physical display(without considering DPI).
        /// X and Y can be negative.
        /// </summary>
        /// <returns></returns>
        public static Rectangle GetPhysicalDisplayRectangle()
        {
            var left = 0;
            var right = 0;
            var top = 0;
            var bottom = 0;

            bool Callback(nint hDesktop, nint hdc, ref RECT rect, nint dwData)
            {
                var info = new MonitorInfoEx
                {
                    cbSize = (uint)Marshal.SizeOf(typeof(MonitorInfoEx)),
                };
                GetMonitorInfo(hDesktop, ref info).Check();

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
                right = Math.Max(right, (int)(x + width));
                top = Math.Min(top, y);
                bottom = Math.Max(bottom, (int)(y + height));

                return true;
            }

            EnumDisplayMonitors(0, 0, Callback, 0).Check();

            return Rectangle.FromLTRB(left, top, right, bottom);
        }

        /// <summary>
        /// Creates screenshot of all available screens. <br/>
        /// If <paramref name="cropRectangle"/> is not null, returns image of this region.
        /// </summary>
        /// <param name="cropRectangle"></param>
        /// <returns></returns>
        public static Bitmap Shot(Rectangle? cropRectangle = null)
        {
            var rectangle = (cropRectangle ?? GetPhysicalDisplayRectangle()).Normalize();
            
            var window = User32.GetDesktopWindow();
            using var dc = User32.GetWindowDC(window);
            using var toDc = Gdi32.CreateCompatibleDC(dc);
            var hBmp = Gdi32.CreateCompatibleBitmap(dc, rectangle.Width, rectangle.Height);
            var hOldBmp = Gdi32.SelectObject(toDc, hBmp);

            // ReSharper disable once BitwiseOperatorOnEnumWithoutFlags
            Gdi32.BitBlt(toDc.DangerousGetHandle(), 
                0, 
                0,
                rectangle.Width,
                rectangle.Height, 
                dc.DangerousGetHandle(),
                rectangle.X,
                rectangle.Y, 
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
        public static async Task<Bitmap> ShotAsync(
            Rectangle? rectangle = null, 
            CancellationToken cancellationToken = default)
        {
            return await Task.Run(() => Shot(rectangle), cancellationToken)
                .ConfigureAwait(false);
        }

        #endregion
    }
}
