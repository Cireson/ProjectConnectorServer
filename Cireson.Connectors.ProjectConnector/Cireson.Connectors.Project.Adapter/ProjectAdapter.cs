using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//Interop reference
using Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.InteropHelper;
//other references
using System.Runtime.InteropServices;
using System.Reflection;
using System.Security;

namespace Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.Adapter
{

    [ObfuscationAttribute(Exclude = true, ApplyToMembers = true)]
    [ComVisible(true)]
    [Guid("A30B8E26-42BA-412A-A86B-45EE714969B3")]
    public class ProjectAdapter : IProjectAdapter
    {
        ProjectConnector connector;

        public void Connect(string url, string username, string password, string domain)
        {
            connector = new ProjectConnector(url, username, password, domain);
            connector.Connect();
        }

        public void Connect(string url, string username, string password)
        {
            connector = new ProjectConnector(url, username, password);
            connector.Connect();
        }

        public void LoadAllProjects()
        {
            if (connector == null)
                throw new Exception("Not connected to the Project Server App.  Run the connect method first.");
            else
                connector.LoadAllProjects();
        }

        //public void LoadProjects(DateTime lastModified)
        //{
        //    if (connector == null)
        //        throw new Exception("Not connected to the Project Server App.  Run the connect method first.");
        //    else
        //        connector.LoadProjects(lastModified);
        //}


        //public void LoadProjectMetadata(Guid id, bool loadTasks = true)
        //{
        //    if (connector == null)
        //        throw new Exception("Not connected to the Project Server App.  Run the connect method first.");
        //    else
        //        connector.LoadProjectMetadata(id, loadTasks);
        //}

        public bool IsConnected()
        {

            if (connector == null)
                return false;
            else
                return connector.IsConnected();

        }

        

    }
}
