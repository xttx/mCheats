Imports WindowsApplication1.WinAPI
Imports System.Runtime.InteropServices

Public Class AppContext
    Inherits ApplicationContext

#Region " Functions "
    <DllImport("kernel32.dll")> _
    Private Shared Function ResumeThread(ByVal hThread As IntPtr) As Integer
    End Function
    <DllImport("Kernel32.dll")> _
    Private Shared Function SuspendThread(ByVal hThread As IntPtr) As Integer
    End Function
    <DllImport("kernel32.dll")> _
    Private Shared Function OpenThread(ByVal dwDesiredAccess As ThreadAccess, ByVal bInheritHandle As Boolean, ByVal dwThreadId As Integer) As IntPtr
    End Function
    Public Enum ThreadAccess
        DIRECT_IMPERSONATION = &H200
        GET_CONTEXT = 8
        IMPERSONATE = &H100
        QUERY_INFORMATION = &H40
        SET_CONTEXT = &H10
        SET_INFORMATION = &H20
        SET_THREAD_TOKEN = &H80
        SUSPEND_RESUME = 2
        TERMINATE = 1
    End Enum
    <DllImport( _
 "user32.dll", CharSet:=CharSet.Auto, CallingConvention:=CallingConvention.StdCall)> _
    Public Shared Function SetWindowPos( _
 ByVal hWnd As IntPtr, _
 ByVal hWndInsertAfter As IntPtr, _
 ByVal X As Int32, _
 ByVal Y As Int32, _
 ByVal cx As Int32, _
 ByVal cy As Int32, _
 ByVal uFlags As Int32) _
 As Boolean
    End Function
    <System.Runtime.InteropServices.DllImport("user32")> _
    Private Shared Function GetWindowRect(ByVal hwnd As IntPtr, ByRef lpRect As RECT) As IntPtr
    End Function
    <DllImport("user32.dll", CharSet:=CharSet.Auto)> _
    Private Shared Function GetClientRect(ByVal hWnd As System.IntPtr, ByRef lpRECT As RECT) As IntPtr
    End Function
    <DllImport("user32.dll")> _
    Private Shared Function ClientToScreen(ByVal hWnd As IntPtr, ByRef lpPoint As Point) As Boolean
    End Function
    <DllImport("User32")> _
    Private Shared Function ShowWindow(ByVal hwnd As IntPtr, ByVal nCmdShow As Integer) As Boolean
    End Function
    <DllImport("user32.dll", SetLastError:=True, CharSet:=CharSet.Auto)> _
    Private Shared Function SendMessage(ByVal hWnd As IntPtr, ByVal Msg As UInteger, ByVal wParam As IntPtr, ByVal lParam As IntPtr) As IntPtr
    End Function
    <DllImport("user32.dll", SetLastError:=True, CharSet:=CharSet.Auto)> _
    Private Shared Function PostMessage(ByVal hWnd As IntPtr, ByVal Msg As UInteger, ByVal wParam As IntPtr, ByVal lParam As IntPtr) As Boolean
    End Function
    <DllImport("user32.dll")> _
    Private Shared Function SetForegroundWindow(ByVal hWnd As IntPtr) As <MarshalAs(UnmanagedType.Bool)> Boolean
    End Function
    Public Const HWND_TOPMOST = -1
    Public Const HWND_NOTOPMOST = -2
    Public Const SWP_NOMOVE = &H2
    Public Const SWP_NOSIZE = &H1
    Private Const SW_HIDE = 0
    Private Const SW_MIN = 1
    Private Const SW_MAX = 2
    Private Const SW_RESTORE = 9
    Const WM_SETFOCUS As Long = &H7
    <DllImport("user32.dll")> _
    Private Shared Function SendInput( _
    ByVal nInputs As Integer, _
    ByVal pInputs() As INPUT, _
    ByVal cbSize As Integer) As Integer
    End Function
    <StructLayout(LayoutKind.Explicit)> _
    Public Structure INPUT        'Field offset 32 bit machine 4, 64 bit machine 8
        <FieldOffset(0)> _
        Public type As Integer
        <FieldOffset(8)> _
        Public mi As MOUSEINPUT
        <FieldOffset(8)> _
        Public ki As KEYBDINPUT
        <FieldOffset(8)> _
        Public hi As HARDWAREINPUT
    End Structure
    Public Structure MOUSEINPUT
        Public dx As Integer
        Public dy As Integer
        Public mouseData As Integer
        Public dwFlags As Integer
        Public time As Integer
        Public dwExtraInfo As IntPtr
    End Structure
    Public Structure KEYBDINPUT
        Public wVk As Short
        Public wScan As Short
        Public dwFlags As Integer
        Public time As Integer
        Public dwExtraInfo As IntPtr
    End Structure
    Public Structure HARDWAREINPUT
        Public uMsg As Integer
        Public wParamL As Short
        Public wParamH As Short
    End Structure
