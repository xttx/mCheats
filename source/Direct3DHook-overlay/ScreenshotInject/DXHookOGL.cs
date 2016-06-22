using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EasyHook;
using System.Runtime.InteropServices;
using System.IO;
using System.Threading;
using System.Drawing;
using System.Reflection;
using System.Security;

namespace ScreenshotInject
{

internal class DXHookOGL: BaseDXHook
{
        //Graphics gfx;
        Image imag; // = new Bitmap(350, 350);
        System.Drawing.Drawing2D.LinearGradientBrush brsh = new System.Drawing.Drawing2D.LinearGradientBrush(new Point(20, 20), new Point(110, 110), Color.FromArgb(80, Color.Green), Color.FromArgb(255, Color.Green));

        #region dllimport
        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool BitBlt(IntPtr hdc, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, TernaryRasterOperations dwRop);

        [DllImport("gdi32.dll", SetLastError = true, EntryPoint = "GdiAlphaBlend")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool AlphaBlend([In] IntPtr hdcDest,
            int nXOriginDest, int nYOriginDest,
            int nWidthDest, int nHeightDest,
            [In] IntPtr hdcSrc,
            int nXOriginSrc, int nYOriginSrc, int nWidthSrc, int nHeightSrc,
            BLENDFUNCTION blendFunction);

        [DllImport("gdi32.dll", SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 SwapBuffers(IntPtr hdc);

        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        static extern bool DeleteDC(IntPtr hdc);

        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        static extern bool DeleteObject(IntPtr hObject);
        #endregion

        public DXHookOGL(ScreenshotInterface.ScreenshotInterface ssInterface)
            : base(ssInterface) {}

        LocalHook SwapBuffers_Hook = null;

        protected override string HookName
        {
            get
            {
                return "DXHookOGL";
            }
        }
        
        public override void Hook()
        {
            SwapBuffers_Hook = LocalHook.Create (LocalHook.GetProcAddress ("gdi32.dll","SwapBuffers"), new Delegate_SwapBuffers(SwapBuffers_Hooked),this);
                        
            //gfx = Graphics.FromImage(imag);
            //gfx.FillRectangle(Brushes.Blue, 0, 0, 350, 350);

            SwapBuffers_Hook.ThreadACL.SetExclusiveACL(new Int32[1]);
            this.DebugMessage("Hook: Done" + DateTime.Now.ToString() + ":" + DateTime.Now.Millisecond.ToString());
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        delegate int Delegate_SwapBuffers(IntPtr a);

        int SwapBuffers_Hooked(IntPtr a)
        {
            try
            {
                if (idxhookUpdateimg != null)
                {
                    imag = Image.FromStream(new MemoryStream(idxhookUpdateimg));
                    if (imag.Height == 1) { imag = null; this.DebugMessage("IMAGE NULLED"); }
                    idxhookUpdateimg = null;
                    this.DebugMessage("HOOKED");
                }

                if (imag != null)
                {
                    Graphics g = Graphics.FromImage(imag);
                    Bitmap bmp = new Bitmap(imag);
                    IntPtr pSource = CreateCompatibleDC(g.GetHdc());
                    IntPtr pOrig = SelectObject(pSource, bmp.GetHbitmap());
                    BitBlt(a, 0, 0, imag.Width, imag.Height, pSource, 0, 0, TernaryRasterOperations.SRCCOPY);
                    //g.ReleaseHdc();
                    IntPtr pNew = SelectObject(pSource, pOrig);
                    DeleteObject(pNew);
                    DeleteDC(pSource);
                }
                else
                {
                    SwapBuffers(a);
                }
            }
            catch (Exception e)
            {
                this.DebugMessage(DateTime.Now.ToString() + ":" + DateTime.Now.Millisecond.ToString() + "err: " + e.Message);
            }
            return 1;
        }

    public override void Cleanup()
        {
        }
    }
}
