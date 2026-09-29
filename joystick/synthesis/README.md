# Joystick synthesis experiment

Target: Waveshare/CoreEP2C5, Cyclone II `EP2C5T144C8`.

The 50 MHz experiment compiled successfully with 0 errors. The original joystick RTL does not use its `CLK` or `RESET` ports in the process; therefore Quartus removes those inputs and TimeQuest cannot create a real clock from them.

| Result | Value |
|---|---:|
| Logic elements | 6 / 4,608 (<1%) |
| Pins | 12 / 89 (13%) |
| Timing result | No user clock remained after optimization |
| Main warning | `CLK` and `RESET` do not drive logic |

This is a valid fit experiment, but it is not a meaningful clock-timing result until the joystick logic is made synchronous. Reports and the SOF are under [`experiments/50MHz`](experiments/50MHz); the attempted constraint is [`joystick.sdc`](joystick.sdc).
