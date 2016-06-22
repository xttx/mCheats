Imports System.Threading
Imports System.Threading.Tasks

Public Class Form1_EmulatorConfigWizard_findWindow
    Public _process As Process
    Dim classList As New Dictionary(Of String, List(Of TreeNode))
    Dim allNodesList As New List(Of myTag)
    Dim looping As Boolean = False
    Public Structure myTag
        Public hwnd As IntPtr
        Public className As String
    End Structure
    Private Sub Form1_EmulatorConfigWizard_findWindow_Load(sender As System.Object, e As System.EventArgs) Handles MyBase.Load
        Label1.Text = "Selected process: " + _process.ProcessName
        refr()
    End Sub


    'Treeview fill and expand
    Private Sub refr()
        classList.Clear()
        TreeView1.Nodes.Clear()
        allNodesList = New List(Of myTag)
        Dim mainNode As New TreeNode
        mainNode.Text = Hex(_process.MainWindowHandle.ToInt64) + " (" + _process.MainWindowTitle + ") "
        mainNode.Name = _process.MainWindowHandle.ToString
        mainNode.ImageIndex = 0 : mainNode.SelectedImageIndex = 0
        mainNode.Tag = New myTag With {.hwnd = _process.MainWindowHandle, .className = ""}
        allNodesList.Add(DirectCast(mainNode.Tag, myTag))
        TreeView1.Nodes.Add(mainNode)
        refr_recur(WinAPI.FindWindowEx(_process.MainWindowHandle, 0, vbNullString, vbNullString), mainNode)

        For Each i As IntPtr In WinAPI.GetOpenWindowsFromPID(_process.Id)
            If i <> _process.MainWindowHandle Then
                Dim node As New TreeNode
                Dim className As String = WinAPI.GetClassNameStr(i)
                node.Text = Hex(i.ToString) + " " + className + " """ + WinAPI.GetWindowText(i) + """"
                node.Name = Hex(i.ToInt32)
                node.Tag = New myTag With {.hwnd = i, .className = className}
                allNodesList.Add(DirectCast(node.Tag, myTag))
                TreeView1.Nodes.Add(node)

                If Not classList.ContainsKey(className) Then classList.Add(className, New List(Of TreeNode))
                classList(className).Add(node)
            End If
        Next

        TreeView1.ExpandAll()
        'If TreeView1.Nodes.Count > 0 Then TreeView1.Nodes(0).EnsureVisible()
    End Sub

    'Treeview fill recursive helper
    Private Sub refr_recur(hwnd As IntPtr, addto As TreeNode)
        If hwnd = IntPtr.Zero Then Exit Sub
        Do While True
            'Dim v As String = ""
            'If WinAPI.isWindowVisible(hwnd) Then v = "(V)"

            Dim className As String = WinAPI.GetClassNameStr(hwnd)
            Dim newNode As New TreeNode With {.Text = Hex(hwnd.ToString) + " " + className + " """ + WinAPI.GetWindowText(hwnd) + """"}
            If WinAPI.isWindowVisible(hwnd) Then
                newNode.ImageIndex = 0 : newNode.SelectedImageIndex = 0
            Else
                newNode.ImageIndex = 1 : newNode.SelectedImageIndex = 1
            End If
            newNode.Tag = New myTag With {.hwnd = hwnd, .className = className}
            allNodesList.Add(DirectCast(newNode.Tag, myTag))
            If Not classList.ContainsKey(className) Then classList.Add(className, New List(Of TreeNode))
            classList(className).Add(newNode)
            addto.Nodes.Add(newNode)

            Dim child As IntPtr = WinAPI.FindWindowEx(hwnd, 0, vbNullString, vbNullString)
            If child <> IntPtr.Zero Then refr_recur(child, newNode)

            hwnd = WinAPI.GetNextWindow(hwnd, 2)
            If hwnd = IntPtr.Zero Then Exit Do
        Loop

    End Sub

    'Button Refresh
    Private Sub Button1_Click(sender As System.Object, e As System.EventArgs) Handles Button1.Click
        refr()
    End Sub

    'Button Delayed refresh
    Private Sub Button2_Click(sender As System.Object, e As System.EventArgs) Handles Button2.Click
        If Not IsNumeric(TextBox1.Text) Then MsgBox("Number of seconds should be a number.") : Exit Sub
        Button1.Enabled = False : Button2.Enabled = False : Button3.Enabled = False : Button4.Enabled = False
        TextBox1.Enabled = False : TextBox2.Enabled = False : TreeView1.Enabled = False : CheckBox1.Enabled = False : CheckBox2.Enabled = False : CheckBox3.Enabled = False
        WaitAsync(CInt(TextBox1.Text))
    End Sub

    'Button test click
    Private Sub Button3_Click(sender As System.Object, e As System.EventArgs) Handles Button3.Click
        If looping Then looping = False : Exit Sub
        cast_image_to_hwnd()
    End Sub

    'Button Delayed test click
    Private Sub Button4_Click(sender As System.Object, e As System.EventArgs) Handles Button4.Click
        If Not IsNumeric(TextBox2.Text) Then MsgBox("Number of seconds should be a number.") : Exit Sub
        Button1.Enabled = False : Button2.Enabled = False : Button3.Enabled = False : Button4.Enabled = False
        TextBox1.Enabled = False : TextBox2.Enabled = False : TreeView1.Enabled = False : CheckBox1.Enabled = False : CheckBox2.Enabled = False : CheckBox3.Enabled = False
        WaitAsync2(CInt(TextBox2.Text))
    End Sub



    'Draw image on currently selected treeview node
    Public Sub cast_image_to_hwnd()
        If TreeView1.SelectedNode Is Nothing Then Exit Sub
        Dim _tag As myTag = DirectCast(TreeView1.SelectedNode.Tag, myTag)

        If CheckBox2.Checked Then
            Dim classname As String = _tag.className
            If classname = "" Then Exit Sub
            'Dim n As Integer = TreeView1.SelectedNode.Index
            refr() ': TreeView1.Nodes(n).EnsureVisible() : TreeView1.SelectedNode = TreeView1.Nodes(n)
            If Not classList.ContainsKey(classname) Then Exit Sub
            _tag = DirectCast(classList(classname)(0).Tag, myTag)
        End If

        Try
            If Not CheckBox3.Checked Then
                'Test one window
                Dim g As Graphics
                If TextBox3.Text.Trim = "" Then
                    g = Graphics.FromHwnd(_tag.hwnd)
                Else
                    Dim t As IntPtr = New IntPtr(CInt(TextBox3.Text.Trim))
                    g = Graphics.FromHwnd(t)
                End If

                If Not CheckBox1.Checked Then
                    'Test 500 times
                    For i As Integer = 0 To 500
                        g.FillEllipse(Brushes.Aqua, New Rectangle(10, 10, 200, 200))
                        Application.DoEvents()
                    Next
                Else
                    'Test continous
                    Button3.Text = "STOP"
                    Button1.Enabled = False : Button2.Enabled = False : Button4.Enabled = False
                    TextBox1.Enabled = False : TextBox2.Enabled = False : TreeView1.Enabled = False : CheckBox1.Enabled = False : CheckBox2.Enabled = False : CheckBox3.Enabled = False
                    looping = True
                    Do While looping
                        g.FillEllipse(Brushes.Aqua, New Rectangle(10, 10, 200, 200))
                        Application.DoEvents()
                    Loop
                End If
            Else
                'Test ALL windows
                Dim c As Integer = 0
                Dim g(allNodesList.Count) As Graphics
                For c = 0 To allNodesList.Count - 1
                    g(c) = Graphics.FromHwnd(allNodesList(c).hwnd)
                Next
                If Not CheckBox1.Checked Then
                    'Test 500 times
                    For i As Integer = 0 To 500
                        For Each gCur As Graphics In g
                            If gCur IsNot Nothing Then gCur.FillEllipse(Brushes.Aqua, New Rectangle(10, 10, 200, 200))
                            Application.DoEvents()
                        Next
                    Next
                Else
                    'Test continous
                    Button3.Text = "STOP"
                    Button1.Enabled = False : Button2.Enabled = False : Button4.Enabled = False
                    TextBox1.Enabled = False : TextBox2.Enabled = False : TreeView1.Enabled = False : CheckBox1.Enabled = False : CheckBox2.Enabled = False : CheckBox3.Enabled = False
                    looping = True
                    Do While looping
                        For Each gCur As Graphics In g
                            If gCur IsNot Nothing Then gCur.FillEllipse(Brushes.Aqua, New Rectangle(10, 10, 200, 200))
                            Application.DoEvents()
                        Next
                    Loop
                End If
            End If

        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
        Button3.Text = "Test"
        Button1.Enabled = True : Button2.Enabled = True : Button3.Enabled = True : Button4.Enabled = True
        TextBox1.Enabled = True : TextBox2.Enabled = True : TreeView1.Enabled = True : CheckBox1.Enabled = True : CheckBox2.Enabled = True : CheckBox3.Enabled = True
    End Sub

    'Wait N seconds and goes to reenable_all()
    Private Sub WaitAsync(seconds As Integer)
        Dim uiScheduler = TaskScheduler.FromCurrentSynchronizationContext()
        Task.Factory.StartNew(Sub() Thread.Sleep(seconds * 1000)).ContinueWith(Sub(t) reenable_all(), uiScheduler)
    End Sub

    'Wait N seconds and goes to reenable_all2()
    Private Sub WaitAsync2(seconds As Integer)
        Dim uiScheduler = TaskScheduler.FromCurrentSynchronizationContext()
        Task.Factory.StartNew(Sub() Thread.Sleep(seconds * 1000)).ContinueWith(Sub(t) reenable_all2(), uiScheduler)
    End Sub

    'Refresh treeview, and enable all controls
    Private Sub reenable_all()
        refr()
        Button1.Enabled = True : Button2.Enabled = True : Button3.Enabled = True : Button4.Enabled = True
        TextBox1.Enabled = True : TextBox2.Enabled = True : TreeView1.Enabled = True : CheckBox1.Enabled = True : CheckBox2.Enabled = True : CheckBox3.Enabled = True
    End Sub
    'Enable all controls and cast_image_to_hwnd()
    Private Sub reenable_all2()
        Button1.Enabled = True : Button2.Enabled = True : Button3.Enabled = True : Button4.Enabled = True
        TextBox1.Enabled = True : TextBox2.Enabled = True : TreeView1.Enabled = True : CheckBox1.Enabled = True : CheckBox2.Enabled = True : CheckBox3.Enabled = True
        cast_image_to_hwnd()
    End Sub
End Class