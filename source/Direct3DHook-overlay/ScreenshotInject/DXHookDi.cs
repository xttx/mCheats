using System;
using EasyHook;
using System.Runtime.InteropServices;

        #region structs & enums
        [StructLayout(LayoutKind.Sequential)]
        public struct NativeMessage
        {
            public IntPtr handle;
            public uint msg;
            public IntPtr wParam;
            public IntPtr lParam;
            public uint time;
            public System.Drawing.Point p;
        }        

        public class DeviceInfo
        {
            public string deviceName;
            public string deviceType;
            public IntPtr deviceHandle;
            public string Name;
            public string source;
            public ushort key;
            public string vKey;
        }
        [StructLayout(LayoutKind.Sequential)]
        internal struct RAWINPUTDEVICELIST
        {
            public IntPtr hDevice;
            [MarshalAs(UnmanagedType.U4)]
            public int dwType;
        }
        [StructLayout(LayoutKind.Explicit)]
        internal struct RAWINPUT
        {
            [FieldOffset(0)]
            public RAWINPUTHEADER header;
            [FieldOffset(16)]
            public RAWMOUSE mouse;
            [FieldOffset(16)]
            public RAWKEYBOARD keyboard;
            [FieldOffset(16)]
            public RAWHID hid;
        }
        [StructLayout(LayoutKind.Sequential)]
        internal struct RAWINPUTHEADER
        {
            [MarshalAs(UnmanagedType.U4)]
            public int dwType;
            [MarshalAs(UnmanagedType.U4)]
            public int dwSize;
            public IntPtr hDevice;
            [MarshalAs(UnmanagedType.U4)]
            public int wParam;
        }
        [StructLayout(LayoutKind.Sequential)]
        internal struct RAWHID
        {
            [MarshalAs(UnmanagedType.U4)]
            public int dwSizHid;
            [MarshalAs(UnmanagedType.U4)]
            public int dwCount;
        }
        [StructLayout(LayoutKind.Sequential)]
        internal struct BUTTONSSTR
        {
            [MarshalAs(UnmanagedType.U2)]
            public ushort usButtonFlags;
            [MarshalAs(UnmanagedType.U2)]
            public ushort usButtonData;
        }
        [StructLayout(LayoutKind.Explicit)]
        internal struct RAWMOUSE
        {
            [MarshalAs(UnmanagedType.U2)]
            [FieldOffset(0)] 
            public ushort usFlags;
            [MarshalAs(UnmanagedType.U4)]
            [FieldOffset(4)] 
            public uint ulButtons; 
            [FieldOffset(4)] 
            public BUTTONSSTR buttonsStr;
            [MarshalAs(UnmanagedType.U4)][FieldOffset(8)] 
            public uint ulRawButtons;
            [FieldOffset(12)]
            public int lLastX;
            [FieldOffset(16)]
            public int lLastY;
            [MarshalAs(UnmanagedType.U4)][FieldOffset(20)]
            public uint ulExtraInformation;
        }
        [StructLayout(LayoutKind.Sequential)]
        internal struct RAWKEYBOARD
        {
            [MarshalAs(UnmanagedType.U2)]
            public ushort MakeCode;
            [MarshalAs(UnmanagedType.U2)]
            public ushort Flags;
            [MarshalAs(UnmanagedType.U2)]
            public ushort Reserved;
            [MarshalAs(UnmanagedType.U2)]
            public ushort VKey;
            [MarshalAs(UnmanagedType.U4)]
            public uint Message;
            [MarshalAs(UnmanagedType.U4)]
            public uint ExtraInformation;
        }
        [StructLayout(LayoutKind.Sequential)]
        internal struct RAWINPUTDEVICE
        {
            [MarshalAs(UnmanagedType.U2)]
            public ushort usUsagePage;
            [MarshalAs(UnmanagedType.U2)]
            public ushort usUsage;
            [MarshalAs(UnmanagedType.U4)]
            public int dwFlags;
            public IntPtr hwndTarget;
        };
        #endregion structs & enums

