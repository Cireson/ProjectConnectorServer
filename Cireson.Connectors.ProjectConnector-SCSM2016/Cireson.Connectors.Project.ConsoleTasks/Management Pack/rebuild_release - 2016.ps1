
Write-Host Obfuscate binaries
& 'C:\Program Files (x86)\PreEmptive Solutions\Dotfuscator Professional Edition 4.15.0\dotfuscator.exe' /q ObfuscatorSettings.xml

Write-Host Console Tasks
Copy-Item Dotfuscated\Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.ConsoleTasks.dll .


Write-Host Interop Helper
Copy-Item Dotfuscated\Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.Adapter.InteropHelper.dll .


Write-Host WPF
Copy-Item Dotfuscated\Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.WPF.dll .



Write-Host Adapter
Copy-Item Dotfuscated\Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.Adapter.dll .

Write-Host Workflows
Copy-Item Dotfuscated\Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.Workflows.dll .

Write-Host Dependencies
Copy-Item ..\..\Dependencies\LicenseManagement.Client.dll .
Copy-Item ..\..\Dependencies\Microsoft.ProjectServer.Client.dll .
Copy-Item ..\..\Dependencies\Microsoft.SharePoint.Client.dll .
Copy-Item ..\..\Dependencies\Microsoft.SharePoint.Client.Runtime.dll .

#Write-Host Moq
#Copy-Item ..\..\Cireson.Connectors.Project.Workflows\bin\Debug\Moq.dll "c:\Program Files\Microsoft System Center\Service Manager\"
#Copy-Item ..\..\Cireson.Connectors.Project.Workflows\bin\Debug\Moq.dll "C:\Program Files\Microsoft System Center\Service Manager\"

Write-Host Register COM Object
#C:\Windows\Microsoft.NET\Framework64\v4.0.30319\regasm.exe "c:\Program Files\Microsoft System Center\Service Manager\Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.Adapter.dll"

Write-Host Copy icons
Copy-Item ..\Icons\*.* .

Write-Host Seal and Bundle
.\fastseal.exe Cireson.ProjectAutomation.Library.xml /keyfile cireson.snk /company Cireson
.\new-mpbfile.ps1 Cireson.ProjectAutomation.Library.mp Cireson.ProjectAutomation.Library


pause;
