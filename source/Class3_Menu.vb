Imports System.Runtime.InteropServices
Public Class Class3_Menu
#Region "DECLARATIONS"
    Implements IDisposable
    Private disposed As Boolean = False
    Event OnExit()
    Event OnUpdate()
    Dim p(13) As Point
    Dim watch As Stopwatch
    Dim entireimage As Image
    Private menuX As Integer = 0
    Private menuLevel As Integer = 0
    Private submenuof As Integer = 0
    Private subMenu(300) As ArrayList
    Private realCheatNumberArr(500) As Integer
    Private menuCount As Integer = 0
    Private xSelector As Integer = 0
    Private ySelector As Integer = 0
    Private SelectorStep As Integer = 5
    Private SelectorStop As Integer = 35
    Private MenuWidth As Integer = 0
    Private MenuOffset As Integer = 0
    Private MenuOffsetMax As Integer = 0
    Private iScreen As Image
    Private ibufer As Image
    Private ibackground As Image
    Private ibackgroundFromEmu As Image
    Private iMainMenu As Image = New Bitmap(1920, 4000)
    Private iMainMenuBrown As Image = New Bitmap(1920, 4000)
    Private gScreen As Graphics
    Private gBufer As Graphics
    Private gBackground As Graphics
    Private gBackgroundFromEmu As Graphics
    Private gMainMenu As Graphics = Graphics.FromImage(iMainMenu)
    Private gMainMenuBrown As Graphics = Graphics.FromImage(iMainMenuBrown)
    Private WithEvents timer As New Timer
    Private WithEvents timer_checkVisibility As New Timer
    Private EmuWinID As IntPtr = IntPtr.Zero
    Private subMenuActive As Boolean = False
    Private askingValueActive As String = ""
    Private askingValueNewValue As String = ""
    Private askingValueCurrent As Integer = 0
    Private askingValueItemNumber As Integer = 0
    Private WithEvents smenu As Class3_Menu
    Private WithEvents KeyUpLocal As Shortcut = KeyUp
    Private WithEvents KeyDownlocal As Shortcut = KeyDown
    Private WithEvents KeyLeftlocal As Shortcut = KeyLeft
    Private WithEvents KeyRightlocal As Shortcut = KeyRight
    Private WithEvents KeyEnterlocal As Shortcut = KeyEnter
    Private WithEvents KeyBackspacelocal As Shortcut = KeyBackSpace
    <DllImport("gdi32.dll")> _
    Public Shared Function BitBlt(ByVal hdcDest As IntPtr, ByVal xDest As Integer, ByVal yDest As Integer, ByVal wDest As Integer, ByVal hDest As Integer, ByVal hdcSource As IntPtr, ByVal xSrc As Integer, ByVal ySrc As Integer, ByVal RasterOp As Integer) As Boolean
    End Function
    <DllImport("user32.dll")> _
    Private Shared Function ReleaseDC(ByVal hWnd As IntPtr, ByVal hDc As IntPtr) As Boolean
    End Function
    <DllImport("user32.dll")> _
    Private Shared Function GetDC(ByVal hWnd As IntPtr) As IntPtr
    End Function
    <DllImport("gdi32.dll")> _
    Private Shared Function DeleteDC(ByVal hDc As IntPtr) As IntPtr
    End Function
    <DllImport("gdi32.dll")> _
    Private Shared Function DeleteObject(ByVal hDc As IntPtr) As IntPtr
    End Function
    <DllImport("gdi32.dll")> _
    Private Shared Function CreateCompatibleBitmap(ByVal hdc As IntPtr, ByVal nWidth As Integer, ByVal nHeight As Integer) As IntPtr
    End Function
    <DllImport("gdi32.dll")> _
    Private Shared Function CreateCompatibleDC(ByVal hdc As IntPtr) As IntPtr
    End Function
    <DllImport("gdi32.dll")> _
    Private Shared Function SelectObject(ByVal hdc As IntPtr, ByVal bmp As IntPtr) As IntPtr
    End Function

