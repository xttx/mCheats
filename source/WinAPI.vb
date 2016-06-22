Imports System.Runtime.InteropServices
Imports System.Text

Public Class WinAPI
    Public Declare Function FindWindow Lib "user32.dll" Alias "FindWindowA" (ByVal lpClassName As String, ByVal lpWindowName As String) As IntPtr
    Public Declare Function FindWindowEx Lib "user32.dll" Alias "FindWindowExA" (ByVal hWndParent As IntPtr, ByVal hWndChildAfter As Integer, ByVal lpClassName As String, ByVal lpWindowName As String) As IntPtr
    Public Declare Function GetNextWindow Lib "user32" Alias "GetWindow" (ByVal hwnd As IntPtr, ByVal wFlag As UInt32) As IntPtr
    Public Declare Auto Function GetClassName Lib "User32.dll" (ByVal hwnd As IntPtr, <Out()> ByVal lpClassName As System.Text.StringBuilder, ByVal nMaxCount As Integer) As Integer
    Public Declare Function GetWindowLong Lib "user32" Alias "GetWindowLongA" (ByVal hwnd As IntPtr, ByVal nIndex As Long) As Long
    Const GWL_STYLE = (-16)
    Const WS_VISIBLE As UInt32 = &H10000000

    <DllImport("kernel32.dll", SetLastError:=True)> Public Shared Function ReadProcessMemory( _
       ByVal hProcess As IntPtr, _
       ByVal lpBaseAddress As IntPtr, _
       <Out()> ByVal lpBuffer() As Byte, _
       ByVal dwSize As Integer, _
       ByRef lpNumberOfBytesRead As Integer) As Boolean
    End Function
    <DllImport("kernel32.dll")> _
    Public Shared Function VirtualQueryEx(ByVal hProcess As IntPtr, ByVal lpAddress As IntPtr, ByRef lpBuffer As MEMORY_BASIC_INFORMATION, ByVal dwLength As UInt32) As Integer
    End Function
    <StructLayout(LayoutKind.Sequential)> Public Structure MEMORY_BASIC_INFORMATION
        Public BaseAddress As IntPtr
        Public AllocationBase As IntPtr
        Public AllocationProtect As UInteger
        Public RegionSize As IntPtr
        Public State As UInteger
        Public Protect As UInteger
        Public Type As UInteger
    End Structure
    Public Enum AllocationProtect As UInteger
        PAGE_EXECUTE = &H10
        PAGE_EXECUTE_READ = &H20
        PAGE_EXECUTE_READWRITE = &H40
        PAGE_EXECUTE_WRITECOPY = &H80
        PAGE_NOACCESS = &H1
        PAGE_READONLY = &H2
        PAGE_READWRITE = &H4
        PAGE_WRITECOPY = &H8
        PAGE_GUARD = &H100
        PAGE_NOCACHE = &H200
        PAGE_WRITECOMBINE = &H400
    End Enum
    <DllImport("psapi.dll", SetLastError:=True)> _
    Public Shared Function EnumProcessModules(ByVal hProcess As IntPtr, <MarshalAs(UnmanagedType.LPArray, ArraySubType:=UnmanagedType.U4)> <[In]()> <Out()> ByVal lphModule As UInteger(), ByVal cb As UInteger, <MarshalAs(UnmanagedType.U4)> ByRef lpcbNeeded As UInteger) As Boolean
    End Function
    <DllImport("coredll.dll", SetLastError:=True)> _
    Public Shared Function GetModuleFileNameEx(ByVal hProcess As IntPtr, ByVal hModule As IntPtr, ByVal lpFileName As System.Text.StringBuilder, ByVal nSize As Int32) As Int32
    End Function
    <DllImport("psapi.dll", SetLastError:=True)> _
    Public Shared Function GetModuleInformation(ByVal hProcess As IntPtr, ByVal hmodule As IntPtr, ByRef MUDULEINFO As MODULEINFO, ByVal cb As UInteger) As Boolean
    End Function
    <DllImport("psapi.dll", SetLastError:=True, CharSet:=CharSet.Unicode)> _
    Public Shared Function GetMappedFileName(ByVal ProcessHandle As IntPtr, _
        ByVal Address As IntPtr, _
        ByVal Buffer As System.Text.StringBuilder, _
        ByVal Size As Integer) As Integer
    End Function

    <DllImport("user32.dll", SetLastError:=True, CharSet:=CharSet.Auto)> _
    Private Shared Function GetWindowText(ByVal hwnd As IntPtr, ByVal lpString As StringBuilder, ByVal cch As Integer) As Integer
    End Function
    <DllImport("user32.dll", SetLastError:=True, CharSet:=CharSet.Auto)> _
    Private Shared Function GetWindowTextLength(ByVal hwnd As IntPtr) As Integer
    End Function
    <DllImport("USER32.DLL")> _
    Private Shared Function EnumWindows(ByVal enumFunc As EnumWindowsProc, ByVal lParam As Integer) As Boolean
    End Function
    Private Delegate Function EnumWindowsProc(ByVal hWnd As IntPtr, ByVal lParam As Integer) As Boolean
    <DllImport("USER32.DLL")> _
    Private Shared Function GetShellWindow() As IntPtr
    End Function
    <DllImport("user32.dll", SetLastError:=True)> _
    Private Shared Function GetWindowThreadProcessId(ByVal hWnd As IntPtr, <Out()> ByRef lpdwProcessId As UInt32) As UInt32
    End Function


    <DllImport("memsearch_x32.dll", SetLastError:=True, exactspelling:=True, CharSet:=CharSet.Ansi, CallingConvention:=CallingConvention.Cdecl)> Public Shared Function fnmemsearch(pid As Integer, pattern() As Integer, patternsize As Integer, ByRef l As Integer, returndata() As Integer) As IntPtr
    End Function
    <DllImport("memsearch_x64.dll", SetLastError:=True, exactspelling:=True, CharSet:=CharSet.Ansi, CallingConvention:=CallingConvention.Cdecl)> Public Shared Function fnmemsearch_x64(pid As Integer, pattern() As Integer, patternsize As Integer, ByRef l As Integer, returndata() As Long) As IntPtr
    End Function
    <DllImport("memsearch_x64.dll", SetLastError:=True, exactspelling:=True, CharSet:=CharSet.Ansi, CallingConvention:=CallingConvention.Cdecl)> Public Shared Function fnmemsearch_x32_on_x64(pid As Integer, pattern() As Integer, patternsize As Integer, ByRef l As Integer, returndata() As Integer) As IntPtr
    End Function
    <DllImport("memsearch_x64.dll", SetLastError:=True, exactspelling:=True, CharSet:=CharSet.Ansi, CallingConvention:=CallingConvention.Cdecl)> Public Shared Function fnmemsearch_pointer_x64(pid As Integer, valmin As UInt64, valmax As UInt64, ByRef l As Integer, returndata() As Int64) As IntPtr
    End Function
    <DllImport("memsearch_x64.dll", SetLastError:=True, exactspelling:=True, CharSet:=CharSet.Ansi, CallingConvention:=CallingConvention.Cdecl)> Public Shared Function fnmemsearch_pointer_x32_on_x64(pid As Integer, valmin As UInt32, valmax As UInt32, ByRef l As Integer, returndata() As Integer) As IntPtr
    End Function

    <DllImport("Kernel32.dll", SetLastError:=True, CallingConvention:=CallingConvention.Winapi)> _
    Public Shared Function IsWow64Process(ByVal hProcess As IntPtr, ByRef wow64Process As Boolean) _
        As <MarshalAs(UnmanagedType.Bool)> Boolean
    End Function

    <DllImport("user32.dll")> _
    Public Shared Function SendInput( _
    ByVal nInputs As Integer, _
    ByVal pInputs() As INPUT, _
    ByVal cbSize As Integer) As Integer
    End Function
    <StructLayout(LayoutKind.Explicit)> _
    Public Structure INPUT        'Field offset 32 bit machine 4, 64 bit machine 8
        <FieldOffset(0)> _
        Public type As Integer
        <FieldOffset(8)> _
        Public mi As MOUSEINPUT
        <FieldOffset(8)> _
        Public ki As KEYBDINPUT
        <FieldOffset(8)> _
        Public hi As HARDWAREINPUT
    End Structure
    Public Structure MOUSEINPUT
        Public dx As Integer
        Public dy As Integer
        Public mouseData As Integer
        Public dwFlags As Integer
        Public time As Integer
        Public dwExtraInfo As IntPtr
    End Structure
    Public Structure KEYBDINPUT
        Public wVk As Short
        Public wScan As Short
        Public dwFlags As Integer
        Public time As Integer
        Public dwExtraInfo As IntPtr
    End Structure
    Public Structure HARDWAREINPUT
        Public uMsg As Integer
        Public wParamL As Short
        Public wParamH As Short
    End Structure
    Public Structure MODULEINFO
        Public lpBaseOfDll As IntPtr
        Public SizeOfImage As UInteger
        Public EntryPoint As IntPtr
    End Structure

    <StructLayout(LayoutKind.Sequential)> Public Structure RECT
        Dim Left As Integer
        Dim Top As Integer
        Dim Right As Integer
        Dim Bottom As Integer
    End Structure

    Public Shared Function getRegionsList(hProcess As IntPtr, Optional ByRef lastQueryModules As List(Of String) = Nothing) As List(Of Long())
        Dim buffStr As String
        Dim res As Integer = 0
        Dim address As Int64 = 0
        Dim maxAddress As Integer = &H7FFFFFFF
        getRegionsList = New List(Of Long())
        lastQueryModules = New List(Of String)
        Dim m As MEMORY_BASIC_INFORMATION
        Do
            Try
                res = VirtualQueryEx(hProcess, New IntPtr(address), m, CUInt(Marshal.SizeOf(GetType(MEMORY_BASIC_INFORMATION))))
                If m.Type = 16777216 Then
                    Dim buff As New System.Text.StringBuilder(255)
                    GetMappedFileName(hProcess, m.BaseAddress, buff, 255)
                    buffStr = buff.ToString.Substring(buff.ToString.LastIndexOf("\") + 1)
                    lastQueryModules.Add(buffStr)
                Else
                    lastQueryModules.Add("")
                End If
                getRegionsList.Add({m.BaseAddress.ToInt64, m.AllocationBase.ToInt64, m.RegionSize.ToInt64, CInt(m.AllocationProtect), CInt(m.State), CInt(m.Protect), CInt(m.Type)})
                If address = m.BaseAddress.ToInt64 + m.RegionSize.ToInt64 Or address - 1 = m.BaseAddress.ToInt64 + m.RegionSize.ToInt64 Then Exit Do
                address = m.BaseAddress.ToInt64 + m.RegionSize.ToInt64 + 1
            Catch ex As Exception
                MsgBox(ex.Message)
            End Try
        Loop While address <= maxAddress
        Return getRegionsList
    End Function

    Private Shared dictWindows As New Dictionary(Of IntPtr, String)
    Private Shared listWindows As New List(Of IntPtr)
    Private Shared currentProcessID As Integer
    Private Shared hShellWindow As IntPtr = GetShellWindow()
    Public Shared Function GetWindowText(hwnd As IntPtr) As String
        Dim length As Integer = GetWindowTextLength(hwnd)
        If length = 0 Then Return ""

        Dim sb As New System.Text.StringBuilder("", length + 1)
        GetWindowText(hwnd, sb, sb.Capacity)
        Return sb.ToString()
    End Function
    Public Shared Function GetClassNameStr(hwnd As IntPtr) As String
        Dim sClassName As New System.Text.StringBuilder("", 256)
        Call GetClassName(hwnd, sClassName, 256)
        Return sClassName.ToString
    End Function
    Public Shared Function isWindowVisible(hwnd As IntPtr) As Boolean
        Dim res As Long = GetWindowLong(hwnd, GWL_STYLE)
        If (res And WS_VISIBLE) <> 0 Then Return True Else Return False
    End Function
    Public Shared Function GetOpenWindowsFromPID(ByVal processID As Integer) As IList(Of IntPtr)
        dictWindows.Clear()
        listWindows.Clear()
        currentProcessID = processID
        EnumWindows(AddressOf enumWindowsInternal, 0)
        Return listWindows
    End Function
    Private Shared Function enumWindowsInternal(ByVal hWnd As IntPtr, ByVal lParam As Integer) As Boolean
        If (hWnd <> hShellWindow) Then
            Dim windowPid As UInt32
            'If Not isWindowVisible(hWnd) Then Return True
            'Dim length As Integer = GetWindowTextLength(hWnd)
            'If (length = 0) Then Return True

            GetWindowThreadProcessId(hWnd, windowPid)

            If (windowPid <> currentProcessID) Then Return True
            'Dim stringBuilder As New System.Text.StringBuilder(length)
            'GetWindowText(hWnd, stringBuilder, (length + 1))
            'dictWindows.Add(hWnd, stringBuilder.ToString)
            'dictWindows.Add(hWnd, StringBuilder.ToString)
            listWindows.Add(hWnd)
        End If
        Return True
    End Function

End Class
