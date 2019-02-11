Write-Host Console Tasks
Copy-Item ..\bin\Debug\Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.ConsoleTasks.dll .
Copy-Item ..\bin\Debug\\Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.ConsoleTasks.* "\\lab1-scsmms1\c$\Program Files\Microsoft System Center 2012 R2\Service Manager"

Write-Host Interop Helper
Copy-Item ..\..\Cireson.Connectors.Project.Helpers\bin\Debug\Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.Adapter.InteropHelper.dll .
Copy-Item ..\..\Cireson.Connectors.Project.Helpers\bin\Debug\\Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.Adapter.InteropHelper.* "\\lab1-scsmms1\c$\Program Files\Microsoft System Center 2012 R2\Service Manager"

Write-Host WPF
Copy-Item ..\..\Cireson.Connectors.Project.WPF\bin\Debug\Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.WPF.dll .
Copy-Item ..\..\Cireson.Connectors.Project.WPF\bin\Debug\Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.WPF.* "\\lab1-scsmms1\c$\Program Files\Microsoft System Center 2012 R2\Service Manager\"
Copy-Item ..\..\Cireson.Connectors.Project.WPF\bin\Debug\Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.WPF.* "C:\Program Files\Microsoft System Center 2012 R2\Service Manager\"

Write-Host Adapter
Copy-Item ..\..\Cireson.Connectors.Project.Adapter\bin\Debug\Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.Adapter.dll .
Copy-Item ..\..\Cireson.Connectors.Project.Adapter\bin\Debug\Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.Adapter.* "\\lab1-scsmms1\c$\Program Files\Microsoft System Center 2012 R2\Service Manager\"

Write-Host Workflows
Copy-Item ..\..\Cireson.Connectors.Project.Workflows\bin\Debug\Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.Workflows.dll .
Copy-Item ..\..\Cireson.Connectors.Project.Workflows\bin\Debug\Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.Workflows.* "\\lab1-scsmms1\c$\Program Files\Microsoft System Center 2012 R2\Service Manager\"

Write-Host Dependencies
Copy-Item ..\..\Dependencies\LicenseManagement.Client.dll .
Copy-Item ..\..\Dependencies\Microsoft.ProjectServer.Client.dll .
Copy-Item ..\..\Dependencies\Microsoft.SharePoint.Client.dll .
Copy-Item ..\..\Dependencies\Microsoft.SharePoint.Client.Runtime.dll .

Write-Host Moq
Copy-Item ..\..\Cireson.Connectors.Project.Workflows\bin\Debug\Moq.dll "\\lab1-scsmms1\c$\Program Files\Microsoft System Center 2012 R2\Service Manager\"
Copy-Item ..\..\Cireson.Connectors.Project.Workflows\bin\Debug\Moq.dll "C:\Program Files\Microsoft System Center 2012 R2\Service Manager\"

Write-Host Register COM Object
#C:\Windows\Microsoft.NET\Framework64\v4.0.30319\regasm.exe "c:\Program Files\Microsoft System Center 2012\Service Manager\Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.Adapter.dll"

Write-Host Copy icons
Copy-Item ..\Icons\*.* .

Write-Host Seal and Bundle
.\fastseal.exe Cireson.ProjectAutomation.Library.xml /keyfile cireson.snk /company Cireson
.\new-mpbfile.ps1 Cireson.ProjectAutomation.Library.mp Cireson.ProjectAutomation.Library lab1-scsmms1