Public Class Class9_process_watcher
    Public Sub New(o As MultiThreading)
        owner = o
    End Sub

    Private owner As MultiThreading
    Dim last_found_pid As Integer = 0
    Dim p_arr() As System.Diagnostics.Process
    Public Sub test()
        Dim node As Xml.XmlNode
        Dim nodelist As Xml.XmlNodeList
        Dim emuExeList As New Dictionary(Of String, List(Of String))
        Dim useInputHook As New List(Of Boolean)
        nodelist = xmlConfig.SelectNodes("/config/emulator")
        For Each node In nodelist
            Dim drawMethodInt As Integer = -1
            Dim drawMethod As Xml.XmlNode = node.SelectSingleNode("drawmethod")
            Dim q As String = node.SelectSingleNode("quicksave").InnerText
            Dim useInputHookNode As Xml.XmlNode = xmlConfig.SelectSingleNode("/config/quicksaves/config" + q).SelectSingleNode("useinputhook")
            If Not drawMethod Is Nothing Then drawMethodInt = CInt(drawMethod.InnerText)

            If drawMethodInt = 10 Or drawMethodInt = 11 Or (useInputHookNode IsNot Nothing) Then
                Dim tempList As New List(Of String)
                For Each vernode As Xml.XmlNode In node.SelectNodes("versions/version")
                    tempList.Add(vernode.SelectSingleNode("crc").InnerText.ToUpper)
                Next
                emuExeList.Add(node.SelectSingleNode("exe").InnerText.ToLower, tempList)

                If useInputHookNode IsNot Nothing Then
                    useInputHook.Add(True)
                Else
                    useInputHook.Add(False)
                End If
            End If
        Next

        Try


            Dim i As Integer
            Dim exeName As String
            Do While True
                For i = 0 To emuExeList.Count - 1
                    exeName = emuExeList.Keys(i)
                    'If exeName.ToUpper.Contains("MAME") Or exeName.ToUpper.Contains("MESS") Then Continue For

                    p_arr = Process.GetProcessesByName(exeName)
                    If p_arr.Count > 0 Then
                        If p_arr(0).Id <> last_found_pid Then
                            If emuExeList(exeName).Contains(AppContext.GetCRC32(p_arr(0).MainModule.FileName).ToUpper) Then
                                last_found_pid = p_arr(0).Id : Log(exeName + " found! Use input hook - " + useInputHook(i).ToString)

                                'Dim p As Process = p_arr(0)
                                'Log(p.WaitForInputIdle.ToString)
                                'While Not p.WaitForInputIdle : End While
                                'For Each m As System.Diagnostics.ProcessModule In p.Modules
                                'Log(m.FileName + "---" + m.ModuleName)
                                'Next

                                owner.receiveEmuFoundMessage(p_arr(0), useInputHook(i))
                            End If
                        End If
                    End If
                Next
                Threading.Thread.Sleep(300)
            Loop


        Catch ex As Exception
            Log("Error in WatchForProcess proc: " + ex.Message)
        End Try
    End Sub
End Class
