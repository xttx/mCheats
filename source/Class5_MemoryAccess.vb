Imports System.Runtime.InteropServices
Public Class Class5_MemoryAccess
    Public Event processHasExited()
    Dim w As ArrayList = New ArrayList
    Dim out As UInteger
    Dim skip As Integer = 0
    Dim psx_slide As UInteger = 0
    Dim node As Xml.XmlNode
    Dim vernode As Xml.XmlNode
    Public emu As String = ""
    Public mType As String = ""
    Public rName As String = ""
    Private tempAddr As IntPtr
    Private baseaddr As IntPtr = IntPtr.Zero
    Private baseaddr2 As IntPtr = IntPtr.Zero
    Private p As Process = Nothing
    Private isTestingInstance As Boolean = False
    Public Cheats_CheatCodes_testing As New List(Of String)
    Private WithEvents timer As New Timer With {.Interval = 1000, .Enabled = False}
    <DllImport("kernel32.dll", SetLastError:=True)> _
    Private Shared Function ReadProcessMemory( _
       ByVal hProcess As IntPtr, _
       ByVal lpBaseAddress As IntPtr, _
       <Out()> ByVal lpBuffer() As Byte, _
       ByVal dwSize As Integer, _
       ByRef lpNumberOfBytesRead As Integer) As Boolean
    End Function
    <DllImport("kernel32.dll")>
    Public Shared Function WriteProcessMemory( _
        ByVal hProcess As IntPtr, _
        ByVal lpBaseAddress As IntPtr, _
        ByVal lpBuffer As Byte(), _
        ByVal nSize As UInt32, _
        ByRef lpNumberOfBytesWritten As UInt32) As Boolean
    End Function
    Public ReadOnly Property getBaseAddr As Long
        Get
            Return baseaddr.ToInt64
        End Get
    End Property
    Public ReadOnly Property getBaseAddr2 As Long
        Get
            Return baseaddr2.ToInt64
        End Get
    End Property


    Public Sub setup(ByVal pr As Process, x As Xml.XmlNode, x2 As Xml.XmlNode, Optional TestingInstance As Boolean = False)
        p = pr
        node = x
        vernode = x2
        isTestingInstance = TestingInstance
    End Sub

    Public Function getMashineType() As String
        Dim systems As Xml.XmlNodeList = node.SelectNodes("system")
        If systems.Count = 1 Then mType = systems(0).SelectSingleNode("name").InnerText : Return mType

        Dim detect As String
        CustomFunctions.p = p
        Dim econtext As New Ciloci.Flee.ExpressionContext(New CustomFunctions)
        For Each sys As Xml.XmlNode In systems
            detect = sys.SelectSingleNode("detect").InnerText.Replace("!=", "<>")
            Dim e As Ciloci.Flee.IGenericExpression(Of Boolean) = econtext.CompileGeneric(Of Boolean)(detect)
            If e.Evaluate Then mType = sys.SelectSingleNode("name").InnerText : Log("Machine: " + mType) : Return mType
        Next
        Log("Machine: NULL") : Return ""
    End Function

    Public Function getRomName() As String
        rName = ""
        If mType = "" Then Return ""

        CustomFunctions.p = p
        Dim detect As String = vernode.SelectSingleNode("romname").InnerText.Replace("!=", "<>")
        Dim econtext As New Ciloci.Flee.ExpressionContext(New CustomFunctions)
        Dim e As Ciloci.Flee.IGenericExpression(Of String) = econtext.CompileGeneric(Of String)(detect)
        rName = e.Evaluate : Log("RomName: " + rName)

        Dim getAddr As String = vernode.SelectSingleNode("baseaddr" + mType).InnerText.Replace("!=", "<>")
        Dim e2 As Ciloci.Flee.IGenericExpression(Of IntPtr) = econtext.CompileGeneric(Of IntPtr)(getAddr)
        baseaddr = e2.Evaluate : Log("BaseAddr: " + baseaddr.ToString)

        If vernode.SelectSingleNode("baseaddr" + mType + "2") IsNot Nothing Then
            getAddr = vernode.SelectSingleNode("baseaddr" + mType + "2").InnerText.Replace("!=", "<>")
            e2 = econtext.CompileGeneric(Of IntPtr)(getAddr)
            baseaddr2 = e2.Evaluate : Log("BaseAddr2: " + baseaddr2.ToString)
        End If

        Return rName
    End Function

    Public Property timerEnable() As Boolean
        Get
            timerEnable = timer.Enabled
        End Get
        Set(ByVal Value As Boolean)
            timer.Enabled = Value
        End Set
    End Property

    Public Function isPaused() As Short
        Dim addr As IntPtr
        Dim tmp(0) As Byte
        If vernode.SelectSingleNode("ispaused") IsNot Nothing Then
            'DOLPHIN: is paused MainModuleAddr + CD0D10 <- uje ne rabotaet. teper +CD0D0A & +48DF3A8 (last one is actually used)
            Try
                addr = p.MainModule.BaseAddress + CInt("&H" + vernode.SelectSingleNode("ispaused").InnerText)
                ReadProcessMemory(p.Handle, addr, tmp, tmp.Length, 0)
                If tmp(0) = 0 Then Return 1 Else Return 0
            Catch ex As Exception
                Return -2
            End Try
        End If
        Return -1
    End Function

    Dim codeIsFailed As Boolean = False
    Private Sub Main_Loop() Handles timer.Tick
        'Dim watch As Stopwatch = Stopwatch.StartNew()
        'Do While True
        'Application.DoEvents()

        If p.HasExited Then
            timer.Enabled = False
            RaiseEvent processHasExited()
            'Module1.MenuActive = False
            'KeyLeft.Unregister() : KeyRight.Unregister()
            'KeyUp.Unregister() : KeyDown.Unregister() : KeyBackSpace.Unregister() : KeyEnter.Unregister() : Exit Sub
        End If

        If isTestingInstance Then
            skip = 0
            codeIsFailed = False
            Dim cheatArr As New ArrayList
            For Each cht As String In Cheats_CheatCodes_testing
                doCheat(cht)
            Next
        Else
            For Each s As DictionaryEntry In CheatsStatus
                If s.Value.ToString = "1" Then
                    skip = 0
                    codeIsFailed = False
                    Dim cheatArr As New ArrayList
                    cheatArr = DirectCast(cl.getCheatsCheatCodes(cl.getCheatsCheatNames(CInt(s.Key))), ArrayList)
                    For Each cht As String In cheatArr
                        doCheat(cht)
                    Next
                End If
            Next
        End If


        'watch.Stop()
        'w.Add(watch.Elapsed.TotalMilliseconds)
        'Loop
    End Sub

    Private Sub doCheat(c As String)
        'NOTE:
        'Dim en1 As Byte() = {&H12, &H34}
        'WriteProcessMemory(p.Handle, baseAddr, en1, Convert.ToUInt32(en1.Length), 0)

        Dim tmp(3) As Byte
        Dim i As Integer = 0
        Dim addr As IntPtr
        Dim value() As Byte
        Dim tvalue As String
        Dim firstPartOfCode As String = ""

        If c.StartsWith("%") Then Exit Sub
        If c.Contains("?") Or c.Contains("x") Or c.Contains("X") Then Exit Sub
        c = c.Trim
        If mType = "NES" Then
            addr = New IntPtr(CInt("&H" + c.Substring(0, c.IndexOf(":"))))
            tvalue = c.Substring(c.IndexOf(":") + 1)
            If tvalue.Length <> 2 Then Exit Sub
            value = BitConverter.GetBytes(CShort("&H" + tvalue))
            Array.Resize(value, 1)
            Try
                If addr.ToInt32 < &H800 Then
                    addr = New IntPtr(baseaddr.ToInt32 + addr.ToInt32)
                    WriteProcessMemory(p.Handle, addr, value, Convert.ToUInt32(value.Length), 0)
                End If
                If addr.ToInt32 >= &H6000 And addr.ToInt32 < &H8000 Then
                    addr = New IntPtr(baseaddr2.ToInt32 + addr.ToInt32 - &H6000)
                    WriteProcessMemory(p.Handle, addr, value, Convert.ToUInt32(value.Length), 0)
                End If
            Catch ex As Exception
                MsgBox(ex.ToString, MsgBoxStyle.Critical, "Cheat Engine")
            End Try
        ElseIf mType = "GG" Or mType = "SMS" Then
            addr = New IntPtr(CInt("&H" + c.Substring(0, 4) + c.Substring(5, 2)))
            tvalue = c.Substring(7, 2)

            If tvalue.Length <> 2 Then Exit Sub
            value = BitConverter.GetBytes(CShort("&H" + tvalue))
            Array.Resize(value, 1)
            Try
                addr = New IntPtr(baseaddr.ToInt32 + addr.ToInt32)
                WriteProcessMemory(p.Handle, addr, value, Convert.ToUInt32(value.Length), 0)
            Catch ex As Exception
                MsgBox(ex.ToString, MsgBoxStyle.Critical, "Cheat Engine")
            End Try
        ElseIf mType = "SMD" Or mType = "32X" Or mType = "SCD" Then
            c = c.Replace(" ", ":")
            If c.Length = 8 Or c.Length = 10 Then c = c.Substring(0, 6) + ":" + c.Substring(6)
            addr = New IntPtr(CInt("&H" + c.Substring(0, c.IndexOf(":"))))
            tvalue = c.Substring(c.IndexOf(":") + 1)

            If tvalue.Length > 4 Then Exit Sub
            value = BitConverter.GetBytes(CInt("&H" + tvalue))
            Array.Resize(value, 2)
            Try
                addr = New IntPtr(baseaddr.ToInt32 + addr.ToInt32)
                WriteProcessMemory(p.Handle, addr, value, Convert.ToUInt32(value.Length), 0)
            Catch ex As Exception
                MsgBox(ex.ToString, MsgBoxStyle.Critical, "Cheat Engine")
            End Try
        ElseIf mType = "SNES" Then
            If c.Length < 8 Then Exit Sub
            c = c.Replace(" ", ":")
            If c.Length = 8 Or c.Length = 10 Then c = c.Substring(0, 6) + ":" + c.Substring(6)
            addr = New IntPtr(CInt("&H" + c.Substring(0, c.IndexOf(":"))))
            tvalue = c.Substring(c.IndexOf(":") + 1)

            If tvalue.Length > 2 Then Exit Sub
            value = BitConverter.GetBytes(CShort("&H" + tvalue))
            Array.Resize(value, 1)
            Try
                addr = New IntPtr(baseaddr.ToInt32 + addr.ToInt32)
                WriteProcessMemory(p.Handle, addr, value, Convert.ToUInt32(value.Length), 0)
            Catch ex As Exception
                MsgBox(ex.ToString, MsgBoxStyle.Critical, "Cheat Engine")
            End Try
        ElseIf mType = "GB" Or mType = "GBC" Then
            addr = New IntPtr(CInt("&H" + c.Substring(6, 2) + c.Substring(4, 2)))
            tvalue = c.Substring(2, 2)

            value = BitConverter.GetBytes(CShort("&H" + tvalue))
            Array.Resize(value, 1)
            Try
                addr = New IntPtr(baseaddr.ToInt32 + addr.ToInt32)
                WriteProcessMemory(p.Handle, addr, value, Convert.ToUInt32(value.Length), 0)
            Catch ex As Exception
                MsgBox(ex.ToString, MsgBoxStyle.Critical, "Cheat Engine")
            End Try
        ElseIf mType = "GBA" Then
            If skip > 0 And skip < 8000 Then skip = skip - 1 : Exit Sub
            If c.Trim.Length = 8 Then
                If firstPartOfCode = "" Then
                    firstPartOfCode = c.Trim
                Else
                    c = firstPartOfCode + " " + c.Trim : firstPartOfCode = ""
                End If
            End If
            If c.Length <> 17 Then Exit Sub
            Dim d As New Decoder.GBA
            Dim code As String
            code = d.decrypt_GameShark(CUInt("&H" + c.Substring(0, 8)), CUInt("&H" + c.Substring(9)), True)
            tvalue = code.Substring(9)
            do_gbaCodeAR(baseaddr, baseaddr2, code.ToUpper, tvalue)
        ElseIf mType = "N64" Then
            If c.Substring(0, 1) = "D" Then Exit Sub
            'byte 0 -> value byte3_byte2
            'byte 1 -> value byte4(!)_byte3
            'byte 2 -> value byte1_byte0
            'Byte 3 -> value byte2_byte1

            'Supports N64 game shark code types: 
            'Code Type Format Code Type Description 
            '80-XXXXXX 00YY	8-Bit Constant Write 
            '81-XXXXXX YYYY 16-Bit Constant Write 
            '50-00AABB CCCC Serial Repeater 50-00AABB CCCC AA-repeatecount, BB-addroffset. Repeat next code AA times, every time addr += BB and value += CCCC
            '88-XXXXXX 00YY 8-Bit GS Button Write 
            '89-XXXXXX YYYY 16-Bit GS Button Write 
            'A0-XXXXXX 00YY 8-Bit Constant Write (Uncached)   'just like 80
            'A1-XXXXXX YYYY 16-Bit Constant Write (Uncached)  'just like 81
            'D0-XXXXXX 00YY 8-Bit If Equal To 
            'D1-XXXXXX YYYY 16-Bit If Equal To 
            'D2-XXXXXX 00YY 8-Bit If Not Equal To 
            'D3-XXXXXX YYYY 16-Bit If Not Equal To 
            'DE-XXXXXX 0000 Download & Execute 
            'F0-XXXXXX 00YY 8-Bit Bootup Write Once    /* mode = 2 Apply code after bootup once only */
            'F1-XXXXXX YYYY 16-Bit Bootup Write Once   /* mode = 2 Apply code after bootup once only */
            c = c.Replace(" ", ":")
            Dim a As String = ""
            Dim offset As Integer = 0
            Dim method As Integer = 0
            If c.Substring(1, 1) = "1" Then method = 1
            If c.Length = 12 Then c = c.Substring(0, 8) + ":" + c.Substring(8)
            If c.Length <> 13 Then Exit Sub
            a = c.Substring(2, c.IndexOf(":") - 2)
            Select Case a.Substring(a.Length - 1)
                Case "0" : a = "3" : offset = -1
                Case "1" : a = "2" : offset = 1
                Case "2" : a = "1" : offset = -1
                Case "3" : a = "0" : offset = 1
                Case "4" : a = "7" : offset = -1
                Case "5" : a = "6" : offset = 1
                Case "6" : a = "5" : offset = -1
                Case "7" : a = "4" : offset = 1
                Case "8" : a = "B" : offset = -1
                Case "9" : a = "A" : offset = 1
                Case "A" : a = "9" : offset = -1
                Case "B" : a = "8" : offset = 1
                Case "C" : a = "F" : offset = -1
                Case "D" : a = "E" : offset = 1
                Case "E" : a = "D" : offset = -1
                Case "F" : a = "C" : offset = 1
            End Select
            addr = New IntPtr(CInt("&H" + c.Substring(2, c.IndexOf(":") - 3) + a))
            tvalue = c.Substring(c.IndexOf(":") + 1)

            If tvalue.Length <> 4 Then Exit Sub
            'tvalue = tvalue.Substring(2) + tvalue.Substring(0, 2)
            value = BitConverter.GetBytes(CInt("&H" + tvalue))
            If method = 1 Then Array.Resize(value, 2) Else Array.Resize(value, 1)
            Try
                'If emu = "project64" Then
                'pntr = New IntPtr(&H507AD8)
                'ReadProcessMemory(p.Handle, pntr, tmp, tmp.Length, 0)
                'addr = New IntPtr(BitConverter.ToInt32(tmp, 0) + addr.ToInt32)
                'Else
                'addr = addr + &H20000000
                'End If
                addr = New IntPtr(baseaddr.ToInt32 + addr.ToInt32)
                If method = 1 Then addr = addr + offset
                WriteProcessMemory(p.Handle, addr, value, Convert.ToUInt32(value.Length), 0)
            Catch ex As Exception
                MsgBox(ex.ToString, MsgBoxStyle.Critical, "Cheat Engine")
            End Try
        ElseIf mType = "NGC" Then
            If skip = -12 Then Exit Sub
            If skip > 0 And skip <> -11 Then skip = skip - 1 : Exit Sub
            If c = "00000000 40000000" Then
                If skip = -11 Then skip = 0
                Exit Sub
            End If
            If skip = -11 Then Exit Sub

            'Pointers in EXE @ 48DF198, 48DF1D0, 4A01D00 and there is another "mirror memory" pointer @ 48DF190
            If c.Length = 15 Then Exit Sub 'xxxx_xxxx_xxxxx Encoded AR code. Don't parse this for instance
            If c.Length <> 17 Then Exit Sub 'wrong format
            If c.Substring(0, 8) = "00000000" Then Exit Sub 'ZeroCode Don't parse this for instance
            'FIRST BYTE OF ADDR
            'xxyyyzza x=SUBTYPE y=TYPE z=SIZE a=ADDR
            'TYPE: 0: not conditional, 1=, 2!=, 3<int, 4>int, 5<uint, 6>uint, 7AND (bitwise AND)
            'SUBTYPE: 	0=RAM_WRITE, 1=WRITE_POINTER, 2=ADD_CODE, 3=MASTER_CODE
            'SIZE: 0=8bit, 1=16bit, 2=32bit, 3=32bitFloat

            Dim bits As String = Convert.ToString(Convert.ToInt32(c.Substring(0, 2), 16), 2)
            bits = Strings.StrDup(8 - bits.Length, "0") + bits
            If bits.Substring(0, 2) = "11" Then Exit Sub 'MASTER CODE subtype. Don't parse this.

            'get addr
            addr = New IntPtr(Convert.ToInt32(bits.Substring(7) + c.Substring(2, 6), 16))
            addr = New IntPtr(baseaddr.ToInt64 + addr.ToInt32)
            do_gamecubeCode(addr, bits, c, baseaddr)
            'If codeIsFailed Then s.Value = "0" : Exit For
        ElseIf mType = "PSX" Then
            If skip > 0 And skip < 4 Then skip = skip - 1 : Exit Sub
            If c.Length <> 13 Then Exit Sub
            addr = New IntPtr(CInt("&H" + c.Substring(2, 6)))

            addr = New IntPtr(baseaddr.ToInt32 + addr.ToInt32)
            tvalue = c.Substring(9)
            do_psxCode(addr, c.ToUpper, tvalue)
        ElseIf mType = "PS2" Then
            If skip > 0 Then skip = skip - 1 : Exit Sub
            If c.Length <> 17 Then Exit Sub
            If c.Substring(8, 1) <> " " Then Exit Sub
            Dim d As New Decoder.PS2
            Dim code As String
            code = d.DecryptGS2v2(CUInt("&H" + c.Substring(0, 8)), CUInt("&H" + c.Substring(9)))
            addr = New IntPtr(CInt("&H" + code.Substring(1, 7)))

            addr = New IntPtr(baseaddr.ToInt32 + addr.ToInt32)
            tvalue = code.Substring(9)
            If do_ps2Code(addr, code.ToUpper, tvalue) = -1 Then
                code = d.DecryptGS2v1(CUInt("&H" + c.Substring(0, 8)), CUInt("&H" + c.Substring(9)))
                addr = New IntPtr(CInt("&H" + code.Substring(1, 7)))

                addr = New IntPtr(baseaddr.ToInt32 + addr.ToInt32)
                tvalue = code.Substring(9)
                do_ps2Code(addr, code.ToUpper, tvalue)
            End If
        ElseIf mType = "SAT" Then
            If c.Length <> 13 Then Exit Sub
            addr = New IntPtr(CInt("&H" + c.Substring(1, 7)))
            tvalue = c.Substring(9).Trim
            If tvalue.Length <> 4 Then Exit Sub
            value = BitConverter.GetBytes(CUShort("&H" + tvalue))
            If c.Substring(0, 1) = "1" Then
                Array.Resize(value, 2)
                Array.Reverse(value)
            ElseIf c.Substring(0, 1) = "3" Then
                Array.Resize(value, 1)
            End If

            Try
                If addr.ToInt32 >= &H6000000 Then
                    addr = New IntPtr(baseaddr.ToInt32 + addr.ToInt32 - &H6000000)
                    WriteProcessMemory(p.Handle, addr, value, Convert.ToUInt32(value.Length), 0)
                End If
                If addr.ToInt32 >= &H200000 And addr.ToInt32 < &H300000 Then
                    addr = New IntPtr(baseaddr2.ToInt32 + addr.ToInt32)
                    WriteProcessMemory(p.Handle, addr, value, Convert.ToUInt32(value.Length), 0)
                End If
            Catch ex As Exception
                MsgBox(ex.ToString, MsgBoxStyle.Critical, "Cheat Engine")
            End Try
        Else 'Generic Codes - if system not found
            c = c.Replace(" ", ":")
            addr = New IntPtr(CInt("&H" + c.Substring(0, c.IndexOf(":"))))
            tvalue = c.Substring(c.IndexOf(":") + 1)

            value = BitConverter.GetBytes(CInt("&H" + tvalue))
            'If tvalue.Length > 4 Then
            'value = BitConverter.GetBytes(CInt("&H" + tvalue))
            'Else
            'value = BitConverter.GetBytes(CShort("&H" + tvalue))
            'End If

            Array.Resize(value, CInt(tvalue.Length / 2))
            Try
                addr = New IntPtr(baseaddr.ToInt32 + addr.ToInt32)
                WriteProcessMemory(p.Handle, addr, value, Convert.ToUInt32(value.Length), 0)
            Catch ex As Exception
                MsgBox(ex.ToString, MsgBoxStyle.Critical, "Cheat Engine")
            End Try
        End If
        'MsgBox("exited: " + c)
    End Sub

    Private Sub do_gamecubeCode(ByVal addr As IntPtr, ByVal bits As String, ByVal tvalue As String, ByVal addr0inemu As IntPtr)
        Dim value() As Byte
        Try
            If bits.Substring(2, 3) = "000" Then
                'NOT CONDITIONAL
                If bits.Substring(0, 2) = "00" Then
                    Dim t As UInteger

                    'Write & fill 8bits
                    If bits.Substring(5, 2) = "00" Then
                        value = BitConverter.GetBytes(CUInt("&H" + tvalue.Substring(15)))
                        Array.Resize(value, 1)
                        Dim repeat As Integer = CInt("&H" + tvalue.Substring(9)) >> 8
                        For i = 0 To repeat
                            WriteProcessMemory(p.Handle, addr + i, value, Convert.ToUInt32(value.Length), t)
                        Next
                    End If

                    'Write & fill 16bits
                    If bits.Substring(5, 2) = "01" Then
                        value = BitConverter.GetBytes(CUInt("&H" + tvalue.Substring(13)))
                        Array.Resize(value, 2)
                        Array.Reverse(value)
                        Dim repeat As Integer = CInt("&H" + tvalue.Substring(9)) >> 16
                        For i = 0 To repeat
                            WriteProcessMemory(p.Handle, addr + i * 2, value, Convert.ToUInt32(value.Length), t)
                        Next
                    End If

                    'Write 32bits or 32bits float
                    If bits.Substring(5, 2) = "10" Or bits.Substring(5, 2) = "11" Then
                        value = BitConverter.GetBytes(CUInt("&H" + tvalue.Substring(9)))
                        Array.Reverse(value)
                        WriteProcessMemory(p.Handle, addr, value, Convert.ToUInt32(value.Length), t)
                    End If
                ElseIf bits.Substring(0, 2) = "01" Then
                    'WRITE TO POINTER
                    Dim tmp(3) As Byte
                    Dim offset As Integer
                    'read pointer to tmp
                    ReadProcessMemory(p.Handle, addr, tmp, tmp.Length, 0)
                    Array.Reverse(tmp)
                    Dim pointaddr As UInteger = CUInt(BitConverter.ToUInt32(tmp, 0))
                    pointaddr = pointaddr << 8 >> 8
                    'If pointaddr > &H7FFFFFFF Then pointaddr = pointaddr - UInt32.Parse("80000000", Globalization.NumberStyles.HexNumber)

                    'Write to pointer 8bits
                    If bits.Substring(5, 2) = "00" Then
                        offset = CInt("&H" + tvalue.Substring(9, 6))
                        addr = New IntPtr(pointaddr + offset + addr0inemu.ToInt64)
                        value = BitConverter.GetBytes(CUInt("&H" + tvalue.Substring(15)))
                        Array.Resize(value, 1)
                        WriteProcessMemory(p.Handle, addr, value, Convert.ToUInt32(value.Length), 0)
                    End If

                    'Write to pointer 16bits
                    If bits.Substring(5, 2) = "01" Then
                        offset = CInt("&H" + tvalue.Substring(9, 4))
                        addr = New IntPtr(pointaddr + offset * 2 + addr0inemu.ToInt64)
                        value = BitConverter.GetBytes(CUInt("&H" + tvalue.Substring(13)))
                        Array.Resize(value, 2) : Array.Reverse(value)
                        WriteProcessMemory(p.Handle, addr, value, Convert.ToUInt32(value.Length), 0)
                    End If

                    'Write to pointer 32bits or 32bits float
                    If bits.Substring(5, 2) = "10" Or bits.Substring(5, 2) = "11" Then
                        addr = New IntPtr(pointaddr + addr0inemu.ToInt64)
                        value = BitConverter.GetBytes(CUInt("&H" + tvalue.Substring(9)))
                        Array.Reverse(value)
                        WriteProcessMemory(p.Handle, addr, value, Convert.ToUInt32(value.Length), 0)
                    End If

                ElseIf bits.Substring(0, 2) = "10" Then
                    'Add 8bit
                    If bits.Substring(5, 2) = "00" Then
                        Dim tmp(0) As Byte
                        ReadProcessMemory(p.Handle, addr, tmp, tmp.Length, 0)
                        value = BitConverter.GetBytes(SByte.Parse(tvalue.Substring(15), Globalization.NumberStyles.HexNumber) + CUInt(tmp(0)))
                        WriteProcessMemory(p.Handle, addr, value, Convert.ToUInt32(value.Length), 0)
                    End If

                    'Add 16bit
                    If bits.Substring(5, 2) = "01" Then
                        Dim tmp(1) As Byte
                        ReadProcessMemory(p.Handle, addr, tmp, tmp.Length, 0)
                        Array.Reverse(tmp)
                        value = BitConverter.GetBytes(Short.Parse(tvalue.Substring(13), Globalization.NumberStyles.HexNumber) + CInt(BitConverter.ToInt16(tmp, 0)))
                        Array.Reverse(value)
                        WriteProcessMemory(p.Handle, addr, value, Convert.ToUInt32(value.Length), 0)
                    End If

                    'Add 32bit - CHECKED: OK
                    If bits.Substring(5, 2) = "10" Then
                        Dim tmp(3) As Byte
                        ReadProcessMemory(p.Handle, addr, tmp, tmp.Length, 0)
                        Array.Reverse(tmp)
                        value = BitConverter.GetBytes(Integer.Parse(tvalue.Substring(9), Globalization.NumberStyles.HexNumber) + CInt(BitConverter.ToInt32(tmp, 0)))
                        Array.Reverse(value)
                        WriteProcessMemory(p.Handle, addr, value, Convert.ToUInt32(value.Length), 0)
                    End If
                End If
            Else
                'FOR CONDITIONAL SUBTYPES ARE 0 = 1 line, 1 = 2 lines, 2 = until '00000000 40000000', 3 = all lines
                Dim lines_to_skip As Integer = 0
                Dim compare_memory, compare_code As UInteger
                lines_to_skip = CInt(bits.Substring(0, 2)) + 1
                If lines_to_skip > 2 Then lines_to_skip = 0 - lines_to_skip
                If bits.Substring(5, 2) = "00" Then
                    Dim tmp(0) As Byte
                    ReadProcessMemory(p.Handle, addr, tmp, tmp.Length, 0)
                    compare_memory = tmp(0)
                    compare_code = CUInt("&H" + tvalue.Substring(15))
                ElseIf bits.Substring(5, 2) = "01" Then
                    Dim tmp(1) As Byte
                    ReadProcessMemory(p.Handle, addr, tmp, tmp.Length, 0)
                    Array.Reverse(tmp)
                    compare_memory = CUInt(BitConverter.ToUInt16(tmp, 0))
                    compare_code = CUInt("&H" + tvalue.Substring(13))
                ElseIf bits.Substring(5, 2) = "10" Or bits.Substring(5, 2) = "11" Then
                    Dim tmp(3) As Byte
                    ReadProcessMemory(p.Handle, addr, tmp, tmp.Length, 0)
                    Array.Reverse(tmp)
                    compare_memory = CUInt(BitConverter.ToUInt32(tmp, 0))
                    compare_code = CUInt("&H" + tvalue.Substring(9))
                End If

                If bits.Substring(2, 3) = "001" Then
                    'equal
                    If Not compare_memory = compare_code Then skip = lines_to_skip : Exit Sub
                ElseIf bits.Substring(2, 3) = "010" Then
                    'not equal
                    If Not compare_memory <> compare_code Then skip = lines_to_skip : Exit Sub
                ElseIf bits.Substring(2, 3) = "011" Then
                    'if memory less then value (signed) - execute, else skip
                    Dim t1 As String = Hex(compare_memory)
                    Dim t2 As String = Hex(compare_code)
                    Dim compare_memory_signed, compare_code_signed As Integer
                    If bits.Substring(5, 2) = "00" Then
                        compare_memory_signed = SByte.Parse(t1, Globalization.NumberStyles.HexNumber)
                        compare_code_signed = SByte.Parse(t2, Globalization.NumberStyles.HexNumber)
                    ElseIf bits.Substring(5, 2) = "01" Then
                        compare_memory_signed = Short.Parse(t1, Globalization.NumberStyles.HexNumber)
                        compare_code_signed = Short.Parse(t2, Globalization.NumberStyles.HexNumber)
                    ElseIf bits.Substring(5, 2) = "10" Or bits.Substring(5, 2) = "11" Then
                        compare_memory_signed = Integer.Parse(t1, Globalization.NumberStyles.HexNumber)
                        compare_code_signed = Integer.Parse(t2, Globalization.NumberStyles.HexNumber)
                    End If
                    If Not compare_memory_signed < compare_code_signed Then skip = lines_to_skip : Exit Sub
                ElseIf bits.Substring(2, 3) = "100" Then
                    'grater than signed
                    Dim t1 As String = Hex(compare_memory)
                    Dim t2 As String = Hex(compare_code)
                    Dim compare_memory_signed, compare_code_signed As Integer
                    If bits.Substring(5, 2) = "00" Then
                        compare_memory_signed = SByte.Parse(t1, Globalization.NumberStyles.HexNumber)
                        compare_code_signed = SByte.Parse(t2, Globalization.NumberStyles.HexNumber)
                    ElseIf bits.Substring(5, 2) = "01" Then
                        compare_memory_signed = Short.Parse(t1, Globalization.NumberStyles.HexNumber)
                        compare_code_signed = Short.Parse(t2, Globalization.NumberStyles.HexNumber)
                    ElseIf bits.Substring(5, 2) = "10" Or bits.Substring(5, 2) = "11" Then
                        compare_memory_signed = Integer.Parse(t1, Globalization.NumberStyles.HexNumber)
                        compare_code_signed = Integer.Parse(t2, Globalization.NumberStyles.HexNumber)
                    End If
                    If Not compare_memory_signed > compare_code_signed Then skip = lines_to_skip : Exit Sub
                ElseIf bits.Substring(2, 3) = "101" Then
                    'less then unsigned
                    If Not compare_memory < compare_code Then skip = lines_to_skip : Exit Sub
                ElseIf bits.Substring(2, 3) = "110" Then
                    'grater than unsigned
                    If Not compare_memory > compare_code Then skip = lines_to_skip : Exit Sub
                ElseIf bits.Substring(2, 3) = "111" Then
                    'bitwise and

                End If
            End If
        Catch ex As Exception
            codeIsFailed = True
            MsgBox(ex.ToString + vbCrLf + "CODE: " + tvalue, MsgBoxStyle.Critical, "Cheat Engine")
        End Try
    End Sub

    Private Sub do_psxCode(ByVal addr As IntPtr, ByVal code As String, ByVal tvalue As String)
        Dim value() As Byte
        Dim slidetimes As Integer = 0, slideaddr As Integer = 0, slideval As Integer = 0

        If psx_slide > 0 Then
            slidetimes = CInt(psx_slide >> 24)
            slideaddr = CInt(psx_slide << 8 >> 24)
            slideval = CInt(psx_slide << 16 >> 16)
            psx_slide = 0
        End If

        'If skip > 3 Then
        'it works, but for positive numbers only!
        'slidetimes = skip >> 24
        'slideaddr = skip << 8 >> 24
        'slideval = skip << 16 >> 16
        'skip = 0
        'End If

        Select Case code.Substring(0, 2)
            Case "30"
                '8 bit write
                For i As Integer = 0 To slidetimes
                    value = BitConverter.GetBytes(CInt("&H" + tvalue.Substring(2)) + slideval * i)
                    Array.Resize(value, 1)
                    WriteProcessMemory(p.Handle, addr, value, Convert.ToUInt32(value.Length), out)
                    addr = addr + slideaddr
                Next
            Case "80"
                '16 bit write
                For i As Integer = 0 To slidetimes
                    value = BitConverter.GetBytes(CInt("&H" + tvalue) + slideval * i)
                    Array.Resize(value, 2)
                    WriteProcessMemory(p.Handle, addr, value, Convert.ToUInt32(value.Length), out)
                    addr = addr + slideaddr
                Next
            Case "50"
                'Slide 5000xxyy zzzz. Use next address. Write it xx times incrementing addr by yy & val by zzzz
                'skip = CInt("&H" + code.Substring(4, 4) + tvalue)
                psx_slide = CUInt("&H" + code.Substring(4, 4) + tvalue)
            Case "E0"
                '8-bit Equal To
                Dim tmp(0) As Byte
                Dim compare_memory, compare_code As UInteger
                ReadProcessMemory(p.Handle, addr, tmp, tmp.Length, 0)
                compare_memory = CUInt(tmp(0))
                compare_code = CUInt("&H" + tvalue.Substring(2))
                If Not compare_memory = compare_code Then skip = 1 : Exit Sub
            Case "E1"
                '8-bit Not Equal To
                Dim tmp(0) As Byte
                Dim compare_memory, compare_code As UInteger
                ReadProcessMemory(p.Handle, addr, tmp, tmp.Length, 0)
                compare_memory = CUInt(tmp(0))
                compare_code = CUInt("&H" + tvalue.Substring(2))
                If Not compare_memory <> compare_code Then skip = 1 : Exit Sub
            Case "E2"
                '8-bit Less Than
                Dim tmp(0) As Byte
                Dim compare_memory, compare_code As UInteger
                ReadProcessMemory(p.Handle, addr, tmp, tmp.Length, 0)
                compare_memory = CUInt(tmp(0))
                compare_code = CUInt("&H" + tvalue.Substring(2))
                If Not compare_memory < compare_code Then skip = 1 : Exit Sub
            Case "E3"
                '8-bit Greater Than
                Dim tmp(0) As Byte
                Dim compare_memory, compare_code As UInteger
                ReadProcessMemory(p.Handle, addr, tmp, tmp.Length, 0)
                compare_memory = CUInt(tmp(0))
                compare_code = CUInt("&H" + tvalue.Substring(2))
                If Not compare_memory > compare_code Then skip = 1 : Exit Sub
            Case "D0"
                '16-bit Equal To
                Dim tmp(1) As Byte
                Dim compare_memory, compare_code As UInteger
                ReadProcessMemory(p.Handle, addr, tmp, tmp.Length, 0)
                compare_memory = CUInt(BitConverter.ToUInt16(tmp, 0))
                compare_code = CUInt("&H" + tvalue)
                If Not compare_memory = compare_code Then skip = 1 : Exit Sub
            Case "D1"
                '16-bit Not Equal To
                Dim tmp(1) As Byte
                Dim compare_memory, compare_code As UInteger
                ReadProcessMemory(p.Handle, addr, tmp, tmp.Length, 0)
                compare_memory = CUInt(BitConverter.ToUInt16(tmp, 0))
                compare_code = CUInt("&H" + tvalue)
                If Not compare_memory <> compare_code Then skip = 1 : Exit Sub
            Case "D2"
                '16-bit Less Than
                Dim tmp(1) As Byte
                Dim compare_memory, compare_code As UInteger
                ReadProcessMemory(p.Handle, addr, tmp, tmp.Length, 0)
                compare_memory = CUInt(BitConverter.ToUInt16(tmp, 0))
                compare_code = CUInt("&H" + tvalue)
                If Not compare_memory < compare_code Then skip = 1 : Exit Sub
            Case "D3"
                '16-bit Greater Than
                '16-bit Less Than
                Dim tmp(1) As Byte
                Dim compare_memory, compare_code As UInteger
                ReadProcessMemory(p.Handle, addr, tmp, tmp.Length, 0)
                compare_memory = CUInt(BitConverter.ToUInt16(tmp, 0))
                compare_code = CUInt("&H" + tvalue)
                If Not compare_memory > compare_code Then skip = 1 : Exit Sub
            Case "D4"
                '16-bit Universal Button Code
            Case "D5"
                '16-bit Codes On Code
            Case "D6"
                '16-bit Codes Off Code
            Case "10"
                '16-bit Increment
            Case "11"
                '16-bit Decrement
            Case "20"
                '8-bit Increment
            Case "21"
                '8-bit Decrement
            Case "C0"
                'Enable All Codes Trigger
            Case "C1"
                'Code Delay
            Case "C2"
                'Memory Copy
            Case "1F"
                'Special 16-bit Write
        End Select
    End Sub

    Private Function do_ps2Code(ByVal addr As IntPtr, ByVal code As String, ByVal tvalue As String) As Integer
        Dim value() As Byte
        If code.Substring(0, 1) = "0" And code.Substring(9, 6) = "000000" Then
            '8-bit RAM write
            value = BitConverter.GetBytes(CInt("&H" + tvalue.Substring(6)))
            Array.Resize(value, 1)
            WriteProcessMemory(p.Handle, addr, value, Convert.ToUInt32(value.Length), 0)
            Return 0
        End If
        If code.Substring(0, 1) = "1" And code.Substring(9, 4) = "0000" Then
            '16-bit RAM write
            value = BitConverter.GetBytes(CInt("&H" + tvalue.Substring(4)))
            Array.Resize(value, 2)
            WriteProcessMemory(p.Handle, addr, value, Convert.ToUInt32(value.Length), 0)
            Return 0
        End If
        If code.Substring(0, 1) = "2" Then
            '32-bit RAM write
            value = BitConverter.GetBytes(CUInt("&H" + tvalue))
            Array.Resize(value, 4)
            WriteProcessMemory(p.Handle, addr, value, Convert.ToUInt32(value.Length), 0)
            Return 0
        End If
        If code.Substring(0, 6) = "301000" Then
            '8-bit Increment
            Return 0
        End If
        If code.Substring(0, 6) = "302000" Then
            '8-bit Decrement
            Return 0
        End If
        If code.Substring(0, 4) = "3030" Then
            '16-bit Increment
            Return 0
        End If
        If code.Substring(0, 4) = "3040" Then
            '16-bit Decrement
            Return 0
        End If
        If code.Substring(0, 8) = "30500000" Then
            '32-bit Increment
            Return 0
        End If
        If code.Substring(0, 8) = "30600000" Then
            '32-bit Decrement
            Return 0
        End If
        If code.Substring(0, 1) = "D" And code.Substring(9, 4) = "0000" Then
            '16-bit If Equal To
            skip = 1
            Return 0
        End If
        If code.Substring(0, 1) = "D" And code.Substring(9, 4) = "0010" Then
            '16-bit If NOT Equal To
            skip = 1
            Return 0
        End If
        If code.Substring(0, 1) = "D" And code.Substring(9, 4) = "0020" Then
            '16-bit If Less Than
            skip = 1
            Return 0
        End If
        If code.Substring(0, 1) = "D" And code.Substring(9, 4) = "0030" Then
            '16-bit If Greater Than
            skip = 1
            Return 0
        End If
        If code.Substring(0, 1) = "E" And code.Substring(9, 1) = "0" Then
            '16-bit If Equal To: Multi-line Skip
            If code.Substring(10, 1) <> "0" And code.Substring(10, 1) <> "1" Then Return -1
            skip = CInt(code.Substring(1, 3))
            Return 0
        End If
        If code.Substring(0, 1) = "E" And code.Substring(9, 1) = "1" Then
            '16-bit If NOT Equal To: Multi-line Skip
            If code.Substring(10, 1) <> "0" And code.Substring(10, 1) <> "1" Then Return -1
            skip = CInt(code.Substring(1, 3))
            Return 0
        End If
        If code.Substring(0, 1) = "E" And code.Substring(9, 1) = "2" Then
            '16-bit If Less Than: Multi-line Skip
            If code.Substring(10, 1) <> "0" And code.Substring(10, 1) <> "1" Then Return -1
            skip = CInt(code.Substring(1, 3))
            Return 0
        End If
        If code.Substring(0, 1) = "E" And code.Substring(9, 1) = "3" Then
            '16-bit If Greater Than: Multi-line Skip
            If code.Substring(10, 1) <> "0" And code.Substring(10, 1) <> "1" Then Return -1
            skip = CInt(code.Substring(1, 3))
            Return 0
        End If
        If code.Substring(0, 1) = "5" Then
            'Copy Bytes
            If code.Substring(1, 1) <> "0" And code.Substring(1, 1) <> "1" Then Return -1
            skip = 1
            Return 0
        End If
        If code.Substring(0, 1) = "4" Then
            'Slide Code
            If code.Substring(1, 1) <> "0" And code.Substring(1, 1) <> "1" Then Return -1
            skip = 1
            Return 0
        End If
        If code.Substring(0, 8) = "DAEDFACE" Then
            'Deadface
            Return 0
        End If
        Return -1
    End Function

    Private Sub do_gbaCodeGS(ByVal addr As IntPtr, ByVal code As String, ByVal tvalue As String)
        Dim value() As Byte
        Select Case code.Substring(0, 1)
            Case "0"
                '8 bit write
                value = BitConverter.GetBytes(CInt("&H" + tvalue.Substring(6)))
                Array.Resize(value, 1)
                WriteProcessMemory(p.Handle, addr, value, Convert.ToUInt32(value.Length), 0)
            Case "1"
                '16 bit write
                value = BitConverter.GetBytes(CInt("&H" + tvalue.Substring(4)))
                Array.Resize(value, 2)
                WriteProcessMemory(p.Handle, addr, value, Convert.ToUInt32(value.Length), 0)
            Case "2"
                '32 bit write
                value = BitConverter.GetBytes(CInt("&H" + tvalue))
                Array.Resize(value, 4)
                WriteProcessMemory(p.Handle, addr, value, Convert.ToUInt32(value.Length), 0)
            Case "3"
                '32-bit group write
            Case "6"
                '16-bit ROM patch
            Case "8"
                '8/16bit GS Button RAM write + Slowdown on GS Button
            Case "D"
                '16-bit If Equal To
            Case "E"
                '16-bit Multiline If Equal To 
                skip = CInt("&H" + code.Substring(1, 2))
            Case "F"
                'Hook routine
        End Select

    End Sub

    Private Sub do_gbaCodeAR(ByVal addr1 As IntPtr, addr2 As IntPtr, ByVal code As String, ByVal tvalue As String)
        Dim addr As IntPtr
        Dim value() As Byte
        Dim slidetimes As Integer = 0, slideaddr As Integer = 0, slideval As Integer = 0
        If CUInt("&H" + tvalue) = 0 Then Exit Sub

        'Zerro codes
        Select Case skip
            Case 8000
                '8-bit slide
                For i As Integer = 0 To CInt("&H" + tvalue.Substring(2, 2)) - 1
                    value = BitConverter.GetBytes(CInt("&H" + code.Substring(6, 2)) + CInt("&H" + tvalue.Substring(0, 2)) * i)
                    Array.Resize(value, 1)
                    WriteProcessMemory(p.Handle, tempAddr, value, Convert.ToUInt32(value.Length), out)
                    tempAddr = tempAddr + CInt("&H" + tvalue.Substring(4))
                Next
                skip = 0 : Exit Sub
            Case 16000
                '16-bit slide
                For i As Integer = 0 To CInt("&H" + tvalue.Substring(2, 2)) - 1
                    value = BitConverter.GetBytes(CInt("&H" + code.Substring(4, 4)) + CInt("&H" + tvalue.Substring(0, 2)) * i)
                    Array.Resize(value, 2) : Array.Reverse(value)
                    WriteProcessMemory(p.Handle, tempAddr, value, Convert.ToUInt32(value.Length), out)
                    tempAddr = tempAddr + CInt("&H" + tvalue.Substring(4)) * 2
                Next
                skip = 0 : Exit Sub
            Case 32000
                '32-bit slide
                For i As Integer = 0 To CInt("&H" + tvalue.Substring(2, 2)) - 1
                    value = BitConverter.GetBytes(CInt("&H" + code.Substring(0, 8)) + CInt("&H" + tvalue.Substring(0, 2)) * i)
                    Array.Resize(value, 4)
                    WriteProcessMemory(p.Handle, tempAddr, value, Convert.ToUInt32(value.Length), out)
                    tempAddr = tempAddr + CInt("&H" + tvalue.Substring(4)) * 4
                Next
                skip = 0 : Exit Sub
        End Select

        Select Case code.Substring(0, 11)
            Case "00000000 80"
                '8-bit slide
                tempAddr = New IntPtr(CInt("&H" + code.Substring(12, 5)))
                skip = 8000
                If code.Substring(11, 1) = "3" Then
                    tempAddr = New IntPtr(baseaddr.ToInt32 + tempAddr.ToInt32)
                ElseIf code.Substring(11, 1) = "2" Then
                    tempAddr = New IntPtr(baseaddr2.ToInt32 + tempAddr.ToInt32)
                Else
                    skip = 1
                End If
                Exit Sub
            Case "00000000 82"
                '16-bit slide
                tempAddr = New IntPtr(CInt("&H" + code.Substring(12, 5)))
                skip = 16000
                If code.Substring(11, 1) = "3" Then
                    tempAddr = New IntPtr(baseaddr.ToInt32 + tempAddr.ToInt32)
                ElseIf code.Substring(11, 1) = "2" Then
                    tempAddr = New IntPtr(baseaddr2.ToInt32 + tempAddr.ToInt32)
                Else
                    skip = 1
                End If
                Exit Sub
            Case "00000000 84"
                '32-bit slide
                tempAddr = New IntPtr(CInt("&H" + code.Substring(12, 5)))
                skip = 32000
                If code.Substring(11, 1) = "3" Then
                    tempAddr = New IntPtr(baseaddr.ToInt32 + tempAddr.ToInt32)
                ElseIf code.Substring(11, 1) = "2" Then
                    tempAddr = New IntPtr(baseaddr2.ToInt32 + tempAddr.ToInt32)
                Else
                    skip = 1
                End If
                Exit Sub
        End Select

        addr = New IntPtr(CInt("&H" + code.Substring(3, 5)))
        If code.Substring(2, 1) = "3" Then
            addr = New IntPtr(baseaddr.ToInt32 + addr.ToInt32)
        ElseIf code.Substring(2, 1) = "2" Then
            addr = New IntPtr(baseaddr2.ToInt32 + addr.ToInt32)
        Else
            addr = IntPtr.Zero
        End If

        Select Case code.Substring(0, 2)
            Case "00"
                '8 bit write/Fill
                value = BitConverter.GetBytes(CInt("&H" + tvalue.Substring(6)))
                Array.Resize(value, 1)
                For i As Integer = 0 To CInt("&H" + tvalue.Substring(0, 6))
                    WriteProcessMemory(p.Handle, addr, value, Convert.ToUInt32(value.Length), 0)
                    addr = addr + 1
                Next
            Case "02"
                '16 bit write/Fill
                value = BitConverter.GetBytes(CInt("&H" + tvalue.Substring(4)))
                Array.Resize(value, 2) : Array.Reverse(value)
                For i As Integer = 0 To CInt("&H" + tvalue.Substring(0, 4)) * 2 Step 2
                    WriteProcessMemory(p.Handle, addr, value, Convert.ToUInt32(value.Length), 0)
                    addr = addr + 2
                Next
            Case "04"
                '32 bit write
                value = BitConverter.GetBytes(CUInt("&H" + tvalue))
                Array.Resize(value, 4) ': Array.Reverse(value)
                WriteProcessMemory(p.Handle, addr, value, Convert.ToUInt32(value.Length), 0)

            Case "40"
                '8-bit Pointer RAM Write
                Dim tmp(3) As Byte
                Dim offset As Integer
                ReadProcessMemory(p.Handle, addr, tmp, 3, 0)
                Dim pointaddr As UInteger = CUInt(BitConverter.ToUInt32(tmp, 0))

                offset = CInt("&H" + tvalue.Substring(0, 6))
                addr = New IntPtr(pointaddr + offset)
                If code.Substring(2, 1) = "3" Then addr = New IntPtr(baseaddr.ToInt32 + addr.ToInt32) Else addr = New IntPtr(baseaddr2.ToInt32 + addr.ToInt32)
                value = BitConverter.GetBytes(CUInt("&H" + tvalue.Substring(6)))
                Array.Resize(value, 1)
                WriteProcessMemory(p.Handle, addr, value, Convert.ToUInt32(value.Length), 0)
            Case "42"
                '16-bit Pointer RAM Write
                Dim tmp(3) As Byte
                Dim offset As Integer
                ReadProcessMemory(p.Handle, addr, tmp, 3, 0)
                Dim pointaddr As UInteger = CUInt(BitConverter.ToUInt32(tmp, 0))

                offset = CInt("&H" + tvalue.Substring(0, 4))
                addr = New IntPtr(pointaddr + offset * 2)
                If code.Substring(2, 1) = "3" Then addr = New IntPtr(baseaddr.ToInt32 + addr.ToInt32) Else addr = New IntPtr(baseaddr2.ToInt32 + addr.ToInt32)
                value = BitConverter.GetBytes(CUInt("&H" + tvalue.Substring(4)))
                Array.Resize(value, 2) : Array.Reverse(value)
                WriteProcessMemory(p.Handle, addr, value, Convert.ToUInt32(value.Length), 0)
            Case "44"
                '32-bit Pointer RAM Write
                Dim tmp(3) As Byte
                ReadProcessMemory(p.Handle, addr, tmp, 3, 0)
                Dim pointaddr As UInteger = CUInt(BitConverter.ToUInt32(tmp, 0))

                addr = New IntPtr(pointaddr)
                If code.Substring(2, 1) = "3" Then addr = New IntPtr(baseaddr.ToInt32 + addr.ToInt32) Else addr = New IntPtr(baseaddr2.ToInt32 + addr.ToInt32)
                value = BitConverter.GetBytes(CUInt("&H" + tvalue))
                Array.Reverse(value)
                WriteProcessMemory(p.Handle, addr, value, Convert.ToUInt32(value.Length), 0)
            Case "80"
                '8-bit Add code
            Case "82"
                '16-bit Add code
            Case "84"
                '32-bit Add Code

            Case "08"
                '8-bit If Equal To 1-line
                Dim tmp(3) As Byte
                ReadProcessMemory(p.Handle, addr, tmp, 1, 0)
                Dim readValue As UInteger = CUInt(BitConverter.ToUInt32(tmp, 0))
                If readValue = CUInt("&H" + tvalue.Substring(6)) Then
                    Exit Sub
                End If
                skip = 1
            Case "48"
                '8-bit If Equal To 2-lines
                Dim tmp(3) As Byte
                ReadProcessMemory(p.Handle, addr, tmp, 1, 0)
                Dim readValue As UInteger = CUInt(BitConverter.ToUInt32(tmp, 0))
                If readValue = CUInt("&H" + tvalue.Substring(6)) Then
                    Exit Sub
                End If
                skip = 2
            Case "88"
                '8-bit If Equal To multi-lines (until the z20 code type or end of code list are executed)
                Dim tmp(3) As Byte
                ReadProcessMemory(p.Handle, addr, tmp, 1, 0)
                Dim readValue As UInteger = CUInt(BitConverter.ToUInt32(tmp, 0))
                If readValue = CUInt("&H" + tvalue.Substring(6)) Then
                    Exit Sub
                End If
                skip = 65535
            Case "C8"
                '8-bit If Equal To all-lines
                Dim tmp(3) As Byte
                ReadProcessMemory(p.Handle, addr, tmp, 1, 0)
                Dim readValue As UInteger = CUInt(BitConverter.ToUInt32(tmp, 0))
                If readValue = CUInt("&H" + tvalue.Substring(6)) Then
                    Exit Sub
                End If
                skip = 65535

            Case "0A"
                '16-bit If Equal To 1-line
                Dim tmp(3) As Byte
                ReadProcessMemory(p.Handle, addr, tmp, 2, 0)
                Dim readValue As UInteger = CUInt(BitConverter.ToUInt32(tmp, 0))
                If readValue = CUInt("&H" + tvalue.Substring(4)) Then
                    Exit Sub
                End If
                skip = 1
            Case "4A"
                '16-bit If Equal To 2-lines
                Dim tmp(3) As Byte
                ReadProcessMemory(p.Handle, addr, tmp, 2, 0)
                Dim readValue As UInteger = CUInt(BitConverter.ToUInt32(tmp, 0))
                If readValue = CUInt("&H" + tvalue.Substring(4)) Then
                    Exit Sub
                End If
                skip = 2
            Case "8A"
                '16-bit If Equal To multi-lines (until the z20 code type or end of code list are executed)
                Dim tmp(3) As Byte
                ReadProcessMemory(p.Handle, addr, tmp, 2, 0)
                Dim readValue As UInteger = CUInt(BitConverter.ToUInt32(tmp, 0))
                If readValue = CUInt("&H" + tvalue.Substring(4)) Then
                    Exit Sub
                End If
                skip = 65535
            Case "CA"
                '16-bit If Equal To all-lines
                Dim tmp(3) As Byte
                ReadProcessMemory(p.Handle, addr, tmp, 2, 0)
                Dim readValue As UInteger = CUInt(BitConverter.ToUInt32(tmp, 0))
                If readValue = CUInt("&H" + tvalue.Substring(4)) Then
                    Exit Sub
                End If
                skip = 65535

            Case "0C"
                '32-bit If Equal To 1-line
                Dim tmp(3) As Byte
                ReadProcessMemory(p.Handle, addr, tmp, 4, 0)
                Dim readValue As UInteger = CUInt(BitConverter.ToUInt32(tmp, 0))
                If readValue = CUInt("&H" + tvalue) Then
                    Exit Sub
                End If
                skip = 1
            Case "4C"
                '32-bit If Equal To 2-lines
                Dim tmp(3) As Byte
                ReadProcessMemory(p.Handle, addr, tmp, 4, 0)
                Dim readValue As UInteger = CUInt(BitConverter.ToUInt32(tmp, 0))
                If readValue = CUInt("&H" + tvalue) Then
                    Exit Sub
                End If
                skip = 2
            Case "8C"
                '32-bit If Equal To multi-lines (until the z20 code type or end of code list are executed)
                Dim tmp(3) As Byte
                ReadProcessMemory(p.Handle, addr, tmp, 4, 0)
                Dim readValue As UInteger = CUInt(BitConverter.ToUInt32(tmp, 0))
                If readValue = CUInt("&H" + tvalue) Then
                    Exit Sub
                End If
                skip = 65535
            Case "CC"
                '32-bit If Equal To all-lines
                Dim tmp(3) As Byte
                ReadProcessMemory(p.Handle, addr, tmp, 4, 0)
                Dim readValue As UInteger = CUInt(BitConverter.ToUInt32(tmp, 0))
                If readValue = CUInt("&H" + tvalue) Then
                    Exit Sub
                End If
                skip = 65535

            Case "10"
                '8-bit If NOT Equal To 1-line
                Dim tmp(3) As Byte
                ReadProcessMemory(p.Handle, addr, tmp, 1, 0)
                Dim readValue As UInteger = CUInt(BitConverter.ToUInt32(tmp, 0))
                If readValue <> CUInt("&H" + tvalue.Substring(6)) Then
                    Exit Sub
                End If
                skip = 1
            Case "50"
                '8-bit If NOT Equal To 2-lines
                Dim tmp(3) As Byte
                ReadProcessMemory(p.Handle, addr, tmp, 1, 0)
                Dim readValue As UInteger = CUInt(BitConverter.ToUInt32(tmp, 0))
                If readValue <> CUInt("&H" + tvalue.Substring(6)) Then
                    Exit Sub
                End If
                skip = 2
            Case "90"
                '8-bit If NOT Equal To multi-lines (until the z20 code type or end of code list are executed)
                Dim tmp(3) As Byte
                ReadProcessMemory(p.Handle, addr, tmp, 1, 0)
                Dim readValue As UInteger = CUInt(BitConverter.ToUInt32(tmp, 0))
                If readValue <> CUInt("&H" + tvalue.Substring(6)) Then
                    Exit Sub
                End If
                skip = 65535
            Case "D0"
                '8-bit If NOT Equal To all-lines
                Dim tmp(3) As Byte
                ReadProcessMemory(p.Handle, addr, tmp, 1, 0)
                Dim readValue As UInteger = CUInt(BitConverter.ToUInt32(tmp, 0))
                If readValue <> CUInt("&H" + tvalue.Substring(6)) Then
                    Exit Sub
                End If
                skip = 65535

            Case "12"
                '16-bit If NOT Equal To 1-line
                Dim tmp(3) As Byte
                ReadProcessMemory(p.Handle, addr, tmp, 2, 0)
                Dim readValue As UInteger = CUInt(BitConverter.ToUInt32(tmp, 0))
                If readValue <> CUInt("&H" + tvalue.Substring(4)) Then
                    Exit Sub
                End If
                skip = 1
            Case "52"
                '16-bit If NOT Equal To 2-lines
                Dim tmp(3) As Byte
                ReadProcessMemory(p.Handle, addr, tmp, 2, 0)
                Dim readValue As UInteger = CUInt(BitConverter.ToUInt32(tmp, 0))
                If readValue <> CUInt("&H" + tvalue.Substring(4)) Then
                    Exit Sub
                End If
                skip = 2
            Case "92"
                '16-bit If NOT Equal To multi-lines (until the z20 code type or end of code list are executed)
                Dim tmp(3) As Byte
                ReadProcessMemory(p.Handle, addr, tmp, 2, 0)
                Dim readValue As UInteger = CUInt(BitConverter.ToUInt32(tmp, 0))
                If readValue <> CUInt("&H" + tvalue.Substring(4)) Then
                    Exit Sub
                End If
                skip = 65535
            Case "D2"
                '16-bit If NOT Equal To all-lines
                Dim tmp(3) As Byte
                ReadProcessMemory(p.Handle, addr, tmp, 2, 0)
                Dim readValue As UInteger = CUInt(BitConverter.ToUInt32(tmp, 0))
                If readValue <> CUInt("&H" + tvalue.Substring(4)) Then
                    Exit Sub
                End If
                skip = 65535

            Case "14"
                '32-bit If NOT Equal To 1-line
                Dim tmp(3) As Byte
                ReadProcessMemory(p.Handle, addr, tmp, 4, 0)
                Dim readValue As UInteger = CUInt(BitConverter.ToUInt32(tmp, 0))
                If readValue <> CUInt("&H" + tvalue) Then
                    Exit Sub
                End If
                skip = 1
            Case "54"
                '32-bit If NOT Equal To 2-lines
                Dim tmp(3) As Byte
                ReadProcessMemory(p.Handle, addr, tmp, 4, 0)
                Dim readValue As UInteger = CUInt(BitConverter.ToUInt32(tmp, 0))
                If readValue <> CUInt("&H" + tvalue) Then
                    Exit Sub
                End If
                skip = 2
            Case "94"
                '32-bit If NOT Equal To multi-lines (until the z20 code type or end of code list are executed)
                Dim tmp(3) As Byte
                ReadProcessMemory(p.Handle, addr, tmp, 4, 0)
                Dim readValue As UInteger = CUInt(BitConverter.ToUInt32(tmp, 0))
                If readValue <> CUInt("&H" + tvalue) Then
                    Exit Sub
                End If
                skip = 65535
            Case "D4"
                '32-bit If NOT Equal To all-lines
                Dim tmp(3) As Byte
                ReadProcessMemory(p.Handle, addr, tmp, 4, 0)
                Dim readValue As UInteger = CUInt(BitConverter.ToUInt32(tmp, 0))
                If readValue <> CUInt("&H" + tvalue) Then
                    Exit Sub
                End If
                skip = 65535

            Case "18"
                '8-bit If LESS than (signed) To 1-line
            Case "58"
                '8-bit If LESS than (signed) To 2-lines
            Case "98"
                '8-bit If LESS than (signed) To multi-lines (until the z20 code type or end of code list are executed)
            Case "D8"
                '8-bit If LESS than (signed) To all-lines

            Case "1A"
                '16-bit If LESS than (signed) To 1-line
            Case "5A"
                '16-bit If LESS than (signed) To 2-lines
            Case "9A"
                '16-bit If LESS than (signed) To multi-lines (until the z20 code type or end of code list are executed)
            Case "DA"
                '16-bit If LESS than (signed) To all-lines

            Case "1C"
                '32-bit If LESS than (signed) To 1-line
            Case "5C"
                '32-bit If LESS than (signed) To 2-lines
            Case "9C"
                '32-bit If LESS than (signed) To multi-lines (until the z20 code type or end of code list are executed)
            Case "DC"
                '32-bit If LESS than (signed) To all-lines

            Case "20"
                '8-bit If GREATER than (signed) To 1-line
            Case "60"
                '8-bit If GREATER than (signed) To 2-lines
            Case "A0"
                '8-bit If GREATER than (signed) To multi-lines (until the z20 code type or end of code list are executed)
            Case "E0"
                '8-bit If GREATER than (signed) To all-lines

            Case "21"
                '16-bit If GREATER than (signed) To 1-line
            Case "61"
                '16-bit If GREATER than (signed) To 2-lines
            Case "A1"
                '16-bit If GREATER than (signed) To multi-lines (until the z20 code type or end of code list are executed)
            Case "E1"
                '16-bit If GREATER than (signed) To all-lines

            Case "22"
                '32-bit If GREATER than (signed) To 1-line
            Case "62"
                '32-bit If GREATER than (signed) To 2-lines
            Case "A2"
                '32-bit If GREATER than (signed) To multi-lines (until the z20 code type or end of code list are executed)
            Case "E2"
                '32-bit If GREATER than (signed) To all-lines

            Case "28"
                '8-bit If LESS than (UNsigned) To 1-line
            Case "68"
                '8-bit If LESS than (UNsigned) To 2-lines
            Case "A8"
                '8-bit If LESS than (UNsigned) To multi-lines (until the z20 code type or end of code list are executed)
            Case "E8"
                '8-bit If LESS than (UNsigned) To all-lines

            Case "2A"
                '16-bit If LESS than (UNsigned) To 1-line
            Case "6A"
                '16-bit If LESS than (UNsigned) To 2-lines
            Case "AA"
                '16-bit If LESS than (UNsigned) To multi-lines (until the z20 code type or end of code list are executed)
            Case "EA"
                '16-bit If LESS than (UNsigned) To all-lines

            Case "2C"
                '32-bit If LESS than (UNsigned) To 1-line
            Case "6C"
                '32-bit If LESS than (UNsigned) To 2-lines
            Case "AC"
                '32-bit If LESS than (UNsigned) To multi-lines (until the z20 code type or end of code list are executed)
            Case "EC"
                '32-bit If LESS than (UNsigned) To all-lines

            Case "30"
                '8-bit If GRATER than (UNsigned) To 1-line
            Case "70"
                '8-bit If GRATER than (UNsigned) To 2-lines
            Case "B0"
                '8-bit If GRATER than (UNsigned) To multi-lines (until the z20 code type or end of code list are executed)
            Case "F0"
                '8-bit If GRATER than (UNsigned) To all-lines

            Case "32"
                '16-bit If GRATER than (UNsigned) To 1-line
            Case "72"
                '16-bit If GRATER than (UNsigned) To 2-lines
            Case "B2"
                '16-bit If GRATER than (UNsigned) To multi-lines (until the z20 code type or end of code list are executed)
            Case "F2"
                '16-bit If GRATER than (UNsigned) To all-lines

            Case "34"
                '32-bit If GRATER than (UNsigned) To 1-line
            Case "74"
                '32-bit If GRATER than (UNsigned) To 2-lines
            Case "B4"
                '32-bit If GRATER than (UNsigned) To multi-lines (until the z20 code type or end of code list are executed)
            Case "F4"
                '32-bit If GRATER than (UNsigned) To all-lines

            Case "38"
                '8-bit If AND 1-line
                Dim tmp(3) As Byte
                ReadProcessMemory(p.Handle, addr, tmp, 1, 0)
                Dim readValue As UInteger = CUInt(BitConverter.ToUInt32(tmp, 0))
                If (readValue And CUInt("&H" + tvalue.Substring(6))) <> 0 Then
                    Exit Sub
                End If
                skip = 1
            Case "78"
                '8-bit If AND 2-lines
                Dim tmp(3) As Byte
                ReadProcessMemory(p.Handle, addr, tmp, 1, 0)
                Dim readValue As UInteger = CUInt(BitConverter.ToUInt32(tmp, 0))
                If (readValue And CUInt("&H" + tvalue.Substring(6))) <> 0 Then
                    Exit Sub
                End If
                skip = 2
            Case "B8"
                '8-bit If AND multi-lines (until the z20 code type or end of code list are executed)
                Dim tmp(3) As Byte
                ReadProcessMemory(p.Handle, addr, tmp, 1, 0)
                Dim readValue As UInteger = CUInt(BitConverter.ToUInt32(tmp, 0))
                If (readValue And CUInt("&H" + tvalue.Substring(6))) <> 0 Then
                    Exit Sub
                End If
                skip = 65535
            Case "F8"
                '8-bit If AND all-lines
                Dim tmp(3) As Byte
                ReadProcessMemory(p.Handle, addr, tmp, 1, 0)
                Dim readValue As UInteger = CUInt(BitConverter.ToUInt32(tmp, 0))
                If (readValue And CUInt("&H" + tvalue.Substring(6))) <> 0 Then
                    Exit Sub
                End If
                skip = 65535

            Case "39"
                '16-bit If AND 1-line
                Dim tmp(3) As Byte
                ReadProcessMemory(p.Handle, addr, tmp, 2, 0)
                Dim readValue As UInteger = CUInt(BitConverter.ToUInt32(tmp, 0))
                If (readValue And CUInt("&H" + tvalue.Substring(4))) <> 0 Then
                    Exit Sub
                End If
                skip = 1
            Case "79"
                '16-bit If AND 2-lines
                Dim tmp(3) As Byte
                ReadProcessMemory(p.Handle, addr, tmp, 2, 0)
                Dim readValue As UInteger = CUInt(BitConverter.ToUInt32(tmp, 0))
                If (readValue And CUInt("&H" + tvalue.Substring(4))) <> 0 Then
                    Exit Sub
                End If
                skip = 2
            Case "B9"
                '16-bit If AND multi-lines (until the z20 code type or end of code list are executed)
                Dim tmp(3) As Byte
                ReadProcessMemory(p.Handle, addr, tmp, 2, 0)
                Dim readValue As UInteger = CUInt(BitConverter.ToUInt32(tmp, 0))
                If (readValue And CUInt("&H" + tvalue.Substring(4))) <> 0 Then
                    Exit Sub
                End If
                skip = 65535
            Case "F9"
                '16-bit If AND all-lines
                Dim tmp(3) As Byte
                ReadProcessMemory(p.Handle, addr, tmp, 2, 0)
                Dim readValue As UInteger = CUInt(BitConverter.ToUInt32(tmp, 0))
                If (readValue And CUInt("&H" + tvalue.Substring(4))) <> 0 Then
                    Exit Sub
                End If
                skip = 65535

            Case "3A"
                '32-bit If AND 1-line
                Dim tmp(3) As Byte
                ReadProcessMemory(p.Handle, addr, tmp, 4, 0)
                Dim readValue As UInteger = CUInt(BitConverter.ToUInt32(tmp, 0))
                If (readValue And CUInt("&H" + tvalue)) <> 0 Then
                    Exit Sub
                End If
                skip = 1
            Case "7A"
                '32-bit If AND 2-lines
                Dim tmp(3) As Byte
                ReadProcessMemory(p.Handle, addr, tmp, 4, 0)
                Dim readValue As UInteger = CUInt(BitConverter.ToUInt32(tmp, 0))
                If (readValue And CUInt("&H" + tvalue)) <> 0 Then
                    Exit Sub
                End If
                skip = 2
            Case "BA"
                '32-bit If AND multi-lines (until the z20 code type or end of code list are executed)
                Dim tmp(3) As Byte
                ReadProcessMemory(p.Handle, addr, tmp, 4, 0)
                Dim readValue As UInteger = CUInt(BitConverter.ToUInt32(tmp, 0))
                If (readValue And CUInt("&H" + tvalue)) <> 0 Then
                    Exit Sub
                End If
                skip = 65535
            Case "FA"
                '32-bit If AND all-lines
                Dim tmp(3) As Byte
                ReadProcessMemory(p.Handle, addr, tmp, 4, 0)
                Dim readValue As UInteger = CUInt(BitConverter.ToUInt32(tmp, 0))
                If (readValue And CUInt("&H" + tvalue)) <> 0 Then
                    Exit Sub
                End If
                skip = 65535

            Case "0E"
                'Always skip next line
                skip = 1
            Case "4E"
                'Always skip next 2-lines
                skip = 2
            Case "8E"
                'Always skip remainig lines
                skip = 65535
            Case "CE"
                'Always skip all codes
                skip = 65535

        End Select
    End Sub
