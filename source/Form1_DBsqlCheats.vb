Imports Microsoft.VisualBasic
Imports DevExpress.XtraGrid.Views.Grid

Public Class Form1_DBsqlCheats
    Private classDB As Class7_db
    Private resultSet As System.Data.SqlServerCe.SqlCeResultSet
    Private list(2) As List(Of String)
    Private filelist As New List(Of String)

    Private Sub Form1_DBsqlCheats_Load(sender As Object, e As System.EventArgs) Handles Me.Load
        ComboBox1.SelectedIndex = 2
    End Sub

    Private Sub Form1_DBsqlCheats_Shown(sender As Object, e As System.EventArgs) Handles Me.Shown
        init()
    End Sub

    Public Sub init()
        classDB = New Class7_db
        list(0) = New List(Of String)
        list(1) = New List(Of String)
        list(2) = New List(Of String)

        resultSet = classDB.FetchData_Table("select game_name, game_name2, game_name3 from " + sqlTableName)
        GridControl1.DataSource = resultSet
        GridView1.Columns(0).Visible = RadioButton1.Checked
        GridView1.Columns(1).Visible = RadioButton2.Checked
        GridView1.Columns(2).Visible = RadioButton3.Checked
        For i As Integer = 0 To GridView1.RowCount - 1
            list(0).Add(removeBrackets(GridView1.GetRowCellValue(i, GridView1.Columns(0)).ToString.ToUpper(), False))
            list(1).Add(removeBrackets(GridView1.GetRowCellValue(i, GridView1.Columns(1)).ToString.ToUpper(), False))
            list(2).Add(removeBrackets(GridView1.GetRowCellValue(i, GridView1.Columns(2)).ToString.ToUpper(), False))
        Next
        Label1.Text = "Total: " + GridView1.RowCount.ToString
    End Sub

    Private Sub RadioButton1_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles RadioButton1.CheckedChanged, RadioButton2.CheckedChanged, RadioButton3.CheckedChanged
        If GridView1.Columns.Count < 3 Then Exit Sub
        If Not DirectCast(sender, RadioButton).Checked Then Exit Sub
        GridView1.Columns(0).Visible = RadioButton1.Checked
        GridView1.Columns(1).Visible = RadioButton2.Checked
        GridView1.Columns(2).Visible = RadioButton3.Checked
    End Sub

    Private Sub TextBox1_TextChanged(sender As System.Object, e As System.EventArgs) Handles TextBox1.TextChanged
        filelist.Clear()
        ListBox1.Items.Clear()
        If FileIO.FileSystem.DirectoryExists(TextBox1.Text) Then
            For Each f As String In FileIO.FileSystem.GetFiles(TextBox1.Text)
                f = f.Substring(f.LastIndexOf("\") + 1)
                Dim fWoBrackets = removeBrackets(f)
                If CheckBox5.Checked Then f = fWoBrackets

                If CheckBox1.Checked And CheckBox2.Checked Then
                    ListBox1.Items.Add(f)
                ElseIf CheckBox1.Checked Then
                    If checkIfInDB(fWoBrackets) Then ListBox1.Items.Add(f)
                ElseIf CheckBox2.Checked Then
                    If Not checkIfInDB(fWoBrackets) Then ListBox1.Items.Add(f)
                End If
                filelist.Add(fWoBrackets)
            Next
        End If
        GridView1.RefreshData()
        Label1.Text = "Total: " + GridView1.RowCount.ToString
        Label2.Text = "Total: " + ListBox1.Items.Count.ToString
    End Sub

    Private Function checkIfInDB(s As String) As Boolean
        If list(0).Contains(s) Then Return True
        If list(1).Contains(s) Then Return True
        If list(2).Contains(s) Then Return True
        Return False
    End Function

    Private Function removeBrackets(ByVal s As String, Optional ByVal removeExtention As Boolean = True) As String
        If s.IndexOf("[") >= 0 Or s.IndexOf("(") >= 0 Then
            If s.IndexOf("[") >= 0 Then s = s.Substring(0, s.IndexOf("["))
            If s.IndexOf("(") >= 0 Then s = s.Substring(0, s.IndexOf("("))
        ElseIf s.IndexOf(".") >= 0 And removeExtention Then
            s = s.Substring(0, s.LastIndexOf("."))
        End If
        Return s.ToUpper.Trim
    End Function

    Private Sub CheckBox1_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles CheckBox1.CheckedChanged, CheckBox2.CheckedChanged, CheckBox5.CheckedChanged
        TextBox1_TextChanged(sender, New System.EventArgs)
    End Sub

    Private Sub CheckBox4_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles CheckBox3.CheckedChanged, CheckBox4.CheckedChanged
        GridView1.RefreshData()
        Label1.Text = "Total: " + GridView1.RowCount.ToString
    End Sub

    Private Sub GridView1_CustomRowFilter(ByVal sender As Object, ByVal e As DevExpress.XtraGrid.Views.Base.RowFilterEventArgs) Handles GridView1.CustomRowFilter
        Dim b As Boolean
        Dim view As GridView = CType(sender, GridView)
        If view.Columns.Count = 0 Then Exit Sub
        Dim val As String = view.GetRowCellValue(view.GetRowHandle(e.ListSourceRow), view.Columns(0)).ToString
        Dim val2 As String = view.GetRowCellValue(view.GetRowHandle(e.ListSourceRow), view.Columns(1)).ToString
        Dim val3 As String = view.GetRowCellValue(view.GetRowHandle(e.ListSourceRow), view.Columns(2)).ToString

        If CheckBox3.Checked And CheckBox4.Checked Then
            e.Visible = True
        Else
            b = filelist.Contains(removeBrackets(val)) And val <> ""
            b = b Or (filelist.Contains(removeBrackets(val2)) And val2 <> "")
            b = b Or (filelist.Contains(removeBrackets(val3)) And val3 <> "")
            If CheckBox3.Checked Then
                If b Then e.Visible = True Else e.Visible = False : If view.GetSelectedRows().Contains(view.GetRowHandle(e.ListSourceRow)) Then view.MoveNext()
            End If
            If CheckBox4.Checked Then
                If Not b Then e.Visible = True Else e.Visible = False
            End If
        End If
        e.Handled = True
    End Sub

    Private Sub Button1_Click(sender As System.Object, e As System.EventArgs) Handles Button1.Click
        If ListBox1.SelectedIndex < 0 Then MsgBox("select rom to associate") : Exit Sub
        Dim s As String = ListBox1.SelectedItem.ToString
        s = s.Substring(0, s.LastIndexOf("."))

        Dim col As Integer = ComboBox1.SelectedIndex
        Dim row As Integer = GridView1.FocusedRowHandle
        Dim nxt As Integer = GridView1.GetVisibleRowHandle(GridView1.GetNextVisibleRow(GridView1.GetVisibleIndex(GridView1.FocusedRowHandle)))
        Dim top As Integer = GridView1.TopRowIndex

        Dim old_gamename1 As String = GridView1.GetRowCellValue(row, GridView1.Columns(0)).ToString
        Dim old_gamename2 As String = GridView1.GetRowCellValue(row, GridView1.Columns(1)).ToString
        Dim old_gamename3 As String = GridView1.GetRowCellValue(row, GridView1.Columns(2)).ToString
        GridView1.SetRowCellValue(row, GridView1.Columns(col), s)
        GridView1.UpdateCurrentRow()
        Dim new_gamename1 As String = GridView1.GetRowCellValue(row, GridView1.Columns(0)).ToString
        Dim new_gamename2 As String = GridView1.GetRowCellValue(row, GridView1.Columns(1)).ToString
        Dim new_gamename3 As String = GridView1.GetRowCellValue(row, GridView1.Columns(2)).ToString
        Dim q As String = "UPDATE " + sqlTableName + " SET game_name = '" + new_gamename1 + "', game_name2 = '" + new_gamename2 + "', game_name3 = '" + new_gamename3 + "' "
        q = q + "WHERE game_name = '" + old_gamename1 + "', game_name2 = '" + old_gamename2 + "', game_name3 = '" + old_gamename3 + "'"
        FileOpen(1, ".\db.log", OpenMode.Append)
        PrintLine(1, q)
        FileClose(1)

        list(col)(GridView1.GetDataSourceRowIndex(row)) = removeBrackets(s)
        Dim sel = ListBox1.SelectedIndex : ListBox1.Items.RemoveAt(ListBox1.SelectedIndex) : ListBox1.SelectedIndex = sel

        GridView1.TopRowIndex = top
        GridView1.FocusedRowHandle = row
    End Sub

    Private Sub Button2_Click(sender As System.Object, e As System.EventArgs) Handles Button2.Click
        resultSet.Update()
        GridControl1.DataSource = Nothing
        classDB.connClose()
        init()
    End Sub

    Private Sub Button3_Click(sender As System.Object, e As System.EventArgs) Handles Button3.Click
        Dim col As Integer = ComboBox1.SelectedIndex
        Dim row As Integer = GridView1.FocusedRowHandle
        Dim nxt As Integer = GridView1.GetVisibleRowHandle(GridView1.GetNextVisibleRow(GridView1.GetVisibleIndex(row)))
        Dim top As Integer = GridView1.TopRowIndex

        Dim old_gamename1 As String = GridView1.GetRowCellValue(row, GridView1.Columns(0)).ToString
        Dim old_gamename2 As String = GridView1.GetRowCellValue(row, GridView1.Columns(1)).ToString
        Dim old_gamename3 As String = GridView1.GetRowCellValue(row, GridView1.Columns(2)).ToString
        GridView1.SetRowCellValue(row, GridView1.Columns(col), TextBox2.Text)
        GridView1.UpdateCurrentRow()
        Dim new_gamename1 As String = GridView1.GetRowCellValue(row, GridView1.Columns(0)).ToString
        Dim new_gamename2 As String = GridView1.GetRowCellValue(row, GridView1.Columns(1)).ToString
        Dim new_gamename3 As String = GridView1.GetRowCellValue(row, GridView1.Columns(2)).ToString
        Dim q As String = "UPDATE " + sqlTableName + " SET game_name = '" + new_gamename1 + "', game_name2 = '" + new_gamename2 + "', game_name3 = '" + new_gamename3 + "' "
        q = q + "WHERE game_name = '" + old_gamename1 + "', game_name2 = '" + old_gamename2 + "', game_name3 = '" + old_gamename3 + "'"
        FileOpen(1, ".\db.log", OpenMode.Append)
        PrintLine(1, q)
        FileClose(1)

        list(col)(GridView1.GetDataSourceRowIndex(row)) = TextBox2.Text
        TextBox2.Text = ""

        GridView1.TopRowIndex = top
        GridView1.FocusedRowHandle = nxt
    End Sub
End Class