#End Region
#Region " Variables "
    Dim romName As String = ""
    Dim sysName As String = ""
    Dim p As Process()
    'Dim tmp As Integer
    Dim hwndd As IntPtr
    Dim Method As Integer = 0
    Dim PauseKey As Short = 0
    Dim PauseKeyUseScanCode As Boolean = False
    Dim WithEvents m As New Class5_MemoryAccess
    Private WithEvents a As Shortcut = T
    Private WithEvents frm1 As Form1
    Private WithEvents Tray As NotifyIcon
    Private WithEvents MainMenu As ContextMenuStrip
    Private WithEvents mnuDisplayForm As ToolStripMenuItem
    Private WithEvents mnuDisplayEmuConfig As ToolStripMenuItem
    Private WithEvents mnuDisplayJoyConfig As ToolStripMenuItem
    Private WithEvents mnuSep1, mnuSep2 As ToolStripSeparator
    Private WithEvents mnuExit As ToolStripMenuItem
    Dim WithEvents hook As Class6_InstallHook
    Public Delegate Sub classMainDelegate(o As Object, sa As Shortcut.HotKeyEventArgs)
    'Public Shared _testing As Boolean = False
#End Region

    Public Sub New()
        'Init System Tables hash-table
        Dim tmpSysName As String = ""
        For Each s As String In db.Enumerate_tables()
            If s.Substring(0, 1) = "_" Then tmpSysName = s.Substring(1) Else tmpSysName = s
            systemTablesSQL.Add(tmpSysName.ToUpper, s)
        Next

        'Initialize the menus
        mnuDisplayEmuConfig = New ToolStripMenuItem("Emulator Config Wizard")
        mnuSep1 = New ToolStripSeparator()
        mnuDisplayForm = New ToolStripMenuItem("Database Manager")
        mnuDisplayJoyConfig = New ToolStripMenuItem("Configure Joystick")
        mnuSep2 = New ToolStripSeparator()
        mnuExit = New ToolStripMenuItem("Exit")
        MainMenu = New ContextMenuStrip
        MainMenu.Items.AddRange(New ToolStripItem() {mnuDisplayEmuConfig, mnuSep1, mnuDisplayForm, mnuDisplayJoyConfig, mnuSep2, mnuExit})

        'Initialize the tray
        Tray = New NotifyIcon
        Tray.Icon = Form1.Icon
        Tray.ContextMenuStrip = MainMenu
        Tray.Text = "mCheat tools"

        'Display
        Tray.Visible = True
        AddHandler m.processHasExited, AddressOf OnProcessExit
        AddHandler MultiThreading.showmenuEvent, AddressOf s
        AddHandler MultiThreading.emulatorLaunched, AddressOf justHookAndGetInfo
    End Sub

