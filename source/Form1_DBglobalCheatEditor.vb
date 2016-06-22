Public Class Form1_DBglobalCheatEditor
    Public Shared newSystemID As String = ""
    Public Shared newSystemName As String = ""
    Private curMax As Integer = 0
    Private curtable As String = ""
    Private content As String = ""
    Private duplicated As New Dictionary(Of Integer, List(Of String))

    Private Sub Form1_DBglobalCheatEditor_Load(sender As System.Object, e As System.EventArgs) Handles MyBase.Load
        'Dim skin As DevExpress.Skins.Skin = DevExpress.Skins.GridSkins.GetSkin(TreeList1.LookAndFeel)
        'skin.Properties(DevExpress.Skins.GridSkins.OptShowTreeLine) = True
        PopulateSystemMenu()
    End Sub

    Private Sub PopulateSystemMenu()
        Menu_SystemSelect.DropDown.Items.Clear()

        'REALISATION - Grabs systems from config (maybe from sql would be better)
        ''''''''''''''''''''''''''''''''''''''''''''
        For Each node As Xml.XmlNode In xmlConfig.SelectNodes("/config/systems/sys")
            Dim id As String = node.Attributes("id").Value
            Dim system As New ToolStripMenuItem With {.Tag = id}
            If id.Substring(0, 1) = "_" Then id = id.Substring(1)
            system.Text = id + " - " + node.InnerText
            AddHandler system.Click, AddressOf ChangeSystemHandler
            Menu_SystemSelect.DropDown.Items.Add(system)
        Next
        ''''''''''''''''''''''''''''''''''''''''''''

        If Menu_SystemSelect.DropDown.Items.Count > 0 Then Menu_SystemSelect.DropDown.Items.Add(New ToolStripSeparator)

        Dim CreateNewMenuItem As New ToolStripMenuItem With {.Text = "Create new system ...", .Tag = "CREATENEW"}
        AddHandler CreateNewMenuItem.Click, AddressOf ChangeSystemHandler
        Menu_SystemSelect.DropDown.Items.Add(CreateNewMenuItem)

        Dim DeleteSystemMenuItem As New ToolStripMenuItem With {.Text = "Delete System ...", .Tag = "DELETESYSTEM"}
        AddHandler DeleteSystemMenuItem.Click, AddressOf ChangeSystemHandler
        Menu_SystemSelect.DropDown.Items.Add(DeleteSystemMenuItem)
    End Sub

    Private Sub refresh_gameNameTable(table As String)
        Dim needRefresh As Boolean = False
        If GridView1.RowCount > 0 Then
            If GridView1.GetSelectedRows(0) = GridView1.GetRowHandle(0) Then needRefresh = True
        End If

        Dim res As System.Data.SqlServerCe.SqlCeResultSet = db.fetchData_row("SELECT max(game_id) FROM " + table)
        res.Read()
        If res.IsDBNull(0) Then curMax = 0 Else curMax = res.GetInt32(0)

        GridControl1.DataSource = db.FetchData_Table("SELECT game_id, game_name, game_name2, game_name3, game_name4, game_name5 FROM " + table + " ORDER BY game_id")
        GridView1.Columns(0).OptionsColumn.AllowEdit = False
        GridView1.Columns(0).Caption = "ID" : GridView1.Columns(0).Resize(18)
        GridView1.Columns(4).Visible = False : GridView1.Columns(5).Visible = False
        GridView1.SelectRow(GridView1.GetRowHandle(0))
        If needRefresh Then GridView1_FocusedRowChanged(GridView1, New DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventArgs(0, 1))
    End Sub