End Class

Public Class CustomFunctions
    Public Shared p As Process
    <DllImport("kernel32.dll", SetLastError:=True)> _
    Private Shared Function ReadProcessMemory( _
       ByVal hProcess As IntPtr, _
       ByVal lpBaseAddress As IntPtr, _
       <Out()> ByVal lpBuffer() As Byte, _
       ByVal dwSize As Integer, _
       ByRef lpNumberOfBytesRead As Integer) As Boolean
    End Function

    Function canReadPointer(a As String) As Boolean
        Dim tmp(3) As Byte
        ReadProcessMemory(p.Handle, New IntPtr(CLng("&H" + a)), tmp, tmp.Length, 0)
        Return ReadProcessMemory(p.Handle, New IntPtr(BitConverter.ToInt32(tmp, 0)), tmp, tmp.Length, 0)
    End Function

    Function addr(a As String) As Integer
        Dim tmp(3) As Byte
        ReadProcessMemory(p.Handle, New IntPtr(CLng("&H" + a)), tmp, tmp.Length, 0)
        Return CInt(BitConverter.ToUInt32(tmp, 0))
    End Function

    Function fromAddr(a As String) As IntPtr
        Return New IntPtr(CLng("&H" + a))
    End Function

    Overloads Function fromPointer(a As String) As IntPtr
        Dim tmp(3) As Byte
        ReadProcessMemory(p.Handle, New IntPtr(CLng("&H" + a)), tmp, tmp.Length, 0)
        Return New IntPtr(BitConverter.ToUInt32(tmp, 0))
    End Function

    Overloads Function fromPointer(a As IntPtr) As IntPtr
        Dim tmp(3) As Byte
        ReadProcessMemory(p.Handle, a, tmp, tmp.Length, 0)
        Return New IntPtr(BitConverter.ToUInt32(tmp, 0))
    End Function

    Overloads Function fromPointer32bit(a As String) As IntPtr
        Dim tmp(7) As Byte
        ReadProcessMemory(p.Handle, New IntPtr(CLng("&H" + a)), tmp, tmp.Length, 0)
        Return New IntPtr(BitConverter.ToInt64(tmp, 0))
    End Function

    Overloads Function fromPointer32bit(a As IntPtr) As IntPtr
        Dim tmp(7) As Byte
        ReadProcessMemory(p.Handle, a, tmp, tmp.Length, 0)
        Return New IntPtr(BitConverter.ToInt64(tmp, 0))
    End Function

    Overloads Function HexAdd(i As IntPtr, s As String) As IntPtr
        Return New IntPtr(i.ToInt64 + CLng("&h" + s))
    End Function

    Overloads Function HexAdd(s1 As String, s2 As String) As IntPtr
        Return New IntPtr(CLng("&H" + s1) + CLng("&H" + s2))
    End Function

    Function HexSubstract(i As IntPtr, s As String) As IntPtr
        Return New IntPtr(i.ToInt32 - CLng("&h" + s))
    End Function

    Overloads Function readFromTo(from As String, to_ As String) As String
        Dim s As String = ""
        Dim addr As IntPtr = New IntPtr(CLng("&H" + from))
        Dim tmp(0) As Byte
        Do While True
            ReadProcessMemory(p.Handle, addr, tmp, tmp.Length, 0)
            s = s + System.Text.Encoding.ASCII.GetString(tmp)
            If tmp(0) = 0 Then Return s.Trim.Replace(Chr(0), "")
            If s.EndsWith("\") Then s = ""
            If s.EndsWith(to_) Then Return s.Trim.Replace(Chr(0), "")
            addr += 1
        Loop
        Return s.Trim
    End Function

    Overloads Function readFromTo(addr As IntPtr, to_ As String) As String
        Dim s As String = ""
        Dim tmp(0) As Byte
        Do While True
            ReadProcessMemory(p.Handle, addr, tmp, tmp.Length, 0)
            s = s + System.Text.Encoding.ASCII.GetString(tmp)
            If tmp(0) = 0 Then Return s.Trim.Replace(Chr(0), "")
            If s.EndsWith("\") Then s = ""
            If s.EndsWith(to_) Then Return s.Trim.Replace(Chr(0), "")
            addr += 1
        Loop
        Return s.Trim
    End Function

    Overloads Function readNByteFrom(from As String, n As Integer) As String
        Dim s As String = ""
        Dim tmp(0) As Byte
        Dim addr As IntPtr = New IntPtr(CLng("&H" + from))
        For i As Integer = 0 To n - 1
            ReadProcessMemory(p.Handle, addr, tmp, tmp.Length, 0)
            s = s + System.Text.Encoding.ASCII.GetString(tmp)
            addr += 1
        Next
        Return s
    End Function

    Overloads Function readNByteFrom(from As IntPtr, n As Integer) As String
        Dim s As String = ""
        Dim tmp(0) As Byte
        Dim addr As IntPtr = from
        For i As Integer = 0 To n - 1
            ReadProcessMemory(p.Handle, addr, tmp, tmp.Length, 0)
            s = s + System.Text.Encoding.ASCII.GetString(tmp)
            addr += 1
        Next
        Return s
    End Function

    Function readFromPointerTo(from As String, to_ As String) As String
        Dim tmp(0) As Byte
        Dim tmp2(3) As Byte
        Dim s As String = ""

        ReadProcessMemory(p.Handle, New IntPtr(CLng("&H" + from)), tmp2, tmp2.Length, 0)
        Dim addr As IntPtr = New IntPtr(BitConverter.ToInt32(tmp2, 0))

        Do While True
            ReadProcessMemory(p.Handle, addr, tmp, tmp.Length, 0)
            s = s + System.Text.Encoding.ASCII.GetString(tmp)
            If tmp(0) = 0 Then Return s.Trim.Replace(Chr(0), "")
            If s.EndsWith("\") Then s = ""
            If s.EndsWith(to_) Then Return s.Trim.Replace(Chr(0), "")
            addr += 1
        Loop
        Return s.Trim
    End Function

    Function BaseAddress(a As String) As String
        Dim t As IntPtr = New IntPtr(p.MainModule.BaseAddress.ToInt64 + CLng("&H" + a))
        Return Hex(t.ToInt64)
    End Function

    Function BaseAddressOf(moduleName As String, a As String) As String
        Dim c As Integer = 0
        Dim QueryModules As New List(Of String)
        Dim regions As List(Of Long()) = WinAPI.getRegionsList(p.Handle, QueryModules)
        Dim addr As IntPtr = IntPtr.Zero
        For Each item As Long() In regions
            If QueryModules(c).Trim.ToLower = moduleName.Trim.ToLower Then addr = New IntPtr(item(0)) : Exit For
            c += 1
        Next
        Dim t As IntPtr = New IntPtr(addr.ToInt64 + CLng("&H" + a))
        Return Hex(t.ToInt64)
    End Function

    Function findBlockBySize(blockSize As String, offset As String) As IntPtr
        Dim blockSizeUlong As ULong = CULng("&H" + blockSize)
        Dim QueryModules As New List(Of String)
        Dim regions As List(Of Long()) = WinAPI.getRegionsList(p.Handle, QueryModules)
        Dim addr As IntPtr = IntPtr.Zero
        For Each item As Long() In regions
            If item(2) = blockSizeUlong Then addr = New IntPtr(item(0)) : Exit For
        Next
        Return New IntPtr(addr.ToInt64 + CLng("&H" + offset))
    End Function

    Function replace(a As String, b As String, c As String) As String
        Return a.Replace(b, c)
    End Function

    Function insert(a As String, b As String, n As Integer) As String
        Return a.Substring(0, n) + b + a.Substring(n)
    End Function

    Public Function titleContains(s As String) As Boolean
        Return p.MainWindowTitle.Contains(s)
    End Function

    Public Overloads Function titleSubstring(n1 As Int32) As String
        Return p.MainWindowTitle.Substring(n1)
    End Function

    Public Overloads Function titleSubstring(n1 As Int32, n2 As Int32) As String
        Return p.MainWindowTitle.Substring(n1, n2)
    End Function

    Public Overloads Function titleIndexOf(s As String) As Integer
        Return p.MainWindowTitle.IndexOf(s)
    End Function

    Public Overloads Function titleIndexOf(c As Char) As Integer
        Return p.MainWindowTitle.IndexOf(c)
    End Function

    Public Overloads Function titleLastIndexOf(s As String) As Integer
        Return p.MainWindowTitle.LastIndexOf(s)
    End Function

    Public Function removeExtension(s As String) As String
        If Not s.Contains(".") Then Return s
        Return s.Substring(0, s.LastIndexOf("."))
    End Function
End Class