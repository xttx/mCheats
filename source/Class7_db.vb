Imports System.Data
Imports System.Data.SqlServerCe

Public Class Class7_db
    Dim tables(3, 8, 2) As String
    Dim fileName As String = Application.StartupPath + "\cheats\cheats.sdf"
    'Dim fileName As String = "\\tv\Hyperspin Project\mCheat\cheats\cheats.sdf"
    Dim connectString As String = String.Format("DataSource=""{0}"";", fileName)
    'Dim cn As SqlCeConnection
    Dim cn As New SqlCeConnection(connectString)

    Public Sub createDatabase()
        If Not FileIO.FileSystem.FileExists(fileName) Then
            connectString = String.Format("DataSource=""{0}"";", fileName)
            Dim eng As SqlCeEngine = New SqlCeEngine(connectString)
            eng.CreateDatabase()
        End If
        Create_system_tables("NES")
        Create_system_tables("SNES")
        Create_system_tables("GB")
        Create_system_tables("GBC")
        Create_system_tables("GBA")
        Create_system_tables("GG")
        Create_system_tables("SMS")
        Create_system_tables("SMD")
        Create_system_tables("SCD")
        Create_system_tables("S32X")
        Create_system_tables("PSX")
        Create_system_tables("PS2")
        Create_system_tables("N64")
        Create_system_tables("NGC")
        MsgBox("Database reseted")
    End Sub

    Public Sub InitTablesArray(sysname As String)
        tables(0, 0, 0) = sysname : tables(0, 0, 1) = "game_id"
        tables(0, 1, 0) = "game_id" : tables(0, 1, 1) = "integer identity not null primary key"
        tables(0, 2, 0) = "game_name" : tables(0, 2, 1) = "nvarchar (255)"
        tables(0, 3, 0) = "game_name2" : tables(0, 3, 1) = "nvarchar (255)"
        tables(0, 4, 0) = "game_name3" : tables(0, 4, 1) = "nvarchar (255)"
        tables(0, 5, 0) = "game_name4" : tables(0, 5, 1) = "nvarchar (255)"
        tables(0, 6, 0) = "game_name5" : tables(0, 6, 1) = "nvarchar (255)"
        tables(0, 7, 0) = "content" : tables(0, 7, 1) = "ntext"

        tables(1, 0, 0) = sysname + "cheats_names" : tables(1, 0, 1) = "id"
        tables(1, 1, 0) = "id" : tables(1, 1, 1) = "integer identity not null primary key"
        'tables(1, 2, 0) = "cheats_names_id" : tables(1, 2, 1) = "integer not null FOREIGN KEY "+sysname+"(0)"
        tables(1, 2, 0) = "game_id" : tables(1, 2, 1) = "integer not null"
        tables(1, 3, 0) = "name" : tables(1, 3, 1) = "nvarchar (1000)"
        tables(1, 4, 0) = "description" : tables(1, 4, 1) = "ntext"

        tables(2, 0, 0) = sysname + "cheats_codes" : tables(2, 0, 1) = "id"
        tables(2, 1, 0) = "id" : tables(2, 1, 1) = "integer identity not null primary key"
        tables(2, 2, 0) = "cheats_names_id" : tables(2, 2, 1) = "integer not null"
        tables(2, 3, 0) = "code" : tables(2, 3, 1) = "nvarchar (1000)"
    End Sub

    Public Sub Create_system_tables(sysname As String)
        Dim s As String
        Dim t, p As Integer
        InitTablesArray(sysname)

        t = 0
        Do While tables(t, 0, 0) <> ""
            If checkIfTableExist(tables(t, 0, 0)) Then CreateTable("DROP TABLE " + tables(t, 0, 0))

            '''''''''''''''''''''''''''''''''''''''
            'Switch to not process secondary tables
            If t > 0 Then t = t + 1 : Continue Do
            '''''''''''''''''''''''''''''''''''''''
            s = "CREATE TABLE " + tables(t, 0, 0) + " ("
            p = 1
            Do While tables(t, p, 0) <> ""
                s = s + tables(t, p, 0) + " " + tables(t, p, 1) + ", "
                p = p + 1
            Loop
            s = s.Substring(0, s.Length - 2) + ")"
            If Not CreateTable(s) Then
                MsgBox("Невозможно создать таблицу: " + (Chr(13) & Chr(10)) + s)
                Exit Sub
            End If
            t = t + 1
        Loop
    End Sub

    Public Function CreateTable(ByVal sql As String, Optional dontCloseCon As Boolean = False) As Boolean
        Dim res As Boolean = True
        Dim cn As New SqlCeConnection(connectString)
        If cn.State = ConnectionState.Closed Then cn.Open()
        Dim cmd As New SqlCeCommand(sql, cn)
        Try
            cmd.ExecuteNonQuery()
        Catch sqlexception As SqlCeException
            MsgBox(sqlexception.Message) : res = False
        Catch ex As Exception
            MsgBox(ex.Message) : res = False
        Finally
            If Not dontCloseCon Then cn.Close()
        End Try
        Return res
    End Function

    Public Sub ConvertTxtToSql(cheatfilename As String, sysname As String)
        cl = New Class4_CheatsLoad(".\cheats\" + cheatfilename, "LOADALL")
        Dim gameID As Integer = 0, cheatid As Integer = 0
        Dim gamename As String = "", gamename2 As String = ""
        Dim sqlGameNames As SqlCeResultSet = FetchData_Table(sysname, True) : sqlGameNames.Read()

        For g As Integer = 0 To cl.getAllGamesNames.Count - 1
            Dim content As String = ""
            gamename = cl.getAllGamesNames(g).ToString.Replace("[+]", "")
            If cl.getAllGamesNames2(g).ToString <> "" Then gamename2 = cl.getAllGamesNames2(g).ToString Else gamename2 = ""

            Dim record = sqlGameNames.CreateRecord()
            record.SetString(1, gamename)
            If Not gamename2 = "" Then record.SetString(2, gamename2)

            For Each cheatName As String In DirectCast(cl.getAllCheatsCheatNames(gamename), ArrayList)
                content = content + """" + cheatName + vbCrLf
                For Each cheatCode As String In DirectCast(cl.getCheatsCheatCodes(gamename + "+" + cheatName), ArrayList)
                    content = content + cheatCode + vbCrLf
                Next
                For Each note As String In DirectCast(cl.getCheatsCheatNotes(gamename + "+" + cheatName), ArrayList)
                    content = content + "." + note + vbCrLf
                Next
            Next
            record.SetString(6, content)
            sqlGameNames.Insert(record) : sqlGameNames.ReadLast()
        Next
        sqlGameNames.Update()
        MsgBox("DONE!")
    End Sub

    Public Sub ConvertTxtToSql_OLD(cheatfilename As String, sysname As String)
        cl = New Class4_CheatsLoad(".\cheats\" + cheatfilename, "LOADALL")
        Dim gameID As Integer = 0, cheatid As Integer = 0
        Dim gamename As String = "", gamename2 As String = ""
        Dim sqlGameNames As SqlCeResultSet = FetchData_Table(sysname) : sqlGameNames.Read()
        Dim sqlCheatNames As SqlCeResultSet = FetchData_Table(sysname + "cheats_names") : sqlCheatNames.Read()
        Dim sqlCheatCodes As SqlCeResultSet = FetchData_Table(sysname + "cheats_codes") : sqlCheatCodes.Read()

        For g As Integer = 0 To cl.getAllGamesNames.Count - 1
            gamename = cl.getAllGamesNames(g).ToString.Replace("[+]", "")
            If cl.getAllGamesNames2(g).ToString <> "" Then gamename2 = cl.getAllGamesNames2(g).ToString Else gamename2 = ""

            Dim record = sqlGameNames.CreateRecord()
            record.SetString(1, gamename)
            If Not gamename2 = "" Then record.SetString(2, gamename2)
            sqlGameNames.Insert(record) : sqlGameNames.ReadLast()
            gameID = DirectCast(sqlGameNames(0), Integer)

            For Each cheatName As String In DirectCast(cl.getAllCheatsCheatNames(gamename), ArrayList)
                Dim notes As String = ""
                Dim record2 = sqlCheatNames.CreateRecord()
                record2.SetInt32(1, gameID)
                record2.SetString(2, cheatName)
                For Each note As String In DirectCast(cl.getCheatsCheatNotes(gamename + "+" + cheatName), ArrayList)
                    notes = notes + note + vbCrLf
                Next
                If notes.Length > 2 Then notes = notes.Substring(0, notes.Length - 2)
                record2.SetString(3, notes)
                sqlCheatNames.Insert(record2) : sqlCheatNames.ReadLast()
                cheatid = DirectCast(sqlCheatNames(0), Integer)

                For Each cheatCode As String In DirectCast(cl.getCheatsCheatCodes(gamename + "+" + cheatName), ArrayList)
                    Dim record3 = sqlCheatCodes.CreateRecord()
                    record3.SetInt32(1, cheatid)
                    record3.SetString(2, cheatCode)
                    sqlCheatCodes.Insert(record3)
                Next
            Next
        Next
        sqlGameNames.Update()
        sqlCheatNames.Update()
        sqlCheatCodes.Read() : sqlCheatCodes.Update()
        MsgBox("DONE!")
    End Sub

    Public Function FetchData_Table(ByVal tableName As String, Optional table As Boolean = False) As SqlCeResultSet
        If cn.State = ConnectionState.Closed Then cn.Open()
        FetchData_Table = Nothing
        Try
            Dim cmd As SqlCeCommand = New SqlCeCommand(tableName, cn)
            If table Then cmd.CommandType = CommandType.TableDirect Else cmd.CommandType = CommandType.Text
            FetchData_Table = cmd.ExecuteResultSet(ResultSetOptions.Scrollable Or ResultSetOptions.Updatable Or ResultSetOptions.Sensitive)
        Catch sqlexception As SqlCeException
            MsgBox(sqlexception.Message)
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
        Return FetchData_Table
    End Function

    Public Sub connClose()
        cn.Close()
    End Sub

    Public Function fetchData_row(sql As String) As SqlCeResultSet
        'Dim cn As New SqlCeConnection(connectString)

        'Update DB
        'Dim tmp As New SqlCeEngine(connectString)
        'tmp.Upgrade()

        If cn.State = ConnectionState.Closed Then cn.Open()
        fetchData_row = Nothing
        Try
            Dim cmd As SqlCeCommand = New SqlCeCommand(sql, cn)
            cmd.CommandType = CommandType.Text
            fetchData_row = cmd.ExecuteResultSet(ResultSetOptions.None)
        Catch sqlexception As SqlCeException
            MsgBox(sqlexception.Message)
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
        Return fetchData_row
    End Function

    Public Function executeQuery(sql As String) As Integer
        executeQuery = 0
        If cn.State = ConnectionState.Closed Then cn.Open()
        Try
            Dim cmd As SqlCeCommand = New SqlCeCommand(sql, cn)
            cmd.CommandType = CommandType.Text
            executeQuery = cmd.ExecuteNonQuery()
        Catch sqlexception As SqlCeException
            MsgBox(sqlexception.Message)
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
        Return executeQuery
    End Function

    Public Function Enumerate_tables() As List(Of String)
        Dim list As New List(Of String)
        Dim res As SqlCeResultSet = fetchData_row("Select TABLE_NAME FROM INFORMATION_SCHEMA.TABLES")
        While res.Read()
            If Not res(0).ToString.Contains("cheats_codes") And Not res(0).ToString.Contains("cheats_names") Then list.Add(res(0).ToString)
        End While
        Return list
    End Function

    Public Function checkIfTableExist(tablename As String) As Boolean
        Dim res As SqlCeResultSet = db.fetchData_row("SELECT count(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '" + tablename + "'")
        res.Read()
        If res.GetInt32(0) = 1 Then Return True Else Return False
    End Function
End Class
