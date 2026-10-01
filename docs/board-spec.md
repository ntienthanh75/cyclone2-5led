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

## LCD and touch module

The attached module is the 3.2-inch 320x240 TFT LCD with a 16-bit parallel
ILI9325-style display interface and an SPI resistive touch controller. These
assignments are confirmed by the existing `lcd_photo_hdl` and `lcd_touch`
Quartus projects.

### LCD parallel interface

| Signal | FPGA pin |
|---|---:|
| `LCD_DATA[0..15]` | 93, 94, 96, 97, 99, 100, 101, 103, 104, 112, 113, 114, 115, 118, 119, 120 |
| `LCD_CS` | 121 |
| `LCD_RS` / command-data | 122 |
| `LCD_WR` | 125 |
| `LCD_RD` | 126 |
| `LCD_RST` | 132 |

The LCD data bus is bidirectional. Display-write designs normally keep
`LCD_RD` high and drive the bus only during write cycles.

### Touch SPI interface

| Signal | FPGA pin |
|---|---:|
| `TOUCH_CS` | 134 |
| `TOUCH_IRQ` | 129 |
| `TOUCH_SCLK` | 133 |
| `TOUCH_MOSI` | 136 |
| `TOUCH_MISO` | 135 |

The touch controller is separate from the LCD pixel bus and requires its own
SPI timing and IRQ handling.

## CY7C68013A FX2 USB FIFO

The separate CY7C68013A USB board is a runtime data interface, not the
USB-Blaster programming interface. The FPGA-side FIFO assignments are
confirmed by the existing USB LED project and the ML bridge:

| Signal | FPGA pin |
|---|---:|
| `clk` | 17 |
| `rst` | 88 |
| `FIFOADR[0]`, `FIFOADR[1]` | 75, 76 |
| `FIFODATA[0..15]` | 81, 86, 87, 92, 93, 94, 96, 97, 99, 100, 101, 103, 104, 112, 113, 114 |
| `SLWR` | 73 |
| `SLOE` | 72 |
| `SLRD` | 74 |
| `FLAGA` | 71 |
| `FLAGB` | 79 |
| `FLAGC` | 80 |

The current ML protocol selects EP2 OUT with `FIFOADR=00` for PC-to-FPGA
frames and EP6 IN with `FIFOADR=10` for FPGA-to-PC results. Windows detects
the board as `EZ-USB FX2`, VID `0547`, PID `1002`, with WinUSB interface GUID
`{8f2f6d1e-5c4c-4bc4-a2d2-6b5a6fce7a21}`.

## SDRAM expansion board

The attached SDRAM board is used by the Nios/LCD design. Its interface
contains a 16-bit `DQ` bus, row/address `RA`, bank address `BA[1:0]`, and
`RAS`, `CAS`, `WE`, `CS`, `CKE`, `S_CLK`, and byte-mask signals. The complete
SDRAM assignments remain in the existing `lcd_touch/synthesis/lcd_nios.qsf`
project. Do not reuse those pins for LCD, FX2, or UART in the same Quartus
revision.

## UART status

UART is an optional runtime interface. RX and TX pins are project-specific and
must be explicitly assigned in each QSF and README. Do not assume that LCD or
FX2 pins are UART pins.

## Design rules

1. USB-Blaster/JTAG configures the FPGA; it is not automatically a runtime
   file-transfer channel.
2. The FX2 USB FIFO, LCD/touch, SDRAM, UART, and joystick pins must not be
   combined in one revision without checking for conflicts.
3. Keep the buzzer muted by default by driving the active-low buzzer output
   high.
4. Each repository README should link to this canonical document instead of
   copying a second board specification.

## Canonical document

This file is the shared board specification for the private GitHub Project
**Cyclone II FPGA Board**. Other repositories should link to:

<https://github.com/ntienthanh75/fpga-cyclone2-5led/blob/main/docs/board-spec.md>
