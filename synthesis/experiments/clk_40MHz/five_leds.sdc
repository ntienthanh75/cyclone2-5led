create_clock -name clk_in -period 25.000 [get_ports {clk}]
derive_clock_uncertainty
