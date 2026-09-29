create_clock -name clk_in -period 10.000 [get_ports {clk}]
derive_clock_uncertainty
