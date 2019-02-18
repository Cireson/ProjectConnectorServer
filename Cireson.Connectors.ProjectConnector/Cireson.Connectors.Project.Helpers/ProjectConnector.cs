


//////////////////////////////////////////////////////////////////////////////
//This file is part of Cireson Project Connector. 
//
//Cireson Project Connector is free software: you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or
//(at your option) any later version.
//
//Cireson Project Connector is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
//GNU General Public License for more details.
//
//You should have received a copy of the GNU General Public License
//along with Cireson Project Connector.  If not, see<https://www.gnu.org/licenses/>.
/////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//project references
using Microsoft.ProjectServer.Client;
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.Utilities;
//other references
using System.Net;
using System.Reflection;
using System.Security;

namespace Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.InteropHelper
{
    [Obfuscation(Exclude = true, ApplyToMembers = false)]
    public class ProjectConnector
    {

        string url;
        NetworkCredential credentials;
        SharePointOnlineCredentials o365credentials;

        //project fields
        ProjectContext projContext;
        
        //public properties
        public IEnumerable<PublishedProject> Projects { get; private set; }

        public ProjectConnector(string pwaUrl, NetworkCredential creds)
        {
            url = pwaUrl;
            credentials = creds;
        }
        
        public ProjectConnector(string pwaUrl, string userName, string password, string domain)
        {
            url = pwaUrl;
            credentials = new NetworkCredential(userName, password, domain);

        }
        
        public ProjectConnector(string pwaUrl, string userName, string password)
        {
            url = pwaUrl;
            var securePassword = new SecureString();
            password.ToCharArray().ToList().ForEach(c => securePassword.AppendChar(c));

            o365credentials = new SharePointOnlineCredentials(userName, securePassword);
        }

        public void Connect()
        {
            projContext = new ProjectContext(url);
            if (credentials != null)
                projContext.Credentials = credentials;
            else
            {
                projContext.Credentials = o365credentials;
                projContext.AuthenticationMode = ClientAuthenticationMode.Anonymous;
                projContext.FormDigestHandlingEnabled = false;
            }
            projContext.Load(projContext.Web, w => w.RegionalSettings.TimeZone);
            projContext.ExecuteQuery();
        }

        public bool IsConnected()
        {
            try
            {
                LoadAllProjects();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void LoadAllProjects()
        {
            projContext.Load(projContext.Projects);
            projContext.ExecuteQuery();
            Projects = projContext.Projects;
        }

        public void LoadProject(Guid id)
        {
            Projects = projContext.LoadQuery(projContext.Projects.Where(p => p.Id == id));
            projContext.ExecuteQuery();
        }

        public void LoadProjects(DateTime lastModified)
        {
            if(DateTime.Equals(lastModified, DateTime.MinValue))
                Projects = projContext.LoadQuery(projContext.Projects.Where(p => p.LastPublishedDate > lastModified));
            else
            //lastModified is in UTC from service manager.  Project CSOM returns in local time.  Convert the UTC date to localtime.
                Projects = projContext.LoadQuery(projContext.Projects.Where(p => p.LastPublishedDate > projContext.Web.RegionalSettings.TimeZone.UTCToLocalTime(lastModified).Value));  
            projContext.ExecuteQuery();
        }

        public void LoadProjectTaskData(PublishedProject project, int outlineLevel = 0)
        {
            if (outlineLevel == 0)
                projContext.Load(project, p => p.Tasks, p => p.PercentComplete);
            else
                projContext.Load(project, p => p.Tasks.Where(t => t.OutlineLevel == outlineLevel), p => p.PercentComplete);
            projContext.ExecuteQuery();

            foreach (var task in project.Tasks)
            {
                projContext.Load(task, t => t.Assignments, t => t.PercentComplete);
                projContext.ExecuteQuery();
                foreach (var assignment in task.Assignments)
                {
                    projContext.Load(assignment.Resource);
                    projContext.Load(assignment.Resource.DefaultAssignmentOwner);
                    projContext.ExecuteQuery();
                }

            }
            
        }

        public void LoadProjectMetaData(PublishedProject project)
        {
            projContext.Load(project, p => p.Calendar, p => p.StartDate, p => p.FinishDate, p => p.ProjectSiteUrl, p => p.Description, p => p.PercentComplete, p => p.Owner);
            projContext.ExecuteQuery();
        }

        
    }
}
