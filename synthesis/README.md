# 5LED synthesis experiment

Target: Waveshare/CoreEP2C5, Cyclone II `EP2C5T144C8`.

This experiment is a clean 50 MHz constrained compile of the standalone `five_leds` design. Quartus II 13.0 SP1 completed fit, assembly, and TimeQuest with 0 errors.

| Result | Value |
|---|---:|
| Logic elements | 62 / 4,608 (1%) |
| Pins | 7 / 89 (8%) |
| Slow setup slack | +11.042 ns |
| Slow hold slack | +0.499 ns |
| Fast setup slack | +17.211 ns |
| Fast hold slack | +0.215 ns |

The source constraint is [`five_leds.sdc`](five_leds.sdc). Reports and the SOF are under [`experiments/50MHz`](experiments/50MHz). The build still warns about internally divided clocks (`clk1` and `clk2`) because this small design does not describe those generated clocks.

## Clock sweep

| Constraint | Slow setup | Slow hold | Fit |
|---:|---:|---:|:---:|
| 40 MHz | +16.059 ns | +0.499 ns | Pass |
| 50 MHz | +11.042 ns | +0.499 ns | Pass |
| 60 MHz | +7.744 ns | +0.499 ns | Pass |
| 75 MHz | +4.527 ns | +0.499 ns | Pass |
| 100 MHz | +2.722 ns | +0.499 ns | Pass |

All tested constraints fit. The experiments are in `experiments/clk_*MHz`.