#Region "Game list grid events"
    Private Sub GridView1_FocusedRowChanged(sender As Object, e As DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventArgs) Handles GridView1.FocusedRowChanged
        TreeList1.Nodes.Clear()
        TreeListColumn1.SortOrder = SortOrder.None
        If curtable = "" Then Exit Sub
        If GridView1.GetSelectedRows.Count = 0 Then Exit Sub
        If GridView1.IsNewItemRow(GridView1.GetSelectedRows(0)) Then Exit Sub
        If GridView1.GetRowCellValue(GridView1.GetSelectedRows(0), GridView1.Columns(0)) Is Nothing Then Exit Sub

        Dim id As String = GridView1.GetRowCellValue(GridView1.GetSelectedRows(0), GridView1.Columns(0)).ToString
        Dim res As System.Data.SqlServerCe.SqlCeResultSet = db.fetchData_row("SELECT content FROM " + curtable + " WHERE game_id = " + id)
        res.Read()
        If res.IsDBNull(0) Then Exit Sub
        Dim content As String = res.GetString(0)

        TreeList1.BeginUpdate() : TreeList1.BeginUnboundLoad()
        Dim counter As Integer = 0
        Dim last_cheat_name As String = ""
        Dim CheatNodes As New Dictionary(Of String, DevExpress.XtraTreeList.Nodes.TreeListNode)
        For Each s As String In content.Split({vbCrLf}, StringSplitOptions.RemoveEmptyEntries)
            If s.StartsWith("""") Then
                s = s.Substring(1)
                last_cheat_name = s
                If shouldBeHidden(id, s) Then Continue For

                counter += 1
                If s.Contains("\") Then
                    Dim curNode As String = ""
                    For Each subNode As String In s.Split({"\"}, StringSplitOptions.None)
                        curNode = curNode + "\" + subNode
                        If curNode.StartsWith("\") Then curNode = curNode.Substring(1)

                        If Not CheatNodes.Keys.Contains(curNode) Then
                            If curNode.Contains("\") Then
                                Dim curNodeText As String = curNode.Substring(curNode.LastIndexOf("\") + 1)
                                Dim curNodeParentName As String = curNode.Substring(0, curNode.LastIndexOf("\"))

                                CheatNodes.Add(curNode, CheatNodes(curNodeParentName).Nodes.Add({curNodeText, counter.ToString, curNode, ""}))
                            Else
                                CheatNodes.Add(curNode, TreeList1.Nodes.Add({curNode, counter.ToString, curNode, ""}))
                            End If
                        End If
                    Next
                Else
                    CheatNodes.Add(s, TreeList1.Nodes.Add({s, counter.ToString, s, ""}))
                End If
            Else
                If shouldBeHidden(id, last_cheat_name) Then Continue For
                CheatNodes(last_cheat_name).Item("cheats") = CheatNodes(last_cheat_name).Item("cheats").ToString + s + vbCrLf
            End If
        Next

        TreeList1.EndUpdate() : TreeList1.EndUnboundLoad()
        TreeList1_FocusedNodeChanged(TreeList1, New DevExpress.XtraTreeList.FocusedNodeChangedEventArgs(TreeList1.Selection(0), TreeList1.Selection(0)))
    End Sub

    Private Function shouldBeHidden(gameID As String, cheat As String) As Boolean
        If SearchDuplicatedCodesWithingASameGemeToolStripMenuItem.Checked = False Then Return False
        If duplicated(CInt(gameID)).Contains(cheat) Then Return False Else Return True
    End Function

    Private Sub GridView1_InitNewRow(sender As Object, e As DevExpress.XtraGrid.Views.Grid.InitNewRowEventArgs) Handles GridView1.InitNewRow
        curMax = curMax + 1
        GridView1.SetRowCellValue(e.RowHandle, "ID", curMax)
    End Sub
#End Region

#Region "Cheats edit panel"
    Private Sub Button1_Click(sender As System.Object, e As System.EventArgs) Handles Button1.Click
        TextBox1.Text = TextBox1.Text.Trim
        If TextBox1.Text = "" Then MsgBox("Enter cheat name.") : Exit Sub

        If TreeList1.Selection.Count = 0 Then Exit Sub
        If TreeList1.Selection(0).Item(0).ToString = TextBox1.Text Then Exit Sub
        'Dim newCheatName As String = addToCurCheats(TextBox1.Text, TreeList1.Selection(0).Tag.ToString)
        'If newCheatName.Contains("\") Then newCheatName = newCheatName.Substring(newCheatName.LastIndexOf("\") + 1)
        'TreeList1.Selection(0).Item(0) = newCheatName
        Dim newUniqueName = checkNodeUniqueness(TextBox1.Text, TreeList1.Selection(0))
        Dim nOfChars As Integer = TreeList1.Selection(0).Item("fullPath").ToString.Length
        Dim newFullPath As String = TreeList1.Selection(0).Item("fullPath").ToString
        If newFullPath.Contains("\") Then newFullPath = newFullPath.Substring(0, newFullPath.LastIndexOf("\") + 1) + newUniqueName Else newFullPath = newUniqueName
        TreeList1.Selection(0).Item(0) = newUniqueName
        TreeList1.Selection(0).Item("fullPath") = newFullPath
        If TreeList1.Selection(0).Nodes.Count > 0 Then Button1_Click_Rename_fullPath_recur(TreeList1.Selection(0).Nodes(0), nOfChars, newFullPath)
    End Sub 'Set

    Private Sub Button1_Click_Rename_fullPath_recur(node As DevExpress.XtraTreeList.Nodes.TreeListNode, nOfCharsToReplace As Integer, newFullPathFirstPart As String)
        Do
            node.Item("fullPath") = newFullPathFirstPart + node.Item("fullPath").ToString.Substring(nOfCharsToReplace)
            If node.Nodes.Count > 0 Then
                Button1_Click_Rename_fullPath_recur(node.Nodes(0), nOfCharsToReplace, newFullPathFirstPart)
            End If
            node = node.NextNode
        Loop While node IsNot Nothing
    End Sub

    Private Sub Button2_Click(sender As System.Object, e As System.EventArgs) Handles Button2.Click
        TextBox1.Text = TextBox1.Text.Trim
        If TextBox1.Text = "" Then MsgBox("Enter cheat name.") : Exit Sub

        If TreeList1.Selection.Count = 0 OrElse TreeList1.Selection(0).ParentNode Is Nothing Then
            Dim newUniqueName As String = TextBox1.Text
            If TreeList1.Nodes.Count > 0 Then newUniqueName = checkNodeUniqueness(TextBox1.Text, TreeList1.Nodes(0))
            TreeList1.Nodes.Add({newUniqueName, TreeList1.AllNodesCount + 1, newUniqueName, ""}).Selected = True
        Else
            Dim parentPath As String = TreeList1.Selection(0).ParentNode.Item("fullPath").ToString + "\"
            Dim newUniqueName = checkNodeUniqueness(TextBox1.Text, TreeList1.Selection(0))
            TreeList1.Selection(0).ParentNode.Nodes.Add({newUniqueName, TreeList1.AllNodesCount + 1, parentPath + newUniqueName, ""}).Selected = True
        End If
    End Sub 'Add new

    Private Sub Button3_Click(sender As System.Object, e As System.EventArgs) Handles Button3.Click
        TextBox1.Text = TextBox1.Text.Trim
        If TextBox1.Text = "" Then MsgBox("Enter cheat name.") : Exit Sub

        Dim newUniqueName As String = TextBox1.Text
        If TreeList1.Selection.Count = 0 Then
            If TreeList1.Nodes.Count > 0 Then newUniqueName = checkNodeUniqueness(TextBox1.Text, TreeList1.Nodes(0))
            TreeList1.Nodes.Add({newUniqueName, TreeList1.AllNodesCount + 1, newUniqueName, ""}).Selected = True
        Else
            Dim parentPath As String = TreeList1.Selection(0).Item("fullPath").ToString + "\"
            If TreeList1.Selection(0).Nodes.Count > 0 Then newUniqueName = checkNodeUniqueness(TextBox1.Text, TreeList1.Selection(0).Nodes(0))
            TreeList1.Selection(0).Nodes.Add({newUniqueName, TreeList1.AllNodesCount + 1, parentPath + newUniqueName, ""}).Selected = True
        End If
    End Sub 'Add child

    Private Sub Button4_Click(sender As System.Object, e As System.EventArgs) Handles Button4.Click
        If TreeList1.Selection.Count = 0 Then Exit Sub
        TreeList1.Nodes.Remove(TreeList1.Selection(0))
    End Sub 'Delete

    Private Function checkNodeUniqueness(name As String, node As DevExpress.XtraTreeList.Nodes.TreeListNode) As String
        Dim altN As Integer = 1
        Dim parentPath As String = ""
        If node.ParentNode IsNot Nothing Then parentPath = node.ParentNode.Item("fullPath").ToString + "\"

        If TreeList1.FindNodeByFieldValue("fullPath", parentPath + name) IsNot Nothing Then
            Do While TreeList1.FindNodeByFieldValue("fullPath", parentPath + name + " (alt" + altN.ToString + ")") IsNot Nothing
                altN += 1
            Loop
            name = name + " (alt" + altN.ToString + ")"
        End If
        Return name
    End Function

    Private Sub TreeList1_FocusedNodeChanged(sender As Object, e As DevExpress.XtraTreeList.FocusedNodeChangedEventArgs) Handles TreeList1.FocusedNodeChanged
        TextBox2.Text = ""
        If e.Node IsNot Nothing Then
            TextBox1.Text = e.Node.Item(0).ToString
            'If e.Node.Tag IsNot Nothing Then
            If e.Node.Item("cheats") IsNot Nothing Then
                'For Each s As String In curCheats(e.Node.Tag.ToString)
                'TextBox2.Text = TextBox2.Text + s + vbCrLf
                TextBox2.Text = e.Node.Item("cheats").ToString
                'Next
            End If
        Else
            TextBox1.Text = ""
        End If

    End Sub 'Refresh textbox
#End Region

#Region "cheats codes control"
    Private Sub Button5_Click(sender As System.Object, e As System.EventArgs) Handles Button5.Click
        content = ""
        If GridView1.GetSelectedRows.Count = 0 Then Exit Sub
        Label1.Visible = True : Me.Refresh()

        If TreeList1.Selection.Count > 0 Then
            TextBox2.Text = TextBox2.Text.Trim
            If Not TextBox2.Text.EndsWith(vbCrLf) And Not TextBox2.Text = "" Then TextBox2.Text = TextBox2.Text + vbCrLf
            TreeList1.Selection(0).Item("cheats") = TextBox2.Text
        End If
        If TreeList1.Nodes.Count > 0 Then updateCheats_recur()

        Dim GameId As String = GridView1.GetRowCellValue(GridView1.GetSelectedRows(0), GridView1.Columns(0)).ToString
        If Not IsNumeric(GameId) Then MsgBox("Error updating: Can't retrive game ID.") : Exit Sub
        Try
            db.executeQuery("UPDATE " + curtable + " SET content = '" + content.Replace("'", "''") + "' WHERE game_id = " + GameId)
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Critical, "Database query error.")
        End Try
        Label1.Visible = False
    End Sub 'Update Cheats

    Private Sub updateCheats_recur(Optional curNode As DevExpress.XtraTreeList.Nodes.TreeListNode = Nothing)
        Dim l As System.Linq.IOrderedEnumerable(Of DevExpress.XtraTreeList.Nodes.TreeListNode)
        If curNode Is Nothing Then
            l = TreeList1.Nodes.OrderBy(Function(o) CInt(o.Item("index").ToString))
        Else
            l = curNode.ParentNode.Nodes.OrderBy(Function(o) CInt(o.Item("index").ToString))
        End If

        For Each n As DevExpress.XtraTreeList.Nodes.TreeListNode In l
            If n.Nodes.Count = 0 Then
                content = content + """" + n.Item("fullPath").ToString + vbCrLf
                content = content + n.Item("cheats").ToString
            Else
                updateCheats_recur(n.Nodes(0))
            End If
        Next
    End Sub
#End Region

#Region "Menus"
    Private Sub ChangeSystemHandler(sender As Object, e As System.EventArgs)
        'MsgBox(DirectCast(sender, ToolStripMenuItem).Text)
        SearchDuplicatedCodesWithingASameGemeToolStripMenuItem.Checked = False
        If DirectCast(sender, ToolStripMenuItem).Tag.ToString = "CREATENEW" Then
            Form1_DBglobalCheaEditorNewSystemDialog.ShowDialog()
            If newSystemID = "CANCEL" Then newSystemID = "" : newSystemName = "" : Exit Sub
            Dim db_name As String = newSystemID
            If IsNumeric(db_name.Substring(0, 1)) Then db_name = "_" + db_name

            If db.checkIfTableExist(db_name) Then
                If MsgBox("Database for """ + newSystemID + """ already exists. Do you really want to reset it?", MsgBoxStyle.YesNo) = MsgBoxResult.No Then Exit Sub
                db.Create_system_tables(db_name)
                MsgBox("Database for """ + newSystemID + """ successfully reseted. It's empty, so you have to add some cheats to it.")
            Else
                db.Create_system_tables(db_name)
                MsgBox("Empty database for """ + newSystemID + """ successfully created. It's empty, so you have to add some cheats to it.")
            End If

            'Add to xml
            Dim x As Xml.XmlNode = xmlConfig.SelectSingleNode("/config/systems")
            Dim newSys As Xml.XmlElement = xmlConfig.CreateElement("sys")
            Dim newSysAttr As Xml.XmlAttribute = xmlConfig.CreateAttribute("id")
            newSys.InnerText = newSystemName
            'newSysAttr.Value = newSystemID
            newSys.SetAttribute("id", newSystemID)
            x.AppendChild(newSys)
            xmlConfig.Save(".\config.xml")

            PopulateSystemMenu() : Exit Sub
        End If

        If DirectCast(sender, ToolStripMenuItem).Tag.ToString = "DELETESYSTEM" Then
            Exit Sub
        End If

        curtable = DirectCast(sender, ToolStripMenuItem).Tag.ToString
        If IsNumeric(curtable.Substring(0, 1)) Then curtable = "_" + curtable
        For Each MenuItem As ToolStripItem In Menu_SystemSelect.DropDown.Items
            If TypeOf (MenuItem) Is ToolStripMenuItem Then DirectCast(MenuItem, ToolStripMenuItem).Checked = False
        Next
        DirectCast(sender, ToolStripMenuItem).Checked = True
        refresh_gameNameTable(curtable)
    End Sub

    Private Sub GameNamesAutoSizeColumnsToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) Handles GameNamesAutoSizeColumnsToolStripMenuItem.Click
        Dim enable As Boolean = DirectCast(sender, ToolStripMenuItem).Checked
        enable = Not enable

        DirectCast(sender, ToolStripMenuItem).Checked = enable
        GridView1.OptionsView.ColumnAutoWidth = enable
    End Sub

    Private Sub GameNamesAllowEditToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) Handles GameNamesAllowEditToolStripMenuItem.Click
        Dim enable As Boolean = DirectCast(sender, ToolStripMenuItem).Checked
        enable = Not enable

        DirectCast(sender, ToolStripMenuItem).Checked = enable
        GridView1.OptionsBehavior.Editable = enable
        If enable Then GridView1.OptionsBehavior.AllowAddRows = DevExpress.Utils.DefaultBoolean.True Else GridView1.OptionsBehavior.AllowAddRows = DevExpress.Utils.DefaultBoolean.False
        If enable Then GridView1.OptionsBehavior.AllowDeleteRows = DevExpress.Utils.DefaultBoolean.True Else GridView1.OptionsBehavior.AllowDeleteRows = DevExpress.Utils.DefaultBoolean.False
        If enable Then GridView1.OptionsView.NewItemRowPosition = DevExpress.XtraGrid.Views.Grid.NewItemRowPosition.Top Else GridView1.OptionsView.NewItemRowPosition = DevExpress.XtraGrid.Views.Grid.NewItemRowPosition.None
    End Sub

    Private Sub GameNamesShowFilterRowToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) Handles GameNamesShowFilterRowToolStripMenuItem.Click
        Dim enable As Boolean = DirectCast(sender, ToolStripMenuItem).Checked
        enable = Not enable

        DirectCast(sender, ToolStripMenuItem).Checked = enable
        GridView1.OptionsView.ShowAutoFilterRow = enable
    End Sub

    Private Sub CheatsShowEditPanelToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) Handles CheatsShowEditPanelToolStripMenuItem.Click
        Dim enable As Boolean = DirectCast(sender, ToolStripMenuItem).Checked
        enable = Not enable

        DirectCast(sender, ToolStripMenuItem).Checked = enable
        GroupBox1.Visible = enable
        If enable Then
            TreeList1.Height = TreeList1.Height - GroupBox1.Height - 10
        Else
            TreeList1.Height = TreeList1.Height + GroupBox1.Height + 10
        End If
    End Sub

    Private Sub ShowCheatCodesToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) Handles ShowCheatCodesToolStripMenuItem.Click
        Dim enable As Boolean = DirectCast(sender, ToolStripMenuItem).Checked
        enable = Not enable

        DirectCast(sender, ToolStripMenuItem).Checked = enable
        GroupBox2.Visible = enable
    End Sub
#End Region

#Region "Maintenance"
    Private Sub SearchDuplicatedCodesWithingASameGemeToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) Handles SearchDuplicatedCodesWithingASameGemeToolStripMenuItem.Click
        If SearchDuplicatedCodesWithingASameGemeToolStripMenuItem.Checked Then SearchDuplicatedCodesWithingASameGemeToolStripMenuItem.Checked = False : GridView1.RefreshData() : Exit Sub

        duplicated.Clear()
        SearchDuplicatedCodesWithingASameGemeToolStripMenuItem.Checked = True
        Label1.Text = "Searching..."
        Label1.Left = Label1.Left - 100
        Label1.Visible = True
        Dim curGameId As String = ""
        Dim curContent As String = ""
        Dim last_cheat_name As String = ""
        Dim curCheats As New Dictionary(Of String, String)
        Dim res As System.Data.SqlServerCe.SqlCeResultSet
        For rowIndex As Integer = 0 To GridView1.RowCount - 1
            Label1.Text = "Searching (" + rowIndex.ToString + " \ " + (GridView1.RowCount - 1).ToString + ")"
            Label1.Refresh()

            curGameId = GridView1.GetRowCellValue(rowIndex, GridView1.Columns(0)).ToString
            res = db.fetchData_row("SELECT content FROM " + curtable + " WHERE game_id = " + curGameId)
            res.Read() : If res.IsDBNull(0) Then Continue For
            curContent = res.GetString(0)

            curCheats.Clear()
            For Each s As String In curContent.Split({vbCrLf}, StringSplitOptions.RemoveEmptyEntries)
                If s.StartsWith("""") Then
                    s = s.Substring(1)
                    last_cheat_name = s
                    curCheats.Add(s, "")
                ElseIf Not s.StartsWith(".") Then
                    curCheats(last_cheat_name) = curCheats(last_cheat_name) + s.Trim.ToUpper + vbCrLf
                End If
            Next

            duplicated.Add(CInt(curGameId), New List(Of String))
            Dim dupes = curCheats.GroupBy(Function(x) x.Value).Where(Function(x) x.Count > 1)
            For Each curDupe In dupes
                For Each curSubDupe In curDupe
                    duplicated(CInt(curGameId)).Add(curSubDupe.Key)
                Next
            Next
        Next
        Label1.Visible = False
        Label1.Left = Label1.Left + 100
        Label1.Text = "UPDATING..."
        GridView1.RefreshData()
    End Sub

    Private Sub DeleteNaFigAllDuplicatesWithingAGameWithSameNameToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) Handles DeleteNaFigAllDupes.Click
        Label1.Text = "Searching..."
        Label1.Left = Label1.Left - 100
        Label1.Visible = True

        Dim curGameId As String = ""
        Dim curContent As String = ""
        Dim last_cheat_name As String = ""
        Dim removedCounter As Integer = 0
        Dim curCheats As New Dictionary(Of String, String)
        Dim res As System.Data.SqlServerCe.SqlCeResultSet
        For rowIndex As Integer = 0 To GridView1.RowCount - 1
            Label1.Text = "Searching (" + rowIndex.ToString + " \ " + (GridView1.RowCount - 1).ToString + ")"
            Label1.Refresh()

            curGameId = GridView1.GetRowCellValue(rowIndex, GridView1.Columns(0)).ToString
            res = db.fetchData_row("SELECT content FROM " + curtable + " WHERE game_id = " + curGameId)
            res.Read() : If res.IsDBNull(0) Then Continue For
            curContent = res.GetString(0)

            curCheats.Clear()
            For Each s As String In curContent.Split({vbCrLf}, StringSplitOptions.RemoveEmptyEntries)
                If s.StartsWith("""") Then
                    s = s.Substring(1)
                    last_cheat_name = s
                    curCheats.Add(s, "")
                Else 'If Not s.StartsWith(".") Then
                    curCheats(last_cheat_name) = curCheats(last_cheat_name) + s.Trim.ToUpper + vbCrLf
                End If
            Next

            Dim tmp As New List(Of String)
            Dim needUpdate As Boolean = False
            Dim dupes = curCheats.GroupBy(Function(x) x.Value).Where(Function(x) x.Count > 1)
            For Each curDupe In dupes
                For Each curSubDupe In curDupe
                    Dim key As String = curSubDupe.Key
                    Do While key.ToUpper.Contains("(ALT")
                        Dim index As Integer = key.ToUpper.IndexOf("(ALT")
                        Dim index2 As Integer = key.ToUpper.IndexOf(")", index + 1)
                        key = key.Substring(0, index).Trim + key.Substring(index2 + 1).Trim
                    Loop

                    If tmp.Contains(key.Trim.ToUpper) Then
                        curCheats.Remove(curSubDupe.Key)
                        needUpdate = True
                        removedCounter += 1
                    Else
                        tmp.Add(key.Trim.ToUpper)
                    End If
                Next
            Next

            If needUpdate Then
                Dim cheats As String = ""
                For Each k In curCheats.Keys
                    cheats = cheats + """" + k + vbCrLf
                    cheats = cheats + curCheats(k).Replace(Chr(0), "")
                Next
                Try
                    db.executeQuery("UPDATE " + curtable + " SET content = '" + cheats.Replace("'", "''") + "' WHERE game_id = " + curGameId)
                Catch ex As Exception
                    MsgBox(ex.Message, MsgBoxStyle.Critical, "Database query error.")
                End Try
            End If
        Next

        MsgBox(removedCounter.ToString + " duplicated removed.")
        Label1.Visible = False
        Label1.Left = Label1.Left + 100
    End Sub
#End Region

    Private Sub GridView1_CustomRowFilter(sender As Object, e As DevExpress.XtraGrid.Views.Base.RowFilterEventArgs) Handles GridView1.CustomRowFilter
        If SearchDuplicatedCodesWithingASameGemeToolStripMenuItem.Checked Then
            Dim resSet As System.Data.SqlServerCe.SqlCeResultSet = DirectCast(GridControl1.DataSource, System.Data.SqlServerCe.SqlCeResultSet)
            If resSet.HasRows And duplicated IsNot Nothing Then
                resSet.ReadAbsolute(e.ListSourceRow)
                If duplicated.ContainsKey(resSet.GetInt32(0)) AndAlso duplicated(resSet.GetInt32(0)).Count = 0 Then
                    e.Visible = False
                    e.Handled = True
                End If
            End If
        End If
    End Sub
End Class