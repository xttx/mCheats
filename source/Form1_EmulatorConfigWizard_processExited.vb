Public Class Form1_EmulatorConfigWizard_processExited
    Private pName As String
    Private trd As Threading.Thread = New Threading.Thread(AddressOf ThreadTask)

    Private Sub PictureBox1_Click(sender As System.Object, e As System.EventArgs) Handles PictureBox1.Click
        If MsgBox("Application is waiting for process ''. Do you want to abort?", MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then
            Form1_EmulatorConfigWizard.Tag = "-1"
            trd.Abort()
            Me.Close()
        End If
    End Sub

    Private Sub Form1_EmulatorConfigWizard_processExited_Shown(sender As Object, e As System.EventArgs) Handles Me.Shown
        PictureBox1.ImageLocation = ".\loader3.gif"
        pName = Form1_EmulatorConfigWizard.Tag.ToString
        trd.IsBackground = True : trd.Start()
    End Sub

    Private Sub ThreadTask()
        Dim ps() As Process
        Do While True
            'Me.Refresh()
            ps = Process.GetProcesses()
            'Me.Refresh()
            'Application.DoEvents()
            'Me.Refresh()
            For Each curp As Process In ps
                If curp.ProcessName = pName Then
                    pName = curp.ProcessName
                    Exit Do
                End If
            Next
            'Me.Refresh()
            'Application.DoEvents()
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
        trd.Abort()
        Form1_EmulatorConfigWizard.Tag = pName
        Me.Close()
    End Sub

    Private Sub Form1_EmulatorConfigWizard_processExited_Load(sender As System.Object, e As System.EventArgs) Handles MyBase.Load

    End Sub
End Class