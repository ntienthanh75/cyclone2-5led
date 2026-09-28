using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using Microsoft.Win32;

internal static class Program
{
    private static readonly Guid InterfaceGuid = new("8f2f6d1e-5c4c-4bc4-a2d2-6b5a6fce7a21");

    public static int Main(string[] args)
    {
        if (args.Length != 1 || !byte.TryParse(args[0], out var command) || command is < 1 or > 6)
        {
            Console.WriteLine("Usage: UsbLedModern.exe <1..6>");
            Console.WriteLine("1-4 LED1-LED4, 5 running pattern, 6 reset/off");
            return 2;
        }
        try
        {
            using var device = WinUsbDevice.Open(InterfaceGuid);
            device.Write(command);
            Console.WriteLine($"Sent command {command}.");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            Console.Error.WriteLine("Load USB_LED.sof first and connect the CY7C68013A board to 32I/Os_1.");
            return 1;
        }
    }
}

internal sealed class WinUsbDevice : IDisposable
{
    private SafeFileHandle file = null!;
    private IntPtr usb;
    private byte pipe;

    public static WinUsbDevice Open(Guid interfaceGuid)
    {
        var path = Native.FindDevicePath(interfaceGuid);
        var device = new WinUsbDevice { file = Native.CreateFile(path, Native.GENERIC_READ | Native.GENERIC_WRITE, Native.FILE_SHARE_READ | Native.FILE_SHARE_WRITE, IntPtr.Zero, Native.OPEN_EXISTING, Native.FILE_FLAG_OVERLAPPED, IntPtr.Zero) };
        if (device.file.IsInvalid) throw new Win32Exception(Marshal.GetLastWin32Error(), "Could not open the WinUSB device.");
        if (!Native.WinUsb_Initialize(device.file.DangerousGetHandle(), out device.usb)) throw new InvalidOperationException("WinUSB initialization failed (Win32=" + Marshal.GetLastWin32Error() + ").");
        var descriptor = new Native.USB_INTERFACE_DESCRIPTOR();
        if (!Native.WinUsb_QueryInterfaceSettings(device.usb, 0, ref descriptor)) throw new Win32Exception(Marshal.GetLastWin32Error(), "Could not read the USB interface descriptor.");
        for (byte i = 0; i < descriptor.bNumEndpoints; i++)
        {
            var info = new Native.WINUSB_PIPE_INFORMATION();
            if (Native.WinUsb_QueryPipe(device.usb, 0, i, ref info) && info.PipeType == Native.USBD_PIPE_TYPE.Bulk && (info.PipeId & 0x80) == 0) { device.pipe = info.PipeId; break; }
        }
        if (device.pipe == 0) throw new InvalidOperationException("No bulk OUT endpoint was found.");
        return device;
    }

    public void Write(byte command)
    {
        if (!Native.WinUsb_WritePipe(usb, pipe, new[] { command }, 1, out _, IntPtr.Zero)) throw new Win32Exception(Marshal.GetLastWin32Error(), "USB bulk write failed.");
    }

    public void Dispose()
    {
        if (usb != IntPtr.Zero) Native.WinUsb_Free(usb);
        file?.Dispose();
    }
}

internal static class Native
{
    internal const uint DIGCF_PRESENT = 2, DIGCF_DEVICEINTERFACE = 16, GENERIC_READ = 0x80000000, GENERIC_WRITE = 0x40000000, FILE_SHARE_READ = 1, FILE_SHARE_WRITE = 2, OPEN_EXISTING = 3, FILE_FLAG_OVERLAPPED = 0x40000000;
    internal enum USBD_PIPE_TYPE { Control, Isochronous, Bulk, Interrupt }
    [StructLayout(LayoutKind.Sequential)] internal struct SP_DEVICE_INTERFACE_DATA { internal int cbSize; internal Guid InterfaceClassGuid; internal int Flags; internal IntPtr Reserved; }
    [StructLayout(LayoutKind.Sequential, Pack = 1)] internal struct USB_INTERFACE_DESCRIPTOR { internal byte bLength,bDescriptorType,bInterfaceNumber,bAlternateSetting,bNumEndpoints,bInterfaceClass,bInterfaceSubClass,bInterfaceProtocol,iInterface; }
    [StructLayout(LayoutKind.Sequential)] internal struct WINUSB_PIPE_INFORMATION { internal USBD_PIPE_TYPE PipeType; internal byte PipeId; internal ushort MaximumPacketSize; internal byte Interval; }