#Region " Event handlers "
    Private Sub s()
        T_Press(New Object, New Shortcut.HotKeyEventArgs)
    End Sub

    Private Sub T_Press(ByVal sender As Object, ByVal e As Shortcut.HotKeyEventArgs) Handles a.Press
        If Module1.MenuActive Then Log("Menu - deactivated.") : OnMenuExit() : Exit Sub
        Log("----------------") : Log("Menu - ACTIVATED")
        'If Module1.MenuActive And p IsNot Nothing And p.Count > 0 AndAlso p(0).HasExited = False Then OnMenuExit() : Exit Sub

        'Method = 0 - Press pause and show menu
        'Method = 1 - Suspend thread and show menu
        'Method = 2 - Press Pause and manipulate windows to show menu in the native form FULLSCREEN MODE
        'Method = 3 - Press Pause and manipulate windows to show menu in the native form WINDOWED MODE
        'Method = 10 - Press Pause and hook
        'Method = 11 - Just hook, but draw with gdi+
        Method = -1
        r.Bottom = 0
        hwndd = IntPtr.Zero
        Dim hookParam As New Dictionary(Of String, Short)
        Dim fromptr As Boolean = False
        'Dim fromhook As Boolean = False
        Dim hookDI As Boolean = False
        Dim cheats_path As String = ".\cheats\"

        hookParam = getEmuData()
        'If Not _testing Then hookParam = getEmuData() Else hookParam = getEmuData_testing()
        If hookParam Is Nothing Then Exit Sub
        If hookParam("fromptr") = 1 Then fromptr = True
        'TODO
        '- Check Visualboyadvance cheat menu showing & cheats
        '- dolphin, epsxe and pcsxr can return garbage as romname when nothing is run
        '- check submenu with metod 10 (and other methods but 11)
        '- dolphin shows window menu when pressing up/down while paused

        'fusion
        '&H18EFE5 - 0=paused 1=unpaused
        'SetForegroundWindow(hwndd)

        'N64
        'FULLSCREEN Method = 0 DOESn'T WORK WITH DirectX plugins

        If p.Length = 0 Or hwndd = IntPtr.Zero Then Exit Sub
        If cl.getCheatsCheatNames.Count = 0 Then
            cl.getCheatsCheatNames.Add("Sorry, There is no cheats for this game yet.")
        Else
            KeyUp.Register() : KeyDown.Register() : KeyEnter.Register()
        End If
        KeyBackSpace.Register()

        m.timerEnable = True
        Module1.MenuActive = True
        'AppActivate(p(0).Id)
        Dim counter As Integer = 0
        If Method <> 1 And m.isPaused() <> -1 Then
            While m.isPaused() = 0
                pressPause(PauseKey, False, PauseKeyUseScanCode)
                Threading.Thread.Sleep(100) : Application.DoEvents() : counter = counter + 1
                If counter > 30 Then MsgBox("Internal Error. Can't pause. Should be wrong version of emulator.") : Exit Sub
            End While
        Else
            If Method <> 1 Then pressPause(PauseKey, False, PauseKeyUseScanCode)
        End If

        Log("Initiate menu drawing...")
        If Method = 0 Then
            'Module1.MainMenu = New Class3_Menu(hwndd, cl.getCheatsCheatNames, 0, fromptr, New Rectangle(r.Left, r.Top, r.Right - r.Left, r.Bottom - r.Top))
            Module1.MainMenu = New Class3_Menu(hwndd, cl.getCheatsCheatNames, 0, fromptr, New Rectangle(r.Left, r.Top, r.Right, r.Bottom))
            AddHandler Module1.MainMenu.OnExit, AddressOf OnMenuExit
        ElseIf Method = 1 Then
            Shell("pausep " + p(0).Id.ToString)
            'For Each th As ProcessThread In p(0).Threads
            'SuspendThread(OpenThread(ThreadAccess.SUSPEND_RESUME, False, th.Id))
            'Next
            Module1.MainMenu = New Class3_Menu(hwndd, cl.getCheatsCheatNames, 0)
            AddHandler Module1.MainMenu.OnExit, AddressOf OnMenuExit
        ElseIf Method = 2 Then
            Form2_overlay.WindowState = FormWindowState.Maximized
            Module1.MainMenu = New Class3_Menu(hwndd, cl.getCheatsCheatNames, 0)
            ShowWindow(hwndd, SW_HIDE)
            Form2_overlay.Show() : SetWindowPos(Form2_overlay.Handle, New IntPtr(HWND_TOPMOST), 0, 0, 0, 0, SWP_NOMOVE Or SWP_NOSIZE)
            AddHandler Form2_overlay.FormClosed, AddressOf OnMenuExit
        ElseIf Method = 3 Then
            Form2_overlay.WindowState = FormWindowState.Normal
            Module1.MainMenu = New Class3_Menu(hwndd, cl.getCheatsCheatNames, 0, True, New Rectangle(r.Left, r.Top, r.Right, r.Bottom))
            'Module1.MainMenu = New Class3_Menu(hwndd, cl.getCheatsCheatNames, 0, True, New Rectangle(r.Left, r.Top, r.Right - r.Left, r.Bottom - r.Top))
            Form2_overlay.Show() : SetWindowPos(Form2_overlay.Handle, New IntPtr(HWND_TOPMOST), 0, 0, 0, 0, SWP_NOMOVE Or SWP_NOSIZE)
            AddHandler Form2_overlay.FormClosed, AddressOf OnMenuExit
        ElseIf Method = 10 Then
            Module1.MainMenu = New Class3_Menu(hwndd, cl.getCheatsCheatNames, 0, fromptr, New Rectangle(r.Left, r.Top, r.Right, r.Bottom), 0, True)
            'Module1.MainMenu = New Class3_Menu(hwndd, cl.getCheatsCheatNames, 0, fromptr, New Rectangle(r.Left, r.Top, r.Right - r.Left, r.Bottom - r.Top), 0, True)
            hook = New Class6_InstallHook(p(0), False, hookParam)
            AddHandler hook.onExit, AddressOf OnMenuExit
        ElseIf Method = 11 Then
            hook = New Class6_InstallHook(p(0), True, hookParam)
            'Threading.Thread.Sleep(20)
            Module1.MainMenu = New Class3_Menu(hwndd, cl.getCheatsCheatNames, 0, fromptr, New Rectangle(r.Left, r.Top, r.Right, r.Bottom))
            'Module1.MainMenu = New Class3_Menu(hwndd, cl.getCheatsCheatNames, 0, fromptr, New Rectangle(r.Left, r.Top, r.Right - r.Left, r.Bottom - r.Top))
            'testarray.Add(Now.Ticks.ToString)
            AddHandler Module1.MainMenu.OnExit, AddressOf OnMenuExit
        End If
        Log("Initiate menu drawing done.")
    End Sub

    Private Function getEmuData() As Dictionary(Of String, Short)
        Dim fromptr As Boolean = False
        Dim hookParam As New Dictionary(Of String, Short)
        Dim cheats_path As String = ".\cheats\"

        Dim crc As String
        Dim node As Xml.XmlNode
        Dim nodelist As Xml.XmlNodeList
        nodelist = xmlConfig.SelectNodes("/config/emulator")
        For Each node In nodelist
            p = Process.GetProcessesByName(node.SelectSingleNode("exe").InnerText)
            If p.Length > 0 Then
                Log("get crc...")
                crc = GetCRC32(p(0).MainModule.FileName).ToUpper
                For Each vernode As Xml.XmlNode In node.SelectNodes("versions/version")
                    If crc = vernode.SelectSingleNode("crc").InnerText.ToUpper Then
                        Log("Found matched emulator: " + p(0).ProcessName)
                        If Not isCurRomEqOld(node, vernode) Then
                            If romName = "" Then Return Nothing
                            'Here we check if SQL table exist for system
                            If systemTablesSQL.Keys.Contains(sysName.ToUpper) Then
                                Log("Loading cheats from SQL...")
                                cl = New Class4_CheatsLoad("", romName, sysName)
                            Else
                                'Here we go cheatFiles if sql table was not found
                                For Each cheatfile As Xml.XmlNode In xmlConfig.SelectNodes("/config/cheatfile")
                                    If cheatfile.Attributes.GetNamedItem("system").Value.ToUpper = sysName.ToUpper Then
                                        Log("Loading cheats from file...")
                                        cl = New Class4_CheatsLoad(cheats_path + cheatfile.InnerText, romName, sysName)
                                        Log("Loading cheats done.")
                                    End If
                                Next
                            End If
                        Else
                            If romName = "" Then Return Nothing
                        End If

                        'Get draw method
                        'Check if window is full screen. TODO: ugly, it assums that window is fullscreen, if window top-left corner is at x=0, y=0
                        Dim drawMethodNode As Xml.XmlNode = Nothing
                        hwndd = p(0).MainWindowHandle
                        Dim point As New Point(0, 0)
                        ClientToScreen(hwndd, point)
                        If point.X = 0 And point.Y = 0 Then drawMethodNode = node.SelectSingleNode("drawmethodfullscreen")
                        If drawMethodNode Is Nothing Then drawMethodNode = node.SelectSingleNode("drawmethod")
                        If vernode.SelectSingleNode("drawmethod") IsNot Nothing Then drawMethodNode = vernode.SelectSingleNode("drawmethod")
                        If vernode.SelectSingleNode("drawmethodfullscreen") IsNot Nothing And point.X = 0 And point.Y = 0 Then drawMethodNode = vernode.SelectSingleNode("drawmethodfullscreen")
                        Method = CInt(drawMethodNode.InnerText)
                        Log("Drawing method: " + Method.ToString)

                        If drawMethodNode.Attributes.GetNamedItem("hwnd") IsNot Nothing Then
                            Dim classes() As String = drawMethodNode.Attributes.GetNamedItem("hwnd").Value.Split(New String() {"->"}, StringSplitOptions.RemoveEmptyEntries)
                            For Each s As String In classes
                                If s = "MainWindow" Then
                                    hwndd = p(0).MainWindowHandle
                                Else
                                    Do While s.StartsWith("@")
                                        s = s.Substring(1)
                                        hwndd = GetNextWindow(hwndd, 2)
                                    Loop
                                    hwndd = FindWindowEx(hwndd, 0, s, vbNullString)
                                End If
                            Next
                            Log("hwnd = " + Hex(hwndd.ToInt64))
                        End If
                        If drawMethodNode.Attributes.GetNamedItem("backgroundSource") IsNot Nothing Then
                            If drawMethodNode.Attributes.GetNamedItem("backgroundSource").Value.ToUpper = "SCREEN" Then
                                fromptr = True
                                Try
                                    Dim ps As New Point(0, 0)
                                    ClientToScreen(hwndd, ps)
                                    GetClientRect(hwndd, r)
                                    r.Left = ps.X : r.Top = ps.Y
                                Catch ex As Exception
                                    MsgBox(ex.Message)
                                End Try
                                Log("backgroundSource = screen")
                            End If
                            If drawMethodNode.Attributes.GetNamedItem("backgroundSource").Value.ToUpper = "HOOK" Then Log("backgroundSource = hook") : hookParam.Add("fromhook", 1)
                        End If

                        'Get Pause key
                        Dim pauseNode As Xml.XmlNode = Nothing
                        If node.SelectSingleNode("pausekey") IsNot Nothing Then pauseNode = node.SelectSingleNode("pausekey")
                        If vernode.SelectSingleNode("pausekey") IsNot Nothing Then pauseNode = vernode.SelectSingleNode("pausekey")
                        If pauseNode IsNot Nothing Then
                            PauseKey = CShort("&H" + pauseNode.InnerText)
                            If pauseNode.Attributes.GetNamedItem("useScanCode") IsNot Nothing Then PauseKeyUseScanCode = True Else PauseKeyUseScanCode = False
                        Else
                            PauseKey = 0
                        End If
                        Log("PauseKey = " + Hex(PauseKey))

                        Dim q As String = node.SelectSingleNode("quicksave").InnerText
                        If xmlConfig.SelectSingleNode("/config/quicksaves/config" + q).SelectSingleNode("useinputhook") IsNot Nothing Then hookParam.Add("hookDI", 1)
                        Exit For
                    End If
                Next
                If hwndd = IntPtr.Zero Then Log("hwnd intptr = 0, aborting!") : Return Nothing Else Exit For
            End If
        Next
        If fromptr = True Then hookParam.Add("fromptr", 1) Else hookParam.Add("fromptr", 0)
        Return hookParam
    End Function

