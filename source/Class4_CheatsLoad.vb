Public Class Class4_CheatsLoad
    Private p_path As String = ""
    Private p_romname As String = ""
    Private p_sysname As String = ""
    Private Cheats_CheatName As New ArrayList
    Private Cheats_CheatCodes As New Hashtable
    Private Cheats_CheatCodes_orig As New Hashtable
    'Private Cheats_CheatCodes_testing As New List(Of String)
    Private Cheats_CheatNotes As New Hashtable
    Private Cheats_CheatValues As New Hashtable
    Private All_Cheats_GameName As New ArrayList
    Private All_Cheats_GameName2 As New ArrayList
    Private All_Cheats_CheatName As New Hashtable
    Private All_Cheats_CheatVars As New Hashtable
    Private Stat_Games, Stat_Cheats, Stat_Codes As Integer

#Region "Sub New and Propertys"
    Public Sub New(ByVal path As String, ByVal romname As String, Optional ByVal sysname As String = "")
        p_path = path
        p_romname = romname
        p_sysname = sysname
        If romname = "" Then Exit Sub
        If path = "" Then
            If romname = "LOADALL" Then loadAllCheats() Else loadCheats()
        Else
            If romname = "LOADALL" Then loadAllCheats() Else loadCheats(False)
        End If
    End Sub

    Public ReadOnly Property getAllGamesNames() As ArrayList
        Get
            Return All_Cheats_GameName
        End Get
    End Property

    Public ReadOnly Property getAllGamesNames2() As ArrayList
        Get
            Return All_Cheats_GameName2
        End Get
    End Property

    Public ReadOnly Property getAllCheatsCheatNames() As Hashtable
        Get
            Return All_Cheats_CheatName
        End Get
    End Property

    Public ReadOnly Property getAllCheatsCheatVars() As Hashtable
        Get
            Return All_Cheats_CheatVars
        End Get
    End Property

    Public ReadOnly Property getCheatsCheatNames() As ArrayList
        Get
            Return Cheats_CheatName
        End Get
    End Property

    Public ReadOnly Property getCheatsCheatCodes() As Hashtable
        Get
            Return Cheats_CheatCodes
        End Get
    End Property

    Public ReadOnly Property getCheatsCheatCodes_orig() As Hashtable
        Get
            Return Cheats_CheatCodes_orig
        End Get
    End Property

    'Public ReadOnly Property getCheatsCheatCodes_testing() As List(Of String)
    'Get
    'Return Cheats_CheatCodes_testing
    'End Get
    'End Property

    Public ReadOnly Property getCheatsCheatNotes() As Hashtable
        Get
            Return Cheats_CheatNotes
        End Get
    End Property

    Public ReadOnly Property getCheatsCheatValues() As Hashtable
        Get
            Return Cheats_CheatValues
        End Get
    End Property

    Public ReadOnly Property StatGames() As Integer
        Get
            Return Stat_Games
        End Get
    End Property

    Public ReadOnly Property StatCheats() As Integer
        Get
            Return Stat_Cheats
        End Get
    End Property

    Public ReadOnly Property StatCodes() As Integer
        Get
            Return Stat_Codes
        End Get
    End Property