    internal static string FindDevicePath(Guid guid)
    {
        using (var classes = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Control\\DeviceClasses\\{" + guid.ToString() + "}"))
        {
            if (classes != null)
            {
                foreach (var name in classes.GetSubKeyNames())
                {
                    if (name.IndexOf("VID_0547&PID_1002", StringComparison.OrdinalIgnoreCase) >= 0 && name.StartsWith("##?#", StringComparison.Ordinal))
                        return "\\\\?\\" + name.Substring(4);
                }
            }
        }
        var info = SetupDiGetClassDevs(ref guid, IntPtr.Zero, IntPtr.Zero, DIGCF_PRESENT | DIGCF_DEVICEINTERFACE);
        if (info == IntPtr.Zero || info == new IntPtr(-1)) throw new InvalidOperationException("WinUSB device interface not found. Install the supplied INF first.");
        try
        {
            for (uint i = 0; ; i++)
            {
                var data = new SP_DEVICE_INTERFACE_DATA { cbSize = Marshal.SizeOf(typeof(SP_DEVICE_INTERFACE_DATA)) };
                if (!SetupDiEnumDeviceInterfaces(info, IntPtr.Zero, ref guid, i, ref data)) break;
                SetupDiGetDeviceInterfaceDetail(info, ref data, IntPtr.Zero, 0, out var required, IntPtr.Zero);
                var buffer = Marshal.AllocHGlobal(required);
                try
                {
                    Marshal.WriteInt32(buffer, IntPtr.Size == 8 ? 8 : 5);
                    if (SetupDiGetDeviceInterfaceDetail(info, ref data, buffer, required, out _, IntPtr.Zero))
                    {
                        var path = Marshal.PtrToStringUni(buffer + 4) ?? "";
                        if (path.IndexOf("vid_0547&pid_1002", StringComparison.OrdinalIgnoreCase) >= 0) return path;
                    }
                }
                finally { Marshal.FreeHGlobal(buffer); }
            }
        }
        finally { SetupDiDestroyDeviceInfoList(info); }
        throw new InvalidOperationException("CoreEP2C5 USB device was not found.");
    }

    [DllImport("setupapi.dll", SetLastError=true)] private static extern IntPtr SetupDiGetClassDevs(ref Guid ClassGuid, IntPtr Enumerator, IntPtr hwndParent, uint Flags);
    [DllImport("setupapi.dll", SetLastError=true)] private static extern bool SetupDiEnumDeviceInterfaces(IntPtr DeviceInfoSet, IntPtr DeviceInfoData, ref Guid InterfaceClassGuid, uint MemberIndex, ref SP_DEVICE_INTERFACE_DATA DeviceInterfaceData);
    [DllImport("setupapi.dll", SetLastError=true)] private static extern bool SetupDiGetDeviceInterfaceDetail(IntPtr DeviceInfoSet, ref SP_DEVICE_INTERFACE_DATA DeviceInterfaceData, IntPtr DeviceInterfaceDetailData, int DeviceInterfaceDetailDataSize, out int RequiredSize, IntPtr DeviceInfoData);
    [DllImport("setupapi.dll")] private static extern bool SetupDiDestroyDeviceInfoList(IntPtr DeviceInfoSet);
    [DllImport("kernel32.dll", CharSet=CharSet.Unicode, SetLastError=true)] internal static extern SafeFileHandle CreateFile(string name, uint access, uint share, IntPtr security, uint creation, uint flags, IntPtr template);
    [DllImport("winusb.dll", SetLastError=true)] internal static extern bool WinUsb_Initialize(IntPtr deviceHandle, out IntPtr interfaceHandle);
    [DllImport("winusb.dll", SetLastError=true)] internal static extern bool WinUsb_QueryInterfaceSettings(IntPtr interfaceHandle, byte alternateInterfaceNumber, ref USB_INTERFACE_DESCRIPTOR descriptor);
    [DllImport("winusb.dll", SetLastError=true)] internal static extern bool WinUsb_QueryPipe(IntPtr interfaceHandle, byte alternateInterfaceNumber, byte pipeIndex, ref WINUSB_PIPE_INFORMATION pipeInformation);
    [DllImport("winusb.dll", SetLastError=true)] internal static extern bool WinUsb_WritePipe(IntPtr interfaceHandle, byte pipeId, byte[] buffer, uint bufferLength, out uint lengthTransferred, IntPtr overlapped);
    [DllImport("winusb.dll", SetLastError=true)] internal static extern bool WinUsb_Free(IntPtr interfaceHandle);
}