#Region "Testing old unneeded stuff"
    'Public Shared _testing_process As System.Diagnostics.Process
    'Public Shared _testing_romname As String
    'Public Shared _testing_system As String = "NES"
    'Public Shared _testing_drawMethod As Integer
    'Public Shared _testing_drawMethod_hwnd As String = ""
    'Public Shared _testing_drawMethod_backgroundSource As String = ""
    'Public Shared _testing_pauseKey As String = ""
    'Private Function getEmuData_testing() As Dictionary(Of String, Short)
    'Dim fromptr As Boolean = False
    'Dim hookParam As New Dictionary(Of String, Short)
    'Dim cheats_path As String = ".\cheats\"

    'p = {_testing_process}
    'Log("Testing emulator...")
    'romName = _testing_romname
    'sysName = _testing_system
    'If romName = "" Then romName = "TESTING"
    'For Each cheatfile As Xml.XmlNode In xmlConfig.SelectNodes("/config/cheatfile")
    'If cheatfile.Attributes.GetNamedItem("system").Value.ToUpper = sysName.ToUpper Then
    'Log("Loading cheats...")
    'cl = New Class4_CheatsLoad(cheats_path + cheatfile.InnerText, romName, sysName)
    'Log("Loading cheats done.")
    'End If
    'Next
    'If romName = "" Then Return Nothing


    'Method = _testing_drawMethod
    'If _testing_pauseKey <> "" Then PauseKey = CShort("&H" + _testing_pauseKey) Else PauseKey = 0
    'If _testing_drawMethod_hwnd = "" Then
    'hwndd = p(0).MainWindowHandle
    'Else
    'Dim classes() As String = _testing_drawMethod_hwnd.Split(New String() {"->"}, StringSplitOptions.RemoveEmptyEntries)
    'For Each s As String In classes
    'If s = "MainWindow" Then
    'hwndd = p(0).MainWindowHandle
    'Else
    'hwndd = FindWindowEx(hwndd, 0, s, vbNullString)
    'End If
    'Next
    'End If
    'If _testing_drawMethod_backgroundSource <> "" Then
    'If _testing_drawMethod_backgroundSource.ToUpper = "SCREEN" Then
    'fromptr = True
    'Try
    'Dim ps As New Point(0, 0)
    'ClientToScreen(hwndd, ps)
    'GetClientRect(hwndd, r)
    'r.Left = ps.X : r.Top = ps.Y
    'Catch ex As Exception
    'MsgBox(ex.Message)
    'End Try
    'End If
    'If _testing_drawMethod_backgroundSource.ToUpper = "HOOK" Then hookParam.Add("fromhook", 1)
    'End If

    'If hwndd = IntPtr.Zero Then Return Nothing
    'If fromptr = True Then hookParam.Add("fromptr", 1) Else hookParam.Add("fromptr", 0)
    'Return hookParam
    'End Function