#End Region

    Public Sub loadCheats(Optional sql As Boolean = True)
        If Not sql Then loadCheats_old() : Exit Sub
        If p_sysname = "" Then loadCheats_old() : Exit Sub
        If p_sysname = "32X" Then p_sysname = "S32X"
        If IsNumeric(p_sysname.Substring(0, 1)) Then p_sysname = "_" + p_sysname
        Dim woBrackets As String = removeBrackets(p_romname)
        Dim content As String = ""

        Dim N As Integer
        Dim T1 As New ArrayList
        Dim T2 As New ArrayList
        Dim CurCheatName As String = ""
        'Dim classDB = New Class7_db
        Dim res As System.Data.SqlServerCe.SqlCeResultSet
        woBrackets = woBrackets.Replace("'", "''")
        Dim where As String = "game_name like '" + woBrackets + "%'"
        where = where + " or game_name2 like '" + woBrackets + "%'"
        where = where + " or game_name3 like '" + woBrackets + "%'"
        where = where + " or game_name4 like '" + woBrackets + "%'"
        where = where + " or game_name5 like '" + woBrackets + "%'"
        res = db.fetchData_row("select content from " + p_sysname + " where " + where)
        Do While res.Read
            content = res.GetString(0)
            For Each s As String In content.Split({vbCrLf}, StringSplitOptions.RemoveEmptyEntries)
                If s.StartsWith(Chr(34)) Then
                    N = 1
                    CurCheatName = s.Substring(1)
                    While Cheats_CheatCodes.Item(CurCheatName) Is Nothing = False
                        CurCheatName = s.Substring(1) + " (alt" + N.ToString + ")" : N = N + 1
                    End While

                    Stat_Cheats += 1
                    Cheats_CheatName.Add(CurCheatName)
                    T1 = New ArrayList
                    T2 = New ArrayList
                    Cheats_CheatCodes.Add(CurCheatName, T1)
                    Cheats_CheatNotes.Add(CurCheatName, T2)
                ElseIf s.StartsWith(".") Then
                    T2.Add(s.Substring(1))
                Else
                    Stat_Codes += 1
                    If s <> "" And Not s.StartsWith("$") And Not s.StartsWith(";") Then T1.Add(s)
                End If
            Next
        Loop

        'Clone Cheats_CheatCodes
        Cheats_CheatCodes_orig = New Hashtable
        Dim st As New System.IO.MemoryStream
        Dim bf As New System.Runtime.Serialization.Formatters.Binary.BinaryFormatter
        bf.Serialize(st, Cheats_CheatCodes)
        st.Position = 0
        Cheats_CheatCodes_orig = DirectCast(bf.Deserialize(st), Hashtable)
        'Cheats_CheatCodes_orig = DirectCast(Cheats_CheatCodes.Clone, Hashtable)
    End Sub

    Public Sub loadCheats_old_v2(Optional sql As Boolean = True)
        If Not sql Then loadCheats_old() : Exit Sub
        If p_sysname = "" Then loadCheats_old() : Exit Sub
        If p_sysname = "32X" Then p_sysname = "S32X"
        Dim woBrackets = removeBrackets(p_romname)

        Dim T1 As New ArrayList
        Dim T2 As New ArrayList
        Dim CurCheatName As String = ""
        Dim classDB = New Class7_db
        Dim res As System.Data.SqlServerCe.SqlCeResultSet
        res = classDB.fetchData_row("select game_id from " + p_sysname + " where game_name like '" + woBrackets + "%' or game_name2 like '" + woBrackets + "%' or game_name3 like '" + woBrackets + "%'")
        If res.Read Then
            Dim res1 As System.Data.SqlServerCe.SqlCeResultSet
            res1 = classDB.fetchData_row("select id, name, description from " + p_sysname + "cheats_names where game_id = " + res.GetInt32(0).ToString)
            Do While res1.Read
                CurCheatName = res1.GetString(1).ToString
                Cheats_CheatName.Add(CurCheatName)
                T1 = New ArrayList
                T2 = New ArrayList
                Cheats_CheatCodes.Add(CurCheatName, T1)
                Cheats_CheatNotes.Add(CurCheatName, T2) : T2.Add(res1.GetString(2))
                'For Each s1 As String In res1.GetString(2).Split(CChar(vbCr))
                'T2.Add(s1)
                'Next

                Dim res2 As System.Data.SqlServerCe.SqlCeResultSet
                res2 = classDB.fetchData_row("select code from " + p_sysname + "cheats_codes where cheats_names_id = " + res1.GetInt32(0).ToString)
                Do While res2.Read
                    T1.Add(res2.GetString(0))
                Loop
            Loop
        End If

        'Clone Cheats_CheatCodes
        Cheats_CheatCodes_orig = New Hashtable
        Dim st As New System.IO.MemoryStream
        Dim bf As New System.Runtime.Serialization.Formatters.Binary.BinaryFormatter
        bf.Serialize(st, Cheats_CheatCodes)
        st.Position = 0
        Cheats_CheatCodes_orig = DirectCast(bf.Deserialize(st), Hashtable)
        'Cheats_CheatCodes_orig = DirectCast(Cheats_CheatCodes.Clone, Hashtable)
    End Sub

    Public Sub loadCheats_old()
        Stat_Cheats = 0
        Stat_Codes = 0
        Dim S As String
        Dim N As Integer
        Dim T1 As New ArrayList
        Dim T2 As New ArrayList
        Dim CurGameName As String = ""
        Dim CurCheatName As String = ""
        Dim getCurGame As Boolean = False
        FileOpen(1, p_path, OpenMode.Input)
        While Not EOF(1)
            S = LineInput(1)
            If S.StartsWith("[") And S.Contains("]") Then
                CurGameName = S.Substring(1, S.IndexOf("]") - 1)
                If removeBrackets(CurGameName) = removeBrackets(p_romname) Then getCurGame = True Else getCurGame = False : Continue While
            ElseIf S.StartsWith(Chr(34)) And getCurGame Then
                N = 1
                CurCheatName = S.Substring(1)
                While Cheats_CheatCodes.Item(CurCheatName) Is Nothing = False
                    CurCheatName = S.Substring(1) + " (alt" + N.ToString + ")" : N = N + 1
                End While

                Stat_Cheats += 1
                Cheats_CheatName.Add(CurCheatName)
                T1 = New ArrayList
                T2 = New ArrayList
                Cheats_CheatCodes.Add(CurCheatName, T1)
                Cheats_CheatNotes.Add(CurCheatName, T2)
            ElseIf S.StartsWith(".") And getCurGame Then
                T2.Add(S.Substring(1))
            ElseIf getCurGame Then
                Stat_Codes += 1
                If S <> "" And Not S.StartsWith("$") And Not S.StartsWith(";") Then T1.Add(S)
            End If
        End While
        FileClose(1)

        'Clone Cheats_CheatCodes
        Cheats_CheatCodes_orig = New Hashtable
        Dim st As New System.IO.MemoryStream
        Dim bf As New System.Runtime.Serialization.Formatters.Binary.BinaryFormatter
        bf.Serialize(st, Cheats_CheatCodes)
        st.Position = 0
        Cheats_CheatCodes_orig = DirectCast(bf.Deserialize(st), Hashtable)
        'Cheats_CheatCodes_orig = DirectCast(Cheats_CheatCodes.Clone, Hashtable)
    End Sub

    Private Function removeBrackets(ByVal s As String, Optional ByVal removeExtention As Boolean = True) As String
        'If s.IndexOf("[") >= 0 Then
        's = s.Substring(0, s.IndexOf("["))
        'ElseIf s.IndexOf(".") >= 0 Then
        's = s.Substring(0, s.LastIndexOf("."))
        'End If

        'If s.IndexOf("(") >= 0 Then
        's = s.Substring(0, s.IndexOf("("))
        'ElseIf s.IndexOf(".") >= 0 Then
        's = s.Substring(0, s.LastIndexOf("."))
        'End If
        If s.IndexOf("[") >= 0 Or s.IndexOf("(") >= 0 Then
            If s.IndexOf("[") >= 0 Then s = s.Substring(0, s.IndexOf("["))
            If s.IndexOf("(") >= 0 Then s = s.Substring(0, s.IndexOf("("))
        ElseIf s.IndexOf(".") >= 0 And removeExtention Then
            s = s.Substring(0, s.LastIndexOf("."))
        End If

        Return s.ToUpper.Trim
    End Function

    Public Sub loadAllCheats()
        Dim S As String
        Dim N As Integer
        Dim T As New ArrayList, T1 As New ArrayList, T2 As New ArrayList, T3 As New ArrayList
        Dim CurGameName As String = ""
        Dim CurCheatName As String = ""
        Stat_Games = 0
        Stat_Cheats = 0
        Stat_Codes = 0
        FileOpen(1, p_path, OpenMode.Input)
        While Not EOF(1)
            S = LineInput(1)
            If S.StartsWith("[") And S.Contains("]") Then
                CurGameName = S.Substring(1, S.IndexOf("]") - 1)
                If All_Cheats_CheatName.Item(CurGameName) Is Nothing Then
                    T = New ArrayList
                    Stat_Games += 1
                    All_Cheats_GameName.Add(CurGameName)
                    All_Cheats_CheatName.Add(CurGameName, T)
                Else
                    'We need to distingue dupes
                    'T = All_Cheats_CheatName.Item(CurGameName)

                    Do While All_Cheats_CheatName.Item(CurGameName) IsNot Nothing
                        CurGameName = CurGameName + "[+]"
                    Loop
                    T = New ArrayList
                    Stat_Games += 1
                    All_Cheats_GameName.Add(CurGameName)
                    All_Cheats_CheatName.Add(CurGameName, T)
                End If
                If Not S.Length = S.IndexOf("]") + 1 Then All_Cheats_GameName2.Add(S.Substring(S.IndexOf("]") + 1)) Else All_Cheats_GameName2.Add("")
            ElseIf S.StartsWith(Chr(34)) Or S.StartsWith("§") Then
                N = 1
                If S.StartsWith(Chr(34)) Then CurCheatName = S.Substring(1) Else CurCheatName = S
                While Cheats_CheatCodes.Item(CurGameName & "+" & CurCheatName) Is Nothing = False
                    CurCheatName = CurCheatName + " (alt" + N.ToString + ")" : N = N + 1
                End While

                Stat_Cheats += 1
                T.Add(CurCheatName)
                T1 = New ArrayList
                T2 = New ArrayList
                T3 = New ArrayList
                Cheats_CheatCodes.Add(CurGameName & "+" & CurCheatName, T1)
                Cheats_CheatNotes.Add(CurGameName & "+" & CurCheatName, T2)
                Cheats_CheatValues.Add(CurGameName & "+" & CurCheatName, T3)
            ElseIf S.StartsWith("%") Then
                T2.Add(S)
            ElseIf S.StartsWith(".") Then
                If S <> "." Then
                    T2.Add(S.Substring(1))
                Else
                    S = LineInput(1)
                    If S.StartsWith(".") Then
                        T2.Add(S.Substring(1))
                    Else
                        T2.Add(S)
                    End If
                End If
            ElseIf S.StartsWith("$") Or S.StartsWith("?") Then
                T3.Add(S)
            ElseIf S.StartsWith("&") Then
                Dim tn As New ArrayList
                'If All_Cheats_CheatVars(CurGameName) IsNot Nothing Then
                'tn = All_Cheats_CheatVars(CurGameName)
                'Else
                All_Cheats_CheatVars.Add(CurGameName, tn)
                'End If
                tn.Add(S)
                S = LineInput(1)
                Do While S.StartsWith(".") Or S.StartsWith("$") Or S.StartsWith("&") Or S.StartsWith("?")
                    tn.Add(S)
                    S = LineInput(1)
                Loop
                Seek(1, Seek(1) - S.Length - 2)
            ElseIf S <> "" And Not S.StartsWith(";") Then
                T1.Add(S)
                Stat_Codes += 1
            End If
        End While
        FileClose(1)
    End Sub

    Public Sub saveCheats(ByVal path As String, Optional ByVal supressAlts As Boolean = False)
        Dim S As String
        Dim i As Integer = 0
        If path = "OVERWRITE" Then path = p_path
        FileOpen(1, path, OpenMode.Output)
        For Each gameName As String In All_Cheats_GameName
            If All_Cheats_GameName2(i).ToString = "" Then
                Print(1, "[" & gameName.Replace("[+]", "") & "]" + ControlChars.NewLine)
            Else
                Print(1, "[" & gameName.Replace("[+]", "") & "]" + All_Cheats_GameName2(i).ToString + ControlChars.NewLine)
            End If

            If All_Cheats_CheatVars(gameName) IsNot Nothing Then
                For Each cheat_var As String In TryCast(All_Cheats_CheatVars(gameName), ArrayList)
                    Print(1, cheat_var + ControlChars.NewLine)
                Next
            End If

            For Each cheatName As String In TryCast(All_Cheats_CheatName(gameName), ArrayList)
                If supressAlts Then S = Supress_Alts(cheatName) Else S = cheatName
                If Not S.StartsWith("§") Then S = """" & S
                Print(1, S + ControlChars.NewLine)
                For Each cheat As String In TryCast(Cheats_CheatCodes(gameName & "+" & cheatName), ArrayList)
                    Print(1, cheat + ControlChars.NewLine)
                Next
                For Each cheat_value As String In TryCast(Cheats_CheatValues(gameName & "+" & cheatName), ArrayList)
                    Print(1, cheat_value + ControlChars.NewLine)
                Next
                For Each cheat_note As String In TryCast(Cheats_CheatNotes(gameName & "+" & cheatName), ArrayList)
                    If cheat_note.StartsWith("%") Then
                        Print(1, cheat_note + ControlChars.NewLine)
                    Else
                        Print(1, "." + cheat_note + ControlChars.NewLine)
                    End If
                Next
            Next
            i = i + 1
        Next
        FileClose(1)
    End Sub

    Private Function Supress_Alts(ByVal s As String) As String
        Dim i As Integer
        For i = 1 To 52
            s = s.Replace(" (alt" + i.ToString + ")", "")
        Next
        Return s
    End Function


#Region "UNNEEDED"
    'Public Sub SetTestingCheats(cheat As String, Optional init As Boolean = False)
    'Dim chArr As New ArrayList
    'If init Then
    'chArr.Add(cheat)
    'CheatsStatus = New Hashtable
    'CheatsStatus.Add(0, 1)
    'Cheats_CheatName = New ArrayList
    'Cheats_CheatName.Add("TESTING")
    'Cheats_CheatCodes = New Hashtable
    'Cheats_CheatCodes.Add("TESTING", chArr)

    'Cheats_CheatCodes_testing = New List(Of String)
    'Cheats_CheatCodes_testing.Add(cheat)
    'Else
    'chArr = TryCast(Cheats_CheatCodes("TESTING"), ArrayList)
    'chArr.Add(cheat)
    'Cheats_CheatCodes_testing.Add(cheat)
    'End If
    'End Sub
#End Region
End Class
