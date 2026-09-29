create_clock -name clk_in -period 20.000 [get_ports {clk}]
derive_clock_uncertainty
