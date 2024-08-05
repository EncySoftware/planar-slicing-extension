using System;
using System.Runtime.InteropServices;

namespace CuraEngineNetWrapper
{

    public static class NativeLibLoader {


        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr LoadLibrary(string libname);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern bool FreeLibrary(IntPtr hModule);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName); 

        public static IntPtr LoadDll(string libName) 
        {
            IntPtr h = LoadLibrary(libName);
            return h;
        }

        public static IntPtr LoadDll(string libName, out int errorCode) 
        {
            errorCode = 0;
            IntPtr h = LoadLibrary(libName);
            if (h == IntPtr.Zero)
                errorCode = Marshal.GetLastWin32Error();
            return h;
        }
        
        public static void FreeDll(IntPtr handle)
        {
            if(handle != IntPtr.Zero)
                FreeLibrary(handle);
        }
        
        public static delegateT GetProc<delegateT>(IntPtr dllHandle, string procName) where delegateT : class
        {
            delegateT result = default(delegateT);
            IntPtr procAdrr = GetProcAddress(dllHandle, procName);
            if (procAdrr!=IntPtr.Zero)
                result = Marshal.GetDelegateForFunctionPointer(procAdrr, typeof(delegateT)) as delegateT;
            return result;
        }

    }



}