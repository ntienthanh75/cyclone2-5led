# Cyclone II Digital Joystick LED Project

This project reads five active-low digital joystick/button signals and displays the selected direction/state on four LEDs. The buzzer is explicitly muted.

## Target board

- FPGA: Altera/Intel Cyclone II `EP2C5T144C8`
- I/O standard: `3.3-V LVTTL`
- Quartus II: 13.0 SP1
- USB-Blaster JTAG programming
- Shared board details: `D:\fpga\5LED\docs\board-spec.md`

## Pin assignments

| Signal | FPGA pin |
|---|---:|
| Clock | 17 |
| LED[0] | 8 |
| LED[1] | 9 |
| LED[2] | 24 |
| LED[3] | 25 |
| BUZZ | 4 |
| DATA[0] | 137 |
| DATA[1] | 139 |
| DATA[2] | 141 |
| DATA[3] | 142 |
| DATA[4] | 143 |

`DATA[0..4]` use weak pull-ups and are active-low. This design is for a digital joystick/button interface. Analog `VRx`/`VRy` signals require an ADC and cannot be connected directly to these FPGA inputs.

## Build

Open `joystick.qpf` in Quartus II, or run:

```powershell
& 'D:\Program\altera\13.0sp1\quartus\bin64\quartus_sh.exe' --flow compile 'D:\fpga\EP2C5-Verilog-VHDL\EP2C5-Verilog-VHDL\vhdl\JOYSTICK\joystick.qpf'
```

The generated file is `joystick.sof` in this project directory.

## Download to the board

1. Power the board and connect the USB-Blaster.
2. Verify the JTAG chain:

   ```powershell
   & 'D:\Program\altera\13.0sp1\quartus\bin64\jtagconfig.exe'
   ```

3. Program the project:

   ```powershell
   & 'D:\Program\altera\13.0sp1\quartus\bin64\quartus_pgm.exe' -c 'USB-Blaster [USB-0]' -m JTAG -o 'p;D:\fpga\EP2C5-Verilog-VHDL\EP2C5-Verilog-VHDL\vhdl\JOYSTICK\joystick.sof'
   ```

The `.sof` configuration is temporary and is lost after power-off.
