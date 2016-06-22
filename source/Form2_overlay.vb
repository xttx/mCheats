Public Class Form2_overlay
    Dim once As Boolean = False

    Private Sub Form2_overlay_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        If MainMenu IsNot Nothing Then MenuExit()
    End Sub

    Private Sub MenuExit()
        MainMenu.Dispose()
        MainMenu = Nothing
        Module1.MenuActive = False
        Me.Close()
    End Sub

    Private Sub Form2_overlay_Paint(ByVal sender As Object, ByVal e As System.Windows.Forms.PaintEventArgs) Handles Me.Paint
        If r.Bottom > 0 Then
            Me.Left = r.Left
            Me.Top = r.Top
            Me.Width = r.Right - r.Left
            Me.Height = r.Bottom - r.Top
        End If
        If Not once Then
            once = True
            MainMenu.reassignHwnd(Me.Handle)
            Me.BackgroundImage = MainMenu.getEntireImage
            AddHandler MainMenu.OnExit, AddressOf MenuExit
        End If
        MainMenu.drawSelector(30, 30)
    End Sub
End Class