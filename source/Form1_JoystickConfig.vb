Public Class Form1_JoystickConfig
    Private Buttons() As Button
    Private Buttons_pad() As Button
    Private TextBoxes() As TextBox
    Private TextBoxes2() As TextBox
    Private TextBoxes_pad() As TextBox
    Public Delegate Sub NotifyMainWindow(ByVal i1 As Integer, ByVal i2 As Integer)
    Public D As NotifyMainWindow

    Private Sub Form2_Load(sender As System.Object, e As System.EventArgs) Handles MyBase.Load
        Buttons = New Button() {Button1, Button2, Button3, Button4, Button5, Button6, Button7, Button8, Button9, Button10, Button11, Button12}
        Buttons_pad = New Button() {Button16, Button17, Button14, Button15}
        TextBoxes = New TextBox() {TextBox1, TextBox2, TextBox3, TextBox4, TextBox5, TextBox6, TextBox7, TextBox8, TextBox9, TextBox10, TextBox11, TextBox12}
        TextBoxes2 = New TextBox() {TextBox13, TextBox14, TextBox15, TextBox16, TextBox17, TextBox18, TextBox19, TextBox20, TextBox21}
        TextBoxes_pad = New TextBox() {TextBox22, TextBox23, TextBox24, TextBox25}

        Dim xmlJoy As New Xml.XmlDocument
        xmlJoy.Load(".\Config_Joystick.xml")
        Dim i As Integer
        Dim node As Xml.XmlNode = xmlJoy.SelectSingleNode("JoystickConfig/LoadState")
        Dim nodes As Xml.XmlNodeList = node.SelectNodes("button")
        For i = 0 To nodes.Count - 1
            TextBoxes(i).Text = nodes(i).InnerText
        Next
        node = xmlJoy.SelectSingleNode("JoystickConfig/SaveState")
        nodes = node.SelectNodes("button")
        For i = 0 To nodes.Count - 1
            TextBoxes(i + 3).Text = nodes(i).InnerText
        Next
        node = xmlJoy.SelectSingleNode("JoystickConfig/PrevSlot")
        nodes = node.SelectNodes("button")
        For i = 0 To nodes.Count - 1
            TextBoxes(i + 6).Text = nodes(i).InnerText
        Next
        node = xmlJoy.SelectSingleNode("JoystickConfig/NextSlot")
        nodes = node.SelectNodes("button")
        For i = 0 To nodes.Count - 1
            TextBoxes(i + 9).Text = nodes(i).InnerText
        Next

        node = xmlJoy.SelectSingleNode("JoystickConfig/ShowMenu")
        nodes = node.SelectNodes("button")
        For i = 0 To nodes.Count - 1
            TextBoxes2(i).Text = nodes(i).InnerText
        Next
        node = xmlJoy.SelectSingleNode("JoystickConfig/Menu_Enter")
        nodes = node.SelectNodes("button")
        For i = 0 To nodes.Count - 1
            TextBoxes2(i + 3).Text = nodes(i).InnerText
        Next
        node = xmlJoy.SelectSingleNode("JoystickConfig/Menu_Back")
        nodes = node.SelectNodes("button")
        For i = 0 To nodes.Count - 1
            TextBoxes2(i + 6).Text = nodes(i).InnerText
        Next

        node = xmlJoy.SelectSingleNode("JoystickConfig/Menu_Up")
        If node IsNot Nothing Then TextBox22.Text = node.InnerText
        node = xmlJoy.SelectSingleNode("JoystickConfig/Menu_Down")
        If node IsNot Nothing Then TextBox23.Text = node.InnerText
        node = xmlJoy.SelectSingleNode("JoystickConfig/Menu_Left")
        If node IsNot Nothing Then TextBox24.Text = node.InnerText
        node = xmlJoy.SelectSingleNode("JoystickConfig/Menu_Right")
        If node IsNot Nothing Then TextBox25.Text = node.InnerText

        AddHandler MultiThreading.myevent, AddressOf s
        D = New NotifyMainWindow(AddressOf checkTextBoxes)
    End Sub

    Private Sub Form1_JoystickConfig_Shown(sender As Object, e As System.EventArgs) Handles Me.Shown
        TextBox1.Focus()
    End Sub

    Private Sub Form1_JoystickConfig_FormClosing(sender As Object, e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        RemoveHandler MultiThreading.myevent, AddressOf s
    End Sub

    Private Sub s(i1 As Integer, i2 As Integer)
        If i2 < 1000 Then
            If i1 = 1 Then Buttons(i2).BackColor = Color.CadetBlue Else Buttons(i2).BackColor = SystemColors.Control : Exit Sub
        Else
            If i1 = 1 Then Buttons_pad(i2 - 1000).BackColor = Color.CadetBlue Else Buttons_pad(i2 - 1000).BackColor = SystemColors.Control : Exit Sub
        End If
        TextBox1.Invoke(D, {i1, i2})
    End Sub

    Private Sub checkTextBoxes(i1 As Integer, i2 As Integer)
        For i As Integer = 0 To 11
            If TextBoxes(i).Focused Then
                If i2 < 1000 Then
                    TextBoxes(i).Text = "Button" + (i2 + 1).ToString
                    If i < 11 Then TextBoxes(i + 1).Focus() Else TextBoxes2(0).Focus()
                Else
                    If i2 = 1000 Then TextBoxes(i).Text = "Up"
                    If i2 = 1001 Then TextBoxes(i).Text = "Down"
                    If i2 = 1002 Then TextBoxes(i).Text = "Left"
                    If i2 = 1003 Then TextBoxes(i).Text = "Right"
                    If i < 11 Then TextBoxes(i + 1).Focus() Else TextBoxes2(0).Focus()
                End If
                Exit For
            End If
        Next
        For i As Integer = 0 To 8
            If TextBoxes2(i).Focused Then
                If i2 < 1000 Then
                    TextBoxes2(i).Text = "Button" + (i2 + 1).ToString
                    If i < 8 Then TextBoxes2(i + 1).Focus() Else TextBoxes_pad(0).Focus()
                Else
                    If i2 = 1000 Then TextBoxes2(i).Text = "Up"
                    If i2 = 1001 Then TextBoxes2(i).Text = "Down"
                    If i2 = 1002 Then TextBoxes2(i).Text = "Left"
                    If i2 = 1003 Then TextBoxes2(i).Text = "Right"
                    If i < 8 Then TextBoxes2(i + 1).Focus() Else TextBoxes_pad(0).Focus()
                End If
                Exit For
            End If
        Next
        For i As Integer = 0 To 3
            If TextBoxes_pad(i).Focused Then
                If i2 < 1000 Then
                    TextBoxes_pad(i).Text = "Button" + (i2 + 1).ToString
                    If i < 3 Then TextBoxes_pad(i + 1).Focus()
                Else
                    If i2 = 1000 Then TextBoxes_pad(i).Text = "Up"
                    If i2 = 1001 Then TextBoxes_pad(i).Text = "Down"
                    If i2 = 1002 Then TextBoxes_pad(i).Text = "Left"
                    If i2 = 1003 Then TextBoxes_pad(i).Text = "Right"
                    If i < 3 Then TextBoxes_pad(i + 1).Focus()
                End If
                Exit For
            End If
        Next
    End Sub

    Private Sub Button13_Click(sender As System.Object, e As System.EventArgs) Handles Button13.Click
        Dim t, i As Integer
        Dim doc As New Xml.XmlDocument
        Dim root As Xml.XmlElement = doc.CreateElement("JoystickConfig")
        Dim child As Xml.XmlElement = doc.CreateElement("LoadState")
        Dim textnode As Xml.XmlNode

        For t = 0 To 9 Step 3
            If t = 3 Then child = doc.CreateElement("SaveState")
            If t = 6 Then child = doc.CreateElement("PrevSlot")
            If t = 9 Then child = doc.CreateElement("NextSlot")
            For i = t To t + 2
                If TextBoxes(i).Text <> "" Then
                    Dim element As Xml.XmlNode = doc.CreateElement("button")
                    child.AppendChild(element)

                    textnode = doc.CreateTextNode(TextBoxes(i).Text)
                    element.AppendChild(textnode)
                Else
                    Exit For
                End If
            Next
            root.AppendChild(child)
        Next

        For t = 0 To 6 Step 3
            If t = 0 Then child = doc.CreateElement("ShowMenu")
            If t = 3 Then child = doc.CreateElement("Menu_Enter")
            If t = 6 Then child = doc.CreateElement("Menu_Back")
            For i = t To t + 2
                If TextBoxes2(i).Text <> "" Then
                    Dim element As Xml.XmlNode = doc.CreateElement("button")
                    child.AppendChild(element)

                    textnode = doc.CreateTextNode(TextBoxes2(i).Text)
                    element.AppendChild(textnode)
                Else
                    Exit For
                End If
            Next
            root.AppendChild(child)
        Next

        If TextBox22.Text <> "" Then
            child = doc.CreateElement("Menu_Up")
            textnode = doc.CreateTextNode(TextBox22.Text)
            child.AppendChild(textnode)
            root.AppendChild(child)
        End If
        If TextBox23.Text <> "" Then
            child = doc.CreateElement("Menu_Down")
            textnode = doc.CreateTextNode(TextBox23.Text)
            child.AppendChild(textnode)
            root.AppendChild(child)
        End If
        If TextBox24.Text <> "" Then
            child = doc.CreateElement("Menu_Left")
            textnode = doc.CreateTextNode(TextBox24.Text)
            child.AppendChild(textnode)
            root.AppendChild(child)
        End If
        If TextBox25.Text <> "" Then
            child = doc.CreateElement("Menu_Right")
            textnode = doc.CreateTextNode(TextBox25.Text)
            child.AppendChild(textnode)
            root.AppendChild(child)
        End If
        doc.AppendChild(root)
        doc.Save("Config_Joystick.xml")
        Module1.ParseJoystickConfig()
        Me.Close()
    End Sub

    Private Sub TextBox1_KeyDown(sender As Object, e As System.Windows.Forms.KeyEventArgs) Handles TextBox1.KeyDown, TextBox2.KeyDown, TextBox3.KeyDown, _
        TextBox4.KeyDown, TextBox5.KeyDown, TextBox6.KeyDown, TextBox7.KeyDown, TextBox8.KeyDown, TextBox9.KeyDown, TextBox10.KeyDown, TextBox11.KeyDown, TextBox12.KeyDown, _
        TextBox13.KeyDown, TextBox14.KeyDown, TextBox15.KeyDown, TextBox16.KeyDown, TextBox17.KeyDown, TextBox18.KeyDown, TextBox19.KeyDown, TextBox20.KeyDown, TextBox21.KeyDown, _
        TextBox22.KeyDown, TextBox23.KeyDown, TextBox24.KeyDown, TextBox25.KeyDown

        If e.KeyCode = Keys.Escape Then DirectCast(sender, TextBox).Text = ""
        If e.KeyCode = Keys.Left Then
            For i As Integer = 1 To 11
                If TextBoxes(i).Focused Then TextBoxes(i - 1).Focus() : Exit For
            Next
            For i As Integer = 1 To 8
                If TextBoxes2(i).Focused Then TextBoxes2(i - 1).Focus() : Exit For
            Next
            If TextBox25.Focused Then TextBox24.Focus() : Exit Sub
            If TextBox24.Focused Then TextBox23.Focus() : Exit Sub
            If TextBox23.Focused Then TextBox22.Focus() : Exit Sub
            If TextBox22.Focused Then TextBox21.Focus() : Exit Sub
        End If
        If e.KeyCode = Keys.Right Then
            For i As Integer = 0 To 10
                If TextBoxes(i).Focused Then TextBoxes(i + 1).Focus() : Exit For
            Next
            For i As Integer = 0 To 7
                If TextBoxes2(i).Focused Then TextBoxes2(i + 1).Focus() : Exit For
            Next
            If TextBox22.Focused Then TextBox23.Focus() : Exit Sub
            If TextBox23.Focused Then TextBox24.Focus() : Exit Sub
            If TextBox24.Focused Then TextBox25.Focus() : Exit Sub
        End If
    End Sub
End Class