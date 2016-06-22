Public Class Class8_network_db_handler
    Public Sub New()

    End Sub

    Dim s As String = ""
    Public Sub test()
        Do While True
            If Microsoft.VisualBasic.FileIO.FileSystem.FileExists(".\commit.db") Then
                Try
                    FileOpen(1, ".\commit.db", OpenMode.Input)
                    Do While Not EOF(1)
                        s = LineInput(1)
                        If s.Trim = "" Then Continue Do
                        db.executeQuery(s)
                    Loop
                    FileClose(1)
                    Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(".\commit.db")
                Catch ex As Exception
                    MsgBox("Error commiting ""commit.db""")
                End Try
            End If

            If Microsoft.VisualBasic.FileIO.FileSystem.FileExists(".\cheats\cheats.new.sdf") Then
                Try
                    db.connClose()
                    Dim tmp As Integer = 0
                    Dim d As String = DateTime.Now.Year.ToString + "."
                    tmp = DateTime.Now.Month : If tmp < 10 Then d = d + "0"
                    d = d + tmp.ToString + "."
                    tmp = DateTime.Now.Day : If tmp < 10 Then d = d + "0"
                    d = d + tmp.ToString + "_"
                    tmp = DateTime.Now.Hour : If tmp < 10 Then d = d + "0"
                    d = d + tmp.ToString + "."
                    tmp = DateTime.Now.Minute : If tmp < 10 Then d = d + "0"
                    d = d + tmp.ToString + "."
                    tmp = DateTime.Now.Second : If tmp < 10 Then d = d + "0"
                    d = d + tmp.ToString

                    Microsoft.VisualBasic.FileIO.FileSystem.MoveFile(".\cheats\cheats.sdf", ".\cheats\cheats.old." + d.ToString + ".sdf")
                    Microsoft.VisualBasic.FileIO.FileSystem.MoveFile(".\cheats\cheats.new.sdf", ".\cheats\cheats.sdf")
                    db = New Class7_db
                Catch ex As Exception
                    MsgBox("Error replacing db." + vbCrLf + ex.Message)
                End Try
            End If
            Application.DoEvents()
            Threading.Thread.Sleep(1000)
        Loop
    End Sub
End Class
