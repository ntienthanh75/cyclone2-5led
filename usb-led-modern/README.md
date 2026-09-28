# CoreEP2C5 WinUSB LED controller

This repository is a modern Windows replacement for Waveshare's old `USB_LED.exe` demo. It controls the LEDs on the Waveshare CoreEP2C5 board through the CY7C68013A USB-FIFO board.

## Target hardware

- Board: Waveshare CoreEP2C5 / OpenEP2C5-C family
- FPGA: Altera/Intel Cyclone II `EP2C5T144C8N`
- USB controller: Cypress/Infineon `CY7C68013A` EZ-USB FX2
- FPGA USB connector: `32I/Os_1`
- FPGA device used by Quartus: `EP2C5T144C8`
- Quartus programming cable: Altera USB-Blaster

The Waveshare manual specifies this USB board and connector for the USB communication demo. The FPGA design is the existing project at:

`D:\fpga\EP2C5-Verilog-VHDL\EP2C5-Verilog-VHDL\vhdl\USB\FPGA_USB`

## Why this replacement was needed

The original `D:\fpga\USB-VC\USB VC\Release\USB_LED.exe` opens the legacy device path `\\.\EZUSB-0` and sends Cypress `IOCTL_EZUSB_BULK_WRITE` requests. Its matching `ezusb.sys` driver is an obsolete legacy driver. The local Waveshare `D:\fpga\EZ-USB\USB Driver\EZ-USB.exe` is only the old utility and is not a complete modern Windows 11 driver package.

Windows 11 initially reported:

`EZ-USB FX2 — CM_PROB_FAILED_INSTALL`

The modern solution is to bind the device to the Windows inbox `WINUSB` service and send the same one-byte bulk commands using the WinUSB API.

### Exact resolution of the problem

The problem was not the FPGA, the USB cable, or the CY7C68013A chip. The problem was the host-side Windows driver interface:

1. Windows detected the board as `USB\VID_0547&PID_1002`.
2. Windows initially had no matching driver and reported Code 28 / `CM_PROB_FAILED_INSTALL`.
3. The Waveshare `EZ-USB.exe` file is an old utility, not a complete Windows 11 x64 driver package.
4. The original Waveshare `USB_LED.exe` was compiled for the legacy Cypress `\\.\EZUSB-0` kernel interface.
5. The legacy `ezusb.sys` approach is obsolete and unsuitable for normal Windows 11 x64 use.
6. A WinUSB INF was created specifically for `USB\VID_0547&PID_1002`, using the Windows inbox `WINUSB` service.
7. Because Windows rejected automatic staging of the unsigned custom INF, the device was manually changed in Device Manager to **Universal Serial Bus devices -> WinUSB**.
8. The USB board was unplugged and reconnected so Windows registered the new interface.
9. A new x64 host program was written using the Windows WinUSB API. It opens the registered interface, finds the bulk OUT endpoint, and sends the same command bytes as the original application.
10. The FPGA `USB_LED.sof` design was programmed through the Altera USB-Blaster.
11. The final program successfully transmitted commands 1, 2, 3, 4, 5, and 6, each with exit code 0.

This means the issue was resolved by replacing the obsolete `EZUSB-0` host/driver combination with WinUSB plus a new compatible host application. The FPGA USB design and the LED command protocol were retained.

## Files created

- `UsbLedModern.csproj` — build project
- `Program.cs` — WinUSB host application
- `CoreEP2C5-WinUSB.inf` — device binding for `USB\VID_0547&PID_1002`
- `publish\UsbLedModern.exe` — compiled x64 executable

All new files and the build output are under:

`D:\Programs\usb-led-modern`

## New files: what they are and how they were created

These are the files created specifically for the Windows 11 WinUSB solution. They are separate from the original Waveshare files under `D:\fpga`.

### `UsbLedModern.csproj`

This is the C# build-project file. It defines an x64 Windows executable and includes `Program.cs`. It targets the .NET Framework 4.0 reference assemblies already present on the computer, so no new SDK was installed on `C:`.

### `Program.cs`

This is the new host application source code. It was written for this task using the Windows APIs:

- `SetupAPI` to locate the registered USB interface for `VID_0547/PID_1002`
- `CreateFile` to open the USB interface
- `WinUsb_Initialize` to attach to WinUSB
- `WinUsb_QueryPipe` to find the bulk OUT endpoint
- `WinUsb_WritePipe` to send one LED command byte

The source preserves the original FPGA protocol found in the old Waveshare C++ source: command bytes `1–6` are sent through USB pipe 2 / bulk OUT. It also includes the Windows x64 overlapped-I/O requirement and a registry fallback for locating the interface path.

### `CoreEP2C5-WinUSB.inf`

This is the driver-binding description created for this board. It matches only:

`USB\VID_0547&PID_1002`

It tells Windows to use the built-in `WINUSB` service and registers the application’s device-interface GUID. The INF does not contain a custom kernel driver; it uses the Microsoft Windows inbox WinUSB driver.

### `publish\UsbLedModern.exe`

This is the compiled x64 program that you run. It was built from `Program.cs` using the Visual Studio 2019 Build Tools already installed on the computer.

The exact build command was:

```powershell
& 'C:\Program Files (x86)\Microsoft Visual Studio\2019\BuildTools\MSBuild\Current\Bin\MSBuild.exe' `
  'D:\Programs\usb-led-modern\UsbLedModern.csproj' `
  /p:Configuration=Release `
  /p:OutputPath='D:\Programs\usb-led-modern\publish\' /m
