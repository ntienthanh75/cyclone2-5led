# Cyclone II 5LED Project

This Quartus II project targets the Cyclone II `EP2C5T144C8` FPGA board and drives five board LEDs. The buzzer is explicitly muted.

## Target board

See [the shared board specification](docs/board-spec.md). The important target is:

- FPGA: Cyclone II `EP2C5T144C8`
- Quartus II: 13.0 SP1
- Programming cable: USB-Blaster

## Pin assignments

- Clock: pin `17`
- LED outputs: pins `8`, `9`, `24`, `25`, and `7`
- Buzzer: pin `4`, held at logic `1` because the buzzer is active-low

## Build

Open `five_leds.qpf` in Quartus II, or run:

```powershell
& 'D:\Program\altera\13.0sp1\quartus\bin64\quartus_sh.exe' --flow compile 'D:\fpga\5LED\five_leds.qpf'
```

The generated file is `output_files\five_leds.sof`.

## Download to the board

1. Power the board and connect the USB-Blaster JTAG cable.
2. Confirm the cable and FPGA:

   ```powershell
   & 'D:\Program\altera\13.0sp1\quartus\bin64\jtagconfig.exe'
   ```

   It should show `USB-Blaster [USB-0]` and `EP2C5`.

3. Program the SRAM configuration:

   ```powershell
   & 'D:\Program\altera\13.0sp1\quartus\bin64\quartus_pgm.exe' -c 'USB-Blaster [USB-0]' -m JTAG -o 'p;D:\fpga\5LED\output_files\five_leds.sof'
   ```

The `.sof` configuration is temporary and is lost after power-off.
