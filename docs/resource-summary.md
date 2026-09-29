# Cyclone II FPGA Board — shared resource summary

This public document summarizes the linked Cyclone II FPGA designs. The GitHub Project itself is private, so this file provides a visitor-readable copy of the shared results.

## Common target

- Board: Waveshare/CoreEP2C5
- FPGA: Intel/Altera Cyclone II `EP2C5T144C8`
- Board clock: 50 MHz (`20 ns`)
- Quartus version: Quartus II 13.0 SP1

## Synthesis resource comparison

| Repository/design | Logic elements | Pins | Memory bits | PLL/DSP | 50 MHz setup slack | Clock-sweep result |
|---|---:|---:|---:|---:|---:|---|
| [cyclone2-5led](https://github.com/ntienthanh75/cyclone2-5led) | 62 / 4,608 (1%) | 7 / 89 (8%) | 0 (0%) | 0 / 0 | +11.042 ns | Pass through 100 MHz |
| [cyclone2-fsm](https://github.com/ntienthanh75/cyclone2-fsm) | 60 / 4,608 (1%) | 7 / 89 (8%) | 0 (0%) | 0 / 0 | +10.964 ns | Pass through 100 MHz |
| [cyclone2-adders-timing](https://github.com/ntienthanh75/cyclone2-adders-timing) | 35 / 4,608 (<1%) | 35 / 89 (39%) | 0 (0%) | 0 / 0 | +14.102 ns | Pass through 100 MHz |
| Joystick design in cyclone2-5led | 6 / 4,608 (<1%) | 12 / 89 (13%) | 0 (0%) | 0 / 0 | Not meaningful | `CLK` is unused in the current RTL |
| [cyclone2-lcd-nios](https://github.com/ntienthanh75/cyclone2-lcd-nios) | 4,282 / 4,608 (93%) | 78 / 89 (88%) | 33,792 / 119,808 (28%) | 1 / 4 | +5.287 ns | Pass at 40–75 MHz; 100 MHz fails setup (-0.999 ns) |

## Interpretation

`cyclone2-lcd-nios` is the resource-heavy design and is the limiting shared hardware case. The recommended programming constraint is 50 MHz. The 100 MHz LCD/Nios image fits the FPGA but fails setup timing and must not be programmed.

The joystick build fits the FPGA, but its current RTL does not use the `CLK` input. A synchronous RTL revision is required before its clock timing can be evaluated.

Detailed reports and clock-sweep experiments are stored in each repository's `synthesis` folder. The `lcd_photo` experiment is intentionally excluded.
