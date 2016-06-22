Imports System
Imports System.Threading
Imports SlimDX
Imports SlimDX.DirectInput
Public Class Class7_joystick

    Dim j As Joystick
    Dim state As JoystickState = New JoystickState()
    Dim hand As IntPtr
    Dim buttons() As Boolean
    Dim PrevButtons(127) As Boolean
    Dim prevUp, prevDawn, prevLeft, prevRight As Boolean
    Dim size2 As Integer
    Private owner As MultiThreading

    Public Sub New(o As MultiThreading, handle As IntPtr)
        owner = o
        hand = handle
        size2 = 1000
    End Sub

    Public Sub New(o As MultiThreading, handle As IntPtr, size As Integer)
        owner = o
        hand = handle
        size2 = size
    End Sub

    '~InputJoystick()
    '{
    'if (joystick != null)
    '{
    'joystick.Unacquire();
    'joystick.Dispose();
    '}
    'joystick = null;
    '}

    Public Function CreateDevice() As Boolean
        'make sure that DirectInput has been initialized
        Dim dinput As DirectInput = New DirectInput()

        'search for devices
        For Each device As DeviceInstance In dinput.GetDevices(DeviceClass.GameController, DeviceEnumerationFlags.AttachedOnly)
            Try
                'create the device
                j = New Joystick(dinput, device.InstanceGuid)
                j.SetCooperativeLevel(hand, CooperativeLevel.Nonexclusive Or CooperativeLevel.Background)
                Exit For
            Catch ex As DirectInputException
            End Try
        Next

        If j Is Nothing Then
            'Нету джостика
            Return False
        End If

        For Each deviceObject As DeviceObjectInstance In j.GetObjects()
            If (deviceObject.ObjectType And ObjectDeviceType.Axis) <> 0 Then
                j.GetObjectPropertiesById(deviceObject.ObjectType).SetRange(-size2, size2)
            End If
        Next

        j.Acquire()

        'включаем таймер
        Dim S As String
        Dim LastSend As String = ""
        Do While True
            S = ""
            If Pov(0) = 0 Then
                S = S + "_1000"
                If Not prevUp Then owner.ReceiveThreadMessage(1, 1000) : prevUp = True
            Else
                If prevUp Then owner.ReceiveThreadMessage(-1, 1000) : prevUp = False
            End If
            If Pov(0) = 18000 Then
                S = S + "_1001"
                If Not prevDawn Then owner.ReceiveThreadMessage(1, 1001) : prevDawn = True
            Else
                If prevDawn Then owner.ReceiveThreadMessage(-1, 1001) : prevDawn = False
            End If
            If Pov(0) = 27000 Then
                S = S + "_1002"
                If Not prevLeft Then owner.ReceiveThreadMessage(1, 1002) : prevLeft = True
            Else
                If prevLeft Then owner.ReceiveThreadMessage(-1, 1002) : prevLeft = False
            End If
            If Pov(0) = 9000 Then
                S = S + "_1003"
                If Not prevRight Then owner.ReceiveThreadMessage(1, 1003) : prevRight = True
            Else
                If prevRight Then owner.ReceiveThreadMessage(-1, 1003) : prevRight = False
            End If

            For i As Integer = 0 To j.Capabilities.ButtonCount - 1
                If Button(i) Then S = S + "_" + (i + 1).ToString
                If Button(i) And Not PrevButtons(i) Then owner.ReceiveThreadMessage(1, i)
                If Not Button(i) And PrevButtons(i) Then owner.ReceiveThreadMessage(-1, i)
                PrevButtons(i) = Button(i)
            Next

            Try
                For Each T As DictionaryEntry In actions
                    If T.Value.ToString = S And LastSend <> S Then
                        owner.CallAction(T.Key.ToString)
                    End If
                Next
            Catch ex As Exception
            End Try

            LastSend = S
            Application.DoEvents()
            Thread.Sleep(10)
        Loop
        Return True
    End Function
        
    Public Function Left() As Boolean
        If (j.Acquire().IsFailure) Then Return False
        If (j.Poll().IsFailure) Then Return False
        state = j.GetCurrentState()
        If Result.Last.IsFailure Then Return False
        If j Is Nothing Then Return False 'выключен

        If state.X = -size2 And state.Y <> -size2 And state.Y <> size2 Then Return True
        Return False
    End Function

    Public Function Right() As Boolean
        If (j.Acquire().IsFailure) Then Return False
        If (j.Poll().IsFailure) Then Return False
        state = j.GetCurrentState()
        If Result.Last.IsFailure Then Return False
        If j Is Nothing Then Return False 'выключен

        If state.X = size2 And state.Y <> -size2 And state.Y <> size2 Then Return True
        Return False
    End Function

    Public Function Up() As Boolean
        If (j.Acquire().IsFailure) Then Return False
        If (j.Poll().IsFailure) Then Return False
        state = j.GetCurrentState()
        If Result.Last.IsFailure Then Return False
        If j Is Nothing Then Return False 'выключен

        If state.Y = -size2 And state.X <> size2 And state.X <> -size2 Then Return True
        Return False
    End Function

    Public Function Dawn() As Boolean
        If (j.Acquire().IsFailure) Then Return False
        If (j.Poll().IsFailure) Then Return False
        state = j.GetCurrentState()
        If Result.Last.IsFailure Then Return False
        If j Is Nothing Then Return False 'выключен

        If state.Y = size2 And state.X <> size2 And state.X <> -size2 Then Return True
        Return False
    End Function

    Public Function Pov() As Integer()
        If (j.Acquire().IsFailure) Then Return {-1}
        If (j.Poll().IsFailure) Then Return {-1}
        state = j.GetCurrentState()
        If Result.Last.IsFailure Then Return {-1}
        If j Is Nothing Then Return {-1} 'выключен

        Return state.GetPointOfViewControllers
    End Function

    Public Function Button(b As Integer) As Boolean
        If (j.Acquire().IsFailure) Then Return False
        If (j.Poll().IsFailure) Then Return False
        state = j.GetCurrentState()
        If Result.Last.IsFailure Then Return False
        If j Is Nothing Then Return False 'выключен

        buttons = state.GetButtons()
        If (buttons(b) = True) Then Return True
        Return False
    End Function
End Class
