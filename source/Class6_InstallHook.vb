Imports EasyHook
Imports System.Runtime.Remoting.Channels
Imports System.Runtime.Remoting.Channels.Ipc

Public Class Class6_InstallHook
    Event onExit()
    Dim _p As Process
    Dim _AutodetectMode As String
    Dim _dontDrawAnything As Boolean
    Dim _getScreenFromHook As Boolean = False
    Dim _justHook As Boolean = False
    Dim ChannelName As String = Nothing
    Dim HookServer As IpcServerChannel

    Public Sub New(ByVal p As Process, dontDrawAnything As Boolean, hookparam As Dictionary(Of String, Short))
        _p = p
        _dontDrawAnything = dontDrawAnything
        If hookparam.ContainsKey("fromhook") Then _getScreenFromHook = True
        If hookparam.ContainsKey("hookDI") Then _AutodetectMode = "Di" Else _AutodetectMode = "AutoDetect"
        If hookparam.ContainsKey("justHook") Then _justHook = True
        HookServer = RemoteHooking.IpcCreateServer(Of ScreenshotInterface.ScreenshotInterface)(ChannelName, Runtime.Remoting.WellKnownObjectMode.Singleton)
        AddHandler ScreenshotInterface.ScreenshotManager.OnScreenshotDebugMessage, AddressOf OnScreenshotDebugMessage
        Install()
    End Sub

    Private Sub Install()
        Log("HOOK: Start Hook")
        If Not ScreenshotInterface.HookManager.IsHooked(_p.Id) Then
            Log("HOOK: Interface is not hooked, processing...")
            Dim path As String = ".\ScreenshotInject.dll"
            'Config.Register("ScreenshotInjector", path)
            Try
                ScreenshotInterface.HookManager.AddHookedProcess(_p.Id)
                RemoteHooking.Inject(_p.Id, InjectionOptions.Default, path, path, ChannelName, _AutodetectMode, True)
                Log("HOOK: Dll injected")
            Catch ex As Exception
                Log("HOOK: Exception: " + ex.Message)
            End Try
        Else
            Log("HOOK: Already hooked.")
        End If

        'Get Screenshot From Hook
        If _getScreenFromHook Then
            Dim r As New ScreenshotInterface.ScreenshotRequestResponseNotification(AddressOf gotScreen)
            ScreenshotInterface.ScreenshotManager.AddScreenshotRequest(_p.Id, New ScreenshotInterface.ScreenshotRequest(New Rectangle(0, 0, 0, 0)), r)
            Exit Sub
        End If

        If Not _justHook Then MenuUpdate()
        If _dontDrawAnything Then Exit Sub
        AddHandler MainMenu.OnExit, AddressOf MenuExit
        AddHandler MainMenu.OnUpdate, AddressOf MenuUpdate
    End Sub

    Private Sub gotScreen(clientPID As Integer, status As ScreenshotInterface.ResponseStatus, screenshotResponse As ScreenshotInterface.ScreenshotResponse)
        MainMenu.setImage(screenshotResponse.CapturedBitmapAsImage)
        MenuUpdate()
        If _dontDrawAnything Then Exit Sub
        AddHandler MainMenu.OnExit, AddressOf MenuExit
        AddHandler MainMenu.OnUpdate, AddressOf MenuUpdate
    End Sub

    Public Sub MenuUpdate()
        Dim stream As New System.IO.MemoryStream()
        If _dontDrawAnything = False Then
            MainMenu.getEntireImage.Save(stream, Imaging.ImageFormat.Bmp)
        Else
            Dim i As Image = New Bitmap(1, 2)
            i.Save(stream, Imaging.ImageFormat.Bmp)
        End If

        Dim b() As Byte = stream.ToArray()
        ScreenshotInterface.ScreenshotManager.AddGraphicsUpdateRequest(_p.Id, b)
    End Sub

    Public Sub MenuExit()
        RaiseEvent onExit()
    End Sub

    Public Sub _exit()
        Dim img As Image = New Bitmap(1, 1)
        Dim stream As New System.IO.MemoryStream()
        img.Save(stream, Imaging.ImageFormat.Bmp)
        ScreenshotInterface.ScreenshotManager.AddGraphicsUpdateRequest(_p.Id, stream.ToArray())
        RemoveHandler ScreenshotInterface.ScreenshotManager.OnScreenshotDebugMessage, AddressOf OnScreenshotDebugMessage

        If MainMenu Is Nothing Then Exit Sub
        RemoveHandler MainMenu.OnExit, AddressOf MenuExit
        RemoveHandler MainMenu.OnUpdate, AddressOf MenuUpdate
    End Sub

    Private Sub OnScreenshotDebugMessage(ByVal clientPID As Integer, ByVal message As String)
        Log("HOOK: Hook Message: " + message)
        If MainMenu IsNot Nothing Then
            If message.StartsWith("DXHookDD: FirstCall") Then
                MainMenu.Update()
            End If
            If message.EndsWith(": HOOKED") And _dontDrawAnything Then
                MainMenu.Update()
                'testarray.Add("hook end: " + Now.Ticks.ToString)
            End If
            'If message = "DXHookDD: SetDisplayMode" Or message = "DXHookDD: RestoreDisplayMode" Then MainMenu.Update()
        End If
        'MsgBox(message)
    End Sub
End Class
