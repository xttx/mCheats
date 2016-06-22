Imports System.Runtime.InteropServices
Public Class Form1_EmulatorConfigWizard
    Dim p As Process
    Dim p_is64bit As Boolean = False
    Dim tmp() As Byte
    Dim addrs(0) As Boolean
    Dim mapped_files As List(Of mapped_File)
    Dim mapped_files_ranges As List(Of mapped_File_range)
    Dim addrs_step5(0) As Boolean
    Dim addrs_step6() As Long
    Dim addrs_step6_offset_in_bytes As List(Of Byte())
    Dim curStep As Integer = 1
    Dim curSystem As String
    Dim processCrc As String = ""
    Dim additional_action_value As UInteger = 0
    Dim updatingEmu As Boolean = False
    Dim theOnlyPredefinedSystem As String = ""
    Dim quickSaveIndexes As New List(Of Integer)
    Dim updatingEmulatorVersionNode As Xml.XmlNode = Nothing
    Dim romAddrExistingSettings(2) As String
    Dim zeroAddrExistingSettings(5) As String
    Dim WithEvents step4_timer As New Timer With {.Enabled = False, .Interval = 1000}
    Dim WithEvents step5_timer As New Timer With {.Enabled = False, .Interval = 1000}
    Dim WithEvents step51_timer As New Timer With {.Enabled = False, .Interval = 2000}
    Dim WithEvents step6_timer As New Timer With {.Enabled = False, .Interval = 3000}
    Dim trd As Threading.Thread
    Dim m As Class5_MemoryAccess
    Dim setRam() As ULong = {0, 0, 0, 0}
    Structure mapped_File
        Dim addrStart As ULong
        Dim length As ULong
        Dim fileName As String
    End Structure
    Structure mapped_File_range
        Dim addrStart As ULong
        Dim addrEnd As ULong
        Dim fileName As String
    End Structure
    Structure returndata
        Dim retInt() As Integer
        Dim retLong As Long()
        Dim retType As Integer
    End Structure

    'TODO quicksave changes does not reflected in preview
    'TODO проверить все хелпы
    'TODO better find-window-class tool. Need interface refactoring, prevent listbox scroll when hit TEST, test for ALL available windows, ability to bring selected class to main textbox
    'TODO when updating existing emulator fill _DETECT SYSTEM_, romaddress and zero-address tables with one row based on current settings
    'TODO when testing codes at last step, menu apears with "TESTING" label instead of cheat listing, but when testing-cheats-textbox is empty, the menu still have only "TESTING" label.
    'TODO как-то проверять тестовые читы, при некоторых вариантах, в мейн лупе субстринг outOfRange (если чит меньше необходимой длинны, например)
    'TODO вставить в паузу возможность шифт/альт/кнтрл + кнопка.
    'TODO не забыть про RECHECK, в 0аддр - он не принимает в счет поиск по уникальному размеру блока.
    'TODO при возврате на шаг назад, не включается рефреш-таймер в таблицах.
    'TODO after closing, form continue to do something
    'TODO update config from textbox, to allow lastminute changes in step 7 - UpdateConfig()
    'TODO просматривать адреса по желанию (любые, а не только найденные)
    'TODO search isPaused flag
    'TODO c++ x32 dll, function for pointer search
    'TODO pointer search sub -- win x32 implimentation
    'TODO searching pointer with negative values
    'TODO pointer offset calculating on 64bit processes
    'TODO возможность изменить drawMode даже если эмулятор уже есть в конфиге
    'TOCHECK В процессе вычисления выражения для захвата romName, поменял создающееся fromPointer32bit на fromPointer. Что-то починил, но возможно что то и сломал.
    'FIXED impliment changing draw-method
    'FIXED При выхода из проги при открытой форме визарда конфига эмулей - крэш
    'FIXED при инициализации этапа 7, в классе cheatsLoad загружаются пустые читы. Если к этому моменту были активированы реальные читы (придти на шаг 7, включить реальный чит, вернутся на шаг назад и вернутся на шаг 7), будет ошибка, т.к. будут активированы читы которых нет.
    'FIXED RELATED TO PREVIOUS возникают проблемы при использовании тестовых читов, и последующем отображении реального чит-меню.
    'FIXED записывать поинтеры для найденых ромнеймов
    'FIXED выделять заполненные пресеты при инициализации
    'DONE Сохранять и восстанавливать из пресетов не только таблицу, но и настройки
    'DONE тестовые читы на последнем шаге
    'DONE дополнительные действия для найденных адресов (add, substract)
    'DONE search two memories in NES and where it is needed
    'DONE не ищет перевернутые адреса (base addr)
    'DONE search emulated system flag
    'DONE add code to adding new emu node in step 7 - UpdateConfig()
    'SEEMS TO BE FIXED pointers are erased while updating even if value is ok
    'SEEMS TO BE FIXED кажется сломалось определение относительно модулей, в маме не работало. Может потому что х64
    Private Sub Form1_EmulatorConfigWizard_Load(sender As System.Object, e As System.EventArgs) Handles MyBase.Load
        GroupBox2.Parent = Me
        PictureBox1.ImageLocation = ".\loader3.gif"
        'Step7_SelectDrawMethod.SelectedIndex = 0

        'fill quicksave schemas
        UpdateQuickSaveSchema = False
        For Each QuickStatesConfigNode As Xml.XmlNode In xmlConfig.SelectSingleNode("/config/quicksaves").ChildNodes
            Dim helpNode As Xml.XmlNode = QuickStatesConfigNode.SelectSingleNode("help")
            If Not helpNode Is Nothing Then
                Dim index As Integer = CInt(QuickStatesConfigNode.Name.ToUpper.Replace("CONFIG", ""))
                Step7_quickSaveSchema.Items.Add(index.ToString + ". " + helpNode.InnerText)
                quickSaveIndexes.Add(index)
            End If
        Next
        If Step7_quickSaveSchema.Items.Count > 0 Then Step7_quickSaveSchema.SelectedIndex = 0
        UpdateQuickSaveSchema = True

        For i As Integer = TabControl1.TabCount - 1 To 1 Step -1
            TabControl1.TabPages.RemoveAt(i)
        Next
        Button1.Enabled = False
        Step6_AdditionalActionsComboBox.SelectedIndex = 0
        Step1_init()
    End Sub

    Private Sub Form1_EmulatorConfigWizard_FormClosing(sender As Object, e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        step5_timer.Enabled = False
        step51_timer.Enabled = False
        step6_timer.Enabled = False
        xmlConfig.Load(".\config.xml")
        Me.Dispose(True)
    End Sub

#Region "Step1 - Show Process List"
    'Fill process list
    Private Sub Step1_init()
        Dim ps() As Process
        Step1_ListBox1.Items.Clear()
        ps = Process.GetProcesses()
        For Each curp As Process In ps
            If curp.MainWindowTitle <> "" And curp.MainWindowTitle <> Me.Text Then
                Step1_ListBox1.Items.Add(curp.ProcessName + " [" + curp.MainWindowTitle + "]")
            End If
        Next
    End Sub

    'Refresh click
    Private Sub Step1_refresh_Click(sender As System.Object, e As System.EventArgs) Handles Step1_refresh.Click
        Step1_init()
    End Sub

    'Process dbl-click
    Private Sub Step1_ListBox1_DoubleClick(sender As Object, e As System.EventArgs) Handles Step1_ListBox1.DoubleClick
        Button2_Click(Step1_ListBox1, New System.EventArgs)
    End Sub

    'Selected process change
    Private Sub Step1_ListBox1_SelectedValueChanged(sender As Object, e As System.EventArgs) Handles Step1_ListBox1.SelectedValueChanged
        Step3_TextBox3.Text = ""
    End Sub
#End Region

#Region "Step2 - TabPage2 - Unneeded"
    Private Sub Step2_RadioButton_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles Step2_RadioButton1.CheckedChanged, Step2_RadioButton2.CheckedChanged
        If Step2_RadioButton1.Checked Then
            Step2_TextBox1.Enabled = True
        Else
            Step2_TextBox1.Enabled = False
        End If
    End Sub
#End Region

#Region "Step3 - Emulator options / choose system"
    'Fill process name and crc
    Private Sub step3_init()
        Step3_TextBox1.Text = p.ProcessName
        Step3_TextBox2.Text = processCrc
    End Sub

    'Radio Option changed
    Private Sub Step3_RadioButton_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles Step3_RadioButton1.CheckedChanged, Step3_RadioButton2.CheckedChanged, Step3_RadioButton3.CheckedChanged
        If Step3_RadioButton2.Checked Then Step51_TextBox1.Enabled = True Else Step51_TextBox1.Enabled = False
    End Sub

    'Capture pause key
    Private Sub Step3_TextBox4_KeyDown(sender As Object, e As System.Windows.Forms.KeyEventArgs) Handles Step3_TextBox4.KeyDown
        Step3_TextBox4.Text = [Enum].GetName(e.KeyCode.GetType, e.KeyCode)
    End Sub

    'Change system
    Private Sub Step3_ComboBox1_SelectedIndexChanged(sender As System.Object, e As System.EventArgs) Handles Step3_ComboBox1.SelectedIndexChanged
        Dim old_curSystem = curSystem
        Select Case Step3_ComboBox1.SelectedIndex
            Case 0
                curSystem = "GB"
            Case 1
                curSystem = "GBC"
            Case 2
                curSystem = "GBA"
            Case 3
                curSystem = "NES"
            Case 4
                curSystem = "SNES"
            Case 5
                curSystem = "N64"
            Case 6
                curSystem = "NGC"
            Case 7
                curSystem = "GG"
            Case 8
                curSystem = "SMS"
            Case 9
                curSystem = "SMD"
            Case 10
                curSystem = "32X"
            Case 11
                curSystem = "SCD"
            Case 12
                curSystem = "PSX"
            Case 13
                curSystem = "PS2"
        End Select

        If old_curSystem <> curSystem Then
            Step4_DataGridView1.Rows.Clear()
            Step5_DataGridView3.Rows.Clear()
            Step6_DataGridView1.Rows.Clear()
            setRam = {0, 0, 0, 0}
            Step6_AdditionalActionsTextBox.Text = ""
            Step6_AdditionalActionsComboBox.SelectedIndex = 0
            Step6_setRam1.Tag = "" : Step6_setRam2.Tag = ""
            Step6_setRam1.Text = "SET RAM 1" : Step6_setRam2.Text = "SET RAM 2"
        End If

        'in NES ram1 = 800+, ram2 = 6000+
        'in GBA ram1 = 03000000-03007FFF(On-chip work ram), ram2 = 02000000-0203FFFF(On-board work ram)
        'in SAT ram1 = 06000000+ ram high, ram2 = 200000-300000 ram low
        If curSystem = "NES" Or curSystem = "GBA" Or curSystem = "SAT" Then
            Step6_setRam1.Visible = True : Step6_setRam2.Visible = True
        Else
            Step6_setRam1.Visible = False : Step6_setRam2.Visible = False
        End If

        If theOnlyPredefinedSystem <> "" And theOnlyPredefinedSystem <> curSystem Then
            If Step3_RadioButton1.Checked Then Step3_RadioButton3.Checked = True
            Step3_RadioButton1.Enabled = False
        Else
            Step3_RadioButton1.Enabled = True
        End If
        If theOnlyPredefinedSystem = curSystem Then Step3_RadioButton1.Checked = True
    End Sub
#End Region

#Region "Step4 - Determin system by address"
    'Fill preset buttons, start timer
    Private Sub step4_init()
        For i As Integer = 1 To 7
            If FileIO.FileSystem.FileExists(".\EmulatorConfigPresets\SysAddr" + processCrc.ToUpper.Trim + "_0" + i.ToString + ".txt") Then
                Dim b As Button = CType(Me.Controls("TabControl1").Controls("TabPage4").Controls("Step4_ButtonPreset0" + i.ToString), Button)
                Dim newBold As New Font(b.Font.FontFamily, b.Font.Size, FontStyle.Bold)
                b.ForeColor = Color.Green : b.Font = newBold
            End If
        Next

        ReDim addrs(0)
        step4_timer.Enabled = True
    End Sub

    'Currently playing THIS system click
    Private Sub Step4_Button1_Click(sender As System.Object, e As System.EventArgs) Handles Step4_Button1.Click
        If p.HasExited Then WaitForProcess()
        Step4_DataGridView1.Rows.Clear()
        Dim out As Integer
        Dim counter As Integer = 0
        Dim addrFrom As IntPtr = p.MainModule.BaseAddress
        Dim addrTo As IntPtr = p.MainModule.BaseAddress + p.MainModule.ModuleMemorySize
        ReDim tmp(CInt(addrTo.ToInt64 - addrFrom.ToInt64))
        WinAPI.ReadProcessMemory(p.Handle, addrFrom, tmp, tmp.Length, out)

        Dim addrCur As Integer
        ProgressBar2.Maximum = tmp.Length - 1
        If addrs.Count = 1 Then
            ReDim addrs(CInt(addrTo.ToInt64 - addrFrom.ToInt64))
            For addrCur = 0 To tmp.Length - 1
                If tmp(addrCur) <> 0 Then addrs(addrCur) = True : counter += 1 Else addrs(addrCur) = False
                If addrCur Mod 10 = 0 Then ProgressBar2.Value = addrCur
                If Step4_DataGridView1.Rows.Count < 700 Then Step4_DataGridView1.Rows.Add({Hex(addrCur), Hex(tmp(addrCur))})
            Next
        Else
            For addrCur = 0 To tmp.Length - 1
                If tmp(addrCur) = 0 Then addrs(addrCur) = False
                If addrCur Mod 10 = 0 Then ProgressBar2.Value = addrCur
                If addrs(addrCur) Then
                    counter += 1
                    If Step4_DataGridView1.Rows.Count < 700 Then Step4_DataGridView1.Rows.Add({Hex(addrCur), Hex(tmp(addrCur))})
                End If
            Next
        End If
        ProgressBar2.Value = 0
        Label13.Text = "Currently found: " + counter.ToString
    End Sub

    'Currently playing OTHER system click
    Private Sub Step4_Button2_Click(sender As System.Object, e As System.EventArgs) Handles Step4_Button2.Click
        If p.HasExited Then WaitForProcess()
        Step4_DataGridView1.Rows.Clear()
        Dim out As Integer
        Dim counter As Integer
        Dim addrFrom As IntPtr = p.MainModule.BaseAddress
        Dim addrTo As IntPtr = p.MainModule.BaseAddress + p.MainModule.ModuleMemorySize
        ReDim tmp(CInt(addrTo.ToInt64 - addrFrom.ToInt64))
        WinAPI.ReadProcessMemory(p.Handle, addrFrom, tmp, tmp.Length, out)

        Dim addrCur As Integer
        ProgressBar2.Maximum = tmp.Length - 1
        If addrs.Count = 1 Then
            ReDim addrs(CInt(addrTo.ToInt64 - addrFrom.ToInt64))
            For addrCur = 0 To tmp.Length - 1
                If tmp(addrCur) = 0 Then addrs(addrCur) = True : counter += 1 Else addrs(addrCur) = False
                If addrCur Mod 10 = 0 Then ProgressBar2.Value = addrCur
                If Step4_DataGridView1.Rows.Count < 700 Then Step4_DataGridView1.Rows.Add({Hex(addrCur), Hex(tmp(addrCur))})
            Next
        Else
            For addrCur = 0 To tmp.Length - 1
                If tmp(addrCur) <> 0 Then addrs(addrCur) = False
                If addrCur Mod 10 = 0 Then ProgressBar2.Value = addrCur
                If addrs(addrCur) Then
                    counter += 1
                    If Step4_DataGridView1.Rows.Count < 700 Then Step4_DataGridView1.Rows.Add({Hex(addrCur), Hex(tmp(addrCur))})
                End If
            Next
        End If
        ProgressBar2.Value = 0
        Label13.Text = "Currently found: " + counter.ToString
    End Sub

    'Timer tick - refresh table
    Private Sub step4_timer4() Handles step4_timer.Tick
        If p.HasExited Then WaitForProcess()
        ReDim tmp(0)
        Dim out As Integer
        For Each r As DataGridViewRow In Step4_DataGridView1.Rows
            Dim addr As IntPtr = p.MainModule.BaseAddress + CInt("&H" + r.Cells(0).Value.ToString)
            WinAPI.ReadProcessMemory(p.Handle, addr, tmp, tmp.Length, out)
            r.Cells(1).Value = Hex(tmp(0))
        Next
    End Sub

    'Preset handler
    Private Sub Step4_ButtonPreset_MouseClick(sender As Object, e As System.Windows.Forms.MouseEventArgs) Handles _
Step4_ButtonPreset01.MouseDown, Step4_ButtonPreset02.MouseDown, Step4_ButtonPreset03.MouseDown, _
Step4_ButtonPreset04.MouseDown, Step4_ButtonPreset05.MouseDown, Step4_ButtonPreset06.MouseDown, Step4_ButtonPreset07.MouseDown

        Dim n As String = DirectCast(sender, Button).Name.Replace("Step4_ButtonPreset", "")
        If Not FileIO.FileSystem.DirectoryExists(".\EmulatorConfigPresets") Then FileIO.FileSystem.CreateDirectory(".\EmulatorConfigPresets")
        If e.Button = Windows.Forms.MouseButtons.Left Then
            If Not FileIO.FileSystem.FileExists(".\EmulatorConfigPresets\SysAddr" + processCrc.ToUpper.Trim + "_" + n + ".txt") Then Exit Sub

            Dim addrFrom As IntPtr = p.MainModule.BaseAddress
            Dim addrTo As IntPtr = p.MainModule.BaseAddress + p.MainModule.ModuleMemorySize
            ReDim addrs(CInt(addrTo.ToInt64 - addrFrom.ToInt64))
            For addrCur = 0 To addrs.Length - 1
                addrs(addrCur) = False
            Next
            Step4_DataGridView1.Rows.Clear()
            FileOpen(10, ".\EmulatorConfigPresets\SysAddr" + processCrc.ToUpper.Trim + "_" + n + ".txt", OpenMode.Input)
            Dim s() As String
            Do While Not EOF(10)
                s = LineInput(10).Split({":::"}, StringSplitOptions.None)
                Step4_DataGridView1.Rows.Add(s(0), s(1))
                addrs(CInt("&H" + s(0))) = True
            Loop
            FileClose(10)
            step4_timer.Enabled = True
            Label13.Text = "Currently found: " + Step4_DataGridView1.Rows.Count.ToString
        ElseIf e.Button = Windows.Forms.MouseButtons.Right Then
            FileOpen(10, ".\EmulatorConfigPresets\SysAddr" + processCrc.ToUpper.Trim + "_" + n + ".txt", OpenMode.Output)
            For Each r As DataGridViewRow In Step4_DataGridView1.Rows
                PrintLine(10, r.Cells(0).Value.ToString + ":::" + r.Cells(1).Value.ToString)
            Next
            FileClose(10)
            Dim newBold As New Font(DirectCast(sender, Button).Font.FontFamily, DirectCast(sender, Button).Font.Size, FontStyle.Bold)
            DirectCast(sender, Button).ForeColor = Color.Green
            DirectCast(sender, Button).Font = newBold
        End If
    End Sub
#End Region

#Region "Step5 - Find romname address"
    Dim blocks_unique As New Dictionary(Of ULong, ULong)

    'Fill preset buttons, init_module_map, tryToLoadPreviousConfig
    Private Sub Step5_init()
        For i As Integer = 1 To 7
            If FileIO.FileSystem.FileExists(".\EmulatorConfigPresets\RomAddr" + processCrc.ToUpper.Trim + "_0" + i.ToString + ".txt") Then
                Dim b As Button = CType(Me.Controls("TabControl1").Controls("TabPage5").Controls("Step5_ButtonPreset0" + i.ToString), Button)
                Dim newBold As New Font(b.Font.FontFamily, b.Font.Size, FontStyle.Bold)
                b.ForeColor = Color.Green : b.Font = newBold
            End If
        Next
        ReDim addrs_step5(0)
        Step5_init_module_map()
        Step5_tryToLoadPreviousConfigIntoTable() : insertCurrentConfigRow()
    End Sub

    'Fill module map regions and unique size
    Private Sub Step5_init_module_map()
        DataGridView2.Rows.Clear()
        mapped_files_ranges = New List(Of mapped_File_range)
        Dim mftmp As New mapped_File
        mapped_files = New List(Of mapped_File)
        Dim counter As Integer = 1
        Dim blocks As New Dictionary(Of ULong, ULong)
        blocks_unique = New Dictionary(Of ULong, ULong)

        Dim QueryModules As New List(Of String)
        Dim regions As List(Of Long()) = WinAPI.getRegionsList(p.Handle, QueryModules)
        For Each item As Long() In regions
            If Not blocks.Keys.Contains(CULng(item(0))) Then blocks.Add(CULng(item(0)), CULng(item(2)))

            If mapped_files.Count = 0 Then
                mftmp.addrStart = CULng(item(0))
                mftmp.length = CULng(item(2))
                mftmp.fileName = QueryModules(counter - 1)
                mapped_files.Add(mftmp)
            Else
                If mapped_files(mapped_files.Count - 1).fileName = QueryModules(counter - 1) Then
                    mftmp = mapped_files(mapped_files.Count - 1)
                    mftmp.length = mftmp.length + CULng(item(2))
                    mapped_files(mapped_files.Count - 1) = mftmp
                Else
                    mftmp.addrStart = CULng(item(0))
                    mftmp.length = CULng(item(2))
                    mftmp.fileName = QueryModules(counter - 1)
                    mapped_files.Add(mftmp)
                End If
            End If
            DataGridView2.Rows.Add({counter, Hex(item(0)), Hex(item(2)), QueryModules(counter - 1)})
            counter += 1
        Next
        For Each m As mapped_File In mapped_files
            If m.fileName.Trim = "" Then Continue For
            DataGridView2.Rows.Add({"xxx", Hex(m.addrStart), Hex(m.addrStart + m.length), m.fileName})
            mapped_files_ranges.Add(New mapped_File_range With {.addrStart = m.addrStart, .addrEnd = m.addrStart + m.length, .fileName = m.fileName})
        Next

        'Fill Unique Blocks
        For Each item In blocks.GroupBy(Function(x) x.Value).Where(Function(x) x.Count = 1)
            blocks_unique.Add(item(0).Key, blocks(item(0).Key))
        Next
    End Sub

    'Search click
    Private Sub Step5_Button_Search_Click(sender As System.Object, e As System.EventArgs) Handles Step5_Button_Search.Click
        step5_timer.Enabled = False
        If p.HasExited Then WaitForProcess()

        Dim c As Integer = 0
        Step5_DataGridView3.Rows.Clear() : insertCurrentConfigRow()

        Step5_TextBox1.Text = Step5_TextBox1.Text.Trim
        If Step5_TextBox1.Text.Length < 4 Then MsgBox("Your romname should be at least 4 characters long. Please, open rom with longer filename.") : Exit Sub
        Dim pattern(Step5_TextBox1.Text.Length - 1) As Integer
        For Each chr As Char In Step5_TextBox1.Text.ToCharArray()
            pattern(c) = Asc(chr)
            c += 1
        Next
        Dim retSize As Integer = 1
        Dim retDataInt(2048) As Integer
        Dim retDataLong(2048) As Long
        If Not System.Environment.Is64BitOperatingSystem Then
            p_is64bit = False
            WinAPI.fnmemsearch(p.Id, pattern, pattern.Length, retSize, retDataInt)
            For c = 0 To 2047
                If retDataInt(c) = 0 Then Exit For
                Step5_DataGridView3.Rows.Add({c + 1, Hex(retDataInt(c)), Step5_TextBox1.Text, "", ""})
            Next
        Else
            Dim check, ret As Boolean
            ret = WinAPI.IsWow64Process(p.Handle, check)
            If check Then
                p_is64bit = False
                WinAPI.fnmemsearch_x32_on_x64(p.Id, pattern, pattern.Length, retSize, retDataInt)
                For c = 0 To 2047
                    If retDataInt(c) = 0 Then Exit For
                    Step5_DataGridView3.Rows.Add({c + 1, Hex(retDataInt(c)), Step5_TextBox1.Text, "", ""})
                Next
            Else
                p_is64bit = True
                WinAPI.fnmemsearch_x64(p.Id, pattern, pattern.Length, retSize, retDataLong)
                For c = 0 To 2047
                    If retDataLong(c) = 0 Then Exit For
                    Step5_DataGridView3.Rows.Add({c + 1, Hex(retDataLong(c)), Step5_TextBox1.Text, "", ""})
                Next
            End If
        End If

        For Each r As DataGridViewRow In Step5_DataGridView3.Rows
            Dim curAddr As UInt64 = CULng("&H" + r.Cells(1).Value.ToString)
            For Each range As mapped_File_range In mapped_files_ranges
                If curAddr >= range.addrStart And curAddr < range.addrEnd Then
                    r.Cells(4).Value = range.fileName + "(+" + Hex(curAddr - range.addrStart) + ")"
                    Exit For
                End If
            Next
        Next

        If Step5_DataGridView3.RowCount > 0 Then
            Step5_Button_Update.Enabled = True : step5_timer.Enabled = True : Step5_NumericUpDown1.Value = Step5_TextBox1.Text.Length
            step5_refreshData(sender, e)
        End If
        Label38.Text = "Total: " + Step5_DataGridView3.Rows.Count.ToString
    End Sub

    'Update click
    Private Sub Step5_Button_Update_Click(sender As System.Object, e As System.EventArgs) Handles Step5_Button_Update.Click
        step5_timer.Enabled = False
        Step5_Button_Update.Enabled = False
        If p.HasExited Then WaitForProcess()

        If Step5_DataGridView3.Rows.Count = 0 Then Exit Sub
        Dim tmp(Step5_TextBox1.Text.Length - 1) As Byte
        Dim rowsToDel As New List(Of Integer)
        For Each r As DataGridViewRow In Step5_DataGridView3.Rows
            If r.Cells(3).Value.ToString().ToUpper.StartsWith("KEEP") Then Continue For
            Dim val_mem As String = Step5_getDataForRow(r.Index)
            'Dim val As String = r.Cells(1).Value.ToString
            'WinAPI.ReadProcessMemory(p.Handle, New IntPtr(CLng("&H" + val)), tmp, tmp.Length, 0)

            If Not val_mem.StartsWith(Step5_TextBox1.Text.Trim) Then rowsToDel.Add(r.Index)
            'For i As Integer = 0 To tmp.Length - 1
            'If tmp(i) <> Asc(Val.Substring(i, 1)) Then rowsToDel.Add(r.Index) : Exit For
            'Next
        Next

        deactivate_autosize_columns(True)
        For i As Integer = rowsToDel.Count - 1 To 0 Step -1
            Step5_DataGridView3.Rows.RemoveAt(rowsToDel(i))
        Next
        deactivate_autosize_columns(False)

        If Step5_DataGridView3.Rows.Count > 0 Then step5_timer.Enabled = True : Step5_Button_Update.Enabled = True
        Label38.Text = "Total: " + Step5_DataGridView3.Rows.Count.ToString
    End Sub

    'deactivate_autosize_columns (for speed up filling table)
    Private Sub deactivate_autosize_columns(deactivate As Boolean)
        If deactivate Then
            Step5_DataGridView3.Columns(0).AutoSizeMode = DataGridViewAutoSizeColumnMode.None
            Step5_DataGridView3.Columns(1).AutoSizeMode = DataGridViewAutoSizeColumnMode.None
            Step5_DataGridView3.Columns(2).AutoSizeMode = DataGridViewAutoSizeColumnMode.None
        Else
            Step5_DataGridView3.Columns(0).AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            Step5_DataGridView3.Columns(1).AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            Step5_DataGridView3.Columns(2).AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
        End If
    End Sub

    'Pointer search click
    Private Sub Step5_Button_pointerSearch_click(sender As System.Object, e As System.EventArgs) Handles Step5_Button_pointerSearch.Click
        If Step5_DataGridView3.SelectedCells.Count = 0 Then MsgBox("Please, select entry in the table.") : Exit Sub
        If Step5_DataGridView3.SelectedCells(0).OwningRow.Cells(3).Value.ToString().ToUpper.StartsWith("KEEP") Then Exit Sub
        Dim minstr, maxstr As String
        Dim minIsNegative As Boolean = False, maxIsNegative As Boolean = False
        minstr = Step5_pointerOffsetMin.Text.Trim
        maxstr = Step5_pointerOffsetMax.Text.Trim
        If minstr.StartsWith("-") Then minstr = minstr.Substring(1) : minIsNegative = True
        If maxstr.StartsWith("-") Then maxstr = maxstr.Substring(1) : maxIsNegative = True
        If Not IsNumeric("&H" + minstr) Then MsgBox("Offset FROM field is not hexadecimal number.") : Exit Sub
        If Not IsNumeric("&H" + maxstr) Then MsgBox("Offset TO field is not hexadecimal number.") : Exit Sub

        Dim min, max As Integer
        If minIsNegative Then min = 0 - CInt("&H" + minstr) Else min = CInt("&H" + minstr)
        If maxIsNegative Then max = 0 - CInt("&H" + maxstr) Else max = CInt("&H" + maxstr)
        If min > max Then MsgBox("Offset TO field is less than offset FROM field. This must be fixed.") : Exit Sub

        step5_timer.Enabled = False
        'TODO UINT in 64x processes not enough
        Dim retSize As Integer = 1
        Dim offset As Long = 0
        Dim offsetStr As String
        Dim retDataInt(100000) As Integer
        Dim retDataLong(100000) As Long

        If Not System.Environment.Is64BitOperatingSystem Then
            p_is64bit = False
            ''''TODO
        Else
            Dim check, ret As Boolean
            ret = WinAPI.IsWow64Process(p.Handle, check)
            If check Then
                p_is64bit = False
                Dim tmp(3) As Byte
                Dim addr As UInteger = CUInt("&H" + Step5_DataGridView3.SelectedCells(0).OwningRow.Cells(1).Value.ToString)
                WinAPI.fnmemsearch_pointer_x32_on_x64(p.Id, CUInt(addr + min), CUInt(addr + max), retSize, retDataInt)
                For c = 0 To 9999
                    If retDataInt(c) = 0 Then Exit For
                    WinAPI.ReadProcessMemory(p.Handle, New IntPtr(retDataInt(c)), tmp, tmp.Length, 0)
                    'Array.Reverse(tmp)
                    offset = addr - BitConverter.ToUInt32(tmp, 0)
                    If offset >= 0 Then offsetStr = Hex(offset) Else offsetStr = "-" + Hex(0 - offset)
                    Step5_DataGridView3.Rows.Add({c + 1, Hex(retDataInt(c)), "", "pointer(" + offsetStr + ")", ""})
                Next
            Else
                p_is64bit = True
                Dim tmp(5) As Byte
                Dim addr As ULong = CULng("&H" + Step5_DataGridView3.SelectedCells(0).OwningRow.Cells(1).Value.ToString)
                WinAPI.fnmemsearch_pointer_x64(p.Id, CULng(addr + min), CULng(addr + max), retSize, retDataLong)
                For c = 0 To 9999
                    If retDataLong(c) = 0 Then Exit For
                    'Array.Resize(tmp, 6)
                    WinAPI.ReadProcessMemory(p.Handle, New IntPtr(retDataLong(c)), tmp, tmp.Length, 0)
                    'Array.Reverse(tmp)
                    Array.Resize(tmp, 8)
                    offset = CLng(addr - BitConverter.ToUInt64(tmp, 0))
                    If offset >= 0 Then offsetStr = Hex(offset) Else offsetStr = "-" + Hex(0 - offset)
                    Step5_DataGridView3.Rows.Add({c + 1, Hex(retDataLong(c)), "", "pointer(" + offsetStr + ")", ""})
                Next
            End If
        End If

        For Each r As DataGridViewRow In Step5_DataGridView3.Rows
            Dim curAddr As UInt64 = CULng("&H" + r.Cells(1).Value.ToString)
            For Each range As mapped_File_range In mapped_files_ranges
                If curAddr >= range.addrStart And curAddr < range.addrEnd Then
                    r.Cells(4).Value = range.fileName + "(+" + Hex(curAddr - range.addrStart) + ")"
                End If
            Next
        Next
        step5_timer.Enabled = True
        Label38.Text = "Total: " + Step5_DataGridView3.Rows.Count.ToString
    End Sub

    'Timer - refresh data
    Private Sub step5_refreshData(sender As Object, e As System.EventArgs) Handles step5_timer.Tick
        If curStep = 5 Then
            If p.HasExited Then WaitForProcess()
            If Step5_DataGridView3.Rows.Count > 2000 Then
                Label41.Visible = True : Exit Sub
            Else
                Label41.Visible = False
            End If

            deactivate_autosize_columns(True)
            For Each r As DataGridViewRow In Step5_DataGridView3.Rows
                Dim val As String = Step5_getDataForRow(r.Index)
                If Step5_RadioButton3.Checked And Step5_TextBox2.Text <> "" Then
                    Dim pos As Integer = val.IndexOf(Step5_TextBox2.Text)
                    If pos >= 0 Then val = val.Substring(0, pos)
                End If
                r.Cells(2).Value = val
            Next
            deactivate_autosize_columns(False)
        End If
    End Sub

    'get data for row sub
    Private Function Step5_getDataForRow(rowIndex As Integer) As String
        Dim len As Integer
        If Step5_RadioButton1.Checked Then len = CInt(Step5_NumericUpDown1.Value) - 1 Else len = 255
        Dim tmp(len) As Byte

        Dim r As DataGridViewRow = Step5_DataGridView3.Rows(rowIndex)
        If r.Cells(3).Value.ToString().ToUpper.StartsWith("KEEP") Then Return ""
        Dim val As String = r.Cells(1).Value.ToString
        If Step5_CheckBox2.Checked Then
            If Not r.Cells(4).Value Is Nothing Then
                Dim modul As String = r.Cells(4).Value.ToString.Trim
                If modul <> "" Then
                    Dim moduleName = modul.Substring(0, modul.IndexOf("("))
                    For Each range As mapped_File_range In mapped_files_ranges
                        If range.fileName = moduleName Then
                            Dim offset_str As String = modul.Substring(modul.IndexOf("(") + 1)
                            Dim offset As UInt64 = CULng("&H" + offset_str.Substring(0, offset_str.Length - 1))
                            val = Hex((range.addrStart + offset).ToString)
                            Exit For
                        End If
                    Next
                End If
            End If
        End If

        If r.Cells(3).Value.ToString.StartsWith("pointer") Then
            'Dim ptrOffsetNegative As Boolean = False
            Dim ptrOffsetStr As String = r.Cells(3).Value.ToString
            ptrOffsetStr = ptrOffsetStr.Substring(ptrOffsetStr.IndexOf("(") + 1)
            ptrOffsetStr = ptrOffsetStr.Substring(0, ptrOffsetStr.IndexOf(")"))

            Dim ptrOffset As Integer
            If ptrOffsetStr.StartsWith("-") Then
                ptrOffset = 0 - CInt("&H" + ptrOffsetStr.Substring(1))
            Else
                ptrOffset = CInt("&H" + ptrOffsetStr)
            End If

            Dim pointer_len As Integer
            If p_is64bit Then pointer_len = 5 Else pointer_len = 3
            Dim tmp2(pointer_len) As Byte
            WinAPI.ReadProcessMemory(p.Handle, New IntPtr(CLng("&H" + val)), tmp2, tmp2.Length, 0)

            Array.Resize(tmp2, 8)
            If p_is64bit Then val = Hex(BitConverter.ToUInt64(tmp2, 0) + ptrOffset) Else val = Hex(BitConverter.ToUInt32(tmp2, 0) + ptrOffset)
        End If
        WinAPI.ReadProcessMemory(p.Handle, New IntPtr(CLng("&H" + val)), tmp, tmp.Length, 0)

        val = ""
        For i As Integer = 0 To tmp.Length - 1
            If tmp(i) = 0 Then Exit For
            If Step5_RadioButton2.Checked And tmp.Length - 1 > i Then
                If tmp(i) = 32 And tmp(i + 1) = 32 Then Exit For
            End If
            val = val + Chr(tmp(i))
        Next
        Return val
    End Function

    'Change retrieving romname mode
    Private Sub Step5_RadioButton1_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles Step5_RadioButton1.CheckedChanged, Step5_RadioButton2.CheckedChanged, Step5_RadioButton3.CheckedChanged
        If Step5_RadioButton3.Checked And Step5_TextBox2.Text = "" Then
            Step5_RadioButton2.Checked = True
            MsgBox("You must enter custom stop string to use it.")
        End If
        If Step5_DataGridView3.Rows.Count = 0 Then Exit Sub
        step5_refreshData(sender, e)
    End Sub

    'Table double-click
    Private Sub Step5_DataGridView3_CellContentDoubleClick(sender As Object, e As System.Windows.Forms.DataGridViewCellEventArgs) Handles Step5_DataGridView3.CellContentDoubleClick
        Step5_TextBox1.Text = Step5_DataGridView3.Rows(e.RowIndex).Cells(2).Value.ToString
    End Sub

    'Search field enter key press handler
    Private Sub Step5_TextBox1_KeyDown(sender As Object, e As System.Windows.Forms.KeyEventArgs) Handles Step5_TextBox1.KeyDown
        If e.KeyCode = Keys.Enter Then Step5_Button_Search_Click(sender, e)
    End Sub

    'Preset handler
    Private Sub Step5_ButtonPreset_MouseClick(sender As Object, e As System.Windows.Forms.MouseEventArgs) Handles _
Step5_ButtonPreset01.MouseDown, Step5_ButtonPreset02.MouseDown, Step5_ButtonPreset03.MouseDown, _
Step5_ButtonPreset04.MouseDown, Step5_ButtonPreset05.MouseDown, Step5_ButtonPreset06.MouseDown, Step5_ButtonPreset07.MouseDown

        Dim n As String = DirectCast(sender, Button).Name.Replace("Step5_ButtonPreset", "")
        If Not FileIO.FileSystem.DirectoryExists(".\EmulatorConfigPresets") Then FileIO.FileSystem.CreateDirectory(".\EmulatorConfigPresets")
        If e.Button = Windows.Forms.MouseButtons.Left Then
            If Not FileIO.FileSystem.FileExists(".\EmulatorConfigPresets\RomAddr" + processCrc.ToUpper.Trim + "_" + n + ".txt") Then Exit Sub
            Step5_DataGridView3.Rows.Clear() : insertCurrentConfigRow()
            FileOpen(10, ".\EmulatorConfigPresets\RomAddr" + processCrc.ToUpper.Trim + "_" + n + ".txt", OpenMode.Input)
            Dim s() As String
            Do While Not EOF(10)
                s = LineInput(10).Split({":::"}, StringSplitOptions.None)
                If s(0).ToUpper.StartsWith("ROMNAME") Then Step5_TextBox1.Text = s(0).Substring(s(0).IndexOf("=") + 1).Trim : Continue Do
                If s(0).ToUpper.StartsWith("FIXED_LENGTH_SIZE") Then Step5_NumericUpDown1.Value = CDec(s(0).Substring(s(0).IndexOf("=") + 1).Trim) : Continue Do
                If s(0).ToUpper.StartsWith("CUSTOM_STRING") Then Step5_TextBox2.Text = s(0).Substring(s(0).IndexOf("=") + 1).Trim : Continue Do
                If s(0).ToUpper.StartsWith("OFFSET_MIN") Then Step5_pointerOffsetMin.Text = s(0).Substring(s(0).IndexOf("=") + 1).Trim : Continue Do
                If s(0).ToUpper.StartsWith("OFFSET_MAX") Then Step5_pointerOffsetMax.Text = s(0).Substring(s(0).IndexOf("=") + 1).Trim : Continue Do
                If s(0).ToUpper.StartsWith("USE_RELATIVE_MODULE_ADDR") Then
                    If s(0).Substring(s(0).IndexOf("=") + 1).Trim = "0" Then Step5_CheckBox2.Checked = False Else Step5_CheckBox2.Checked = True
                    Continue Do
                End If
                If s(0).ToUpper.StartsWith("REMOVE_EXTENSION") Then
                    If s(0).Substring(s(0).IndexOf("=") + 1).Trim = "0" Then Step5_CheckBox1.Checked = False Else Step5_CheckBox1.Checked = True
                    Continue Do
                End If
                If s(0).ToUpper.StartsWith("RETRIVING_ROMNAME_MODE") Then
                    Dim t As Integer = CInt(s(0).Substring(s(0).IndexOf("=") + 1).Trim)
                    If t = 1 Then Step5_RadioButton1.Checked = True
                    If t = 2 Then Step5_RadioButton2.Checked = True
                    If t = 3 Then Step5_RadioButton3.Checked = True
                    Continue Do
                End If

                Step5_DataGridView3.Rows.Add(s(0), s(1), s(2), s(3), s(4))
            Loop
            FileClose(10)
            step5_timer.Enabled = True
            Label38.Text = "Total: " + Step5_DataGridView3.Rows.Count.ToString
            If Step5_DataGridView3.Rows.Count > 0 Then Step5_Button_Update.Enabled = True Else Step5_Button_Update.Enabled = False
        ElseIf e.Button = Windows.Forms.MouseButtons.Right Then
            FileOpen(10, ".\EmulatorConfigPresets\RomAddr" + processCrc.ToUpper.Trim + "_" + n + ".txt", OpenMode.Output)
            For Each r As DataGridViewRow In Step5_DataGridView3.Rows
                PrintLine(10, r.Cells(0).Value.ToString + ":::" + r.Cells(1).Value.ToString + ":::" + _
                          r.Cells(2).Value.ToString.Replace(vbCr, "").Replace(vbLf, "") + ":::" + r.Cells(3).Value.ToString + ":::" + r.Cells(4).Value.ToString)
            Next
            PrintLine(10, "RomName = " + Step5_TextBox1.Text)
            PrintLine(10, "Fixed_Length_Size = " + Step5_NumericUpDown1.Value.ToString)
            PrintLine(10, "Custom_String = " + Step5_TextBox2.Text)

            PrintLine(10, "Offset_min = " + Step5_pointerOffsetMin.Text)
            PrintLine(10, "Offset_max = " + Step5_pointerOffsetMax.Text)
            If Step5_CheckBox2.Checked Then PrintLine(10, "USE_RELATIVE_MODULE_ADDR = 1") Else PrintLine(10, "USE_RELATIVE_MODULE_ADDR = 0")
            If Step5_CheckBox1.Checked Then PrintLine(10, "REMOVE_EXTENSION = 1") Else PrintLine(10, "REMOVE_EXTENSION = 0")
            If Step5_RadioButton1.Checked Then PrintLine(10, "RETRIVING_ROMNAME_MODE = 1")
            If Step5_RadioButton2.Checked Then PrintLine(10, "RETRIVING_ROMNAME_MODE = 2")
            If Step5_RadioButton3.Checked Then PrintLine(10, "RETRIVING_ROMNAME_MODE = 3")
            FileClose(10)
            Dim newBold As New Font(DirectCast(sender, Button).Font.FontFamily, DirectCast(sender, Button).Font.Size, FontStyle.Bold)
            DirectCast(sender, Button).ForeColor = Color.Green
            DirectCast(sender, Button).Font = newBold
        End If
    End Sub

    'Loading previous config to table (if updating emulator)
    Private Sub Step5_tryToLoadPreviousConfigIntoTable()
        romAddrExistingSettings(0) = ""
        romAddrExistingSettings(1) = ""
        romAddrExistingSettings(2) = ""
        If updatingEmulatorVersionNode Is Nothing Then Exit Sub
        Dim hex_op As String = "0"
        Dim pointer_str As String = ""
        Dim addr_str As String = ""
        Dim modul_str As String = ""
        Dim expression As String = updatingEmulatorVersionNode.SelectSingleNode("romname").InnerText.ToLower.Trim

        If expression.StartsWith("removeextension") Then
            Step5_CheckBox1.Checked = True
            expression = replaceFirst(expression, "removeextension", "")
            expression = replaceFirst(expression, "(", "")
            expression = replaceLast(expression, ")", "").Trim
        Else
            Step5_CheckBox1.Checked = False
        End If

        If expression.StartsWith("readfromto") Then
            Dim QuoterLast As Integer = expression.LastIndexOf("""")
            Dim QuoterFirst As Integer = expression.LastIndexOf("""", QuoterLast - 1)
            Dim QuoterSubstr As String = expression.Substring(QuoterFirst + 1, QuoterLast - QuoterFirst - 1)
            If QuoterSubstr.Trim = "" Then
                Step5_RadioButton2.Checked = True
            Else
                Step5_RadioButton3.Checked = True
                Step5_TextBox2.Text = QuoterSubstr
            End If

            expression = replaceFirst(expression, "readfromto", "")
            expression = replaceFirst(expression, "(", "")
            expression = expression.Substring(0, expression.LastIndexOf(";")).Trim
        End If

        If expression.StartsWith("readnbytefrom") Then
            Step5_RadioButton1.Checked = True
            Dim value As Integer = CInt(expression.Substring(expression.LastIndexOf(";") + 1).Replace(")", "").Trim)
            Step5_NumericUpDown1.Value = value
            expression = replaceFirst(expression, "readnbytefrom", "")
            expression = replaceFirst(expression, "(", "")
            expression = expression.Substring(0, expression.LastIndexOf(";")).Trim
        End If

        If expression.StartsWith("hexadd") Then
            Dim QuoterLast As Integer = expression.LastIndexOf("""")
            Dim QuoterFirst As Integer = expression.LastIndexOf("""", QuoterLast - 1)
            hex_op = expression.Substring(QuoterFirst + 1, QuoterLast - QuoterFirst - 1)

            expression = replaceFirst(expression, "hexadd", "")
            expression = replaceFirst(expression, "(", "")
            expression = expression.Substring(0, expression.LastIndexOf(";")).Trim
        ElseIf expression.StartsWith("hexsubstract") Then
            Dim QuoterLast As Integer = expression.LastIndexOf("""")
            Dim QuoterFirst As Integer = expression.LastIndexOf("""", QuoterLast - 1)
            hex_op = "-" + expression.Substring(QuoterFirst + 1, QuoterLast - QuoterFirst - 1)

            expression = replaceFirst(expression, "hexsubstract", "")
            expression = replaceFirst(expression, "(", "")
            expression = expression.Substring(0, expression.LastIndexOf(";")).Trim
        End If

        If expression.StartsWith("frompointer32bit") Then
            expression = replaceFirst(expression, "frompointer32bit", "")
            expression = replaceFirst(expression, "(", "")
            expression = replaceLast(expression, ")", "").Trim

            pointer_str = "pointer(" + hex_op + ")"
        ElseIf expression.StartsWith("frompointer") Then
            expression = replaceFirst(expression, "frompointer", "")
            expression = replaceFirst(expression, "(", "")
            expression = replaceLast(expression, ")", "").Trim

            pointer_str = "pointer(" + hex_op + ")"
        End If

        If expression.StartsWith("baseaddressof") Then
            Dim QuoterLast As Integer = expression.LastIndexOf("""")
            Dim QuoterFirst As Integer = expression.LastIndexOf("""", QuoterLast - 1)
            Dim offset As String = expression.Substring(QuoterFirst + 1, QuoterLast - QuoterFirst - 1)

            QuoterLast = expression.LastIndexOf("""", QuoterFirst - 1)
            QuoterFirst = expression.LastIndexOf("""", QuoterLast - 1)
            modul_str = expression.Substring(QuoterFirst + 1, QuoterLast - QuoterFirst - 1) + "(+" + offset + ")"

            Step5_CheckBox2.Checked = True
            expression = replaceFirst(expression, "baseaddress", "")
            expression = replaceFirst(expression, "(", "")
            expression = expression.Substring(0, expression.LastIndexOf(";")).Trim
        ElseIf expression.StartsWith("baseaddress") Then
            Dim QuoterLast As Integer = expression.LastIndexOf("""")
            Dim QuoterFirst As Integer = expression.LastIndexOf("""", QuoterLast - 1)
            Dim offset As String = expression.Substring(QuoterFirst + 1, QuoterLast - QuoterFirst - 1)
            modul_str = p.MainModule.FileName.Substring(p.MainModule.FileName.LastIndexOf("\") + 1) + "(+" + offset + ")"

            Step5_CheckBox2.Checked = True
            expression = replaceFirst(expression, "baseaddress", "")
            expression = replaceFirst(expression, "(", "")
            expression = replaceLast(expression, ")", "").Trim
        ElseIf Not expression.Contains("(") And Not expression.Contains(")") Then
            Step5_CheckBox2.Checked = False
            Dim QuoterLast As Integer = expression.LastIndexOf("""")
            Dim QuoterFirst As Integer = expression.LastIndexOf("""", QuoterLast - 1)
            addr_str = expression.Substring(QuoterFirst + 1, QuoterLast - QuoterFirst - 1)
        Else
            romAddrExistingSettings(0) = ""
            romAddrExistingSettings(1) = "Keep current Config"
            romAddrExistingSettings(2) = ""
        End If
        romAddrExistingSettings(0) = addr_str
        romAddrExistingSettings(1) = pointer_str
        romAddrExistingSettings(2) = modul_str
    End Sub

    'Insert current config row to table
    Private Sub insertCurrentConfigRow()
        If romAddrExistingSettings(0) <> "" Or romAddrExistingSettings(2) <> "" Then
            If Step5_DataGridView3.Rows.Count = 0 Then
                Step5_DataGridView3.Rows.Add({0, romAddrExistingSettings(0), "", romAddrExistingSettings(1), romAddrExistingSettings(2)})
                If romAddrExistingSettings(1).ToUpper.StartsWith("KEEP") Then Exit Sub
                Dim r As DataGridViewRow = Step5_DataGridView3.Rows(0)


                'Calculate address if there is no one
                If romAddrExistingSettings(0) = "" Then
                    Dim modul As String = r.Cells(4).Value.ToString.Trim
                    Dim moduleName = modul.Substring(0, modul.IndexOf("("))
                    For Each range As mapped_File_range In mapped_files_ranges
                        If range.fileName = moduleName Then
                            Dim offset_str As String = modul.Substring(modul.IndexOf("(") + 1)
                            Dim offset As UInt64 = CULng("&H" + offset_str.Substring(0, offset_str.Length - 1))
                            r.Cells(1).Value = Hex((range.addrStart + offset).ToString)
                            Exit For
                        End If
                    Next
                End If

                'Update module info if there is no one
                If romAddrExistingSettings(2) = "" Then
                    Dim curAddr As UInt64 = CULng("&H" + r.Cells(1).Value.ToString)
                    For Each range As mapped_File_range In mapped_files_ranges
                        If curAddr >= range.addrStart And curAddr < range.addrEnd Then
                            r.Cells(4).Value = range.fileName + "(+" + Hex(curAddr - range.addrStart) + ")"
                            Exit For
                        End If
                    Next
                End If

                step5_timer.Enabled = True
            End If
        End If
    End Sub

    'String helper functions
    Private Function replaceFirst(text As String, pattern As String, replacer As String) As String
        Dim pos As Integer = text.IndexOf(pattern)
        If pos < 0 Then Return text
        Return text.Substring(0, pos) + replacer + text.Substring(pos + pattern.Length)
    End Function
    Private Function replaceLast(text As String, pattern As String, replacer As String) As String
        Dim pos As Integer = text.LastIndexOf(pattern)
        If pos < 0 Then Return text
        Return text.Substring(0, pos) + replacer + text.Substring(pos + pattern.Length)
    End Function
#End Region

#Region "Step5.1 - Find romname by expression"
    Private Sub Step51_init()
        If Step51_TextBox2.Text <> "" Then step51_timer_sub(step51_timer, New System.EventArgs)
        step51_timer.Enabled = True
        Step5_init_module_map() 'It is needed for step 6, but if we pass through step 5.1 it was not initialized in step 5
    End Sub

    Private Sub step51_timer_sub(sender As Object, e As System.EventArgs) Handles step51_timer.Tick
        If curStep = 51 Then
            If p.HasExited Then WaitForProcess()
            CustomFunctions.p = p
            Dim econtext As New Ciloci.Flee.ExpressionContext(New CustomFunctions)
            Try
                Dim ev As Ciloci.Flee.IGenericExpression(Of String) = econtext.CompileGeneric(Of String)(Step51_TextBox2.Text)
                Step51_label1.Text = "Result:" + vbCrLf + ev.Evaluate().ToString
            Catch ex As Exception
                Step51_label1.Text = "Result ERROR:" + vbCrLf + ex.Message
            End Try
        End If
    End Sub

    Private Sub Button3_Click(sender As System.Object, e As System.EventArgs) Handles Button3.Click
        If ListBox1.SelectedIndex >= 0 Then
            Step51_TextBox2.Text = Step51_TextBox2.Text.Insert(Step51_TextBox2.SelectionStart, ListBox1.SelectedItem.ToString.Replace("index", "0").Replace("length", "10").Replace("text", """text"""))
        End If
    End Sub
#End Region

#Region "Step6 - Find 0 address"
    Private Sub Step6_init()
        For i As Integer = 1 To 7
            If FileIO.FileSystem.FileExists(".\EmulatorConfigPresets\Addr" + processCrc.ToUpper.Trim + "_0" + i.ToString + ".txt") Then
                Dim b As Button = CType(Me.Controls("TabControl1").Controls("TabPage6").Controls("Step6_ButtonPreset0" + i.ToString), Button)
                Dim newBold As New Font(b.Font.FontFamily, b.Font.Size, FontStyle.Bold)
                b.ForeColor = Color.Green : b.Font = newBold
            End If
        Next
        Step6_tryToLoadPreviousConfigIntoTable() : Step6_insert_current_config_row()
    End Sub
    Private Sub Step6_ButtonGo_Click(sender As System.Object, e As System.EventArgs) Handles Step6_ButtonGo.Click
        Dim offset As Integer
        Dim value() As Byte
        Dim value_r() As Byte
        If Step6_TextBox1.Text.Length = 4 Or Step6_TextBox1.Text.Length = 8 Or Step6_TextBox1.Text.Length = 16 Then
            Try
                value = BitConverter.GetBytes(CULng("&H" + Step6_TextBox1.Text))
                value_r = BitConverter.GetBytes(CULng("&H" + Step6_TextBox1.Text))
            Catch ex As Exception
                MsgBox("Invalid value.") : Exit Sub
            End Try
        Else
            MsgBox("Invalid value length. Should be 2, 4 or 8 bytes.") : Exit Sub
        End If
        Try
            offset = CInt(Step6_TextBox2.Text)
        Catch ex As Exception
            MsgBox("Invalid offset.") : Exit Sub
        End Try
        If p.HasExited Then
            Dim s As String = p.ProcessName
            If Process.GetProcessesByName(s).Count = 0 Then MsgBox("Process has exited. Please, reopen emulator.") : Exit Sub
            p = Process.GetProcessesByName(s)(0)
        End If

        Dim b As Boolean = False
        Dim c As Integer = 0
        'Step6_ListBox1.Items.Clear()
        Step6_DataGridView1.Rows.Clear() : Step6_insert_current_config_row()
        'addrs_step6_in_bytes = New List(Of Byte())
        addrs_step6_offset_in_bytes = New List(Of Byte())
        ReDim addrs_step6(0)
        Array.Resize(value, Step6_TextBox1.Text.Length \ 2)
        Array.Resize(value_r, Step6_TextBox1.Text.Length \ 2)
        Array.Reverse(value_r)
        Dim regions As List(Of Long()) = WinAPI.getRegionsList(p.Handle)

        ProgressBar2.Value = 0
        ProgressBar2.Maximum = 11
        Label19.Text = "Status: Searching addresses (1/3)" : Label19.BackColor = Color.Yellow : Label19.Refresh()

        Dim retSize As Integer = 1
        Dim retDataInt(90000) As Integer
        Dim retDataLong(90000) As Long
        Dim pattern((Step6_TextBox1.Text.Length \ 2) - 1) As Integer
        Dim pattern_r((Step6_TextBox1.Text.Length \ 2) - 1) As Integer
        For i As Integer = 0 To Step6_TextBox1.Text.Length - 1 Step 2
            pattern(c) = CInt("&H" + Step6_TextBox1.Text.Substring(i, 2))
            If pattern(c) > 127 Then pattern(c) = -128 + (pattern(c) - 128)
            pattern_r(c) = pattern(c)
            c += 1
        Next
        Array.Reverse(pattern_r)

        Dim _retdata As returndata = New returndata
        _retdata = fnMemSearch(p.Id, p.Handle, pattern, pattern.Length, retSize)
        For c = 0 To 2047
            If _retdata.retType = 0 Then
                If _retdata.retInt(c) = 0 Then Exit For
                Step6_DataGridView1.Rows.Add({c + 1, Hex(_retdata.retInt(c)), Step6_TextBox1.Text, "", "", ""})
            Else
                If _retdata.retLong(c) = 0 Then Exit For
                Step6_DataGridView1.Rows.Add({c + 1, Hex(_retdata.retLong(c)), Step6_TextBox1.Text, "", "", ""})
            End If
        Next

        retSize = 1
        _retdata = New returndata
        _retdata = fnMemSearch(p.Id, p.Handle, pattern_r, pattern_r.Length, retSize)
        For c = 0 To 2047
            If _retdata.retType = 0 Then
                If _retdata.retInt(c) = 0 Then Exit For
                Step6_DataGridView1.Rows.Add({c + 1, Hex(_retdata.retInt(c)) + "-R", Step6_TextBox1.Text, "", "", ""})
            Else
                If _retdata.retLong(c) = 0 Then Exit For
                Step6_DataGridView1.Rows.Add({c + 1, Hex(_retdata.retLong(c)) + "-R", Step6_TextBox1.Text, "", "", ""})
                'addrs_step6_in_bytes.Add(BitConverter.GetBytes(retDataLong(c)))
            End If
        Next

        ProgressBar2.Value = 1 : ProgressBar2.Refresh()

        Label19.Text = "Status: Searching pointers (2/3)" : Label19.BackColor = Color.YellowGreen : Label19.Refresh()
        Dim c2 As Integer
        Dim offsetStr As String
        Dim offsetUint As UInteger
        If Step6_DataGridView1.Rows.Count > 10 Then c2 = 9 Else c2 = Step6_DataGridView1.Rows.Count - 1
        ProgressBar2.Maximum = c2 + 2
        For i = 0 To c2
            Dim curaddr_str As String = Step6_DataGridView1.Rows(i).Cells(1).Value.ToString.Replace("@", "").Replace("-R", "").Trim
            Dim curaddr_ulong As ULong = CULng("&H" + curaddr_str)

            retSize = 1
            Dim retdata As New returndata
            retdata = fnMemSearchPointer(p.Id, p.Handle, curaddr_ulong - CUInt(offset), curaddr_ulong, retSize)
            If retdata.retType = 0 Then
                Array.Resize(tmp, 4)
                For c = 0 To 9999
                    If retdata.retInt(c) = 0 Then Exit For
                    WinAPI.ReadProcessMemory(p.Handle, New IntPtr(retdata.retInt(c)), tmp, tmp.Length, 0)
                    Try
                        If BitConverter.ToUInt32(tmp, 0) > curaddr_ulong Then Continue For
                        If curaddr_ulong - BitConverter.ToUInt32(tmp, 0) > CUInt(offset) Then Continue For
                        offsetUint = CUInt(curaddr_ulong - BitConverter.ToUInt32(tmp, 0))
                    Catch ex As Exception
                        MsgBox("Crash! addr - value from pointer @ " + Hex(retdata.retInt(c)) + vbCrLf + "addr: " + Hex(curaddr_ulong) + vbCrLf + "val: " + Hex(BitConverter.ToUInt32(tmp, 0)))
                    End Try

                    If offset >= 0 Then offsetStr = Hex(offset) Else offsetStr = "-" + Hex(0 - offset)
                    Step6_DataGridView1.Rows.Add({c + 1, Hex(retdata.retInt(c)), Step6_TextBox1.Text, "pointer( + " + offsetUint.ToString + " )", "", Hex(BitConverter.ToUInt32(tmp, 0)) + " + (" + offsetUint.ToString + " ) = " + Hex(curaddr_ulong)})
                Next
            Else
                Array.Resize(tmp, 8)
                For c = 0 To 9999
                    If retdata.retLong(c) = 0 Then Exit For
                    WinAPI.ReadProcessMemory(p.Handle, New IntPtr(retdata.retLong(c)), tmp, 6, 0)
                    ''''UGLY HACK - memsearch x64 seams to check addresses values in int32
                    If BitConverter.ToUInt64(tmp, 0) > curaddr_ulong Then Continue For
                    ''''UGLY HACK
                    Try
                        If BitConverter.ToUInt64(tmp, 0) > curaddr_ulong Then Continue For
                        If curaddr_ulong - BitConverter.ToUInt64(tmp, 0) > CUInt(offset) Then Continue For
                        offsetUint = CUInt(curaddr_ulong - BitConverter.ToUInt64(tmp, 0))
                    Catch ex As Exception
                        MsgBox("Crash! ""addr - value"" from pointer @ " + Hex(retdata.retLong(c)) + vbCrLf + "addr: " + Hex(curaddr_ulong) + vbCrLf + "val: " + Hex(BitConverter.ToUInt64(tmp, 0)))
                    End Try

                    If offset >= 0 Then offsetStr = Hex(offset) Else offsetStr = "-" + Hex(0 - offset)
                    Step6_DataGridView1.Rows.Add({c + 1, Hex(retdata.retLong(c)), Step6_TextBox1.Text, "pointer( + " + offsetUint.ToString + " )", "", Hex(BitConverter.ToUInt64(tmp, 0)) + " + (" + offsetUint.ToString + " ) = " + Hex(curaddr_ulong)})
                Next
            End If
            ProgressBar2.Value = ProgressBar2.Value + 1 : ProgressBar2.Refresh()
        Next

        ProgressBar2.Value = 0
        Label19.Text = "Status: Searching pointers to pointers (3/3)" : Label19.BackColor = Color.LightGreen : Label19.Refresh()

        Label19.Text = "Status: Retrive module names" : Label19.BackColor = Color.LightBlue : Label19.Refresh()
        For Each r As DataGridViewRow In Step6_DataGridView1.Rows
            Dim curAddr As UInt64 = CULng("&H" + r.Cells(1).Value.ToString.Replace("-R", ""))
            For Each range As mapped_File_range In mapped_files_ranges
                If curAddr >= range.addrStart And curAddr < range.addrEnd Then
                    r.Cells(4).Value = range.fileName + "(+" + Hex(curAddr - range.addrStart) + ")"
                End If
            Next

            If r.Cells(3).Value.ToString = "" Then
                For Each block In blocks_unique
                    If curAddr >= block.Key And curAddr < block.Key + block.Value Then
                        r.Cells(3).Value = "Unique Block Size: " + Hex(block.Value) + " + " + Hex(curAddr - block.Key)
                    End If
                Next
            End If
        Next

        ProgressBar2.Value = 0
        Label19.Text = "Status: IDLE" : Label19.BackColor = Color.Transparent

        'deactivate_autosize_columns(False)
        Step6_LabelTotal.Text = "Total: " + Step6_DataGridView1.Rows.Count.ToString
        step6_timer.Enabled = True
    End Sub

    Private Sub recalculate_pointers()
        step6_timer.Enabled = False
        Label19.Text = "Status: Searching pointers... " : Label19.BackColor = Color.YellowGreen : Label19.Refresh()
        Dim retSize As Integer = 1
        Dim offset As Integer

        Dim c2 As Integer
        Dim offsetStr As String
        Dim offsetUint As UInteger
        If Step6_DataGridView1.Rows.Count > 10 Then c2 = 9 Else c2 = Step6_DataGridView1.Rows.Count - 1
        ProgressBar2.Maximum = c2 + 2
        For i = 0 To c2
            Dim curaddr_str As String = Step6_DataGridView1.Rows(i).Cells(1).Value.ToString.Replace("@", "").Replace("-R", "").Trim
            'Dim curaddr_uint As UInteger = CUInt("&H" + curaddr_str)
            Dim curaddr_ulong As ULong = CULng("&H" + curaddr_str)

            retSize = 1
            Dim retdata As New returndata
            retdata = fnMemSearchPointer(p.Id, p.Handle, curaddr_ulong - CUInt(offset), curaddr_ulong, retSize)
            If retdata.retType = 0 Then
                Array.Resize(tmp, 4)
                For c = 0 To 9999
                    If retdata.retInt(c) = 0 Then Exit For
                    WinAPI.ReadProcessMemory(p.Handle, New IntPtr(retdata.retInt(c)), tmp, tmp.Length, 0)
                    Try
                        If BitConverter.ToUInt32(tmp, 0) > curaddr_ulong Then Continue For
                        If curaddr_ulong - BitConverter.ToUInt32(tmp, 0) > CUInt(offset) Then Continue For
                        offsetUint = CUInt(curaddr_ulong - BitConverter.ToUInt32(tmp, 0))
                    Catch ex As Exception
                        MsgBox("Crash! addr - value from pointer @ " + Hex(retdata.retInt(c)) + vbCrLf + "addr: " + Hex(curaddr_ulong) + vbCrLf + "val: " + Hex(BitConverter.ToUInt32(tmp, 0)))
                    End Try

                    If offset >= 0 Then offsetStr = Hex(offset) Else offsetStr = "-" + Hex(0 - offset)
                    Step6_DataGridView1.Rows.Add({c + 1, Hex(retdata.retInt(c)), Step6_TextBox1.Text, "pointer( + " + offsetUint.ToString + " )", "", Hex(BitConverter.ToUInt32(tmp, 0)) + " + (" + offsetUint.ToString + " ) = " + Hex(curaddr_ulong)})
                Next
            Else
                Array.Resize(tmp, 8)
                For c = 0 To 9999
                    If retdata.retLong(c) = 0 Then Exit For
                    WinAPI.ReadProcessMemory(p.Handle, New IntPtr(retdata.retLong(c)), tmp, 6, 0)
                    ''''UGLY HACK - memsearch x64 seams to check addresses values in int32
                    If BitConverter.ToUInt64(tmp, 0) > curaddr_ulong Then Continue For
                    ''''UGLY HACK
                    Try
                        If BitConverter.ToUInt64(tmp, 0) > curaddr_ulong Then Continue For
                        If curaddr_ulong - BitConverter.ToUInt64(tmp, 0) > CUInt(offset) Then Continue For
                        offsetUint = CUInt(curaddr_ulong - BitConverter.ToUInt64(tmp, 0))
                    Catch ex As Exception
                        MsgBox("Crash! ""addr - value"" from pointer @ " + Hex(retdata.retLong(c)) + vbCrLf + "addr: " + Hex(curaddr_ulong) + vbCrLf + "val: " + Hex(BitConverter.ToUInt64(tmp, 0)))
                    End Try

                    If offset >= 0 Then offsetStr = Hex(offset) Else offsetStr = "-" + Hex(0 - offset)
                    Step6_DataGridView1.Rows.Add({c + 1, Hex(retdata.retLong(c)), Step6_TextBox1.Text, "pointer( + " + offsetUint.ToString + " )", "", Hex(BitConverter.ToUInt64(tmp, 0)) + " + (" + offsetUint.ToString + " ) = " + Hex(curaddr_ulong)})
                Next
            End If
            ProgressBar2.Value = ProgressBar2.Value + 1 : ProgressBar2.Refresh()
        Next

        ProgressBar2.Value = 0
        Label19.Text = "Status: IDLE" : Label19.BackColor = Color.Transparent

        'deactivate_autosize_columns(False)
        Step6_LabelTotal.Text = "Total: " + Step6_DataGridView1.Rows.Count.ToString
        step6_timer.Enabled = True
    End Sub

    Private Sub Step6_ButtonRecheck_Click(sender As System.Object, e As System.EventArgs) Handles Step6_ButtonRecheck.Click
        '4do = 00000000a04000ea
        step6_timer.Enabled = False
        If p.HasExited Then
            Dim s As String = p.ProcessName
            If Process.GetProcessesByName(s).Count = 0 Then MsgBox("Process has exited. Please, reopen emulator.") : Exit Sub
            p = Process.GetProcessesByName(s)(0)
        End If

        Dim out As Integer
        Dim value() As Byte
        Dim value_r() As Byte
        If Step6_TextBox1.Text.Length = 4 Or Step6_TextBox1.Text.Length = 8 Or Step6_TextBox1.Text.Length = 16 Then
            Try
                value = BitConverter.GetBytes(CULng("&H" + Step6_TextBox1.Text))
                value_r = BitConverter.GetBytes(CULng("&H" + Step6_TextBox1.Text))
            Catch ex As Exception
                MsgBox("Invalid value.") : Exit Sub
            End Try
        Else
            MsgBox("Invalid value length. Should be 2, 4 or 8 bytes.") : Exit Sub
        End If
        Dim tmp2((Step6_TextBox1.Text.Length \ 2) - 1) As Byte
        Array.Resize(value, Step6_TextBox1.Text.Length \ 2)
        Array.Resize(value_r, Step6_TextBox1.Text.Length \ 2)
        Array.Reverse(value_r)

        'deactivate_autosize_columns(True)
        For i = Step6_DataGridView1.Rows.Count - 1 To 0 Step -1
            Array.Clear(tmp2, 0, tmp2.Length)
            Dim row As DataGridViewRow = Step6_DataGridView1.Rows(i)
            If row.Cells(3).Value.ToString().ToUpper.StartsWith("KEEP") Then Continue For

            Dim item_addr As String = row.Cells(1).Value.ToString.Replace("-R", "")
            Dim item_type As String = row.Cells(3).Value.ToString

            If Step6_CheckBox1.Checked Then
                If Not row.Cells(4).Value Is Nothing Then
                    Dim modul As String = row.Cells(4).Value.ToString.Trim
                    If modul <> "" Then
                        Dim moduleName = modul.Substring(0, modul.IndexOf("("))
                        For Each range As mapped_File_range In mapped_files_ranges
                            If range.fileName = moduleName Then
                                Dim offset_str As String = modul.Substring(modul.IndexOf("(") + 1)
                                Dim offset As UInt64 = CULng("&H" + offset_str.Substring(0, offset_str.Length - 1))
                                'item_addr = Hex((range.addrStart + offset).ToString)
                                item_addr = Hex(range.addrStart + offset)
                                Exit For
                            End If
                        Next
                    End If
                End If
            End If
            Dim addrNum As New IntPtr(CLng("&H" + item_addr))
            If Not item_type.StartsWith("pointer") Then
                WinAPI.ReadProcessMemory(p.Handle, addrNum, tmp2, tmp2.Length, out)
            Else
                Dim l As Integer = Step6_DataGridView1.Rows(i).Cells(5).Value.ToString.IndexOf(" ")
                If l > 8 Then Array.Resize(tmp, 8) Else Array.Resize(tmp, 4)
                Dim plus As String = item_type.Substring(item_type.IndexOf("+") + 2) : plus = plus.Substring(0, plus.IndexOf(" ")).Trim
                Dim plusNum As Integer = CInt(plus)
                WinAPI.ReadProcessMemory(p.Handle, addrNum, tmp, tmp.Length, out)
                If l > 8 Then
                    WinAPI.ReadProcessMemory(p.Handle, New IntPtr(BitConverter.ToInt64(tmp, 0) + plusNum), tmp2, tmp2.Length, out)
                Else
                    WinAPI.ReadProcessMemory(p.Handle, New IntPtr(BitConverter.ToUInt32(tmp, 0) + plusNum), tmp2, tmp2.Length, out)
                End If
            End If

            If tmp2.Length < 8 Then Array.Resize(tmp2, 8)
            If value.Length < 8 Then Array.Resize(value, 8)
            If value_r.Length < 8 Then Array.Resize(value_r, 8)
            If Not BitConverter.ToUInt64(value, 0) = BitConverter.ToUInt64(tmp2, 0) Then
                If Not BitConverter.ToUInt64(value_r, 0) = BitConverter.ToUInt64(tmp2, 0) Then
                    Step6_DataGridView1.Rows.RemoveAt(i)
                End If
            End If
        Next

        'deactivate_autosize_columns(False)
        Step6_LabelTotal.Text = "Total: " + Step6_DataGridView1.Rows.Count.ToString
        step6_timer.Enabled = True
    End Sub

    Private Shared Function fnMemSearch(pid As Integer, phnd As IntPtr, pattern() As Integer, patternsize As Integer, ByRef l As Integer) As returndata
        Dim retDataInt(90000) As Integer
        Dim retDataLong(90000) As Long
        Dim retData As returndata = New returndata
        Try
            If Not System.Environment.Is64BitOperatingSystem Then
                retData.retType = 0
                WinAPI.fnmemsearch(pid, pattern, patternsize, l, retDataInt)
                retData.retInt = retDataInt
            Else
                Dim check, ret As Boolean
                ret = WinAPI.IsWow64Process(phnd, check)
                If check Then
                    retData.retType = 0
                    WinAPI.fnmemsearch_x32_on_x64(pid, pattern, patternsize, l, retDataInt)
                    retData.retInt = retDataInt
                Else
                    retData.retType = 1
                    WinAPI.fnmemsearch_x64(pid, pattern, patternsize, l, retDataLong)
                    retData.retLong = retDataLong
                End If
            End If
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
        Return retData
    End Function

    Private Shared Function fnMemSearchPointer(pid As Integer, phnd As IntPtr, valmin As ULong, valmax As ULong, ByRef l As Integer) As returndata
        Dim retDataInt(90000) As Integer
        Dim retDataLong(90000) As Long
        Dim retData As returndata = New returndata
        Try
            If Not System.Environment.Is64BitOperatingSystem Then
                'p_is64bit = False
                retData.retType = 0
                ''''TODO
            Else
                Dim check, ret As Boolean
                ret = WinAPI.IsWow64Process(phnd, check)
                If check Then
                    'p_is64bit = False
                    retData.retType = 0
                    WinAPI.fnmemsearch_pointer_x32_on_x64(pid, CUInt(valmin), CUInt(valmax), l, retDataInt)
                    retData.retInt = retDataInt
                Else
                    'p_is64bit = True
                    retData.retType = 1
                    WinAPI.fnmemsearch_pointer_x64(pid, valmin, valmax, l, retDataLong)
                    retData.retLong = retDataLong
                End If
            End If
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
        Return retData
    End Function

    Private Sub deactivate_autosize_columns_alt(deactivate As Boolean)
        If deactivate Then
            Step6_DataGridView1.Columns(0).AutoSizeMode = DataGridViewAutoSizeColumnMode.None
            Step6_DataGridView1.Columns(1).AutoSizeMode = DataGridViewAutoSizeColumnMode.None
            Step6_DataGridView1.Columns(2).AutoSizeMode = DataGridViewAutoSizeColumnMode.None
            Step6_DataGridView1.Columns(3).AutoSizeMode = DataGridViewAutoSizeColumnMode.None
            Step6_DataGridView1.Columns(4).AutoSizeMode = DataGridViewAutoSizeColumnMode.None
            Step6_DataGridView1.Columns(5).AutoSizeMode = DataGridViewAutoSizeColumnMode.None
        Else
            Step6_DataGridView1.Columns(0).AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            Step6_DataGridView1.Columns(1).AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            Step6_DataGridView1.Columns(2).AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            Step6_DataGridView1.Columns(3).AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            Step6_DataGridView1.Columns(5).AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
        End If
    End Sub

    Private Sub Step6_refresh_table() Handles step6_timer.Tick
        If Step6_TextBox1.Text.Length = 0 Then Exit Sub
        If Step6_TextBox1.Text.Length Mod 4 <> 0 Then Exit Sub
        If curStep = 6 Then
            If p.HasExited Then WaitForProcess()
            'deactivate_autosize_columns(True)
            For Each r As DataGridViewRow In Step6_DataGridView1.Rows
                'Dim val As String = Step6_getDataForRow(r.Index)
                'r.Cells(2).Value = val
                Step6_getDataForRow(r)
            Next
        End If
    End Sub

    Private Sub Step6_getDataForRow(row As DataGridViewRow) 'As String
        If row.Cells(3).Value.ToString().ToUpper.StartsWith("KEEP") Then Exit Sub

        Dim out As Integer
        Dim item_addr As String = row.Cells(1).Value.ToString.Replace("-R", "")
        Dim item_type As String = row.Cells(3).Value.ToString
        Dim tmp2((Step6_TextBox1.Text.Length \ 2) - 1) As Byte
        Dim plus As String = ""


        Dim calculatedByUniqueBlockSize As Boolean = False
        If Step6_CheckBox2.Checked And item_type.StartsWith("Unique Block Size:") Then
            'Calculate addr based on Unique Block Size
            Dim tmp As String = ""
            Dim blockAddr As ULong = 2
            Dim blockSize As ULong = 0
            Dim offset As ULong = 0

            tmp = item_type.Replace("Unique Block Size:", "")
            blockSize = CULng("&H" + tmp.Substring(0, tmp.IndexOf("+")).Trim)
            offset = CULng("&H" + item_type.Substring(item_type.IndexOf("+") + 1).Trim)
            For Each block In blocks_unique
                If block.Value = blockSize Then blockAddr = block.Key : Exit For
            Next
            If blockAddr <> 2 Then
                calculatedByUniqueBlockSize = True
                item_addr = Hex(blockAddr + offset)
            End If
        End If

        'Calculate module relative addre
        If Step6_CheckBox1.Checked And Not calculatedByUniqueBlockSize Then
            If Not row.Cells(4).Value Is Nothing Then
                Dim modul As String = row.Cells(4).Value.ToString.Trim
                If modul <> "" Then
                    Dim moduleName = modul.Substring(0, modul.IndexOf("("))
                    For Each range As mapped_File_range In mapped_files_ranges
                        If range.fileName = moduleName Then
                            Dim offset_str As String = modul.Substring(modul.IndexOf("(") + 1)
                            Dim offset As UInt64 = CULng("&H" + offset_str.Substring(0, offset_str.Length - 1))
                            item_addr = Hex((range.addrStart + offset).ToString)
                            Exit For
                        End If
                    Next
                End If
            End If
        End If

        Dim addrNum As New IntPtr(CLng("&H" + item_addr))

        If Not item_type.StartsWith("pointer") Then
            WinAPI.ReadProcessMemory(p.Handle, addrNum, tmp2, tmp2.Length, out)
        Else
            Dim l As Integer = row.Cells(5).Value.ToString.IndexOf(" ")
            If row.Cells(1).Value.ToString.Length > 8 Or l > 8 Then Array.Resize(tmp, 8) Else Array.Resize(tmp, 4)
            If item_type.IndexOf("+") >= 0 Then
                plus = item_type.Substring(item_type.IndexOf("+") + 2) : plus = plus.Substring(0, plus.IndexOf(" ")).Trim
            Else
                plus = item_type.Substring(item_type.IndexOf("-") + 2) : plus = "-" + plus.Substring(0, plus.IndexOf(" ")).Trim
            End If

            Dim plusNum As Integer = CInt(plus)
            WinAPI.ReadProcessMemory(p.Handle, addrNum, tmp, tmp.Length, out)

            If row.Cells(1).Value.ToString.Length > 8 Or l > 8 Then
                WinAPI.ReadProcessMemory(p.Handle, New IntPtr(BitConverter.ToInt64(tmp, 0) + plusNum), tmp2, tmp2.Length, out)
                row.Cells(5).Value = Hex(BitConverter.ToUInt64(tmp, 0)) + " + (" + plus + " ) = " + Hex(BitConverter.ToUInt64(tmp, 0) + plusNum)
            Else
                WinAPI.ReadProcessMemory(p.Handle, New IntPtr(BitConverter.ToUInt32(tmp, 0) + plusNum), tmp2, tmp2.Length, out)
                row.Cells(5).Value = Hex(BitConverter.ToUInt32(tmp, 0)) + " + (" + plus + " ) = " + Hex(BitConverter.ToUInt32(tmp, 0) + plusNum)
            End If
        End If
        Array.Reverse(tmp2)
        Array.Resize(tmp2, 8)
        row.Cells(2).Value = Hex(BitConverter.ToUInt64(tmp2, 0)).PadLeft(Step6_TextBox1.Text.Length, "0"c)
    End Sub

    Private Sub Step6_recalculateOffset_Click(sender As System.Object, e As System.EventArgs) Handles Step6_recalculateOffset.Click
        Dim offset As UInteger
        Try
            offset = CUInt("&H" + Step6_TextBoxOffset.Text)
        Catch ex As Exception
            MsgBox("Invalid offset value.") : Exit Sub
        End Try
        For rowIndex As Integer = Step6_DataGridView1.Rows.Count - 1 To 0 Step -1
            If Step6_DataGridView1.Rows(rowIndex).Cells(3).Value.ToString.StartsWith("pointer") Then
                Step6_DataGridView1.Rows.RemoveAt(rowIndex)
            Else
                Dim r As String = ""
                Dim row As DataGridViewRow = Step6_DataGridView1.Rows(rowIndex)
                If row.Cells(1).Value.ToString.Contains("-R") Then r = "-R"
                Dim addr As ULong = CULng("&H" + row.Cells(1).Value.ToString.Replace("-R", ""))

                'deal with relative addr: GET RELATIVE ADDR
                If Step6_CheckBox1.Checked Then
                    If Not row.Cells(4).Value Is Nothing Then
                        Dim modul As String = row.Cells(4).Value.ToString.Trim
                        If modul <> "" Then
                            Dim moduleName = modul.Substring(0, modul.IndexOf("("))
                            For Each range As mapped_File_range In mapped_files_ranges
                                If range.fileName = moduleName Then
                                    Dim offset_str As String = modul.Substring(modul.IndexOf("(") + 1)
                                    Dim offset_rel As UInt64 = CULng("&H" + offset_str.Substring(0, offset_str.Length - 1))
                                    addr = range.addrStart + offset_rel
                                    Exit For
                                End If
                            Next
                        End If
                    End If
                End If

                addr = addr - offset
                Step6_DataGridView1.Rows(rowIndex).Cells(1).Value = Hex(addr) + r

                'deal with relative addr: SET RELATIVE ADDR
                For Each range As mapped_File_range In mapped_files_ranges
                    If addr >= range.addrStart And addr < range.addrEnd Then
                        row.Cells(4).Value = range.fileName + "(+" + Hex(addr - range.addrStart) + ")"
                    End If
                Next

                'Recalculate unique blocks
                For Each block In blocks_unique
                    If addr >= block.Key And addr < block.Key + block.Value Then
                        row.Cells(3).Value = "Unique Block Size: " + Hex(block.Value) + " + " + Hex(addr - block.Key)
                    End If
                Next
            End If
        Next
        recalculate_pointers()
        Step6_TextBoxOffset.Text = "0"
    End Sub

    Private Sub Step6_ButtonPreset_MouseClick(sender As Object, e As System.Windows.Forms.MouseEventArgs) Handles _
    Step6_ButtonPreset01.MouseDown, Step6_ButtonPreset02.MouseDown, Step6_ButtonPreset03.MouseDown, _
    Step6_ButtonPreset04.MouseDown, Step6_ButtonPreset05.MouseDown, Step6_ButtonPreset06.MouseDown, Step6_ButtonPreset07.MouseDown

        Dim n As String = DirectCast(sender, Button).Name.Replace("Step6_ButtonPreset", "")
        If Not FileIO.FileSystem.DirectoryExists(".\EmulatorConfigPresets") Then FileIO.FileSystem.CreateDirectory(".\EmulatorConfigPresets")
        If e.Button = Windows.Forms.MouseButtons.Left Then
            If Not FileIO.FileSystem.FileExists(".\EmulatorConfigPresets\Addr" + processCrc.ToUpper.Trim + "_" + n + ".txt") Then Exit Sub
            Step6_DataGridView1.Rows.Clear() : Step6_insert_current_config_row()
            FileOpen(10, ".\EmulatorConfigPresets\Addr" + processCrc.ToUpper.Trim + "_" + n + ".txt", OpenMode.Input)
            Dim s() As String
            Do While Not EOF(10)
                s = LineInput(10).Split({":::"}, StringSplitOptions.None)
                If s(0).ToUpper.StartsWith("VALUE_TO_SEARCH") Then Step6_TextBox1.Text = s(0).Substring(s(0).IndexOf("=") + 1).Trim : Continue Do
                If s(0).ToUpper.StartsWith("MAX_OFFSET") Then Step6_TextBox2.Text = s(0).Substring(s(0).IndexOf("=") + 1).Trim : Continue Do
                If s(0).ToUpper.StartsWith("OFFSET_IN_EMULATED_SYSTEM") Then Step6_TextBoxOffset.Text = s(0).Substring(s(0).IndexOf("=") + 1).Trim : Continue Do
                If s(0).ToUpper.StartsWith("ADDITIONAL_ACTION_TYPE") Then Step6_AdditionalActionsComboBox.Text = s(0).Substring(s(0).IndexOf("=") + 1).Trim : Continue Do
                If s(0).ToUpper.StartsWith("ADDITIONAL_ACTION_VALUE") Then Step6_AdditionalActionsTextBox.Text = s(0).Substring(s(0).IndexOf("=") + 1).Trim : Continue Do
                If s(0).ToUpper.StartsWith("USE_RELATIVE_MODULE_ADDR") Then
                    If s(0).Substring(s(0).IndexOf("=") + 1).Trim = "0" Then Step6_CheckBox1.Checked = False Else Step6_CheckBox1.Checked = True
                    Continue Do
                End If
                If s(0).ToUpper.StartsWith("USE_FIND_BLOCK_BY_SIZE") Then
                    If s(0).Substring(s(0).IndexOf("=") + 1).Trim = "0" Then Step6_CheckBox2.Checked = False Else Step6_CheckBox2.Checked = True
                    Continue Do
                End If

                Step6_DataGridView1.Rows.Add(s(0), s(1), s(2), s(3), s(4), s(5))
            Loop
            FileClose(10)
            step6_timer.Enabled = True
            Step6_LabelTotal.Text = "Total: " + Step6_DataGridView1.Rows.Count.ToString
        ElseIf e.Button = Windows.Forms.MouseButtons.Right Then
            FileOpen(10, ".\EmulatorConfigPresets\Addr" + processCrc.ToUpper.Trim + "_" + n + ".txt", OpenMode.Output)
            For Each r As DataGridViewRow In Step6_DataGridView1.Rows
                PrintLine(10, r.Cells(0).Value.ToString + ":::" + r.Cells(1).Value.ToString + ":::" + r.Cells(2).Value.ToString + ":::" + _
                           r.Cells(3).Value.ToString + ":::" + r.Cells(4).Value.ToString + ":::" + r.Cells(5).Value.ToString)
            Next
            PrintLine(10, "Value_to_Search = " + Step6_TextBox1.Text)
            PrintLine(10, "Max_Offset = " + Step6_TextBox2.Text)
            PrintLine(10, "Offset_In_Emulated_System = " + Step6_TextBoxOffset.Text)
            PrintLine(10, "Additional_Action_Type = " + Step6_AdditionalActionsComboBox.Text)
            PrintLine(10, "Additional_Action_Value = " + Step6_AdditionalActionsTextBox.Text)
            If Step6_CheckBox1.Checked Then PrintLine(10, "USE_RELATIVE_MODULE_ADDR = 1") Else PrintLine(10, "USE_RELATIVE_MODULE_ADDR = 0")
            If Step6_CheckBox2.Checked Then PrintLine(10, "USE_FIND_BLOCK_BY_SIZE = 1") Else PrintLine(10, "USE_RELATIVE_MODULE_ADDR = 0")
            FileClose(10)
            Dim newBold As New Font(DirectCast(sender, Button).Font.FontFamily, DirectCast(sender, Button).Font.Size, FontStyle.Bold)
            DirectCast(sender, Button).ForeColor = Color.Green
            DirectCast(sender, Button).Font = newBold
        End If
    End Sub

    Private Sub Step6_DataGridView1_CellContentDoubleClick(sender As Object, e As System.Windows.Forms.DataGridViewCellEventArgs) Handles Step6_DataGridView1.CellContentDoubleClick
        Step6_TextBox1.Text = Step6_DataGridView1.Rows(e.RowIndex).Cells(2).Value.ToString
    End Sub

    Private Sub Step6_AdditionalActionsComboBox_SelectedIndexChanged(sender As System.Object, e As System.EventArgs) Handles Step6_AdditionalActionsComboBox.SelectedIndexChanged
        If Step6_AdditionalActionsComboBox.SelectedIndex = 0 Then Step6_AdditionalActionsTextBox.Enabled = False Else Step6_AdditionalActionsTextBox.Enabled = True
    End Sub

    Private Sub Step6_setRam1_Click(sender As System.Object, e As System.EventArgs) Handles Step6_setRam1.Click, Step6_setRam2.Click
        If Step6_DataGridView1.SelectedRows.Count = 0 Then MsgBox("You have to select an address first.") : Exit Sub

        Dim b As Button = DirectCast(sender, Button)
        If Step6_AdditionalActionsComboBox.SelectedIndex > 0 Then
            Try
                If b.Text.StartsWith("SET RAM 1") Then
                    setRam(0) = CULng(Step6_AdditionalActionsComboBox.SelectedIndex)
                    setRam(1) = CULng("&H" + Step6_AdditionalActionsTextBox.Text)
                Else
                    setRam(2) = CULng(Step6_AdditionalActionsComboBox.SelectedIndex)
                    setRam(3) = CULng("&H" + Step6_AdditionalActionsTextBox.Text)
                End If
            Catch ex As Exception
                MsgBox("Invalid value in additional action.") : Exit Sub
            End Try
        End If

        b.Tag = Step6_DataGridView1.SelectedRows(0)
        If b.Text.StartsWith("SET RAM 1") Then b.Text = "SET RAM 1 (set)"
        If b.Text.StartsWith("SET RAM 2") Then b.Text = "SET RAM 2 (set)"
    End Sub

    Private Sub Step6_tryToLoadPreviousConfigIntoTable()
        zeroAddrExistingSettings = {"", "", "", "", "", ""}
        If updatingEmulatorVersionNode Is Nothing Then Exit Sub
        Dim addr_str As String = ""
        Dim pointer_str As String = ""
        Dim modul_str As String = ""
        Dim hex_op As String = "+ 0"
        Dim hex_op2 As String = "+ 0"
        Dim expression As String = ""

        Dim arr() As String = {"", "2"}
        For Each t As String In arr
            If updatingEmulatorVersionNode.SelectSingleNode("baseaddr" + curSystem + t) Is Nothing Then Continue For
            expression = updatingEmulatorVersionNode.SelectSingleNode("baseaddr" + curSystem + t).InnerText.ToLower.Trim

            Dim indexFirst As Integer = 0
            If t = "2" Then indexFirst = 3


            'First part of hex add/substract
            If expression.StartsWith("hexadd") Then
                Dim QuoterLast As Integer = expression.LastIndexOf("""")
                Dim QuoterFirst As Integer = expression.LastIndexOf("""", QuoterLast - 1)
                hex_op = "+ " + expression.Substring(QuoterFirst + 1, QuoterLast - QuoterFirst - 1)

                expression = replaceFirst(expression, "hexadd", "")
                expression = replaceFirst(expression, "(", "")
                expression = expression.Substring(0, expression.LastIndexOf(";")).Trim
            ElseIf expression.StartsWith("hexsubstract") Then
                Dim QuoterLast As Integer = expression.LastIndexOf("""")
                Dim QuoterFirst As Integer = expression.LastIndexOf("""", QuoterLast - 1)
                hex_op = "- " + expression.Substring(QuoterFirst + 1, QuoterLast - QuoterFirst - 1)

                expression = replaceFirst(expression, "hexsubstract", "")
                expression = replaceFirst(expression, "(", "")
                expression = expression.Substring(0, expression.LastIndexOf(";")).Trim
            End If

            'Seconf part of hex add/substract
            If expression.StartsWith("hexadd") Then
                Dim QuoterLast As Integer = expression.LastIndexOf("""")
                Dim QuoterFirst As Integer = expression.LastIndexOf("""", QuoterLast - 1)
                hex_op2 = "+ " + expression.Substring(QuoterFirst + 1, QuoterLast - QuoterFirst - 1)

                expression = replaceFirst(expression, "hexadd", "")
                expression = replaceFirst(expression, "(", "")
                expression = expression.Substring(0, expression.LastIndexOf(";")).Trim
            ElseIf expression.StartsWith("hexsubstract") Then
                Dim QuoterLast As Integer = expression.LastIndexOf("""")
                Dim QuoterFirst As Integer = expression.LastIndexOf("""", QuoterLast - 1)
                hex_op2 = "- " + expression.Substring(QuoterFirst + 1, QuoterLast - QuoterFirst - 1)

                expression = replaceFirst(expression, "hexsubstract", "")
                expression = replaceFirst(expression, "(", "")
                expression = expression.Substring(0, expression.LastIndexOf(";")).Trim
            End If

            If hex_op <> "+ 0" And hex_op2 <> "+ 0" Then
                'if both of hex_op are set, then use first for "additional operation" and second for pointer
                If hex_op.StartsWith("+") Then Step6_AdditionalActionsComboBox.Text = "HexAdd"
                If hex_op.StartsWith("-") Then Step6_AdditionalActionsComboBox.Text = "HexSubstract"
                Step6_AdditionalActionsTextBox.Text = hex_op.Substring(2)
                hex_op = hex_op2(2)
            ElseIf hex_op <> "+ 0" And Not expression.StartsWith("frompointer") Then
                'if one hex_op is set, but the next expression is not a pointer, then use it as "additional operation"
                If hex_op.StartsWith("+") Then Step6_AdditionalActionsComboBox.Text = "HexAdd"
                If hex_op.StartsWith("-") Then Step6_AdditionalActionsComboBox.Text = "HexSubstract"
                Step6_AdditionalActionsTextBox.Text = hex_op.Substring(2)
            End If

            If expression.StartsWith("frompointer") Then
                expression = replaceFirst(expression, "frompointer", "")
                expression = replaceFirst(expression, "(", "")
                expression = replaceLast(expression, ")", "").Trim


                pointer_str = "pointer( " + hex_op.Substring(0, 1) + " " + CUInt("&H" + hex_op.Substring(2)).ToString + " )"
            ElseIf expression.StartsWith("findblockbySize") Then
                Dim QuoterLast As Integer = expression.LastIndexOf("""")
                Dim QuoterFirst As Integer = expression.LastIndexOf("""", QuoterLast - 1)
                Dim offset As String = expression.Substring(QuoterFirst + 1, QuoterLast - QuoterFirst - 1)

                QuoterLast = expression.LastIndexOf("""", QuoterFirst - 1)
                QuoterFirst = expression.LastIndexOf("""", QuoterLast - 1)
                pointer_str = "Unique Block Size: " + expression.Substring(QuoterFirst + 1, QuoterLast - QuoterFirst - 1) + " + " + offset

                Step6_CheckBox2.Checked = True
                expression = replaceFirst(expression, "findblockbySize", "")
                expression = replaceFirst(expression, "(", "")
                expression = expression.Substring(0, expression.LastIndexOf(";")).Trim
            ElseIf expression.StartsWith("fromaddr") Then
                expression = replaceFirst(expression, "fromaddr", "")
                expression = replaceFirst(expression, "(", "")
                expression = replaceLast(expression, ")", "").Trim

                pointer_str = ""
            End If

            If expression.StartsWith("baseaddressof") Then
                Dim QuoterLast As Integer = expression.LastIndexOf("""")
                Dim QuoterFirst As Integer = expression.LastIndexOf("""", QuoterLast - 1)
                Dim offset As String = expression.Substring(QuoterFirst + 1, QuoterLast - QuoterFirst - 1)

                QuoterLast = expression.LastIndexOf("""", QuoterFirst - 1)
                QuoterFirst = expression.LastIndexOf("""", QuoterLast - 1)
                modul_str = expression.Substring(QuoterFirst + 1, QuoterLast - QuoterFirst - 1) + "(+" + offset + ")"

                Step6_CheckBox1.Checked = True
                expression = replaceFirst(expression, "baseaddress", "")
                expression = replaceFirst(expression, "(", "")
                expression = expression.Substring(0, expression.LastIndexOf(";")).Trim
            ElseIf expression.StartsWith("baseaddress") Then
                Dim QuoterLast As Integer = expression.LastIndexOf("""")
                Dim QuoterFirst As Integer = expression.LastIndexOf("""", QuoterLast - 1)
                Dim offset As String = expression.Substring(QuoterFirst + 1, QuoterLast - QuoterFirst - 1)
                modul_str = p.MainModule.FileName.Substring(p.MainModule.FileName.LastIndexOf("\") + 1) + "(+" + offset + ")"

                Step6_CheckBox1.Checked = True
                expression = replaceFirst(expression, "baseaddress", "")
                expression = replaceFirst(expression, "(", "")
                expression = replaceLast(expression, ")", "").Trim
            ElseIf Not expression.Contains("(") And Not expression.Contains(")") Then
                Step6_CheckBox1.Checked = False
                Dim QuoterLast As Integer = expression.LastIndexOf("""")
                Dim QuoterFirst As Integer = expression.LastIndexOf("""", QuoterLast - 1)
                addr_str = expression.Substring(QuoterFirst + 1, QuoterLast - QuoterFirst - 1)
            Else
                zeroAddrExistingSettings(indexFirst) = ""
                zeroAddrExistingSettings(indexFirst + 1) = "Keep current Config"
                zeroAddrExistingSettings(indexFirst + 2) = ""
            End If
            zeroAddrExistingSettings(indexFirst) = addr_str
            zeroAddrExistingSettings(indexFirst + 1) = pointer_str
            zeroAddrExistingSettings(indexFirst + 2) = modul_str
        Next
    End Sub

    Private Sub Step6_insert_current_config_row()
        If Step6_DataGridView1.Rows.Count <> 0 Then Exit Sub
        For i As Integer = 0 To 3 Step 3
            If zeroAddrExistingSettings(i) <> "" Or zeroAddrExistingSettings(i + 2) <> "" Then

                Step6_DataGridView1.Rows.Add({0, zeroAddrExistingSettings(i), "", zeroAddrExistingSettings(i + 1), zeroAddrExistingSettings(i + 2), ""})
                If zeroAddrExistingSettings(i + 1).ToUpper.StartsWith("KEEP") Then Exit Sub
                Dim r As DataGridViewRow = Step6_DataGridView1.Rows(0)
                Dim item_type As String = r.Cells(3).Value.ToString

                'Calculate addr based on Unique Block Size
                Dim calculatedByUniqueBlockSize As Boolean = False
                If Step6_CheckBox2.Checked And item_type.StartsWith("Unique Block Size:") Then
                    Dim tmp As String = ""
                    Dim blockAddr As ULong = 2
                    Dim blockSize As ULong = 0
                    Dim offset As ULong = 0

                    tmp = item_type.Replace("Unique Block Size:", "")
                    blockSize = CULng("&H" + tmp.Substring(0, tmp.IndexOf("+")).Trim)
                    offset = CULng("&H" + item_type.Substring(item_type.IndexOf("+") + 1).Trim)
                    For Each block In blocks_unique
                        If block.Value = blockSize Then blockAddr = block.Key : Exit For
                    Next
                    If blockAddr <> 2 Then
                        calculatedByUniqueBlockSize = True
                        r.Cells(1).Value = Hex(blockAddr + offset)
                    End If
                End If

                'Calculate address if there is no one
                If Step6_CheckBox1.Checked And Not calculatedByUniqueBlockSize Then
                    If Not r.Cells(4).Value Is Nothing Then
                        Dim modul As String = r.Cells(4).Value.ToString.Trim
                        If modul <> "" Then
                            Dim moduleName = modul.Substring(0, modul.IndexOf("("))
                            For Each range As mapped_File_range In mapped_files_ranges
                                If range.fileName = moduleName Then
                                    Dim offset_str As String = modul.Substring(modul.IndexOf("(") + 1)
                                    Dim offset As UInt64 = CULng("&H" + offset_str.Substring(0, offset_str.Length - 1))
                                    r.Cells(1).Value = Hex((range.addrStart + offset).ToString)
                                    Exit For
                                End If
                            Next
                        End If
                    End If
                End If


                'Update module info if there is no any
                Dim curAddr As UInt64 = CULng("&H" + r.Cells(1).Value.ToString.Replace("-R", ""))
                If zeroAddrExistingSettings(i + 2) = "" Then
                    For Each range As mapped_File_range In mapped_files_ranges
                        If curAddr >= range.addrStart And curAddr < range.addrEnd Then
                            r.Cells(4).Value = range.fileName + "(+" + Hex(curAddr - range.addrStart) + ")"
                        End If
                    Next

                End If
                'Update unique block size if there is no any
                If zeroAddrExistingSettings(i + 1) = "" Then
                    For Each block In blocks_unique
                        If curAddr >= block.Key And curAddr < block.Key + block.Value Then
                            r.Cells(3).Value = "Unique Block Size: " + Hex(block.Value) + " + " + Hex(curAddr - block.Key)
                        End If
                    Next
                End If

                If i = 0 And Step6_setRam1.Visible Then Step6_DataGridView1.Rows(0).Selected = True : Step6_setRam1_Click(Step6_setRam1, New EventArgs) : Step6_DataGridView1.Rows(0).Selected = False
                If i = 3 And Step6_setRam2.Visible Then Step6_DataGridView1.Rows(1).Selected = True : Step6_setRam1_Click(Step6_setRam2, New EventArgs) : Step6_DataGridView1.Rows(1).Selected = False
            End If
        Next
        step6_timer.Enabled = True
    End Sub
#End Region

#Region "Step7 - Final step, test and save config"
    Dim baseAddrExpr As String = ""
    Dim baseAddrExpr2 As String = ""
    Dim romExpressionString As String = ""
    Dim UpdateDrawMode As Boolean = False
    Dim UpdateQuickSaveSchema As Boolean = False
    Private Sub Step7_init()
        'Calculate romname expression
        If Step3_romnameFromTitle.Checked Then
            romExpressionString = Step51_TextBox2.Text
        Else
            romExpressionString = Calculate_romname_expression(Step5_DataGridView3.SelectedRows(0))
        End If

        'Calculate baseaddr expression
        If Step6_setRam1.Visible Then
            Dim curSelInd As Integer = Step6_AdditionalActionsComboBox.SelectedIndex
            Dim curVal As String = Step6_AdditionalActionsTextBox.Text
            Step6_AdditionalActionsComboBox.SelectedIndex = CInt(setRam(0)) : Step6_AdditionalActionsTextBox.Text = Hex(setRam(1))
            baseAddrExpr = Calculate_baseaddr_expression(DirectCast(Step6_setRam1.Tag, DataGridViewRow))
            Step6_AdditionalActionsComboBox.SelectedIndex = CInt(setRam(2)) : Step6_AdditionalActionsTextBox.Text = Hex(setRam(3))
            baseAddrExpr2 = Calculate_baseaddr_expression(DirectCast(Step6_setRam2.Tag, DataGridViewRow))
            Step6_AdditionalActionsComboBox.SelectedIndex = curSelInd : Step6_AdditionalActionsTextBox.Text = curVal
        Else
            baseAddrExpr = Calculate_baseaddr_expression(Step6_DataGridView1.SelectedRows(0))
        End If

        'Fill xml node preview
        'Step7_textbox1.Text = "<version ver=""" + Step3_TextBox3.Text + """>"
        'Step7_textbox1.Text += vbCrLf + "   <crc>" + processCrc.ToUpper.Trim() + "</crc>"
        'Step7_textbox1.Text += vbCrLf + "   <baseaddr" + curSystem + ">" + baseAddrExpr + "</baseaddr" + curSystem + ">"
        'If Step6_setRam1.Visible Then Step7_textbox1.Text += vbCrLf + "   <baseaddr" + curSystem + "2>" + baseAddrExpr2 + "</baseaddr" + curSystem + "2>"
        'Step7_textbox1.Text += vbCrLf + "   <romname>" + romExpressionString + "</romname>"
        'Step7_textbox1.Text += vbCrLf + "</version>"

        'CustomFunctions.p = p
        'Dim detect As String = romExpressionString.Replace("!=", "<>")
        'Dim econtext As New Ciloci.Flee.ExpressionContext(New CustomFunctions)
        'Dim e As Ciloci.Flee.IGenericExpression(Of String) = econtext.CompileGeneric(Of String)(detect)
        'Label35.Text = "Current romname: " + e.Evaluate

        Try
            Dim x() As Xml.XmlNode = UpdateConfig()
            fill_xml_preview(x)
            m = New Class5_MemoryAccess
            m.setup(p, x(0), x(1), True)
            Label39.Text = "Current system: " + m.getMashineType()
            If System.Text.RegularExpressions.Regex.Match(m.getRomName(), "\p{C}") Is System.Text.RegularExpressions.Match.Empty Then
                Label35.Text = "Current romname: " + m.rName
            Else
                Label35.Text = "Current romname: CONTAINS ILLEGAL CHARACTERS"
                MsgBox("Evaluated romname contains no utf characters. Please recheck your romName address.")
            End If
            Label42.Text = "Evaluated baseaddr:" + Hex(m.getBaseAddr)
            Label43.Text = "Evaluated baseaddr2:" + Hex(m.getBaseAddr2)
        Catch ex As Exception
            Label35.Text = "Current romname: Unknown"
            Label39.Text = "Current system: Unknown"
            Label42.Text = "Evaluated baseaddr: None"
            Label43.Text = "Evaluated baseaddr2: None"
            MsgBox("There is error somewhere in found addresses, memoryPatcher returns error.")
        End Try
    End Sub

    Private Function Calculate_romname_expression(r As DataGridViewRow) As String
        If r.Cells(3).Value.ToString.ToUpper.StartsWith("KEEP") Then
            Return updatingEmulatorVersionNode.SelectSingleNode("romname").InnerText
        End If

        Dim expr As String = ""
        Dim rom_addr As String = r.Cells(1).Value.ToString
        Dim pointer_str As String = r.Cells(3).Value.ToString
        expr = """" + rom_addr + """"

        If Step5_CheckBox2.Checked Then
            If Not r.Cells(4).Value Is Nothing Then
                Dim modul As String = r.Cells(4).Value.ToString.Trim
                If modul <> "" Then
                    Dim moduleName = modul.Substring(0, modul.IndexOf("("))
                    Dim offset_str As String = modul.Substring(modul.IndexOf("(") + 1)
                    Dim offset As UInt64 = CULng("&H" + offset_str.Substring(0, offset_str.Length - 1))
                    Dim exename = p.MainModule.FileName.Substring(p.MainModule.FileName.LastIndexOf("\") + 1)
                    If exename.ToLower = moduleName.ToLower Then
                        expr = " BaseAddress(""" + Hex(offset) + """)"
                    Else
                        expr = " BaseAddressOf(""" + moduleName + """; """ + Hex(offset) + """)"
                    End If
                End If
            End If
        End If

        If pointer_str.StartsWith("pointer") Then
            If Not System.Environment.Is64BitOperatingSystem Then
                expr = "fromPointer(" + expr + ")"
            Else
                Dim check, ret As Boolean
                ret = WinAPI.IsWow64Process(p.Handle, check)
                If check Then
                    expr = "fromPointer(" + expr + ")"
                Else
                    expr = "fromPointer32bit(" + expr + ")"
                End If
            End If

            pointer_str = pointer_str.Substring(pointer_str.IndexOf("(") + 1).Trim
            pointer_str = pointer_str.Substring(0, pointer_str.IndexOf(")")).Trim
            If pointer_str.Trim <> "" And pointer_str.Trim <> "0" Then
                If pointer_str.StartsWith("-") Then
                    pointer_str = pointer_str.Replace("-", "").Trim
                    expr = "hexSubstract(" + expr + "; """ + pointer_str + """ )"
                Else
                    expr = "hexAdd(" + expr + "; """ + pointer_str + """ )"
                End If
            End If
        End If
        If Step5_RadioButton1.Checked Then expr = "readNByteFrom( " + expr + "; " + Step5_NumericUpDown1.Value.ToString + ")"
        If Step5_RadioButton2.Checked Then expr = "readFromTo( " + expr + "; ""  "")"
        If Step5_RadioButton3.Checked Then expr = "readFromTo( " + expr + "; """ + Step5_TextBox2.Text + """ )"
        If Step5_CheckBox1.Checked Then expr = "removeExtension( " + expr + " )"
        Return expr
    End Function

    Private Function Calculate_baseaddr_expression(addrRow As DataGridViewRow) As String
        If addrRow.Cells(3).Value.ToString.ToUpper.StartsWith("KEEP") Then
            Return updatingEmulatorVersionNode.SelectSingleNode("baseaddr" + curSystem).InnerText
        End If

        Dim baseAddrExprTmp As String = ""
        Dim item_addr As String = """" + addrRow.Cells(1).Value.ToString.Replace("-R", "") + """"
        Dim item_type As String = addrRow.Cells(3).Value.ToString

        If Step6_CheckBox1.Checked Then
            If Not addrRow.Cells(4).Value Is Nothing Then
                Dim modul As String = addrRow.Cells(4).Value.ToString.Trim
                If modul <> "" Then
                    Dim moduleName = modul.Substring(0, modul.IndexOf("("))
                    Dim offset_str As String = modul.Substring(modul.IndexOf("(") + 1)
                    Dim offset As UInt64 = CULng("&H" + offset_str.Substring(0, offset_str.Length - 1))
                    Dim exename = p.MainModule.FileName.Substring(p.MainModule.FileName.LastIndexOf("\") + 1)
                    If exename.ToLower = moduleName.ToLower Then
                        item_addr = " BaseAddress(""" + Hex(offset) + """)"
                    Else
                        item_addr = " BaseAddressOf(""" + moduleName + """; """ + Hex(offset) + """)"
                    End If
                End If
            End If
        End If
        If item_type.StartsWith("pointer") Then
            Dim plus As String = ""
            If item_type.IndexOf("+") >= 0 Then
                plus = item_type.Substring(item_type.IndexOf("+") + 2) : plus = plus.Substring(0, plus.IndexOf(" ")).Trim
            Else
                plus = item_type.Substring(item_type.IndexOf("-") + 2) : plus = "-" + plus.Substring(0, plus.IndexOf(" ")).Trim
            End If

            If plus = "0" Then
                baseAddrExprTmp = "fromPointer(" + item_addr + ")"
            Else
                If plus.StartsWith("-") Then
                    plus = plus.Replace("-", "").Trim
                    baseAddrExprTmp = "hexSubstract( fromPointer(" + item_addr + "); """ + Hex(CInt(plus)) + """)"
                Else
                    baseAddrExprTmp = "hexAdd( fromPointer(" + item_addr + "); """ + Hex(CInt(plus)) + """)"
                End If
            End If
        ElseIf item_type.StartsWith("Unique Block Size") And Step6_CheckBox2.Checked Then
            Dim tmp As String = ""
            Dim blockSize As String
            Dim offset As String
            tmp = item_type.Replace("Unique Block Size:", "")
            blockSize = tmp.Substring(0, tmp.IndexOf("+")).Trim
            offset = item_type.Substring(item_type.IndexOf("+") + 1).Trim
            baseAddrExprTmp = "findBlockBySize(""" + blockSize + """; """ + offset + """ )"
        Else
            baseAddrExprTmp = "fromAddr(" + item_addr + ")"
        End If

        If Step6_AdditionalActionsComboBox.SelectedIndex <> 0 Then
            baseAddrExprTmp = Step6_AdditionalActionsComboBox.SelectedItem.ToString + "( " + baseAddrExprTmp + "; """ + Step6_AdditionalActionsTextBox.Text + """ )"
        End If
        Return baseAddrExprTmp
    End Function

    'Update config
    Private Function UpdateConfig() As Xml.XmlNode()
        Dim x() As Xml.XmlNode = {Nothing, Nothing}
        Dim emuFoundInConfig As Boolean = False
        Dim emuFoundInConfigRightVersion As Boolean = False

        'find pause code
        Dim pauseCode As Long = 0
        For Each k As Windows.Forms.Keys In [Enum].GetValues(Windows.Forms.Keys.KeyCode.GetType)
            If k.ToString = Step3_TextBox4.Text Then pauseCode = k : Exit For
        Next

        'Update config
        For Each emuNode As Xml.XmlNode In xmlConfig.SelectNodes("/config/emulator")
            If emuNode.SelectSingleNode("exe").InnerText.ToUpper.Trim = p.ProcessName.ToUpper.Trim Then
                x(0) = emuNode
                emuFoundInConfig = True

                'Get current draw method and options from config
                getDrawMethod(emuNode.SelectSingleNode("drawmethod"))

                'Update pausebutton in config
                If emuNode.SelectSingleNode("pausekey") Is Nothing Then
                    If pauseCode <> 0 Then
                        Dim newPauseKeyNode As Xml.XmlElement = xmlConfig.CreateElement("pausekey")
                        newPauseKeyNode.InnerText = Hex(pauseCode)
                        emuNode.AppendChild(newPauseKeyNode)
                    End If
                Else
                    If pauseCode <> 0 Then
                        Dim newPauseKeyNode As Xml.XmlNode = emuNode.SelectSingleNode("pausekey")
                        newPauseKeyNode.InnerText = Hex(pauseCode)
                    Else
                        emuNode.RemoveChild(emuNode.SelectSingleNode("pausekey"))
                    End If
                End If

                'Update version node
                For Each verNode As Xml.XmlNode In emuNode.SelectNodes("versions/version")
                    If verNode.SelectSingleNode("crc").InnerText.ToUpper.Trim = processCrc.ToUpper.Trim Then
                        emuFoundInConfigRightVersion = True

                        'Get draw method version-node-ovverride and options from config
                        If verNode.SelectSingleNode("drawmethod") IsNot Nothing Then getDrawMethod(verNode.SelectSingleNode("drawmethod"))

                        'Updating existing version
                        verNode.Attributes("ver").Value = Step3_TextBox3.Text

                        Dim newRomNameNodeForUpdate As Xml.XmlNode = verNode.SelectSingleNode("romname")
                        newRomNameNodeForUpdate.InnerText = romExpressionString

                        Dim newBaseAddrNodeForUpdate As Xml.XmlNode = verNode.SelectSingleNode("baseaddr" + curSystem)
                        newBaseAddrNodeForUpdate.InnerText = baseAddrExpr

                        If Step6_setRam1.Visible Then
                            Dim newBaseAddr2NodeForUpdate As Xml.XmlNode = verNode.SelectSingleNode("baseaddr" + curSystem + "2")

                            'dirty hack, if system should have two 0address node, but have only one
                            If newBaseAddr2NodeForUpdate Is Nothing Then
                                newBaseAddr2NodeForUpdate = xmlConfig.CreateElement("baseaddr" + curSystem + "2")
                                verNode.AppendChild(newBaseAddr2NodeForUpdate)
                            End If
                            newBaseAddr2NodeForUpdate.InnerText = baseAddrExpr2
                        End If

                        x(1) = verNode
                        Exit For
                    End If
                Next

                'Adding new version
                If Not emuFoundInConfigRightVersion Then
                    Dim versionsNode As Xml.XmlNode = emuNode.SelectSingleNode("versions")
                    Dim newVersionNode As Xml.XmlElement = xmlConfig.CreateElement("version")
                    Dim newVersionNodeAttr As Xml.XmlAttribute = xmlConfig.CreateAttribute("ver")
                    versionsNode.AppendChild(newVersionNode)
                    newVersionNodeAttr.Value = Step3_TextBox3.Text
                    newVersionNode.Attributes.Append(newVersionNodeAttr)

                    Dim newCrcNode As Xml.XmlElement = xmlConfig.CreateElement("crc")
                    newCrcNode.InnerText = processCrc.ToUpper.Trim
                    newVersionNode.AppendChild(newCrcNode)

                    Dim newRomNameNode As Xml.XmlElement = xmlConfig.CreateElement("romname")
                    newRomNameNode.InnerText = romExpressionString
                    newVersionNode.AppendChild(newRomNameNode)

                    Dim newBaseAddrNode As Xml.XmlElement = xmlConfig.CreateElement("baseaddr" + curSystem)
                    newBaseAddrNode.InnerText = baseAddrExpr
                    newVersionNode.AppendChild(newBaseAddrNode)

                    If Step6_setRam1.Visible Then
                        Dim newBaseAddr2Node As Xml.XmlNode = xmlConfig.CreateElement("baseaddr" + curSystem + "2")
                        newBaseAddr2Node.InnerText = baseAddrExpr2
                        newVersionNode.AppendChild(newBaseAddr2Node)
                    End If

                    x(1) = newVersionNode
                End If
            End If
            If emuFoundInConfig Then Exit For
        Next

        If emuFoundInConfig = False Then
            'Adding new emulator node
            Dim c As Integer = xmlConfig.SelectNodes("/config/emulator").Count
            Dim emuNode As Xml.XmlNode = xmlConfig.CreateElement("emulator")

            Dim exeNode As Xml.XmlElement = xmlConfig.CreateElement("exe")
            exeNode.InnerText = p.ProcessName
            emuNode.AppendChild(exeNode)

            Dim pauseKeyNode As Xml.XmlNode = xmlConfig.CreateElement("pausekey")
            pauseKeyNode.InnerText = Hex(pauseCode)
            emuNode.AppendChild(pauseKeyNode)

            Dim quickSaveNode As Xml.XmlNode = xmlConfig.CreateElement("quicksave")
            quickSaveNode.InnerText = "1"
            emuNode.AppendChild(quickSaveNode)

            Dim drawMethodNode As Xml.XmlNode = xmlConfig.CreateElement("drawmethod")
            drawMethodNode.InnerText = "11"
            emuNode.AppendChild(drawMethodNode)
            UpdateDrawMode = False
            Step7_SelectDrawMethod.SelectedIndex = 5 : Step7_CheckBox1.Checked = False : Step7_CheckBox2.Checked = False : Step7_WindowClassTextBox.Text = ""
            UpdateDrawMode = True

            'VERSIONS NODE
            Dim versionsNode As Xml.XmlNode = xmlConfig.CreateElement("versions")
            emuNode.AppendChild(versionsNode)

            Dim newVersionNode As Xml.XmlElement = xmlConfig.CreateElement("version")
            Dim newVersionNodeAttr As Xml.XmlAttribute = xmlConfig.CreateAttribute("ver")
            versionsNode.AppendChild(newVersionNode)
            newVersionNodeAttr.Value = Step3_TextBox3.Text
            newVersionNode.Attributes.Append(newVersionNodeAttr)

            Dim newCrcNode As Xml.XmlElement = xmlConfig.CreateElement("crc")
            newCrcNode.InnerText = processCrc.ToUpper.Trim
            newVersionNode.AppendChild(newCrcNode)

            Dim newRomNameNode As Xml.XmlElement = xmlConfig.CreateElement("romname")
            newRomNameNode.InnerText = romExpressionString
            newVersionNode.AppendChild(newRomNameNode)

            Dim newBaseAddrNode As Xml.XmlElement = xmlConfig.CreateElement("baseaddr" + curSystem)
            newBaseAddrNode.InnerText = baseAddrExpr
            newVersionNode.AppendChild(newBaseAddrNode)

            If Step6_setRam1.Visible Then
                Dim newBaseAddr2Node As Xml.XmlNode = xmlConfig.CreateElement("baseaddr" + curSystem + "2")
                newBaseAddr2Node.InnerText = baseAddrExpr2
                newVersionNode.AppendChild(newBaseAddr2Node)
            End If

            'SYSTEM NODE
            Dim systemNode As Xml.XmlNode = xmlConfig.CreateElement("system")
            emuNode.AppendChild(systemNode)
            Dim systemNameNode As Xml.XmlNode = xmlConfig.CreateElement("name")
            systemNameNode.InnerText = curSystem
            systemNode.AppendChild(systemNameNode)

            xmlConfig.SelectSingleNode("/config").InsertAfter(emuNode, xmlConfig.SelectNodes("/config/emulator")(c - 1))
            x(0) = emuNode
            x(1) = newVersionNode
        End If
        Return x
        'Не обрабатывается добавление новой системы. Вообще никак и нигде.
    End Function

    'Fill xml preview
    Private Sub fill_xml_preview(x() As Xml.XmlNode)
        Dim curVal As String = ""

        Step7_textbox1.Text = ""
        If x(0).SelectSingleNode("exe") IsNot Nothing Then Step7_textbox1.Text += "<exe>" + x(0).SelectSingleNode("exe").InnerText + "</exe>" + vbCrLf
        If x(0).SelectSingleNode("pausekey") IsNot Nothing Then Step7_textbox1.Text += "<pausekey>" + x(0).SelectSingleNode("pausekey").InnerText + "</pausekey>" + vbCrLf
        If x(0).SelectSingleNode("quicksave") IsNot Nothing Then Step7_textbox1.Text += "<quicksave>" + x(0).SelectSingleNode("quicksave").InnerText + "</quicksave>" + vbCrLf
        If x(0).SelectSingleNode("drawmethod") IsNot Nothing Then Step7_textbox1.Text += x(0).SelectSingleNode("drawmethod").OuterXml + vbCrLf
        If x(0).SelectSingleNode("drawmethodfullscreen") IsNot Nothing Then Step7_textbox1.Text += x(0).SelectSingleNode("drawmethodfullscreen").OuterXml + vbCrLf

        If x(0).SelectNodes("versions/version").Count > 1 Then Step7_textbox1.Text += "<...otherVersionNodes... />" + vbCrLf

        Step7_textbox1.Text += "<version ver=""" + Step3_TextBox3.Text + """>" + vbCrLf
        Step7_textbox1.Text += "   <crc>" + processCrc.ToUpper.Trim() + "</crc>" + vbCrLf
        If x(1).SelectSingleNode("drawmethod") IsNot Nothing Then Step7_textbox1.Text += "   " + x(1).SelectSingleNode("drawmethod").OuterXml + vbCrLf
        If x(1).SelectSingleNode("drawmethodfullscreen") IsNot Nothing Then Step7_textbox1.Text += "   " + x(1).SelectSingleNode("drawmethodfullscreen").OuterXml + vbCrLf
        Step7_textbox1.Text += "   <baseaddr" + curSystem + ">" + baseAddrExpr + "</baseaddr" + curSystem + ">" + vbCrLf
        If Step6_setRam1.Visible Then Step7_textbox1.Text += "   <baseaddr" + curSystem + "2>" + baseAddrExpr2 + "</baseaddr" + curSystem + "2>" + vbCrLf
        Step7_textbox1.Text += "   <romname>" + romExpressionString + "</romname>" + vbCrLf
        Step7_textbox1.Text += "</version>"
    End Sub

    'Get current draw method from config and set interface
    Private Sub getDrawMethod(drawNode As Xml.XmlNode)
        UpdateDrawMode = False
        Dim curmode As Integer = CInt(drawNode.InnerText)
        If curmode = 10 Then
            Step7_SelectDrawMethod.SelectedIndex = 4
        ElseIf curmode = 11 Then
            Step7_SelectDrawMethod.SelectedIndex = 5
        Else
            Step7_SelectDrawMethod.SelectedIndex = curmode
        End If
        If drawNode.Attributes("backgroundSource") IsNot Nothing Then
            Step7_CheckBox1.Checked = True
            If drawNode.Attributes("backgroundSource").Value.ToLower = "screen" Then Step7_RadioButton1.Checked = True
            If drawNode.Attributes("backgroundSource").Value.ToLower = "hook" Then Step7_RadioButton2.Checked = True
        Else
            Step7_CheckBox1.Checked = False
        End If
        If drawNode.Attributes("hwnd") IsNot Nothing Then
            Step7_CheckBox2.Checked = True
            Step7_WindowClassTextBox.Text = drawNode.Attributes("hwnd").Value
        Else
            Step7_CheckBox2.Checked = False
            Step7_WindowClassTextBox.Text = ""
        End If
        UpdateDrawMode = True
    End Sub

    'Change drawMethod 
    Private Sub Step7_SelectDrawMethod_SelectedIndexChanged(sender As System.Object, e As System.EventArgs) Handles Step7_SelectDrawMethod.SelectedIndexChanged
        If UpdateDrawMode Then
            Dim curVerNode As Xml.XmlNode = Nothing
            Dim newSelectedDrawMode As String = Step7_SelectDrawMethod.SelectedItem.ToString.Substring(11, 2).Replace(":", "")
            Dim newSelectedBackgroundSource As String = ""
            If Step7_CheckBox1.Checked Then
                If Step7_RadioButton1.Checked Then newSelectedBackgroundSource = "screen"
                If Step7_RadioButton2.Checked Then newSelectedBackgroundSource = "hook"
            End If

            For Each emuNode As Xml.XmlNode In xmlConfig.SelectNodes("/config/emulator")
                If emuNode.SelectSingleNode("exe").InnerText.ToUpper.Trim = p.ProcessName.ToUpper.Trim Then
                    Dim drawMethodNode As Xml.XmlNode = emuNode.SelectSingleNode("drawmethod")
                    Dim drawMethodVersionNode As Xml.XmlNode = Nothing

                    'If emuNode.SelectSingleNode("versions").SelectNodes("version").Count > 1 Then
                    For Each verNode As Xml.XmlNode In emuNode.SelectNodes("versions/version")
                        If verNode.SelectSingleNode("crc").InnerText.ToUpper.Trim = processCrc.ToUpper.Trim Then
                            curVerNode = verNode
                            If verNode.SelectSingleNode("drawmethod") IsNot Nothing Then
                                drawMethodVersionNode = verNode.SelectSingleNode("drawmethod")
                            End If
                        End If
                    Next
                    'End If

                    'If current drawmode fully equal to default, we should delete vernode-override if it exist
                    If drawMethodNode.InnerText = newSelectedDrawMode Then
                        Dim attr_bckg As Xml.XmlAttribute = drawMethodNode.Attributes("backgroundSource")
                        Dim attr_hwnd As Xml.XmlAttribute = drawMethodNode.Attributes("hwnd")
                        If (Not Step7_CheckBox1.Checked And attr_bckg Is Nothing) Or (Step7_CheckBox1.Checked And attr_bckg IsNot Nothing AndAlso attr_bckg.Value = newSelectedBackgroundSource) Then
                            If (Not Step7_CheckBox2.Checked And attr_hwnd Is Nothing) Or (Step7_CheckBox2.Checked And attr_hwnd IsNot Nothing AndAlso attr_hwnd.Value = Step7_WindowClassTextBox.Text.Trim) Then
                                If drawMethodVersionNode IsNot Nothing Then drawMethodVersionNode.ParentNode.RemoveChild(drawMethodVersionNode)
                                fill_xml_preview({emuNode, curVerNode})
                                Exit Sub
                            End If
                        End If
                    End If

                    'If we have only one vernode, we set default value and delete override if it exist
                    If emuNode.SelectSingleNode("versions").SelectNodes("version").Count = 1 Then
                        drawMethodNode.InnerText = newSelectedDrawMode
                        If newSelectedBackgroundSource <> "" Then
                            If drawMethodNode.Attributes("backgroundSource") IsNot Nothing Then
                                drawMethodNode.Attributes("backgroundSource").Value = newSelectedBackgroundSource
                            Else
                                Dim newAttr As Xml.XmlAttribute = xmlConfig.CreateAttribute("backgroundSource")
                                newAttr.Value = newSelectedBackgroundSource
                                drawMethodNode.Attributes.Append(newAttr)
                            End If
                        Else
                            If drawMethodNode.Attributes("backgroundSource") IsNot Nothing Then
                                Dim a As Xml.XmlAttribute = drawMethodNode.Attributes("backgroundSource")
                                drawMethodNode.Attributes.Remove(a)
                            End If
                        End If
                        If Step7_CheckBox2.Checked Then
                            If drawMethodNode.Attributes("hwnd") IsNot Nothing Then
                                drawMethodNode.Attributes("hwnd").Value = Step7_WindowClassTextBox.Text
                            Else
                                Dim newAttr As Xml.XmlAttribute = xmlConfig.CreateAttribute("hwnd")
                                newAttr.Value = Step7_WindowClassTextBox.Text
                                drawMethodNode.Attributes.Append(newAttr)
                            End If
                        Else
                            If drawMethodNode.Attributes("hwnd") IsNot Nothing Then
                                Dim a As Xml.XmlAttribute = drawMethodNode.Attributes("hwnd")
                                drawMethodNode.Attributes.Remove(a)
                            End If
                        End If

                        If drawMethodVersionNode IsNot Nothing Then drawMethodVersionNode.ParentNode.RemoveChild(drawMethodVersionNode)
                        fill_xml_preview({emuNode, curVerNode})
                        Exit Sub
                    End If

                    'If we have multiple values, than we set override
                    If emuNode.SelectSingleNode("versions").SelectNodes("version").Count > 1 Then
                        If drawMethodVersionNode Is Nothing Then
                            drawMethodVersionNode = xmlConfig.CreateElement("drawmethod")
                            curVerNode.AppendChild(drawMethodVersionNode)
                        End If

                        drawMethodVersionNode.InnerText = newSelectedDrawMode

                        If newSelectedBackgroundSource <> "" Then
                            If drawMethodVersionNode.Attributes("backgroundSource") IsNot Nothing Then
                                drawMethodVersionNode.Attributes("backgroundSource").Value = newSelectedBackgroundSource
                            Else
                                Dim newAttr As Xml.XmlAttribute = xmlConfig.CreateAttribute("backgroundSource")
                                newAttr.Value = newSelectedBackgroundSource
                                drawMethodVersionNode.Attributes.Append(newAttr)
                            End If
                        Else
                            If drawMethodVersionNode.Attributes("backgroundSource") IsNot Nothing Then
                                Dim a As Xml.XmlAttribute = drawMethodVersionNode.Attributes("backgroundSource")
                                drawMethodVersionNode.Attributes.Remove(a)
                            End If
                        End If
                        If Step7_CheckBox2.Checked Then
                            If drawMethodVersionNode.Attributes("hwnd") IsNot Nothing Then
                                drawMethodVersionNode.Attributes("hwnd").Value = Step7_WindowClassTextBox.Text
                            Else
                                Dim newAttr As Xml.XmlAttribute = xmlConfig.CreateAttribute("hwnd")
                                newAttr.Value = Step7_WindowClassTextBox.Text
                                drawMethodVersionNode.Attributes.Append(newAttr)
                            End If
                        Else
                            If drawMethodVersionNode.Attributes("hwnd") IsNot Nothing Then
                                Dim a As Xml.XmlAttribute = drawMethodVersionNode.Attributes("hwnd")
                                drawMethodVersionNode.Attributes.Remove(a)
                            End If
                        End If
                        fill_xml_preview({emuNode, curVerNode})
                    End If
                End If
            Next
        End If
    End Sub

    'Change drawMethod options
    Private Sub Step7_CheckBox1_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles Step7_CheckBox1.CheckedChanged, Step7_CheckBox2.CheckedChanged, Step7_RadioButton1.CheckedChanged, Step7_RadioButton2.CheckedChanged, Step7_WindowClassTextBox.TextChanged
        Step7_SelectDrawMethod_SelectedIndexChanged(sender, New EventArgs)
    End Sub

    'Quicksave changed
    Private Sub Step7_quickSaveSchema_SelectedIndexChanged(sender As System.Object, e As System.EventArgs) Handles Step7_quickSaveSchema.SelectedIndexChanged
        If UpdateQuickSaveSchema Then
            For Each emuNode As Xml.XmlNode In xmlConfig.SelectNodes("/config/emulator")
                If emuNode.SelectSingleNode("exe").InnerText.ToUpper.Trim = p.ProcessName.ToUpper.Trim Then
                    Dim quickSaveNode As Xml.XmlNode = emuNode.SelectSingleNode("quicksave")
                    quickSaveNode.InnerText = (quickSaveIndexes(Step7_quickSaveSchema.SelectedIndex)).ToString
                    Exit For
                End If
            Next
        End If
    End Sub

    'Save config
    Private Sub Step7_SaveConfig_Click(sender As System.Object, e As System.EventArgs) Handles Step7_SaveConfig.Click
        FileIO.FileSystem.CopyFile(".\config.xml", ".\config.backup.xml", True)
        xmlConfig.Save(".\config.xml")
    End Sub
#End Region

    Private Sub Button2_Click(sender As System.Object, e As System.EventArgs) Handles Button2.Click
        If curStep = 1 Then
            If Step1_ListBox1.SelectedIndex < 0 Then MsgBox("You have to choose an Emulator.") : Exit Sub
            Button1.Enabled = True
            Dim tmp As String = Step1_ListBox1.SelectedItem.ToString
            If Process.GetProcessesByName(tmp.Substring(0, tmp.IndexOf("[") - 1)).Count = 0 Then MsgBox("Process can't be found. Relaunch it or refresh list.") : Exit Sub
            If Process.GetProcessesByName(tmp.Substring(0, tmp.IndexOf("[") - 1)).Count > 1 Then MsgBox("More than one process found. Please close all of them and leave just one.") : Exit Sub
            p = Process.GetProcessesByName(tmp.Substring(0, tmp.IndexOf("[") - 1))(0)
            Dim newprocessCrc = AppContext.GetCRC32(p.MainModule.FileName).ToUpper.Trim
            If Not processCrc = newprocessCrc Then
                processCrc = newprocessCrc
                Step4_DataGridView1.Rows.Clear()
                Step5_DataGridView3.Rows.Clear()
                Step6_DataGridView1.Rows.Clear()
                setRam = {0, 0, 0, 0}
                Step6_AdditionalActionsTextBox.Text = ""
                Step6_AdditionalActionsComboBox.SelectedIndex = 0
                Step6_setRam1.Tag = "" : Step6_setRam2.Tag = ""
                Step6_setRam1.Text = "SET RAM 1" : Step6_setRam2.Text = "SET RAM 2"
            End If

            updatingEmu = False
            theOnlyPredefinedSystem = ""
            Step3_RadioButton1.Checked = True
            Step3_RadioButton1.Enabled = True
            Step3_romnameFromTitle.Checked = False
            Step3_info.Text = "You are adding a brand new emulator."
            Dim node As Xml.XmlNode
            Dim nodelist As Xml.XmlNodeList
            updatingEmulatorVersionNode = Nothing
            nodelist = xmlConfig.SelectNodes("/config/emulator")
            For Each node In nodelist
                If p.ProcessName.ToUpper = node.SelectSingleNode("exe").InnerText.ToUpper Then
                    updatingEmu = True
                    'TabControl1.TabPages.Insert(1, TabPage2)
                    'TabControl1.TabPages.RemoveAt(0)
                    'curStep = 3

                    If node.SelectNodes("system").Count = 1 Then
                        theOnlyPredefinedSystem = node.SelectSingleNode("system").SelectSingleNode("name").InnerText.ToUpper.Trim
                        Select Case theOnlyPredefinedSystem
                            Case "GB"
                                Step3_ComboBox1.SelectedIndex = 0
                            Case "GBC"
                                Step3_ComboBox1.SelectedIndex = 1
                            Case "GBA"
                                Step3_ComboBox1.SelectedIndex = 2
                            Case "NES"
                                Step3_ComboBox1.SelectedIndex = 3
                            Case "SNES"
                                Step3_ComboBox1.SelectedIndex = 4
                            Case "N64"
                                Step3_ComboBox1.SelectedIndex = 5
                            Case "NGC"
                                Step3_ComboBox1.SelectedIndex = 6
                            Case "GG"
                                Step3_ComboBox1.SelectedIndex = 7
                            Case "SMS"
                                Step3_ComboBox1.SelectedIndex = 8
                            Case "SMD"
                                Step3_ComboBox1.SelectedIndex = 9
                            Case "32X"
                                Step3_ComboBox1.SelectedIndex = 10
                            Case "SCD"
                                Step3_ComboBox1.SelectedIndex = 11
                            Case "PSX"
                                Step3_ComboBox1.SelectedIndex = 12
                            Case "PS2"
                                Step3_ComboBox1.SelectedIndex = 13
                        End Select
                    Else
                        Step3_RadioButton1.Enabled = False
                        Step3_ComboBox1.SelectedIndex = -1
                        Dim allSystemsDetectedFromTitle As Boolean = True
                        For Each n As Xml.XmlNode In node.SelectNodes("system")
                            If Not n.SelectSingleNode("detect").InnerText.ToLower.Contains("title") Then allSystemsDetectedFromTitle = False : Exit For
                        Next
                        If allSystemsDetectedFromTitle Then Step3_RadioButton2.Checked = True Else Step3_RadioButton3.Checked = True
                    End If
                    If node.SelectSingleNode("pausekey") IsNot Nothing Then Step3_TextBox4.Text = [Enum].GetName(System.Windows.Forms.Keys.KeyCode.GetType, CInt("&H" + node.SelectSingleNode("pausekey").InnerText))
                    If node.SelectSingleNode("quicksave") IsNot Nothing Then
                        UpdateQuickSaveSchema = False
                        Step7_quickSaveSchema.SelectedIndex = quickSaveIndexes.IndexOf(CInt(node.SelectSingleNode("quicksave").InnerText))
                        UpdateQuickSaveSchema = True
                    End If


                    Dim allVersionsObtainsRomnameFromTitle As Boolean = True
                    Dim lastVersionsRomnameString As String = ""
                    Step3_info.Text = "The emulator you are trying to add already exist in config. However, you emulator crc doesn't match none of existing versions in config. The wizard will presume that you are adding a new version of emulator."

                    If Not node.SelectSingleNode("versions") Is Nothing Then 'Dirty hack, if there is no VERSIONS node
                        For Each versionNode As Xml.XmlNode In node.SelectSingleNode("versions").SelectNodes("version")
                            Dim crc As String = versionNode.SelectSingleNode("crc").InnerText.Trim.ToUpper
                            If processCrc = crc Then
                                Step3_info.Text = "The emulator you are trying to add already exist in config. You can add new system to this emulator by selecting a new system, or you can update existed system."
                                Step3_TextBox3.Text = versionNode.Attributes("ver").Value
                                updatingEmulatorVersionNode = versionNode
                            End If

                            If Not versionNode.SelectSingleNode("romname").InnerText.ToUpper.Contains("TITLE") Then allVersionsObtainsRomnameFromTitle = False
                            lastVersionsRomnameString = versionNode.SelectSingleNode("romname").InnerText
                        Next
                    End If
                    If allVersionsObtainsRomnameFromTitle Then
                        Step3_romnameFromTitle.Checked = True
                        Step51_TextBox2.Text = lastVersionsRomnameString
                    End If
                End If
                If updatingEmu Then Exit For
            Next

            TabControl1.TabPages.Insert(1, TabPage3)
            TabControl1.TabPages.RemoveAt(0)
            curStep = 3
            step3_init()
            Exit Sub
        End If

        If curStep = 2 Then
            If Step2_RadioButton1.Checked Then Step3_TextBox3.Text = Step2_TextBox1.Text
            TabControl1.TabPages.Insert(1, TabPage3)
            TabControl1.TabPages.RemoveAt(0)
            curStep = 3
            step3_init()
            Exit Sub
        End If

        If curStep = 3 Then
            If Step3_ComboBox1.SelectedIndex < 0 Then MsgBox("Please, select emulated system") : Exit Sub
            If Step3_RadioButton3.Checked Then
                TabControl1.TabPages.Insert(1, TabPage4)
                curStep = 4
                step4_init()
            Else
                If Step3_romnameFromTitle.Checked Then
                    TabControl1.TabPages.Insert(1, TabPage51)
                    curStep = 51
                    Step51_init()
                Else
                    TabControl1.TabPages.Insert(1, TabPage5)
                    curStep = 5
                    Step5_init()
                End If
            End If
            TabControl1.TabPages.RemoveAt(0)
            Exit Sub
        End If

        If curStep = 4 Then
            If Step3_romnameFromTitle.Checked Then
                TabControl1.TabPages.Insert(1, TabPage51)
                curStep = 51
                Step51_init()
            Else
                TabControl1.TabPages.Insert(1, TabPage5)
                curStep = 5
                Step5_init()
            End If
            'TabControl1.TabPages.Insert(1, TabPage5)
            TabControl1.TabPages.RemoveAt(0)
            'curStep = 5
            'Step5_init()
            Exit Sub
        End If

        If curStep = 5 Or curStep = 51 Then
            If curStep = 5 And Step5_DataGridView3.SelectedRows.Count = 0 Then MsgBox("You have to select an address first.") : Exit Sub
            TabControl1.TabPages.Insert(1, TabPage6)
            TabControl1.TabPages.RemoveAt(0)
            curStep = 6
            Step6_init()
            Exit Sub
        End If

        If curStep = 6 Then
            If Step6_AdditionalActionsComboBox.SelectedIndex <> 0 Then
                Try
                    additional_action_value = CUInt("&H" + Step6_AdditionalActionsTextBox.Text)
                Catch ex As Exception
                    MsgBox("Invalid value in additional action.") : Exit Sub
                End Try
            End If
            If Step6_setRam1.Visible Then
                If Not Step6_setRam1.Text.EndsWith("(set)") Or Not Step6_setRam2.Text.EndsWith("(set)") Then MsgBox("You have to set both ram 0 addr first.") : Exit Sub
            Else
                If Step6_DataGridView1.SelectedRows.Count = 0 Then MsgBox("You have to select an address first.") : Exit Sub
            End If

            'Disable drawmethod control if updating emu
            'Dim controlEnabled As Boolean = Not updatingEmu
            'Step7_SelectDrawMethod.Enabled = controlEnabled
            'Step7_CheckBox1.Enabled = controlEnabled
            'Step7_CheckBox2.Enabled = controlEnabled
            'Step7_WindowClassTextBox.Enabled = controlEnabled
            'Step7_RadioButton1.Enabled = controlEnabled
            'Step7_RadioButton2.Enabled = controlEnabled

            TabControl1.TabPages.Insert(1, TabPage7)
            TabControl1.TabPages.RemoveAt(0)
            curStep = 7
            Step7_init()
            Exit Sub
        End If
    End Sub 'Next Button

    Private Sub Button1_Click(sender As System.Object, e As System.EventArgs) Handles Button1.Click
        If curStep > 1 Then curStep -= 1 Else Exit Sub
        If curStep = 2 Then curStep = 1
        If curStep = 50 Then curStep = 4
        If curStep = 4 And Not Step3_RadioButton3.Checked Then curStep = 3
        If curStep = 5 And Step3_romnameFromTitle.Checked Then curStep = 51

        If curStep = 1 Then TabControl1.TabPages.Insert(1, TabPage1) : Button1.Enabled = False
        If curStep = 2 Then TabControl1.TabPages.Insert(1, TabPage2)
        If curStep = 3 Then TabControl1.TabPages.Insert(1, TabPage3)
        If curStep = 4 Then TabControl1.TabPages.Insert(1, TabPage4)
        If curStep = 5 Then TabControl1.TabPages.Insert(1, TabPage5)
        If curStep = 51 Then TabControl1.TabPages.Insert(1, TabPage51)
        If curStep = 6 Then TabControl1.TabPages.Insert(1, TabPage6)
        If curStep = 7 Then TabControl1.TabPages.Insert(1, TabPage7)
        TabControl1.TabPages.RemoveAt(0)
    End Sub 'Prev Button

    Private Sub Step1_Help_Click(sender As System.Object, e As System.EventArgs) Handles Step1_Help.Click, Step3_Help.Click, Step4_Help.Click, Step5_Help.Click, Step51_Help.Click, Step6_Help.Click, Step7_Help.Click
        If Form1_EmulatorConfigWizard_help.Visible Or Form1_EmulatorConfigWizard_helpSpec.Visible Then
            MsgBox("Close the opened Help window, to display a help for this step.") : Exit Sub
        End If

        Dim _step As String = DirectCast(sender, Button).Name.Substring(4, 2)
        If _step.Substring(1) = "_" Then _step = _step.Substring(0, 1)
        Dim stepInt As Integer = CInt(_step)
        If stepInt = 6 Then
            Form1_EmulatorConfigWizard_helpSpec.SelectedSystem = curSystem
            Form1_EmulatorConfigWizard_helpSpec.Show()
        Else
            Form1_EmulatorConfigWizard_help._step = stepInt
            Form1_EmulatorConfigWizard_help.Show()
        End If

    End Sub 'Help buttons

    Private Sub Button4_Click(sender As System.Object, e As System.EventArgs) Handles Button4.Click
        Dim f As New Form1_EmulatorConfigWizard_findWindow
        f._process = p
        f.Show()
    End Sub 'Show find window class tool

    Private Sub WaitForProcess()
        'If Form1_EmulatorConfigWizard_processExited.Visible = True Then Exit Sub
        'Me.Tag = p.ProcessName
        'Form1_EmulatorConfigWizard_processExited.ShowDialog(Me)
        'If Me.Tag.ToString = "-1" Then step5_timer.Enabled = False : Exit Sub
        'p = Process.GetProcessesByName(Me.Tag.ToString)(0)

        If GroupBox2.Visible = True Then Exit Sub
        GroupBox2.Visible = True
        GroupBox2.BringToFront()
        'PictureBox1.ImageLocation = ".\loader3.gif"
        TabControl1.Enabled = False : Button1.Enabled = False : Button2.Enabled = False
        trd = New Threading.Thread(AddressOf ThreadTask)
        trd.IsBackground = True : trd.Start()
    End Sub
    Private Sub ThreadTask()
        Dim ps() As Process
        Do While True
            ps = Process.GetProcesses()
            For Each curp As Process In ps
                If curp.ProcessName = p.ProcessName Then
                    Exit Do
                End If
            Next
            Threading.Thread.Sleep(100)
        Loop
        workCompleted()
    End Sub
    Private Delegate Sub workCompletedDelegate()
    Private Sub workCompleted()
        If InvokeRequired Then
            Invoke(New workCompletedDelegate(AddressOf workCompleted))
            Exit Sub
        End If
        GroupBox2.Visible = False
        TabControl1.Enabled = True : Button1.Enabled = True : Button2.Enabled = True
        p = Process.GetProcessesByName(p.ProcessName)(0)
        If curStep = 5 Or curStep = 6 Then Step5_init_module_map()
    End Sub
    Private Sub PictureBox1_Click(sender As System.Object, e As System.EventArgs) Handles PictureBox1.Click
        If MsgBox("Application is waiting for process ''. Do you want to abort?", MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then
            step5_timer.Enabled = False
            GroupBox2.Visible = False
            TabControl1.Enabled = True : Button1.Enabled = True : Button2.Enabled = True
        End If
    End Sub

    Private Sub Step7_TestCheatsTextBox_TextChanged(sender As System.Object, e As System.EventArgs) Handles Step7_TestCheatsTextBox.TextChanged
        If Not m Is Nothing Then
            If Step7_TestCheatsTextBox.Text.Trim = "" Then
                m.timerEnable = False
            Else
                Dim c As Integer = 0
                For Each s As String In Step7_TestCheatsTextBox.Text.Trim.Split({vbCrLf}, StringSplitOptions.RemoveEmptyEntries)
                    'If c = 0 Then cl.SetTestingCheats(s, True) Else cl.SetTestingCheats(s)
                    If c = 0 Then m.Cheats_CheatCodes_testing = New List(Of String)
                    m.Cheats_CheatCodes_testing.Add(s)
                    c += 1
                Next
                m.timerEnable = True
            End If
        End If
    End Sub
End Class