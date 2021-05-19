'Imports System.Drawing
'Imports System.Drawing.Drawing2D
'Imports System.IO

Class SNES_65816

    Const Negative_Flag = &H80
    Const Overflow_Flag = &H40
    Const Accumulator_8_Bits_Flag = &H20
    Const Index_8_Bits_Flag = &H10
    Const Decimal_Flag = &H8
    Const Interrupt_Flag = &H4
    Const Zero_Flag = &H2
    Const Carry_Flag = &H1

    Public Structure CPURegs
        Dim A As Integer 'Accumulator (16 bits)
        Dim X As Integer
        Dim Y As Integer 'Index X/Y (16 bits)
        Dim Stack_Pointer As Integer
        Dim Data_Bank As Byte
        Dim Direct_Page As Integer
        Dim Program_Bank As Byte
        Dim P As Byte 'Flags de status - Ver flags acima
        Dim Program_Counter As Integer 'Posição para leitura de instruções
    End Structure
    Public Registers As CPURegs
    Dim Emulate_6502 As Boolean = True

    Dim Effective_Address As Integer
    Dim Page_Crossed As Boolean

    Public Cycles As Double

    Public SNES_On As Boolean
    Public STP_Disable As Boolean
    Dim WAI_Disable As Boolean

    Public Memory(&H1FFFF) As Byte 'WRAM de 128kb
    Public Save_RAM(7, &H7FFF) As Byte
    Dim WRAM_Address As Integer

    Public IRQ_Ocurred, V_Blank, H_Blank As Boolean
    Public Current_Line As Integer

    Const Cycles_Per_Scanline As Double = 0.0000635 / (1 / 3580000.0)
    Const H_Blank_Cycles As Double = (Cycles_Per_Scanline / 340) * 84

    Public Debug As Boolean = False

    '#Region "Memory Read/Write"
    Public Function Read_Memory(Bank As Byte, Address As Integer) As Byte
        If Address > &HFFFF Then Bank += 1
        Address = Address And &HFFFF
        If Header.Hi_ROM Then
            If ((Bank And &H7F) < &H40) Then
                Select Case Address
                    Case 0 To &H1FFF : Return Memory(Address)
                    Case &H2000 To &H213F : Return Read_PPU(Address)
                    Case &H2140 To &H217F : Return Read_SPU(Address)
                    Case &H2180
                        Dim Value As Byte = Memory(WRAM_Address)
                        WRAM_Address = (WRAM_Address + 1) And &H1FFFF
                        Return Value
                    Case &H4000 To &H41FF : Return Read_IO(Address)
                    Case &H4200 To &H43FF : Return Read_IO(Address)
                    Case &H6000 To &H7FFF : Return Save_RAM(0, Address And &H1FFF)
                    Case &H8000 To &HFFFF : Return ROM_Data(((Bank And &H3F) * 2) + 1, Address And &H7FFF)
                End Select
            End If

            If ((Bank And &H7F) < &H7E) Then
                If Address And &H8000 Then
                    Return ROM_Data(((Bank And &H3F) * 2) + 1, Address And &H7FFF)
                Else
                    Return ROM_Data((Bank And &H3F) * 2, Address And &H7FFF)
                End If
            End If

            If Bank = &HFE Then
                If (Address And &H8000) Then
                    Return ROM_Data(&H7D, Address And &H7FFF)
                Else
                    Return ROM_Data(&H7C, Address And &H7FFF)
                End If
            End If

            If Bank = &HFF Then
                If (Address And &H8000) Then
                    Return ROM_Data(&H7F, Address And &H7FFF)
                Else
                    Return ROM_Data(&H7E, Address And &H7FFF)
                End If
            End If
        Else
            Bank = Bank And &H7F
            If Bank < &H70 Then
                Select Case Address
                    Case 0 To &H1FFF : Return Memory(Address)
                    Case &H2000 To &H213F : Return Read_PPU(Address)
                    Case &H2140 To &H217F : Return Read_SPU(Address)
                    Case &H2180
                        Dim Value As Byte = Memory(WRAM_Address)
                        WRAM_Address = (WRAM_Address + 1) And &H1FFFF
                        Return Value
                    Case &H4000 To &H41FF : Return Read_IO(Address)
                    Case &H4200 To &H43FF : Return Read_IO(Address)
                    Case &H8000 To &HFFFF
                        If Header.Banks <= &H10 Then '???
                            Return ROM_Data(Bank And &HF, Address And &H7FFF)
                        ElseIf Header.Banks <= &H20 Then '???
                            Return ROM_Data(Bank And &H1F, Address And &H7FFF)
                        Else
                            If Bank < &H40 Then
                                Return ROM_Data(Bank, Address And &H7FFF)
                            Else '???
                                Return ROM_Data(Bank And &H3F, Address And &H7FFF)
                            End If
                        End If
                End Select
            End If

            If Bank >= &H70 And Bank <= &H77 Then Return Save_RAM(Bank And 7, Address And &H1FFF)
        End If
        If Bank = &H7E Then Return Memory(Address)
        If Bank = &H7F Then Return Memory(Address + &H10000)

        Return 0 'Nunca deve acontecer
    End Function
    Public Function Read_Memory_16(Bank As Integer, Address As Integer) As Integer
        Return Read_Memory(Bank, Address) + _
            (Read_Memory(Bank, Address + 1) * &H100)
    End Function
    Public Function Read_Memory_24(Bank As Integer, Address As Integer) As Integer
        Return Read_Memory(Bank, Address) + _
            (Read_Memory(Bank, Address + 1) * &H100) + _
            (Read_Memory(Bank, Address + 2) * &H10000)
    End Function
    Public Sub Write_Memory(Bank As Integer, Address As Integer, Value As Byte)
        If Address > &HFFFF Then Bank += 1
        Address = Address And &HFFFF
        Bank = Bank And &H7F
        If Bank < &H70 Then
            Select Case Address
                Case 0 To &H1FFF : Memory(Address) = Value
                Case &H2000 To &H213F : Write_PPU(Address, Value)
                Case &H2140 To &H217F : Write_SPU(Address, Value)
                Case &H2180
                    Memory(WRAM_Address) = Value
                    WRAM_Address = (WRAM_Address + 1) And &H1FFFF
                Case &H2181 : WRAM_Address = Value + (WRAM_Address And &H1FF00)
                Case &H2182 : WRAM_Address = (Value * &H100) + (WRAM_Address And &H100FF)
                Case &H2183 : If Value And 1 Then WRAM_Address = WRAM_Address Or &H10000 Else WRAM_Address = WRAM_Address And Not &H10000
                Case &H4000 To &H41FF : Write_IO(Address, Value)
                Case &H4200 To &H43FF : Write_IO(Address, Value)
                Case &H6000 To &H7FFF : If Header.Hi_ROM And (Bank > &H2F And Bank < &H40) Then Save_RAM(0, Address And &H1FFF) = Value
            End Select
        End If

        If Bank >= &H70 And Bank <= &H77 Then Save_RAM(Bank And 7, Address And &H1FFF) = Value
        If Bank = &H7E Then Memory(Address) = Value
        If Bank = &H7F Then Memory(Address + &H10000) = Value
    End Sub
    Public Sub Write_Memory_16(Bank As Integer, Address As Integer, Value As Integer)
        Write_Memory(Bank, Address, Value And &HFF)
        Write_Memory(Bank, Address + 1, (Value And &HFF00) / &H100)
    End Sub
    Public Sub Write_Memory_24(Bank As Integer, Address As Integer, Value As Integer)
        Write_Memory(Bank, Address, Value And &HFF)
        Write_Memory(Bank, Address + 1, (Value And &HFF00) / &H100)
        Write_Memory(Bank, Address + 2, (Value And &HFF0000) / &H10000)
    End Sub
    '#End Region

    '#Region "CPU Reset/Execute"
    Public Sub Reset_65816()

        Registers.A = 0
        Registers.X = 0
        Registers.Y = 0
        Registers.Stack_Pointer = &H1FF
        Registers.Data_Bank = 0
        Registers.Direct_Page = 0
        Registers.Program_Bank = 0

        Registers.P = 0
        Set_Flag(Accumulator_8_Bits_Flag)
        Set_Flag(Index_8_Bits_Flag) 'Processador inicia no modo 8 bits

        Emulate_6502 = True
        STP_Disable = False
        WAI_Disable = False

        Cycles = 0

        Registers.Program_Counter = Read_Memory_16(0, &HFFFC)
    End Sub
    Public Sub Execute_65816(Target_Cycles As Double)

        While Cycles < Target_Cycles

            Dim Opcode As Byte = Read_Memory(Registers.Program_Bank, Registers.Program_Counter)

            If Debug Then
                WriteLine(1, "PC: " & Hex(Registers.Program_Bank) & ":" & Hex(Registers.Program_Counter) & " DBR: " & Hex(Registers.Data_Bank) & " D: " & Hex(Registers.Direct_Page) & " SP: " & Hex(Registers.Stack_Pointer) & " P: " & Hex(Registers.P) & " A: " & Hex(Registers.A) & " X: " & Hex(Registers.X) & " Y: " & Hex(Registers.Y) & " EA OLD: " & Hex(Effective_Address) & " -- OP: " & Hex(Opcode))
            End If

            Registers.Program_Counter += 1
            Page_Crossed = False

            Select Case Opcode
                Case &H61 'ADC (_dp_,X)
                    DP_Indirect_X()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then
                        Add_With_Carry()
                    Else
                        Add_With_Carry_16()
                        Cycles += 1
                    End If
                    If Registers.Direct_Page And &HFF <> 0 Then Cycles += 1
                    Cycles += 6
                Case &H63 'ADC sr,S
                    Stack_Relative()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Add_With_Carry() Else Add_With_Carry_16()
                    Cycles += 4
                Case &H65 'ADC dp
                    Zero_Page()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Add_With_Carry() Else Add_With_Carry_16()
                    Cycles += 3
                Case &H67 'ADC dp
                    Indirect_Long()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Add_With_Carry() Else Add_With_Carry_16()
                    Cycles += 6
                Case &H69 'ADC #const
                    If (Registers.P And Accumulator_8_Bits_Flag) Then
                        Immediate()
                        Add_With_Carry()
                    Else
                        Immediate_16()
                        Add_With_Carry_16()
                    End If
                    Cycles += 2
                Case &H6D 'ADC addr
                    Absolute()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Add_With_Carry() Else Add_With_Carry_16()
                    Cycles += 4
                Case &H6F 'ADC long
                    Absolute_Long()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Add_With_Carry() Else Add_With_Carry_16()
                    Cycles += 5
                Case &H71 'ADC ( dp),Y
                    Indirect_Y()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Add_With_Carry() Else Add_With_Carry_16()
                    If Page_Crossed Then Cycles += 1
                    Cycles += 5
                Case &H72 'ADC (_dp_)
                    DP_Indirect()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Add_With_Carry() Else Add_With_Carry_16()
                    Cycles += 5
                Case &H73 'ADC (_sr_,S),Y
                    Indirect_Stack_Y()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Add_With_Carry() Else Add_With_Carry_16()
                    Cycles += 7
                Case &H75 'ADC dp,X
                    Zero_Page_X()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Add_With_Carry() Else Add_With_Carry_16()
                    Cycles += 4
                Case &H77 'ADC dp,Y
                    Indirect_Long_Y()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Add_With_Carry() Else Add_With_Carry_16()
                    Cycles += 6
                Case &H79 'ADC addr,Y
                    Absolute_Y()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Add_With_Carry() Else Add_With_Carry_16()
                    Cycles += 4
                Case &H7D 'ADC addr,X
                    Absolute_X()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Add_With_Carry() Else Add_With_Carry_16()
                    Cycles += 4
                Case &H7F 'ADC long,X
                    Absolute_Long_X()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Add_With_Carry() Else Add_With_Carry_16()
                    Cycles += 5

                Case &H21 'AND (_dp_,X)
                    DP_Indirect_X()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then And_With_Accumulator() Else And_With_Accumulator_16()
                    Cycles += 6
                Case &H23 'AND sr,S
                    Stack_Relative()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then And_With_Accumulator() Else And_With_Accumulator_16()
                    Cycles += 4
                Case &H25 'AND dp
                    Zero_Page()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then And_With_Accumulator() Else And_With_Accumulator_16()
                    Cycles += 3
                Case &H27 'AND dp
                    Indirect_Long()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then And_With_Accumulator() Else And_With_Accumulator_16()
                    Cycles += 6
                Case &H29 'AND #const
                    If (Registers.P And Accumulator_8_Bits_Flag) Then
                        Immediate()
                        And_With_Accumulator()
                    Else
                        Immediate_16()
                        And_With_Accumulator_16()
                    End If
                    Cycles += 2
                Case &H2D 'AND addr
                    Absolute()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then And_With_Accumulator() Else And_With_Accumulator_16()
                    Cycles += 4
                Case &H2F 'AND long
                    Absolute_Long()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then And_With_Accumulator() Else And_With_Accumulator_16()
                    Cycles += 5
                Case &H31 'AND ( dp),Y
                    Indirect_Y()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then And_With_Accumulator() Else And_With_Accumulator_16()
                    Cycles += 5
                Case &H32 'AND (_dp_)
                    DP_Indirect()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then And_With_Accumulator() Else And_With_Accumulator_16()
                    Cycles += 5
                Case &H33 'AND (_sr_,S),Y
                    Indirect_Stack_Y()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then And_With_Accumulator() Else And_With_Accumulator_16()
                    Cycles += 7
                Case &H35 'AND dp,X
                    Zero_Page_X()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then And_With_Accumulator() Else And_With_Accumulator_16()
                    Cycles += 4
                Case &H37 'AND dp,Y
                    Indirect_Long_Y()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then And_With_Accumulator() Else And_With_Accumulator_16()
                    Cycles += 6
                Case &H39 'AND addr,Y
                    Absolute_Y()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then And_With_Accumulator() Else And_With_Accumulator_16()
                    Cycles += 4
                Case &H3D 'AND addr,X
                    Absolute_X()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then And_With_Accumulator() Else And_With_Accumulator_16()
                    Cycles += 4
                Case &H3F 'AND long,X
                    Absolute_Long_X()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then And_With_Accumulator() Else And_With_Accumulator_16()
                    Cycles += 5

                Case &H6 'ASL dp
                    Zero_Page()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then
                        Arithmetic_Shift_Left()
                    Else
                        Arithmetic_Shift_Left_16()
                        Cycles += 2
                    End If
                    Cycles += 5
                Case &HA 'ASL A
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Arithmetic_Shift_Left_A() Else Arithmetic_Shift_Left_A_16()
                    Cycles += 2
                Case &HE 'ASL addr
                    Absolute()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Arithmetic_Shift_Left() Else Arithmetic_Shift_Left_16()
                    Cycles += 6
                Case &H16 'ASL dp,X
                    Zero_Page_X()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Arithmetic_Shift_Left() Else Arithmetic_Shift_Left_16()
                    Cycles += 6
                Case &H1E 'ASL addr,X
                    Absolute_X()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Arithmetic_Shift_Left() Else Arithmetic_Shift_Left_16()
                    Cycles += 7

                Case &H90 : Branch_On_Carry_Clear() : Cycles += 2 'BCC nearlabel
                Case &HB0 : Branch_On_Carry_Set() : Cycles += 2 'BCS nearlabel
                Case &HF0 : Branch_On_Equal() : Cycles += 2 'BEQ nearlabel

                Case &H24 'BIT dp
                    Zero_Page()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Test_Bits() Else Test_Bits_16()
                    Cycles += 3
                Case &H2C 'BIT addr
                    Absolute()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Test_Bits() Else Test_Bits_16()
                    Cycles += 4
                Case &H34 'BIT dp,X
                    Zero_Page_X()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Test_Bits() Else Test_Bits_16()
                    Cycles += 4
                Case &H3C 'BIT addr,X
                    Absolute_X()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Test_Bits() Else Test_Bits_16()
                    Cycles += 4
                Case &H89 'BIT #const
                    If (Registers.P And Accumulator_8_Bits_Flag) Then
                        Immediate()
                        Test_Bits()
                    Else
                        Immediate_16()
                        Test_Bits_16()
                    End If
                    Cycles += 2

                Case &H30 : Branch_On_Minus() : Cycles += 2 'BMI nearlabel
                Case &HD0 : Branch_On_Not_Equal() : Cycles += 2 'BNE nearlabel
                Case &H10 : Branch_On_Plus() : Cycles += 2 'BPL nearlabel
                Case &H80 : Branch_Always() : Cycles += 3 'BRA nearlabel

                Case &H0 : Break() : If Emulate_6502 Then Cycles += 7 Else Cycles += 8 'BRK

                Case &H82 : Branch_Long_Always() : Cycles += 4 'BRL label
                Case &H50 : Branch_On_Overflow_Clear() : Cycles += 2 'BVC nearlabel
                Case &H70 : Branch_On_Overflow_Set() : Cycles += 2 'BVS nearlabel

                Case &H18 : Clear_Carry() : Cycles += 2 'CLC
                Case &HD8 : Clear_Decimal() : Cycles += 2 'CLD
                Case &H58 : Clear_Interrupt_Disable() : Cycles += 2 'CLI
                Case &HB8 : Clear_Overflow() : Cycles += 2 'CLV

                Case &HC1 'CMP (_dp_,X)
                    DP_Indirect_X()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Compare() Else Compare_16()
                    Cycles += 6
                Case &HC3 'CMP sr,S
                    Stack_Relative()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Compare() Else Compare_16()
                    Cycles += 4
                Case &HC5 'CMP dp
                    Zero_Page()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Compare() Else Compare_16()
                    Cycles += 3
                Case &HC7 'CMP dp
                    Indirect_Long()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Compare() Else Compare_16()
                    Cycles += 6
                Case &HC9 'CMP #const
                    If (Registers.P And Accumulator_8_Bits_Flag) Then
                        Immediate()
                        Compare()
                    Else
                        Immediate_16()
                        Compare_16()
                    End If
                    Cycles += 2
                Case &HCD 'CMP addr
                    Absolute()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Compare() Else Compare_16()
                    Cycles += 4
                Case &HCF 'CMP long
                    Absolute_Long()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Compare() Else Compare_16()
                    Cycles += 5
                Case &HD1 'CMP ( dp),Y
                    Indirect_Y()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Compare() Else Compare_16()
                    Cycles += 5
                Case &HD2 'CMP (_dp_)
                    DP_Indirect()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Compare() Else Compare_16()
                    Cycles += 5
                Case &HD3 'CMP (_sr_,S),Y
                    Indirect_Stack_Y()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Compare() Else Compare_16()
                    Cycles += 7
                Case &HD5 'CMP dp,X
                    Zero_Page_X()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Compare() Else Compare_16()
                    Cycles += 4
                Case &HD7 'CMP dp,Y
                    Indirect_Long_Y()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Compare() Else Compare_16()
                    Cycles += 6
                Case &HD9 'CMP addr,Y
                    Absolute_Y()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Compare() Else Compare_16()
                    Cycles += 4
                Case &HDD 'CMP addr,X
                    Absolute_X()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Compare() Else Compare_16()
                    Cycles += 4
                Case &HDF 'CMP long,X
                    Absolute_Long_X()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Compare() Else Compare_16()
                    Cycles += 5

                Case &H2 : CoP_Enable() : Cycles += 7 'COP const

                Case &HE0 'CPX #const
                    If (Registers.P And Index_8_Bits_Flag) Then
                        Immediate()
                        Compare_With_X()
                    Else
                        Immediate_16()
                        Compare_With_X_16()
                    End If
                    Cycles += 2
                Case &HE4 'CPX dp
                    Zero_Page()
                    If (Registers.P And Index_8_Bits_Flag) Then Compare_With_X() Else Compare_With_X_16()
                    Cycles += 3
                Case &HEC 'CPX addr
                    Absolute()
                    If (Registers.P And Index_8_Bits_Flag) Then Compare_With_X() Else Compare_With_X_16()
                    Cycles += 4

                Case &HC0 'CPY #const
                    If (Registers.P And Index_8_Bits_Flag) Then
                        Immediate()
                        Compare_With_Y()
                    Else
                        Immediate_16()
                        Compare_With_Y_16()
                    End If
                    Cycles += 2
                Case &HC4 'CPY dp
                    Zero_Page()
                    If (Registers.P And Index_8_Bits_Flag) Then Compare_With_Y() Else Compare_With_Y_16()
                    Cycles += 3
                Case &HCC 'CPY addr
                    Absolute()
                    If (Registers.P And Index_8_Bits_Flag) Then Compare_With_Y() Else Compare_With_Y_16()
                    Cycles += 4

                Case &H3A 'DEC A
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Decrement_A() Else Decrement_A_16()
                    Cycles += 2
                Case &HC6 'DEC dp
                    Zero_Page()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Decrement() Else Decrement_16()
                    Cycles += 5
                Case &HCE 'DEC addr
                    Absolute()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Decrement() Else Decrement_16()
                    Cycles += 6
                Case &HD6 'DEC dp,X
                    Zero_Page_X()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Decrement() Else Decrement_16()
                    Cycles += 6
                Case &HDE 'DEC addr,X
                    Absolute_X()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Decrement() Else Decrement_16()
                    Cycles += 7

                Case &HCA 'DEX
                    If (Registers.P And Index_8_Bits_Flag) Then Decrement_X() Else Decrement_X_16()
                    Cycles += 2

                Case &H88 'DEY
                    If (Registers.P And Index_8_Bits_Flag) Then Decrement_Y() Else Decrement_Y_16()
                    Cycles += 2

                Case &H41 'EOR (_dp_,X)
                    DP_Indirect_X()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Exclusive_Or() Else Exclusive_Or_16()
                    Cycles += 6
                Case &H43 'EOR sr,S
                    Stack_Relative()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Exclusive_Or() Else Exclusive_Or_16()
                    Cycles += 4
                Case &H45 'EOR dp
                    Zero_Page()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Exclusive_Or() Else Exclusive_Or_16()
                    Cycles += 3
                Case &H47 'EOR dp
                    Indirect_Long()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Exclusive_Or() Else Exclusive_Or_16()
                    Cycles += 6
                Case &H49 'EOR #const
                    If (Registers.P And Accumulator_8_Bits_Flag) Then
                        Immediate()
                        Exclusive_Or()
                    Else
                        Immediate_16()
                        Exclusive_Or_16()
                    End If
                    Cycles += 2
                Case &H4D 'EOR addr
                    Absolute()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Exclusive_Or() Else Exclusive_Or_16()
                    Cycles += 4
                Case &H4F 'EOR long
                    Absolute_Long()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Exclusive_Or() Else Exclusive_Or_16()
                    Cycles += 5
                Case &H51 'EOR ( dp),Y
                    Indirect_Y()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Exclusive_Or() Else Exclusive_Or_16()
                    Cycles += 5
                Case &H52 'EOR (_dp_)
                    DP_Indirect()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Exclusive_Or() Else Exclusive_Or_16()
                    Cycles += 5
                Case &H53 'EOR (_sr_,S),Y
                    Indirect_Stack_Y()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Exclusive_Or() Else Exclusive_Or_16()
                    Cycles += 7
                Case &H55 'EOR dp,X
                    Zero_Page_X()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Exclusive_Or() Else Exclusive_Or_16()
                    Cycles += 4
                Case &H57 'EOR dp,Y
                    Indirect_Long_Y()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Exclusive_Or() Else Exclusive_Or_16()
                    Cycles += 6
                Case &H59 'EOR addr,Y
                    Absolute_Y()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Exclusive_Or() Else Exclusive_Or_16()
                    Cycles += 4
                Case &H5D 'EOR addr,X
                    Absolute_X()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Exclusive_Or() Else Exclusive_Or_16()
                    Cycles += 4
                Case &H5F 'EOR long,X
                    Absolute_Long_X()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Exclusive_Or() Else Exclusive_Or_16()
                    Cycles += 5

                Case &H1A 'INC A
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Increment_A() Else Increment_A_16()
                    Cycles += 2
                Case &HE6 'INC dp
                    Zero_Page()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Increment() Else Increment_16()
                    Cycles += 5
                Case &HEE 'INC addr
                    Absolute()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Increment() Else Increment_16()
                    Cycles += 6
                Case &HF6 'INC dp,X
                    Zero_Page_X()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Increment() Else Increment_16()
                    Cycles += 6
                Case &HFE 'INC addr,X
                    Absolute_X()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Increment() Else Increment_16()
                    Cycles += 7

                Case &HE8 'INX
                    If (Registers.P And Index_8_Bits_Flag) Then Increment_X() Else Increment_X_16()
                    Cycles += 2

                Case &HC8 'INY
                    If (Registers.P And Index_8_Bits_Flag) Then Increment_Y() Else Increment_Y_16()
                    Cycles += 2

                Case &H4C : Absolute() : Jump() : Cycles += 3 'JMP addr
                Case &H5C : Absolute_Long() : Jump() : Registers.Program_Bank = (Effective_Address And &HFF0000) / &H10000 : Cycles += 4 'JMP long
                Case &H6C : Indirect() : Jump() : Cycles += 5 'JMP (_addr_)
                Case &H7C : Indirect_X() : Jump() : Cycles += 6 'JMP (_addr,X_)
                Case &HDC : Indirect_Long_Jump() : Jump() : Registers.Program_Bank = (Effective_Address And &HFF0000) / &H10000 : Cycles += 6 'JMP addr

                Case &H20 : Absolute() : Jump_To_Subroutine() : Cycles += 6 'JSR addr
                Case &H22 : Absolute_Long() : Jump_To_Subroutine(True) : Cycles += 8 'JSR long
                Case &HFC : Indirect_X() : Jump_To_Subroutine() : Cycles += 8 'JSR (addr,X)

                Case &HA1 'LDA (_dp_,X)
                    DP_Indirect_X()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Load_Accumulator() Else Load_Accumulator_16()
                    Cycles += 6
                Case &HA3 'LDA sr,S
                    Stack_Relative()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Load_Accumulator() Else Load_Accumulator_16()
                    Cycles += 4
                Case &HA5 'LDA dp
                    Zero_Page()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Load_Accumulator() Else Load_Accumulator_16()
                    Cycles += 3
                Case &HA7 'LDA dp
                    Indirect_Long()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Load_Accumulator() Else Load_Accumulator_16()
                    Cycles += 6
                Case &HA9 'LDA #const
                    If (Registers.P And Accumulator_8_Bits_Flag) Then
                        Immediate()
                        Load_Accumulator()
                    Else
                        Immediate_16()
                        Load_Accumulator_16()
                    End If
                    Cycles += 2
                Case &HAD 'LDA addr
                    Absolute()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Load_Accumulator() Else Load_Accumulator_16()
                    Cycles += 4
                Case &HAF 'LDA long
                    Absolute_Long()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Load_Accumulator() Else Load_Accumulator_16()
                    Cycles += 5
                Case &HB1 'LDA ( dp),Y
                    Indirect_Y()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Load_Accumulator() Else Load_Accumulator_16()
                    Cycles += 5
                Case &HB2 'LDA (_dp_)
                    DP_Indirect()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Load_Accumulator() Else Load_Accumulator_16()
                    Cycles += 5
                Case &HB3 'LDA (_sr_,S),Y
                    Indirect_Stack_Y()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Load_Accumulator() Else Load_Accumulator_16()
                    Cycles += 7
                Case &HB5 'LDA dp,X
                    Zero_Page_X()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Load_Accumulator() Else Load_Accumulator_16()
                    Cycles += 4
                Case &HB7 'LDA dp,Y
                    Indirect_Long_Y()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Load_Accumulator() Else Load_Accumulator_16()
                    Cycles += 6
                Case &HB9 'LDA addr,Y
                    Absolute_Y()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Load_Accumulator() Else Load_Accumulator_16()
                    Cycles += 4
                Case &HBD 'LDA addr,X
                    Absolute_X()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Load_Accumulator() Else Load_Accumulator_16()
                    Cycles += 4
                Case &HBF 'LDA long,X
                    Absolute_Long_X()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Load_Accumulator() Else Load_Accumulator_16()
                    Cycles += 5

                Case &HA2 'LDX #const
                    If (Registers.P And Index_8_Bits_Flag) Then
                        Immediate()
                        Load_X()
                    Else
                        Immediate_16()
                        Load_X_16()
                    End If
                    Cycles += 2
                Case &HA6 'LDX dp
                    Zero_Page()
                    If (Registers.P And Index_8_Bits_Flag) Then Load_X() Else Load_X_16()
                    Cycles += 3
                Case &HAE 'LDX addr
                    Absolute()
                    If (Registers.P And Index_8_Bits_Flag) Then Load_X() Else Load_X_16()
                    Cycles += 4
                Case &HB6 'LDX dp,Y
                    Zero_Page_Y()
                    If (Registers.P And Index_8_Bits_Flag) Then Load_X() Else Load_X_16()
                    Cycles += 4
                Case &HBE 'LDX addr,Y
                    Absolute_Y()
                    If (Registers.P And Index_8_Bits_Flag) Then Load_X() Else Load_X_16()
                    Cycles += 4

                Case &HA0 'LDY #const
                    If (Registers.P And Index_8_Bits_Flag) Then
                        Immediate()
                        Load_Y()
                    Else
                        Immediate_16()
                        Load_Y_16()
                    End If
                    Cycles += 2
                Case &HA4 'LDY dp
                    Zero_Page()
                    If (Registers.P And Index_8_Bits_Flag) Then Load_Y() Else Load_Y_16()
                    Cycles += 3
                Case &HAC 'LDY addr
                    Absolute()
                    If (Registers.P And Index_8_Bits_Flag) Then Load_Y() Else Load_Y_16()
                    Cycles += 4
                Case &HB4 'LDY dp,X
                    Zero_Page_X()
                    If (Registers.P And Index_8_Bits_Flag) Then Load_Y() Else Load_Y_16()
                    Cycles += 4
                Case &HBC 'LDY addr,X
                    Absolute_X()
                    If (Registers.P And Index_8_Bits_Flag) Then Load_Y() Else Load_Y_16()
                    Cycles += 4

                Case &H46 'LSR dp
                    Zero_Page()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Logical_Shift_Right() Else Logical_Shift_Right_16()
                    Cycles += 5
                Case &H4A 'LSR A
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Logical_Shift_Right_A() Else Logical_Shift_Right_A_16()
                    Cycles += 2
                Case &H4E 'LSR addr
                    Absolute()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Logical_Shift_Right() Else Logical_Shift_Right_16()
                    Cycles += 6
                Case &H56 'LSR dp,X
                    Zero_Page_X()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Logical_Shift_Right() Else Logical_Shift_Right_16()
                    Cycles += 6
                Case &H5E 'LSR addr,X
                    Absolute_X()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Logical_Shift_Right() Else Logical_Shift_Right_16()
                    Cycles += 7

                Case &H54 : Block_Move_Negative() : Cycles += 1 'MVN srcbk,destbk
                Case &H44 : Block_Move_Positive() : Cycles += 1 'MVP srcbk,destbk

                Case &HEA : Cycles += 2 'NOP

                Case &H1 'ORA (_dp_,X)
                    DP_Indirect_X()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Or_With_Accumulator() Else Or_With_Accumulator_16()
                    Cycles += 6
                Case &H3 'ORA sr,S
                    Stack_Relative()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Or_With_Accumulator() Else Or_With_Accumulator_16()
                    Cycles += 4
                Case &H5 'ORA dp
                    Zero_Page()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Or_With_Accumulator() Else Or_With_Accumulator_16()
                    Cycles += 3
                Case &H7 'ORA dp
                    Indirect_Long()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Or_With_Accumulator() Else Or_With_Accumulator_16()
                    Cycles += 6
                Case &H9 'ORA #const
                    If (Registers.P And Accumulator_8_Bits_Flag) Then
                        Immediate()
                        Or_With_Accumulator()
                    Else
                        Immediate_16()
                        Or_With_Accumulator_16()
                    End If
                    Cycles += 2
                Case &HD 'ORA addr
                    Absolute()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Or_With_Accumulator() Else Or_With_Accumulator_16()
                    Cycles += 4
                Case &HF 'ORA long
                    Absolute_Long()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Or_With_Accumulator() Else Or_With_Accumulator_16()
                    Cycles += 5
                Case &H11 'ORA ( dp),Y
                    Indirect_Y()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Or_With_Accumulator() Else Or_With_Accumulator_16()
                    Cycles += 5
                Case &H12 'ORA (_dp_)
                    DP_Indirect()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Or_With_Accumulator() Else Or_With_Accumulator_16()
                    Cycles += 5
                Case &H13 'ORA (_sr_,S),Y
                    Indirect_Stack_Y()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Or_With_Accumulator() Else Or_With_Accumulator_16()
                    Cycles += 7
                Case &H15 'ORA dp,X
                    Zero_Page_X()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Or_With_Accumulator() Else Or_With_Accumulator_16()
                    Cycles += 4
                Case &H17 'ORA dp,Y
                    Indirect_Long_Y()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Or_With_Accumulator() Else Or_With_Accumulator_16()
                    Cycles += 6
                Case &H19 'ORA addr,Y
                    Absolute_Y()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Or_With_Accumulator() Else Or_With_Accumulator_16()
                    Cycles += 4
                Case &H1D 'ORA addr,X
                    Absolute_X()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Or_With_Accumulator() Else Or_With_Accumulator_16()
                    Cycles += 4
                Case &H1F 'ORA long,X
                    Absolute_Long_X()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Or_With_Accumulator() Else Or_With_Accumulator_16()
                    Cycles += 5

                Case &HF4 : Absolute() : Push_Effective_Address() : Cycles += 5 'PEA addr
                Case &HD4 : DP_Indirect() : Push_Effective_Address() : Cycles += 6 'PEI (dp)
                Case &H62 'PER label
                    Effective_Address = Read_Memory_16(Registers.Program_Bank, Registers.Program_Counter)
                    Registers.Program_Counter += 2
                    Effective_Address += Registers.Program_Counter
                    Push_Effective_Address()
                    Cycles += 6
                Case &H48 'PHA
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Push_Accumulator() Else Push_Accumulator_16()
                    Cycles += 3
                Case &H8B : Push_Data_Bank() : Cycles += 3 'PHB
                Case &HB : Push_Direct_Page() : Cycles += 4 'PHD
                Case &H4B : Push_Program_Bank() : Cycles += 3 'PHK
                Case &H8 : Push_Processor_Status() : Cycles += 3 'PHP
                Case &HDA 'PHX
                    If (Registers.P And Index_8_Bits_Flag) Then Push_X() Else Push_X_16()
                    Cycles += 3
                Case &H5A
                    If (Registers.P And Index_8_Bits_Flag) Then Push_Y() Else Push_Y_16()
                    Cycles += 3 'PHY

                Case &H68 'PLA
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Pull_Accumulator() Else Pull_Accumulator_16()
                    Cycles += 4
                Case &HAB : Pull_Data_Bank() : Cycles += 4 'PLB
                Case &H2B : Pull_Direct_Page() : Cycles += 5 'PLD
                Case &H28 : Pull_Processor_Status() : Cycles += 4 'PLP
                Case &HFA 'PLX
                    If (Registers.P And Index_8_Bits_Flag) Then Pull_X() Else Pull_X_16()
                    Cycles += 4
                Case &H7A 'PLY
                    If (Registers.P And Index_8_Bits_Flag) Then Pull_Y() Else Pull_Y_16()
                    Cycles += 4

                Case &HC2 : Immediate() : Reset_Status() : Cycles += 3 'REP #const

                Case &H26 'ROL dp
                    Zero_Page()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Rotate_Left() Else Rotate_Left_16()
                    Cycles += 5
                Case &H2A 'ROL A
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Rotate_Left_A() Else Rotate_Left_A_16()
                    Cycles += 2
                Case &H2E 'ROL addr
                    Absolute()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Rotate_Left() Else Rotate_Left_16()
                    Cycles += 6
                Case &H36 'ROL dp,X
                    Zero_Page_X()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Rotate_Left() Else Rotate_Left_16()
                    Cycles += 6
                Case &H3E 'ROL addr,X
                    Absolute_X()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Rotate_Left() Else Rotate_Left_16()
                    Cycles += 7

                Case &H66 'ROR dp
                    Zero_Page()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Rotate_Right() Else Rotate_Right_16()
                    Cycles += 5
                Case &H6A 'ROR A
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Rotate_Right_A() Else Rotate_Right_A_16()
                    Cycles += 2
                Case &H6E 'ROR addr
                    Absolute()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Rotate_Right() Else Rotate_Right_16()
                    Cycles += 6
                Case &H76 'ROR dp,X
                    Zero_Page_X()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Rotate_Right() Else Rotate_Right_16()
                    Cycles += 6
                Case &H7E 'ROR addr,X
                    Absolute_X()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Rotate_Right() Else Rotate_Right_16()
                    Cycles += 7

                Case &H40 : Return_From_Interrupt() : Cycles += 6 'RTI
                Case &H6B : Return_From_Subroutine_Long() : Cycles += 6 'RTL
                Case &H60 : Return_From_Subroutine() : Cycles += 6 'RTS

                Case &HE1 'SBC (_dp_,X)
                    DP_Indirect_X()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Subtract_With_Carry() Else Subtract_With_Carry_16()
                    Cycles += 6
                Case &HE3 'SBC sr,S
                    Stack_Relative()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Subtract_With_Carry() Else Subtract_With_Carry_16()
                    Cycles += 4
                Case &HE5 'SBC dp
                    Zero_Page()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Subtract_With_Carry() Else Subtract_With_Carry_16()
                    Cycles += 3
                Case &HE7 'SBC dp
                    Indirect_Long()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Subtract_With_Carry() Else Subtract_With_Carry_16()
                    Cycles += 6
                Case &HE9 'SBC #const
                    If (Registers.P And Accumulator_8_Bits_Flag) Then
                        Immediate()
                        Subtract_With_Carry()
                    Else
                        Immediate_16()
                        Subtract_With_Carry_16()
                    End If
                    Cycles += 2
                Case &HED 'SBC addr
                    Absolute()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Subtract_With_Carry() Else Subtract_With_Carry_16()
                    Cycles += 4
                Case &HEF 'SBC long
                    Absolute_Long()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Subtract_With_Carry() Else Subtract_With_Carry_16()
                    Cycles += 5
                Case &HF1 'SBC ( dp),Y
                    Indirect_Y()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Subtract_With_Carry() Else Subtract_With_Carry_16()
                    Cycles += 5
                Case &HF2 'SBC (_dp_)
                    DP_Indirect()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Subtract_With_Carry() Else Subtract_With_Carry_16()
                    Cycles += 5
                Case &HF3 'SBC (_sr_,S),Y
                    Indirect_Stack_Y()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Subtract_With_Carry() Else Subtract_With_Carry_16()
                    Cycles += 7
                Case &HF5 'SBC dp,X
                    Zero_Page_X()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Subtract_With_Carry() Else Subtract_With_Carry_16()
                    Cycles += 4
                Case &HF7 'SBC dp,Y
                    Indirect_Long_Y()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Subtract_With_Carry() Else Subtract_With_Carry_16()
                    Cycles += 6
                Case &HF9 'SBC addr,Y
                    Absolute_Y()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Subtract_With_Carry() Else Subtract_With_Carry_16()
                    Cycles += 4
                Case &HFD 'SBC addr,X
                    Absolute_X()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Subtract_With_Carry() Else Subtract_With_Carry_16()
                    Cycles += 4
                Case &HFF 'SBC long,X
                    Absolute_Long_X()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Subtract_With_Carry() Else Subtract_With_Carry_16()
                    Cycles += 5

                Case &H38 : Set_Carry() : Cycles += 2 'SEC
                Case &HF8 : Set_Decimal() : Cycles += 2 'SED
                Case &H78 : Set_Interrupt_Disable() : Cycles += 2 'SEI
                Case &HE2 : Immediate() : Set_Status() : Cycles += 3 'SEP

                Case &H81 'STA (_dp_,X)
                    DP_Indirect_X()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Store_Accumulator() Else Store_Accumulator_16()
                    Cycles += 6
                Case &H83 'STA sr,S
                    Stack_Relative()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Store_Accumulator() Else Store_Accumulator_16()
                    Cycles += 4
                Case &H85 'STA dp
                    Zero_Page()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Store_Accumulator() Else Store_Accumulator_16()
                    Cycles += 3
                Case &H87 'STA dp
                    Indirect_Long()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Store_Accumulator() Else Store_Accumulator_16()
                    Cycles += 6
                Case &H89 'STA #const
                    If (Registers.P And Accumulator_8_Bits_Flag) Then
                        Immediate()
                        Store_Accumulator()
                    Else
                        Immediate_16()
                        Store_Accumulator_16()
                    End If
                    Cycles += 2
                Case &H8D 'STA addr
                    Absolute()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Store_Accumulator() Else Store_Accumulator_16()
                    Cycles += 4
                Case &H8F 'STA long
                    Absolute_Long()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Store_Accumulator() Else Store_Accumulator_16()
                    Cycles += 5
                Case &H91 'STA ( dp),Y
                    Indirect_Y()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Store_Accumulator() Else Store_Accumulator_16()
                    Cycles += 5
                Case &H92 'STA (_dp_)
                    DP_Indirect()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Store_Accumulator() Else Store_Accumulator_16()
                    Cycles += 5
                Case &H93 'STA (_sr_,S),Y
                    Indirect_Stack_Y()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Store_Accumulator() Else Store_Accumulator_16()
                    Cycles += 7
                Case &H95 'STA dp,X
                    Zero_Page_X()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Store_Accumulator() Else Store_Accumulator_16()
                    Cycles += 4
                Case &H97 'STA dp,Y
                    Indirect_Long_Y()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Store_Accumulator() Else Store_Accumulator_16()
                    Cycles += 6
                Case &H99 'STA addr,Y
                    Absolute_Y()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Store_Accumulator() Else Store_Accumulator_16()
                    Cycles += 4
                Case &H9D 'STA addr,X
                    Absolute_X()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Store_Accumulator() Else Store_Accumulator_16()
                    Cycles += 4
                Case &H9F 'STA long,X
                    Absolute_Long_X()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Store_Accumulator() Else Store_Accumulator_16()
                    Cycles += 5

                Case &HDB : Stop_Processor() : Cycles += 3 'STP (STOP, volta com Reset)

                Case &H86 'STX dp
                    Zero_Page()
                    If (Registers.P And Index_8_Bits_Flag) Then Store_X() Else Store_X_16()
                    Cycles += 3
                Case &H8E 'STX addr
                    Absolute()
                    If (Registers.P And Index_8_Bits_Flag) Then Store_X() Else Store_X_16()
                    Cycles += 4
                Case &H96 'STX dp,Y
                    Zero_Page_Y()
                    If (Registers.P And Index_8_Bits_Flag) Then Store_X() Else Store_X_16()
                    Cycles += 4

                Case &H84 'STY dp
                    Zero_Page()
                    If (Registers.P And Index_8_Bits_Flag) Then Store_Y() Else Store_Y_16()
                    Cycles += 3
                Case &H8C 'STY addr
                    Absolute()
                    If (Registers.P And Index_8_Bits_Flag) Then Store_Y() Else Store_Y_16()
                    Cycles += 4
                Case &H94 'STY dp,X
                    Zero_Page_X()
                    If (Registers.P And Index_8_Bits_Flag) Then Store_Y() Else Store_Y_16()
                    Cycles += 4

                Case &H64 'STZ dp
                    Zero_Page()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Store_Zero() Else Store_Zero_16()
                    Cycles += 3
                Case &H74 'STZ dp,X
                    Zero_Page_X()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Store_Zero() Else Store_Zero_16()
                    Cycles += 4
                Case &H9C 'STZ addr
                    Absolute()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Store_Zero() Else Store_Zero_16()
                    Cycles += 4
                Case &H9E 'STZ addr,X
                    Absolute_X()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Store_Zero() Else Store_Zero_16()
                    Cycles += 5

                Case &HAA 'TAX
                    If (Registers.P And Index_8_Bits_Flag) Then Transfer_Accumulator_To_X() Else Transfer_Accumulator_To_X_16()
                    Cycles += 2
                Case &HA8 'TAY
                    If (Registers.P And Index_8_Bits_Flag) Then Transfer_Accumulator_To_Y() Else Transfer_Accumulator_To_Y_16()
                    Cycles += 2
                Case &H5B : Transfer_Accumulator_To_DP() : Cycles += 2 'TCD
                Case &H1B : Transfer_Accumulator_To_SP() : Cycles += 2 'TCS
                Case &H7B : Transfer_DP_To_Accumulator() : Cycles += 2 'TDC

                Case &H14 'TRB dp
                    Zero_Page()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Test_And_Reset_Bit() Else Test_And_Reset_Bit_16()
                    Cycles += 5
                Case &H1C 'TRB addr
                    Absolute()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Test_And_Reset_Bit() Else Test_And_Reset_Bit_16()
                    Cycles += 6

                Case &H4 'TSB dp
                    Zero_Page()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Test_And_Set_Bit() Else Test_And_Set_Bit_16()
                    Cycles += 5
                Case &HC 'TSB addr
                    Absolute()
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Test_And_Set_Bit() Else Test_And_Set_Bit_16()
                    Cycles += 6

                Case &H3B : Transfer_SP_To_Accumulator() : Cycles += 2 'TSC
                Case &HBA : Transfer_SP_To_X() : Cycles += 2 'TSX
                Case &H8A 'TXA
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Transfer_X_To_Accumulator() Else Transfer_X_To_Accumulator_16()
                    Cycles += 2
                Case &H9A : Transfer_X_To_SP() : Cycles += 2 'TXS
                Case &H9B 'TXY
                    If (Registers.P And Index_8_Bits_Flag) Then Transfer_X_To_Y() Else Transfer_X_To_Y_16()
                    Cycles += 2
                Case &H98 'TYA
                    If (Registers.P And Accumulator_8_Bits_Flag) Then Transfer_Y_To_Accumulator() Else Transfer_Y_To_Accumulator_16()
                    Cycles += 2
                Case &HBB 'TYX
                    If (Registers.P And Index_8_Bits_Flag) Then Transfer_Y_To_X() Else Transfer_Y_To_X_16()
                    Cycles += 2

                Case &HCB : Wait_For_Interrupt() : Cycles += 3 'WAI (STOP, volta com Interrupt)

                Case &H42 'WDM (Não usado, expansão)

                Case &HEB : Exchange_Accumulator() : Cycles += 3 'XBA

                Case &HFB : Exchange_Carry_And_Emulation() : Cycles += 2 'XCE

                Case Else : MsgBox("Opcode não implementado em 0x" & Hex(Registers.Program_Counter) & " -> " & Hex(Opcode)) : Cycles += 1
            End Select
        End While
        Cycles -= Target_Cycles
    End Sub
    '#End Region

    '#Region "Flag Handling Functions"
    Private Sub Set_Flag(Value As Byte)
        Registers.P = Registers.P Or Value
    End Sub
    Private Sub Clear_Flag(Value As Byte)
        Registers.P = Registers.P And Not Value
    End Sub
    Private Sub Set_Zero_Negative_Flag(Value As Byte)
        If Value Then Clear_Flag(Zero_Flag) Else Set_Flag(Zero_Flag)
        If Value And &H80 Then Set_Flag(Negative_Flag) Else Clear_Flag(Negative_Flag)
    End Sub
    Private Sub Set_Zero_Negative_Flag_16(Value As Integer)
        If Value Then Clear_Flag(Zero_Flag) Else Set_Flag(Zero_Flag)
        If Value And &H8000 Then Set_Flag(Negative_Flag) Else Clear_Flag(Negative_Flag)
    End Sub
    Private Sub Test_Flag(Condition As Boolean, Value As Byte)
        If Condition Then Set_Flag(Value) Else Clear_Flag(Value)
    End Sub
    '#End Region

    '#Region "Stack Push/Pull"
    Private Sub Push(Value As Byte)
        Write_Memory(0, Registers.Stack_Pointer, Value)
        Registers.Stack_Pointer -= 1
    End Sub
    Private Function Pull() As Byte
        Registers.Stack_Pointer += 1
        Return Read_Memory(0, Registers.Stack_Pointer)
    End Function
    Private Sub Push_16(Value As Integer)
        Push((Value And &HFF00) / &H100)
        Push(Value And &HFF)
    End Sub
    Private Function Pull_16() As Integer
        Return Pull() + (Pull() * &H100)
    End Function
    '#End Region

    '#Region "Unsigned/Signed converter, Update_Mode"
    Private Function Signed_Byte(Byte_To_Convert As Byte) As SByte
        If (Byte_To_Convert < &H80) Then Return Byte_To_Convert
        Return Byte_To_Convert - &H100
    End Function
    Private Function Signed_Integer(Integer_To_Convert As Integer) As Integer
        If (Integer_To_Convert < &H8000) Then Return Integer_To_Convert
        Return Integer_To_Convert - &H10000
    End Function

    Private Sub Update_Mode()
        If (Registers.P And Index_8_Bits_Flag) Or Emulate_6502 Then 'Remove High Byte
            Registers.X = Registers.X And &HFF
            Registers.Y = Registers.Y And &HFF
        End If
    End Sub
    '#End Region

    '#Region "Addressing Modes"
    Private Sub Immediate() '8 bits
        Effective_Address = Registers.Program_Counter + (Registers.Program_Bank * &H10000)
        Registers.Program_Counter += 1
    End Sub
    Private Sub Immediate_16() '16 bits
        Effective_Address = Registers.Program_Counter + (Registers.Program_Bank * &H10000)
        Registers.Program_Counter += 2
    End Sub
    Private Sub Zero_Page()
        Effective_Address = Read_Memory(Registers.Program_Bank, Registers.Program_Counter) + Registers.Direct_Page
        Registers.Program_Counter += 1
    End Sub
    Private Sub Zero_Page_X()
        Effective_Address = Read_Memory(Registers.Program_Bank, Registers.Program_Counter) + Registers.Direct_Page + Registers.X
        Registers.Program_Counter += 1
    End Sub
    Private Sub Zero_Page_Y()
        Effective_Address = Read_Memory(Registers.Program_Bank, Registers.Program_Counter) + Registers.Direct_Page + Registers.Y
        Registers.Program_Counter += 1
    End Sub
    Private Sub Stack_Relative()
        Effective_Address = Read_Memory(Registers.Program_Bank, Registers.Program_Counter) + Registers.Stack_Pointer
        Registers.Program_Counter += 1
    End Sub
    Private Sub Absolute()
        Effective_Address = Read_Memory_16(Registers.Program_Bank, Registers.Program_Counter) + (Registers.Data_Bank * &H10000)
        Registers.Program_Counter += 2
    End Sub
    Private Sub Absolute_X()
        Effective_Address = Read_Memory_16(Registers.Program_Bank, Registers.Program_Counter) + (Registers.Data_Bank * &H10000) + Registers.X
        Registers.Program_Counter += 2
    End Sub
    Private Sub Absolute_Y()
        Effective_Address = Read_Memory_16(Registers.Program_Bank, Registers.Program_Counter) + (Registers.Data_Bank * &H10000) + Registers.Y
        Registers.Program_Counter += 2
    End Sub
    Private Sub Absolute_Long()
        Effective_Address = Read_Memory_24(Registers.Program_Bank, Registers.Program_Counter)
        Registers.Program_Counter += 3
    End Sub
    Private Sub Absolute_Long_X()
        Effective_Address = Read_Memory_24(Registers.Program_Bank, Registers.Program_Counter) + Registers.X
        Registers.Program_Counter += 3
    End Sub
    Private Sub Indirect()
        Dim Addr As Integer = Read_Memory_16(Registers.Program_Bank, Registers.Program_Counter)
        Effective_Address = Read_Memory_16(Registers.Program_Bank, Addr)
        Registers.Program_Counter += 2
    End Sub
    Private Sub DP_Indirect()
        Dim Addr As Integer = Read_Memory(Registers.Program_Bank, Registers.Program_Counter) + Registers.Direct_Page
        Effective_Address = Read_Memory_16(0, Addr) + (Registers.Data_Bank * &H10000)
        Registers.Program_Counter += 1
    End Sub
    Private Sub Indirect_Y()
        Dim Addr As Integer = Read_Memory(Registers.Program_Bank, Registers.Program_Counter) + Registers.Direct_Page
        Effective_Address = Read_Memory_16(0, Addr) + (Registers.Data_Bank * &H10000)
        If (Effective_Address And &HFF00) <> ((Effective_Address + Registers.Y) And &HFF00) Then Page_Crossed = True
        Effective_Address += Registers.Y
        Registers.Program_Counter += 1
    End Sub
    Private Sub Indirect_Stack_Y()
        Dim Addr As Integer = Read_Memory(Registers.Program_Bank, Registers.Program_Counter) + Registers.Stack_Pointer
        Effective_Address = Read_Memory_16(0, Addr) + (Registers.Data_Bank * &H10000) + Registers.Y
        Registers.Program_Counter += 1
    End Sub
    Private Sub Indirect_Long()
        Dim Addr As Integer = Read_Memory(Registers.Program_Bank, Registers.Program_Counter) + Registers.Direct_Page
        Effective_Address = Read_Memory_24(0, Addr)
        Registers.Program_Counter += 1
    End Sub
    Private Sub Indirect_Long_Jump()
        Dim Addr As Integer = Read_Memory_16(Registers.Program_Bank, Registers.Program_Counter)
        Effective_Address = Read_Memory_24(0, Addr)
        Registers.Program_Counter += 2
    End Sub
    Private Sub Indirect_Long_Y()
        Dim Addr As Integer = Read_Memory(Registers.Program_Bank, Registers.Program_Counter) + Registers.Direct_Page
        Effective_Address = Read_Memory_24(0, Addr) + Registers.Y
        Registers.Program_Counter += 1
    End Sub
    Private Sub Indirect_X()
        Dim Addr As Integer = Read_Memory_16(Registers.Program_Bank, Registers.Program_Counter) + Registers.X
        Effective_Address = Read_Memory_16(Registers.Program_Bank, Addr)
        Registers.Program_Counter += 2
    End Sub
    Private Sub DP_Indirect_X()
        Dim Addr As Integer = Read_Memory(Registers.Program_Bank, Registers.Program_Counter) + Registers.Direct_Page + Registers.X
        Effective_Address = Read_Memory_16(0, Addr) + (Registers.Data_Bank * &H10000)
        Registers.Program_Counter += 1
    End Sub
    '#End Region

    '#Region "Instructions"
    Private Sub Add_With_Carry() 'ADC (8 bits)
        If (Registers.P And Decimal_Flag) = 0 Then
            Dim Value As Byte = Read_Memory((Effective_Address And &HFF0000) / &H10000, Effective_Address And &HFFFF)
            Dim Result As Integer = (Registers.A And &HFF) + Value + (Registers.P And Carry_Flag)
            Test_Flag(Result > &HFF, Carry_Flag)
            Test_Flag(((Not ((Registers.A And &HFF) Xor Value)) And ((Registers.A And &HFF) Xor Result) And &H80), Overflow_Flag)
            Registers.A = (Result And &HFF) + (Registers.A And &HFF00)
            Set_Zero_Negative_Flag(Registers.A And &HFF)
        Else
            Add_With_Carry_BCD()
        End If
    End Sub
    Private Sub Add_With_Carry_BCD() 'ADC (BCD) (8 bits)
        Dim Value As Byte = Read_Memory((Effective_Address And &HFF0000) / &H10000, Effective_Address And &HFFFF)
        Dim Result As Integer = (Registers.A And &HF) + (Value And &HF) + (Registers.P And Carry_Flag)
        If Result > 9 Then Result += 6
        Test_Flag(Result > &HF, Carry_Flag)
        Result = (Registers.A And &HF0) + (Value And &HF0) + (Result And &HF) + ((Registers.P And Carry_Flag) * &H10)
        Test_Flag(((Not ((Registers.A And &HFF) Xor Value)) And ((Registers.A And &HFF) Xor Result) And &H80), Overflow_Flag)
        If Result > &H9F Then Result += &H60
        Test_Flag(Result > &HFF, Carry_Flag)
        Registers.A = (Result And &HFF) + (Registers.A And &HFF00)
        Set_Zero_Negative_Flag(Registers.A And &HFF)
    End Sub
    Private Sub Add_With_Carry_16() 'ADC (16 bits)
        If (Registers.P And Decimal_Flag) = 0 Then
            Dim Value As Integer = Read_Memory_16((Effective_Address And &HFF0000) / &H10000, Effective_Address And &HFFFF)
            Dim Result As Integer = Registers.A + Value + (Registers.P And Carry_Flag)
            Test_Flag(Result > &HFFFF, Carry_Flag)
            Test_Flag(((Not (Registers.A Xor Value)) And (Registers.A Xor Result) And &H8000), Overflow_Flag)
            Registers.A = Result And &HFFFF
            Set_Zero_Negative_Flag_16(Registers.A)
        Else
            Add_With_Carry_BCD_16()
        End If
    End Sub
    Private Sub Add_With_Carry_BCD_16() 'ADC (BCD) (16 bits)
        Dim Value As Integer = Read_Memory_16((Effective_Address And &HFF0000) / &H10000, Effective_Address And &HFFFF)
        Dim Result As Integer = (Registers.A And &HF) + (Value And &HF) + (Registers.P And Carry_Flag)
        If Result > 9 Then Result += 6
        Test_Flag(Result > &HF, Carry_Flag)
        Result = (Registers.A And &HF0) + (Value And &HF0) + (Result And &HF) + ((Registers.P And Carry_Flag) * &H10)
        If Result > &H9F Then Result += &H60
        Test_Flag(Result > &HFF, Carry_Flag)
        Result = (Registers.A And &HF00) + (Value And &HF00) + (Result And &HFF) + ((Registers.P And Carry_Flag) * &H100)
        If Result > &H9FF Then Result += &H600
        Test_Flag(Result > &HFFF, Carry_Flag)
        Result = (Registers.A And &HF000) + (Value And &HF000) + (Result And &HFFF) + ((Registers.P And Carry_Flag) * &H1000)
        Test_Flag(((Not (Registers.A Xor Value)) And (Registers.A Xor Result) And &H8000), Overflow_Flag)
        If Result > &H9FFF Then Result += &H6000
        Test_Flag(Result > &HFFFF, Carry_Flag)
        Registers.A = Result And &HFFFF
        Set_Zero_Negative_Flag_16(Registers.A)
    End Sub
    Private Sub And_With_Accumulator() 'AND (8 bits)
        Dim Value As Byte = Read_Memory((Effective_Address And &HFF0000) / &H10000, Effective_Address And &HFFFF)
        Registers.A = (Value And (Registers.A And &HFF)) + (Registers.A And &HFF00)
        Set_Zero_Negative_Flag(Registers.A And &HFF)
    End Sub
    Private Sub And_With_Accumulator_16() 'AND (16 bits)
        Dim Value As Integer = Read_Memory_16((Effective_Address And &HFF0000) / &H10000, Effective_Address And &HFFFF)
        Registers.A = Registers.A And Value
        Set_Zero_Negative_Flag_16(Registers.A)
    End Sub
    Private Sub Arithmetic_Shift_Left() 'ASL (8 bits)
        Dim Value As Byte = Read_Memory((Effective_Address And &HFF0000) / &H10000, Effective_Address And &HFFFF)
        Test_Flag(Value And &H80, Carry_Flag)
        Value = (Value << 1) And &HFF
        Write_Memory((Effective_Address And &HFF0000) / &H10000, Effective_Address And &HFFFF, Value)
        Set_Zero_Negative_Flag(Value)
    End Sub
    Private Sub Arithmetic_Shift_Left_A() 'ASL_A (8 bits)
        Test_Flag((Registers.A And &HFF) And &H80, Carry_Flag)
        Registers.A = (((Registers.A And &HFF) << 1) And &HFF) + (Registers.A And &HFF00)
        Set_Zero_Negative_Flag(Registers.A And &HFF)
    End Sub
    Private Sub Arithmetic_Shift_Left_16() 'ASL (16 bits)
        Dim Value As Integer = Read_Memory_16((Effective_Address And &HFF0000) / &H10000, Effective_Address And &HFFFF)
        Test_Flag(Value And &H8000, Carry_Flag)
        Value = (Value << 1) And &HFFFF
        Write_Memory_16((Effective_Address And &HFF0000) / &H10000, Effective_Address And &HFFFF, Value)
        Set_Zero_Negative_Flag_16(Value)
    End Sub
    Private Sub Arithmetic_Shift_Left_A_16() 'ASL_A (16 bits)
        Test_Flag(Registers.A And &H8000, Carry_Flag)
        Registers.A = (Registers.A << 1) And &HFFFF
        Set_Zero_Negative_Flag_16(Registers.A)
    End Sub
    Private Sub Branch_On_Carry_Clear() 'BCC
        Dim Offset As SByte = Signed_Byte(Read_Memory(Registers.Program_Bank, Registers.Program_Counter))
        Registers.Program_Counter += 1
        If (Registers.P And Carry_Flag) = 0 Then
            Registers.Program_Counter += Offset
            Cycles += 1
        End If
    End Sub
    Private Sub Branch_On_Carry_Set() 'BCS
        Dim Offset As SByte = Signed_Byte(Read_Memory(Registers.Program_Bank, Registers.Program_Counter))
        Registers.Program_Counter += 1
        If (Registers.P And Carry_Flag) Then Registers.Program_Counter += Offset
    End Sub
    Private Sub Branch_On_Equal() 'BEQ
        Dim Offset As SByte = Signed_Byte(Read_Memory(Registers.Program_Bank, Registers.Program_Counter))
        Registers.Program_Counter += 1
        If (Registers.P And Zero_Flag) Then Registers.Program_Counter += Offset
    End Sub
    Private Sub Test_Bits() 'BIT (8 bits)
        Dim Value As Byte = Read_Memory((Effective_Address And &HFF0000) / &H10000, Effective_Address And &HFFFF)
        Test_Flag((Value And (Registers.A And &HFF)) = 0, Zero_Flag)
        Test_Flag(Value And &H80, Negative_Flag)
        Test_Flag(Value And &H40, Overflow_Flag)
    End Sub
    Private Sub Test_Bits_16() 'BIT (16 bits)
        Dim Value As Integer = Read_Memory_16((Effective_Address And &HFF0000) / &H10000, Effective_Address And &HFFFF)
        Test_Flag((Value And Registers.A) = 0, Zero_Flag)
        Test_Flag(Value And &H8000, Negative_Flag)
        Test_Flag(Value And &H4000, Overflow_Flag)
    End Sub
    Private Sub Branch_On_Minus() 'BMI
        Dim Offset As SByte = Signed_Byte(Read_Memory(Registers.Program_Bank, Registers.Program_Counter))
        Registers.Program_Counter += 1
        If (Registers.P And Negative_Flag) Then Registers.Program_Counter += Offset
    End Sub
    Private Sub Branch_On_Not_Equal() 'BNE
        Dim Offset As SByte = Signed_Byte(Read_Memory(Registers.Program_Bank, Registers.Program_Counter))
        Registers.Program_Counter += 1
        If (Registers.P And Zero_Flag) = 0 Then Registers.Program_Counter += Offset
    End Sub
    Private Sub Branch_On_Plus() 'BPL
        Dim Offset As SByte = Signed_Byte(Read_Memory(Registers.Program_Bank, Registers.Program_Counter))
        Registers.Program_Counter += 1
        If (Registers.P And Negative_Flag) = 0 Then Registers.Program_Counter += Offset
    End Sub
    Private Sub Branch_Always() 'BRA
        Dim Offset As SByte = Signed_Byte(Read_Memory(Registers.Program_Bank, Registers.Program_Counter))
        Registers.Program_Counter += 1
        Registers.Program_Counter += Offset
    End Sub
    Private Sub Break() 'BRK
        If Emulate_6502 Then
            Push_16(Registers.Program_Counter)
            Push(Registers.P Or &H30)
            Registers.Program_Bank = 0
            Registers.Program_Counter = Read_Memory_16(0, &HFFFE)
        Else
            Push(Registers.Program_Bank)
            Push_16(Registers.Program_Counter)
            Push(Registers.P)
            Registers.Program_Bank = 0
            Registers.Program_Counter = Read_Memory_16(0, &HFFE6)
        End If
    End Sub
    Private Sub Branch_Long_Always() 'BRL
        Dim Offset As Integer = Signed_Integer(Read_Memory_16(Registers.Program_Bank, Registers.Program_Counter))
        Registers.Program_Counter += 2
        Registers.Program_Counter += Offset
    End Sub
    Private Sub Branch_On_Overflow_Clear() 'BVC
        Dim Offset As SByte = Signed_Byte(Read_Memory(Registers.Program_Bank, Registers.Program_Counter))
        Registers.Program_Counter += 1
        If (Registers.P And Overflow_Flag) = 0 Then Registers.Program_Counter += Offset
    End Sub
    Private Sub Branch_On_Overflow_Set() 'BVS
        Dim Offset As SByte = Signed_Byte(Read_Memory(Registers.Program_Bank, Registers.Program_Counter))
        Registers.Program_Counter += 1
        If (Registers.P And Overflow_Flag) Then Registers.Program_Counter += Offset
    End Sub
    Private Sub Clear_Carry() 'CLC
        Clear_Flag(Carry_Flag)
    End Sub
    Private Sub Clear_Decimal() 'CLD
        Clear_Flag(Decimal_Flag)
    End Sub
    Private Sub Clear_Interrupt_Disable() 'CLI
        Clear_Flag(Interrupt_Flag)
    End Sub
    Private Sub Clear_Overflow() 'CLV
        Clear_Flag(Overflow_Flag)
    End Sub
    Private Sub Compare() 'CMP (8 bits)
        Dim Value As Byte = Read_Memory((Effective_Address And &HFF0000) / &H10000, Effective_Address And &HFFFF)
        Dim Result As Integer = (Registers.A And &HFF) - Value
        Test_Flag((Registers.A And &HFF) >= Value, Carry_Flag)
        Set_Zero_Negative_Flag(Result And &HFF)
    End Sub
    Private Sub Compare_16() 'CMP (16 bits)
        Dim Value As Integer = Read_Memory_16((Effective_Address And &HFF0000) / &H10000, Effective_Address And &HFFFF)
        Dim Result As Integer = Registers.A - Value
        Test_Flag(Registers.A >= Value, Carry_Flag)
        Set_Zero_Negative_Flag_16(Result)
    End Sub
    Private Sub CoP_Enable()
        Push(Registers.Program_Bank)
        Push_16(Registers.Program_Counter)
        Push(Registers.P)
        Registers.Program_Bank = 0
        Registers.Program_Counter = Read_Memory_16(0, &HFFE4)
        Set_Flag(Interrupt_Flag)
    End Sub
    Private Sub Compare_With_X() 'CPX (8 bits)
        Dim Value As Byte = Read_Memory((Effective_Address And &HFF0000) / &H10000, Effective_Address And &HFFFF)
        Dim Result As Integer = (Registers.X And &HFF) - Value
        Test_Flag((Registers.X And &HFF) >= Value, Carry_Flag)
        Set_Zero_Negative_Flag(Result And &HFF)
    End Sub
    Private Sub Compare_With_X_16() 'CPX (16 bits)
        Dim Value As Integer = Read_Memory_16((Effective_Address And &HFF0000) / &H10000, Effective_Address And &HFFFF)
        Dim Result As Integer = Registers.X - Value
        Test_Flag(Registers.X >= Value, Carry_Flag)
        Set_Zero_Negative_Flag_16(Result)
    End Sub
    Private Sub Compare_With_Y() 'CPY (8 bits)
        Dim Value As Byte = Read_Memory((Effective_Address And &HFF0000) / &H10000, Effective_Address And &HFFFF)
        Dim Result As Integer = (Registers.Y And &HFF) - Value
        Test_Flag((Registers.Y And &HFF) >= Value, Carry_Flag)
        Set_Zero_Negative_Flag(Result And &HFF)
    End Sub
    Private Sub Compare_With_Y_16() 'CPY (16 bits)
        Dim Value As Integer = Read_Memory_16((Effective_Address And &HFF0000) / &H10000, Effective_Address And &HFFFF)
        Dim Result As Integer = Registers.Y - Value
        Test_Flag(Registers.Y >= Value, Carry_Flag)
        Set_Zero_Negative_Flag_16(Result)
    End Sub
    Private Sub Decrement() 'DEC (8 bits)
        Dim Value As Byte = (Read_Memory((Effective_Address And &HFF0000) / &H10000, Effective_Address And &HFFFF) - 1) And &HFF
        Set_Zero_Negative_Flag(Value)
        Write_Memory((Effective_Address And &HFF0000) / &H10000, Effective_Address And &HFFFF, Value)
    End Sub
    Private Sub Decrement_A() 'DEC (8 bits)
        Registers.A = ((Registers.A - 1) And &HFF) + (Registers.A And &HFF00)
        Set_Zero_Negative_Flag(Registers.A And &HFF)
    End Sub
    Private Sub Decrement_16() 'DEC (16 bits)
        Dim Value As Integer = (Read_Memory_16((Effective_Address And &HFF0000) / &H10000, Effective_Address And &HFFFF) - 1) And &HFFFF
        Set_Zero_Negative_Flag_16(Value)
        Write_Memory_16((Effective_Address And &HFF0000) / &H10000, Effective_Address And &HFFFF, Value)
    End Sub
    Private Sub Decrement_A_16() 'DEC (16 bits)
        Registers.A = (Registers.A - 1) And &HFFFF
        Set_Zero_Negative_Flag_16(Registers.A)
    End Sub
    Private Sub Decrement_X() 'DEX (8 bits)
        Registers.X = ((Registers.X - 1) And &HFF) + (Registers.X And &HFF00)
        Set_Zero_Negative_Flag(Registers.X And &HFF)
    End Sub
    Private Sub Decrement_X_16() 'DEX (16 bits)
        Registers.X = (Registers.X - 1) And &HFFFF
        Set_Zero_Negative_Flag_16(Registers.X)
    End Sub
    Private Sub Decrement_Y() 'DEY (8 bits)
        Registers.Y = ((Registers.Y - 1) And &HFF) + (Registers.Y And &HFF00)
        Set_Zero_Negative_Flag(Registers.Y And &HFF)
    End Sub
    Private Sub Decrement_Y_16() 'DEY (16 bits)
        Registers.Y = (Registers.Y - 1) And &HFFFF
        Set_Zero_Negative_Flag_16(Registers.Y)
    End Sub
    Private Sub Exclusive_Or() 'EOR (8 bits)
        Dim Value As Byte = Read_Memory((Effective_Address And &HFF0000) / &H10000, Effective_Address And &HFFFF)
        Registers.A = ((Registers.A And &HFF) Xor Value) + (Registers.A And &HFF00)
        Set_Zero_Negative_Flag(Registers.A And &HFF)
    End Sub
    Private Sub Exclusive_Or_16() 'EOR (16 bits)
        Dim Value As Integer = Read_Memory_16((Effective_Address And &HFF0000) / &H10000, Effective_Address And &HFFFF)
        Registers.A = Registers.A Xor Value
        Set_Zero_Negative_Flag_16(Registers.A)
    End Sub
    Private Sub Increment() 'INC (8 bits)
        Dim Value As Byte = (Read_Memory((Effective_Address And &HFF0000) / &H10000, Effective_Address And &HFFFF) + 1) And &HFF
        Set_Zero_Negative_Flag(Value)
        Write_Memory((Effective_Address And &HFF0000) / &H10000, Effective_Address And &HFFFF, Value)
    End Sub
    Private Sub Increment_A() 'INC (8 bits)
        Registers.A = ((Registers.A + 1) And &HFF) + (Registers.A And &HFF00)
        Set_Zero_Negative_Flag(Registers.A And &HFF)
    End Sub
    Private Sub Increment_16() 'INC (16 bits)
        Dim Value As Integer = (Read_Memory_16((Effective_Address And &HFF0000) / &H10000, Effective_Address And &HFFFF) + 1) And &HFFFF
        Set_Zero_Negative_Flag_16(Value)
        Write_Memory_16((Effective_Address And &HFF0000) / &H10000, Effective_Address And &HFFFF, Value)
    End Sub
    Private Sub Increment_A_16() 'INC (16 bits)
        Registers.A = (Registers.A + 1) And &HFFFF
        Set_Zero_Negative_Flag_16(Registers.A)
    End Sub
    Private Sub Increment_X() 'INX (8 bits)
        Registers.X = ((Registers.X + 1) And &HFF) + (Registers.X And &HFF00)
        Set_Zero_Negative_Flag(Registers.X And &HFF)
    End Sub
    Private Sub Increment_X_16() 'INX (16 bits)
        Registers.X = (Registers.X + 1) And &HFFFF
        Set_Zero_Negative_Flag_16(Registers.X)
    End Sub
    Private Sub Increment_Y() 'INY (8 bits)
        Registers.Y = ((Registers.Y + 1) And &HFF) + (Registers.Y And &HFF00)
        Set_Zero_Negative_Flag(Registers.Y And &HFF)
    End Sub
    Private Sub Increment_Y_16() 'INY (16 bits)
        Registers.Y = (Registers.Y + 1) And &HFFFF
        Set_Zero_Negative_Flag_16(Registers.Y)
    End Sub
    Private Sub Jump() 'JMP
        Registers.Program_Counter = Effective_Address And &HFFFF
    End Sub
    Private Sub Jump_To_Subroutine(Optional DBR As Boolean = False) 'JSR
        If DBR Then
            Push(Registers.Program_Bank)
            Registers.Program_Bank = (Effective_Address And &HFF0000) / &H10000
        End If
        Push_16((Registers.Program_Counter - 1) And &HFFFF)
        Registers.Program_Counter = Effective_Address And &HFFFF
    End Sub
    Private Sub Load_Accumulator() 'LDA (8 bits)
        Registers.A = Read_Memory((Effective_Address And &HFF0000) / &H10000, Effective_Address And &HFFFF) + (Registers.A And &HFF00)
        Set_Zero_Negative_Flag(Registers.A And &HFF)
    End Sub
    Private Sub Load_Accumulator_16() 'LDA (16 bits)
        Registers.A = Read_Memory_16((Effective_Address And &HFF0000) / &H10000, Effective_Address And &HFFFF)
        Set_Zero_Negative_Flag_16(Registers.A)
    End Sub
    Private Sub Load_X() 'LDX (8 bits)
        Registers.X = Read_Memory((Effective_Address And &HFF0000) / &H10000, Effective_Address And &HFFFF) + (Registers.X And &HFF00)
        Set_Zero_Negative_Flag(Registers.X And &HFF)
    End Sub
    Private Sub Load_X_16() 'LDX (16 bits)
        Registers.X = Read_Memory_16((Effective_Address And &HFF0000) / &H10000, Effective_Address And &HFFFF)
        Set_Zero_Negative_Flag_16(Registers.X)
    End Sub
    Private Sub Load_Y() 'LDY (8 bits)
        Registers.Y = Read_Memory((Effective_Address And &HFF0000) / &H10000, Effective_Address And &HFFFF) + (Registers.Y And &HFF00)
        Set_Zero_Negative_Flag(Registers.Y And &HFF)
    End Sub
    Private Sub Load_Y_16() 'LDY (16 bits)
        Registers.Y = Read_Memory_16((Effective_Address And &HFF0000) / &H10000, Effective_Address And &HFFFF)
        Set_Zero_Negative_Flag_16(Registers.Y)
    End Sub
    Private Sub Logical_Shift_Right() 'LSR (8 bits)
        Dim Value As Byte = Read_Memory((Effective_Address And &HFF0000) / &H10000, Effective_Address And &HFFFF)
        Test_Flag(Value And &H1, Carry_Flag)
        Value = (Value >> 1) And &HFF
        Write_Memory((Effective_Address And &HFF0000) / &H10000, Effective_Address And &HFFFF, Value)
        Set_Zero_Negative_Flag(Value)
    End Sub
    Private Sub Logical_Shift_Right_A() 'LSR_A (8 bits)
        Test_Flag((Registers.A And &HFF) And &H1, Carry_Flag)
        Registers.A = (((Registers.A And &HFF) >> 1) And &HFF) + (Registers.A And &HFF00)
        Set_Zero_Negative_Flag(Registers.A And &HFF)
    End Sub
    Private Sub Logical_Shift_Right_16() 'LSR (16 bits)
        Dim Value As Integer = Read_Memory_16((Effective_Address And &HFF0000) / &H10000, Effective_Address And &HFFFF)
        Test_Flag(Value And &H1, Carry_Flag)
        Value = (Value >> 1) And &HFFFF
        Write_Memory_16((Effective_Address And &HFF0000) / &H10000, Effective_Address And &HFFFF, Value)
        Set_Zero_Negative_Flag_16(Value)
    End Sub
    Private Sub Logical_Shift_Right_A_16() 'LSR_A (16 bits)
        Test_Flag(Registers.A And &H1, Carry_Flag)
        Registers.A = (Registers.A >> 1) And &HFFFF
        Set_Zero_Negative_Flag_16(Registers.A)
    End Sub
    Private Sub Block_Move_Negative() 'MVN
        Registers.Data_Bank = Read_Memory(Registers.Program_Bank, Registers.Program_Counter)
        Dim Bank As Byte = Read_Memory(Registers.Program_Bank, Registers.Program_Counter + 1)
        Registers.Program_Counter += 2
        Dim Byte_To_Transfer As Byte = Read_Memory(Bank, Registers.X)
        Registers.X = (Registers.X + 1) And &HFFFF
        Write_Memory(Registers.Data_Bank, Registers.Y, Byte_To_Transfer)
        Registers.Y = (Registers.Y + 1) And &HFFFF
        Registers.A = (Registers.A - 1) And &HFFFF
        If Registers.A <> &HFFFF Then Registers.Program_Counter -= 3
    End Sub
    Private Sub Block_Move_Positive() 'MVP
        Registers.Data_Bank = Read_Memory(Registers.Program_Bank, Registers.Program_Counter)
        Dim Bank As Byte = Read_Memory(Registers.Program_Bank, Registers.Program_Counter + 1)
        Registers.Program_Counter += 2
        Dim Byte_To_Transfer As Byte = Read_Memory(Bank, Registers.X)
        Registers.X = (Registers.X - 1) And &HFFFF
        Write_Memory(Registers.Data_Bank, Registers.Y, Byte_To_Transfer)
        Registers.Y = (Registers.Y - 1) And &HFFFF
        Registers.A = (Registers.A - 1) And &HFFFF
        If Registers.A <> &HFFFF Then Registers.Program_Counter -= 3
    End Sub
    Private Sub Or_With_Accumulator() 'ORA (8 bits)
        Dim Value As Byte = Read_Memory((Effective_Address And &HFF0000) / &H10000, Effective_Address And &HFFFF)
        Registers.A = ((Registers.A And &HFF) Or Value) + (Registers.A And &HFF00)
        Set_Zero_Negative_Flag(Registers.A And &HFF)
    End Sub
    Private Sub Or_With_Accumulator_16() 'ORA (16 bits)
        Dim Value As Integer = Read_Memory_16((Effective_Address And &HFF0000) / &H10000, Effective_Address And &HFFFF)
        Registers.A = Registers.A Or Value
        Set_Zero_Negative_Flag_16(Registers.A)
    End Sub
    Private Sub Push_Effective_Address() 'PEA/PEI/PER
        Push_16(Effective_Address)
    End Sub
    Private Sub Push_Accumulator() 'PHA (8 bits)
        Push(Registers.A And &HFF)
    End Sub
    Private Sub Push_Accumulator_16() 'PHA (16 bits)
        Push_16(Registers.A)
    End Sub
    Private Sub Push_Data_Bank() 'PHB
        Push(Registers.Data_Bank)
    End Sub
    Private Sub Push_Direct_Page() 'PHD
        Push_16(Registers.Direct_Page)
    End Sub
    Private Sub Push_Program_Bank() 'PHK
        Push(Registers.Program_Bank)
    End Sub
    Private Sub Push_Processor_Status() 'PHP
        Push(Registers.P)
    End Sub
    Private Sub Push_X() 'PHX (8 bits)
        Push(Registers.X And &HFF)
    End Sub
    Private Sub Push_X_16() 'PHX (16 bits)
        Push_16(Registers.X)
    End Sub
    Private Sub Push_Y() 'PHY (8 bits)
        Push(Registers.Y And &HFF)
    End Sub
    Private Sub Push_Y_16() 'PHY (16 bits)
        Push_16(Registers.Y)
    End Sub
    Private Sub Pull_Accumulator() 'PLA (8 bits)
        Registers.A = Pull() + (Registers.A And &HFF00)
        Set_Zero_Negative_Flag(Registers.A And &HFF)
    End Sub
    Private Sub Pull_Accumulator_16() 'PLA (16 bits)
        Registers.A = Pull_16()
        Set_Zero_Negative_Flag_16(Registers.A)
    End Sub
    Private Sub Pull_Data_Bank() 'PLB
        Registers.Data_Bank = Pull()
    End Sub
    Private Sub Pull_Direct_Page() 'PLD
        Registers.Direct_Page = Pull_16()
    End Sub
    Private Sub Pull_Processor_Status() 'PLP
        Registers.P = Pull()
        Update_Mode()
    End Sub
    Private Sub Pull_X() 'PLX (8 bits)
        Registers.X = Pull() + (Registers.X And &HFF00)
        Set_Zero_Negative_Flag(Registers.X And &HFF)
    End Sub
    Private Sub Pull_X_16() 'PLX (16 bits)
        Registers.X = Pull_16()
        Set_Zero_Negative_Flag_16(Registers.X)
    End Sub
    Private Sub Pull_Y() 'PLY (8 bits)
        Registers.Y = Pull() + (Registers.Y And &HFF00)
        Set_Zero_Negative_Flag(Registers.Y And &HFF)
    End Sub
    Private Sub Pull_Y_16() 'PLY (16 bits)
        Registers.Y = Pull_16()
        Set_Zero_Negative_Flag_16(Registers.Y)
    End Sub
    Private Sub Reset_Status() 'REP
        Dim Value As Byte = Read_Memory((Effective_Address And &HFF0000) / &H10000, Effective_Address And &HFFFF)
        Clear_Flag(Value)
        Update_Mode()
    End Sub
    Private Sub Rotate_Left() 'ROL (8 bits)
        Dim Value As Byte = Read_Memory((Effective_Address And &HFF0000) / &H10000, Effective_Address And &HFFFF)
        If (Registers.P And Carry_Flag) Then
            Test_Flag(Value And &H80, Carry_Flag)
            Value = ((Value << 1) And &HFF) Or &H1
        Else
            Test_Flag(Value And &H80, Carry_Flag)
            Value = (Value << 1) And &HFF
        End If
        Write_Memory((Effective_Address And &HFF0000) / &H10000, Effective_Address And &HFFFF, Value)
        Set_Zero_Negative_Flag(Value)
    End Sub
    Private Sub Rotate_Left_A() 'ROL (8 bits)
        If (Registers.P And Carry_Flag) Then
            Test_Flag(Registers.A And &H80, Carry_Flag)
            Registers.A = ((((Registers.A And &HFF) << 1) And &HFF) Or &H1) + (Registers.A And &HFF00)
        Else
            Test_Flag(Registers.A And &H80, Carry_Flag)
            Registers.A = (((Registers.A And &HFF) << 1) And &HFF) + (Registers.A And &HFF00)
        End If
        Set_Zero_Negative_Flag(Registers.A And &HFF)
    End Sub
    Private Sub Rotate_Left_16() 'ROL (16 bits)
        Dim Value As Integer = Read_Memory_16((Effective_Address And &HFF0000) / &H10000, Effective_Address And &HFFFF)
        If (Registers.P And Carry_Flag) Then
            Test_Flag(Value And &H8000, Carry_Flag)
            Value = ((Value << 1) And &HFFFF) Or &H1
        Else
            Test_Flag(Value And &H8000, Carry_Flag)
            Value = (Value << 1) And &HFFFF
        End If
        Write_Memory_16((Effective_Address And &HFF0000) / &H10000, Effective_Address And &HFFFF, Value)
        Set_Zero_Negative_Flag_16(Value)
    End Sub
    Private Sub Rotate_Left_A_16() 'ROL (16 bits)
        If (Registers.P And Carry_Flag) Then
            Test_Flag(Registers.A And &H8000, Carry_Flag)
            Registers.A = ((Registers.A << 1) And &HFFFF) Or &H1
        Else
            Test_Flag(Registers.A And &H8000, Carry_Flag)
            Registers.A = (Registers.A << 1) And &HFFFF
        End If
        Set_Zero_Negative_Flag_16(Registers.A)
    End Sub
    Private Sub Rotate_Right() 'ROR (8 bits)
        Dim Value As Byte = Read_Memory((Effective_Address And &HFF0000) / &H10000, Effective_Address And &HFFFF)
        If (Registers.P And Carry_Flag) Then
            Test_Flag(Value And &H1, Carry_Flag)
            Value = ((Value >> 1) And &HFF) Or &H80
        Else
            Test_Flag(Value And &H1, Carry_Flag)
            Value = (Value >> 1) And &HFF
        End If
        Write_Memory((Effective_Address And &HFF0000) / &H10000, Effective_Address And &HFFFF, Value)
        Set_Zero_Negative_Flag(Value)
    End Sub
    Private Sub Rotate_Right_A() 'ROR (8 bits)
        If (Registers.P And Carry_Flag) Then
            Test_Flag(Registers.A And &H1, Carry_Flag)
            Registers.A = ((((Registers.A And &HFF) >> 1) And &HFF) Or &H80) + (Registers.A And &HFF00)
        Else
            Test_Flag(Registers.A And &H1, Carry_Flag)
            Registers.A = (((Registers.A And &HFF) >> 1) And &HFF) + (Registers.A And &HFF00)
        End If
        Set_Zero_Negative_Flag(Registers.A And &HFF)
    End Sub
    Private Sub Rotate_Right_16() 'ROR (16 bits)
        Dim Value As Integer = Read_Memory_16((Effective_Address And &HFF0000) / &H10000, Effective_Address And &HFFFF)
        If (Registers.P And Carry_Flag) Then
            Test_Flag(Value And &H1, Carry_Flag)
            Value = ((Value >> 1) And &HFFFF) Or &H8000
        Else
            Test_Flag(Value And &H1, Carry_Flag)
            Value = (Value >> 1) And &HFFFF
        End If
        Write_Memory_16((Effective_Address And &HFF0000) / &H10000, Effective_Address And &HFFFF, Value)
        Set_Zero_Negative_Flag_16(Value)
    End Sub
    Private Sub Rotate_Right_A_16() 'ROR (16 bits)
        If (Registers.P And Carry_Flag) Then
            Test_Flag(Registers.A And &H1, Carry_Flag)
            Registers.A = ((Registers.A >> 1) And &HFFFF) Or &H8000
        Else
            Test_Flag(Registers.A And &H1, Carry_Flag)
            Registers.A = (Registers.A >> 1) And &HFFFF
        End If
        Set_Zero_Negative_Flag_16(Registers.A)
    End Sub
    Private Sub Return_From_Interrupt() 'RTI
        Registers.P = Pull()
        Registers.Program_Counter = Pull_16()
        Registers.Program_Bank = Pull()
    End Sub
    Private Sub Return_From_Subroutine_Long() 'RTL
        Registers.Program_Counter = Pull_16()
        Registers.Program_Counter += 1
        Registers.Program_Bank = Pull()
    End Sub
    Private Sub Return_From_Subroutine() 'RTS
        Registers.Program_Counter = Pull_16()
        Registers.Program_Counter += 1
    End Sub
    Private Sub Subtract_With_Carry() 'SBC (8 bits)
        If (Registers.P And Decimal_Flag) = 0 Then
            Dim Value As Byte = Read_Memory((Effective_Address And &HFF0000) / &H10000, Effective_Address And &HFFFF) Xor &HFF
            Dim Result As Integer = (Registers.A And &HFF) + Value + (Registers.P And Carry_Flag)
            Test_Flag(Result > &HFF, Carry_Flag)
            Test_Flag(((Not ((Registers.A And &HFF) Xor Value)) And ((Registers.A And &HFF) Xor Result) And &H80), Overflow_Flag)
            Registers.A = (Result And &HFF) + (Registers.A And &HFF00)
            Set_Zero_Negative_Flag(Registers.A And &HFF)
        Else
            Subtract_With_Carry_BCD()
        End If
    End Sub
    Private Sub Subtract_With_Carry_BCD() 'SBC (BCD) (8 bits)
        Dim Value As Byte = Read_Memory((Effective_Address And &HFF0000) / &H10000, Effective_Address And &HFFFF) Xor &HFF
        Dim Result As Integer = (Registers.A And &HF) + (Value And &HF) + (Registers.P And Carry_Flag)
        If Result < &H10 Then Result -= 6
        Test_Flag(Result > &HF, Carry_Flag)
        Result = (Registers.A And &HF0) + (Value And &HF0) + (Result And &HF) + ((Registers.P And Carry_Flag) * &H10)
        Test_Flag(((Not ((Registers.A And &HFF) Xor Value)) And ((Registers.A And &HFF) Xor Result) And &H80), Overflow_Flag)
        If Result < &H100 Then Result -= &H60
        Test_Flag(Result > &HFF, Carry_Flag)
        Registers.A = (Result And &HFF) + (Registers.A And &HFF00)
        Set_Zero_Negative_Flag(Registers.A And &HFF)
    End Sub
    Private Sub Subtract_With_Carry_16() 'SBC (16 bits)
        If (Registers.P And Decimal_Flag) = 0 Then
            Dim Value As Integer = Read_Memory_16((Effective_Address And &HFF0000) / &H10000, Effective_Address And &HFFFF) Xor &HFFFF
            Dim Result As Integer = Registers.A + Value + (Registers.P And Carry_Flag)
            Test_Flag(Result > &HFFFF, Carry_Flag)
            Test_Flag(((Not (Registers.A Xor Value)) And (Registers.A Xor Result) And &H8000), Overflow_Flag)
            Registers.A = Result And &HFFFF
            Set_Zero_Negative_Flag_16(Registers.A)
        Else
            Subtract_With_Carry_BCD_16()
        End If
    End Sub
    Private Sub Subtract_With_Carry_BCD_16() 'SBC (BCD) (16 bits)
        Dim Value As Integer = Read_Memory_16((Effective_Address And &HFF0000) / &H10000, Effective_Address And &HFFFF) Xor &HFFFF
        Dim Result As Integer = (Registers.A And &HF) + (Value And &HF) + (Registers.P And Carry_Flag)
        If Result < &H10 Then Result -= 6
        Test_Flag(Result > &HF, Carry_Flag)
        Result = (Registers.A And &HF0) + (Value And &HF0) + (Result And &HF) + ((Registers.P And Carry_Flag) * &H10)
        If Result < &H100 Then Result -= &H60
        Test_Flag(Result > &HFF, Carry_Flag)
        Result = (Registers.A And &HF00) + (Value And &HF00) + (Result And &HFF) + ((Registers.P And Carry_Flag) * &H100)
        If Result < &H1000 Then Result -= &H600
        Test_Flag(Result > &HFFF, Carry_Flag)
        Result = (Registers.A And &HF000) + (Value And &HF000) + (Result And &HFFF) + ((Registers.P And Carry_Flag) * &H1000)
        Test_Flag(((Not (Registers.A Xor Value)) And (Registers.A Xor Result) And &H8000), Overflow_Flag)
        If Result < &H10000 Then Result -= &H6000
        Test_Flag(Result > &HFFFF, Carry_Flag)
        Registers.A = Result And &HFFFF
        Set_Zero_Negative_Flag_16(Registers.A)
    End Sub
    Private Sub Set_Carry() 'SEC
        Set_Flag(Carry_Flag)
    End Sub
    Private Sub Set_Decimal() 'SED
        Set_Flag(Decimal_Flag)
    End Sub
    Private Sub Set_Interrupt_Disable() 'SEI
        Set_Flag(Interrupt_Flag)
    End Sub
    Private Sub Set_Status() 'SEP
        Dim Value As Byte = Read_Memory((Effective_Address And &HFF0000) / &H10000, Effective_Address And &HFFFF)
        Set_Flag(Value)
        Update_Mode()
    End Sub
    Private Sub Store_Accumulator() 'STA (8 bits)
        Write_Memory((Effective_Address And &HFF0000) / &H10000, Effective_Address And &HFFFF, Registers.A And &HFF)
    End Sub
    Private Sub Store_Accumulator_16() 'STA (16 bits)
        Write_Memory_16((Effective_Address And &HFF0000) / &H10000, Effective_Address And &HFFFF, Registers.A)
    End Sub
    Private Sub Stop_Processor()
        STP_Disable = True
    End Sub
    Private Sub Store_X() 'STX (8 bits)
        Write_Memory((Effective_Address And &HFF0000) / &H10000, Effective_Address And &HFFFF, Registers.X And &HFF)
    End Sub
    Private Sub Store_X_16() 'STX (16 bits)
        Write_Memory_16((Effective_Address And &HFF0000) / &H10000, Effective_Address And &HFFFF, Registers.X)
    End Sub
    Private Sub Store_Y() 'STY (8 bits)
        Write_Memory((Effective_Address And &HFF0000) / &H10000, Effective_Address And &HFFFF, Registers.Y And &HFF)
    End Sub
    Private Sub Store_Y_16() 'STY (16 bits)
        Write_Memory_16((Effective_Address And &HFF0000) / &H10000, Effective_Address And &HFFFF, Registers.Y)
    End Sub
    Private Sub Store_Zero() 'STZ (8 bits)
        Write_Memory((Effective_Address And &HFF0000) / &H10000, Effective_Address And &HFFFF, 0)
    End Sub
    Private Sub Store_Zero_16() 'STZ (16 bits)
        Write_Memory_16((Effective_Address And &HFF0000) / &H10000, Effective_Address And &HFFFF, 0)
    End Sub
    Private Sub Transfer_Accumulator_To_X() 'TAX (8 bits)
        Registers.X = (Registers.A And &HFF) + (Registers.X And &HFF00)
        Set_Zero_Negative_Flag(Registers.X And &HFF)
    End Sub
    Private Sub Transfer_Accumulator_To_X_16() 'TAX (16 bits)
        Registers.X = Registers.A
        Set_Zero_Negative_Flag_16(Registers.X)
    End Sub
    Private Sub Transfer_Accumulator_To_Y() 'TAY (8 bits)
        Registers.Y = (Registers.A And &HFF) + (Registers.Y And &HFF00)
        Set_Zero_Negative_Flag(Registers.Y And &HFF)
    End Sub
    Private Sub Transfer_Accumulator_To_Y_16() 'TAY (16 bits)
        Registers.Y = Registers.A
        Set_Zero_Negative_Flag_16(Registers.Y)
    End Sub
    Private Sub Transfer_Accumulator_To_DP() 'TCD
        Registers.Direct_Page = Registers.A
        Set_Zero_Negative_Flag_16(Registers.Direct_Page)
    End Sub
    Private Sub Transfer_Accumulator_To_SP() 'TCS
        Registers.Stack_Pointer = Registers.A
    End Sub
    Private Sub Transfer_DP_To_Accumulator() 'TDC
        Registers.A = Registers.Direct_Page
        Set_Zero_Negative_Flag_16(Registers.A)
    End Sub
    Private Sub Test_And_Reset_Bit() 'TRB (8 bits)
        Dim Value As Byte = Read_Memory((Effective_Address And &HFF0000) / &H10000, Effective_Address And &HFFFF)
        Test_Flag(Not ((Registers.A And &HFF) And Value), Zero_Flag)
        Value = Value And Not (Registers.A And &HFF)
        Write_Memory((Effective_Address And &HFF0000) / &H10000, Effective_Address And &HFFFF, Value)
    End Sub
    Private Sub Test_And_Reset_Bit_16() 'TRB (16 bits)
        Dim Value As Integer = Read_Memory_16((Effective_Address And &HFF0000) / &H10000, Effective_Address And &HFFFF)
        Test_Flag(Not (Registers.A And Value), Zero_Flag)
        Value = Value And Not Registers.A
        Write_Memory_16((Effective_Address And &HFF0000) / &H10000, Effective_Address And &HFFFF, Value)
    End Sub
    Private Sub Test_And_Set_Bit() 'TSB (8 bits)
        Dim Value As Byte = Read_Memory((Effective_Address And &HFF0000) / &H10000, Effective_Address And &HFFFF)
        Test_Flag(Not ((Registers.A And &HFF) And Value), Zero_Flag)
        Value = Value Or (Registers.A And &HFF)
        Write_Memory((Effective_Address And &HFF0000) / &H10000, Effective_Address And &HFFFF, Value)
    End Sub
    Private Sub Test_And_Set_Bit_16() 'TSB (16 bits)
        Dim Value As Integer = Read_Memory_16((Effective_Address And &HFF0000) / &H10000, Effective_Address And &HFFFF)
        Test_Flag(Not (Registers.A And Value), Zero_Flag)
        Value = Value Or Registers.A
        Write_Memory_16((Effective_Address And &HFF0000) / &H10000, Effective_Address And &HFFFF, Value)
    End Sub
    Private Sub Transfer_SP_To_Accumulator() 'TSC
        Registers.A = Registers.Stack_Pointer
        Set_Zero_Negative_Flag_16(Registers.A)
    End Sub
    Private Sub Transfer_SP_To_X() 'TSX
        Registers.X = Registers.Stack_Pointer
        Set_Zero_Negative_Flag_16(Registers.X)
    End Sub
    Private Sub Transfer_X_To_Accumulator() 'TXA (8 bits)
        Registers.A = (Registers.X And &HFF) + (Registers.A And &HFF00)
        Set_Zero_Negative_Flag(Registers.A And &HFF)
    End Sub
    Private Sub Transfer_X_To_Accumulator_16() 'TXA (16 bits)
        Registers.A = Registers.X
        Set_Zero_Negative_Flag_16(Registers.A)
    End Sub
    Private Sub Transfer_X_To_SP() 'TXS
        Registers.Stack_Pointer = Registers.X
    End Sub
    Private Sub Transfer_X_To_Y() 'TXY (8 bits)
        Registers.Y = (Registers.X And &HFF) + (Registers.Y And &HFF00)
        Set_Zero_Negative_Flag(Registers.Y And &HFF)
    End Sub
    Private Sub Transfer_X_To_Y_16() 'TXY (16 bits)
        Registers.Y = Registers.X
        Set_Zero_Negative_Flag_16(Registers.Y)
    End Sub
    Private Sub Transfer_Y_To_Accumulator() 'TYA (8 bits)
        Registers.A = (Registers.Y And &HFF) + (Registers.A And &HFF00)
        Set_Zero_Negative_Flag(Registers.A And &HFF)
    End Sub
    Private Sub Transfer_Y_To_Accumulator_16() 'TYA (16 bits)
        Registers.A = Registers.Y
        Set_Zero_Negative_Flag_16(Registers.A)
    End Sub
    Private Sub Transfer_Y_To_X() 'TYX (8 bits)
        Registers.X = (Registers.Y And &HFF) + (Registers.X And &HFF00)
        Set_Zero_Negative_Flag(Registers.X And &HFF)
    End Sub
    Private Sub Transfer_Y_To_X_16() 'TYX (16 bits)
        Registers.X = Registers.Y
        Set_Zero_Negative_Flag_16(Registers.X)
    End Sub
    Private Sub Wait_For_Interrupt() 'WAI
        WAI_Disable = True
    End Sub
    Private Sub Exchange_Accumulator() 'XBA
        Dim Low_Byte As Byte = Registers.A And &HFF
        Dim High_Byte As Byte = (Registers.A And &HFF00) / &H100
        Registers.A = High_Byte + (Low_Byte * &H100)
    End Sub
    Private Sub Exchange_Carry_And_Emulation() 'XCE
        Dim Carry As Boolean = Registers.P And Carry_Flag
        If Emulate_6502 Then Set_Flag(Carry_Flag) Else Clear_Flag(Carry_Flag)
        Emulate_6502 = Carry
    End Sub
    '#End Region

    '#Region "Interrupts"
    Public Sub IRQ()
        If Registers.P And Interrupt_Flag Then
            If WAI_Disable Then Registers.Program_Counter += 1
            WAI_Disable = False
        End If

        IRQ_Ocurred = True

        If Emulate_6502 Then
            Push_16(Registers.Program_Counter)
            Push(Registers.P Or &H30)
            Registers.Program_Bank = 0
            Registers.Program_Counter = Read_Memory_16(0, &HFFFE)
            Set_Flag(Interrupt_Flag)
            Cycles += 7
        Else
            Push(Registers.Program_Bank)
            Push_16(Registers.Program_Counter)
            Push(Registers.P)
            Registers.Program_Bank = 0
            Registers.Program_Counter = Read_Memory_16(0, &HFFEE)
            Set_Flag(Interrupt_Flag)
            Cycles += 8
        End If
    End Sub
    Public Sub NMI()
        If Registers.P And Interrupt_Flag Then
            If WAI_Disable Then Registers.Program_Counter += 1
            WAI_Disable = False
        End If

        If Emulate_6502 Then
            Push_16(Registers.Program_Counter)
            Push(Registers.P Or &H30)
            Registers.Program_Bank = 0
            Registers.Program_Counter = Read_Memory_16(0, &HFFFA)
            Set_Flag(Interrupt_Flag)
            Cycles += 7
        Else
            Push(Registers.Program_Bank)
            Push_16(Registers.Program_Counter)
            Push(Registers.P)
            Registers.Program_Bank = 0
            Registers.Program_Counter = Read_Memory_16(0, &HFFEA)
            Set_Flag(Interrupt_Flag)
            Cycles += 8
        End If
    End Sub
    '#End Region

    '#Region "Main Loop"
    Public Sub Main_Loop()

        While SNES_On
            V_Blank = False
            For Scanline As Integer = 0 To 261
                Current_Line = Scanline
                H_Blank = False
                If (Not WAI_Disable) And (Not STP_Disable) Then
                    If (IRQ_Enable = 2 And Scanline = V_Count) Then IRQ()
                    Execute_65816(Cycles_Per_Scanline - H_Blank_Cycles)

                    '+=========+
                    '| H-Blank |
                    '+=========+
                    H_Blank = True
                    H_Blank_DMA(Scanline)
                    If (IRQ_Enable = 3 And Scanline = V_Count) Or (IRQ_Enable = 1) Then IRQ()
                    Execute_65816(H_Blank_Cycles)
                End If

                If Scanline < 224 Then
                    Render_Scanline(Scanline)
                Else
                    '+=========+
                    '| V-Blank |
                    '+=========+
                    If Scanline = 224 Then
                        Controller_Ready = True
                        Obj_RAM_Address = Obj_RAM_First_Address
                        V_Blank = True
                        If NMI_Enable Then NMI()
                    ElseIf Scanline = 227 Then
                        Controller_Ready = False
                    End If
                End If
            Next
            If Take_Screenshot Then Screenshot()
            Blit()
            If Limit_FPS Then Lock_Framerate(60)

            'frmSobre.Text = Header.Name & " @ " & Get_FPS()

            Application.DoEvents()
        End While
        Application.DoEvents()
    End Sub
    '#End Region


    Private Declare Function QueryPerformanceCounter Lib "Kernel32" (ByRef X As Long) As Integer
    Private Declare Function QueryPerformanceFrequency Lib "Kernel32" (ByRef X As Long) As Integer

    Private Ticks_Per_Second As Long
    Private Start_Time As Long

    Private Milliseconds As Integer
    Private Get_Frames_Per_Second As Integer
    Private Frame_Count As Integer

    Public Limit_FPS As Boolean = True

    Public Function Hi_Res_Timer_Initialize() As Boolean
        If QueryPerformanceFrequency(Ticks_Per_Second) = 0 Then
            Hi_Res_Timer_Initialize = False
        Else
            QueryPerformanceCounter(Start_Time)
            Hi_Res_Timer_Initialize = True
        End If
    End Function
    Private Function Get_Elapsed_Time() As Single
        Dim Last_Time As Long
        Dim Current_Time As Long

        QueryPerformanceCounter(Current_Time)
        Get_Elapsed_Time = Convert.ToSingle((Current_Time - Last_Time) / Ticks_Per_Second)
        QueryPerformanceCounter(Last_Time)
    End Function
    Public Sub Lock_Framerate(ByVal Target_FPS As Long)
        Static Last_Time As Long
        Dim Current_Time As Long
        Dim FPS As Single

        Do
            QueryPerformanceCounter(Current_Time)
            FPS = Convert.ToSingle(Ticks_Per_Second / (Current_Time - Last_Time))
        Loop While (FPS > Target_FPS)

        QueryPerformanceCounter(Last_Time)
    End Sub
    Public Function Get_FPS() As String
        Frame_Count = Frame_Count + 1

        If Get_Elapsed_Time() - Milliseconds >= 1 Then
            Get_Frames_Per_Second = Frame_Count
            Frame_Count = 0
            Milliseconds = Convert.ToInt32(Get_Elapsed_Time)
        End If

        Get_FPS = Get_Frames_Per_Second & " fps"
    End Function


    Private Declare Function GetKeyState Lib "USER32" Alias "GetKeyState" (ByVal nVirtKey As Integer) As Short

    Private Structure DMA_Channel
        Dim Control As Byte
        Dim Dest As Byte
        Dim Source_Bank As Byte
        Dim Source As Integer
        Dim Size As Integer
        Dim HDMA_Bank As Byte
    End Structure
    Private Structure HDMA_Channel
        Dim Source_Bank As Byte
        Dim Source As Integer
        Dim Count, Repeat As Integer
        Dim First As Boolean
        Dim Data() As Byte
    End Structure
    Dim DMA_Channels(7) As DMA_Channel
    Dim HDMA_Channels(7) As HDMA_Channel
    Dim HDMA_Enabled As Byte 'Canais ativados para transferência de DMA

    Public NMI_Enable As Boolean
    Public IRQ_Enable As Byte
    Dim Multiplicand, Multiplier, Divisor, Dividend As Integer
    Dim Mult_Result As Integer
    Dim Div_Result As Integer

    Public Controller_Ready As Boolean
    'Dim Controller_Read_Position As Integer
    Public H_Count, V_Count As Integer
    Public Fast_ROM As Boolean
    Public Sub Reset_IO()
        Array.Clear(DMA_Channels, 0, DMA_Channels.Length)
        Array.Clear(HDMA_Channels, 0, HDMA_Channels.Length)
        For Channel = 0 To 7
            ReDim HDMA_Channels(Channel).Data(3)
        Next
        HDMA_Enabled = 0

        NMI_Enable = False
        IRQ_Enable = 0

        V_Blank = False
        Controller_Ready = False
    End Sub
    Private Function Key_Pressed(Key As Integer) As Boolean
        Return GetKeyState(Key) < 0
    End Function
    Public Function Read_IO(Address As Integer) As Byte

        'Dim Temp As Integer = 0

        Select Case Address
            Case &H4210
                If V_Blank Then
                    V_Blank = False
                    Return &H80
                Else
                    Return 0
                End If
            Case &H4211
                If IRQ_Ocurred Then
                    IRQ_Ocurred = False
                    Return &H80
                Else
                    Return 0
                End If
            Case &H4212
                Dim Value As Byte
                If Controller_Ready Then Value = Value Or &H1
                If H_Blank Then Value = Value Or &H40
                If V_Blank Then Value = Value Or &H80
                Return Value
            Case &H4016 'Input on CPU Pin 32, connected to gameport 1, pin 4 (JOY1) (1=Low)
                'Note to Mike: On some games the controller don't work properly when reading
                'this address (from what I understood, this is latched from 4018/4019)
