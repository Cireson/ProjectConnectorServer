
REM Console Tasks
copy /y Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.ConsoleTasks.dll "%PROGRAMFILES%\Microsoft System Center 2012 R2\Service Manager\"

REM Interop Helper
copy /y Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.Adapter.InteropHelper.dll "%PROGRAMFILES%\Microsoft System Center 2012 R2\Service Manager\"

REM WPF
copy /y Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.WPF.dll "%PROGRAMFILES%\Microsoft System Center 2012 R2\Service Manager\"

REM Adapter
copy /y Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.Adapter.dll "%PROGRAMFILES%\Microsoft System Center 2012 R2\Service Manager\"

REM Workflows
copy /y Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.Workflows.dll "%PROGRAMFILES%\Microsoft System Center 2012 R2\Service Manager\"

REM Licensing Client
copy /y LicenseManagement.Client.dll "%PROGRAMFILES%\Microsoft System Center 2012 R2\Service Manager\"

REM Dependencies
copy /y Microsoft.ProjectServer.Client.dll "%PROGRAMFILES%\Microsoft System Center 2012 R2\Service Manager\"
copy /y Microsoft.SharePoint.Client.dll "%PROGRAMFILES%\Microsoft System Center 2012 R2\Service Manager\"
copy /y Microsoft.SharePoint.Client.Runtime.dll "%PROGRAMFILES%\Microsoft System Center 2012 R2\Service Manager\"

REM Register
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\regasm.exe "%PROGRAMFILES%\Microsoft System Center 2012 R2\Service Manager\Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.Adapter.dll"
