using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX.Direct3D9;
using EasyHook;
using System.Runtime.InteropServices;
using System.IO;
using System.Threading;
using System.Drawing;
using Microsoft.DirectX;
//using Microsoft.DirectX.DirectDraw;
using System.Reflection;
using System.Security;

namespace ScreenshotInject
{
 
    #region Com declare
    [StructLayout(LayoutKind.Sequential)]
    public class RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }

    /// <summary>
    /// CLSID_IDirect3DDevice9
    /// </summary>
    [ComImport, Guid("D0223B96-BF7A-43fd-92BD-A43B0D82B9EB"),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown), SuppressUnmanagedCodeSecurity]
    public interface IDirect3DDevice9
    {
        [PreserveSig, SuppressUnmanagedCodeSecurity]
        int TestCooperativeLevel();
        [PreserveSig, SuppressUnmanagedCodeSecurity]
        uint GetAvailableTextureMem();
        [PreserveSig, SuppressUnmanagedCodeSecurity]
        int EvictManagedResources();
        [PreserveSig, SuppressUnmanagedCodeSecurity]
        int GetDirect3D([Out] out IDirect3D9 ppD3D9);
        [PreserveSig, SuppressUnmanagedCodeSecurity]
        int GetDeviceCaps([In, Out] ref D3DCAPS9 pCaps);
        [PreserveSig, SuppressUnmanagedCodeSecurity]
        int GetDisplayMode(uint iSwapChain, D3DDISPLAYMODE pMode);
        [PreserveSig, SuppressUnmanagedCodeSecurity]
        int GetCreationParameters([In, Out] ref DDSURFACEDESC pParameters);
        int SetCursorProperties();
        int SetCursorPosition();
        int ShowCursor(bool bShow);
        int CreateAdditionalSwapChain();
        int GetSwapChain();
        uint GetNumberOfSwapChains();
        [PreserveSig, SuppressUnmanagedCodeSecurity]
        int Reset([In, Out] ref D3DPRESENT_PARAMETERS2 pPresentationParameters);
        int Present();
        int GetBackBuffer();
        int GetRasterStatus();
        int SetDialogBoxMode();
        int SetGammaRamp();
        int GetGammaRamp();
        [PreserveSig, SuppressUnmanagedCodeSecurity]
        int CreateTexture(int Width, int Height, int Levels, int Usage, D3DFORMAT Format, int Pool,
                          out IDirect3DTexture9 ppTexture, IntPtr pSharedHandle);

        int CreateVolumeTexture();
        int CreateCubeTexture();
        int CreateVertexBuffer();
        int CreateIndexBuffer();
        [PreserveSig, SuppressUnmanagedCodeSecurity]
        int CreateRenderTarget(int width, int height, D3DFORMAT Format, D3DMULTISAMPLE_TYPE MultiSample,
                                 uint MultisampleQuality, [MarshalAs(UnmanagedType.Bool)] bool Lockable, [Out]out IntPtr pSurface,
                                 IntPtr pSharedSurface);
        int CreateDepthStencilSurface();
        int UpdateSurface();
        int UpdateTexture();
        int GetRenderTargetData();
        int GetFrontBufferData();
        [PreserveSig, SuppressUnmanagedCodeSecurity]
        int StretchRect();
        int ColorFill();
        int CreateOffscreenPlainSurface();
        int SetRenderTarget();
        [PreserveSig, SuppressUnmanagedCodeSecurity]
        int GetRenderTarget([Out]out IntPtr pSurface);
        int SetDepthStencilSurface();
        int GetDepthStencilSurface();
        int BeginScene();
        int EndScene();
        int Clear();
        int SetTransform();
        int GetTransform();
        int MultiplyTransform();
        int SetViewport();
        int GetViewport();
        int SetMaterial();
        int GetMaterial();
        int SetLight();
        int GetLight();
        int LightEnable();
        int GetLightEnable();
        int SetClipPlane();
        int GetClipPlane();
        int SetRenderState();
        int GetRenderState();
        int CreateStateBlock();
        int BeginStateBlock();
        int EndStateBlock();
        int SetClipStatus();
        int GetClipStatus();
        int GetTexture();
        int SetTexture();
        int GetTextureStageState();
        int SetTextureStageState();
        int GetSamplerState();
        int SetSamplerState();
        int ValidateDevice();
        int SetPaletteEntries();
        int GetPaletteEntries();
        int SetCurrentTexturePalette();
        int GetCurrentTexturePalette();
        int SetScissorRect();
        int GetScissorRect();
        int SetSoftwareVertexProcessing(bool bSoftware);
        bool GetSoftwareVertexProcessing();
        int SetNPatchMode(float nSegments);
        float GetNPatchMode();
        int DrawPrimitive();
        int DrawIndexedPrimitive();
        int DrawPrimitiveUP();
        int DrawIndexedPrimitiveUP();
        int ProcessVertices();
        int CreateVertexDeclaration();
        int SetVertexDeclaration();
        int GetVertexDeclaration();
        int SetFVF();
        int GetFVF();
        int CreateVertexShader();
        int SetVertexShader();
        int GetVertexShader();
        int SetVertexShaderConstantF();
        int GetVertexShaderConstantF();
        int SetVertexShaderConstantI();
        int GetVertexShaderConstantI();
        int SetVertexShaderConstantB();
        int GetVertexShaderConstantB();
        int SetStreamSource();
        int GetStreamSource();
        int SetStreamSourceFreq();
        int GetStreamSourceFreq();
        int SetIndices();
        int GetIndices();
        int CreatePixelShader();
        int SetPixelShader();
        int GetPixelShader();
        int SetPixelShaderConstantF();
        int GetPixelShaderConstantF();
        int SetPixelShaderConstantI();
        int GetPixelShaderConstantI();
        int SetPixelShaderConstantB();
        int GetPixelShaderConstantB();
        int DrawRectPatch();
        int DrawTriPatch();
        int DeletePatch(uint Handle);
        int CreateQuery();
    }

    [ComImport, Guid("B18B10CE-2649-405a-870F-95F777D4313A"),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown), SuppressUnmanagedCodeSecurity]
    public interface IDirect3DDevice9Ex : IDirect3DDevice9
    {
        [PreserveSig, SuppressUnmanagedCodeSecurity]
        new int TestCooperativeLevel();
        [PreserveSig, SuppressUnmanagedCodeSecurity]
        new int GetAvailableTextureMem();
        [PreserveSig, SuppressUnmanagedCodeSecurity]
        new int EvictManagedResources();
        [PreserveSig, SuppressUnmanagedCodeSecurity]
        new int GetDirect3D([Out] out IDirect3D9 ppD3D9);
        [PreserveSig, SuppressUnmanagedCodeSecurity]
        new int GetDeviceCaps([In, Out] ref D3DCAPS9 pCaps);
        [PreserveSig, SuppressUnmanagedCodeSecurity]
        new int GetDisplayMode(uint iSwapChain, D3DDISPLAYMODE pMode);
        [PreserveSig, SuppressUnmanagedCodeSecurity]
        new int GetCreationParameters([In, Out] ref DDSURFACEDESC pParameters);
        new int SetCursorProperties();
        new int SetCursorPosition();
        new int ShowCursor(bool bShow);
        new int CreateAdditionalSwapChain();
        new int GetSwapChain();
        new int GetNumberOfSwapChains();
        [PreserveSig, SuppressUnmanagedCodeSecurity]
        new int Reset([In, Out] ref D3DPRESENT_PARAMETERS2 pPresentationParameters);
        new int Present();
        new int GetBackBuffer();
        new int GetRasterStatus();
        new int SetDialogBoxMode();
        new int SetGammaRamp();
        new int GetGammaRamp();
        [PreserveSig, SuppressUnmanagedCodeSecurity]
        new int CreateTexture(int Width, int Height, int Levels, int Usage, D3DFORMAT Format, int Pool,
                          out IDirect3DTexture9 ppTexture, IntPtr pSharedHandle);
        new int CreateVolumeTexture();
        new int CreateCubeTexture();
        new int CreateVertexBuffer();
        new int CreateIndexBuffer();
        [PreserveSig, SuppressUnmanagedCodeSecurity]
        new int CreateRenderTarget(int width, int height, D3DFORMAT Format, D3DMULTISAMPLE_TYPE MultiSample,
                                 uint MultisampleQuality, [MarshalAs(UnmanagedType.Bool)] bool Lockable, [Out]out IntPtr pSurface,

                                 IntPtr pSharedSurface);
        new int CreateDepthStencilSurface();
        new int UpdateSurface();
        new int UpdateTexture();
        new int GetRenderTargetData();
        new int GetFrontBufferData();
        new int StretchRect();
        new int ColorFill();
        new int CreateOffscreenPlainSurface();
        new int SetRenderTarget();
        [PreserveSig, SuppressUnmanagedCodeSecurity]
        new int GetRenderTarget([Out]out IntPtr pSurface);
        new int SetDepthStencilSurface();
        new int GetDepthStencilSurface();
        new int BeginScene();
        new int EndScene();
        new int Clear();
        new int SetTransform();
        new int GetTransform();
        new int MultiplyTransform();
        new int SetViewport();
        new int GetViewport();
        new int SetMaterial();
        new int GetMaterial();
        new int SetLight();
        new int GetLight();
        new int LightEnable();
        new int GetLightEnable();
        new int SetClipPlane();
        new int GetClipPlane();
        new int SetRenderState();
        new int GetRenderState();
        new int CreateStateBlock();
        new int BeginStateBlock();
        new int EndStateBlock();
        new int SetClipStatus();
        new int GetClipStatus();
        new int GetTexture();
        new int SetTexture();
        new int GetTextureStageState();
        new int SetTextureStageState();
        new int GetSamplerState();
        new int SetSamplerState();
        new int ValidateDevice();
        new int SetPaletteEntries();
        new int GetPaletteEntries();
        new int SetCurrentTexturePalette();
        new int GetCurrentTexturePalette();
        new int SetScissorRect();
        new int GetScissorRect();
        new int SetSoftwareVertexProcessing(bool bSoftware);
        new bool GetSoftwareVertexProcessing();
        new int SetNPatchMode(float nSegments);
        new float GetNPatchMode();
        new int DrawPrimitive();
        new int DrawIndexedPrimitive();
        new int DrawPrimitiveUP();
        new int DrawIndexedPrimitiveUP();
        new int ProcessVertices();
        new int CreateVertexDeclaration();
        new int SetVertexDeclaration();
        new int GetVertexDeclaration();
        new int SetFVF();
        new int GetFVF();
        new int CreateVertexShader();
        new int SetVertexShader();
        new int GetVertexShader();
        new int SetVertexShaderConstantF();
        new int GetVertexShaderConstantF();
        new int SetVertexShaderConstantI();
        new int GetVertexShaderConstantI();
        new int SetVertexShaderConstantB();
        new int GetVertexShaderConstantB();
        new int SetStreamSource();
        new int GetStreamSource();
        new int SetStreamSourceFreq();
        new int GetStreamSourceFreq();
        new int SetIndices();
        new int GetIndices();
        new int CreatePixelShader();
        new int SetPixelShader();
        new int GetPixelShader();
        new int SetPixelShaderConstantF();
        new int GetPixelShaderConstantF();
        new int SetPixelShaderConstantI();
        new int GetPixelShaderConstantI();
        new int SetPixelShaderConstantB();
        new int GetPixelShaderConstantB();
        new int DrawRectPatch();
        new int DrawTriPatch();
        new int DeletePatch(uint Handle);
        new int CreateQuery();
        [PreserveSig, SuppressUnmanagedCodeSecurity]
        int SetConvolutionMonoKernel(int width, int height, IntPtr rows, IntPtr columns);
        int ComposeRects();
        int PresentEx();
        int GetGPUThreadPriority();
        int SetGPUThreadPriority();
        int WaitForVBlank();
        int CheckResourceResidency();
        int SetMaximumFrameLatency();
        int GetMaximumFrameLatency();
        [PreserveSig, SuppressUnmanagedCodeSecurity]
        int CheckDeviceState(IntPtr hWnd);
        [PreserveSig, SuppressUnmanagedCodeSecurity]
        int CreateRenderTargetEx(int width, int height, D3DFORMAT Format, D3DMULTISAMPLE_TYPE MultiSample,
                                 uint MultisampleQuality, [MarshalAs(UnmanagedType.Bool)] bool Lockable, [Out]out IntPtr pSurface,
                                 [In, Out]ref IntPtr pSharedSurface, uint Usage);
        /*
         STDMETHOD(SetConvolutionMonoKernel)(THIS_ UINT width,UINT height,float* rows,float* columns) PURE;
    STDMETHOD(ComposeRects)(THIS_ IDirect3DSurface9* pSrc,IDirect3DSurface9* pDst,IDirect3DVertexBuffer9* pSrcRectDescs,UINT NumRects,IDirect3DVertexBuffer9* pDstRectDescs,D3DCOMPOSERECTSOP Operation,int Xoffset,int Yoffset) PURE;
    STDMETHOD(PresentEx)(THIS_ CONST RECT* pSourceRect,CONST RECT* pDestRect,HWND hDestWindowOverride,CONST RGNDATA* pDirtyRegion,int dwFlags) PURE;
    STDMETHOD(GetGPUThreadPriority)(THIS_ INT* pPriority) PURE;
    STDMETHOD(SetGPUThreadPriority)(THIS_ INT Priority) PURE;
    STDMETHOD(WaitForVBlank)(THIS_ UINT iSwapChain) PURE;
    STDMETHOD(CheckResourceResidency)(THIS_ IDirect3DResource9** pResourceArray,UINT32 NumResources) PURE;
    STDMETHOD(SetMaximumFrameLatency)(THIS_ UINT MaxLatency) PURE;
    STDMETHOD(GetMaximumFrameLatency)(THIS_ UINT* pMaxLatency) PURE;
    STDMETHOD(CheckDeviceState)(THIS_ HWND hDestinationWindow) PURE;
    STDMETHOD(CreateRenderTargetEx)(THIS_ UINT Width,UINT Height,D3DFORMAT Format,D3DMULTISAMPLE_TYPE MultiSample,int MultisampleQuality,BOOL Lockable,IDirect3DSurface9** ppSurface,HANDLE* pSharedHandle,int Usage) PURE;
    STDMETHOD(CreateOffscreenPlainSurfaceEx)(THIS_ UINT Width,UINT Height,D3DFORMAT Format,D3DPOOL Pool,IDirect3DSurface9** ppSurface,HANDLE* pSharedHandle,int Usage) PURE;
    STDMETHOD(CreateDepthStencilSurfaceEx)(THIS_ UINT Width,UINT Height,D3DFORMAT Format,D3DMULTISAMPLE_TYPE MultiSample,int MultisampleQuality,BOOL Discard,IDirect3DSurface9** ppSurface,HANDLE* pSharedHandle,int Usage) PURE;
    STDMETHOD(ResetEx)(THIS_ D3DPRESENT_PARAMETERS* pPresentationParameters,D3DDISPLAYMODEEX *pFullscreenDisplayMode) PURE;
    STDMETHOD(GetDisplayModeEx)(THIS_ UINT iSwapChain,D3DDISPLAYMODEEX* pMode,D3DDISPLAYROTATION* pRotation) PURE; 
         */
    }

    [ComImport, SuppressUnmanagedCodeSecurity,
    Guid("0CFBAF3A-9FF6-429a-99B3-A2796AF8B89B"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown), SuppressUnmanagedCodeSecurity]
    public interface IDirect3DSurface9
    {
        [PreserveSig, SuppressUnmanagedCodeSecurity]
        void GetDevice(out IDirect3DDevice9 ppDevice);
        [PreserveSig, SuppressUnmanagedCodeSecurity]
        void SetPrivateData(Guid refguid, IntPtr pData, int SizeOfData, int Flags);
        [PreserveSig, SuppressUnmanagedCodeSecurity]
        void GetPrivateData(Guid refguid, IntPtr pData, out int pSizeOfData);
        [PreserveSig, SuppressUnmanagedCodeSecurity]
        void FreePrivateData(Guid refguid);
        [PreserveSig, SuppressUnmanagedCodeSecurity]
        int SetPriority(int PriorityNew);
        [PreserveSig, SuppressUnmanagedCodeSecurity]
        int GetPriority();
        void PreLoad();
        [PreserveSig, SuppressUnmanagedCodeSecurity]
        D3DRESOURCETYPE GetType();
        void GetContainer(Guid riid, out object ppContainer);
        void GetDesc(out D3DSURFACE_DESC pDesc);
        void LockRect(D3DLOCKED_RECT pLockedRect, Rectangle pRect, int Flags);
        void UnlockRect();
        [PreserveSig, SuppressUnmanagedCodeSecurity]
        int GetDC(out IntPtr phdc);
        [PreserveSig, SuppressUnmanagedCodeSecurity]
        int ReleaseDC(IntPtr hdc);
    }

    public enum D3DRESOURCETYPE
    {
        D3DRTYPE_SURFACE = 1,
        D3DRTYPE_VOLUME = 2,
        D3DRTYPE_TEXTURE = 3,
        D3DRTYPE_VOLUMETEXTURE = 4,
        D3DRTYPE_CUBETEXTURE = 5,
        D3DRTYPE_VERTEXBUFFER = 6,
        D3DRTYPE_INDEXBUFFER = 7,           //if this changes, change _D3DDEVINFO_RESOURCEMANAGER definition


        D3DRTYPE_FORCE_int = 0x7fffffff
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct D3DLOCKED_RECT
    {
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct D3DSURFACE_DESC
    {
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DDSURFACEDESC
    {
        uint AdapterOrdinal;
        D3DDEVTYPE DeviceType;
        IntPtr hFocusWindow;
        int BehaviorFlags;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct D3DCAPS9
    {
        /* Device Info */
        public D3DDEVTYPE DeviceType;
        public uint AdapterOrdinal;

        /* Caps from DX7 Draw */
        public int Caps;
        public int Caps2;
        public int Caps3;
        public int PresentationIntervals;

        /* Cursor Caps */
        public int CursorCaps;

        /* 3D Device Caps */
        public int DevCaps;

        public int PrimitiveMiscCaps;
        public int RasterCaps;
        public int ZCmpCaps;
        public int SrcBlendCaps;
        public int DestBlendCaps;
        public int AlphaCmpCaps;
        public int ShadeCaps;
        public int TextureCaps;
        public int TextureFilterCaps;          // D3DPTFILTERCAPS for IDirect3DTexture9's
        public int CubeTextureFilterCaps;      // D3DPTFILTERCAPS for IDirect3DCubeTexture9's
        public int VolumeTextureFilterCaps;    // D3DPTFILTERCAPS for IDirect3DVolumeTexture9's
        public int TextureAddressCaps;         // D3DPTADDRESSCAPS for IDirect3DTexture9's
        public int VolumeTextureAddressCaps;   // D3DPTADDRESSCAPS for IDirect3DVolumeTexture9's

        public int LineCaps;                   // D3DLINECAPS

        public int MaxTextureWidth, MaxTextureHeight;
        public int MaxVolumeExtent;

        public int MaxTextureRepeat;
        public int MaxTextureAspectRatio;
        public int MaxAnisotropy;
        float MaxVertexW;

        float GuardBandLeft;
        float GuardBandTop;
        float GuardBandRight;
        float GuardBandBottom;

        float ExtentsAdjust;
        public int StencilCaps;

        public int FVFCaps;
        public int TextureOpCaps;
        public int MaxTextureBlendStages;
        public int MaxSimultaneousTextures;

        public int VertexProcessingCaps;
        public int MaxActiveLights;
        public int MaxUserClipPlanes;
        public int MaxVertexBlendMatrices;
        public int MaxVertexBlendMatrixIndex;

        float MaxPointSize;

        public int MaxPrimitiveCount;          // max number of primitives per DrawPrimitive call
        public int MaxVertexIndex;
        public int MaxStreams;
        public int MaxStreamStride;            // max stride for SetStreamSource

        public int VertexShaderVersion;
        public int MaxVertexShaderConst;       // number of vertex shader constant registers

        public int PixelShaderVersion;
        float PixelShader1xMaxValue;      // max value storable in registers of ps.1.x shaders

        // Here are the DX9 specific ones
        public int DevCaps2;

        float MaxNpatchTessellationLevel;
        public int Reserved5;

        public uint MasterAdapterOrdinal;       // ordinal of master adaptor for adapter group
        public uint AdapterOrdinalInGroup;      // ordinal inside the adapter group
        public uint NumberOfAdaptersInGroup;    // number of adapters in this adapter group (only if master)
        public int DeclTypes;                  // Data types, supported in vertex declarations
        public int NumSimultaneousRTs;         // Will be at least 1
        public int StretchRectFilterCaps;      // Filter caps supported by StretchRect
        D3DVSHADERCAPS2_0 VS20Caps;
        D3DPSHADERCAPS2_0 PS20Caps;
        public int VertexTextureFilterCaps;    // D3DPTFILTERCAPS for IDirect3DTexture9's for texture, used in vertex shaders
        public int MaxVShaderInstructionsExecuted; // maximum number of vertex shader instructions that can be executed
        public int MaxPShaderInstructionsExecuted; // maximum number of pixel shader instructions that can be executed
        public int MaxVertexShader30InstructionSlots;
        public int MaxPixelShader30InstructionSlots;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct D3DVSHADERCAPS2_0
    {
        int Caps;
        int DynamicFlowControlDepth;
        int NumTemps;
        int StaticFlowControlDepth;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct D3DPSHADERCAPS2_0
    {
        int Caps;
        int DynamicFlowControlDepth;
        int NumTemps;
        int StaticFlowControlDepth;
        int NumInstructionSlots;
    }

    public enum D3DSCANLINEORDERING
    {
        D3DSCANLINEORDERING_UNKNOWN = 0,
        D3DSCANLINEORDERING_PROGRESSIVE = 1,
        D3DSCANLINEORDERING_INTERLACED = 2,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct D3DDISPLAYMODEEX
    {
        public uint Size;
        public uint Width;
        public uint Height;
        public uint RefreshRate;
        public D3DFORMAT Format;
        public D3DSCANLINEORDERING ScanLineOrdering;
    }

    [ComImport, SuppressUnmanagedCodeSecurity,
    Guid("85C31227-3DE5-4f00-9B3A-F11AC38C18B5"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown), SuppressUnmanagedCodeSecurity]
    public interface IDirect3DTexture9
    {
        [PreserveSig, SuppressUnmanagedCodeSecurity]
        void GetDevice();
        [PreserveSig, SuppressUnmanagedCodeSecurity]
        void SetPrivateData(Guid refguid, IntPtr pData, int SizeOfData, int Flags);
        [PreserveSig, SuppressUnmanagedCodeSecurity]
        void GetPrivateData(Guid refguid, IntPtr pData, IntPtr pSizeOfData);
        [PreserveSig, SuppressUnmanagedCodeSecurity]
        void FreePrivateData(Guid refguid);
        [PreserveSig, SuppressUnmanagedCodeSecurity]
        void SetPriority(int PriorityNew);
        void GetPriority();
        void PreLoad();
        void GetType();
        void SetLOD(int LODNew);
        void GetLOD();
        void GetLevelCount();
        void SetAutoGenFilterType(int FilterType);
        int GetAutoGenFilterType();
        void GenerateMipSubLevels();
        [PreserveSig, SuppressUnmanagedCodeSecurity]
        void GetLevelDesc(int Level, IntPtr pDesc);
        [PreserveSig, SuppressUnmanagedCodeSecurity]
        int GetSurfaceLevel(int Level, out IDirect3DSurface9 ppSurfaceLevel);
        [PreserveSig, SuppressUnmanagedCodeSecurity]
        void LockRect(int Level, ref D3DLOCKED_RECT pLockedRect, RECT pRect, int Flags);
        [PreserveSig, SuppressUnmanagedCodeSecurity]
        void UnlockRect(int Level);
        [PreserveSig, SuppressUnmanagedCodeSecurity]
        void AddDirtyRect(RECT pDirtyRect);
    }



    [ComImport, SuppressUnmanagedCodeSecurity,
    Guid("02177241-69FC-400C-8FF1-93A44DF6861D"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown), SuppressUnmanagedCodeSecurity]
    public interface IDirect3D9Ex : IDirect3D9
    {
        [PreserveSig, SuppressUnmanagedCodeSecurity]
        new int RegisterSoftwareDevice([In, Out]IntPtr pInitializeFunction);

        [PreserveSig, SuppressUnmanagedCodeSecurity]
        new int GetAdapterCount();

        [PreserveSig, SuppressUnmanagedCodeSecurity]
        new int GetAdapterIdentifier(uint Adapter, uint Flags, uint pIdentifier);

        [PreserveSig, SuppressUnmanagedCodeSecurity]
        new uint GetAdapterModeCount(uint Adapter, D3DFORMAT Format);

        [PreserveSig, SuppressUnmanagedCodeSecurity]
        new int EnumAdapterModes(uint Adapter, D3DFORMAT Format, uint Mode, [Out] out D3DDISPLAYMODE pMode);

        [PreserveSig, SuppressUnmanagedCodeSecurity]
        new int GetAdapterDisplayMode(ushort Adapter, [Out]out D3DFORMAT Format);

        #region Method Placeholders
        [PreserveSig]
        new int CheckDeviceType();

        [PreserveSig]
        new int CheckDeviceFormat();

        [PreserveSig]
        new int CheckDeviceMultiSampleType();

        [PreserveSig]
        new int CheckDepthStencilMatch();

        [PreserveSig]
        new int CheckDeviceFormatConversion();

        [PreserveSig]
        new int GetDeviceCaps();
        #endregion

        [PreserveSig, SuppressUnmanagedCodeSecurity]
        new IntPtr GetAdapterMonitor(uint Adapter);

        [PreserveSig, SuppressUnmanagedCodeSecurity]
        int CreateDevice(int Adapter,
                          D3DDEVTYPE DeviceType,
                          IntPtr hFocusWindow,
                          CreateFlags BehaviorFlags,
                          [In, Out]
                          ref D3DPRESENT_PARAMETERS2 pPresentationParameters,
                          [Out]out IntPtr ppReturnedDeviceInterface);

        [PreserveSig, SuppressUnmanagedCodeSecurity]
        uint GetAdapterModeCountEx();

        [PreserveSig, SuppressUnmanagedCodeSecurity]
        int EnumAdapterModesEx();

        [PreserveSig, SuppressUnmanagedCodeSecurity]
        int GetAdapterDisplayModeEx();

        [PreserveSig, SuppressUnmanagedCodeSecurity]
        int CreateDeviceEx(int Adapter,
                          D3DDEVTYPE DeviceType,
                          IntPtr hFocusWindow,
                          CreateFlags BehaviorFlags,
                          [In, Out]
                          ref D3DPRESENT_PARAMETERS2 pPresentationParameters,
                          [In, Out]
                          IntPtr pFullscreenDisplayMode,
                          [Out]out IntPtr ppReturnedDeviceInterface);

        [PreserveSig, SuppressUnmanagedCodeSecurity]
        int GetAdapterLUID();
    }

    [ComImport, SuppressUnmanagedCodeSecurity,
    Guid("81BDCBCA-64D4-426d-AE8D-AD0147F4275C"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown), SuppressUnmanagedCodeSecurity]
    public interface IDirect3D9
    {
        [PreserveSig, SuppressUnmanagedCodeSecurity]
        int RegisterSoftwareDevice([In, Out]IntPtr pInitializeFunction);

        [PreserveSig, SuppressUnmanagedCodeSecurity]
        int GetAdapterCount();

        [PreserveSig, SuppressUnmanagedCodeSecurity]
        int GetAdapterIdentifier(uint Adapter, uint Flags, uint pIdentifier);

        [PreserveSig, SuppressUnmanagedCodeSecurity]
        uint GetAdapterModeCount(uint Adapter, D3DFORMAT Format);

        [PreserveSig, SuppressUnmanagedCodeSecurity]
        int EnumAdapterModes(uint Adapter, D3DFORMAT Format, uint Mode, [Out] out D3DDISPLAYMODE pMode);

        [PreserveSig, SuppressUnmanagedCodeSecurity]
        int GetAdapterDisplayMode(ushort Adapter, [Out]out D3DFORMAT Format);

        #region Method Placeholders
        [PreserveSig]
        int CheckDeviceType();

        [PreserveSig]
        int CheckDeviceFormat();

        [PreserveSig]
        int CheckDeviceMultiSampleType();

        [PreserveSig]
        int CheckDepthStencilMatch();

        [PreserveSig]
        int CheckDeviceFormatConversion();

        [PreserveSig]
        int GetDeviceCaps();
        #endregion

        [PreserveSig, SuppressUnmanagedCodeSecurity]
        IntPtr GetAdapterMonitor(uint Adapter);

        [PreserveSig, SuppressUnmanagedCodeSecurity]
        int CreateDevice(int Adapter,
                          D3DDEVTYPE DeviceType,
                          IntPtr hFocusWindow,
                          CreateFlags BehaviorFlags,
                          [In, Out]
                          ref D3DPRESENT_PARAMETERS2 pPresentationParameters,
                          [Out]out IDirect3DDevice9 ppReturnedDeviceInterface);
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct D3DDISPLAYMODE
    {
        public uint Width;
        public uint Height;
        public uint RefreshRate;
        public D3DFORMAT Format;
    }

    [Flags]
    public enum CreateFlags
    {
        D3DCREATE_FPU_PRESERVE = 0x00000002,
        D3DCREATE_MULTITHREADED = 0x00000004,
        D3DCREATE_PUREDEVICE = 0x00000010,
        D3DCREATE_SOFTWARE_VERTEXPROCESSING = 0x00000020,
        D3DCREATE_HARDWARE_VERTEXPROCESSING = 0x00000040,
        D3DCREATE_MIXED_VERTEXPROCESSING = 0x00000080,
        D3DCREATE_DISABLE_DRIVER_MANAGEMENT = 0x00000100,
        D3DCREATE_ADAPTERGROUP_DEVICE = 0x00000200,
        D3DCREATE_DISABLE_DRIVER_MANAGEMENT_EX = 0x00000400
    }

    [Flags]
    public enum D3DDEVTYPE
    {
        D3DDEVTYPE_HAL = 1,
        D3DDEVTYPE_REF = 2,
        D3DDEVTYPE_SW = 3,
        D3DDEVTYPE_NULLREF = 4,
    }

    [Flags]
    public enum D3DFORMAT
    {
        D3DFMT_UNKNOWN = 0,
        D3DFMT_R8G8B8 = 20,
        D3DFMT_A8R8G8B8 = 21,
        D3DFMT_X8R8G8B8 = 22,
        D3DFMT_R5G6B5 = 23,
        D3DFMT_X1R5G5B5 = 24,
        D3DFMT_A1R5G5B5 = 25,
        D3DFMT_A4R4G4B4 = 26,
        D3DFMT_R3G3B2 = 27,
        D3DFMT_A8 = 28,
        D3DFMT_A8R3G3B2 = 29,
        D3DFMT_X4R4G4B4 = 30,
        D3DFMT_A2B10G10R10 = 31,
        D3DFMT_A8B8G8R8 = 32,
        D3DFMT_X8B8G8R8 = 33,
        D3DFMT_G16R16 = 34,
        D3DFMT_A2R10G10B10 = 35,
        D3DFMT_A16B16G16R16 = 36,
        D3DFMT_A8P8 = 40,
        D3DFMT_P8 = 41,
        D3DFMT_L8 = 50,
        D3DFMT_A8L8 = 51,
        D3DFMT_A4L4 = 52,
        D3DFMT_V8U8 = 60,
        D3DFMT_L6V5U5 = 61,
        D3DFMT_X8L8V8U8 = 62,
        D3DFMT_Q8W8V8U8 = 63,
        D3DFMT_V16U16 = 64,
        D3DFMT_A2W10V10U10 = 67,
        D3DFMT_D16_LOCKABLE = 70,
        D3DFMT_D32 = 71,
        D3DFMT_D15S1 = 73,
        D3DFMT_D24S8 = 75,
        D3DFMT_D24X8 = 77,
        D3DFMT_D24X4S4 = 79,
        D3DFMT_D16 = 80,
        D3DFMT_D32F_LOCKABLE = 82,
        D3DFMT_D24FS8 = 83,
        /* Z-Stencil formats valid for CPU access */
        D3DFMT_D32_LOCKABLE = 84,
        D3DFMT_S8_LOCKABLE = 85,
        D3DFMT_L16 = 81,
        D3DFMT_VERTEXDATA = 100,
        D3DFMT_INDEX16 = 101,
        D3DFMT_INDEX32 = 102,
        D3DFMT_Q16W16V16U16 = 110,
        // Floating point surface formats
        // s10e5 formats (16-bits per channel)
        D3DFMT_R16F = 111,
        D3DFMT_G16R16F = 112,
        D3DFMT_A16B16G16R16F = 113,
        // IEEE s23e8 formats (32-bits per channel)
        D3DFMT_R32F = 114,
        D3DFMT_G32R32F = 115,
        D3DFMT_A32B32G32R32F = 116,
        D3DFMT_CxV8U8 = 117,
        // Monochrome 1 bit per pixel format
        D3DFMT_A1 = 118,
        // Binary format indicating that the data has no inherent type
        D3DFMT_BINARYBUFFER = 199,
    }

    [Flags]
    public enum D3DSWAPEFFECT
    {
        D3DSWAPEFFECT_DISCARD = 1,
        D3DSWAPEFFECT_FLIP = 2,
        D3DSWAPEFFECT_COPY = 3,
    }

    [Flags]
    public enum D3DMULTISAMPLE_TYPE
    {
        D3DMULTISAMPLE_NONE = 0,
        D3DMULTISAMPLE_NONMASKABLE = 1,
        D3DMULTISAMPLE_2_SAMPLES = 2,
        D3DMULTISAMPLE_3_SAMPLES = 3,
        D3DMULTISAMPLE_4_SAMPLES = 4,
        D3DMULTISAMPLE_5_SAMPLES = 5,
        D3DMULTISAMPLE_6_SAMPLES = 6,
        D3DMULTISAMPLE_7_SAMPLES = 7,
        D3DMULTISAMPLE_8_SAMPLES = 8,
        D3DMULTISAMPLE_9_SAMPLES = 9,
        D3DMULTISAMPLE_10_SAMPLES = 10,
        D3DMULTISAMPLE_11_SAMPLES = 11,
        D3DMULTISAMPLE_12_SAMPLES = 12,
        D3DMULTISAMPLE_13_SAMPLES = 13,
        D3DMULTISAMPLE_14_SAMPLES = 14,
        D3DMULTISAMPLE_15_SAMPLES = 15,
        D3DMULTISAMPLE_16_SAMPLES = 16,
    }

    [Flags]
    public enum D3DPRESENTFLAG
    {
        D3DPRESENTFLAG_LOCKABLE_BACKBUFFER = 0x00000001,
        D3DPRESENTFLAG_DISCARD_DEPTHSTENCIL = 0x00000002,
        D3DPRESENTFLAG_DEVICECLIP = 0x00000004,
        D3DPRESENTFLAG_VIDEO = 0x00000010
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct D3DPRESENT_PARAMETERS2
    {
        public uint BackBufferWidth;
        public uint BackBufferHeight;
        public D3DFORMAT BackBufferFormat;
        public uint BackBufferCount;
        public D3DMULTISAMPLE_TYPE MultiSampleType;
        public int MultiSampleQuality;
        public D3DSWAPEFFECT SwapEffect;
        public IntPtr hDeviceWindow;
        public int Windowed;
        public int EnableAutoDepthStencil;
        public D3DFORMAT AutoDepthStencilFormat;
        public int Flags;
        /* FullScreen_RefreshRateInHz must be zero for Windowed mode */
        public uint FullScreen_RefreshRateInHz;
        public uint PresentationInterval;
    }
#endregion

    internal class DXHookD3D8: BaseDXHook
    {
        //Microsoft.DirectX.DirectDraw.Device dev = new Microsoft.DirectX.DirectDraw.Device();
        //Microsoft.DirectX.DirectDraw.SurfaceDescription desc = new Microsoft.DirectX.DirectDraw.SurfaceDescription();
        //Microsoft.DirectX.DirectDraw.Surface s1;
        
        [DllImport("d3d9.dll", EntryPoint = "Direct3DCreate9", CallingConvention = CallingConvention.StdCall),
SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Interface)]
        public static extern IDirect3D9 Direct3DCreate9(ushort SDKVersion);
        
        [DllImport("ddraw.dll", EntryPoint = "DirectDrawCreate", CallingConvention = CallingConvention.StdCall),
SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Interface)]
        public static extern void DirectDrawCreate(IntPtr GUID, [Out, MarshalAs(UnmanagedType.Interface)] out IDirectDraw7 dd, IntPtr pUnkOuter);

        public DXHookD3D8(ScreenshotInterface.ScreenshotInterface ssInterface)
            : base(ssInterface)
        {
        }

        LocalHook Direct3DDevice_EndSceneHook = null;
        LocalHook Direct3DDevice_ResetHook = null;
        LocalHook DirectDrawSurface_BltHook = null;
        object _lockRenderTarget = new object();
        SlimDX.Direct3D9.Surface _renderTarget;

        protected override string HookName
        {
            get
            {
                return "DXHookDD";
            }
        }
        
        const int D3D9_DEVICE_METHOD_COUNT = 119;
        public override void Hook()
        {
            //dev.SetCooperativeLevel(new System.Windows.Forms.Form(), CooperativeLevelFlags.Normal);
            //desc.SurfaceCaps.OffScreenPlain = true;
            //desc.Height = 300;
            //desc.Width = 300;
            //s1 = new Microsoft.DirectX.DirectDraw.Surface(desc, dev);
            

            this.DebugMessage("Hook: DD Begin");
            // First we need to determine the function address for IDirect3DDevice9
            SlimDX.Direct3D9.Device mydevice;
            List<IntPtr> id3dDeviceFunctionAddresses = new List<IntPtr>();
            this.DebugMessage("Hook: Before device creation");
            using (Direct3D d3d = new Direct3D())
            {
                this.DebugMessage("Hook: Device created");
                using (mydevice = new SlimDX.Direct3D9.Device(d3d, 0, DeviceType.NullReference, IntPtr.Zero, SlimDX.Direct3D9.CreateFlags.HardwareVertexProcessing, new PresentParameters() { BackBufferWidth = 1, BackBufferHeight = 1 }))
                {
                    id3dDeviceFunctionAddresses.AddRange(GetVTblAddresses(mydevice.ComPointer, D3D9_DEVICE_METHOD_COUNT));
                }
            }
            
            
            int r;
            IntPtr ir;
            #region Test - try to find dd surface blt method addr
            IDirectDraw7 mydd;
            Type DDinterfaceType = typeof(IDirectDrawSurface);
            DirectDrawCreate(IntPtr.Zero, out mydd, IntPtr.Zero);
            this.DebugMessage("Created directDraw object");
            IDirectDrawSurface mysurface;
            r = mydd.SetCooperativeLevel(IntPtr.Zero, new IntPtr(1));
            this.DebugMessage("Setcooperativelevel, returned: " + r.ToString());
            DDSURFACEDESC2 ddesc = new DDSURFACEDESC2();
            ddesc.dwSize = 124;
            ddesc.ddsCaps.dwCaps = 64;
            ddesc.dwFlags = 7;
            ddesc.dwHeight = 300;
            ddesc.dwWidth = 300;

            ir = mydd.CreateSurface(ref ddesc, out mysurface, IntPtr.Zero);
            //ir = mydd.CreateSurface(0x0018fbf8, out mysurface, IntPtr.Zero);
            this.DebugMessage("Created directDraw surface, returned: " + ir.ToString() );
            IntPtr ddinterfaceIntPtr = Marshal.GetComInterfaceForObject(mysurface, DDinterfaceType);
            unsafe
            {
                int* faddr;
                int*** ddinterfaceRawPtr = (int***)ddinterfaceIntPtr.ToPointer();
                int** vTable = *ddinterfaceRawPtr;
                this.DebugMessage("directDraw surface intptr, returned: " + ddinterfaceIntPtr.ToString());
                                
                MethodInfo mi = DDinterfaceType.GetMethod("Blt");
                int mi_vto = Marshal.GetComSlotForMethodInfo(mi);
                faddr = vTable[mi_vto];
                
                this.DebugMessage("Hook: comslot: " + mi_vto.ToString ());
                this.DebugMessage("Hook: final blt addr: " + (int)faddr);

                DirectDrawSurface_BltHook = LocalHook.Create(new System.IntPtr(faddr), new DirectDrawSurface_BltDelegate(BltHook), this);
            }
            #endregion
                        
            #region Test - try to find device addr my way, and FOUND IT!
            Type interfaceType = typeof(IDirect3DDevice9);
            IDirect3D9 d = Direct3DCreate9(32);
            IDirect3DDevice9 mydevice2;
            D3DPRESENT_PARAMETERS2 d3dpp = new D3DPRESENT_PARAMETERS2();
            d3dpp.Windowed = 1;
            d3dpp.SwapEffect = D3DSWAPEFFECT.D3DSWAPEFFECT_DISCARD ;
            d3dpp.BackBufferFormat = D3DFORMAT.D3DFMT_A8R8G8B8;
            d3dpp.EnableAutoDepthStencil = 1;
            d3dpp.AutoDepthStencilFormat = D3DFORMAT.D3DFMT_D16;

            r = d.CreateDevice(0, D3DDEVTYPE.D3DDEVTYPE_NULLREF, IntPtr.Zero, CreateFlags.D3DCREATE_MIXED_VERTEXPROCESSING, ref d3dpp, out mydevice2);
            this.DebugMessage("Hook: Device create return 2.0 " + r.ToString ());
            IntPtr interfaceIntPtr = Marshal.GetComInterfaceForObject(mydevice2, interfaceType);
            
            unsafe {int*** interfaceRawPtr = (int***)interfaceIntPtr.ToPointer();
            int** vTable = *interfaceRawPtr; 
            this.DebugMessage("Hook: ih com ptr " + mydevice.ComPointer.ToString ());
            this.DebugMessage("Hook: my com ptr (interface int ptr) " + interfaceIntPtr.ToString() );
            

            MethodInfo mi = interfaceType.GetMethod("EndScene");
            int mi_vto = Marshal.GetComSlotForMethodInfo(mi);
            int* faddr = vTable[mi_vto];

            this.DebugMessage("Hook: ih addr to end_scene " + id3dDeviceFunctionAddresses[(int)Direct3DDevice9FunctionOrdinals.EndScene]);
            this.DebugMessage("Hook: my addr to end_scene " + (int)faddr);
            }
            #endregion

            // We want to hook each method of the IDirect3DDevice9 interface that we are interested in
            
            // 42 - EndScene (we will retrieve the back buffer here)
            Direct3DDevice_EndSceneHook = LocalHook.Create(
                id3dDeviceFunctionAddresses[(int)Direct3DDevice9FunctionOrdinals.EndScene],
                // On Windows 7 64-bit w/ 32-bit app and d3d9 dll version 6.1.7600.16385, the address is equiv to:
                // (IntPtr)(GetModuleHandle("d3d9").ToInt32() + 0x1ce09),
                // A 64-bit app would use 0xff18
                // Note: GetD3D9DeviceFunctionAddress will output these addresses to a log file
                new Direct3D9Device_EndSceneDelegate(EndSceneHook),
                this);

            // 16 - Reset (called on resolution change or windowed/fullscreen change - we will reset some things as well)
            Direct3DDevice_ResetHook = LocalHook.Create(
                id3dDeviceFunctionAddresses[(int)Direct3DDevice9FunctionOrdinals.Reset],
                // On Windows 7 64-bit w/ 32-bit app and d3d9 dll version 6.1.7600.16385, the address is equiv to:
                //(IntPtr)(GetModuleHandle("d3d9").ToInt32() + 0x58dda),
                // A 64-bit app would use 0x3b3a0
                // Note: GetD3D9DeviceFunctionAddress will output these addresses to a log file
                new Direct3D9Device_ResetDelegate(ResetHook),
                this);

            /*
             * Don't forget that all hooks will start deactivated...
             * The following ensures that all threads are intercepted:
             * Note: you must do this for each hook.
             */
            Direct3DDevice_EndSceneHook.ThreadACL.SetExclusiveACL(new Int32[1]);

            Direct3DDevice_ResetHook.ThreadACL.SetExclusiveACL(new Int32[1]);

            DirectDrawSurface_BltHook.ThreadACL.SetExclusiveACL(new Int32[1]);

            this.DebugMessage("Hook: End");
        }



        /// <summary>
        /// Just ensures that the surface we created is cleaned up.
        /// </summary>
        public override void Cleanup()
        {
            try
            {
                lock (_lockRenderTarget)
                {
                    if (_renderTarget != null)
                    {
                        _renderTarget.Dispose();
                        _renderTarget = null;
                    }

                    Request = null;
                }
            }
            catch
            {
            }
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        delegate int DirectDrawSurface_BltDelegate(IntPtr a, IntPtr b, IntPtr c, IntPtr d, IntPtr e, IntPtr f);

        /// <summary>
        /// The IDirect3DDevice9.EndScene function definition
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        delegate int Direct3D9Device_EndSceneDelegate(IntPtr device);

        /// <summary>
        /// The IDirect3DDevice9.Reset function definition
        /// </summary>
        /// <param name="device"></param>
        /// <param name="presentParameters"></param>
        /// <returns></returns>
        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        delegate int Direct3D9Device_ResetDelegate(IntPtr device, ref D3DPRESENT_PARAMETERS presentParameters);

        int BltHook(IntPtr dsur, IntPtr bptr, IntPtr ssur, IntPtr dptr, IntPtr dflags, IntPtr hren)
        {
            //this.DebugMessage("intercepted");
            try
            {
                int res;
                IDirectDrawSurface src;
                IDirectDrawSurface dst;
                src = (IDirectDrawSurface)Marshal.GetObjectForIUnknown(ssur);
                dst = (IDirectDrawSurface)Marshal.GetObjectForIUnknown(dsur);
                /*
                if (src.IsLost() == 0)
                {
                    IntPtr hdc;
                    res = src.GetDC(out hdc);
                    if (res != 0) { this.DebugMessage(DateTime.Now.ToString() + "getDC err: " + res.ToString()); }

                    s1.DrawCircle(50, 50, 40);
                    s1.DrawToDc(hdc, new Rectangle(10, 50, 100, 100), new Rectangle(10, 50, 100, 100));
                    src.ReleaseDC(hdc);
                }
                else { this.DebugMessage(DateTime.Now.ToString() + "surface is lost! " + src.IsLost()); }
                */

                res = dst.Blt(bptr, ssur, dptr, dflags, hren);
                if (res != 0) { this.DebugMessage(DateTime.Now.ToString() + "dst blt err: " + res.ToString()); }
                res = Marshal.ReleaseComObject(src);
                res = Marshal.ReleaseComObject(dst);
            }
            catch (Exception e)
            {
                this.DebugMessage(DateTime.Now.ToString() + ":" + DateTime.Now.Millisecond.ToString() + "err: " + e.Message);
            }
            return 0;
        }

        /// <summary>
        /// Reset the _renderTarget so that we are sure it will have the correct presentation parameters (required to support working across changes to windowed/fullscreen or resolution changes)
        /// </summary>
        /// <param name="devicePtr"></param>
        /// <param name="presentParameters"></param>
        /// <returns></returns>
        int ResetHook(IntPtr devicePtr, ref D3DPRESENT_PARAMETERS presentParameters)
        {
            using (SlimDX.Direct3D9.Device device = SlimDX.Direct3D9.Device.FromPointer(devicePtr))
            {
                PresentParameters pp = new PresentParameters()
                {
                    AutoDepthStencilFormat = (Format)presentParameters.AutoDepthStencilFormat,
                    BackBufferCount = presentParameters.BackBufferCount,
                    BackBufferFormat = (Format)presentParameters.BackBufferFormat,
                    BackBufferHeight = presentParameters.BackBufferHeight,
                    BackBufferWidth = presentParameters.BackBufferWidth,
                    DeviceWindowHandle = presentParameters.DeviceWindowHandle,
                    EnableAutoDepthStencil = presentParameters.EnableAutoDepthStencil,
                    FullScreenRefreshRateInHertz = presentParameters.FullScreen_RefreshRateInHz,
                    Multisample = (MultisampleType)presentParameters.MultiSampleType,
                    MultisampleQuality = presentParameters.MultiSampleQuality,
                    PresentationInterval = (PresentInterval)presentParameters.PresentationInterval,
                    PresentFlags = (PresentFlags)presentParameters.Flags,
                    SwapEffect = (SwapEffect)presentParameters.SwapEffect,
                    Windowed = presentParameters.Windowed
                };
                lock (_lockRenderTarget)
                {
                    if (_renderTarget != null)
                    {
                        _renderTarget.Dispose();
                        _renderTarget = null;
                    }
                }
                // EasyHook has already repatched the original Reset so calling it here will not cause an endless recursion to this function
                return device.Reset(pp).Code;
            }
        }

        TimeSpan _lastScreenshotTime = new TimeSpan(0);

        DateTime? _lastFrame;
        
        // Used in the overlay
        DateTime? _lastRequestTime;
        SlimDX.Vector2[] _lineVectors = null;
        float _lineAlpha = 1;

        /// <summary>
        /// Hook for IDirect3DDevice9.EndScene
        /// </summary>
        /// <param name="devicePtr">Pointer to the IDirect3DDevice9 instance. Note: object member functions always pass "this" as the first parameter.</param>
        /// <returns>The HRESULT of the original EndScene</returns>
        /// <remarks>Remember that this is called many times a second by the Direct3D application - be mindful of memory and performance!</remarks>
        int EndSceneHook(IntPtr devicePtr)
        {
            using (SlimDX.Direct3D9.Device device = SlimDX.Direct3D9.Device.FromPointer(devicePtr))
            {
                // If you need to capture at a particular frame rate, add logic here decide whether or not to skip the frame
                try
                {
                    #region Screenshot Request
                    // Is there a screenshot request? If so lets grab the backbuffer
                    lock (_lockRenderTarget)
                    {
                        if (Request != null)
                        {
                            _lastRequestTime = DateTime.Now;
                            DateTime start = DateTime.Now;
                            try
                            {
                                // First ensure we have a Surface to the render target data into
                                if (_renderTarget == null)
                                {
                                    // Create offscreen surface to use as copy of render target data
                                    using (SwapChain sc = device.GetSwapChain(0))
                                    {
                                        _renderTarget = SlimDX.Direct3D9.Surface.CreateOffscreenPlain(device, sc.PresentParameters.BackBufferWidth, sc.PresentParameters.BackBufferHeight, sc.PresentParameters.BackBufferFormat, Pool.SystemMemory);
                                    }
                                }

                                #region Prepare lines for overlay
                                if (this.Request.RegionToCapture.Width == 0)
                                {
                                    _lineVectors = new SlimDX.Vector2[] { 
                                        new SlimDX.Vector2(0, 0),
                                        new SlimDX.Vector2(_renderTarget.Description.Width - 1, _renderTarget.Description.Height - 1),
                                        new SlimDX.Vector2(0, _renderTarget.Description.Height - 1),
                                        new SlimDX.Vector2(_renderTarget.Description.Width - 1, 0),
                                        new SlimDX.Vector2(0, 0),
                                        new SlimDX.Vector2(0, _renderTarget.Description.Height - 1),
                                        new SlimDX.Vector2(_renderTarget.Description.Width - 1, _renderTarget.Description.Height - 1),
                                        new SlimDX.Vector2(_renderTarget.Description.Width - 1, 0),
                                    };
                                }
                                else
                                {
                                    _lineVectors = new SlimDX.Vector2[] { 
                                        new SlimDX.Vector2(this.Request.RegionToCapture.X, this.Request.RegionToCapture.Y),
                                        new SlimDX.Vector2(this.Request.RegionToCapture.Right, this.Request.RegionToCapture.Bottom),
                                        new SlimDX.Vector2(this.Request.RegionToCapture.X, this.Request.RegionToCapture.Bottom),
                                        new SlimDX.Vector2(this.Request.RegionToCapture.Right, this.Request.RegionToCapture.Y),
                                        new SlimDX.Vector2(this.Request.RegionToCapture.X, this.Request.RegionToCapture.Y),
                                        new SlimDX.Vector2(this.Request.RegionToCapture.X, this.Request.RegionToCapture.Bottom),
                                        new SlimDX.Vector2(this.Request.RegionToCapture.Right, this.Request.RegionToCapture.Bottom),
                                        new SlimDX.Vector2(this.Request.RegionToCapture.Right, this.Request.RegionToCapture.Y),
                                    };
                                }
                                #endregion

                                using (SlimDX.Direct3D9.Surface backBuffer = device.GetBackBuffer(0, 0))
                                {
                                    // Create a super fast copy of the back buffer on our Surface
                                    device.GetRenderTargetData(backBuffer, _renderTarget);

                                    // We have the back buffer data and can now work on copying it to a bitmap
                                    ProcessRequest();
                                }
                            }
                            finally
                            {
                                // We have completed the request - mark it as null so we do not continue to try to capture the same request
                                // Note: If you are after high frame rates, consider implementing buffers here to capture more frequently
                                //         and send back to the host application as needed. The IPC overhead significantly slows down 
                                //         the whole process if sending frame by frame.
                                Request = null;
                            }
                            DateTime end = DateTime.Now;
                            this.DebugMessage("EndSceneHook: Capture time: " + (end - start).ToString());
                            _lastScreenshotTime = (end - start);
                        }
                    }
                    #endregion

                    #region Example: Draw Overlay (after screenshot so we don't capture overlay as well)

                    if (this.ShowOverlay)
                    {
                        #region Draw fading lines based on last screencapture request
                        if (_lastRequestTime != null && _lineVectors != null)
                        {
                            TimeSpan timeSinceRequest = DateTime.Now - _lastRequestTime.Value;
                            if (timeSinceRequest.TotalMilliseconds < 1000.0)
                            {
                                using (Line line = new Line(device))
                                {
                                    _lineAlpha = (float)((1000.0 - timeSinceRequest.TotalMilliseconds) / 1000.0); // This is our fade out
                                    line.Antialias = true;
                                    line.Width = 1.0f;
                                    line.Begin();
                                    line.Draw(_lineVectors, new SlimDX.Color4(_lineAlpha, 0.5f, 0.5f, 1.0f));
                                    line.End();
                                }
                            }
                            else
                            {
                                _lineVectors = null;
                            }
                        }
                        #endregion

                        #region Draw frame rate
                        using (SlimDX.Direct3D9.Font font = new SlimDX.Direct3D9.Font(device, new System.Drawing.Font("Times New Roman", 16.0f)))
                        {
                            if (_lastFrame != null)
                            {
                                font.DrawString(null, String.Format("{0:N1} fps", (1000.0 / (DateTime.Now - _lastFrame.Value).TotalMilliseconds)), 100, 100, System.Drawing.Color.Red);
                            }
                            _lastFrame = DateTime.Now;
                        }
                        #endregion
                    }

                    #endregion
                }
                catch(Exception e)
                {
                    // If there is an error we do not want to crash the hooked application, so swallow the exception
                    this.DebugMessage("EndSceneHook: Exeception: " + e.GetType().FullName + ": " + e.Message);
                }

                // EasyHook has already repatched the original EndScene - so calling EndScene against the SlimDX device will call the original
                // EndScene and bypass this hook. EasyHook will automatically reinstall the hook after this hook function exits.
                return device.EndScene().Code;
            }
        }

        /// <summary>
        /// Copies the _renderTarget surface into a stream and starts a new thread to send the data back to the host process
        /// </summary>
        void ProcessRequest()
        {
            if (Request != null)
            {
                // SlimDX now uses a marshal_as for Rectangle to RECT that correctly does the conversion for us, therefore no need
                // to adjust the region Width/Height to fit the x1,y1,x2,y2 format.
                Rectangle region = Request.RegionToCapture;
                
                // Prepare the parameters for RetrieveImageData to be called in a separate thread.
                RetrieveImageDataParams retrieveParams = new RetrieveImageDataParams();

                // After the Stream is created we are now finished with _renderTarget and have our own separate copy of the data,
                // therefore it will now be safe to begin a new thread to complete processing.
                // Note: RetrieveImageData will take care of closing the stream.
                // Note 2: Surface.ToStream is the slowest part of the screen capture process - the other methods
                //         available to us at this point are _renderTarget.GetDC(), and _renderTarget.LockRectangle/UnlockRectangle
                if (Request.RegionToCapture.Width == 0)
                {
                    // The width is 0 so lets grab the entire window
                    retrieveParams.Stream = SlimDX.Direct3D9.Surface.ToStream(_renderTarget, ImageFileFormat.Bmp);
                }
                else if (Request.RegionToCapture.Height > 0)
                {
                    retrieveParams.Stream = SlimDX.Direct3D9.Surface.ToStream(_renderTarget, ImageFileFormat.Bmp, region);
                }

                if (retrieveParams.Stream != null)
                {
                    // _screenshotRequest will most probably be null by the time RetrieveImageData is executed 
                    // in a new thread, therefore we must provide the RequestId separately.
                    retrieveParams.RequestId = Request.RequestId;

                    // Begin a new thread to process the image data and send the request result back to the host application
                    Thread t = new Thread(new ParameterizedThreadStart(RetrieveImageData));
                    t.Start(retrieveParams);
                }
            }
        }

        /// <summary>
        /// Used to hold the parameters to be passed to RetrieveImageData
        /// </summary>
        struct RetrieveImageDataParams
        {
            internal Stream Stream;
            internal Guid RequestId;
        }

        /// <summary>
        /// ParameterizedThreadStart method that places the image data from the stream into a byte array and then sets the Interface screenshot response. This can be called asynchronously.
        /// </summary>
        /// <param name="param">An instance of RetrieveImageDataParams is required to be passed as the parameter.</param>
        /// <remarks>The stream object passed will be closed!</remarks>
        void RetrieveImageData(object param)
        {
            RetrieveImageDataParams retrieveParams = (RetrieveImageDataParams)param;
            try
            {
                SendResponse(retrieveParams.Stream, retrieveParams.RequestId);
            }
            finally
            {
                retrieveParams.Stream.Close();
            }
        }
    }
}