#End Region
    Dim tmparr As New ArrayList

    Public Sub New(ByVal winID As IntPtr, ByVal mainmenu As ArrayList, ByVal menuType As Integer, Optional ByVal useIntPtrZero As Boolean = False, Optional ByVal wr As Rectangle = Nothing, Optional ByVal sOf As Integer = 0, Optional ByVal HOOK As Boolean = False)
        EmuWinID = winID
        timer.Interval = 50
        menuLevel = menuType
        menuX = menuType * 40
        submenuof = sOf

        'create graphics from emulator window and get its width/height
        gScreen = Graphics.FromHwnd(EmuWinID)
        gScreen.CompositingMode = Drawing2D.CompositingMode.SourceCopy
        gScreen.TextRenderingHint = Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit
        Dim emuw As Integer = Convert.ToInt32(gScreen.VisibleClipBounds.Width)
        Dim emuh As Integer = Convert.ToInt32(gScreen.VisibleClipBounds.Height)

        'create graphics with emu's window w/h and copy emu screen to it, than release HDCs
        If useIntPtrZero Then
            'Use form overlay (use copy from screen)
            If wr <> Nothing Then
                ibackgroundFromEmu = New Bitmap(wr.Width, wr.Height)
                gBackgroundFromEmu = Graphics.FromImage(ibackgroundFromEmu)
                gBackgroundFromEmu.CopyFromScreen(wr.Left, wr.Top, 0, 0, New Size(wr.Width, wr.Height))
            Else
                ibackgroundFromEmu = New Bitmap(emuw, emuh)
                gBackgroundFromEmu = Graphics.FromImage(ibackgroundFromEmu)
                gBackgroundFromEmu.CopyFromScreen(0, 0, 0, 0, New Size(emuw, emuh))
            End If
        Else
            'Use emu screen (copy from emu window)
            ibackgroundFromEmu = New Bitmap(emuw, emuh)
            gBackgroundFromEmu = Graphics.FromImage(ibackgroundFromEmu)
            BitBlt(gBackgroundFromEmu.GetHdc, 0, 0, emuw, emuh, gScreen.GetHdc, 0, 0, 13369376)
            gBackgroundFromEmu.ReleaseHdc() : gScreen.ReleaseHdc()
        End If

        If HOOK Then
            'this is a signal to use a hook, so draw gScreen on image instead of emuWindow
            iScreen = New Bitmap(emuw, emuh)
            gScreen = Graphics.FromImage(iScreen)
            gScreen.DrawImageUnscaled(ibackgroundFromEmu, 0, 0)
        End If

        'Create Font and brushes
        Dim drawFont As New Font("Arial", 20, FontStyle.Bold)
        Dim textBrush As New Drawing2D.LinearGradientBrush(New Rectangle(0, 0, 2000, 500), Color.LightBlue, Color.LightSkyBlue, Drawing2D.LinearGradientMode.Horizontal)
        Dim backBrush As New Drawing2D.LinearGradientBrush(New Rectangle(0, 0, 300, 500), Color.FromArgb(100, Color.Blue), Color.FromArgb(100, Color.LightSkyBlue), Drawing2D.LinearGradientMode.Vertical)
        Dim drawFormat As New StringFormat With {.FormatFlags = StringFormatFlags.NoFontFallback}

        'Draw menu items, create submenu array (if needed), calculate menuwidth and MenuOffsetMax
        'Try
        Dim cheatNumber As Integer = 0
        Dim realCheatNumber As Integer = 0
        If menuLevel <> 0 Then realCheatNumber += Convert.ToInt32(mainmenu(0).ToString.Substring(mainmenu(0).ToString.Length - 5))
        Dim sm As New Hashtable, arr As New ArrayList 'Think sm as 'ALLREADY DRAWN' array
        Dim t As String, i As Integer = 0, w As Integer = 0, lastDrawed As String = ""
        For Each s As String In mainmenu
            If s.Contains("\") Then t = s.Substring(0, s.IndexOf("\")) Else If menuLevel = 0 Then t = s Else t = s.Substring(0, s.Length - 5)
            If lastDrawed <> t Then
                If Not s.Contains("\") Then realCheatNumberArr(menuCount) = realCheatNumber
                menuCount += 1
                gMainMenu.DrawString(t, drawFont, textBrush, 0, i, drawFormat)
                i += 30
            End If
            If s.Contains("\") Then
                If lastDrawed <> t Then
                    w = CType(gMainMenu.MeasureString(t, drawFont).Width, Integer)
                    Dim p(2) As Point : p(0) = New Point(w + 20, i - 30 + 5) : p(1) = New Point(w + 20, i - 30 + 25) : p(2) = New Point(w + 35, i - 30 + 15)
                    gMainMenu.FillPolygon(Brushes.Aquamarine, p)
                End If

                If subMenu(menuCount) Is Nothing Then subMenu(menuCount) = New ArrayList
                If menuLevel = 0 Then
                    subMenu(menuCount).Add(s.Substring(s.IndexOf("\") + 1) + Format(cheatNumber, "00000"))
                Else
                    subMenu(menuCount).Add(s.Substring(s.IndexOf("\") + 1))
                End If
            End If
            lastDrawed = t
            cheatNumber += 1 : realCheatNumber += 1
            w = CType(gMainMenu.MeasureString(t, drawFont).Width, Integer)
            If w > MenuWidth Then MenuWidth = w
        Next
        'Catch ex As Exception
        'MsgBox("MenuModule: " + ex.Message)
        'End Try
        MenuWidth = MenuWidth + 50
        If MenuWidth > emuw - 60 Then MenuWidth = emuw - 60
        MenuOffsetMax = menuCount - 13 : If MenuOffsetMax < 0 Then MenuOffsetMax = 0
        CreateMainMenuBrown(mainmenu, drawFont, textBrush, drawFormat)

        'Create gbufer and gBackground with appropriate width and height=500
        ibufer = New Bitmap(emuw, emuh)
        'ibufer = New Bitmap(MenuWidth, 500)
        gBufer = Graphics.FromImage(ibufer)
        'ibackground = New Bitmap(MenuWidth, 500)
        ibackground = New Bitmap(emuw, emuh)
        gBackground = Graphics.FromImage(ibackground)

        'Draw rectangle on gBackground
        gBackground.DrawRectangle(Pens.Aquamarine, 0, 0, MenuWidth - 1, 400)
        gBackground.FillRectangle(backBrush, 1, 1, MenuWidth - 2, 398)

        'Draw on gbuffer <- ibackgroundFromEmu <- background <- mainmenu
        'gBufer.DrawImage(ibackgroundFromEmu, 0, 0, New Rectangle(menuX + 30, menuX + 30, MenuWidth, 500), GraphicsUnit.Pixel)
        Dim tt As Integer = menuX + 30
        gBufer.DrawImage(ibackgroundFromEmu, 0, 0, New Rectangle(tt, tt, emuw - tt, emuh - tt), GraphicsUnit.Pixel)
        gBufer.DrawImage(ibackground, 0, 0)
        gBufer.DrawImage(iMainMenu, 20, 0, New Rectangle(0, 0, MenuWidth - 30, 390), GraphicsUnit.Pixel)

        'Draw circles to the left of active cheats
        For i = 0 To menuCount - 1
            If i > 12 Then Exit For
            If subMenu(i + 1) Is Nothing AndAlso CheatsStatus(realCheatNumberArr(i)) IsNot Nothing AndAlso CheatsStatus(realCheatNumberArr(i)).ToString = "1" Then
                gBufer.FillEllipse(Brushes.LightGreen, 3, i * 30 + 5, 20, 20)
            End If
        Next
        AddHandler MultiThreading.menuNavigEvent, AddressOf processJoy
        drawSelector(0, 0)
    End Sub

    Private Sub processJoy(a As String)
        If a = "enter" Then KeyEnterPress(New Object, New Shortcut.HotKeyEventArgs)
        If a = "back" Then KeyBackspacePress(New Object, New Shortcut.HotKeyEventArgs)
        If a = "up" Then KeyUpPress(New Object, New Shortcut.HotKeyEventArgs)
        If a = "down" Then KeyDownPress(New Object, New Shortcut.HotKeyEventArgs)
    End Sub

    Public Sub drawSelector(ByVal x As Integer, ByVal y As Integer)
        x += 30 : y += 30
        x += menuX : y += menuX

        'CreatePointsArrayForSelector(x, y)
        'gScreen.DrawImage(ibufer, x, y - 30, New Rectangle(x - 30, y - 60, x + 200 - 30, y + 60 - 30), GraphicsUnit.Pixel)
        Try
            'Dim ptrdst As IntPtr = gScreen.GetHdc
            'Dim ptrsrc As IntPtr = CreateCompatibleDC(ptrdst)
            'SelectObject(ptrsrc, New Bitmap(ibackgroundFromEmu).GetHbitmap)
            'BitBlt(ptrdst, 28, 478, ibackgroundFromEmu.Width - 56, ibackgroundFromEmu.Height - 506, ptrsrc, 28, 478, 13369376)

            'SelectObject(ptrsrc, New Bitmap(ibufer).GetHbitmap)
            'BitBlt(ptrdst, menuX + 30, menuX + 30, ibufer.Width, ibufer.Height, ptrsrc, 0, 0, 13369376)
            'DeleteObject(ptrsrc)
            'gScreen.ReleaseHdc()

            gScreen.DrawImage(ibufer, menuX + 30, menuX + 30)
            'gScreen.FillClosedCurve(Brushes.Brown, p, Drawing2D.FillMode.Winding, 0.2)
            'gScreen.DrawCurve(New Pen(Color.LightCyan, 3), p, 0.2)
            'gScreen.DrawImage(iMainMenu, x + 20, y + 4, New Rectangle(x - 30 - menuX, y - 30 - menuX + MenuOffset * 30 + 4, MenuWidth - 30, 25), GraphicsUnit.Pixel)
            gScreen.DrawImage(iMainMenuBrown, x + 20, y + 1, New Rectangle(x - 30 - menuX, (y - 30 - menuX + MenuOffset * 30 + 1) + (ySelector \ 30 + MenuOffset) * 10, MenuWidth - 30, 31), GraphicsUnit.Pixel)

            Dim curMenuItem As Integer = ySelector \ 30 + MenuOffset
            If subMenu(curMenuItem + 1) Is Nothing AndAlso CheatsStatus(realCheatNumberArr(curMenuItem)) IsNot Nothing AndAlso CheatsStatus(realCheatNumberArr(curMenuItem)).ToString = "1" Then
                gScreen.FillEllipse(Brushes.LightGreen, x + 3, y + 5, 20, 20)
            End If
            DrawNotes(curMenuItem)

            If askingValueActive <> "" Then AskValue2()
            RaiseEvent OnUpdate()
        Catch ex As Exception
            RaiseEvent OnExit()
        End Try
    End Sub

    Private Sub timertick(ByVal sender As Object, ByVal e As System.EventArgs) Handles timer.Tick
        drawSelector(xSelector, ySelector)
        ySelector += SelectorStep
        If ySelector = SelectorStop Then drawSelector(xSelector, ySelector) : timer.Enabled = False
    End Sub

    Private Sub KeyDownPress(ByVal s As Object, ByVal e As Shortcut.HotKeyEventArgs) Handles KeyDownlocal.Press
        'watch = Stopwatch.StartNew()
        If subMenuActive Then Exit Sub
        If askingValueActive <> "" Then AskValue3_changeValue("DOWN") : Exit Sub
        ySelector += 30
        If ySelector > 30 * menuCount - 30 Then ySelector = 30 * menuCount - 30 : Exit Sub
        If ySelector > 360 Then
            ySelector = 360
            If MenuOffset = MenuOffsetMax Then Exit Sub
            MenuOffset += 1
            gBufer.DrawImage(ibackgroundFromEmu, 0, 0, New Rectangle(menuX + 30, menuX + 30, MenuWidth, 500), GraphicsUnit.Pixel)
            gBufer.DrawImage(ibackground, 0, 0)
            gBufer.DrawImage(iMainMenu, 20, 0, New Rectangle(0, 0 + MenuOffset * 30, MenuWidth - 35, 390), GraphicsUnit.Pixel)

            'Draw circles to the left of active cheats
            For i = 0 + MenuOffset To menuCount - 1
                If i > 12 + MenuOffset Then Exit For
                If subMenu(i + 1) Is Nothing AndAlso CheatsStatus(realCheatNumberArr(i)) IsNot Nothing AndAlso CheatsStatus(realCheatNumberArr(i)).ToString = "1" Then
                    gBufer.FillEllipse(Brushes.LightGreen, 3, (i - MenuOffset) * 30 + 5, 20, 20)
                End If
            Next
        End If
        drawSelector(xSelector, ySelector)
        'SelectorStep = 5
        'SelectorStop = ySelector + 30
        'timer.Enabled = True
    End Sub

    Private Sub KeyUpPress(ByVal s As Object, ByVal e As Shortcut.HotKeyEventArgs) Handles KeyUpLocal.Press
        If subMenuActive Then Exit Sub
        If askingValueActive <> "" Then AskValue3_changeValue("UP") : Exit Sub
        ySelector -= 30
        If ySelector < 0 Then
            ySelector = 0
            If MenuOffset = 0 Then Exit Sub
            MenuOffset -= 1
            gBufer.DrawImage(ibackgroundFromEmu, 0, 0, New Rectangle(menuX + 30, menuX + 30, MenuWidth, 500), GraphicsUnit.Pixel)
            gBufer.DrawImage(ibackground, 0, 0)
            gBufer.DrawImage(iMainMenu, 20, 0, New Rectangle(0, 0 + MenuOffset * 30, MenuWidth - 35, 390), GraphicsUnit.Pixel)

            'Draw circles to the left of active cheats
            For i = 0 + MenuOffset To menuCount - 1
                If i > 12 + MenuOffset Then Exit For
                If subMenu(i + 1) Is Nothing AndAlso CheatsStatus(realCheatNumberArr(i)) IsNot Nothing AndAlso CheatsStatus(realCheatNumberArr(i)).ToString = "1" Then
                    gBufer.FillEllipse(Brushes.LightGreen, 3, (i - MenuOffset) * 30 + 5, 20, 20)
                End If
            Next
        End If
        drawSelector(xSelector, ySelector)
        'SelectorStep = -5
        'SelectorStop = ySelector - 30
        'timer.Enabled = True
    End Sub

    Private Sub KeyEnterPress(ByVal s As Object, ByVal e As Shortcut.HotKeyEventArgs) Handles KeyEnterlocal.Press
        If subMenuActive Then Exit Sub
        If askingValueActive <> "" Then
            askingValueActive = ""
            KeyLeft.Unregister() : KeyRight.Unregister()
            gScreen.DrawImage(ibackgroundFromEmu, 0, 0)
            drawSelector(xSelector, ySelector)
            Exit Sub
        End If
        Dim curMenuItem As Integer = ySelector \ 30 + MenuOffset + 1
        If subMenu(curMenuItem) Is Nothing Then ChangeActivity() : Exit Sub
        subMenuActive = True
        If iScreen Is Nothing Then
            smenu = New Class3_Menu(EmuWinID, subMenu(curMenuItem), 1, False, Nothing, curMenuItem)
        Else
            'smenu = New Class3_Menu(EmuWinID, subMenu(curMenuItem), 1, False, New Rectangle(1, 1, 1, 1), curMenuItem)
            smenu = New Class3_Menu(EmuWinID, subMenu(curMenuItem), 1, False, Nothing, curMenuItem, True)
            AddHandler smenu.OnUpdate, AddressOf OnUpdate_Raised
            RaiseEvent OnUpdate()
        End If
        AddHandler smenu.OnExit, AddressOf OnExit_raised
    End Sub

    Private Sub KeyBackspacePress(ByVal s As Object, ByVal e As Shortcut.HotKeyEventArgs) Handles KeyBackspacelocal.Press
        If subMenuActive Then Exit Sub
        If askingValueActive <> "" Then
            askingValueActive = ""
            KeyLeft.Unregister() : KeyRight.Unregister()
            gScreen.DrawImage(ibackgroundFromEmu, 0, 0)
            drawSelector(xSelector, ySelector)
            Exit Sub
        End If
        RaiseEvent OnExit()
    End Sub

    Private Sub KeyLeftPress(ByVal s As Object, ByVal e As Shortcut.HotKeyEventArgs) Handles KeyLeftlocal.Press
        If askingValueCurrent = 0 Then Exit Sub
        askingValueCurrent = askingValueCurrent - 1
        Dim br As Brush
        For i As Integer = 0 To askingValueNewValue.Length - 1
            Dim p(2) As Point
            If i = askingValueCurrent Then br = Brushes.Red Else br = Brushes.LightPink
            p(0) = New Point(120 + i * 60, 165) : p(1) = New Point(150 + i * 60, 165) : p(2) = New Point(135 + i * 60, 145)
            gScreen.FillPolygon(br, p)
            p(0) = New Point(120 + i * 60, 225) : p(1) = New Point(150 + i * 60, 225) : p(2) = New Point(135 + i * 60, 245)
            gScreen.FillPolygon(br, p)
        Next
        RaiseEvent OnUpdate()
    End Sub

    Private Sub KeyRightPress(ByVal s As Object, ByVal e As Shortcut.HotKeyEventArgs) Handles KeyRightlocal.Press
        If askingValueCurrent = askingValueNewValue.Length - 1 Then Exit Sub
        askingValueCurrent = askingValueCurrent + 1
        Dim br As Brush
        For i As Integer = 0 To askingValueNewValue.Length - 1
            Dim p(2) As Point
            If i = askingValueCurrent Then br = Brushes.Red Else br = Brushes.LightPink
            p(0) = New Point(120 + i * 60, 165) : p(1) = New Point(150 + i * 60, 165) : p(2) = New Point(135 + i * 60, 145)
            gScreen.FillPolygon(br, p)
            p(0) = New Point(120 + i * 60, 225) : p(1) = New Point(150 + i * 60, 225) : p(2) = New Point(135 + i * 60, 245)
            gScreen.FillPolygon(br, p)
        Next
        RaiseEvent OnUpdate()
    End Sub

    Private Sub ChangeActivity()
        Dim curMenuItem As Integer = ySelector \ 30 + MenuOffset
        If CheatsStatus(realCheatNumberArr(curMenuItem)) Is Nothing Then
            AskValue()
            CheatsStatus.Add(realCheatNumberArr(curMenuItem), 1)
            gBufer.FillEllipse(Brushes.LightGreen, 3, ySelector + 5, 20, 20)
        Else
            If CheatsStatus(realCheatNumberArr(curMenuItem)).ToString = "0" Then
                AskValue()
                CheatsStatus(realCheatNumberArr(curMenuItem)) = 1
                gBufer.FillEllipse(Brushes.LightGreen, 3, ySelector + 5, 20, 20)
            Else
                CheatsStatus.Remove(realCheatNumberArr(curMenuItem))
                Dim b As New SolidBrush(Color.FromArgb(0, 0, 0, 0))
                gBufer.DrawImage(ibackgroundFromEmu, 3, ySelector + 5, New Rectangle(3, ySelector + 5, 20, 20), GraphicsUnit.Pixel)
                gBufer.DrawImage(ibackground, 3, ySelector + 5, New Rectangle(3, ySelector + 5, 20, 20), GraphicsUnit.Pixel)
            End If
        End If
        drawSelector(xSelector, ySelector)
    End Sub

    Private Sub OnExit_raised()
        If subMenuActive Then
            subMenuActive = False
            drawSelector(xSelector, ySelector)
            smenu.Dispose()
            smenu = Nothing
        End If
    End Sub

    Private Sub OnUpdate_Raised()
        RaiseEvent OnUpdate()
    End Sub

    Public Sub reassignHwnd(ByVal hwnd As IntPtr)
        EmuWinID = hwnd
        gScreen = Graphics.FromHwnd(EmuWinID)
    End Sub

    Public Function getEntireImage() As Image
        If iScreen IsNot Nothing Then
            If subMenuActive Then
                gScreen.DrawImage(smenu.getEntireImage, 0, 0)
            End If
            Return iScreen
        End If

        entireimage = New Bitmap(ibackgroundFromEmu.Width, ibackgroundFromEmu.Height)
        Dim entiregraphics As Graphics = Graphics.FromImage(entireimage)
        entiregraphics.DrawImage(ibackgroundFromEmu, 0, 0)
        entiregraphics.DrawImage(ibufer, menuX + 30, menuX + 30)
        Return entireimage
    End Function

    Private Sub CreatePointsArrayForSelector(ByVal x As Integer, ByVal y As Integer)
        Dim w As Integer = MenuWidth - 5
        w = w - 25
        p(0) = New Point(x + w - 10, y + 2)
        p(1) = New Point(x + 190, y + 2)
        p(2) = New Point(x + 10, y + 2)
        p(3) = New Point(x + 2, y + 10)
        p(4) = New Point(x + 2, y + 20)
        p(5) = New Point(x + 10, y + 30)
        p(6) = New Point(x + 190, y + 30)
        p(7) = New Point(x + w - 190, y + 30)
        p(8) = New Point(x + w - 10, y + 30)
        p(9) = New Point(x + w - 2, y + 20)
        p(10) = New Point(x + w - 2, y + 10)
        p(11) = New Point(x + w - 10, y + 2)
        p(12) = New Point(x + w - 190, y + 2)
        p(13) = New Point(x + 10, y + 2)
    End Sub

    Private Sub DrawNotes(curMenuItem As Integer)
        Dim a As ArrayList
        a = TryCast(cl.getCheatsCheatNotes(cl.getCheatsCheatNames(realCheatNumberArr(curMenuItem))), ArrayList)
        If a Is Nothing Then Exit Sub 'ZAGLUSHKA. ETO PROISHODIT KOGDA 'sorry, no cheats for this game'

        Dim noterectangle As New Rectangle(30, 480, ibackgroundFromEmu.Width - 60, ibackgroundFromEmu.Height - 510)
        If a.Count = 0 Or subMenu(curMenuItem + 1) IsNot Nothing Then
            gScreen.DrawImage(ibackgroundFromEmu, 28, 478, New Rectangle(28, 478, ibackgroundFromEmu.Width - 56, ibackgroundFromEmu.Height - 506), GraphicsUnit.Pixel)
            Exit Sub
        End If

        If ibackgroundFromEmu.Height < 600 Then Exit Sub
        Dim str As String = ""
        Dim notepen As New Pen(New SolidBrush(Color.LightGreen), 3)
        Dim drawFont As New Font("Arial", 12, FontStyle.Bold)
        For Each s As String In a
            str = str + s + vbCrLf
        Next
        gScreen.DrawRectangle(notepen, noterectangle)
        gScreen.FillRectangle(Brushes.Blue, 32, 482, ibackgroundFromEmu.Width - 64, ibackgroundFromEmu.Height - 514)
        If watch IsNot Nothing Then watch.Stop() : str = watch.Elapsed.TotalMilliseconds.ToString + vbCrLf + str
        gScreen.DrawString(str, drawFont, Brushes.LightGreen, noterectangle)
    End Sub

    Private Sub CreateMainMenuBrown(ByVal mm As ArrayList, ByVal drawfont As Font, ByVal textbrush As Brush, ByVal drawformat As StringFormat)
        'Dim sm As New Hashtable
        Dim previous As String = ""
        Dim t As String, i As Integer = 0, w As Integer
        For Each s As String In mm
            If s.Contains("\") Then t = s.Substring(0, s.IndexOf("\")) Else If menuLevel = 0 Then t = s Else t = s.Substring(0, s.Length - 5)
            If previous <> t Then
                'If sm(t) Is Nothing Then
                CreatePointsArrayForSelector(0, i)
                gMainMenuBrown.FillClosedCurve(Brushes.Brown, p, Drawing2D.FillMode.Winding, 0.2)
                gMainMenuBrown.DrawCurve(New Pen(Color.LightCyan, 3), p, 0.2)
                'gMainMenuBrown.DrawString(t, drawfont, textbrush, 0, i, drawformat)
                Dim r As New RectangleF(0, i, MenuWidth - 20.0F, 30)
                gMainMenuBrown.DrawString(t, drawfont, textbrush, r, drawformat)
                i += 40
                previous = t
                'sm.Add(t, "filled")

                If s.Contains("\") Then
                    w = CType(gMainMenu.MeasureString(t, drawfont).Width, Integer)
                    Dim p(2) As Point : p(0) = New Point(w + 20, i - 40 + 5) : p(1) = New Point(w + 20, i - 40 + 25) : p(2) = New Point(w + 35, i - 40 + 15)
                    gMainMenuBrown.FillPolygon(Brushes.Aquamarine, p)
                End If
            End If
        Next
    End Sub

    Private Sub AskValue()
        Dim curMenuItem As Integer = ySelector \ 30 + MenuOffset
        Dim i As Integer = 0
        For Each s As String In DirectCast(cl.getCheatsCheatCodes_orig(cl.getCheatsCheatNames(realCheatNumberArr(curMenuItem))), ArrayList)
            s = s.Replace("x", "?").Replace("X", "?")
            If s.Contains("?") Then askingValueItemNumber = i : askingValueActive = s : Exit Sub
            i = i + 1
        Next
    End Sub

    Private Sub AskValue2()
        askingValueNewValue = ""
        Dim curMenuItem As Integer = ySelector \ 30 + MenuOffset
        Dim curValue As String = DirectCast(DirectCast(cl.getCheatsCheatCodes(cl.getCheatsCheatNames(realCheatNumberArr(curMenuItem))), ArrayList)(askingValueItemNumber), String)

        Dim qpen As New Pen(New SolidBrush(Color.LightGreen), 3)
        Dim drawFont As New Font("Arial", 24, FontStyle.Bold)
        Dim askFont As New Font("Courier New", 64, FontStyle.Bold)
        Dim qrectangle As New Rectangle(90, 90, ibackgroundFromEmu.Width - 180, 200)
        gScreen.DrawRectangle(qpen, qrectangle)
        gScreen.FillRectangle(Brushes.Blue, 92, 92, ibackgroundFromEmu.Width - 184, 196)
        gScreen.DrawString("This code need a value. Enter in HEX:", drawFont, Brushes.LightGreen, 95, 95)

        Dim br As Brush
        Dim count As Integer = askingValueActive.LastIndexOf("?") - askingValueActive.IndexOf("?") + 1
        For i As Integer = 0 To count - 1
            Dim t As String = curValue.Substring(askingValueActive.IndexOf("?") + i, 1)
            If t = "?" Or t = "x" Or t = "X" Then t = "0"
            gScreen.DrawString(t, askFont, Brushes.White, 95 + i * 60, 150)
            askingValueNewValue = askingValueNewValue + t
            Dim p(2) As Point
            If i = 0 Then br = Brushes.Red Else br = Brushes.LightPink
            p(0) = New Point(120 + i * 60, 165) : p(1) = New Point(150 + i * 60, 165) : p(2) = New Point(135 + i * 60, 145)
            gScreen.FillPolygon(br, p)
            p(0) = New Point(120 + i * 60, 225) : p(1) = New Point(150 + i * 60, 225) : p(2) = New Point(135 + i * 60, 245)
            gScreen.FillPolygon(br, p)
        Next
        askingValueCurrent = 0
        KeyLeft.Register() : KeyRight.Register()
    End Sub

    Private Sub AskValue3_changeValue(ByVal button As String)
        Dim askFont As New Font("Courier New", 64, FontStyle.Bold)
        Dim curHalfByte As String = askingValueNewValue.Substring(askingValueCurrent, 1)
        If button = "UP" Then
            If curHalfByte = "F" Then curHalfByte = "0" Else curHalfByte = Hex(CInt("&H" + curHalfByte) + 1)
        End If
        If button = "DOWN" Then
            If curHalfByte = "0" Then curHalfByte = "F" Else curHalfByte = Hex(CInt("&H" + curHalfByte) - 1)
        End If
        Dim newvalue As String = askingValueNewValue.Substring(0, askingValueCurrent) + curHalfByte
        If askingValueNewValue.Length > askingValueCurrent + 1 Then newvalue = newvalue + askingValueNewValue.Substring(askingValueCurrent + 1)
        askingValueNewValue = newvalue
        gScreen.FillRectangle(Brushes.Blue, 105 + askingValueCurrent * 60, 167, 60, 57)
        gScreen.DrawString(curHalfByte, askFont, Brushes.White, 95 + askingValueCurrent * 60, 150)

        'Dim ml As String = ""
        Dim origVal As String
        'If menuLevel > 0 Then ml = submenuof.ToString + ":"
        'Dim curMenuItem As String = ml + (ySelector \ 30 + MenuOffset + 1).ToString
        Dim curMenuItem As Integer = ySelector \ 30 + MenuOffset
        origVal = DirectCast(DirectCast(cl.getCheatsCheatCodes_orig(cl.getCheatsCheatNames(realCheatNumberArr(curMenuItem))), ArrayList)(askingValueItemNumber), String)
        origVal = origVal.Replace("x", "?").Replace("X", "?")
        Mid(askingValueActive, origVal.IndexOf("?") + 1, newvalue.Length) = newvalue
        DirectCast(cl.getCheatsCheatCodes(cl.getCheatsCheatNames(realCheatNumberArr(curMenuItem))), ArrayList)(askingValueItemNumber) = askingValueActive
        RaiseEvent OnUpdate()
    End Sub

    Public Sub Update()
        drawSelector(xSelector, ySelector)
    End Sub

    Public Sub Update2()
        watch = Stopwatch.StartNew()
        Do While watch.Elapsed.TotalSeconds < 2
            Application.DoEvents()
            Update3()
        Loop
        watch.Stop()
    End Sub

    Public Sub Update3()
        Dim tmpimg As New Bitmap(1, 1)
        Dim tmpgfx As Graphics = Graphics.FromImage(tmpimg)

        Dim ptrsrc As IntPtr = GetDC(EmuWinID)
        BitBlt(tmpgfx.GetHdc, 0, 0, 1, 1, ptrsrc, 30, 30, 13369376)
        ReleaseDC(EmuWinID, ptrsrc)
        tmpgfx.ReleaseHdc()

        Dim tmpclr As Color = tmpimg.GetPixel(0, 0)
        If Not (tmpclr.R = 127 And tmpclr.G = 255 And tmpclr.B = 212) Then
            'tmparr.Add(tmpclr.R.ToString + " " + tmpclr.G.ToString + " " + tmpclr.B.ToString)
            drawSelector(xSelector, ySelector)
        End If
    End Sub

    Public Sub setImage(image As Bitmap)
        image.SetResolution(96, 96)
        ibackgroundFromEmu = image
        gBufer.DrawImage(ibackgroundFromEmu, 0, 0, New Rectangle(menuX + 30, menuX + 30, ibackgroundFromEmu.Width, 500), GraphicsUnit.Pixel)
        gBufer.DrawImage(ibackground, 0, 0)
        gBufer.DrawImage(iMainMenu, 20, 0, New Rectangle(0, 0 + MenuOffset * 30, MenuWidth - 5, 390), GraphicsUnit.Pixel)

        'Draw circles to the left of active cheats
        For i = 0 + MenuOffset To menuCount - 1
            If i > 12 + MenuOffset Then Exit For
            If subMenu(i + 1) Is Nothing AndAlso CheatsStatus(realCheatNumberArr(i)) IsNot Nothing AndAlso CheatsStatus(realCheatNumberArr(i)).ToString = "1" Then
                gBufer.FillEllipse(Brushes.LightGreen, 3, (i - MenuOffset) * 30 + 5, 20, 20)
            End If
        Next
        drawSelector(xSelector, ySelector)
    End Sub

    Public Sub exitMenu()
        RaiseEvent OnExit()
    End Sub

    Protected Overridable Sub Dispose(ByVal disposing As Boolean)
        If Not Me.disposed Then
            OnExit_raised()
            Try
                gScreen.DrawImage(ibackgroundFromEmu, 0, 0)
            Catch ex As Exception

            End Try
            ibufer.Dispose()
            ibackground.Dispose()
            ibackgroundFromEmu.Dispose()
            iMainMenu.Dispose()
            gScreen.Dispose()
            gBufer.Dispose()
            gBackground.Dispose()
            gBackgroundFromEmu.Dispose()
            gMainMenu.Dispose()
            timer.Dispose()
            smenu = Nothing
            KeyUpLocal = Nothing
            KeyDownlocal = Nothing
            KeyLeftlocal = Nothing
            KeyRightlocal = Nothing
            KeyEnterlocal = Nothing
            KeyBackspacelocal = Nothing
            RemoveHandler MultiThreading.menuNavigEvent, AddressOf processJoy
        End If
        Me.disposed = True
    End Sub
    Public Sub Dispose() Implements IDisposable.Dispose
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub
    Protected Overrides Sub Finalize()
        Dispose(False)
        MyBase.Finalize()
    End Sub
End Class
