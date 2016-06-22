using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EasyHook;
using System.Runtime.InteropServices;
using System.IO;
using System.Threading;
using System.Drawing;
//using Microsoft.DirectX;
//using Microsoft.DirectX.DirectDraw;
using System.Reflection;
using System.Security;
using SlimDX;
//using ClassLibrary1;
namespace ScreenshotInject
{

#region Com Declarations - DirectDraw
    [ComImport, Guid("15E65EC0-3B9C-11D2-B92F-00609797EA5B"),
InterfaceType(ComInterfaceType.InterfaceIsIUnknown), SuppressUnmanagedCodeSecurity]
public interface IDirectDraw7
    {
        int Compact();
        int CreateClipper();
        int CreatePalette();
        IntPtr CreateSurface(ref DDSURFACEDESC2 TDDSurfaceDesc,
            [Out] out IDirectDrawSurface lplpDDSurface,
            IntPtr pUnkOuter);
        int DuplicateSurface();
        int EnumDisplayModes();
        int EnumSurfaces();
        int FlipToGDISurface();
        int GetCaps();
        int GetDisplayMode();
        int GetFourCCCodes();
        int GetGDISurface();
        int GetMonitorFrequency();
        int GetScanLine();
        int GetVerticalBlankStatus();
        int Initialize();
        int RestoreDisplayMode();
        int SetCooperativeLevel(IntPtr hwnd, IntPtr flags);
        int SetDisplayMode(IntPtr w, IntPtr h, IntPtr bpp, IntPtr refr, IntPtr flags);
        int WaitForVerticalBlank();
        int WaitForVerticalBlank2();
        int GetSurfaceFromDC(IntPtr hdc, IDirectDrawSurface surface);
    }

[ComImport, Guid("6C14DB81-A733-11CE-A521-0020AF0BE560"),
InterfaceType(ComInterfaceType.InterfaceIsIUnknown), SuppressUnmanagedCodeSecurity]
public interface IDirectDrawSurface{
    int AddAttachedSurface();
    int AddOverlayDirtyRect();
    int Blt(IntPtr destrect, IntPtr s, IntPtr srcrect, IntPtr f, IntPtr something);
    int BltBatch();
    int BltFast();
    int DeleteAttachedSurface();
    int EnumAttachedSurfaces([Out] out IntPtr primary, [Out] out IntPtr callback);
    int EnumOverlayZOrders();
    int Flip(IntPtr a, IntPtr b);
    int GetAttachedSurface(ref DDSCAPS2 caps, [Out] out IntPtr surface);
    int GetBltStatus();
    int GetCaps();
    int GetClipper();
    int GetColorKey();
    int GetDC([Out] out IntPtr hdc);
    int GetFlipStatus();
    int GetOverlayPosition();
    int GetPalette();
    int GetPixelFormat();
    int GetSurfaceDesc();
    int Initialize();
    int IsLost();
    int Lock(IntPtr a, IntPtr b, IntPtr c, IntPtr d);
    int ReleaseDC(IntPtr hdc);
    int Restore();
    int SetClipper();
    int SetColorKey();
    int SetOverlayPosition();
    int SetPalette();
    int Unlock(IntPtr a);
    int UpdateOverlay();
    int UpdateOverlayDisplay();
    int UpdateOverlayZOrder();
}

[ComImport, Guid("6C14DB80-A733-11CE-A521-0020AF0BE560"),
InterfaceType(ComInterfaceType.InterfaceIsIUnknown), SuppressUnmanagedCodeSecurity]
public interface IDirectDraw
{
    int Compact();
    int CreateClipper();
    int CreatePalette ();
    IntPtr CreateSurface(ref DDSURFACEDESC2 TDDSurfaceDesc,
        [Out] out IDirectDrawSurface lplpDDSurface,
        IntPtr pUnkOuter);
    int DuplicateSurface ();
    int EnumDisplayModes ();
    int EnumSurfaces ();
    int FlipToGDISurface ();
    int GetCaps ();
    int GetDisplayMode ();
    int GetFourCCCodes ();
    int GetGDISurface () ;
    int GetMonitorFrequency ();
    int GetScanLine ();
    int GetVerticalBlankStatus ();
    int Initialize ();
    int RestoreDisplayMode ();
    int SetCooperativeLevel(IntPtr hwnd, int flags);
    int SetDisplayMode ();
    int WaitForVerticalBlank () ;
}

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct DDSURFACEDESC2
{
    [FieldOffset(0)]
    public int dwSize;
    [FieldOffset(4)]
    public int dwFlags;
    [FieldOffset(8)]
    public int dwHeight;
    [FieldOffset(12)]
    public int dwWidth;
    [FieldOffset(16)]
    public int lPitch;
    [FieldOffset(16)]
    public int dwLinearSize;
    [FieldOffset(20)]
    public int dwBackBufferCount;
    [FieldOffset(24)]
    public int dwMipMapCount;
    [FieldOffset(24)]
    public int dwRefreshRate;
    [FieldOffset(28)]
    public int dwAlphaBitDepth;
    [FieldOffset(32)]
    public int dwReserved;
    [FieldOffset(36)]
    public int lpSurface;
    [FieldOffset(40)]
    public DDCOLORKEY ddckCKDestOverlay;
    [FieldOffset(40)]
    public int dwEmptyFaceColor;
    [FieldOffset(48)]
    public DDCOLORKEY ddckCKDestBlt;
    [FieldOffset(56)]
    public DDCOLORKEY ddckCKSrcOverlay;
    [FieldOffset(56)]
    public DDCOLORKEY ddckCKSrcBlt;
    [FieldOffset(72)]
    public DDPIXELFORMAT ddpfPixelFormat;
    [FieldOffset(104)]
    public DDSCAPS2 ddsCaps;
    [FieldOffset(120)]
    public int dwTextureStage;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct DDCOLORKEY
{
    int dwColorSpaceLowValue;
    int dwColorSpaceHighValue; 
}

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct DDPIXELFORMAT
{
    [FieldOffset(0)]
    public int dwSize;
    [FieldOffset(4)]
    public int dwFlags;
    [FieldOffset(8)]
    public int dwFourCC;
    [FieldOffset(12)]
    public int dwRGBBitCount;
    [FieldOffset(12)]
    public int dwYUVBitCount;
    [FieldOffset(12)]
    public int dwZBufferBitDepth;
    [FieldOffset(12)]
    public int dwAlphaBitDepth;
    [FieldOffset(12)]
    public int dwLuminanceBitCount;
    [FieldOffset(12)]
    public int dwBumpBitCount;
    [FieldOffset(12)]
    public int dwPrivateFormatBitCount;
    [FieldOffset(16)]
    public int dwRBitMask;
    [FieldOffset(16)]
    public int dwYBitMask;
    [FieldOffset(16)]
    public int dwStencilBitDepth;
    [FieldOffset(16)]
    public int dwLuminanceBitMask;
    [FieldOffset(16)]
    public int dwBumpDuBitMask;
    [FieldOffset(16)]
    public int dwOperations;
    [FieldOffset(20)]
    public int dwGBitMask;
    [FieldOffset(20)]
    public int dwUBitMask;
    [FieldOffset(20)]
    public int dwZBitMask;
    [FieldOffset(20)]
    public int dwBumpDvBitMask;
    [FieldOffset(24)]
    public int dwBBitMask;
    [FieldOffset(24)]
    public int dwVBitMask;
    [FieldOffset(24)]
    public int dwStencilBitMask;
    [FieldOffset(24)]
    public int dwBumpLuminanceBitMask;
    [FieldOffset(28)]
    public int dwRGBAlphaBitMask;
    [FieldOffset(28)]
    public int dwYUVAlphaBitMask;
    [FieldOffset(28)]
    public int dwLuminanceAlphaBitMask;
    [FieldOffset(28)]
    public int dwRGBZBitMask;
    [FieldOffset(28)]
    public int dwYUVZBitMask;
}
    
[StructLayout(LayoutKind.Sequential)]
public struct DDSCAPS2
{
    public int dwCaps;
    public int dwCaps2;
    public int dwCaps3;
    public int dwCaps4;
}


[StructLayout(LayoutKind.Sequential)]
public struct BLENDFUNCTION
{
    byte BlendOp;
    byte BlendFlags;
    byte SourceConstantAlpha;
    byte AlphaFormat;

    public BLENDFUNCTION(byte op, byte flags, byte alpha, byte format)
    {
        BlendOp = op;
        BlendFlags = flags;
        SourceConstantAlpha = alpha;
        AlphaFormat = format;
    }
}

public enum TernaryRasterOperations : uint
{
    SRCCOPY = 0x00CC0020,
    SRCPAINT = 0x00EE0086,
    SRCAND = 0x008800C6,
    SRCINVERT = 0x00660046,
    SRCERASE = 0x00440328,
    NOTSRCCOPY = 0x00330008,
    NOTSRCERASE = 0x001100A6,
    MERGECOPY = 0x00C000CA,
    MERGEPAINT = 0x00BB0226,
    PATCOPY = 0x00F00021,
    PATPAINT = 0x00FB0A09,
    PATINVERT = 0x005A0049,
    DSTINVERT = 0x00550009,
    BLACKNESS = 0x00000042,
    WHITENESS = 0x00FF0062,
    CAPTUREBLT = 0x40000000 //only if WinVer >= 5.0.0 (see wingdi.h)
}
    #endregion

internal class DXHookDD: BaseDXHook
{
    const byte AC_SRC_OVER = 0x00;
    const byte AC_SRC_ALPHA = 0x01;
        //ClassLibrary1.Menu mymenu;
        int FirstCall = 0;
        IDirectDraw7 mydd;
        IDirectDrawSurface mysurface;
        IntPtr ddinterfaceIntPtr;
        IntPtr ddinterfaceIntPtr2;
        //Graphics gfx;
        Image imag; // = new Bitmap(350, 350);
        System.Drawing.Drawing2D.LinearGradientBrush brsh = new System.Drawing.Drawing2D.LinearGradientBrush(new Point(20, 20), new Point(110, 110), Color.FromArgb(80, Color.Green), Color.FromArgb(255, Color.Green));
        IntPtr faddrX; IntPtr faddrX2;

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

        [DllImport("ddraw.dll", EntryPoint = "DirectDrawCreate", CallingConvention = CallingConvention.StdCall),
SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Interface)]
        public static extern void DirectDrawCreate(IntPtr GUID, [Out, MarshalAs(UnmanagedType.Interface)] out IDirectDraw7 dd, IntPtr pUnkOuter);

        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        static extern bool DeleteDC(IntPtr hdc);

        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        static extern bool DeleteObject(IntPtr hObject);


        #endregion

        public DXHookDD(ScreenshotInterface.ScreenshotInterface ssInterface)
            : base(ssInterface) {}

        IntPtr s;
        LocalHook DirectDrawSurface_BltHook = null;
        LocalHook DirectDrawSurface_FlipHook = null;
        //LocalHook DirectDrawSurface_LockHook = null;
        //LocalHook DirectDrawSurface_UnlockHook = null;
        //LocalHook DirectDrawSurface_ReleaseHook = null;
        //LocalHook DirectDraw_SetCooperativeLevelHook = null;
        LocalHook DirectDraw_SetDisplayModeHook = null;
        LocalHook DirectDraw_RestoreDisplayModeHook = null;

        protected override string HookName
        {
            get
            {
                return "DXHookDD";
            }
        }
        
        public override void Hook()
        {
            int r;
            IntPtr ir;
            #region Test - try to find dd surface blt method addr
            Type DDinterfaceType = typeof(IDirectDrawSurface);
            Type DDinterfaceType2 = typeof(IDirectDraw7);
            DirectDrawCreate(IntPtr.Zero, out mydd, IntPtr.Zero);
            this.DebugMessage("Created directDraw object");
            r = mydd.SetCooperativeLevel(IntPtr.Zero, new IntPtr(8));
            DDSURFACEDESC2 ddesc = new DDSURFACEDESC2();
            ddesc.dwSize = 124;
            ddesc.ddsCaps.dwCaps = 64;
            ddesc.dwFlags = 7; //7 + 128 (alphabitdepth) + 4096 (pixelformat)
            ddesc.dwHeight = 1920;
            ddesc.dwWidth = 1080;
            //ddesc.dwAlphaBitDepth = 8;


            ir = mydd.CreateSurface(ref ddesc, out mysurface, IntPtr.Zero);
            ddinterfaceIntPtr = Marshal.GetComInterfaceForObject(mysurface, DDinterfaceType);
            ddinterfaceIntPtr2 = Marshal.GetComInterfaceForObject(mydd, DDinterfaceType2);
            unsafe
            {
                int* faddr; int* faddr3; int* faddr6; int* faddr7;
                int*** ddinterfaceRawPtr = (int***)ddinterfaceIntPtr.ToPointer();
                int** vTable = *ddinterfaceRawPtr;
                int*** ddinterfaceRawPtr2 = (int***)ddinterfaceIntPtr2.ToPointer();
                int** vTable2 = *ddinterfaceRawPtr2;
                MethodInfo mi = DDinterfaceType.GetMethod("Blt");
                MethodInfo mi2 = DDinterfaceType2.GetMethod("SetCooperativeLevel");
                int mi_vto = Marshal.GetComSlotForMethodInfo(mi);
                int mi_vto2 = Marshal.GetComSlotForMethodInfo(mi);
                faddr = vTable[mi_vto];
                DirectDrawSurface_BltHook = LocalHook.Create(new System.IntPtr(faddr), new DirectDrawSurface_BltDelegate(BltHook), this);
                //faddr2 = vTable[2];
                //DirectDrawSurface_ReleaseHook = LocalHook.Create(new System.IntPtr(faddr2), new DirectDrawSurface_ReleaseDelegate(ReleaseHook), this);
                faddr3 = vTable[11];
                DirectDrawSurface_FlipHook = LocalHook.Create(new System.IntPtr(faddr3), new DirectDrawSurface_FlipDelegate(FlipHook), this);
                //faddr4 = vTable[25];
                //DirectDrawSurface_LockHook = LocalHook.Create(new System.IntPtr(faddr4), new DirectDrawSurface_LockDelegate(LockHook), this);
                //faddr5 = vTable[32];
                //DirectDrawSurface_UnlockHook = LocalHook.Create(new System.IntPtr(faddr5), new DirectDrawSurface_UnlockDelegate(UnlockHook), this);
                //faddr6 = vTable2[20];
                //DirectDraw_SetCooperativeLevelHook = LocalHook.Create(new System.IntPtr(faddr6), new Delegate_SetCooperativeLevel(SetCooperativeLevel_Hooked), this);
                faddr6 = vTable2[21];
                DirectDraw_SetDisplayModeHook = LocalHook.Create(new System.IntPtr(faddr6), new Delegate_SetDisplayModeHook(SetDisplayModeHook_Hooked), this);
                faddr7 = vTable2[19];
                DirectDraw_RestoreDisplayModeHook = LocalHook.Create(new System.IntPtr(faddr7), new Delegate_RestoreDisplayModeHook(RestoreDisplayModeHook_Hooked), this);

                faddrX = new System.IntPtr(faddr6);
                faddrX2 = new System.IntPtr(faddr7);
                Marshal.Release(ddinterfaceIntPtr);
                Marshal.Release(ddinterfaceIntPtr2);
                Marshal.FinalReleaseComObject(mydd);
                Marshal.FinalReleaseComObject(mysurface);
            }
            #endregion

            
            //mysurface.GetDC(out hdc);
            //mysurface.ReleaseDC(hdc);
            /*
            System.Collections.ArrayList mm = new System.Collections.ArrayList();
            mm.Add ("qwrwqer");
            mm.Add ("hyrereyre");
            mm.Add ("rhdthdfghdfghdfh");
            mm.Add ("safxcvbxcvbvb");
            mm.Add("safxcvbxcvbvb");
            mm.Add("safxcvbxcvbvb");
            mm.Add("safxcvbxcvbvb");
            mm.Add("safxcvbxcvbvb");
            mm.Add("safxcvbxcvbvb");
            mm.Add("safxcvbxcvbvb");
            mm.Add("safxcvbxcvbvb");
            mm.Add("safxcvbxcvbvb");
            mm.Add("safxcvbxcvbvb");
            mymenu = new ClassLibrary1.Menu(new IntPtr(0), mm, 0, false, new Rectangle(1, 1, 1, 1));
            using (MemoryStream stream = new MemoryStream())
            {
                mymenu.getEntireImage().Save(stream, System.Drawing.Imaging.ImageFormat.Bmp);
                idxhookUpdateimg = stream.ToArray();
            }*/

            //gfx = Graphics.FromImage(imag);
            //gfx.FillRectangle(Brushes.Blue, 0, 0, 350, 350);

            DirectDrawSurface_BltHook.ThreadACL.SetExclusiveACL(new Int32[1]);
            DirectDrawSurface_FlipHook.ThreadACL.SetExclusiveACL(new Int32[1]);
            //DirectDraw_SetCooperativeLevelHook.ThreadACL.SetExclusiveACL(new Int32[1]); 
            //DirectDrawSurface_LockHook.ThreadACL.SetExclusiveACL(new Int32[1]);
            //DirectDrawSurface_UnlockHook.ThreadACL.SetExclusiveACL(new Int32[1]);
            //DirectDrawSurface_ReleaseHook.ThreadACL.SetExclusiveACL(new Int32[1]);
            DirectDraw_SetDisplayModeHook.ThreadACL.SetExclusiveACL(new Int32[1]);
            DirectDraw_RestoreDisplayModeHook.ThreadACL.SetExclusiveACL(new Int32[1]);
            this.DebugMessage("Hook: End" + DateTime.Now.ToString() + ":" + DateTime.Now.Millisecond.ToString());
        }



        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        delegate int DirectDrawSurface_BltDelegate(IntPtr a, IntPtr b, IntPtr c, IntPtr d, IntPtr e, IntPtr f);
        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        delegate int DirectDrawSurface_FlipDelegate(IntPtr a, IntPtr b, IntPtr c);
        //[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        //delegate int DirectDrawSurface_LockDelegate(IntPtr a, IntPtr b, IntPtr c, IntPtr d, IntPtr e);
        //[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        //delegate int DirectDrawSurface_UnlockDelegate(IntPtr a, IntPtr b);
        //[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        //delegate int DirectDrawSurface_ReleaseDelegate(IntPtr a);
        //[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        //delegate int Delegate_SetCooperativeLevel(IntPtr a, IntPtr b, IntPtr c);
        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        delegate int Delegate_SetDisplayModeHook(IntPtr a, IntPtr w, IntPtr h, IntPtr bpp, IntPtr refr, IntPtr flags);
        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        delegate int Delegate_RestoreDisplayModeHook(IntPtr a);

        int SetDisplayModeHook_Hooked(IntPtr a, IntPtr b, IntPtr c, IntPtr d, IntPtr e, IntPtr f)
        {
            FirstCall = 1;
            try
            {
                this.DebugMessage("SetDisplayMode");
                Delegate_SetDisplayModeHook del = (Delegate_SetDisplayModeHook)
                Marshal.GetDelegateForFunctionPointer(faddrX, typeof(Delegate_SetDisplayModeHook));
                del(a, b, c, d, e, f);
            }
            catch (Exception ex)
            {
                this.DebugMessage(DateTime.Now.ToString() + ":" + DateTime.Now.Millisecond.ToString() + "SCL err: " + ex.Message);
            }
            return 0;
        }

        int RestoreDisplayModeHook_Hooked(IntPtr a)
        {
            FirstCall = 1;
            try
            {
                this.DebugMessage("RestoreDisplayMode");
                Delegate_RestoreDisplayModeHook del = (Delegate_RestoreDisplayModeHook)
                Marshal.GetDelegateForFunctionPointer(faddrX2, typeof(Delegate_RestoreDisplayModeHook));
                del(a);
            }
            catch (Exception ex)
            {
                this.DebugMessage(DateTime.Now.ToString() + ":" + DateTime.Now.Millisecond.ToString() + "SCL err: " + ex.Message);
            }
            return 0;
        }

        int LockHook(IntPtr a, IntPtr b, IntPtr c, IntPtr d, IntPtr e)
        {
            int res;
            IDirectDrawSurface src;
            src = (IDirectDrawSurface)Marshal.GetObjectForIUnknown(a);
            s = a;

            src.Lock(b, c, d, e);
            res = Marshal.ReleaseComObject(src);
            return 0;
        }

        int UnlockHook(IntPtr a, IntPtr b)
        {
            this.DebugMessage(DateTime.Now.ToString() + "dst blt opacity err: ");
            int res;
            IDirectDrawSurface src;
            src = (IDirectDrawSurface)Marshal.GetObjectForIUnknown(a);

            IntPtr hdc;
            res = src.GetDC(out hdc);
            Graphics g = Graphics.FromImage(imag);
            g.FillRectangle(Brushes.Blue, 0, 0, 600, 500);
            Bitmap bmp = new Bitmap(imag);
            IntPtr pSource = CreateCompatibleDC(g.GetHdc());
            IntPtr pOrig = SelectObject(pSource, bmp.GetHbitmap());
            BitBlt(hdc, 0, 0, imag.Width, imag.Height, pSource, 0, 0, TernaryRasterOperations.SRCCOPY);
            g.ReleaseHdc();
            IntPtr pNew = SelectObject(pSource, pOrig);
            DeleteObject(pNew);
            DeleteDC(pSource);
            src.ReleaseDC(hdc);



            src.Unlock (b);
            res = Marshal.ReleaseComObject(src);
            return 0;
        }

        int FlipHook(IntPtr a, IntPtr surf, IntPtr flags)
        {
            if (FirstCall == 1) { FirstCall = 0; this.DebugMessage("FirstCall Flip"); }
            try
            {
                int res;
                IDirectDrawSurface src;
                IDirectDrawSurface bckbf;
                IntPtr bckbuf_pointer;
                src = (IDirectDrawSurface)Marshal.GetObjectForIUnknown(a);

                DDSCAPS2 ddc = new DDSCAPS2();
                ddc.dwCaps = 4;
                src.GetAttachedSurface(ddc, out bckbuf_pointer);
                bckbf = (IDirectDrawSurface)Marshal.GetObjectForIUnknown(bckbuf_pointer);

                if (idxhookUpdateimg != null)
                {
                    imag = Image.FromStream(new MemoryStream(idxhookUpdateimg));
                    if (imag.Height == 1) {imag = null;}
                    idxhookUpdateimg = null;
                    this.DebugMessage("HOOKED");
                }

                if (imag != null)
                {
                    IntPtr hdc;
                    res = bckbf.GetDC(out hdc);
                    Graphics g = Graphics.FromImage(imag);
                    Bitmap bmp = new Bitmap(imag);
                    IntPtr pSource = CreateCompatibleDC(g.GetHdc());
                    IntPtr pOrig = SelectObject(pSource, bmp.GetHbitmap());
                    BitBlt(hdc, 0, 0, imag.Width, imag.Height, pSource, 0, 0, TernaryRasterOperations.SRCCOPY);
                    IntPtr pNew = SelectObject(pSource, pOrig);
                    DeleteObject(pNew);
                    DeleteDC(pSource);
                    bckbf.ReleaseDC(hdc);
                }
                src.Flip(surf, flags);
                res = Marshal.ReleaseComObject(src);
                res = Marshal.ReleaseComObject(bckbf);
            }
            catch (Exception e)
            {
                this.DebugMessage(DateTime.Now.ToString() + ":" + DateTime.Now.Millisecond.ToString() + "Flip err: " + e.Message);
            }
            return 0;
        }

        int ReleaseHook(IntPtr a)
        {
            //DirectDrawSurface_BltHook.ThreadACL.SetExclusiveACL(new Int32[0]);
            Marshal.Release(a);
            return 0;
        }

        int SetCooperativeLevel_Hooked(IntPtr a, IntPtr b, IntPtr c)
        {
            int res;
            try
            {
                IDirectDraw7 id7;
                id7 = (IDirectDraw7)Marshal.GetObjectForIUnknown(a);
                this.DebugMessage("SetCooperativeLevel");
                res = id7.SetCooperativeLevel(b, c);
                Marshal.ReleaseComObject(id7);
            }
            catch (Exception e)
            {
                res = 0;
                this.DebugMessage(DateTime.Now.ToString() + ":" + DateTime.Now.Millisecond.ToString() + "SCL err: " + e.Message);
            }
            return res;
        }

        int BltHook(IntPtr dsur, IntPtr bptr, IntPtr ssur, IntPtr dptr, IntPtr dflags, IntPtr hren)
        {
            if (FirstCall == 1) { FirstCall = 0; this.DebugMessage("FirstCall Blt"); }
            try
            {
                int res;
                IDirectDrawSurface src;
                IDirectDrawSurface dst;
                src = (IDirectDrawSurface)Marshal.GetObjectForIUnknown(ssur);
                dst = (IDirectDrawSurface)Marshal.GetObjectForIUnknown(dsur);

                if (idxhookUpdateimg != null)
                {
                    imag = Image.FromStream(new MemoryStream(idxhookUpdateimg));
                    if (imag.Height == 1){imag = null;}
                    idxhookUpdateimg = null;
                    this.DebugMessage("HOOKED");
                }

                if (imag != null)
                {
                    if (imag.Height == 2) {res = Marshal.ReleaseComObject(src); res = Marshal.ReleaseComObject(dst); return 0;}

                    IntPtr hdc;
                    res = src.GetDC(out hdc);

                    Graphics g = Graphics.FromImage(imag);
                    //g.FillRectangle(Brushes.Blue, 0, 0, 600, 500);
                    Bitmap bmp = new Bitmap(imag);
                    IntPtr pSource = CreateCompatibleDC(g.GetHdc());
                    IntPtr pOrig = SelectObject(pSource, bmp.GetHbitmap());

                    BitBlt(hdc, 0, 0, imag.Width, imag.Height, pSource, 0, 0, TernaryRasterOperations.SRCCOPY);
                    //g.ReleaseHdc();
                    IntPtr pNew = SelectObject(pSource, pOrig);
                    DeleteObject(pNew);
                    DeleteDC(pSource);
                    src.ReleaseDC(hdc);
                }

                //gfx.FillEllipse(brsh, 30, 30, 30, 50);

                //Image i = new Bitmap(500, 500);
                //Graphics g = Graphics.FromImage(i);
                //Image i1 = new Bitmap(500, 500,System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
                //Graphics g1 = Graphics.FromImage(i1);
                //g1.FillRectangle(Brushes.Blue, 0, 0, 500, 500);
                //BitBlt(g.GetHdc(), 10, 10, 150, 150, gfx.GetHdc(), 1, 1, TernaryRasterOperations.SRCCOPY);
                //g.ReleaseHdc();

                //g = Graphics.FromImage(i);
                //BLENDFUNCTION bf = new BLENDFUNCTION(0,0,255,0);
                //AlphaBlend(g.GetHdc(), 1, 1, 150, 150, g1.GetHdc(), 1, 1, 150, 150, bf);
                //g1.ReleaseHdc(); 

                
                //gfx = Graphics.FromHdc(hdc);
                //gfx.DrawImage(i, new Point (10,10));

                res = dst.Blt(bptr, ssur, dptr, dflags, hren);
                if (res != 0) { this.DebugMessage(DateTime.Now.ToString() + "dst blt opacity err: " + res.ToString()); }
                //dst.Blt(bptr, ddinterfaceIntPtr, dptr, dflags, hren);
                //if (res != 0) { this.DebugMessage(DateTime.Now.ToString() + "dst blt final err: " + res.ToString()); }
                res = Marshal.ReleaseComObject(src);
                res = Marshal.ReleaseComObject(dst);
            }
            catch (Exception e)
            {
                this.DebugMessage(DateTime.Now.ToString() + ":" + DateTime.Now.Millisecond.ToString() + "err: " + e.Message);
            }
            return 0;
        }
 
    public override void Cleanup()
        {
        }
    }
}
