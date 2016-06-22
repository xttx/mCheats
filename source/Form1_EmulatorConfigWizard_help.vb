Public Class Form1_EmulatorConfigWizard_help
    Public _step As Integer = 0
    Private Sub Form1_EmulatorConfigWizard_help_Load(sender As System.Object, e As System.EventArgs) Handles MyBase.Load
        Me.Text = "mCheat - Emulator Config Wizard - Help - step " + _step.ToString
        Dim name As String = "Notes" + _step.ToString + "_*.*"
        If Microsoft.VisualBasic.FileIO.FileSystem.GetFiles("./EmulatorConfigPresets", FileIO.SearchOption.SearchTopLevelOnly, {name}).Count > 0 Then
            name = Microsoft.VisualBasic.FileIO.FileSystem.GetFiles("./EmulatorConfigPresets", FileIO.SearchOption.SearchTopLevelOnly, {name})(0)
        Else
            RichTextBox1.Text = "" : Exit Sub
        End If
        RichTextBox1.LoadFile(name, RichTextBoxStreamType.UnicodePlainText)
    End Sub
End Class