namespace ScreenshotInject
{

internal class DXHookDi: BaseDXHook
{
        public DXHookDi(ScreenshotInterface.ScreenshotInterface ssInterface) : base(ssInterface) {}
        RAWINPUT inp;
        int slotNumber, shift;
        //int getStructure = 0;
        int sendDataNumber = 0;
        System.Drawing.Image imag;
        IntPtr keybHandle, faddr; 
        LocalHook inputHook = null;

        protected override string HookName { get{return "DXHookDi";} }
        
        public override void Hook()
        {
            this.DebugMessage("inputHook: Begin" + DateTime.Now.ToString() + ":" + DateTime.Now.Millisecond.ToString());
            faddr = LocalHook.GetProcAddress("User32.dll", "GetRawInputData");
            inputHook = LocalHook.Create(faddr, new inputHookDelegate(inputHookSub),this);
            inputHook.ThreadACL.SetExclusiveACL(new Int32[1]);
            foreach (RAWINPUTDEVICELIST d in GetAllRawDevices())
            { 
                this.DebugMessage("found handle: " + d.hDevice.ToString() + " type:" + d.dwType.ToString());
                if (d.dwType == 1) { keybHandle = d.hDevice; this.DebugMessage("got: " + d.hDevice); }
            }
            inp.header.dwType = 1;
            inp.header.wParam = 0; //0=foreground 1=background
            inp.header.hDevice = keybHandle;
            inp.header.dwSize = Marshal.SizeOf(inp);
            inp.keyboard.Flags = 0; //0-key is down, 1-key is up, 2-left version of key, 4-right version
            inp.keyboard.VKey = 0;
            inp.keyboard.Message = 65; //F7
            inp.keyboard.Reserved = 0;
            inp.keyboard.MakeCode = 1; //ponyat chto eto
            //inp.keyboard.ExtraInformation = 7733248; //ponyat chto eto
            this.DebugMessage("inputHook: End" + DateTime.Now.ToString() + ":" + DateTime.Now.Millisecond.ToString());
        }
    
        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        delegate uint inputHookDelegate(IntPtr a, IntPtr b, IntPtr c, IntPtr d, IntPtr e);
   
