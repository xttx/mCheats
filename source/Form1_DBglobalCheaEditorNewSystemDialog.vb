Public Class Form1_DBglobalCheaEditorNewSystemDialog

    Private Sub Form1_DBglobalCheaEditorNewSystemDialog_Load(sender As System.Object, e As System.EventArgs) Handles MyBase.Load
        TextBox1.Text = "" : TextBox2.Text = ""
    End Sub

    Private Sub Button1_Click(sender As System.Object, e As System.EventArgs) Handles Button1.Click
        If TextBox1.Text.Trim = "" Then MsgBox("You must enter system id.") : Exit Sub
        'If IsNumeric(TextBox1.Text.Substring(0, 1)) Then MsgBox("First symbol of system id can't be a number.") : Exit Sub
        Form1_DBglobalCheatEditor.newSystemID = TextBox1.Text.Trim.ToUpper
        Form1_DBglobalCheatEditor.newSystemName = TextBox2.Text.Trim
        Me.Close()
    End Sub

    Private Sub Button2_Click(sender As System.Object, e As System.EventArgs) Handles Button2.Click
        Form1_DBglobalCheatEditor.newSystemID = "CANCEL"
        Me.Close()
    End Sub
End Class