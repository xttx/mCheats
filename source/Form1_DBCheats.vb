Public Class Form1_DBCheats
    Private myBool As Boolean
    Private counter As Integer = 0
    Private cheatList As New Dictionary(Of String, Boolean)
    Private dupeColor As Color = Color.DarkOrange

    Private Sub Form1_DBCheats_Load(sender As System.Object, e As System.EventArgs) Handles MyBase.Load
        cheatsFindDupes()
    End Sub

    Private Sub cheatsFindDupes()
        counter = 0
        cheatList.Clear()
        TreeView1.Nodes.Clear()
        For Each item As String In cl.getAllGamesNames
            For Each cheatName As String In DirectCast(cl.getAllCheatsCheatNames(item), ArrayList)
                If cheatList.Keys.Contains(item + "+" + cheatName) Then Continue For
                For Each cheatName_c As String In DirectCast(cl.getAllCheatsCheatNames(item), ArrayList)
                    If cheatName = cheatName_c Then Continue For
                    myBool = compareArrayLists(DirectCast(cl.getCheatsCheatCodes(item + "+" + cheatName), ArrayList), DirectCast(cl.getCheatsCheatCodes(item + "+" + cheatName_c), ArrayList))
                    If myBool Then
                        Dim t As TreeNode : Dim t1 As TreeNode
                        If TreeView1.Nodes(item) Is Nothing Then t = TreeView1.Nodes.Add(item, item) Else t = TreeView1.Nodes(item)
                        If t.Nodes(cheatName) Is Nothing Then
                            t1 = t.Nodes.Add(cheatName, cheatName)
                            t1.ForeColor = Color.DarkRed
                        End If
                        If t.Nodes(cheatName_c) Is Nothing Then
                            t1 = t.Nodes.Add(cheatName_c, cheatName_c)
                            t1.ForeColor = dupeColor
                            counter += 1
                        End If
                        cheatList(item + "+" + cheatName_c) = True
                    End If
                Next
            Next
        Next
        Label1.Text = "Total duplicated: " + counter.ToString
    End Sub

    Private Function compareArrayLists(a As ArrayList, b As ArrayList) As Boolean
        If a.Count <> b.Count Then Return False
        For Each s As String In a
            If Not b.Contains(s) Then Return False
        Next
        Return True
    End Function

    Private Sub Button1_Click(sender As System.Object, e As System.EventArgs) Handles Button1.Click
        For Each node As TreeNode In TreeView1.Nodes
            For Each nodeL2 As TreeNode In node.Nodes
                If nodeL2.ForeColor <> dupeColor Then Continue For

                DirectCast(cl.getAllCheatsCheatNames(node.Text), ArrayList).Remove(nodeL2.Text)
                cl.getCheatsCheatCodes.Remove(node.Text & "+" & nodeL2.Text)
                cl.getCheatsCheatNotes.Remove(node.Text & "+" & nodeL2.Text)
                cl.getCheatsCheatValues.Remove(node.Text & "+" & nodeL2.Text)
            Next
        Next
        cheatsFindDupes()
    End Sub

    Private Sub Button2_Click(sender As System.Object, e As System.EventArgs) Handles Button2.Click
        cl.saveCheats("OVERWRITE", True)
    End Sub
End Class