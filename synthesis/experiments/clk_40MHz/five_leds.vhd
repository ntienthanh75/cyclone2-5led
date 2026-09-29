-- 4 LED AND BUZZ
-- EP2C5T144C8N
-- author: tien thanh NGUYEN
-- date: 15/01/2023
-- Note: the signals are inverted 
------------------------------------------------------------------
LIBRARY IEEE;
USE IEEE.STD_LOGIC_1164.ALL;
USE IEEE.std_logic_unsigned.ALL;
ENTITY five_leds is
    PORT(
        clk  : in  STD_LOGIC;  --System Clk 
        led1 : out STD_LOGIC_VECTOR(4 DOWNTO 0);
        buzz : out STD_LOGIC --buzz output
    );   
END five_leds ;

ARCHITECTURE light OF five_leds IS
    SIGNAL clk1 : STD_LOGIC;
    SIGNAL clk2 : STD_LOGIC;
BEGIN

P1:PROCESS (clk)
VARIABLE count:INTEGER RANGE  0 TO 9999999;
BEGIN
    IF clk'event AND clk = '1' THEN
         IF count <= 4999999 THEN
            clk1  <= '0';
            count := count+1;
         ELSIF count >= 4999999 AND count <= 9999999 THEN
            clk1  <='1';
            count := count+1;
         ELSE 
            count := 0;
         END IF;
     END IF;
END PROCESS ;
------------------------------------------------------------------
P3:PROCESS(clk1)   
begin
    IF clk1'event AND clk1 = '1'THEN  
        clk2 <= not clk2;
    END IF; 
END PROCESS P3;
------------------------------------------------------------------
P2:PROCESS(clk2)
variable count1 : INTEGER RANGE 0 TO 16;
BEGIN
    IF clk2'event AND clk2='1'THEN
        --IF count1 <= 4 then
            IF count1 = 6 THEN
               count1 := 0;
            END IF;
            CASE count1 IS
                WHEN 0 =>
                    led1 <= "11110";
                    buzz <= '1';
                WHEN 1 =>
                    led1 <= "11101";
                    buzz <= '1';
                WHEN 2 =>
                    led1 <="11011";
                    buzz <= '1';
                WHEN 3 =>
                    led1 <="10111";
                    buzz <= '1';
                WHEN 4 =>
                    led1 <= "01111";
                    buzz <= '1';
                WHEN OTHERS =>
                    led1 <= "11111";
                    buzz <= '1';
            END CASE;
            count1 := count1 + 1;
        --END IF;
    END IF;
END PROCESS;
END light;