```

No source files were changed in the original Waveshare USB demo. The original `USB_LED.exe` remains at `D:\fpga\USB-VC\USB VC\Release\USB_LED.exe`; the new program is a separate replacement because the old program requires `EZUSB-0`.

## FPGA programming

1. Connect the Altera USB-Blaster to the main CoreEP2C5 board.
2. Connect the CY7C68013A USB board to the FPGA `32I/Os_1` connector.
3. Open Quartus Programmer.
4. Select the Altera USB-Blaster/JTAG chain containing `EP2C5`.
5. Load this bitstream:

`D:\fpga\EP2C5-Verilog-VHDL\EP2C5-Verilog-VHDL\vhdl\USB\FPGA_USB\USB_LED.sof`

6. Program the FPGA.

The USB design uses the FPGA pins assigned in `USB_LED.qsf`, including the four active-low onboard LEDs and the muted buzzer output.

## Windows driver installation

The custom INF was placed at:

`D:\Programs\usb-led-modern\CoreEP2C5-WinUSB.inf`

Automatic `pnputil` staging was rejected because the custom INF is not signed. The driver was installed manually:

1. Open Device Manager.
2. Right-click `EZ-USB FX2`.
3. Select **Update driver**.
4. Select **Browse my computer for drivers**.
5. Select **Let me pick from a list of available drivers**.
6. Select **Universal Serial Bus devices**.
7. Select **WinUSB** and confirm.
8. Unplug and reconnect the CY7C68013A USB cable after changing the driver.

Verified device state after installation:

- Status: `OK`
- Problem: `CM_PROB_NONE`
- Service: `WINUSB`
- Installed package: `oem111.inf`
- Hardware ID: `USB\VID_0547&PID_1002`

## LED protocol

The FPGA USB design reads the low four bits of each 16-bit FIFO word and interprets these command values:

- `1` — LED1
- `2` — LED2
- `3` — LED3
- `4` — LED4
- `5` — running LED pattern
- `6` — reset/off

The new host program discovers the WinUSB interface, finds the bulk OUT endpoint, and sends exactly one command byte. The original source labels this endpoint as pipe 0 / USB pipe 2.

## Running the replacement

From PowerShell:

```powershell
& 'D:\Programs\usb-led-modern\publish\UsbLedModern.exe' 1
& 'D:\Programs\usb-led-modern\publish\UsbLedModern.exe' 2
& 'D:\Programs\usb-led-modern\publish\UsbLedModern.exe' 3
& 'D:\Programs\usb-led-modern\publish\UsbLedModern.exe' 4
& 'D:\Programs\usb-led-modern\publish\UsbLedModern.exe' 5
& 'D:\Programs\usb-led-modern\publish\UsbLedModern.exe' 6
```

The program targets x64 Windows and was compiled with the Visual Studio 2019 Build Tools already available on the computer. No new program was installed on `C:`.

## Can the original USB_LED.exe still be used?

No, not directly on this Windows 11 x64 installation. The original file:

`D:\fpga\USB-VC\USB VC\Release\USB_LED.exe`

expects the old `\\.\EZUSB-0` device interface. The board is now correctly bound to WinUSB, which exposes a different API and device interface. Installing WinUSB does not make the old executable compatible.

Use this replacement instead:

`D:\Programs\usb-led-modern\publish\UsbLedModern.exe`

Example:

```powershell
& 'D:\Programs\usb-led-modern\publish\UsbLedModern.exe' 1
```

The command-line program replaces the six buttons of the old graphical utility:

| Command | Function |
|---:|---|
| `1` | Select LED1 |
| `2` | Select LED2 |
| `3` | Select LED3 |
| `4` | Select LED4 |
| `5` | Start the running LED pattern |
| `6` | Reset LEDs/off |

Each execution sends one byte to the FPGA USB-FIFO interface and exits. The program prints `Sent command N.` when the USB transfer succeeds.

For a graphical replacement later, the same WinUSB code can be placed behind six buttons in a Windows Forms application. The current command-line version is the tested reference implementation.

## Troubleshooting history

- Quartus/JTAG was verified separately with the Altera USB-Blaster and `EP2C5` JTAG device.
- The Cypress board first appeared as `EZ-USB FX2` with hardware ID `VID_0547&PID_1002` and Code 28.
- The local `EZ-USB.exe` was inspected and found to be an unsigned legacy utility without `.inf` or `.sys` driver files.
- The obsolete Cypress `ezusb.sys` route was not used on Windows 11 x64.
- A public Cypress `cyusb.sys` reference package was inspected but not installed because it would not provide the old `EZUSB-0` interface required by the original application.
- The replacement executable built successfully.
- The WinUSB binding was installed manually and Windows reported the device as `OK` with service `WINUSB`.
- If the replacement reports that the device is not found immediately after driver installation, unplug and reconnect the CY7C68013A board so Windows refreshes the device-interface registration.
- The first runtime test required one additional Windows x64 fix: use overlapped I/O when opening the WinUSB interface. The final executable includes this fix and reads the registered device-interface path from the Windows registry before falling back to SetupAPI enumeration.
- Final hardware communication test completed successfully: commands `1`, `2`, `3`, `4`, `5`, and `6` each returned exit code `0`.

## References

- Waveshare CoreEP2C5 documentation: https://www.waveshare.com/wiki/CoreEP2C5
- Waveshare OpenEP2C5-C user manual: https://www.waveshare.com/wiki/OpenEP2C5-C_User_Manual
- Waveshare Altera software page: https://www.waveshare.com/wiki/Altera_Software
- Infineon legacy EZ-USB driver information: https://community.infineon.com/t5/Knowledge-Base-Articles/Source-Code-of-ezusb-sys/ta-p/249684
