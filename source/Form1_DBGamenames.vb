Imports DevExpress.XtraGrid.Views.Grid

Public Class Form1_DBGamenames
    Private romList As New ArrayList
    Private filterfailed As Boolean = False
    Private listDataSource1 As New System.ComponentModel.BindingList(Of Record)
    Private listDataSource2 As New System.ComponentModel.BindingList(Of Record)

    Private Sub Form1_DBGamenames_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        'Remplir_list()
        ComboBox1.SelectedIndex = 0
        ComboBox2.SelectedIndex = 0
        Label4.Text = "Games: " + cl.StatGames.ToString
        Label5.Text = "Cheats: " + cl.StatCheats.ToString
        Label6.Text = "Codes: " + cl.StatCodes.ToString
        GridControl1.DataSource = listDataSource1
        GridControl2.DataSource = listDataSource2
        GridView1.Columns(0).Visible = False : GridView1.Columns(2).Visible = False : GridView1.Columns(3).Visible = False
        GridView2.Columns(0).Visible = False : GridView2.Columns(2).Visible = False : GridView2.Columns(3).Visible = False
    End Sub

    Private Sub Remplir_list()
        listDataSource1.Clear()

        Dim i = 0
        Dim name As String
        For Each s As String In cl.getAllGamesNames
            name = s.Replace("[+]", "")
            If CheckBox1.Checked Then s = s.Replace("[+]", "")
            If cl.getAllGamesNames2(i).ToString <> "" Then s = s + " - " + cl.getAllGamesNames2(i).ToString

            Dim item As New Record(1, s, i.ToString, "black")
            If CheckBox2.Checked And Not CheckBox3.Checked Then
                If Not cl.getAllGamesNames.Contains(name + "[+]") Then
                    If Remplir_list_check_match(s) Then listDataSource1.Add(item)
                End If
            ElseIf Not CheckBox2.Checked And CheckBox3.Checked Then
                If cl.getAllGamesNames.Contains(name + "[+]") Then
                    If Remplir_list_check_match(s) Then listDataSource1.Add(item) : item.Color = "brown"
                End If
            ElseIf CheckBox2.Checked And CheckBox3.Checked Then
                If Remplir_list_check_match(s) Then
                    listDataSource1.Add(item)
                    If cl.getAllGamesNames.Contains(name + "[+]") Then item.Color = "brown"
                End If
            End If
            i = i + 1
        Next
        Label1.Text = "Total: " + listDataSource1.Count.ToString
    End Sub

    Private Function Remplir_list_check_match(ByVal s As String) As Boolean
        If ComboBox1.SelectedIndex = 0 Then Return True
        If romList.Count = 0 Then Return True

        s = Remove_brackets(s)
        Dim i As Integer
        For i = 0 To romList.Count - 1
            If s = Remove_brackets(romList(i).ToString) Then
                If ComboBox1.SelectedIndex = 1 Then
                    Return True
                Else
                    Return False
                End If
            End If
        Next
        If ComboBox1.SelectedIndex = 1 Then
            Return False
        Else
            Return True
        End If
    End Function

    Private Sub Associate(ByVal newgameName As String)
        Dim t As String
        Dim i, N As Integer
        Dim oldGameName As String = "", newgamename2 As String = ""
        Dim oldCheatsNotesArr, oldCheatsValuesArr As New Hashtable
        Dim oldCheatsVarsArr, oldCheatsNamesArr, oldCheatsCodesArr As New ArrayList

        If newgameName.StartsWith("[") And newgameName.Contains("]") Then
            If Not newgameName.Length = newgameName.IndexOf("]") + 1 Then newgamename2 = newgameName.Substring(newgameName.IndexOf("]") + 1)
            newgameName = newgameName.Substring(1, newgameName.IndexOf("]") - 1)
        End If

        'Getting old values for
        'oldGameName, oldCheatsNamesArr, oldCheatsCodesArr, oldCheatsNotesArr
        Dim selectedtag As Integer = CInt(GridView1.GetRowCellDisplayText(GridView1.GetSelectedRows(0), "tag"))
        oldGameName = cl.getAllGamesNames(selectedtag).ToString
        oldCheatsNamesArr = TryCast(cl.getAllCheatsCheatNames(oldGameName), ArrayList)
        oldCheatsVarsArr = TryCast(cl.getAllCheatsCheatVars(oldGameName), ArrayList)
        For Each CheatName As String In oldCheatsNamesArr
            t = oldGameName & "+" & CheatName
            oldCheatsCodesArr.Add(cl.getCheatsCheatCodes(t))
            If Not cl.getCheatsCheatNotes(t) Is Nothing Then oldCheatsNotesArr.Add(t, cl.getCheatsCheatNotes(t))
            If Not cl.getCheatsCheatValues(t) Is Nothing Then oldCheatsValuesArr.Add(t, cl.getCheatsCheatValues(t))
        Next

        If oldGameName.ToUpper = newgameName.ToUpper Then
            MsgBox("This cheat is allredy assosiated with this game", MsgBoxStyle.Exclamation)
            Exit Sub
        End If

        Dim res = From r In cl.getAllGamesNames Where r.ToString.ToUpper = newgameName.ToUpper

        'Now if this game have NO cheats (no entry in database withing selected filename in right pane),
        'just create cheat and add old values to it, and then delete old one
        'If this game HAVE cheats, add values to this one
        If res.Count = 0 Then
            cl.getAllGamesNames(selectedtag) = newgameName
            If newgamename2 <> "" Then cl.getAllGamesNames2(selectedtag) = newgamename2

            cl.getAllCheatsCheatNames.Remove(oldGameName)
            cl.getAllCheatsCheatNames.Add(newgameName, oldCheatsNamesArr)
            cl.getAllCheatsCheatVars.Remove(oldGameName)
            cl.getAllCheatsCheatVars.Add(newgameName, oldCheatsVarsArr)

            i = 0
            For Each CheatName As String In TryCast(cl.getAllCheatsCheatNames(newgameName), ArrayList)
                t = oldGameName & "+" & CheatName
                cl.getCheatsCheatCodes.Remove(t)
                cl.getCheatsCheatCodes.Add(newgameName & "+" & CheatName, oldCheatsCodesArr(i))
                i += 1

                If oldCheatsNotesArr(t) IsNot Nothing Then
                    cl.getCheatsCheatNotes.Remove(t)
                    cl.getCheatsCheatNotes.Add(newgameName & "+" & CheatName, oldCheatsNotesArr(t))
                End If
                If oldCheatsValuesArr(t) IsNot Nothing Then
                    cl.getCheatsCheatValues.Remove(t)
                    cl.getCheatsCheatValues.Add(newgameName & "+" & CheatName, oldCheatsValuesArr(t))
                End If
            Next

            Dim s As String = cl.getAllGamesNames(selectedtag).ToString
            If cl.getAllGamesNames2(selectedtag).ToString <> "" Then s = s + " - " + cl.getAllGamesNames2(selectedtag).ToString
            GridView1.SetRowCellValue(GridView1.GetSelectedRows(0), "item", s)
            GridView1.SetRowCellValue(GridView1.GetSelectedRows(0), "Color", "black")
        Else
            If CheckBox4.Checked Then MsgBox("The game '" + newgameName + "' is alredy exist in CheatsDB. If you want to merge theese games, uncheck 'Deny Merge'") : Exit Sub
            If MsgBox("The game '" + newgameName + "' is alredy exist in CheatsDB. Merge them?", MsgBoxStyle.YesNo) <> MsgBoxResult.Yes Then Exit Sub
            cl.getAllGamesNames.RemoveAt(CInt(selectedtag))
            cl.getAllGamesNames2.RemoveAt(CInt(selectedtag))

            'if additional name exist either, in game beyond added, and in game to which it is added
            If newgamename2 <> "" Then
                Dim index As Integer = cl.getAllGamesNames.IndexOf(newgameName)
                If DirectCast(cl.getAllGamesNames2(index), String) <> newgamename2 Then
                    Dim m As MsgBoxResult = MsgBox("The old name2 was: '" + DirectCast(cl.getAllGamesNames2(index), String) + "', the new one: '" + newgamename2 + "'. Replace?", MsgBoxStyle.YesNo)
                    If m = MsgBoxResult.Yes Then cl.getAllGamesNames2(index) = newgamename2
                End If
            End If


            cl.getAllCheatsCheatNames.Remove(oldGameName)
            For Each s As String In oldCheatsNamesArr
                DirectCast(cl.getAllCheatsCheatNames(newgameName), ArrayList).Add(s) : Next
            cl.getAllCheatsCheatVars.Remove(oldGameName)
            If oldCheatsVarsArr IsNot Nothing Then
                For Each s As String In oldCheatsVarsArr
                    DirectCast(cl.getAllCheatsCheatVars(newgameName), ArrayList).Add(s) : Next
            End If


            i = 0
            For Each CheatName As String In oldCheatsNamesArr
                N = 1
                While cl.getCheatsCheatCodes(newgameName & "+" & CheatName) Is Nothing = False
                    CheatName = CheatName + " (alt" + N.ToString + ")" : N = N + 1
                End While
                cl.getCheatsCheatCodes.Add(newgameName & "+" & CheatName, oldCheatsCodesArr(i))
                i += 1

                t = oldGameName & "+" & CheatName
                If oldCheatsNotesArr(t) IsNot Nothing Then
                    cl.getCheatsCheatNotes.Remove(t)
                    cl.getCheatsCheatNotes.Add(newgameName & "+" & CheatName, oldCheatsNotesArr(t))
                End If
                If oldCheatsValuesArr(t) IsNot Nothing Then
                    cl.getCheatsCheatValues.Remove(t)
                    cl.getCheatsCheatValues.Add(newgameName & "+" & CheatName, oldCheatsValuesArr(t))
                End If
            Next
            GridView1.DeleteRow(GridView1.GetSelectedRows(0))
            i = cl.getAllGamesNames.IndexOf(newgameName)
            For r As Integer = 0 To listDataSource1.Count - 1
                If CInt(listDataSource1(r).tag) > selectedtag Then listDataSource1(r).tag = (CInt(listDataSource1(r).tag) - 1).ToString
                If CInt(listDataSource1(r).tag) = i Then GridView1.FocusedRowHandle = GridView1.GetRowHandle(r)
            Next
        End If
    End Sub

    Private Sub RemplirRomList()
        listDataSource2.Clear()
        For Each s As String In romList
            If CheckBox5.Checked Then
                If s.IndexOf(".") >= 0 Then s = s.Substring(0, s.LastIndexOf("."))
            End If
            If RemplirRomList_CheckMatch(s) Then listDataSource2.Add(New Record(1, s, "", "black"))
        Next
    End Sub

    Private Function RemplirRomList_CheckMatch(ByVal s As String) As Boolean
        If ComboBox2.SelectedIndex = 0 Then Return True
        If cl.getAllGamesNames.Count = 0 Then Return True
        s = Remove_brackets(s)

        For Each t As String In cl.getAllGamesNames
            If s = Remove_brackets(t) Then
                If ComboBox2.SelectedIndex = 1 Then
                    Return True
                Else
                    Return False
                End If
            End If
        Next

        If ComboBox2.SelectedIndex = 1 Then
            Return False
        Else
            Return True
        End If
        Return True
    End Function

    Private Function Remove_brackets(ByVal s As String, Optional ByVal removeExtention As Boolean = True) As String
        If s.IndexOf("[") >= 0 Or s.IndexOf("(") >= 0 Then
            If s.IndexOf("[") >= 0 Then s = s.Substring(0, s.IndexOf("["))
            If s.IndexOf("(") >= 0 Then s = s.Substring(0, s.IndexOf("("))
        ElseIf s.IndexOf(".") >= 0 And removeExtention Then
            s = s.Substring(0, s.LastIndexOf("."))
        End If
        Return s.ToUpper.Trim
    End Function

    Private Sub TextBox1_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TextBox1.TextChanged
        If TextBox1.Text = "" Then
            GridView1.ActiveFilterEnabled = False
        Else
            GridView1.ActiveFilterString = "([item] LIKE '" + TextBox1.Text + "')"
        End If
        Label1.Text = "Total: " + GridView1.RowCount.ToString
    End Sub

    Private Sub TextBox4_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TextBox4.TextChanged
        If TextBox1.Text = "" Then
            GridView2.ActiveFilterEnabled = False
        Else
            GridView2.ActiveFilterString = "([item] LIKE '" + TextBox4.Text + "')"
        End If
    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        Dim fb As New FolderBrowserDialog
        fb.ShowDialog()
        If fb.SelectedPath = "" Then Exit Sub
        If Not My.Computer.FileSystem.DirectoryExists(fb.SelectedPath) Then Exit Sub
        romList = New ArrayList

        Dim di As New System.IO.DirectoryInfo(fb.SelectedPath)
        For Each fi As System.IO.FileInfo In di.GetFiles
            romList.Add(fi.Name)
        Next
        RemplirRomList()
    End Sub

    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click
        Dim s As String
        Dim fd As New OpenFileDialog
        fd.ShowDialog()
        If Dir(fd.FileName) = "" Then Exit Sub
        romList = New ArrayList
        FileOpen(1, fd.FileName, OpenMode.Input)
        Do While Not EOF(1)
            s = LineInput(1)
            romList.Add(s)
        Loop
        FileClose(1)
        RemplirRomList()
    End Sub

    Private Sub Button3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button3.Click
        If GridView1.SelectedRowsCount <= 0 Or GridView2.SelectedRowsCount <= 0 Then Exit Sub
        Dim newGameName As String
        If TextBox2.Text = "0" Then
            newGameName = GridView2.GetRowCellDisplayText(GridView2.GetSelectedRows(0), "item")
        Else
            If Not IsNumeric(TextBox2.Text) Then MsgBox("You have to enter a Number in 'only N symbols' box.") : Exit Sub
            newGameName = GridView2.GetRowCellDisplayText(GridView2.GetSelectedRows(0), "item").Substring(0, CInt(TextBox2.Text))
        End If
        Associate(newGameName)
    End Sub

    Private Sub Button4_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button4.Click
        cl.saveCheats("OVERWRITE", True)
    End Sub

    Private Sub Button5_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button5.Click
        If TextBox3.Text = "" Then MsgBox("You have to enter the new game name.") : Exit Sub
        Associate(TextBox3.Text)
    End Sub

    Private Sub CheckBox1_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CheckBox1.CheckedChanged
        Remplir_list()
    End Sub

    Private Sub CheckBox2_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CheckBox2.CheckedChanged
        Remplir_list()
    End Sub

    Private Sub CheckBox3_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CheckBox3.CheckedChanged
        Remplir_list()
    End Sub

    Private Sub CheckBox5_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CheckBox5.CheckedChanged
        RemplirRomList()
    End Sub

    Private Sub ComboBox1_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ComboBox1.SelectedIndexChanged
        Remplir_list()
    End Sub

    Private Sub ComboBox2_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ComboBox2.SelectedIndexChanged
        RemplirRomList()
    End Sub

    Private Sub gridView1_RowCellStyle(ByVal sender As Object, ByVal e As RowCellStyleEventArgs) Handles GridView1.RowCellStyle
        Dim View As GridView = DirectCast(sender, GridView)
        e.Appearance.ForeColor = Color.FromName(View.GetRowCellDisplayText(e.RowHandle, View.Columns("Color")))
    End Sub

    'Private Function FilterMethod(ByVal itemToFilter As Telerik.WinControls.UI.RadListDataItem) As Boolean
    'Return itemToFilter.Text.ToLower.Contains(TextBox1.Text.ToLower)
    'End Function

    'Private Function FilterMethod2(ByVal itemToFilter As Telerik.WinControls.UI.RadListDataItem) As Boolean
    'If filterfailed Then Return False
    'Try
    'Return itemToFilter.Text Like TextBox4.Text
    'Catch ex As Exception
    'filterfailed = True
    'Return False
    'End Try
    'End Function
End Class

Public Class Record
    Dim _c As String
    Dim _id As Integer
    Dim _item, _tag As String
    Public Sub New(ByVal id As Integer, ByVal item As String, ByVal tag As String, ByVal c As String)
        Me._id = id
        Me._item = item
        Me._tag = tag
        Me._c = c
    End Sub

    Public ReadOnly Property ID() As Integer
        Get
            Return _id
        End Get
    End Property

    Public Property item() As String
        Get
            Return _item
        End Get
        Set(ByVal Value As String)
            _item = Value
        End Set
    End Property

    Public Property tag() As String
        Get
            Return _tag
        End Get
        Set(ByVal Value As String)
            _tag = Value
        End Set
    End Property

    Public Property Color() As String
        Get
            Return _c
        End Get
        Set(ByVal Value As String)
            _c = Value
        End Set
    End Property
End Class