#If True Then
                Return &HFF
#Else
                Temp = Controller_Read_Position
                Controller_Read_Position = (Controller_Read_Position + 1) And &HF
                Select Case Temp
                    Case 0 : If Key_Pressed(Keys.Z) Then Return 1
                    Case 1 : If Key_Pressed(Keys.X) Then Return 1
                    Case 2 : If Key_Pressed(Keys.Tab) Then Return 1
                    Case 3 : If Key_Pressed(Keys.Return) Then Return 1
                    Case 4 : If Key_Pressed(Keys.Up) Then Return 1
                    Case 5 : If Key_Pressed(Keys.Down) Then Return 1
                    Case 6 : If Key_Pressed(Keys.Left) Then Return 1
                    Case 7 : If Key_Pressed(Keys.Right) Then Return 1
                    Case 8 : If Key_Pressed(Keys.A) Then Return 1
                    Case 9 : If Key_Pressed(Keys.S) Then Return 1
                    Case 10 : If Key_Pressed(Keys.Q) Then Return 1
                    Case 11 : If Key_Pressed(Keys.W) Then Return 1
                End Select
#End If
            Case &H4218
                Dim Value As Byte
                If Key_Pressed(Keys.A) Then Value = Value Or &H80 'A
                If Key_Pressed(Keys.S) Then Value = Value Or &H40 'X
                If Key_Pressed(Keys.Q) Then Value = Value Or &H20 'L
                If Key_Pressed(Keys.W) Then Value = Value Or &H10 'R
                Return Value
            Case &H4219
                Dim Value As Byte
                If Key_Pressed(Keys.Z) Then Value = Value Or &H80 'B
                If Key_Pressed(Keys.X) Then Value = Value Or &H40 'Y
                If Key_Pressed(Keys.Tab) Then Value = Value Or &H20 'Select
                If Key_Pressed(Keys.Return) Then Value = Value Or &H10 'Start
                If Key_Pressed(Keys.Up) Then Value = Value Or &H8
                If Key_Pressed(Keys.Down) Then Value = Value Or &H4
                If Key_Pressed(Keys.Left) Then Value = Value Or &H2
                If Key_Pressed(Keys.Right) Then Value = Value Or &H1
                Return Value
            Case &H4214 : Return Div_Result And &HFF
            Case &H4215 : Return (Div_Result And &HFF00) / &H100
            Case &H4216 : Return Mult_Result And &HFF
            Case &H4217 : Return (Mult_Result And &HFF00) / &H100
            Case &H4300, &H4310, &H4320, &H4330, &H4340, &H4350, &H4360, &H4370 : Return DMA_Channels((Address And &HF0) / &H10).Control
            Case &H4301, &H4311, &H4321, &H4331, &H4341, &H4351, &H4361, &H4371 : Return DMA_Channels((Address And &HF0) / &H10).Dest
            Case &H4302, &H4312, &H4322, &H4332, &H4342, &H4352, &H4362, &H4372 : Return DMA_Channels((Address And &HF0) / &H10).Source And &HFF
            Case &H4303, &H4313, &H4323, &H4333, &H4343, &H4353, &H4363, &H4373 : Return (DMA_Channels((Address And &HF0) / &H10).Source >> 8) And &HFF
            Case &H4304, &H4314, &H4324, &H4334, &H4344, &H4354, &H4364, &H4374 : Return DMA_Channels((Address And &HF0) / &H10).Source_Bank
            Case &H4305, &H4315, &H4325, &H4335, &H4345, &H4355, &H4365, &H4375 : Return DMA_Channels((Address And &HF0) / &H10).Size And &HFF
            Case &H4306, &H4316, &H4326, &H4336, &H4346, &H4356, &H4366, &H4376 : Return (DMA_Channels((Address And &HF0) / &H10).Size >> 8) And &HFF
        End Select

        Return 0 'Nunca deve acontecer
    End Function
    Public Sub Write_IO(Address As Integer, Value As Byte)
        Select Case Address
            Case &H4200
                NMI_Enable = Value And &H80
                IRQ_Enable = (Value And &H30) / &H10
                If IRQ_Enable = 0 Then IRQ_Ocurred = False
            Case &H4202 : Multiplicand = Value
            Case &H4203
                Multiplier = Value
                Mult_Result = Multiplicand * Multiplier
            Case &H4204 : Dividend = Value + (Dividend And &HFF00)
            Case &H4205 : Dividend = (Value * &H100) + (Dividend And &HFF)
            Case &H4206
                Divisor = Value
                If Not Dividend Or Not Divisor Then
                    Div_Result = &HFFFF
                    Mult_Result = Dividend
                Else
                    Div_Result = Dividend / Divisor
                    Mult_Result = Dividend Mod Divisor
                End If
            Case &H4207 : H_Count = Value + (H_Count And &HFF00)
            Case &H4208 : H_Count = (Value * &H100) + (H_Count And &HFF)
            Case &H4209 : V_Count = Value + (V_Count And &HFF00)
            Case &H420A : V_Count = (Value * &H100) + (V_Count And &HFF) : IRQ_Ocurred = False
            Case &H420B 'Transferência de DMA
                For Channel As Byte = 0 To 7
                    If Value And (1 << Channel) Then 'Verifica se deve transferir
                        With DMA_Channels(Channel)
                            Dim Original_Dest As Integer = .Dest

                            If .Size = 0 Then .Size = &H10000
                            While .Size
                                If .Control And &H80 Then
                                    Write_Memory(.Source_Bank, .Source, Read_Memory(0, &H2100 Or .Dest))
                                Else
                                    Write_Memory(0, &H2100 Or .Dest, Read_Memory(.Source_Bank, .Source))
                                End If

                                Cycles += 1

                                Select Case .Control And &HF
                                    Case 0, 2 : If .Control And &H10 Then .Source -= 1 Else .Source += 1
                                    Case 1
                                        If .Dest = Original_Dest Then .Dest += 1 Else .Dest -= 1
                                        If .Control And &H10 Then .Source -= 1 Else .Source += 1
                                    Case 9 : If .Dest = Original_Dest Then .Dest += 1 Else .Dest -= 1
                                End Select
                                .Size -= 1
                            End While
                        End With
                    End If
                Next
            Case &H420C : HDMA_Enabled = Value
            Case &H420D : Fast_ROM = Value And &H1
            Case &H4300, &H4310, &H4320, &H4330, &H4340, &H4350, &H4360, &H4370 : DMA_Channels((Address And &HF0) / &H10).Control = Value
            Case &H4301, &H4311, &H4321, &H4331, &H4341, &H4351, &H4361, &H4371 : DMA_Channels((Address And &HF0) / &H10).Dest = Value
            Case &H4302, &H4312, &H4322, &H4332, &H4342, &H4352, &H4362, &H4372 'High Byte de leitura
                With DMA_Channels((Address And &HF0) / &H10)
                    .Source = Value + (.Source And &HFF00)
                End With
            Case &H4303, &H4313, &H4323, &H4333, &H4343, &H4353, &H4363, &H4373 'Low Byte de leitura
                With DMA_Channels((Address And &HF0) / &H10)
                    .Source = (Value * &H100) + (.Source And &HFF)
                End With
            Case &H4304, &H4314, &H4324, &H4334, &H4344, &H4354, &H4364, &H4374 : DMA_Channels((Address >> 4) And 7).Source_Bank = Value
            Case &H4305, &H4315, &H4325, &H4335, &H4345, &H4355, &H4365, &H4375 'High Byte do tamanho
                With DMA_Channels((Address And &HF0) / &H10)
                    .Size = Value + (.Size And &HFF00)
                End With
            Case &H4306, &H4316, &H4326, &H4336, &H4346, &H4356, &H4366, &H4376 'Low Byte do tamanho
                With DMA_Channels((Address And &HF0) / &H10)
                    .Size = (Value * &H100) + (.Size And &HFF)
                End With
            Case &H4307, &H4317, &H4327, &H4337, &H4347, &H4357, &H4367, &H4377 : DMA_Channels((Address And &HF0) / &H10).HDMA_Bank = Value
        End Select
    End Sub
    Public Sub H_Blank_DMA(Scanline As Integer)
        For Channel As Integer = 0 To 7
            With HDMA_Channels(Channel)
                If Scanline = 0 Then 'Novo Frame
                    .Source = DMA_Channels(Channel).Source
                    .Source_Bank = DMA_Channels(Channel).Source_Bank
                    .Count = 0
                End If

                If HDMA_Enabled And (1 << Channel) Then 'Verifica se deve transferir
                    '+===========================+
                    '| Carrega valores da tabela |
                    '+===========================+

                    If (.Count And &H7F) = 0 Then
                        .Count = Read_Memory(.Source_Bank, .Source)
                        .Source += 1
                        .Repeat = .Count And &H80
                        .Count = .Count And &H7F

                        Select Case DMA_Channels(Channel).Control And &H47
                            Case 0 'Modo Normal
                                .Data(0) = Read_Memory(.Source_Bank, .Source)
                                .Source += 1
                            Case 1, 2
                                .Data(0) = Read_Memory(.Source_Bank, .Source)
                                .Data(1) = Read_Memory(.Source_Bank, .Source + 1)
                                .Source += 2
                            Case 3, 4
                                .Data(0) = Read_Memory(.Source_Bank, .Source)
                                .Data(1) = Read_Memory(.Source_Bank, .Source + 1)
                                .Data(2) = Read_Memory(.Source_Bank, .Source + 2)
                                .Data(3) = Read_Memory(.Source_Bank, .Source + 3)
                                .Source += 4
                            Case &H40 'Modo Indireto
                                Dim Address As Integer = Read_Memory_16(.Source_Bank, .Source)
                                .Data(0) = Read_Memory(DMA_Channels(Channel).HDMA_Bank, Address)
                                .Source += 2
                            Case &H41, &H42
                                Dim Address As Integer = Read_Memory_16(.Source_Bank, .Source)
                                .Data(0) = Read_Memory(DMA_Channels(Channel).HDMA_Bank, Address)
                                .Data(1) = Read_Memory(DMA_Channels(Channel).HDMA_Bank, Address + 1)
                                .Source += 2
                            Case &H43, &H44
                                Dim Address As Integer = Read_Memory_16(.Source_Bank, .Source)
                                .Data(0) = Read_Memory(DMA_Channels(Channel).HDMA_Bank, Address)
                                .Data(1) = Read_Memory(DMA_Channels(Channel).HDMA_Bank, Address + 1)
                                .Data(2) = Read_Memory(DMA_Channels(Channel).HDMA_Bank, Address + 2)
                                .Data(3) = Read_Memory(DMA_Channels(Channel).HDMA_Bank, Address + 3)
                                .Source += 2
                        End Select
                        .First = True
                    End If

                    '+=================+
                    '| Escreve valores |
                    '+=================+

                    If .First Or .Repeat Then
                        .First = False
                        Select Case DMA_Channels(Channel).Control And &H7
                            Case 0 : Write_Memory(0, &H2100 Or DMA_Channels(Channel).Dest, .Data(0))
                            Case 1
                                Write_Memory(0, &H2100 Or DMA_Channels(Channel).Dest, .Data(0))
                                Write_Memory(0, &H2100 Or (DMA_Channels(Channel).Dest + 1), .Data(1))
                            Case 2
                                Write_Memory(0, &H2100 Or DMA_Channels(Channel).Dest, .Data(0))
                                Write_Memory(0, &H2100 Or DMA_Channels(Channel).Dest, .Data(1))
                            Case 3
                                Write_Memory(0, &H2100 Or DMA_Channels(Channel).Dest, .Data(0))
                                Write_Memory(0, &H2100 Or (DMA_Channels(Channel).Dest + 1), .Data(1))
                                Write_Memory(0, &H2100 Or DMA_Channels(Channel).Dest, .Data(2))
                                Write_Memory(0, &H2100 Or (DMA_Channels(Channel).Dest + 1), .Data(3))
                            Case 4
                                Write_Memory(0, &H2100 Or DMA_Channels(Channel).Dest, .Data(0))
                                Write_Memory(0, &H2100 Or (DMA_Channels(Channel).Dest + 1), .Data(1))
                                Write_Memory(0, &H2100 Or (DMA_Channels(Channel).Dest + 2), .Data(2))
                                Write_Memory(0, &H2100 Or (DMA_Channels(Channel).Dest + 3), .Data(3))
                        End Select
                    End If

                    .Count -= 1
                End If
            End With
        Next
    End Sub

    Private Structure Color_Palette
        Dim R As Byte
        Dim G As Byte
        Dim B As Byte
    End Structure
    Private Structure PPU_Background
        Dim Address As Integer
        Dim CHR_Address As Integer
        Dim Size As Byte
        Dim Tile_16x16 As Boolean
        Dim H_Scroll, V_Scroll As Integer
        Dim H_Low_High_Toggle, V_Low_High_Toggle As Boolean
        Dim Mosaic As Boolean
    End Structure
    Dim Palette(255) As Color_Palette
    Dim Background(3) As PPU_Background
    Dim Bg_Main_Enabled As Byte
    Dim Bg_Sub_Enabled As Byte
    Dim Pal_Address As Integer
    Dim PPU_Mode As Byte
    Dim BG3_Priority As Boolean

    Dim Color_Math_Enable As Byte
    Dim Color_Math_Obj As Boolean
    Dim Color_Math_Add_Sub As Boolean
    Dim Color_Math_Div_2 As Boolean
    Dim Color_Math_BGs As Byte
    Dim Fixed_Color As Color_Palette

    Dim Obj_Size, Obj_Name, Obj_Chr_Offset As Integer
    Public Obj_RAM_Address, Obj_RAM_First_Address As Integer
    Public Obj_RAM(&H21F) As Byte
    Dim Obj_Low_High_Toggle, First_Read_Obj As Boolean

    Dim VRAM_Address, VRAM_Increment As Integer
    Dim Increment_2119_213A As Boolean
    Dim First_Read_VRAM As Boolean

    Dim V_Latch As Integer

    Dim Mode_7_Multiplicand, Mode_7_Multiplier As Integer
    Dim Mode_7_C, Mode_7_D, Mode_7_X, Mode_7_Y As Byte
    Dim Mode_7_Low_High As Boolean
    Dim Mult_Result2 As Integer

    Dim Mosaic_Size As Byte

    Public VRAM(&HFFFF) As Byte
    Dim CGRAM(&H1FF) As Byte

    Dim Screen_Enabled As Boolean

    Dim Video_Buffer((256 * 224) - 1) As Integer
    Dim Video_Buffer_Sub((256 * 224) - 1) As Integer
    Public Power_Of_2(31) As Integer

    Public Take_Screenshot As Boolean
    Public Sub Reset_PPU()
        For i As Integer = 0 To 30
            Power_Of_2(i) = 2 ^ i
        Next
        Power_Of_2(31) = -2147483648.0#

        For BgNum As Integer = 0 To 3
            With Background(BgNum)
                .Address = 0
                .Size = 0
                .CHR_Address = 0
                .H_Scroll = 0
                .H_Low_High_Toggle = False
                .V_Scroll = 0
                .V_Low_High_Toggle = False
            End With
        Next
        Obj_RAM_Address = 0
        Obj_RAM_First_Address = 0
        Obj_Low_High_Toggle = False
        First_Read_Obj = False
        Array.Clear(Obj_RAM, 0, Obj_RAM.Length)
        Array.Clear(Palette, 0, Palette.Length)
    End Sub
    Public Sub Write_PPU(Address As Integer, Value As Byte)
        Select Case Address
            Case &H2100 : Screen_Enabled = If(Value And &H80, False, True)
            Case &H2101
                Obj_Chr_Offset = (Value And 3) * &H4000
                Obj_Name = ((Value >> 3) And 3) << 13
                Obj_Size = Value / &H20
            Case &H2102
                Obj_RAM_Address = Value + (Obj_RAM_Address And &H100)
                Obj_RAM_First_Address = Obj_RAM_Address
            Case &H2103
                If Value And 1 Then
                    Obj_RAM_Address = Obj_RAM_Address Or &H100
                Else
                    Obj_RAM_Address = Obj_RAM_Address And Not &H100
                End If
                Obj_RAM_First_Address = Obj_RAM_Address
                Obj_Low_High_Toggle = True
            Case &H2104
                If Obj_RAM_Address > &H10F Then
                    Obj_RAM_Address = 0
                    Obj_Low_High_Toggle = True
                End If

                If Obj_Low_High_Toggle Then
                    Obj_RAM(Obj_RAM_Address * 2) = Value
                Else
                    Obj_RAM((Obj_RAM_Address * 2) + 1) = Value
                    Obj_RAM_Address += 1
                End If
                Obj_Low_High_Toggle = Not Obj_Low_High_Toggle
            Case &H2105
                PPU_Mode = Value And &H7
                BG3_Priority = Value And &H8
                Background(0).Tile_16x16 = Value And &H10
                Background(1).Tile_16x16 = Value And &H20
                Background(2).Tile_16x16 = Value And &H40
                Background(3).Tile_16x16 = Value And &H80
            Case &H2106 'Mosaico
                Mosaic_Size = (Value And &HF0) >> 4
                Background(0).Mosaic = Value And &H1
                Background(1).Mosaic = Value And &H2
                Background(2).Mosaic = Value And &H4
                Background(3).Mosaic = Value And &H8
            Case &H2107 'Address
                Background(0).Address = (Value And &H7C) * &H200
                Background(0).Size = Value And 3
            Case &H2108
                Background(1).Address = (Value And &H7C) * &H200
                Background(1).Size = Value And 3
            Case &H2109
                Background(2).Address = (Value And &H7C) * &H200
                Background(2).Size = Value And 3
            Case &H210A
                Background(3).Address = (Value And &H7C) * &H200
                Background(3).Size = Value And 3
            Case &H210B 'CHR Address
                Background(0).CHR_Address = (Value And 7) * &H2000
                Background(1).CHR_Address = (Value >> 4) * &H2000
            Case &H210C
                Background(2).CHR_Address = (Value And 7) * &H2000
                Background(3).CHR_Address = (Value >> 4) * &H2000
            Case &H210D
                With Background(0)
                    If .H_Low_High_Toggle Then
                        .H_Scroll = (Value * &H100) + (.H_Scroll And &HFF)
                    Else
                        .H_Scroll = Value + (.H_Scroll And &HFF00)
                    End If
                    .H_Low_High_Toggle = Not .H_Low_High_Toggle
                End With
            Case &H210E
                With Background(0)
                    If .V_Low_High_Toggle Then
                        .V_Scroll = (Value * &H100) + (.V_Scroll And &HFF)
                    Else
                        .V_Scroll = Value + (.V_Scroll And &HFF00)
                    End If
                    .V_Low_High_Toggle = Not .V_Low_High_Toggle
                End With
            Case &H210F
                With Background(1)
                    If .H_Low_High_Toggle Then
                        .H_Scroll = (Value * &H100) + (.H_Scroll And &HFF)
                    Else
                        .H_Scroll = Value + (.H_Scroll And &HFF00)
                    End If
                    .H_Low_High_Toggle = Not .H_Low_High_Toggle
                End With
            Case &H2110
                With Background(1)
                    If .V_Low_High_Toggle Then
                        .V_Scroll = (Value * &H100) + (.V_Scroll And &HFF)
                    Else
                        .V_Scroll = Value + (.V_Scroll And &HFF00)
                    End If
                    .V_Low_High_Toggle = Not .V_Low_High_Toggle
                End With
            Case &H2111
                With Background(2)
                    If .H_Low_High_Toggle Then
                        .H_Scroll = (Value * &H100) + (.H_Scroll And &HFF)
                    Else
                        .H_Scroll = Value + (.H_Scroll And &HFF00)
                    End If
                    .H_Low_High_Toggle = Not .H_Low_High_Toggle
                End With
            Case &H2112
                With Background(2)
                    If .V_Low_High_Toggle Then
                        .V_Scroll = (Value * &H100) + (.V_Scroll And &HFF)
                    Else
                        .V_Scroll = Value + (.V_Scroll And &HFF00)
                    End If
                    .V_Low_High_Toggle = Not .V_Low_High_Toggle
                End With
            Case &H2113
                With Background(3)
                    If .H_Low_High_Toggle Then
                        .H_Scroll = (Value * &H100) + (.H_Scroll And &HFF)
                    Else
                        .H_Scroll = Value + (.H_Scroll And &HFF00)
                    End If
                    .H_Low_High_Toggle = Not .H_Low_High_Toggle
                End With
            Case &H2114
                With Background(3)
                    If .V_Low_High_Toggle Then
                        .V_Scroll = (Value * &H100) + (.V_Scroll And &HFF)
                    Else
                        .V_Scroll = Value + (.V_Scroll And &HFF00)
                    End If
                    .V_Low_High_Toggle = Not .V_Low_High_Toggle
                End With
            Case &H2115 'VRAM Control
                Select Case Value And 3
                    Case 0 : VRAM_Increment = 1
                    Case 1 : VRAM_Increment = 32
                    Case 2 : VRAM_Increment = 128
                    Case 3 : VRAM_Increment = 256
                End Select
                Increment_2119_213A = Value And &H80
            Case &H2116 'VRAM Access
                VRAM_Address = Value + (VRAM_Address And &HFF00)
                First_Read_VRAM = True
            Case &H2117
                VRAM_Address = (Value * &H100) + (VRAM_Address And &HFF)
                First_Read_VRAM = True
            Case &H2118
                VRAM((VRAM_Address << 1) And &HFFFF) = Value
                If Not Increment_2119_213A Then VRAM_Address += VRAM_Increment
                First_Read_VRAM = True
            Case &H2119
                VRAM(((VRAM_Address << 1) + 1) And &HFFFF) = Value
                If Increment_2119_213A Then VRAM_Address += VRAM_Increment
                First_Read_VRAM = True
            Case &H211B
                If Mode_7_Low_High Then
                    Mode_7_Multiplicand = (Value * &H100) + (Mode_7_Multiplicand And &HFF)
                Else
                    Mode_7_Multiplicand = Value + (Mode_7_Multiplicand And &HFF00)
                End If
                Mode_7_Low_High = Not Mode_7_Low_High
            Case &H211C
                Mode_7_Multiplier = Value
                Mult_Result2 = Mode_7_Multiplicand * Mode_7_Multiplier
            Case &H211D : Mode_7_C = Value
            Case &H211E : Mode_7_D = Value
            Case &H211F : Mode_7_X = Value
            Case &H2120 : Mode_7_Y = Value
            Case &H2121 : Pal_Address = Value * 2
            Case &H2122
                CGRAM(Pal_Address And &H1FF) = Value
                Dim Palette_Value As Integer = CGRAM(Pal_Address And &H1FE) + (CGRAM((Pal_Address And &H1FE) + 1) * &H100)
                Palette((Pal_Address \ 2) And &HFF).R = (Palette_Value And &H1F) * 8
                Palette((Pal_Address \ 2) And &HFF).G = ((Palette_Value >> 5) And &H1F) * 8
                Palette((Pal_Address \ 2) And &HFF).B = ((Palette_Value >> 10) And &H1F) * 8
                Pal_Address += 1
            Case &H212C : Bg_Main_Enabled = Value
            Case &H212D
                Bg_Sub_Enabled = Value
            Case &H2130
                Color_Math_Obj = Value And &H2
                Color_Math_Enable = (Value And &H30) / &H10
            Case &H2131
                Color_Math_Add_Sub = Value And &H80
                Color_Math_Div_2 = Value And &H40
                Color_Math_BGs = Value And &H3F
            Case &H2132
                If Value And &H20 Then Fixed_Color.R = (Value And &H1F) * 8
                If Value And &H40 Then Fixed_Color.G = (Value And &H1F) * 8
                If Value And &H80 Then Fixed_Color.B = (Value And &H1F) * 8
        End Select
    End Sub
    Public Function Read_PPU(Address As Integer) As Byte
        Select Case Address
            Case &H2134 : Return Mult_Result2 And &HFF
            Case &H2135 : Return (Mult_Result2 And &HFF00) / &H100
            Case &H2136 : Return (Mult_Result2 And &HFF0000) / &H10000
            Case &H2137 : V_Latch = Current_Line
            Case &H2138
                If First_Read_Obj Then
                    First_Read_Obj = False
                    Return Obj_RAM(Obj_RAM_Address << 1)
                End If
                Dim Value As Byte = Obj_RAM(((Obj_RAM_Address << 1) + 1) And &H10F)
                Obj_RAM_Address = (Obj_RAM_Address + 1) And &H10F
                Return Value
            Case &H2139
                If First_Read_VRAM Then
                    First_Read_VRAM = False
                    Return VRAM((VRAM_Address << 1) And &HFFFF)
                End If
                Dim Value As Byte = VRAM(((VRAM_Address << 1) - 2) And &HFFFF)
                If Not Increment_2119_213A Then VRAM_Address += VRAM_Increment
                Return Value
            Case &H213A
                If First_Read_VRAM Then
                    First_Read_VRAM = False
                    Return VRAM(((VRAM_Address << 1) + 1) And &HFFFF)
                End If
                Dim Value As Byte = VRAM(((VRAM_Address << 1) - 1) And &HFFFF)
                If Increment_2119_213A Then VRAM_Address += VRAM_Increment
                Return Value
            Case &H213D
                Dim Value As Byte = V_Latch And &HFF
                V_Latch >>= 8
                Return Value
        End Select

        Return 0
    End Function
    Public Sub Render_Scanline(Scanline As Integer)
        If Screen_Enabled Then
            Render_Bg_Layer(Scanline, 3, False)
            Render_Bg_Layer(Scanline, 2, False)
            Render_Sprites(Scanline, 0)
            If BG3_Priority Then
                Render_Bg_Layer(Scanline, 3, True)
                Render_Sprites(Scanline, 1)
                Render_Bg_Layer(Scanline, 1, False)
                Render_Bg_Layer(Scanline, 0, False)
                Render_Bg_Layer(Scanline, 1, True)
                Render_Sprites(Scanline, 2)
                Render_Bg_Layer(Scanline, 0, True)
                Render_Sprites(Scanline, 3)
                Render_Bg_Layer(Scanline, 2, True)
            Else
                Render_Bg_Layer(Scanline, 3, True)
                Render_Bg_Layer(Scanline, 2, True)
                Render_Sprites(Scanline, 1)
                Render_Bg_Layer(Scanline, 1, False)
                Render_Bg_Layer(Scanline, 0, False)
                Render_Sprites(Scanline, 2)
                Render_Bg_Layer(Scanline, 1, True)
                Render_Bg_Layer(Scanline, 0, True)
                Render_Sprites(Scanline, 3)
            End If
        End If
    End Sub
    Private Sub Render_Bg_Layer(Scanline As Integer, Layer As Integer, Foreground As Boolean)
        If (Bg_Main_Enabled Or Bg_Sub_Enabled) And Power_Of_2(Layer) Then
            Dim Color_Math As Boolean
            If Color_Math_Enable <> 3 Then
                If Bg_Main_Enabled And Power_Of_2(Layer) Then
                    Color_Math = Color_Math_BGs And Power_Of_2(Layer)
                End If
                If ((Color_Math_BGs And &H20) And (Layer = 1 And Foreground = False)) Then Color_Math = True
            End If

            Dim BPP As Integer = 0
            Select Case Layer
                Case 0
                    Select Case PPU_Mode
                        Case 0 : BPP = 2
                        Case 1, 2, 5, 6 : BPP = 4
                        Case 3, 4 : BPP = 8
                    End Select
                Case 1
                    Select Case PPU_Mode
                        Case 0, 4, 5 : BPP = 2
                        Case 1, 2, 3 : BPP = 4
                    End Select
                Case 2 : If PPU_Mode < 2 Then BPP = 2
                Case 3 : If PPU_Mode = 0 Then BPP = 2
            End Select

            With Background(Layer)
                Dim Reverse_X, Reverse_Y As Boolean
                Reverse_X = If((.H_Scroll \ 256) Mod 2, False, True)
                Reverse_Y = If((.V_Scroll \ 256) Mod 2, False, True)

                Dim Scroll_Y As Integer = 0
                If Scanline >= (256 - (.V_Scroll Mod 256)) Then Scroll_Y = 1

                If PPU_Mode = 7 And Layer = 0 Then
                    'Mode 7
                    Dim Base_Char_Num As Integer = ((((Scanline + (.V_Scroll Mod 8)) \ 8) + ((.V_Scroll Mod 1024) \ 8)) Mod 128) * 256
                    Dim Temp As Integer = (Scanline + (.V_Scroll Mod 8)) Mod 8
                    For X As Integer = 0 To 127
                        Dim Character_Number As Integer = Base_Char_Num + (X * 2)
                        For Scroll_X As Integer = 0 To 1
                            Dim Temp_X As Integer = ((X * 8) + (Scroll_X * 1024) - (.H_Scroll Mod 1024))
                            If (Temp_X > -8 And Temp_X < 256) Then
                                Dim Tile_Number As Integer = VRAM(Character_Number)
                                Dim Base_Tile As Integer = (Tile_Number * 128) + 1
                                Base_Tile += Temp * 16
                                For Tile_X As Integer = 0 To 7
                                    Dim Color As Byte = VRAM(Base_Tile + (Tile_X * 2))
                                    Draw_Pixel(((X * 8) + Tile_X) + (Scroll_X * 1024) - (.H_Scroll Mod 1024), _
                                        Scanline, _
                                        Color)
                                Next
                            End If
                        Next
                    Next
                Else
                    If BPP <> 0 Then
                        If .Tile_16x16 Then
                            Dim Base_Char_Num As Integer = ((((Scanline + (.V_Scroll Mod 16)) \ 16) + ((.V_Scroll Mod 256) \ 16)) Mod 16) * 64
                            Dim Temp As Integer = (Scanline + (.V_Scroll Mod 16)) Mod 16
                            For X As Integer = 0 To 16
                                Dim Character_Number As Integer = Base_Char_Num + (X * 2)
                                For Scroll_X As Integer = 0 To 1
                                    Dim Temp_X As Integer = ((X * 16) + (Scroll_X * 256) - (.H_Scroll Mod 256))
                                    If (Temp_X > -16 And Temp_X < 256) Then
                                        Dim Tile_Offset As Integer = .Address + Character_Number
                                        Select Case .Size
                                            Case 1 : Tile_Offset += (512 * If(Reverse_X, Scroll_X, 1 - Scroll_X))
                                            Case 2 : Tile_Offset += (512 * If(Reverse_Y, Scroll_Y, 1 - Scroll_Y))
                                            Case 3 : Tile_Offset += (512 * (If(Reverse_Y, Scroll_Y, 1 - Scroll_Y) * 2)) + _
                                            (512 * If(Reverse_X, Scroll_X, 1 - Scroll_X))
                                        End Select
                                        Dim Tile_Data As Integer = VRAM(Tile_Offset) + (VRAM(Tile_Offset + 1) * &H100)
                                        Dim Tile_Number As Integer = Tile_Data And &H3FF
                                        Dim Pal_Num As Integer = (Tile_Data And &H1C00) >> 10
                                        Dim Priority As Boolean = Tile_Data And &H2000
                                        Dim H_Flip As Boolean = Tile_Data And &H4000
                                        Dim V_Flip As Boolean = Tile_Data And &H8000
                                        If V_Flip Then
                                            If ((Scanline + (.V_Scroll Mod 16)) Mod 16) < 8 Then Tile_Number += 16
                                        Else
                                            If ((Scanline + (.V_Scroll Mod 16)) Mod 16) > 7 Then Tile_Number += 16
                                        End If
                                        If Priority = Foreground Then
                                            For TX = 0 To 1
                                                Dim Base_Tile As Integer = .CHR_Address + (Tile_Number * (BPP * 8))
                                                If Temp > 7 Then Temp -= 8
                                                Base_Tile += If(V_Flip, (7 - Temp) * 2, Temp * 2)
                                                Dim Byte_0, Byte_1, Byte_2, Byte_3, Byte_4, Byte_5, Byte_6, Byte_7 As Byte
                                                Byte_0 = VRAM(Base_Tile)
                                                Byte_1 = VRAM(Base_Tile + 1)
                                                If BPP = 4 Or BPP = 8 Then
                                                    Byte_2 = VRAM(Base_Tile + 16)
                                                    Byte_3 = VRAM(Base_Tile + 17)
                                                    If BPP = 8 Then
                                                        Byte_4 = VRAM(Base_Tile + 32)
                                                        Byte_5 = VRAM(Base_Tile + 33)
                                                        Byte_6 = VRAM(Base_Tile + 48)
                                                        Byte_7 = VRAM(Base_Tile + 49)
                                                    End If
                                                End If
                                                Dim X_Flip As Integer
                                                If H_Flip Then X_Flip = 8 * (1 - TX) Else X_Flip = 8 * TX
                                                For Tile_X As Integer = 0 To 7
                                                    Dim Pixel_Color As Integer = 0
                                                    Dim Bit_To_Test As Integer = Power_Of_2(If(H_Flip, Tile_X, 7 - Tile_X))
                                                    If Byte_0 And Bit_To_Test Then Pixel_Color += 1
                                                    If Byte_1 And Bit_To_Test Then Pixel_Color += 2
                                                    If BPP = 4 Or BPP = 8 Then
                                                        If Byte_2 And Bit_To_Test Then Pixel_Color += 4
                                                        If Byte_3 And Bit_To_Test Then Pixel_Color += 8
                                                        If BPP = 8 Then
                                                            If Byte_4 And Bit_To_Test Then Pixel_Color += 16
                                                            If Byte_5 And Bit_To_Test Then Pixel_Color += 32
                                                            If Byte_6 And Bit_To_Test Then Pixel_Color += 64
                                                            If Byte_7 And Bit_To_Test Then Pixel_Color += 128
                                                        End If
                                                    End If
                                                    Dim Color As Byte = (Pal_Num * Power_Of_2(BPP)) + Pixel_Color
                                                    If Pixel_Color <> 0 Or (Layer = 1 And Foreground = False) Then
                                                        Draw_Pixel(((X * 16) + Tile_X + X_Flip) + (Scroll_X * 256) - (.H_Scroll Mod 256), _
                                                        Scanline, _
                                                        Color, _
                                                        Color_Math, _
                                                        Pixel_Color = 0)
                                                    End If
                                                Next
                                                Tile_Number += 1
                                            Next
                                        End If
                                    End If
                                Next
                            Next
                        Else
                            Dim Base_Char_Num As Integer = ((((Scanline + (.V_Scroll Mod 8)) \ 8) + ((.V_Scroll Mod 256) \ 8)) Mod 32) * 64
                            Dim Temp As Integer = (Scanline + (.V_Scroll Mod 8)) Mod 8
                            For X As Integer = 0 To 31
                                Dim Character_Number As Integer = Base_Char_Num + (X * 2)
                                For Scroll_X As Integer = 0 To 1
                                    Dim Temp_X As Integer = ((X * 8) + (Scroll_X * 256) - (.H_Scroll Mod 256))
                                    If (Temp_X > -8 And Temp_X < 256) Then
                                        Dim Tile_Offset As Integer = .Address + Character_Number
                                        Select Case .Size
                                            Case 1 : Tile_Offset += (2048 * If(Reverse_X, Scroll_X, 1 - Scroll_X))
                                            Case 2 : Tile_Offset += (2048 * If(Reverse_Y, Scroll_Y, 1 - Scroll_Y))
                                            Case 3 : Tile_Offset += (2048 * (If(Reverse_Y, Scroll_Y, 1 - Scroll_Y) * 2)) + _
                                            (2048 * If(Reverse_X, Scroll_X, 1 - Scroll_X))
                                        End Select
                                        Dim Tile_Data As Integer = VRAM(Tile_Offset) + (VRAM(Tile_Offset + 1) * &H100)
                                        Dim Tile_Number As Integer = Tile_Data And &H3FF
                                        Dim Pal_Num As Integer = (Tile_Data And &H1C00) >> 10
                                        Dim Priority As Boolean = Tile_Data And &H2000
                                        Dim H_Flip As Boolean = Tile_Data And &H4000
                                        Dim V_Flip As Boolean = Tile_Data And &H8000
                                        If Priority = Foreground Then
                                            Dim Base_Tile As Integer = .CHR_Address + (Tile_Number * (BPP * 8))
                                            Base_Tile += If(V_Flip, (7 - Temp) * 2, Temp * 2)
                                            Dim Byte_0, Byte_1, Byte_2, Byte_3, Byte_4, Byte_5, Byte_6, Byte_7 As Byte
                                            Byte_0 = VRAM(Base_Tile)
                                            Byte_1 = VRAM(Base_Tile + 1)
                                            If BPP = 4 Or BPP = 8 Then
                                                Byte_2 = VRAM(Base_Tile + 16)
                                                Byte_3 = VRAM(Base_Tile + 17)
                                                If BPP = 8 Then
                                                    Byte_4 = VRAM(Base_Tile + 32)
                                                    Byte_5 = VRAM(Base_Tile + 33)
                                                    Byte_6 = VRAM(Base_Tile + 48)
                                                    Byte_7 = VRAM(Base_Tile + 49)
                                                End If
                                            End If
                                            For Tile_X As Integer = 0 To 7
                                                Dim Pixel_Color As Integer = 0
                                                Dim Bit_To_Test As Integer = Power_Of_2(If(H_Flip, Tile_X, 7 - Tile_X))
                                                If Byte_0 And Bit_To_Test Then Pixel_Color += 1
                                                If Byte_1 And Bit_To_Test Then Pixel_Color += 2
                                                If BPP = 4 Or BPP = 8 Then
                                                    If Byte_2 And Bit_To_Test Then Pixel_Color += 4
                                                    If Byte_3 And Bit_To_Test Then Pixel_Color += 8
                                                    If BPP = 8 Then
                                                        If Byte_4 And Bit_To_Test Then Pixel_Color += 16
                                                        If Byte_5 And Bit_To_Test Then Pixel_Color += 32
                                                        If Byte_6 And Bit_To_Test Then Pixel_Color += 64
                                                        If Byte_7 And Bit_To_Test Then Pixel_Color += 128
                                                    End If
                                                End If
                                                Dim Color As Byte = (Pal_Num * Power_Of_2(BPP)) + Pixel_Color
                                                If Pixel_Color <> 0 Or (Layer = 1 And Foreground = False) Then
                                                    Draw_Pixel(((X * 8) + Tile_X) + (Scroll_X * 256) - (.H_Scroll Mod 256), _
                                                    Scanline, _
                                                    Color, _
                                                    Color_Math, _
                                                    Pixel_Color = 0)
                                                End If
                                            Next
                                        End If
                                    End If
                                Next
                            Next
                        End If

                    End If
                End If

                If .Mosaic And Mosaic_Size Then Apply_Mosaic(Scanline)
            End With
        End If
    End Sub
    Private Sub Render_Sprites(Scanline As Integer, Priority As Integer)
        If (Bg_Main_Enabled Or Bg_Sub_Enabled) And &H10 Then
            Dim Tbl_2_Byte As Integer, Tbl_2_Shift As Integer = 1
            Dim Temp As Integer
            For Offset As Integer = 0 To &H1FF Step 4
                Dim Temp_X As Integer = Obj_RAM(Offset)
                Dim Y As Integer = Obj_RAM(Offset + 1)
                Dim Tile_Number As Integer = Obj_RAM(Offset + 2)
                Dim Attributes As Byte = Obj_RAM(Offset + 3)
                If Attributes And &H1 Then Tile_Number = Tile_Number Or &H100
                Dim Pal_Num As Integer = (Attributes And &HE) >> 1
                Dim Obj_Priority As Integer = (Attributes And &H30) >> 4
                Dim H_Flip As Boolean = Attributes And &H40
                Dim V_Flip As Boolean = Attributes And &H80
                Dim Tbl_2_Data As Byte = Obj_RAM(&H200 + Tbl_2_Byte)
                If Tbl_2_Data And Power_Of_2(Tbl_2_Shift - 1) Then Temp_X = Temp_X Or &H100
                Dim X As Integer = Temp_X
                Dim Tile_Size As Boolean = Tbl_2_Data And Power_Of_2(Tbl_2_Shift)
                Dim TX, TY As Integer
                Select Case Obj_Size
                    Case 0 : If Tile_Size Then TX = 1 : TY = 1 Else TX = 0 : TY = 0 '8x8/16x16
                    Case 1 : If Tile_Size Then TX = 3 : TY = 3 Else TX = 0 : TY = 0 '8x8/32x32
                    Case 2 : If Tile_Size Then TX = 7 : TY = 7 Else TX = 0 : TY = 0 '8x8/64x64
                    Case 3 : If Tile_Size Then TX = 3 : TY = 3 Else TX = 1 : TY = 1 '16x16/32x32
                    Case 4 : If Tile_Size Then TX = 7 : TY = 7 Else TX = 1 : TY = 1 '16x16/64x64
                    Case 5 : If Tile_Size Then TX = 7 : TY = 7 Else TX = 3 : TY = 3 '32x32/64x64
                End Select

                If Obj_Priority = Priority Then
                    If V_Flip Then Y += (8 * TY)
                    For Tile_Num_Y As Integer = 0 To TY
                        If H_Flip Then X += (8 * TX)
                        For Tile_Num_X As Integer = 0 To TX
                            For Tile_Y As Integer = 0 To 7
                                If Y + If(V_Flip, (7 - Tile_Y), Tile_Y) = Scanline Then
                                    Dim Byte_0, Byte_1, Byte_2, Byte_3 As Byte
                                    Byte_0 = VRAM(Obj_Chr_Offset + (Tile_Y * 2) + ((Tile_Number + (Tile_Num_Y * 16) + Tile_Num_X) * 32))
                                    Byte_1 = VRAM(Obj_Chr_Offset + (Tile_Y * 2) + ((Tile_Number + (Tile_Num_Y * 16) + Tile_Num_X) * 32) + 1)
                                    Byte_2 = VRAM(Obj_Chr_Offset + (Tile_Y * 2) + ((Tile_Number + (Tile_Num_Y * 16) + Tile_Num_X) * 32) + 16)
                                    Byte_3 = VRAM(Obj_Chr_Offset + (Tile_Y * 2) + ((Tile_Number + (Tile_Num_Y * 16) + Tile_Num_X) * 32) + 17)
                                    For Tile_X As Integer = 0 To 7
                                        Dim Pixel_Color As Integer = 0
                                        Dim Bit_To_Test As Integer = Power_Of_2(If(H_Flip, Tile_X, 7 - Tile_X))
                                        If Byte_0 And Bit_To_Test Then Pixel_Color += 1
                                        If Byte_1 And Bit_To_Test Then Pixel_Color += 2
                                        If Byte_2 And Bit_To_Test Then Pixel_Color += 4
                                        If Byte_3 And Bit_To_Test Then Pixel_Color += 8
                                        If V_Flip Then
                                            Draw_Pixel(X + Tile_X, Y + (7 - Tile_Y), 128 + (Pal_Num * 16) + Pixel_Color, False, Pixel_Color = 0)
                                        Else
                                            Draw_Pixel(X + Tile_X, Y + Tile_Y, 128 + (Pal_Num * 16) + Pixel_Color, False, Pixel_Color = 0)
                                        End If
                                    Next
                                End If
                            Next
                            If H_Flip Then X -= 8 Else X += 8
                        Next
                        X = Temp_X
                        If V_Flip Then Y -= 8 Else Y += 8
                    Next
                End If
                If Temp < 3 Then
                    Temp += 1
                    Tbl_2_Shift += 2
                Else
                    Temp = 0
                    Tbl_2_Byte += 1
                    Tbl_2_Shift = 1
                End If
            Next
        End If
    End Sub
    Private Sub Draw_Pixel(X As Integer, Y As Integer, _
        Color_Index As Byte, _
        Optional Color_Math As Boolean = False, _
        Optional Transparent As Boolean = False)
        If (X >= 0 And X < 256) And (Y >= 0 And Y < 224) Then
            Dim Buffer_Position As Integer = X + (Y * 256)
            Dim Color As Color_Palette = Palette(Color_Index)
            With Color
                If Color_Math And Transparent Then
                    If Color_Math_Add_Sub Then
                        .R = Sub_Color(.R, Fixed_Color.R)
                        .G = Sub_Color(.G, Fixed_Color.G)
                        .B = Sub_Color(.B, Fixed_Color.B)
                    Else
                        .R = Add_Color(.R, Fixed_Color.R)
                        .G = Add_Color(.G, Fixed_Color.G)
                        .B = Add_Color(.B, Fixed_Color.B)
                    End If

                    If Color_Math_Div_2 Then
                        .R *= 0.5
                        .G *= 0.5
                        .B *= 0.5
                    End If
                End If
                If Not Transparent Or Color_Math Then Video_Buffer(Buffer_Position) = (.R * &H10000) + (.G * &H100) + .B
            End With
        End If
    End Sub
    Private Sub Apply_Mosaic(Scanline As Integer)
        Dim Src_Y As Integer = Scanline - (Scanline Mod Mosaic_Size)
        For X As Integer = 0 To 255 Step Mosaic_Size
            Dim Src_Color As Integer = Video_Buffer(X + (Src_Y * 256))
            For Copy_X As Integer = 0 To Mosaic_Size - 1
                If X + Copy_X >= 0 And X + Copy_X < 256 Then
                    Video_Buffer((X + Copy_X) + (Scanline * 256)) = Src_Color
                End If
            Next
        Next
    End Sub

    Private Function Add_Color(Val1 As Integer, Val2 As Integer)
        Dim Result As Integer = Val1 + Val2
        If Result > &HFF Then
            Return &HFF
        Else
            Return Result
        End If
    End Function
    Private Function Sub_Color(Val1 As Integer, Val2 As Integer)
        Dim Result As Integer = Val1 - Val2
        If Result < 0 Then
            Return 0
        Else
            Return Result
        End If
    End Function

    Public Sub Blit()

        Dim Img As New Bitmap(256, 224, Imaging.PixelFormat.Format32bppRgb)
        Dim BitmapData1 As Imaging.BitmapData
        BitmapData1 = Img.LockBits(New Rectangle(0, 0, 256, 224), Imaging.ImageLockMode.WriteOnly, Imaging.PixelFormat.Format32bppRgb)
        Dim Scan0 As IntPtr = BitmapData1.Scan0
        Runtime.InteropServices.Marshal.Copy(Video_Buffer, 0, Scan0, 256 * 224)
        Img.UnlockBits(BitmapData1)

        frmSobre.PicScreen.Image = Img

        'Limpa a tela
        Array.Clear(Video_Buffer, 0, Video_Buffer.Length)
        For Position As Integer = 0 To Video_Buffer_Sub.Length - 1
            Video_Buffer_Sub(Position) = (Fixed_Color.R * &H10000) + (Fixed_Color.G * &H100) + Fixed_Color.B
        Next

    End Sub
    Public Sub Screenshot()

        Take_Screenshot = False
        Dim Img As New Bitmap(256, 224, Imaging.PixelFormat.Format32bppRgb)
        Dim BitmapData1 As Imaging.BitmapData
        BitmapData1 = Img.LockBits(New Rectangle(0, 0, 256, 224), Imaging.ImageLockMode.WriteOnly, Imaging.PixelFormat.Format32bppRgb)
        Dim Scan0 As IntPtr = BitmapData1.Scan0
        Runtime.InteropServices.Marshal.Copy(Video_Buffer, 0, Scan0, 256 * 224)
        Img.UnlockBits(BitmapData1)
        Dim Save_Dlg As New SaveFileDialog
        Save_Dlg.Title = "Salvar Screenshot"
        Save_Dlg.Filter = "Imagem|*.png"
        Save_Dlg.FileName = Header.Name
        Save_Dlg.ShowDialog()
        If Save_Dlg.FileName <> Nothing Then Img.Save(Save_Dlg.FileName)

    End Sub

    Public Structure ROMHeader
        Dim Name As String
        Dim Hi_ROM As Boolean
        Dim Type As Byte
        Dim Banks As Byte
        Dim SRAM_Size As Byte
    End Structure
    Public Header As ROMHeader

    Public ROM_Data(0, &H7FFF) 'As ROMs são mapeadas em bancos de 32kb

    Public Sub Load_Rom(File_Name As String)

        Try


            Dim Data() As Byte = My.Resources.SMW

            Dim Banks As Integer = Data.Length / &H8000
            Dim Banks_Hi_ROM As Integer = Data.Length / &H10000
            ReDim ROM_Data(Banks - 1, &H7FFF)
            For Bank As Integer = 0 To Banks - 1
                For Offset As Integer = 0 To &H7FFF
                    ROM_Data(Bank, Offset) = Data((Bank * &H8000) + Offset)
                Next
            Next

            Dim Header_Bank As Integer
            If ROM_Data(1, &H7FDC) + (ROM_Data(1, &H7FDD) * &H100) + _
                ROM_Data(1, &H7FDE) + (ROM_Data(1, &H7FDF) * &H100) = &HFFFF Then Header_Bank = 1

            With Header
                .Name = Nothing
                For Offset As Integer = 0 To 20
                    .Name &= Chr(ROM_Data(Header_Bank, &H7FC0 + Offset))
                Next
                .Name = Header.Name.Trim
                .Hi_ROM = Header_Bank
                If Not .Hi_ROM Then
                    'Note to Mike: This should be used to load Interleaved Hi-ROMs
                    'but even LoRom was loading with this -> .Hi_ROM = ROM_Data(0, &H7FD5) And 1,
                    'so i Disabled for now...

                    '.Hi_ROM = ROM_Data(0, &H7FD5) And 1
                    If .Hi_ROM Then
                        Dim Read_Position As Integer
                        ReDim ROM_Data((Banks * 2) - 1, &H7FFF)
                        For Bank As Integer = 0 To Banks_Hi_ROM - 1
                            For Offset As Integer = 0 To &H7FFF
                                ROM_Data((Bank * 2) + 1, Offset) = Data(Read_Position + Offset)
                            Next
                            Read_Position += &H8000
                        Next
                        For Bank As Integer = 0 To Banks_Hi_ROM - 1
                            For Offset As Integer = 0 To &H7FFF
                                ROM_Data(Bank * 2, Offset) = Data(Read_Position + Offset)
                            Next
                            Read_Position += &H8000
                        Next
                    End If
                End If

                .Type = ROM_Data(Header_Bank, &H7FD6)
                .Banks = Banks
                .SRAM_Size = ROM_Data(Header_Bank, &H7FD8)
            End With


            'MsgBox("ok")
        Catch ex As Exception
            'MsgBox("erro  " & ex.Message)
        End Try


    End Sub

    Dim Skip, Set_ZF As Integer
    Public Sub Write_SPU(Address As Integer, Value As Byte)
        Select Case Address
            Case &H2140 To &H2147 : Set_ZF = 0
        End Select
    End Sub
    Public Function Read_SPU(Address As Integer) As Byte
        Dim Temp As Integer = Skip
        If Skip < 18 Then Skip += 1 Else Skip = 0
        Select Case Temp >> 1
            Case 0, 1, 6 : Set_ZF = 2 : Return 0
            Case 2 : If Temp And 1 Then Return (Registers.A And &HFF00) / &H100 Else Return Registers.A And &HFF
            Case 3 : If Temp And 1 Then Return (Registers.X And &HFF00) / &H100 Else Return Registers.X And &HFF
            Case 4 : If Temp And 1 Then Return (Registers.Y And &HFF00) / &H100 Else Return Registers.Y And &HFF
            Case 5, 7 : If Temp And 1 Then Return &HBB Else Return &HAA
            Case 8 : Return &H33
            Case 9 : Return 0
        End Select

        Return Nothing
    End Function

End Class
