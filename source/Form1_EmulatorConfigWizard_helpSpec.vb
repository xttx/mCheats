Public Class Form1_EmulatorConfigWizard_helpSpec
    Public SelectedSystem As String = ""

    Private Sub Form1_EmulatorConfigWizard_helpSpec_Load(sender As System.Object, e As System.EventArgs) Handles MyBase.Load
        Dim name As String = "Notes6*General*.*"
        If Microsoft.VisualBasic.FileIO.FileSystem.GetFiles("./EmulatorConfigPresets", FileIO.SearchOption.SearchTopLevelOnly, {name}).Count > 0 Then
            name = Microsoft.VisualBasic.FileIO.FileSystem.GetFiles("./EmulatorConfigPresets", FileIO.SearchOption.SearchTopLevelOnly, {name})(0)
            RichTextBox1.LoadFile(name, RichTextBoxStreamType.UnicodePlainText)
        Else
            RichTextBox1.Text = ""
        End If

        name = "Notes6*System*" + SelectedSystem + "*.*"
        If Microsoft.VisualBasic.FileIO.FileSystem.GetFiles("./EmulatorConfigPresets", FileIO.SearchOption.SearchTopLevelOnly, {name}).Count > 0 Then
            name = Microsoft.VisualBasic.FileIO.FileSystem.GetFiles("./EmulatorConfigPresets", FileIO.SearchOption.SearchTopLevelOnly, {name})(0)
            RichTextBox2.LoadFile(name, RichTextBoxStreamType.UnicodePlainText)
        Else
            RichTextBox2.Text = ""
        End If
    End Sub
End Class