Public Class Form1_DBsqlCheats2
    Private classDB As Class7_db
    Private curCodes As New List(Of String)
    Private resultSet1 As System.Data.SqlServerCe.SqlCeResultSet
    Private resultSet2 As System.Data.SqlServerCe.SqlCeResultSet

    Private Sub Form1_DBsqlCheats2_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        classDB.connClose()
        ListBox1.DataSource = Nothing
    End Sub

    Private Sub Form1_DBsqlCheats2_Shown(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Shown
        ListBox1.Items.Clear()
        classDB = New Class7_db
        resultSet1 = classDB.FetchData_Table("select game_id, game_name from " + sqlTableName + " order by game_name")
        ListBox1.DisplayMember = "game_name"
        ListBox1.ValueMember = "game_id"
        ListBox1.DataSource = resultSet1
    End Sub

    Private Sub ListBox1_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ListBox1.SelectedIndexChanged
        curCodes.Clear()
        ListBox2.Items.Clear()
        If ListBox1.SelectedIndex < 0 Then Exit Sub

        resultSet2 = classDB.FetchData_Table("select content from " + sqlTableName + " where game_id = " + ListBox1.SelectedValue.ToString)
        resultSet2.ReadFirst()
        Dim cheats() As String = resultSet2(0).ToString.Split({Chr(34)}, System.StringSplitOptions.RemoveEmptyEntries)
        For Each c As String In cheats
            Dim i As Integer = c.IndexOf(vbCrLf)
            If i >= 0 Then
                ListBox2.Items.Add(c.Substring(0, i))
                Dim curCode As String = c.Substring(i + 2)
                curCodes.Add(curCode)
            Else
                ListBox2.Items.Add(c)
                curCodes.Add("")
            End If
        Next
        If ListBox2.Items.Count > 0 Then ListBox2.SelectedIndex = 0
    End Sub

    Private Sub ListBox2_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ListBox2.SelectedIndexChanged
        TextBox1.Text = ""
        If ListBox2.SelectedIndex < 0 Then Exit Sub
        TextBox1.Text = curCodes(ListBox2.SelectedIndex)
    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        Dim s As String = InputBox("Enter game name")
        Dim record = resultSet1.CreateRecord()
        record.SetString(1, s)
        resultSet1.Insert(record)
        resultSet1.Update()
        'Dim t = resultSet1.ResultSetView

        ListBox1.DataSource = Nothing
        ListBox1.Items.Clear()
        resultSet1 = classDB.FetchData_Table("select game_id, game_name from " + sqlTableName + " order by game_name")
        ListBox1.DisplayMember = "game_name"
        ListBox1.ValueMember = "game_id"
        ListBox1.DataSource = resultSet1
        'ListBox1.SelectedItem = T
    End Sub 'Add game

    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click
        If ListBox1.SelectedIndex < 0 Then Exit Sub
        resultSet1.ReadFirst()
        Do While resultSet1(0).ToString <> ListBox1.SelectedValue.ToString
            resultSet1.Read()
        Loop
        resultSet1.Delete()
        ListBox1_SelectedIndexChanged(ListBox1, New System.EventArgs)
    End Sub 'Remove game

    Private Sub Button4_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button4.Click
        Dim s As String = InputBox("Enter cheat name")
        resultSet2 = classDB.FetchData_Table("select content from " + sqlTableName + " where game_id = " + ListBox1.SelectedValue.ToString)
        resultSet2.ReadFirst()
        Dim content As String = resultSet2(0).ToString
        If Not content.EndsWith(vbCrLf) And content <> "" Then content = content + vbCrLf
        content = content + """" + s
        resultSet2.SetString(0, content)
        resultSet2.Update()
        ListBox1_SelectedIndexChanged(ListBox1, New System.EventArgs)
        ListBox2.SelectedIndex = ListBox2.Items.Count - 1
    End Sub 'Add cheat

    Private Sub Button3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button3.Click
        If ListBox2.SelectedIndex < 0 Then Exit Sub
        curCodes.RemoveAt(ListBox2.SelectedIndex)
        ListBox2.Items.RemoveAt(ListBox2.SelectedIndex)
        Button7_Click(Button7, New EventArgs)
    End Sub 'Remove cheat


    Private Sub Button7_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button7.Click
        If ListBox2.SelectedIndex >= 0 Then curCodes(ListBox2.SelectedIndex) = TextBox1.Text

        Dim i As Integer
        Dim content As String = ""
        For i = 0 To ListBox2.Items.Count - 1
            content = content + """" + ListBox2.Items(i).ToString + vbCrLf
            content = content + curCodes(i)
        Next

        resultSet2 = classDB.FetchData_Table("select content from " + sqlTableName + " where game_id = " + ListBox1.SelectedValue.ToString)
        resultSet2.ReadFirst()
        resultSet2.SetString(0, content)
        resultSet2.Update()

        i = ListBox2.SelectedIndex
        ListBox1_SelectedIndexChanged(ListBox1, New System.EventArgs)
        ListBox2.SelectedIndex = i
    End Sub 'Update Codes
End Class