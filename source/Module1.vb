Imports WindowsApplication1.WinAPI
Imports System.Runtime.InteropServices
Imports EasyHook
Imports System.Runtime.Remoting.Channels
Imports System.Runtime.Remoting.Channels.Ipc

Public Module Module1
    Public db As Class7_db
    Public logging As Boolean = True
    Public actions As Hashtable
    Public keytable As New Dictionary(Of String, Short)
    Public keytable_scan As New Dictionary(Of String, Short)
    Public systemTablesSQL As New Dictionary(Of String, String)
    Public testarray As New ArrayList
    Public currentSaveStateSlot As Integer = 0
    Public r As RECT
    Public xmlConfig As New Xml.XmlDocument
    Public WithEvents MainMenu As Class3_Menu
    Public MenuActive As Boolean = False
    Public cl As Class4_CheatsLoad = Nothing
    Public CheatsStatus As Hashtable = New Hashtable
    Public sqlTableName As String
    Public WithEvents T, KeyUp, KeyDown, KeyRight, KeyLeft, KeyEnter, KeyBackSpace, qSave, qLoad As Shortcut

    Public Sub Main()
        'Try
        AddHandler AppDomain.CurrentDomain.AssemblyResolve, AddressOf loadSlimDX

        xmlConfig.Load(".\config.xml")
        ParseJoystickConfig()
        initKeyTable()

        db = New Class7_db
        T = Shortcut.Create(Shortcut.Modifier.Ctrl, Keys.E) : T.Register()
        KeyUp = Shortcut.Create(Shortcut.Modifier.None, Keys.Up)
        KeyDown = Shortcut.Create(Shortcut.Modifier.None, Keys.Down)
        KeyLeft = Shortcut.Create(Shortcut.Modifier.None, Keys.Left)
        KeyRight = Shortcut.Create(Shortcut.Modifier.None, Keys.Right)
        KeyEnter = Shortcut.Create(Shortcut.Modifier.None, Keys.End)
        KeyBackSpace = Shortcut.Create(Shortcut.Modifier.None, Keys.Back)
        'qSave = Shortcut.Create(Shortcut.Modifier.Ctrl, Keys.Up) : qSave.Register()
        'qLoad = Shortcut.Create(Shortcut.Modifier.Ctrl, Keys.Down) : qLoad.Register()
        qSave = Shortcut.Create(Shortcut.Modifier.None, Keys.Multiply) : qSave.Register()
        qLoad = Shortcut.Create(Shortcut.Modifier.None, Keys.Divide) : qLoad.Register()
        Dim j As New MultiThreading()

        Application.EnableVisualStyles()
        Application.Run(New AppContext)
        'Catch ex As Exception
        'MsgBox("Error while initializing. " + ex.Message)
        'Application.Exit()
        'End Try
    End Sub

    Public Function loadSlimDX(ByVal sender As Object, ByVal args As System.ResolveEventArgs) As System.Reflection.Assembly
        Log(args.Name)
        If args.Name.IndexOf("Decoder") >= 0 Then
            If System.Environment.Is64BitOperatingSystem Then
                Return System.Reflection.Assembly.LoadFrom(".\Decoder_x64.dll")
            Else
                Return System.Reflection.Assembly.LoadFrom(".\Decoder.dll")
            End If
        End If

        If args.Name.IndexOf("sql", StringComparison.InvariantCultureIgnoreCase) >= 0 Then
            Log("loading sql")
            If System.Environment.Is64BitOperatingSystem Then
                Log(".\sql-x64\" & args.Name.Substring(0, args.Name.IndexOf(",")) & ".dll")
                Return System.Reflection.Assembly.LoadFrom(".\sql-x64\" & args.Name.Substring(0, args.Name.IndexOf(",")) & ".dll")
            Else
                Log(".\sql\" & args.Name.Substring(0, args.Name.IndexOf(",")) & ".dll")
                Return System.Reflection.Assembly.LoadFrom(".\sql\" & args.Name.Substring(0, args.Name.IndexOf(",")) & ".dll")
            End If
        End If

        If args.Name.IndexOf("SlimDX") >= 0 Then
            If System.Environment.Is64BitOperatingSystem Then
                Return System.Reflection.Assembly.LoadFrom(".\SlimDX-x64\SlimDX.dll")
            Else
                Return System.Reflection.Assembly.LoadFrom(".\SlimDX.dll")
            End If
        End If

        If args.Name.Contains("mscorlib.resources") Then Return Nothing
        Return Nothing
    End Function

    Public Sub ParseJoystickConfig()
        actions = New Hashtable
        Dim xmlJoy As New Xml.XmlDocument
        xmlJoy.Load(".\Config_Joystick.xml")

        actions.Add("loadstate", ParseJoystickConfig_(xmlJoy, "JoystickConfig/LoadState"))
        actions.Add("savestate", ParseJoystickConfig_(xmlJoy, "JoystickConfig/SaveState"))
        actions.Add("slotprev", ParseJoystickConfig_(xmlJoy, "JoystickConfig/PrevSlot"))
        actions.Add("slotnext", ParseJoystickConfig_(xmlJoy, "JoystickConfig/NextSlot"))
        actions.Add("showmenu", ParseJoystickConfig_(xmlJoy, "JoystickConfig/ShowMenu"))
        actions.Add("menu_enter", ParseJoystickConfig_(xmlJoy, "JoystickConfig/Menu_Enter"))
        actions.Add("menu_back", ParseJoystickConfig_(xmlJoy, "JoystickConfig/Menu_Back"))
        actions.Add("menu_up", ParseJoystickConfig_(xmlJoy, "JoystickConfig/Menu_Up", True))
        actions.Add("menu_down", ParseJoystickConfig_(xmlJoy, "JoystickConfig/Menu_Down", True))
        actions.Add("menu_left", ParseJoystickConfig_(xmlJoy, "JoystickConfig/Menu_Left", True))
        actions.Add("menu_right", ParseJoystickConfig_(xmlJoy, "JoystickConfig/Menu_Right", True))
    End Sub

    Private Function ParseJoystickConfig_(xmlJoy As Xml.XmlDocument, nodeName As String, Optional oneButtonNode As Boolean = False) As String
        Dim s As String
        Dim arr(2) As String
        Dim node As Xml.XmlNode = xmlJoy.SelectSingleNode(nodeName)
        Dim nodes As Xml.XmlNodeList
        If oneButtonNode Then
            nodes = xmlJoy.SelectNodes(nodeName)
        Else
            nodes = node.SelectNodes("button")
        End If

        For i = 0 To nodes.Count - 1
            arr(i) = nodes(i).InnerText.Replace("Button", "")
            arr(i) = arr(i).Replace("Up", "1000")
            arr(i) = arr(i).Replace("Down", "1001")
            arr(i) = arr(i).Replace("Left", "1002")
            arr(i) = arr(i).Replace("Right", "1003")
        Next
        Array.Sort(arr)
        s = String.Join("_", arr)
        s = s.Replace("__", "_")
        If Not s.StartsWith("_") Then s = "_" + s
        Return s
    End Function

    Public Sub Log(s As String)
        If Not logging Then Exit Sub
        Try
            FileOpen(1, ".\mCheat.log", OpenMode.Append)
            PrintLine(1, DateTime.Now.ToString + "." + DateTime.Now.Millisecond.ToString + " - " + s)
            FileClose(1)
        Catch ex As Exception
            FileOpen(1, ".\mCheat.log.access.log", OpenMode.Append)
            PrintLine(1, DateTime.Now.ToString + "." + DateTime.Now.Millisecond.ToString + " - Can't write to regular log file.")
            PrintLine(1, DateTime.Now.ToString + "." + DateTime.Now.Millisecond.ToString + " - " + s)
            FileClose(1)
        End Try
    End Sub

    Private Sub initKeyTable()
        keytable.Add("~", &HC0)
        keytable.Add("`", &HC0)
        keytable.Add("0", &H30)
        keytable.Add("1", &H31)
        keytable.Add("2", &H32)
        keytable.Add("3", &H33)
        keytable.Add("4", &H34)
        keytable.Add("5", &H35)
        keytable.Add("6", &H36)
        keytable.Add("7", &H37)
        keytable.Add("8", &H38)
        keytable.Add("9", &H39)
        keytable.Add("A", &H41)
        keytable.Add("B", &H42)
        keytable.Add("C", &H43)
        keytable.Add("D", &H44)
        keytable.Add("E", &H45)
        keytable.Add("F", &H46)
        keytable.Add("G", &H47)
        keytable.Add("H", &H48)
        keytable.Add("I", &H49)
        keytable.Add("J", &H4A)
        keytable.Add("K", &H4B)
        keytable.Add("L", &H4C)
        keytable.Add("M", &H4D)
        keytable.Add("N", &H4E)
        keytable.Add("O", &H4F)
        keytable.Add("P", &H50)
        keytable.Add("Q", &H51)
        keytable.Add("R", &H52)
        keytable.Add("S", &H53)
        keytable.Add("T", &H54)
        keytable.Add("U", &H55)
        keytable.Add("V", &H56)
        keytable.Add("W", &H57)
        keytable.Add("X", &H58)
        keytable.Add("Y", &H59)
        keytable.Add("Z", &H5A)
        keytable.Add("F1", &H70)
        keytable.Add("F2", &H71)
        keytable.Add("F3", &H72)
        keytable.Add("F4", &H73)
        keytable.Add("F5", &H74)
        keytable.Add("F6", &H75)
        keytable.Add("F7", &H76)
        keytable.Add("F8", &H77)
        keytable.Add("F9", &H78)
        keytable.Add("F10", &H79)
        keytable.Add("F11", &H7A)
        keytable.Add("F12", &H7B)
        keytable.Add("ALT", &H12)
        keytable.Add("SHIFT", &H10)
        keytable.Add("CTRL", &H11)
        keytable.Add("CONTROL", &H11)
        keytable_scan.Add("ALT", &H38)
        keytable_scan.Add("SHIFT", &H2A)
        keytable_scan.Add("CTRL", &H1D)
        keytable_scan.Add("CONTROL", &H1D)
    End Sub
End Module

Public Class MultiThreading
    Dim p() As Process
    Dim quicksaved As Boolean = True
    Private WithEvents _qSave As Shortcut = qSave
    Private WithEvents _qLoad As Shortcut = qLoad
    Public Shared Event myevent(i1 As Integer, i2 As Integer)
    Public Shared Event showmenuEvent()
    Public Shared Event menuNavigEvent(action As String)
    Public Shared Event emulatorLaunched(p As System.Diagnostics.Process, useInput As Boolean)
    Dim ThreadClass As New Class7_joystick(Me, IntPtr.Zero)
    Dim ThreadClass2 As New Class8_network_db_handler()
    Dim ThreadClass3 As New Class9_process_watcher(Me)
    Dim NewThread As New Threading.Thread(AddressOf ThreadClass.CreateDevice)
    Dim NewThread2 As New Threading.Thread(AddressOf ThreadClass2.test)
    Dim NewThread3 As New Threading.Thread(AddressOf ThreadClass3.test)
    Private context As New WindowsFormsSynchronizationContext

    Public Sub New()
        NewThread.IsBackground = True
        NewThread.Start()
        NewThread2.IsBackground = True
        NewThread2.Start()
        NewThread3.IsBackground = True
        NewThread3.Start()
    End Sub

    Public Sub ReceiveThreadMessage(ByVal param1 As Integer, ByVal param2 As Integer)
        RaiseEvent myevent(param1, param2)
    End Sub

    Public Sub CallAction(action As String)
        Dim configNode As Xml.XmlNode = getEmulatorQuickSaveConfig()
        If configNode Is Nothing Then Exit Sub
        Select Case action
            Case "savestate"
                CallAction_readQuickSaveConf(configNode, "savebutton")
            Case "loadstate"
                CallAction_readQuickSaveConf(configNode, "loadbutton")
            Case "slotprev"
                If currentSaveStateSlot > 0 Then currentSaveStateSlot = currentSaveStateSlot - 1
                CallAction_readQuickSaveConf(configNode, "slot", "prevbutton")
            Case "slotnext"
                If currentSaveStateSlot < 9 Then currentSaveStateSlot = currentSaveStateSlot + 1
                CallAction_readQuickSaveConf(configNode, "slot", "nextbutton")
            Case "showmenu"
                context.Post(New Threading.SendOrPostCallback(AddressOf t), Nothing)
            Case "menu_enter"
                context.Post(New Threading.SendOrPostCallback(AddressOf t2), DirectCast("enter", Object))
            Case "menu_back"
                context.Post(New Threading.SendOrPostCallback(AddressOf t2), DirectCast("back", Object))
            Case "menu_up"
                context.Post(New Threading.SendOrPostCallback(AddressOf t2), DirectCast("up", Object))
            Case "menu_down"
                context.Post(New Threading.SendOrPostCallback(AddressOf t2), DirectCast("down", Object))
            Case "menu_left"
                context.Post(New Threading.SendOrPostCallback(AddressOf t2), DirectCast("left", Object))
            Case "menu_right"
                context.Post(New Threading.SendOrPostCallback(AddressOf t2), DirectCast("right", Object))
        End Select
    End Sub

    Private Sub CallAction_readQuickSaveConf(configNode As Xml.XmlNode, nodename As String, Optional nodename2 As String = "")
        Dim button As String = "", buttonMod As String = ""
        If configNode.SelectSingleNode("useinputhook") IsNot Nothing Then CallAction_HookAndSaveOrLoad(nodename) : Exit Sub
        If configNode.SelectSingleNode(nodename + "0") IsNot Nothing Then
            nodename = nodename + currentSaveStateSlot.ToString
        Else
            If nodename2 <> "" Then nodename = nodename2
        End If

        If configNode.SelectSingleNode(nodename) Is Nothing Then Exit Sub
        button = configNode.SelectSingleNode(nodename).InnerText.ToUpper
        If configNode.SelectSingleNode(nodename).Attributes.GetNamedItem("modifier") IsNot Nothing Then
            buttonMod = configNode.SelectSingleNode(nodename).Attributes.GetNamedItem("modifier").Value.ToUpper
        End If
        If button = "" Then Exit Sub
        Log("Calling press button: " + button + " (" + buttonMod + ")")
        CallAction_pressButton(button, buttonMod)

        If configNode.SelectSingleNode(nodename + ".1") IsNot Nothing Then
            button = "" : buttonMod = ""
            button = configNode.SelectSingleNode(nodename + ".1").InnerText.ToUpper
            If configNode.SelectSingleNode(nodename + ".1").Attributes.GetNamedItem("modifier") IsNot Nothing Then
                buttonMod = configNode.SelectSingleNode(nodename + ".1").Attributes.GetNamedItem("modifier").Value.ToUpper
            End If
            Threading.Thread.Sleep(50)
            CallAction_pressButton(button, buttonMod)
        End If
    End Sub

    Private Sub CallAction_pressButton(button As String, buttonmod As String)
        'Log("Pressing button: " + button + "(" + keytable(button).ToString + ") (" + buttonmod + ")")
        Dim Inpts(0) As INPUT

        'Release ctrl if pressef on keyboard
        'Inpts(0).type = 1
        'Inpts(0).ki.wVk = keytable("CTRL")
        'Inpts(0).ki.wScan = keytable_scan("CTRL")
        'Inpts(0).ki.dwFlags = 2
        'SendInput(1, Inpts, Marshal.SizeOf(GetType(INPUT)))
        'Threading.Thread.Sleep(50)
        '''''''''''''''''''''''''''''''''''''

        If buttonmod <> "" Then
            Inpts(0).type = 1
            Inpts(0).ki.wVk = keytable(buttonmod)
            Inpts(0).ki.wScan = keytable_scan(buttonmod)
            SendInput(1, Inpts, Marshal.SizeOf(GetType(INPUT)))
            Threading.Thread.Sleep(50)
        End If
        Inpts(0).type = 1
        Inpts(0).ki.wVk = keytable(button)
        SendInput(1, Inpts, Marshal.SizeOf(GetType(INPUT)))
        Threading.Thread.Sleep(50)
        Inpts(0).type = 1
        Inpts(0).ki.wVk = keytable(button)
        Inpts(0).ki.dwFlags = 2
        SendInput(1, Inpts, Marshal.SizeOf(GetType(INPUT)))
        If buttonmod <> "" Then
            Threading.Thread.Sleep(50)
            Inpts(0).type = 1
            Inpts(0).ki.wVk = keytable(buttonmod)
            Inpts(0).ki.wScan = keytable_scan(buttonmod)
            Inpts(0).ki.dwFlags = 2
            SendInput(1, Inpts, Marshal.SizeOf(GetType(INPUT)))
        End If
    End Sub

    Private Sub CallAction_HookAndSaveOrLoad(action As String)
        Dim ChannelName As String = Nothing
        Dim HookServer As IpcServerChannel
        Dim imag As Image
        Dim h As Integer
        If action = "savebutton" Then
            h = 5
        ElseIf action = "loadbutton" Then
            h = 6
        Else
            Exit Sub
        End If
        If currentSaveStateSlot > 0 Then
            imag = New Bitmap(currentSaveStateSlot, h)
        Else
            imag = New Bitmap(10, h)
        End If

        If action = "loadbutton" Then
            If p(0).MainWindowTitle.ToUpper.Contains("GAMEGEAR") Then
                Dim path As String = p(0).MainModule.FileName
                path = path.Substring(0, path.LastIndexOf("\")) + "\sta\gamegear\"

                Dim rom As String = ""
                Dim QS As New System.Management.ManagementObjectSearcher("Select * from Win32_Process WHERE name like '" + p(0).ProcessName + ".exe'")
                Dim objCol As System.Management.ManagementObjectCollection = QS.Get
                For Each objMgmt In objCol
                    rom = objMgmt("commandline").ToString()
                Next
                If rom.IndexOf("cartridge", StringComparison.InvariantCultureIgnoreCase) >= 0 Then
                    rom = rom.Substring(rom.IndexOf("cartridge", StringComparison.InvariantCultureIgnoreCase) + 10).Trim
                Else
                    rom = rom.Substring(rom.IndexOf("cart", StringComparison.InvariantCultureIgnoreCase) + 5).Trim
                End If
                rom = rom.Substring(rom.IndexOf(Chr(34)) + 1).Trim
                rom = rom.Substring(0, rom.IndexOf(Chr(34))).Trim
                If rom.Contains("/") Then rom = rom.Substring(rom.LastIndexOf("/") + 1)
                If rom.Contains("\") Then rom = rom.Substring(rom.LastIndexOf("\") + 1)
                If Microsoft.VisualBasic.FileIO.FileSystem.FileExists(path + rom + currentSaveStateSlot.ToString + ".sta") Then
                    Microsoft.VisualBasic.FileIO.FileSystem.CopyFile(path + rom + currentSaveStateSlot.ToString + ".sta", path + currentSaveStateSlot.ToString + ".sta", True)
                End If
            End If
        End If

        HookServer = RemoteHooking.IpcCreateServer(Of ScreenshotInterface.ScreenshotInterface)(ChannelName, Runtime.Remoting.WellKnownObjectMode.Singleton)
        AddHandler ScreenshotInterface.ScreenshotManager.OnScreenshotDebugMessage, AddressOf OnScreenshotDebugMessage
        If Not ScreenshotInterface.HookManager.IsHooked(p(0).Id) Then
            Dim path As String = ".\ScreenshotInject.dll"
            Try
                ScreenshotInterface.HookManager.AddHookedProcess(p(0).Id)
                RemoteHooking.Inject(p(0).Id, InjectionOptions.Default, path, path, ChannelName, "Di", True)
                Log("HOOK: Dll injected")
            Catch ex As Exception
                Log("HOOK: Exception: " + ex.Message)
            End Try
        End If

        Dim stream As New System.IO.MemoryStream()
        imag.Save(stream, Imaging.ImageFormat.Bmp)
        Dim b() As Byte = stream.ToArray()
        ScreenshotInterface.ScreenshotManager.AddGraphicsUpdateRequest(p(0).Id, b)

        quicksaved = False
        Dim inp(0) As INPUT
        Do While Not quicksaved
            inp(0).type = 1
            inp(0).ki.dwFlags = 0
            inp(0).ki.wScan = 3 'And &HFF
            SendInput(1, inp, Marshal.SizeOf(GetType(INPUT)))
            Threading.Thread.Sleep(150)
            Application.DoEvents()
        Loop
        RemoveHandler ScreenshotInterface.ScreenshotManager.OnScreenshotDebugMessage, AddressOf OnScreenshotDebugMessage

        If action = "savebutton" Then
            If p(0).MainWindowTitle.ToUpper.Contains("GAMEGEAR") Then
                Dim path As String = p(0).MainModule.FileName
                path = path.Substring(0, path.LastIndexOf("\")) + "\sta\gamegear\"

                Dim rom As String = ""
                Dim QS As New System.Management.ManagementObjectSearcher("Select * from Win32_Process WHERE name like '" + p(0).ProcessName + ".exe'")
                Dim objCol As System.Management.ManagementObjectCollection = QS.Get
                For Each objMgmt In objCol
                    rom = objMgmt("commandline").ToString()
                Next
                If rom.IndexOf("cartridge", StringComparison.InvariantCultureIgnoreCase) >= 0 Then
                    rom = rom.Substring(rom.IndexOf("cartridge", StringComparison.InvariantCultureIgnoreCase) + 10).Trim
                Else
                    rom = rom.Substring(rom.IndexOf("cart", StringComparison.InvariantCultureIgnoreCase) + 5).Trim
                End If

                rom = rom.Substring(rom.IndexOf(Chr(34)) + 1).Trim
                rom = rom.Substring(0, rom.IndexOf(Chr(34))).Trim
                If rom.Contains("/") Then rom = rom.Substring(rom.LastIndexOf("/") + 1)
                If rom.Contains("\") Then rom = rom.Substring(rom.LastIndexOf("\") + 1)
                If Microsoft.VisualBasic.FileIO.FileSystem.FileExists(path + currentSaveStateSlot.ToString + ".sta") Then
                    Microsoft.VisualBasic.FileIO.FileSystem.CopyFile(path + currentSaveStateSlot.ToString + ".sta", path + rom + currentSaveStateSlot.ToString + ".sta", True)
                End If
            End If
        End If
    End Sub

    Private Sub OnScreenshotDebugMessage(ByVal clientPID As Integer, ByVal message As String)
        Log("INPUT HOOK: Hook Message: " + message)
        If message.EndsWith("QUICKSTATED") Then
            quicksaved = True
        End If
    End Sub

    Private Sub t()
        RaiseEvent showmenuEvent()
    End Sub

    Private Sub t2(o As Object)
        RaiseEvent menuNavigEvent(DirectCast(o, String))
    End Sub

    Public Function getEmulatorQuickSaveConfig() As Xml.XmlNode
        Dim node As Xml.XmlNode
        Dim nodelist As Xml.XmlNodeList
        nodelist = xmlConfig.SelectNodes("/config/emulator")
        For Each node In nodelist
            p = Process.GetProcessesByName(node.SelectSingleNode("exe").InnerText)
            If p.Length > 0 Then
                Dim s As String = node.SelectSingleNode("quicksave").InnerText
                Return xmlConfig.SelectSingleNode("/config/quicksaves/config" + s)
            End If
        Next
        Return Nothing
    End Function

    Private Sub qLoadSub() Handles _qLoad.Press
        Log("Keyboard shortcut LoadState Pressed")
        CallAction("loadstate")
    End Sub

    Private Sub qSaveSub() Handles _qSave.Press
        Log("Keyboard shortcut SaveState Pressed")
        CallAction("savestate")
    End Sub

    Public Sub receiveEmuFoundMessage(p As System.Diagnostics.Process, useInputHook As Boolean)
        Dim l As New ArrayList
        l.Add(p)
        l.Add(useInputHook)
        context.Post(New Threading.SendOrPostCallback(AddressOf callEmuFoundProc), DirectCast(l, Object))
    End Sub
    Private Sub callEmuFoundProc(o As Object)
        Dim l As ArrayList = DirectCast(o, ArrayList)
        RaiseEvent emulatorLaunched(DirectCast(l(0), System.Diagnostics.Process), DirectCast(l(1), Boolean))
    End Sub
End Class
