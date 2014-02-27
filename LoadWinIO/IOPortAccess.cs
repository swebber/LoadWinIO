using System;
using System.Runtime.InteropServices;

namespace LoadWinIO
{
    public unsafe class IOPortAccess
    {
        [DllImport("kernel32.dll")]
        private extern static IntPtr LoadLibrary(String DllName);

        [DllImport("kernel32.dll")]
        private extern static IntPtr GetProcAddress(IntPtr hModule, String ProcName);

        [DllImport("kernel32")]
        private extern static bool FreeLibrary(IntPtr hModule);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate bool InitializeWinIoType();

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate bool GetPortValType(UInt16 PortAddr, UInt32* pPortVal, UInt16 Size);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate bool SetPortValType(UInt16 PortAddr, UInt32 PortVal, UInt16 Size);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate bool ShutdownWinIoType();

        private IntPtr hMod;
        private GetPortValType GetPortVal = null;
        private SetPortValType SetPortVal = null;

        public IOPortAccess()
        {
            switch (IntPtr.Size)
            {
                case 4:
                    hMod = LoadLibrary("WinIo32.dll");
                    break;

                case 8:
                    hMod = LoadLibrary("WinIo64.dll");
                    break;

                default:
                    hMod = IntPtr.Zero;
                    break;
            }

            if (hMod == IntPtr.Zero)
            {
                string msg = "Can't find WinIo.dll. Make sure the WinIo.dll library files are located in the same directory as your executable file.";
                Console.WriteLine(msg);
                // throw new Exception(msg);
            }

            IntPtr pFunc = GetProcAddress(hMod, "InitializeWinIo");
            if (pFunc != IntPtr.Zero)
            {
                InitializeWinIoType InitializeWinIo = (InitializeWinIoType)Marshal.GetDelegateForFunctionPointer(pFunc, typeof(InitializeWinIoType));
                bool result = InitializeWinIo();

                if (!result)
                {
                    FreeLibrary(hMod);
                    hMod = IntPtr.Zero;

                    string msg = "Error returned from InitializeWinIo. Make sure you are running with administrative privilages are that WinIo library files are located in the same directory as your executable file.";
                    Console.WriteLine(msg);
                    // throw new Exception(msg);
                }
            }
        }

        public void Close()
        {
            if (hMod != IntPtr.Zero)
            {
                IntPtr pFunc = GetProcAddress(hMod, "ShutdownWinIo");
                if (pFunc != IntPtr.Zero)
                {
                    this.GetPortVal = null;
                    this.SetPortVal = null;

                    ShutdownWinIoType ShutdownWinIo = (ShutdownWinIoType)Marshal.GetDelegateForFunctionPointer(pFunc, typeof(ShutdownWinIoType));
                    ShutdownWinIo();

                    FreeLibrary(hMod);
                    hMod = IntPtr.Zero;
                }
            }
        }

        private UInt32 GetPortValue(UInt16 portAddress)
        {
            UInt32 portValue = 0;

            if (hMod != IntPtr.Zero)
            {
                if (this.GetPortVal == null)
                {
                    IntPtr pFunc = GetProcAddress(hMod, "GetPortVal");
                    if (pFunc != IntPtr.Zero)
                    {
                        this.GetPortVal = (GetPortValType)Marshal.GetDelegateForFunctionPointer(pFunc, typeof(GetPortValType));
                    }
                    else
                    {
                        string msg = "GetPortValue(): Unable to lcoate procedure address.";
                        Console.WriteLine(msg);
                        // throw new Exception(msg);
                    }
                }

                if (!this.GetPortVal(portAddress, &portValue, 1))
                {
                    string msg = "GetPortValue(): Error returned from GetPortVal()";
                    Console.WriteLine(msg);
                    // throw new Exception(msg);
                }
            }

            return portValue;
        }

        private void SetPortValue(UInt16 portAddress, UInt32 portValue)
        {
            if (hMod != IntPtr.Zero)
            {
                if (this.SetPortVal == null)
                {
                    IntPtr pFunc = GetProcAddress(this.hMod, "SetPortVal");
                    if (pFunc != IntPtr.Zero)
                    {
                        this.SetPortVal = (SetPortValType)Marshal.GetDelegateForFunctionPointer(pFunc, typeof(SetPortValType));
                    }
                    else
                    {
                        string msg = "SetPortValue(): Unable to locate procedure address.";
                        Console.WriteLine(msg);
                        // throw new Exception(msg);
                    }
                }

                bool result = this.SetPortVal(portAddress, portValue, 1);
                if (!result)
                {
                    string msg = "SetPortValue(): Error returned from SetPortVal.";
                    Console.WriteLine(msg);
                    // throw new Exception(msg);
                }
            }
        }

        private bool GetBit(UInt32 data, int index)
        {
            if ((index < 0) || (index > 31))
            {
                string msg = "Bit index must be between 0 and 31.";
                Console.WriteLine(msg);
                // throw new ArgumentOutOfRangeException("index", index, msg);
            }

            UInt32 mask = (UInt32)1 << index;
            UInt32 result = data & mask;

            return (result > 0);
        }

        private UInt32 SetBit(UInt32 data, int index, bool value)
        {
            if ((index < 0) || (index > 31))
            {
                string msg = "Bit index must be between 0 and 31.";
                Console.WriteLine(msg);
                // throw new ArgumentOutOfRangeException("index", index, msg);
            }

            UInt32 result;
            UInt32 mask = (UInt32)1 << index;

            if (value)
            {
                result = data | mask;
            }
            else
            {
                result = data & ~mask;
            }

            return result;
        }
    }
}
