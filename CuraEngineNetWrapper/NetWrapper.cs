using System.Runtime.InteropServices;
using CuraConnectionInterface;
using System.Reflection;
using CAMAPI.DotnetHelper;
namespace CuraEngineNetWrapper;
[UnmanagedFunctionPointerAttribute(CallingConvention.StdCall)]
public delegate IntPtr GetCuraEngineConnectionLibPointer(); //uint64
public delegate void FinalizeCuraEngineConnectionLib(); 

public static class CuraEngineConnectionHelper
{
    private static IntPtr fCuraEngineConnectionNativeHandle = IntPtr.Zero;
    private static IntPtr fArcusNativeHandle = IntPtr.Zero;
    public static ComWrapper<ICuraConnectionLibrary>? LoadNativeLib(string pathToCura)
    {
        string assemblyLocation = Assembly.GetExecutingAssembly().Location;
        //CuraEngineConnection.dll is taken from the launch location, 
        //for debugging you will need to build the whole project in debug and manually set the path to CuraEngineConnection.dll and Arcus.dll in the debug folder
        string pathToCuraEngineConnectionDll = Path.GetDirectoryName(assemblyLocation)+"\\CuraEngineConnection.dll";
        //string pathToCuraEngineConnectionDll = "D:/sources/planar-slicing-extension/CuraEngineConnection/build/Debug/CuraEngineConnection.dll";
        if (!File.Exists(pathToCuraEngineConnectionDll))
          return null;
        int LoadDllError = 0;
        fArcusNativeHandle = NativeLibLoader.LoadDll(pathToCura+"Arcus.dll");
        //fArcusNativeHandle = NativeLibLoader.LoadDll("D:/sources/planar-slicing-extension/CuraEngineConnection/build/Debug/Arcus.dll");
        fCuraEngineConnectionNativeHandle = NativeLibLoader.LoadDll(pathToCuraEngineConnectionDll, out LoadDllError);
        if (fCuraEngineConnectionNativeHandle!=IntPtr.Zero) {
            var getLibPointer = NativeLibLoader.GetProc<GetCuraEngineConnectionLibPointer>(fCuraEngineConnectionNativeHandle, "GetCuraEngineConnectionLibPointer");
            if (getLibPointer != null) {
                var ptr = getLibPointer();
                if (ptr!=System.IntPtr.Zero)
                {
                    var Lib = new ComWrapper<ICuraConnectionLibrary>(ptr);
                    return Lib;
                }
            }
        }
        return null;
    }
    public static void FinalizeLib()
    {
        if (fCuraEngineConnectionNativeHandle!=IntPtr.Zero) {
            try {
                // GC.Collect();
                // GC.WaitForPendingFinalizers(); 
                var finalizeLib = NativeLibLoader.GetProc<FinalizeCuraEngineConnectionLib>(fCuraEngineConnectionNativeHandle, "FinalizeCuraEngineConnectionLib");
                if (finalizeLib != null) {
                    finalizeLib();
                    NativeLibLoader.FreeDll(fCuraEngineConnectionNativeHandle);
                }
                if (fArcusNativeHandle!=IntPtr.Zero)
                {
                    NativeLibLoader.FreeDll(fArcusNativeHandle);
                }
            } catch 
            {}
        }    
    }
}
