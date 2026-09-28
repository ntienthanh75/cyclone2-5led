# Shared FPGA Board Specification

## Target board

These projects target the Cyclone II development board fitted with:

- FPGA: Altera/Intel Cyclone II `EP2C5T144C8`
- Package: `T144`
- I/O voltage: `3.3-V LVTTL`
- System clock: board clock on FPGA pin `17`
- Programming interface: Altera USB-Blaster through the JTAG header
- Configuration used here: JTAG/SRAM configuration from a `.sof` file

The `.sof` configuration is volatile. It is lost when the board is powered off or reconfigured. Use a `.pof` only when configuring nonvolatile configuration memory and only after verifying the board's configuration-device wiring.

## Existing board connections

The basic LED outputs used by these projects are:

| Signal | FPGA pin | Notes |
|---|---:|---|
| LED 0 | 8 | Board LED output |
| LED 1 | 9 | Board LED output |
| LED 2 | 24 | Board LED output |
| LED 3 | 25 | Board LED output |
| LED 4 | 7 | Used by the 5LED project |
| Clock | 17 | Board clock input |
| Buzzer | 4 | Active-low; drive `1` to mute |

The joystick project uses five active-low digital inputs:

| Signal | FPGA pin |
|---|---:|
| DATA[0] | 137 |
| DATA[1] | 139 |
| DATA[2] | 141 |
| DATA[3] | 142 |
| DATA[4] | 143 |

The joystick inputs have weak pull-ups enabled. Do not connect analog joystick outputs such as `VRx` or `VRy` directly to these FPGA pins; an ADC or digital joystick interface is required for analog modules.

## Programming from PowerShell

With the USB-Blaster connected and the board powered:

```powershell
& 'D:\Program\altera\13.0sp1\quartus\bin64\jtagconfig.exe'
& 'D:\Program\altera\13.0sp1\quartus\bin64\quartus_pgm.exe' `
  -c 'USB-Blaster [USB-0]' -m JTAG `
  -o 'p;D:\fpga\5LED\output_files\five_leds.sof'
```

The expected JTAG device is `EP2C5` with ID code `020B10DD`.
