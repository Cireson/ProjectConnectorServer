using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Security;

namespace Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.Adapter
{
    [ObfuscationAttribute(Exclude = true, ApplyToMembers = true)]
    [ComVisible(true)]
    [Guid("2CC066CD-0887-4D91-82B4-1050F0F0BFDA")]
    public interface IProjectAdapter
    {
        void LoadAllProjects();
        //void LoadProjects(DateTime lastModified);
        //void LoadProjectMetadata(Guid id, bool loadTasks = true);
        void Connect(string url, string username, string password, string domain);
        void Connect(string url, string username, string password);
        bool IsConnected();


    }
}