#End Region

    Private Sub justHookAndGetInfo(p As System.Diagnostics.Process, useInputHook As Boolean)
        Log("Autofound pid: " + p.Id.ToString + ". Hooking.")
        Dim hookParam As New Dictionary(Of String, Short)
        If useInputHook Then hookParam.Add("hookDI", 1)
        hookParam.Add("justHook", 1)
        hook = New Class6_InstallHook(p, True, hookParam)
        hook._exit()
        hook = Nothing
        GC.Collect()
    End Sub

    Private Function isCurRomEqOld(x As Xml.XmlNode, currentVersion As Xml.XmlNode) As Boolean
        Dim tSysName As String = sysName
        Dim tRomName As String = romName
        m.setup(p(0), x, currentVersion)
        sysName = m.getMashineType()
        romName = m.getRomName().Replace(Chr(0), "").Trim

        Dim f As Boolean = ((sysName = tSysName) And (romName = tRomName))
        If Not f Then CheatsStatus = New Hashtable
        Return f
    End Function

    Private Sub pressPause(ByVal pausekey As Short, Optional ByVal unpause As Boolean = False, Optional useScancode As Boolean = False)
        'If Not unpause Then
        If Not useScancode Then
            Dim Inpts(2) As INPUT
            Inpts(0).type = 1
            Inpts(0).ki.wVk = &H11
            Inpts(0).ki.wScan = &H1D
            Inpts(0).ki.dwFlags = 2
            Inpts(1).type = 1
            Inpts(1).ki.wVk = pausekey
            Inpts(2).type = 1
            Inpts(2).ki.wVk = pausekey
            Inpts(2).ki.dwFlags = 2
            SendInput(3, Inpts, Marshal.SizeOf(GetType(INPUT)))
            Log("Pause use VK code: " + pausekey.ToString)
        Else
            Dim Inpts(0) As INPUT
            Inpts(0).type = 1
            Inpts(0).ki.wVk = &H11
            Inpts(0).ki.wScan = &H1D
            Inpts(0).ki.dwFlags = 2
            SendInput(1, Inpts, Marshal.SizeOf(GetType(INPUT)))
            Threading.Thread.Sleep(50)

            Inpts(0).type = 1
            Inpts(0).ki.time = 0
            Inpts(0).ki.wVk = 0
            Inpts(0).ki.dwExtraInfo = IntPtr.Zero
            Inpts(0).ki.wScan = pausekey
            Inpts(0).ki.dwFlags = 8
            SendInput(1, Inpts, Marshal.SizeOf(GetType(INPUT)))
            Threading.Thread.Sleep(50)

            Inpts(0).type = 1
            Inpts(0).ki.time = 0
            Inpts(0).ki.wVk = 0
            Inpts(0).ki.dwExtraInfo = IntPtr.Zero
            Inpts(0).ki.wScan = pausekey
            Inpts(0).ki.dwFlags = 10
            SendInput(1, Inpts, Marshal.SizeOf(GetType(INPUT)))
            Log("Pause use SCAN code: " + pausekey.ToString)
        End If

        'Else
        'Dim Inpts(1) As INPUT
        'Inpts(0).type = 1
        'Inpts(0).ki.wVk = pausekey
        'Inpts(1).type = 1
        'Inpts(1).ki.wVk = pausekey
        'Inpts(1).ki.dwFlags = 2
        'SendInput(2, Inpts, Marshal.SizeOf(GetType(INPUT)))
        'End If
    End Sub

    Private Sub OnProcessExit()
        Module1.MenuActive = False
        KeyUp.Unregister() : KeyDown.Unregister() : KeyBackSpace.Unregister() : KeyEnter.Unregister()
        KeyLeft.Unregister() : KeyRight.Unregister()
        If hook IsNot Nothing Then
            hook._exit()
            hook = Nothing
        End If
        If Module1.MainMenu IsNot Nothing Then
            Module1.MainMenu.Dispose()
            Module1.MainMenu = Nothing
        End If
        GC.Collect()
    End Sub

    Private Sub OnMenuExit()
        Module1.MenuActive = False
        KeyUp.Unregister() : KeyDown.Unregister() : KeyBackSpace.Unregister() : KeyEnter.Unregister()
        KeyLeft.Unregister() : KeyRight.Unregister()
        If hook IsNot Nothing Then
            hook._exit()
            hook = Nothing
        End If
        If Module1.MainMenu IsNot Nothing Then
            Module1.MainMenu.Dispose()
            Module1.MainMenu = Nothing
        End If
        'If menu IsNot Nothing Then
        'Menu.Dispose()
        'Menu = Nothing
        'End If
        GC.Collect()
        If Method <> 1 Then
            If m.isPaused() <> -1 Then
                Dim counter As Integer = 0
                While m.isPaused() = 1
                    pressPause(PauseKey, True, PauseKeyUseScanCode)
                    Threading.Thread.Sleep(100)
                    Application.DoEvents()
                    counter = counter + 1
                    If counter > 30 Then MsgBox("Internal Error. Should be wrong version of emulator.") : Exit Sub
                End While
            Else : pressPause(PauseKey, True, PauseKeyUseScanCode)
            End If
        End If
        If Method = 1 Then
            Shell("pausep " + p(0).Id.ToString + " /r")
            'For Each th As ProcessThread In p(0).Threads
            'ResumeThread(OpenThread(ThreadAccess.SUSPEND_RESUME, False, th.Id))
            'Next
        End If
    End Sub

    Private Sub AppContext_ThreadExit(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.ThreadExit
        'Guarantees that the icon will not linger.
        Tray.Visible = False
    End Sub

    Private Sub mnuDisplayEmuWizard_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles mnuDisplayEmuConfig.Click
        Form1_EmulatorConfigWizard.Show()
    End Sub

    Private Sub mnuDisplayForm_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles mnuDisplayForm.Click
        Form1.Show()
    End Sub

    Private Sub mnuConfigureJoy_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles mnuDisplayJoyConfig.Click
        Dim f As New Form1_JoystickConfig : f.Show()
    End Sub

    Private Sub mnuExit_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles mnuExit.Click
        Module1.MenuActive = False
        If Form1.Visible Then Form1.Close()
        If Form1_JoystickConfig.Visible Then Form1_JoystickConfig.Close()
        If Form1_EmulatorConfigWizard.Visible Then Form1_EmulatorConfigWizard.Close()
        Application.Exit()
    End Sub

    Private Sub Tray_DoubleClick(ByVal sender As Object, ByVal e As System.EventArgs) _
    Handles Tray.DoubleClick
        Form1.Show()
    End Sub
#End Region

    Public Shared Function GetCRC32(ByVal sFileName As String) As String
        Try
            Dim FS As IO.FileStream = New IO.FileStream(sFileName, IO.FileMode.Open, IO.FileAccess.Read, IO.FileShare.Read, 8192)
            Dim CRC32Result As Integer = &HFFFFFFFF
            Dim Buffer(4096) As Byte
            Dim ReadSize As Integer = 4096
            Dim Count As Integer = FS.Read(Buffer, 0, ReadSize)
            Dim CRC32Table(256) As Integer
            Dim DWPolynomial As Integer = &HEDB88320
            Dim DWCRC As Long
            Dim i As Integer, j As Integer, n As Integer

            'Create CRC32 Table
            For i = 0 To 255
                DWCRC = i
                For j = 8 To 1 Step -1
                    If CBool(DWCRC And 1) Then
                        DWCRC = ((DWCRC And &HFFFFFFFE) \ 2&) And &H7FFFFFFF
                        DWCRC = DWCRC Xor DWPolynomial
                    Else
                        DWCRC = ((DWCRC And &HFFFFFFFE) \ 2&) And &H7FFFFFFF
                    End If
                Next j
                CRC32Table(i) = CInt(DWCRC)
            Next i

            'Calcualting CRC32 Hash
            Do While (Count > 0)
                For i = 0 To Count - 1
                    n = (CRC32Result And &HFF) Xor Buffer(i)
                    CRC32Result = ((CRC32Result And &HFFFFFF00) \ &H100) And &HFFFFFF
                    CRC32Result = CRC32Result Xor CRC32Table(n)
                Next i
                Count = FS.Read(Buffer, 0, ReadSize)
            Loop
            Return Hex(Not (CRC32Result))
        Catch ex As Exception
            Return ""
        End Try
    End Function

    Private Sub ofrm()
        Dim PF As Form1 = Form1
        If PF IsNot Nothing AndAlso Not PF.IsDisposed Then Exit Sub

        Dim CloseApp As Boolean = False

        PF = New Form1
        PF.ShowDialog()
        CloseApp = (PF.DialogResult = DialogResult.Abort)
        PF = Nothing
        If CloseApp Then Application.Exit()
    End Sub
End Class