        uint inputHookSub(IntPtr a, IntPtr b, IntPtr c, IntPtr d, IntPtr e)
        {
            uint res = 0;
            inputHookDelegate del = (inputHookDelegate)Marshal.GetDelegateForFunctionPointer(faddr, typeof(inputHookDelegate));
            
            try
            {
                res = del(a, b, c, d, e);
                //return res;
                /*if (getStructure == 0 && res < 10000 && res != 0)
                {
                    inp = (RAWINPUT)Marshal.PtrToStructure(c, typeof(RAWINPUT));
                    if (inp.header.dwType == 1) { getStructure = 1; this.DebugMessage("got structure"); }
                }*/
                if (idxhookUpdateimg != null)
                {
                    imag = System.Drawing.Image.FromStream(new System.IO.MemoryStream(idxhookUpdateimg));
                    if (imag.Height == 5) { shift = 1; } else { shift = 0; }
                    if (imag.Height == 5 || imag.Height == 6) { slotNumber = imag.Width; imag = null; idxhookUpdateimg = null; sendDataNumber = 1; }
                }
                
                if (res != 0)
                {
                    RAWINPUT inp2 = (RAWINPUT)Marshal.PtrToStructure(c, typeof(RAWINPUT));
                    Marshal.Release(c);
                    if (inp2.header.dwType != 0)
                    {
                        //this.DebugMessage("header t:" + inp2.header.dwType.ToString() + "size: " + inp2.header.dwSize.ToString() + " dev:" + inp2.header.hDevice.ToString() + " wparam:" + inp2.header.wParam.ToString());
                        //this.DebugMessage("keyb message:" + inp2.keyboard.Message.ToString() + " vkey:" + inp2.keyboard.VKey.ToString() + " flag: " + inp2.keyboard.Flags.ToString() + " makecode: " + inp2.keyboard.MakeCode.ToString() + " eInf: " + inp2.keyboard.ExtraInformation.ToString());
                        //this.DebugMessage(inp2.mouse.usFlags.ToString() +" : "+inp2.mouse.ulButtons.ToString());
                        //this.DebugMessage("x:" + inp2.mouse.lLastX.ToString() + "y:" + inp2.mouse.lLastY.ToString());
                    }
                    if (sendDataNumber == 1 && inp2.header.dwType != 0)
                    {
                        if (shift == 1)
                        {
                            inp.keyboard.Message = 42;
                            Marshal.StructureToPtr(inp, c, false);
                            this.DebugMessage("shift down");
                            shift = 2;
                            return res;
                        }
                        this.DebugMessage("header t:" + inp.header.dwType.ToString() + " dev:" + inp.header.hDevice.ToString() + " wParam: " + inp.header.wParam);
                        this.DebugMessage("keyb message:" + inp.keyboard.Message.ToString() + " vkey:" + inp.keyboard.VKey.ToString() + " flag: " + inp.keyboard.Flags.ToString() + " makecode: " + inp.keyboard.MakeCode.ToString() + " eInf: " + inp.keyboard.ExtraInformation.ToString());
                        inp.keyboard.Message = 65; //F7
                        //c = Marshal.AllocHGlobal(Marshal.SizeOf(inp));
                        Marshal.StructureToPtr(inp, c, false);
                        sendDataNumber += 1;
                    }
                    else if (sendDataNumber == 2 && inp2.header.dwType != 0)
                    {
                        this.DebugMessage("Button UP");
                        inp.keyboard.Message = 65601;
                        Marshal.StructureToPtr(inp, c, false);
                        sendDataNumber += 1;
                    }
                    else if (sendDataNumber == 3 && inp2.header.dwType != 0)
                    {
                        if (shift == 2)
                        {
                            inp.keyboard.Message = 65578;
                            Marshal.StructureToPtr(inp, c, false);
                            this.DebugMessage("shift up");
                            shift = 0;
                            return res;
                        }
                        this.DebugMessage("Selecting slot...");
                        inp.keyboard.Message = (uint)slotNumber + 1; //"1" = 2, "9" = 10, "0" = 11
                        Marshal.StructureToPtr(inp, c, false);
                        sendDataNumber += 1;
                    }
                    else if (sendDataNumber == 4 && inp2.header.dwType != 0)
                    {
                        this.DebugMessage("QUICKSTATED");
                        inp.keyboard.Message = (uint)slotNumber + 65537; //"1" = 65538, "9" = 10, "0" = 11
                        Marshal.StructureToPtr(inp, c, false);
                        sendDataNumber += 1;
                    }
                }
            }
            catch (Exception ex) { this.DebugMessage("ERROR input: " + ex.Message);}
            return res;
        }

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern uint GetRawInputDeviceList
        ([In, Out] IntPtr RawInputDeviceList, ref uint NumDevices, uint Size);
        public static RAWINPUTDEVICELIST[] GetAllRawDevices()
        {
            uint deviceCount = 0;
            uint dwSize = (uint)Marshal.SizeOf(typeof(RAWINPUTDEVICELIST));

            // First call the system routine with a null pointer
            // for the array to get the size needed for the list
            uint retValue = GetRawInputDeviceList(IntPtr.Zero, ref deviceCount, dwSize);

            // If anything but zero is returned, the call failed, so return a null list
            if (0 != retValue)
                return null;

            // Now allocate an array of the specified number of entries
            RAWINPUTDEVICELIST[] deviceList = new RAWINPUTDEVICELIST[deviceCount];
            IntPtr pRawInputDeviceList = Marshal.AllocHGlobal((int)(dwSize * deviceCount));

            // Now make the call again, using the array
            GetRawInputDeviceList(pRawInputDeviceList, ref deviceCount, dwSize);

            // Fill up the array with the structures
            for (int i = 0; i < deviceCount; i++)
            {
                deviceList[i] = (RAWINPUTDEVICELIST)Marshal.PtrToStructure(
                    new IntPtr((pRawInputDeviceList.ToInt32() + (dwSize * i))),
                    typeof(RAWINPUTDEVICELIST));
            }

            // Free up the memory we first got the information into as
            // it is no longer needed, since the structures have been
            // copied to the deviceList array.
            Marshal.FreeHGlobal(pRawInputDeviceList);

            // Finally, return the filled in list
            return deviceList;
        }
    public override void Cleanup() {}
}
}
