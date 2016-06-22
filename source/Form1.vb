Imports System.Runtime.InteropServices
Public Class Form1

    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        '.txt
        Dim di As New IO.DirectoryInfo(".\cheats\")
        Dim diar1 As IO.FileInfo() = di.GetFiles()
        Dim dra As IO.FileInfo
        For Each dra In diar1
            ComboBox1.Items.Add(dra.Name)
        Next
        If ComboBox1.Items.Count > 0 Then ComboBox1.SelectedIndex = 0

        'SQL
        ComboBox3.Items.Clear()
        Dim db As New Class7_db
        For Each s As String In db.Enumerate_tables()
            ComboBox3.Items.Add(s)
        Next
        If ComboBox3.Items.Count > 0 Then ComboBox3.SelectedIndex = 0
    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        If ComboBox1.SelectedIndex < 0 Then MsgBox("Please, select cheat file.") : Exit Sub
        cl = New Class4_CheatsLoad(".\cheats\" + ComboBox1.SelectedItem.ToString, "LOADALL")
        Form1_DBGamenames.ShowDialog()
    End Sub

    Private Sub Button2_Click(sender As System.Object, e As System.EventArgs) Handles Button2.Click
        If ComboBox1.SelectedIndex < 0 Then MsgBox("Please, select cheat file.") : Exit Sub
        cl = New Class4_CheatsLoad(".\cheats\" + ComboBox1.SelectedItem.ToString, "LOADALL")
        Form1_DBCheats.ShowDialog()
    End Sub

    Private Sub Button3_Click(sender As System.Object, e As System.EventArgs)
        Dim db As New Class7_db
        db.createDatabase()
    End Sub

    Private Sub Button4_Click(sender As System.Object, e As System.EventArgs) Handles Button4.Click
        If ComboBox2.SelectedIndex < 0 Then MsgBox("Press select system.") : Exit Sub

        Dim db As New Class7_db
        If CheckBox1.Checked Then db.Create_system_tables(ComboBox2.SelectedItem.ToString)
        db.ConvertTxtToSql(ComboBox1.SelectedItem.ToString, ComboBox2.SelectedItem.ToString)
    End Sub

    Private Sub Button5_Click(sender As System.Object, e As System.EventArgs) Handles Button5.Click
        If ComboBox3.SelectedIndex < 0 Then MsgBox("Please, select system table.") : Exit Sub
        sqlTableName = ComboBox3.SelectedItem.ToString
        Form1_DBsqlCheats.ShowDialog()
    End Sub

    Private Sub Button6_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button6.Click
        If ComboBox3.SelectedIndex < 0 Then MsgBox("Please, select system table.") : Exit Sub
        sqlTableName = ComboBox3.SelectedItem.ToString
        Form1_DBsqlCheats2.ShowDialog()
    End Sub

    Private Sub Button7_Click(sender As System.Object, e As System.EventArgs) Handles Button7.Click
        Form1_DBglobalCheatEditor.ShowDialog()
    End Sub
End Class
