Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.IdentityModel
Imports System.IO
Imports System.Management.Automation
Imports System.Management.Automation.Runspaces
Imports System.Net
Imports System.Net.Sockets
Imports System.Security.Cryptography.X509Certificates
Imports System.Text
Imports Microsoft.Identity.Client
Imports System
Imports System.Linq
Imports System.Security.Principal
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports System.Net.Http
Imports System.Text.RegularExpressions

Public Class VLClient
    Dim client As TcpClient
    Dim sWriter As StreamWriter
    Dim NIckFrefix As Integer = New Random().Next(1111, 9999)
    Public IPAddress As String
    Public ConnectionStatus As Boolean
    Public CertiRDPCheck As Boolean = False
    Dim countdown As New TimeSpan(0, 0, 3650)
    Dim stpw As New Stopwatch

    Private Shared ClientId As String = "96b8cefd-7b38-409a-9e07-392fd190a83c" 'Client ID For Charusat.org
    Private Shared Tenant As String = "8dbd2884-120c-413b-80b2-1117dd3469a4" 'Tenant ID for Charusat.org
    Public Shared PublicClientApp As IPublicClientApplication
    Private Shared ReadOnly _scopes As String() = {"user.read"}
    Public authResult

    Public LocalFileVersion As String
    Public RemoteFileVersion As String


    Public tcpClient As New TcpClient 'creating the client
    Dim MyPublicIP As IPAddress 'Get Public IP Address
    Private Sub VLClient_FormClosed(sender As Object, e As FormClosedEventArgs) Handles Me.FormClosed


        Try
            DisconnectSession()
            'Shell("C:\Windows\System32\WindowsPowerShell\v1.0\Powershell.exe get-process | where-object {$_.mainwindowhandle -ne 0 -and $_.ProcessName -eq 'mstsc' -and $_.mainwindowtitle -like '172.16*'}| select-object id,ProcessName, mainwindowtitle | Stop-Process")
            'Shell("C:\Windows\System32\WindowsPowerShell\v1.0\Powershell.exe CmdKey /delete: " & IPAddress & " ")
            'BtnConnect.Enabled = True
        Catch ex As Exception

        End Try
    End Sub
    Private Function GetExternalIP() As Net.IPAddress
        Using wc As New Net.WebClient
            Return Net.IPAddress.Parse(Encoding.ASCII.GetString(wc.DownloadData("https://api.ipify.org/")))
            'Return IP
        End Using
    End Function
    Private Function RunScriptExecute(ByVal ScriptCommand As String) As Object


        Dim MyRunSpace As Runspace = RunspaceFactory.CreateRunspace()
        MyRunSpace.Open()
        Dim MyPipeline As Pipeline = MyRunSpace.CreatePipeline()
        Dim MyPipeline1 As Pipeline = MyRunSpace.CreatePipeline()
        MyPipeline.Commands.AddScript(ScriptCommand)
        Dim results As Collection(Of PSObject) = MyPipeline.Invoke()
        MyRunSpace.Close()
        Dim MyStringBuilder As New StringBuilder()

        For Each obj As PSObject In results
            MyStringBuilder.AppendLine(obj.ToString())
        Next
        Return MyStringBuilder

    End Function
    Private Sub VLClient_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            MyPublicIP = GetExternalIP()
            Label5.Text = MyPublicIP.ToString()

        Catch ex As Exception

        End Try

        Try
            RunScriptExecute("C:\Windows\System32\WindowsPowerShell\v1.0\Powershell.exe Invoke-WebRequest -Uri http://rdgateway.charusat.org:51240/CHARUSATVLAB.exe -OutFile C:\CHARUSATVLAB.exe")

            'Shell("C:\Windows\System32\WindowsPowerShell\v1.0\Powershell.exe Invoke-WebRequest -Uri http://rdgateway.charusat.org:51240/CHARUSATVLAB.exe -OutFile C:\CHARUSATVLAB.exe")
        Catch ex As Exception

        End Try
        'Try

        '    tcpClient.Connect("136.233.130.146", 5124) 'connecting the client the server
        '    Label5.Text = "Connected"
        'Catch ex As Exception

        'End Try

        '       Module1.Main()
        'scopes.Add($"{client_id}/.default")
        '        Console.WriteLine("Starting Synchronous Sample...")

        'Console.WriteLine($"{Environment.NewLine}End Synchronous Sample...{Environment.NewLine}Start Asynchronous Sample...")
        'AsyncSample()
        '        Console.WriteLine($"{Environment.NewLine}End Asynchronous Sample.{Environment.NewLine}Press any key to close...")
        'Console.ReadKey()

        CheckSoftwareUpdate()
        BtnConnect.Enabled = False
        BtnDisconnect.Enabled = False
        Label1.Text = ""
        Label2.Text = "Please Login to Your Charusat.Org Account First"
        CertiRDPCheck = False
        RunScript2("Certificate")
        If CertiRDPCheck = False Then
            InstallCertificate("C:\rdgateway.charusat.org.cer")
        End If
        If ConnectionStatus = False Then
        End If

    End Sub
    Private Shared Sub InstallCertificate(ByVal cerFileName As String)
        Try

            Dim certificate As X509Certificate2 = New X509Certificate2("C:\rdgateway.charusat.org.cer")
            'For Cyberoam SSL certificate for LocalMachine  in Root
            Dim store As X509Store = New X509Store(StoreName.Root, StoreLocation.LocalMachine)
            store.Open(OpenFlags.ReadWrite)
            store.Add(certificate)
            store.Close()

        Catch ex As Exception
            'Dim certificate As X509Certificate2 = New X509Certificate2("\\172.16.1.11\Source\CharusatService\Cyberoam_SSL_CA.pem")
            ''For Cyberoam SSL certificate for LocalMachine  in Root
            'Dim store As X509Store = New X509Store(StoreName.Root, StoreLocation.LocalMachine)
            'store.Open(OpenFlags.ReadWrite)
            'store.Add(certificate)
            'store.Close()

        End Try
    End Sub
    Private Function RunScript2(ByVal scriptText As String) As Object
        Dim MyRunSpace As Runspace = RunspaceFactory.CreateRunspace()
        MyRunSpace.Open()
        Dim MyPipeline As Pipeline = MyRunSpace.CreatePipeline()
        Dim MyPipeline1 As Pipeline = MyRunSpace.CreatePipeline()
        'MyPipeline.Commands.AddScript(scriptText)
        MyPipeline.Commands.AddScript("Get-ChildItem Cert:\LocalMachine\root")
        Dim results As Collection(Of PSObject) = MyPipeline.Invoke()
        For Each Id In results
            If Id.Properties("Thumbprint").Value.ToString() = "00E643B5195666CCC57C0224169DCF8B5B118FEC" Then
                'CertificateLbl.Text = Id.Properties("Thumbprint").Value.ToString()
                CertiRDPCheck = True
            End If
        Next

        MyRunSpace.Close()
        Dim MyStringBuilder As New StringBuilder()
        For Each obj As PSObject In results
            MyStringBuilder.AppendLine(obj.ToString())
        Next
        Return MyStringBuilder
    End Function
    Private Sub BtnConnect_Click(sender As Object, e As EventArgs) Handles BtnConnect.Click
        CertiRDPCheck = False
        RunScript2("Certificate")
        If CertiRDPCheck = False Then
            InstallCertificate("C:\rdgateway.charusat.org.cer")
        End If

        'ConnectToServer()
        EstablishConnection()




        'ConnectRDPInstranet()
        'Dim rdp As Object = Nothing
        'Dim client6 As MSTSCLib.IMsRdpClient6 = TryCast(rdpView.GetOcx(), MSTSCLib.IMsRdpClient6)
        'client6.Server = "34.194.31.195"
        'client6.UserName = "electromech\Administrator"
        'client6.AdvancedSettings2.ClearTextPassword = "password"
        'client6.Connect()

    End Sub
    Public Sub CheckRDPProcess()

        Try

        Catch ex As Exception

        End Try
        Dim MyRunSpace As Runspace = RunspaceFactory.CreateRunspace()
        MyRunSpace.Open()
        Dim MyPipeline As Pipeline = MyRunSpace.CreatePipeline()
        Dim MyPipeline1 As Pipeline = MyRunSpace.CreatePipeline()
        'MyPipeline.Commands.AddScript(scriptText)
        'MyPipeline.Commands.AddScript("get-wmiobject -class  win32_quickfixengineering | Where-Object {$_.HotFixID -like" + Chr(34) + "  'kb*' " + Chr(34) + " }")
        Dim BuildIPString As String = Nothing
        BuildIPString = IPAddress + "*"
        'Shell("C:\Windows\System32\WindowsPowerShell\v1.0\Powershell.exe get-process | where-object {$_.mainwindowhandle -ne 0 -and $_.ProcessName -eq 'mstsc' -and $_.mainwindowtitle -like '172.16*'}| select-object id,ProcessName, mainwindowtitle | Stop-Process")
        'Shell("C:\Windows\System32\WindowsPowerShell\v1.0\Powershell.exe get-process | where-object {$_.mainwindowhandle -ne 0 -and $_.ProcessName -eq 'mstsc' -and $_.mainwindowtitle -like " & BuildIPString & "}| select-object id,ProcessName, mainwindowtitle | Stop-Process")
        MyPipeline.Commands.AddScript(" get-process | where-object {$_.mainwindowhandle -ne 0 -and $_.ProcessName -eq 'mstsc' -and $_.mainwindowtitle -like '172.16*'}| select-object id,ProcessName, mainwindowtitle" + vbCrLf)
        'MyPipeline.Commands.AddScript(" get-process | where-object {$_.mainwindowhandle -ne 0 -and $_.ProcessName -eq 'mstsc' -and $_.mainwindowtitle -like " & BuildIPString & "}| select-object id,ProcessName, mainwindowtitle" + vbCrLf)

        'OLD String
        'MyPipeline.Commands.AddScript(" get-process | where-object {$_.mainwindowhandle -ne 0 -and $_.ProcessName -eq 'mstsc' -and $_.mainwindowtitle -like '172.16*'}| select-object id,ProcessName, mainwindowtitle" + vbCrLf)
        Dim results As Collection(Of PSObject) = MyPipeline.Invoke()

        If results.Count = 0 Then
            BtnConnect.Enabled = True
        Else
            BtnConnect.Enabled = False
            BtnDisconnect.Enabled = True
            If IPAddress = "" Then
                For Each Id In results
                    If Not Id.Properties("mainwindowtitle").Value.ToString() = "" Then
                        Dim FullIPString As String = Id.Properties("mainwindowtitle").Value.ToString()
                        Dim parts As String() = FullIPString.Split(New Char() {"-"c})
                        IPAddress = parts(0).Replace(" ", "")
                        Label1.Text = IPAddress

                    Else
                        Exit Sub
                    End If
                Next

            End If
        End If

        MyRunSpace.Close()
        Dim MyStringBuilder As New StringBuilder()
        For Each obj As PSObject In results
            MyStringBuilder.AppendLine(obj.ToString())

        Next

        'Shell("C:\Windows\System32\WindowsPowerShell\v1.0\Powershell.exe get-process | where-object {$_.mainwindowhandle -ne 0 -and $_.ProcessName -eq 'mstsc' -and $_.mainwindowtitle -like '172.16*'}| select-object id,ProcessName, mainwindowtitle | Stop-Process")
        'Button1.Text = "Connect"
    End Sub
    Public Sub ConnectRDPInstranet()
        'rdpView.Server = "172.16.12.195"
        'rdpView.UserName = "Administrator"

        'Dim isSecured As IMsTscNonScriptable = DirectCast(rdpView.GetOcx(), IMsTscNonScriptable)

        'isSecured.ClearTextPassword = "password"

        'rdpView.Connect()


    End Sub
    Public Sub EstablishConnection()
        ConnectionStatus = False
        RunScript("test")

        If ConnectionStatus = False Then

            Try
                ConnectToServer()
                'client = New TcpClient("rdgateway.charusat.org", CInt(5124))
                'client.GetStream.BeginRead(New Byte() {0}, 0, 0, New AsyncCallback(AddressOf Read), Nothing)
                'Send("Hello")
            Catch ex As Exception
                XUpdate("Can't connect to the server!")
            End Try

        Else
            'Button1.Enabled = False
            RunScriptExecute("C:\Windows\System32\WindowsPowerShell\v1.0\Powershell.exe get-process | where-object {$_.mainwindowhandle -ne 0 -and $_.ProcessName -eq 'mstsc' -and $_.mainwindowtitle -like '172.16*'}| select-object id,ProcessName, mainwindowtitle | Stop-Process")
            'Shell("C:\Windows\System32\WindowsPowerShell\v1.0\Powershell.exe get-process | where-object {$_.mainwindowhandle -ne 0 -and $_.ProcessName -eq 'mstsc' -and $_.mainwindowtitle -like '172.16*'}| select-object id,ProcessName, mainwindowtitle | Stop-Process")
            Label1.Text = "Client Already Connected"
        End If
    End Sub
    Private Function RunScript(ByVal IPADDR As String) As Object


        Dim MyRunSpace As Runspace = RunspaceFactory.CreateRunspace()
        MyRunSpace.Open()
        Dim MyPipeline As Pipeline = MyRunSpace.CreatePipeline()
        Dim MyPipeline1 As Pipeline = MyRunSpace.CreatePipeline()
        'MyPipeline.Commands.AddScript(scriptText)
        'MyPipeline.Commands.AddScript("get-wmiobject -class  win32_quickfixengineering | Where-Object {$_.HotFixID -like" + Chr(34) + "  'kb*' " + Chr(34) + " }")
        MyPipeline.Commands.AddScript(" get-process | where-object {$_.mainwindowhandle -ne 0 -and $_.ProcessName -eq 'mstsc'}| select-object ProcessName, mainwindowtitle" + vbCrLf)
        Dim results As Collection(Of PSObject) = MyPipeline.Invoke()

        For Each Id In results
            If Id.Properties("mainwindowtitle").Value.ToString() Like "172.16*" Then
                Label1.Text = "Lab PC Connected"
                ConnectionStatus = True
                'CertificateLbl.Text = Id.Properties("Thumbprint").Value.ToString()
            Else

            End If
        Next

        MyRunSpace.Close()
        Dim MyStringBuilder As New StringBuilder()
        For Each obj As PSObject In results
            MyStringBuilder.AppendLine(obj.ToString())

        Next
        Return MyStringBuilder

    End Function
    Private Sub BtnDisconnect_Click(sender As Object, e As EventArgs) Handles BtnDisconnect.Click

        DisconnectSession()

        'Try
        '    Shell("C:\Windows\System32\WindowsPowerShell\v1.0\Powershell.exe get-process | where-object {$_.mainwindowhandle -ne 0 -and $_.ProcessName -eq 'mstsc' -and $_.mainwindowtitle -like '172.16*'}| select-object id,ProcessName, mainwindowtitle | Stop-Process")
        '    Shell("C:\Windows\System32\WindowsPowerShell\v1.0\Powershell.exe CmdKey /delete: " & IPAddress & " ")
        '    BtnConnect.Enabled = True
        'Catch ex As Exception

        'End Try
        'End
        ''rdpView.Disconnect()
    End Sub
    Sub XLoad() Handles Me.Load
        Me.Text &= " " & NIckFrefix
    End Sub
    Delegate Sub _xUpdate(ByVal str As String)
    Sub XUpdate(ByVal str As String)
        If InvokeRequired Then
            Invoke(New _xUpdate(AddressOf XUpdate), str)
        Else
            If str Like "10*" Then
                IPAddress = str.ToString()
                Label1.Text = IPAddress
                '*** Connect RDP to getting IP
                ConnectToRDP()
                ClosedConnection()

                '***Timer Start**************************
                If stpw.IsRunning Then
                    stpw.Stop()
                    Timer1.Stop()
                Else
                    stpw.Stop()
                    stpw.Reset()
                    stpw.Start()
                    Timer1.Interval = 100
                    Timer1.Start()
                    'Timer1.Interval = 10000
                End If
            ElseIf str Like "full*" Then
                Label1.Text = "All the Computers Occupied"
            End If

            TextBox3.AppendText(str & vbNewLine)
        End If
    End Sub


    Sub Read(ByVal ar As IAsyncResult)
        Try
            XUpdate(New StreamReader(client.GetStream).ReadLine)
            client.GetStream.BeginRead(New Byte() {0}, 0, 0, AddressOf Read, Nothing)

        Catch ex As Exception
            XUpdate("You have disconnecting from server")
            Exit Sub
        End Try
    End Sub
    Private Sub Send(ByVal str As String)
        Try
            sWriter = New StreamWriter(client.GetStream)
            sWriter.WriteLine(str)
            sWriter.Flush()
        Catch ex As Exception
            XUpdate("You're not server")
        End Try
    End Sub


    Private Function BuildString1()
        Dim Script As New StringBuilder()
        Script.Append("C:\Windows\system32\mstsc.exe 'C:\Program Files\CharusatVirtualLab\CHARUSAT-VirtualLab\sample.rdp' /v:172.16.2.156" + vbCrLf)
        Return Script.ToString()
    End Function

    Public Sub ConnectToRDP()
        Try
            'Add RDPGateway UserName & Password to Credential Manager
            'RunScriptExecute("C:\Windows\System32\WindowsPowerShell\v1.0\Powershell.exe cmdkey /add:rdgateway.charusat.org /user:rdgateway\student /pass:password")
            Shell("C:\Windows\System32\WindowsPowerShell\v1.0\Powershell.exe cmdkey /add:rdgateway.charusat.org /user:rdgateway\student /pass:password")
            BtnConnect.Enabled = False
            Dim rdcProcess As Process = New Process()
            rdcProcess.StartInfo.FileName = Environment.ExpandEnvironmentVariables("C:\Windows\system32\cmdkey.exe")
            rdcProcess.StartInfo.Arguments = "/generic:TERMSRV/" & IPAddress & "/user:" & "Administrator" & " /pass:" & "Password"
            rdcProcess.StartInfo.Arguments = "C:\Windows\System32\WindowsPowerShell\v1.0\Powershell.exe cmdkey /add:" & IPAddress & " /user:Administrator /pass:password"
            'rdcProcess.StartInfo.Arguments = "/generic:TERMSRV/172.16.1.52 /user:" & "Administrator" & " /pass:" & "kaushik@123"

            'rdcProcess.StartInfo.Arguments = "C:\Windows\System32\WindowsPowerShell\v1.0\Powershell.exe cmdkey /add:rdgateway.charusat.org /user:rdgateway\student /pass:password"
            rdcProcess.Start()

            rdcProcess.StartInfo.FileName = Environment.ExpandEnvironmentVariables("C:\Windows\system32\mstsc.exe")
            'mstsc  10.0.1.20file.rdp /v:172.16.2.156 command for windows 7
            'C:\Windows\system32\mstsc.exe 'C:\Program Files\CharusatVirtualLab\CHARUSAT-VirtualLab\sample.rdp' /v:172.16.2.156
            'rdcProcess.StartInfo.Arguments = "'C:\Program Files\CharusatVirtualLab\CHARUSAT-VirtualLab\sample.rdp'"
            'rdcProcess.StartInfo.Arguments = "/v:" & IPAddress & ""
            Dim osVer As System.OperatingSystem = System.Environment.OSVersion
            If osVer.Version.Major = 6 AndAlso osVer.Version.Minor = 1 Then
                'Windows 7 & Windows Server 2008 R2
                rdcProcess.StartInfo.Arguments = "'C:\Program Files\CharusatVirtualLab\CHARUSAT-VirtualLab\sample.rdp'"
                rdcProcess.StartInfo.Arguments = "/v:" & IPAddress & ""
                'running Windows 7 or Windows 2008 R2
            Else
                'Windows 10 & Windows Server 2012
                rdcProcess.StartInfo.Arguments = "/v:" & IPAddress & " /g:rdgateway.charusat.org"
            End If
            'rdcProcess.StartInfo.Arguments = "C:\Program Files\CharusatVirtualLab\CHARUSAT-VirtualLab\sample.rdp" " /v:" & IPAddress. & ""
            'C :  \Program Files\CharusatVirtualLab\CHARUSAT-VirtualLab
            rdcProcess.Start()
            'OneHourTimer3.Start()
            BtnDisconnect.Enabled = True
            '***Timer Start**************************
            If stpw.IsRunning Then
                stpw.Stop()
                Timer1.Stop()
            Else
                stpw.Stop()
                stpw.Reset()
                stpw.Start()
                Timer1.Interval = 100
                Timer1.Start()
                'Timer1.Interval = 10000
            End If
        Catch ex As Exception
            Label1.Text = ex.Message
        End Try
    End Sub
    Public Sub ClosedConnection()
        client.Client.Close()
        client = Nothing
        'Button1.Text = "Connect"
    End Sub

    Public Sub DisconnectSession()
        Try
            Dim BuildIPString As String = Nothing
            BuildIPString = IPAddress + "*"
            'BuildIPString = IPAddress

            'Shell("C:\Windows\System32\WindowsPowerShell\v1.0\Powershell.exe get-process | where-object {$_.mainwindowhandle -ne 0 -and $_.ProcessName -eq 'mstsc' -and $_.mainwindowtitle -like " & BuildIPString & "}| select-object id,ProcessName, mainwindowtitle | Stop-Process")

            ''close Process
            Shell("C:\Windows\System32\WindowsPowerShell\v1.0\Powershell.exe get-process | where-object {$_.mainwindowhandle -ne 0 -and $_.ProcessName -eq 'mstsc' -and $_.mainwindowtitle -like '172.16*'}| select-object id,ProcessName, mainwindowtitle | Stop-Process")
            Shell("C:\Windows\System32\WindowsPowerShell\v1.0\Powershell.exe CmdKey /delete:rdgateway.charusat.org")
            Shell("C:\Windows\System32\WindowsPowerShell\v1.0\Powershell.exe CmdKey /delete: " & IPAddress & " ")

            'RunScriptExecute("C:\Windows\System32\WindowsPowerShell\v1.0\Powershell.exe get-process | where-object {$_.mainwindowhandle -ne 0 -and $_.ProcessName -eq 'mstsc' -and $_.mainwindowtitle -like '172.16*'}| select-object id,ProcessName, mainwindowtitle | Stop-Process")
            'RunScriptExecute("C:\Windows\System32\WindowsPowerShell\v1.0\Powershell.exe CmdKey /delete:rdgateway.charusat.org")
            'RunScriptExecute("C:\Windows\System32\WindowsPowerShell\v1.0\Powershell.exe CmdKey /delete: " & IPAddress & " ")

            BtnConnect.Enabled = True
        Catch ex As Exception

        End Try
        End

    End Sub
    Private Function CheckVersionScript(ByVal FileName As String) As Object

        Dim MyRunSpace As Runspace = RunspaceFactory.CreateRunspace()
        MyRunSpace.Open()
        Dim MyPipeline As Pipeline = MyRunSpace.CreatePipeline()
        Dim MyPipeline1 As Pipeline = MyRunSpace.CreatePipeline()
        'MyPipeline.Commands.AddScript(scriptText)
        'MyPipeline.Commands.AddScript("get-wmiobject -class  win32_quickfixengineering | Where-Object {$_.HotFixID -like" + Chr(34) + "  'kb*' " + Chr(34) + " }")
        MyPipeline.Commands.AddScript(FileName)
        Dim results As Collection(Of PSObject) = MyPipeline.Invoke()

        For Each Id In results

            If FileName Like "dir C:\CHARUSATVLAB.exe*" Then
                RemoteFileVersion = Id.Properties("FileVersion").Value.ToString()
                'Label1.Text = "Lab PC Connected"
                'ConnectionStatus = True
                'CertificateLbl.Text = Id.Properties("Thumbprint").Value.ToString()
            Else
                LocalFileVersion = Id.Properties("FileVersion").Value.ToString()
            End If
        Next

        MyRunSpace.Close()
        Dim MyStringBuilder As New StringBuilder()

        For Each obj As PSObject In results
            MyStringBuilder.AppendLine(obj.ToString())
        Next
        Return MyStringBuilder
    End Function
    Private Function LocalFile()
        Dim Script As New StringBuilder()
        Script.Append("dir 'C:\Program Files\CharusatVirtualLab\CHARUSAT-VirtualLab\CHARUSATVLAB.exe' | %{ $_.VersionInfo }" + vbCrLf)
        Return Script.ToString()
    End Function
    Private Function AutoUpdateCall()
        Dim Script As New StringBuilder()
        Script.Append("" + Chr(34) + "C:\Program Files\CharusatVirtualLab\CHARUSAT-VirtualLab\AutoUpdate.exe" + Chr(34) + "" + vbCrLf)
        Return Script.ToString()
    End Function
    Public Sub CheckSoftwareUpdate()
        Try
            RunScriptExecute("C:\Windows\System32\WindowsPowerShell\v1.0\Powershell.exe Invoke-WebRequest -Uri http://rdgateway.charusat.org:51240/CHARUSATVLAB.exe -OutFile C:\CHARUSATVLAB.exe")
            'Shell("C:\Windows\System32\WindowsPowerShell\v1.0\Powershell.exe Invoke-WebRequest -Uri http://rdgateway.charusat.org:51240/CHARUSATVLAB.exe -OutFile C:\CHARUSATVLAB.exe")
        Catch ex As Exception

        End Try

        Try
            CheckVersionScript("dir C:\CHARUSATVLAB.exe | %{ $_.VersionInfo }")
            CheckVersionScript(LocalFile)



            ' Check Last Modified Date of CHARUSATVLAB At RDGATEWAY.CHARUSAT.ORG
            'Dim Request As System.Net.WebRequest
            'Dim Response As System.Net.WebResponse
            'Dim WebFileSize As Integer
            'Request = Net.WebRequest.Create("http://rdgateway.charusat.org:51240/CHARUSATVLAB.exe")
            'Request.Method = Net.WebRequestMethods.Http.Get
            'Response = Request.GetResponse
            'WebFileSize = Response.ContentLength

            'Dim infoexeReader As System.IO.FileInfo
            'infoexeReader = My.Computer.FileSystem.GetFileInfo("C:\Program Files\CharusatVirtualLab\CHARUSAT-VirtualLab\CHARUSATVLAB.exe")
            'Dim DBfilesize As String = infoexeReader.LastWriteTime
            'Dim dbfileversion As String = infoexeReader.CreationTime.ToString()

            'Dim WebinfoexeReader As System.IO.FileInfo
            'WebinfoexeReader = My.Computer.FileSystem.GetFileInfo("C:\CHARUSATVLAB.exe")
            'Dim WebDBfilesize As String = WebinfoexeReader.LastWriteTime

            ' Check Last Modified Date of C:\Program Files\CharusatVirtualLab\CHARUSAT-VirtualLab\CHARUSATVLAB.exe
            'Dim LocalFile As New FileInfo("C:\Program Files\CharusatVirtualLab\CHARUSAT-VirtualLab\CHARUSATVLAB.exe")
            'Dim LocalFileSize As Long = LocalFile.Length

            If LocalFileVersion <> RemoteFileVersion Then
                Dim msg = "New Update Found " + vbNewLine + "Application will Closed Automatically to Install Updates" + vbNewLine + "Start Application Again Manually!!"
                Dim style1 = MsgBoxStyle.OkOnly
                Dim title = "CHARUSAT VIRTUAL LAB"
                Dim response1 = MsgBox(msg, style1, title)
                If response1 = MsgBoxResult.Ok Then
                    'RunScriptExecute(AutoUpdateCall)
                    'RunScriptExecute("C:\Program Files\CharusatVirtualLab\CHARUSAT-VirtualLab\AutoUpdate.exe")
                    Shell("C:\Program Files\CharusatVirtualLab\CHARUSAT-VirtualLab\AutoUpdate.exe")
                End If
            End If


            'If WebDBfilesize.ToString() <> DBfilesize.ToString() Then
            '    'File.Copy("\\172.16.1.11\source\CharusatService\AutoUpdate.bat", "C:\Program Files\CharusatApps\CharusatApps\AutoUpdate.bat", True)
            '    Shell("C:\Program Files\CharusatVirtualLab\CHARUSAT-VirtualLab\AutoUpdate.bat")
            '    'File.Copy("\\172.16.1.11\source\CharusatService\IPADDRA.mdb", "C:\Program Files\CharusatApps\CharusatApps\IPADDRA.mdb", True)
            '    Dim dbfile As System.IO.StreamWriter
            '    dbfile = My.Computer.FileSystem.OpenTextFileWriter("C:\CharusatApps.txt", False)
            '    dbfile.WriteLine("" & DateAndTime.Now.ToString() & "CharusatApps Updated to = " & WebinfoexeReader.ToString() & " ")
            '    dbfile.Close()
            'End If
        Catch ex As Exception
            ''*****************************Log Write***************************************************************
            'Dim dbfile As System.IO.StreamWriter
            'dbfile = My.Computer.FileSystem.OpenTextFileWriter("C:\CharusatApps.txt", True)
            'dbfile.WriteLine("" & DateAndTime.Now.ToString() & " CheckSoftwareUpdate:Server FILE Path Not Found ")
            'dbfile.Close()
        End Try
    End Sub
    Private Sub CheckRDPTimer1_Tick(sender As Object, e As EventArgs) Handles CheckRDPTimer1.Tick
        CheckRDPProcess()
    End Sub

    Private Sub OneHourTimer3_Tick(sender As Object, e As EventArgs) Handles OneHourTimer3.Tick
        Dim i As Integer = 0
        i = i + 1
        Label2.Text = i
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        If stpw.Elapsed <= countdown Then
            'Label3.Text = stpw.Elapsed.ToString
            Label4.Text = (countdown - stpw.Elapsed).ToString("hh\:mm\:ss")
        Else
            stpw.Stop()
            'Label3.Text = countdown.ToString
            Label4.Text = "00:00:00"
            DisconnectSession()
        End If
    End Sub

    Public Sub ConnectToServer()
        Try
            If IPAddress = "" Then
                Dim tcpClient As New TcpClient 'creating the client
                ''Console.WriteLine("Connecting....")
                tcpClient.Connect("136.233.130.146", 5124) 'connecting the client the server
                ''port is same as in the server
                ''    Console.WriteLine("Connected")
                ''Console.Write("Enter the string to be transmitted : ")
                Label2.Text = "Socket Connected"

                Dim UserStr As String = LblUser.Text
                Dim strMessage As String
                'strMessage = Console.ReadLine() 'reading the message from console to send
                strMessage = "Hello," + MyPublicIP.ToString() + "," + UserStr + "," + NIckFrefix.ToString()
                'strMessage = "Hello"
                Dim stm As Stream = tcpClient.GetStream() 'getting the stream of the client
                Dim ascenc As New ASCIIEncoding
                Dim byteData() As Byte = ascenc.GetBytes(strMessage) 'converting the data into bytes
                'Console.WriteLine("Transmitting")
                stm.Write(byteData, 0, byteData.Length()) 'writing/transmitting the message
                Dim replymsg(100) As Byte
                Dim size As Integer = stm.Read(replymsg, 0, 100) 'reading the reply message and getting its size

                Dim ReceivedString As String = Encoding.UTF8.GetString(replymsg)
                Dim kalpesh As String = ReceivedString.Replace(vbNullChar, "")

                If kalpesh Like "172.16*" Then
                    IPAddress = kalpesh.ToString()
                    Label1.Text = IPAddress
                    '*** Connect RDP to getting IP
                    If BtnLogin.Text = "LogOut" Then

                        ConnectToRDP()
                    End If

                    'ClosedConnection()
                    tcpClient.Close() 'closing the connection

                ElseIf kalpesh Like "Full*" Then
                    Label1.Text = "All the Computers Are Occupied"
                    MsgBox("All the Computers Are Occupied")
                End If
            Else
                ConnectToRDP()
            End If
            '   Console.WriteLine("Acknoledgement from Server")
            'For i As Integer = 0 To size
            'Console.Write(Convert.ToChar(replymsg(i))) 'writing the reply into the console
            'Next


        Catch ex As Exception
            'Console.WriteLine("Error..." + ex.StackTrace.ToString()) 'writing the exception into the console
            MsgBox("Server is Offline Please try After Sometimes")
            Label1.Text = "Server is Offline Please try After Sometimes"
        End Try
    End Sub

    Private Async Sub BtnLogin_Click(sender As Object, e As EventArgs) Handles BtnLogin.Click
        'RunScriptExecute("C:\Windows\System32\WindowsPowerShell\v1.0\Powershell.exe cmdkey /add:rdgateway.charusat.org /user:rdgateway\student /pass:password")
        Shell("C:\Windows\System32\WindowsPowerShell\v1.0\Powershell.exe cmdkey /add:rdgateway.charusat.org /user:rdgateway\student /pass:password")
        PublicClientApp = PublicClientApplicationBuilder.Create(ClientId).WithRedirectUri("http://localhost").WithDefaultRedirectUri().WithAuthority(AzureCloudInstance.AzurePublic, Tenant).Build()
        ' ConnectToServer()
        If BtnLogin.Text = "LogIn" Then
            Try
                authResult = Await PublicClientApp.AcquireTokenInteractive(_scopes).ExecuteAsync()
                'Dim authResult1 = Await PublicClientApp.AcquireTokenInteractive(_scopes).ExecuteAsync()


                BtnLogin.Text = "LogOut"
                LblUser.Text = authResult.Account.Username
                Label2.Text = "Logged In Successfully"
                'RunScriptExecute("C:\Windows\System32\WindowsPowerShell\v1.0\Powershell.exe Invoke-WebRequest -Uri http://136.233.130.146:51240/rdgateway.charusat.org.cer -OutFile C:\rdgateway.charusat.org.cer")
                Shell("C:\Windows\System32\WindowsPowerShell\v1.0\Powershell.exe Invoke-WebRequest -Uri http://136.233.130.146:51240/rdgateway.charusat.org.cer -OutFile C:\rdgateway.charusat.org.cer")
                BtnConnect.Enabled = True
                CheckRDPTimer1.Start()

            Catch ex As Exception

            End Try
        Else
            Dim accounts = Await PublicClientApp.GetAccountsAsync()
            If accounts.Any() Then
                Try
                    Await PublicClientApp.RemoveAsync(accounts.FirstOrDefault())
                    BtnLogin.Text = "Log In"
                    Label2.Text = "Logged Out Successfully"
                Catch ex As Exception

                End Try
            Else
                DisconnectSession()
                BtnLogin.Text = "Log In"
                BtnConnect.Enabled = False
                LblUser.Text = "Guest"
                Label2.Text = "Logged Out Successfully"
            End If
        End If

    End Sub


End Class
