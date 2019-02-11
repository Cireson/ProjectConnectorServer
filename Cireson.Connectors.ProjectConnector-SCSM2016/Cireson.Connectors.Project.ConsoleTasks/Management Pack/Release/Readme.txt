Cireson Project Server 2013 Connector *****RC*****
v0.8.9.4

1. Run install.ps1 on the primary management server (workflow server) via an elevated PowerShell prompt.
2. Done.


******** RELEASE NOTES *********
The install.ps1 script should install the binaries, register the interop helper via regasm.exe and install the management pack via SCSM CmdLets.  After running the script, please 
ensure the DLLs are copied to the Service Manager install directory and the MP has been installed in the management group.  If you are upgrading from a previous install
of Project Server connector, you will need to stop the "Microsoft Monitoring Agent" service prior to starting this script or it may fail to copy over the new assemblies.