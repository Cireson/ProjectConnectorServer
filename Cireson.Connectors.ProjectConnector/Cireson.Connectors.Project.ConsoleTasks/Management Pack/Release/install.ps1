$installdir = (Get-ItemProperty -Path 'HKLM:\software\Microsoft\System Center\2010\Service Manager\Setup' -Name InstallDirectory).InstallDirectory

Write-Host("Copying files to: $installdir")

#Copy-Item Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.ConsoleTasks.dll $installdir
Copy-Item Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.Adapter.InteropHelper.dll $installdir
Copy-Item Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.WPF.dll $installdir
Copy-Item Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.Adapter.dll $installdir
Copy-Item Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.Workflows.dll $installdir

Write-Host("Copying dependencies to: $installdir")

Copy-Item Microsoft.ProjectServer.Client.dll $installdir
Copy-Item Microsoft.SharePoint.Client.dll $installdir
Copy-Item Microsoft.SharePoint.Client.Runtime.dll $installdir

#Write-Host("Installing Cireson Licensing Client")
#$installedClientVersion = New-Object System.Version([System.Diagnostics.FileVersionInfo]::GetVersionInfo($installdir + "LicenseManagement.Client.dll").FileVersion)
#$installerClientVersion = New-Object System.Version([System.Diagnostics.FileVersionInfo]::GetVersionInfo($PSScriptRoot + "\LicenseManagement.Client.dll").FileVersion)

#if($installedClientVersion -eq $null)
#{
#	Copy-Item LicenseManagement.Client.dll $installdir
#}
#elseif([version]$installedClientVersion -lt [version]$installerClientVersion)
#{
#	Copy-Item LicenseManagement.Client.dll $installdir
#}

Write-Host Register COM Object
$regasm = [Environment]::GetFolderPath("Windows") + '\Microsoft.NET\Framework64\v4.0.30319\regasm.exe'
$parms =  $installdir + 'Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.Adapter.dll'

& $regasm $parms

Write-Host("Installing Management Pack")
Import-Module Microsoft.EnterpriseManagement.Core.Cmdlets
Import-SCManagementPack Cireson.ProjectAutomation.Library.mpb

Write-Host("Complete")