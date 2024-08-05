using System.Runtime.InteropServices;
using CuraConnectionInterface;
using System.Reflection;
namespace CuraEngineNetWrapper;
[UnmanagedFunctionPointerAttribute(CallingConvention.StdCall)]
public delegate IntPtr GetCuraEngineConnectionLibPointer(); //uint64
public delegate void FinalizeCuraEngineConnectionLib(); 

public static class CuraEngineConnectionHelper
{
    private static IntPtr fCuraEngineConnectionNativeHandle = IntPtr.Zero;
    private static IntPtr fArcusNativeHandle = IntPtr.Zero;
    private static ICuraConnectionLibrary fNativeLib = null;
    public static ICuraConnectionLibrary LoadNativeLib(string pathToCura)
    {
        fNativeLib = null;
        string assemblyLocation = Assembly.GetExecutingAssembly().Location;
        //CuraEngineConnection.dll is taken from the launch location, 
        //for debugging you will need to manually set the path to CuraEngineConnection.dll in the release folder,
        //since debugging with Cura installed and a release version of the library is needed
        string pathToCuraEngineConnectionDll = Path.GetDirectoryName(assemblyLocation)+"\\CuraEngineConnection.dll";
        if (!File.Exists(pathToCuraEngineConnectionDll))
          return null;
        int LoadDllError = 0;
        fArcusNativeHandle = NativeLibLoader.LoadDll(pathToCura+"Arcus.dll");
        fCuraEngineConnectionNativeHandle = NativeLibLoader.LoadDll(pathToCuraEngineConnectionDll, out LoadDllError);
        if (fCuraEngineConnectionNativeHandle!=IntPtr.Zero) {
            var getLibPointer = NativeLibLoader.GetProc<GetCuraEngineConnectionLibPointer>(fCuraEngineConnectionNativeHandle, "GetCuraEngineConnectionLibPointer");
            if (getLibPointer != null) {
                var ptr = getLibPointer();
                if (ptr!=System.IntPtr.Zero)
                {
                    return (ICuraConnectionLibrary)Marshal.GetTypedObjectForIUnknown(ptr, typeof(ICuraConnectionLibrary));
                }
            }
        }
        return null;
    }
    public static void FinalizeLib()
    {
        if (fCuraEngineConnectionNativeHandle!=IntPtr.Zero) {
            try {
                // Marshal.FinalReleaseComObject(fNativeLib);
                fNativeLib = null;
                GC.Collect();
                GC.WaitForPendingFinalizers(); 
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
