using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EasyHook;
using ScreenshotInterface;
using System.Runtime.InteropServices;
using System.Drawing;
using SlimDX;
using SlimDX.Direct3D9;
using System.Threading;
using System.IO;
using System.Reflection;

namespace ScreenshotInject
{
    public class ScreenshotInjection : EasyHook.IEntryPoint
    {
        IDXHook _directXHook = null;
        IDXHook _directXHookDI = null;
        public string v;
        private ScreenshotInterface.ScreenshotInterface _interface = null;
        
        /*
        internal static void init()
        {
            System.Windows.Forms.MessageBox.Show("aaaaaaaaa");
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomainAssemblyResolve);
        }

        private static System.Reflection.Assembly CurrentDomainAssemblyResolve(object sender, ResolveEventArgs args)
        {
            try
            {
                System.Reflection.Assembly ass = System.Reflection.Assembly.LoadFrom("F:\\Documents\\My_Progs\\MemoryEditor_0.04\\Direct3DHook-overlay\\bin\\SlimDX-x32\\SlimDX.dll");
                System.Windows.Forms.MessageBox.Show("name: "+ass.FullName);
                return ass;
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show("exception: " + e.Message);
                return null;
            }                        
        }*/

        public ScreenshotInjection(RemoteHooking.IContext context, String channelName, String version, bool showOverlay)
        {
            // Get reference to IPC to host application
            // Note: any methods called or events triggered against _interface will execute in the host process.
            _interface = RemoteHooking.IpcConnectClient<ScreenshotInterface.ScreenshotInterface>(channelName);
        }

