
// Copyright (C) 2019 Cireson, LLC. All Rights Reserved

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
