create_clock -name clk_in -period 13.333 [get_ports {CLK}]
derive_clock_uncertainty
set_false_path -from [get_ports {RESET}]