        /// <summary>
        /// Called by EasyHook to begin any hooking etc in the target process
        /// </summary>
        /// <param name="InContext"></param>
        /// <param name="channelName"></param>
        /// <param name="strVersion">Direct3DVersion passed as a string so that GAC registration is not required</param>
        /// <param name="showOverlay">Whether or not to show an overlay</param>
        public void Run(RemoteHooking.IContext InContext, String channelName, String strVersion, bool showOverlay)
        {
            Direct3DVersion version = (Direct3DVersion)Enum.Parse(typeof(Direct3DVersion), strVersion);
            // NOTE: We are now already running within the target process
            try
            {
                v = strVersion;
                _interface.OnDebugMessage(RemoteHooking.GetCurrentProcessId(), "DLL Injection succeeded");
                _interface.OnDebugMessage(RemoteHooking.GetCurrentProcessId(), DateTime.Now.ToString() + ":" + DateTime.Now.Millisecond.ToString());
                bool isX64Process = RemoteHooking.IsX64Process(RemoteHooking.GetCurrentProcessId());
                _interface.OnDebugMessage(RemoteHooking.GetCurrentProcessId(), "64-bit Process: " + isX64Process.ToString());

                if (version == Direct3DVersion.Di) {
                   _directXHookDI = new DXHookDi(_interface); _directXHookDI.Hook();
                   _interface.OnDebugMessage(RemoteHooking.GetCurrentProcessId(), "DI Initialized"); 
                }
                if (version == Direct3DVersion.AutoDetect || version == Direct3DVersion.Di)
                {
                    // Attempt to determine the correct version based on loaded module.
                    // In most cases this will work fine, however it is perfectly ok for an application to use a D3D10 device along with D3D11 devices
                    // so the version might matched might not be the one you want to use
                    IntPtr ddLoaded = IntPtr.Zero;
                    IntPtr oglLoaded = IntPtr.Zero;
                    IntPtr gdiLoaded = IntPtr.Zero;
                    IntPtr d3D9Loaded = IntPtr.Zero;
                    IntPtr d3D10Loaded = IntPtr.Zero;
                    IntPtr d3D10_1Loaded = IntPtr.Zero;
                    IntPtr d3D11Loaded = IntPtr.Zero;
                    IntPtr d3D11_1Loaded = IntPtr.Zero;

                    int delayTime = 100;
                    int retryCount = 0;
                    while (ddLoaded == IntPtr.Zero && d3D9Loaded == IntPtr.Zero && d3D10Loaded == IntPtr.Zero && d3D10_1Loaded == IntPtr.Zero && d3D11Loaded == IntPtr.Zero && d3D11_1Loaded == IntPtr.Zero)
                    {
                        retryCount++;
                        ddLoaded = GetModuleHandle("ddraw.dll");
                        oglLoaded = GetModuleHandle("opengl32.dll");
                        gdiLoaded = GetModuleHandle("gdi32.dll");
                        d3D9Loaded = GetModuleHandle("d3d9.dll");
                        d3D10Loaded = GetModuleHandle("d3d10.dll");
                        d3D10_1Loaded = GetModuleHandle("d3d10_1.dll");
                        d3D11Loaded = GetModuleHandle("d3d11.dll");
                        d3D11_1Loaded = GetModuleHandle("d3d11_1.dll");
                        Thread.Sleep(delayTime);

                        if (retryCount * delayTime > 5000)
                        {
                            _interface.OnDebugMessage(RemoteHooking.GetCurrentProcessId(), "Unsupported Direct3DVersion, or Direct3D DLL not loaded within 5 seconds.");
                            return;
                        }
                    }

                    version = Direct3DVersion.Unknown;
                    if (d3D11_1Loaded != IntPtr.Zero)
                    {
                        _interface.OnDebugMessage(RemoteHooking.GetCurrentProcessId(), "Autodetect found Direct3D 11.1");
                        version = Direct3DVersion.Direct3D11_1;
                    }
                    else if (d3D11Loaded != IntPtr.Zero)
                    {
                        _interface.OnDebugMessage(RemoteHooking.GetCurrentProcessId(), "Autodetect found Direct3D 11");
                        version = Direct3DVersion.Direct3D11;
                        _directXHook = new DXHookD3D11(_interface);
                    }
                    else if (d3D10_1Loaded != IntPtr.Zero)
                    {
                        _interface.OnDebugMessage(RemoteHooking.GetCurrentProcessId(), "Autodetect found Direct3D 10.1");
                        version = Direct3DVersion.Direct3D10_1;
                        _directXHook = new DXHookD3D10_1(_interface);
                    }
                    else if (d3D10Loaded != IntPtr.Zero)
                    {
                        _interface.OnDebugMessage(RemoteHooking.GetCurrentProcessId(), "Autodetect found Direct3D 10");
                        version = Direct3DVersion.Direct3D10;
                        _directXHook = new DXHookD3D10(_interface);
                    }
                    else if (d3D9Loaded != IntPtr.Zero)
                    {
                        _interface.OnDebugMessage(RemoteHooking.GetCurrentProcessId(), "Autodetect found Direct3D 9");
                        version = Direct3DVersion.Direct3D9;
                        _directXHook = new DXHookD3D9(_interface);
                    }
                    else if (oglLoaded != IntPtr.Zero && gdiLoaded != IntPtr.Zero)
                    {
                        _interface.OnDebugMessage(RemoteHooking.GetCurrentProcessId(), "Autodetect found OPENGL+GDI");
                        version = Direct3DVersion.OGL;
                        _directXHook = new DXHookOGL(_interface);
                    }
                    else if (ddLoaded != IntPtr.Zero)
                    {
                        _interface.OnDebugMessage(RemoteHooking.GetCurrentProcessId(), "Autodetect found DDRAW");
                        version = Direct3DVersion.DDraw;
                        _directXHook = new DXHookDD(_interface);
                    }
                    //else {_interface.OnDebugMessage(RemoteHooking.GetCurrentProcessId(), "Unsupported Direct3DVersion");}
                }

                _directXHook.ShowOverlay = showOverlay;
                _directXHook.Hook();
            }
            catch (Exception e)
            {
                /*
                    We should notify our host process about this error...
                 */
                //_interface.ReportError(RemoteHooking.GetCurrentProcessId(), e);
                _interface.OnDebugMessage(RemoteHooking.GetCurrentProcessId(),"Exception during device creation and hooking: \r\n" + e.Message);
                while (_interface.Ping(RemoteHooking.GetCurrentProcessId())) {Thread.Sleep(100);}
                return;
            }

            // Wait for host process termination...
            try
            {
                // When not using GAC there can be issues with remoting assemblies resolving correctly
                // this is a workaround that ensures that the current assembly is correctly associated
                AppDomain currentDomain = AppDomain.CurrentDomain;
                currentDomain.AssemblyResolve += (sender, args) =>
                {
                    _interface.OnDebugMessage(RemoteHooking.GetCurrentProcessId(), "Searching: " + args.Name);
                    return this.GetType().Assembly.FullName == args.Name ? this.GetType().Assembly : null;
                };

                while (_interface.Ping(RemoteHooking.GetCurrentProcessId()))
                {
                    Thread.Sleep(10);

                    ScreenshotRequest request = _interface.GetScreenshotRequest(RemoteHooking.GetCurrentProcessId());
                    byte[] updateimg = _interface.GetUpdateRequest(RemoteHooking.GetCurrentProcessId());
                    if (request != null)
                    {
                        _directXHook.Request = request;
                    }
                    if (updateimg != null)
                    {
                        _directXHook.idxhookUpdateimg = updateimg;
                        if (_directXHookDI != null) _directXHookDI.idxhookUpdateimg = updateimg;
                    }
                }
            }
            catch
            {
                // .NET Remoting will raise an exception if host is unreachable
            }
            finally
            {
                try
                {
                    _directXHook.Cleanup();
                }
                catch
                {
                }
            }
        }

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

    }
}
