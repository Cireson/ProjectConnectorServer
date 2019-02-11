
Write-Host Obfuscate binaries
& 'C:\Program Files (x86)\PreEmptive Solutions\Dotfuscator Professional Edition 4.15.0\dotfuscator.exe' /q ObfuscatorSettings.xml

Write-Host Console Tasks
Copy-Item Dotfuscated\Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.ConsoleTasks.dll .\Release
#Copy-Item ..\bin\Release\\Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.ConsoleTasks.dll "%PROGRAMFIELS%\Microsoft System Center 2012 R2\Service Manager"

Write-Host Interop Helper
Copy-Item Dotfuscated\\Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.Adapter.InteropHelper.dll .\Release
#Copy-Item ..\..\Cireson.Connectors.Project.Helpers\bin\Release\\Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.Adapter.InteropHelper.* "\\lab1-scsmms1\c$\Program Files\Microsoft System Center 2012 R2\Service Manager"

Write-Host WPF
Copy-Item Dotfuscated\\Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.WPF.dll .\Release
#Copy-Item ..\..\Cireson.Connectors.Project.WPF\bin\Release\Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.WPF.* "\\lab1-scsmms1\c$\Program Files\Microsoft System Center 2012 R2\Service Manager\"

Write-Host Adapter
Copy-Item Dotfuscated\\Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.Adapter.dll .\Release
#Copy-Item ..\..\Cireson.Connectors.Project.Adapter\bin\Release\Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.Adapter.* "\\lab1-scsmms1\c$\Program Files\Microsoft System Center 2012 R2\Service Manager\"

Write-Host Workflows
Copy-Item Dotfuscated\Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.Workflows.dll .\Release
#Copy-Item ..\..\Cireson.Connectors.Project.Workflows\bin\Release\Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.Workflows.* "\\lab1-scsmms1\c$\Program Files\Microsoft System Center 2012 R2\Service Manager\"

Write-Host Licensing Client
Copy-Item ..\..\Cireson.Connectors.Project.Workflows\bin\Release\LicenseManagement.Client.dll .\Release

Write-Host Dependencies
Copy-Item ..\..\Cireson.Connectors.Project.Helpers\bin\Release\Microsoft.ProjectServer.Client.dll .\Release
Copy-Item ..\..\Cireson.Connectors.Project.Helpers\bin\Release\Microsoft.SharePoint.Client.dll .\Release
Copy-Item ..\..\Cireson.Connectors.Project.Helpers\bin\Release\Microsoft.SharePoint.Client.Runtime.dll .\Release

Write-Host Remove Moq
Remove-Item "\\lab1-scsmms1\c$\Program Files\Microsoft System Center 2012 R2\Service Manager\Moq.dll" 
Remove-Item "C:\Program Files\Microsoft System Center 2012 R2\Service Manager\Moq.dll"

Write-Host Remove PDBs
Remove-Item "\\lab1-scsmms1\c$\Program Files\Microsoft System Center 2012 R2\Service Manager\*.pdb"

Write-Host Management Pack XML
Copy-Item Cireson.ProjectAutomation.Library.xml .\Release

Write-Host Register
#C:\Windows\Microsoft.NET\Framework64\v4.0.30319\regasm.exe "c:\Program Files\Microsoft System Center 2012\Service Manager\Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.Adapter.dll"

Write-Host Copy icons
Copy-Item ..\Images\*.* .

Write-Host Seal and bundle
.\fastseal.exe .\Release\Cireson.ProjectAutomation.Library.xml /keyfile cireson.snk /company Cireson /Outdir .\Release
.\new-mpbfile.ps1 .\Release\Cireson.ProjectAutomation.Library.mp .\Release\Cireson.ProjectAutomation.Library lab1-scsmms1



