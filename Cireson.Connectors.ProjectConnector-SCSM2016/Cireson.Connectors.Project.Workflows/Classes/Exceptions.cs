


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

namespace Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.Workflows.Licensing
{
    [Serializable]
    class ProjectImportException : Exception
    {
        public ProjectImportException() { }
        public ProjectImportException(string message) : base(message) { }
        public ProjectImportException(string message, Exception inner) : base(message, inner) { }
    }

    [Serializable]
    class ProjectTaskImportException : Exception
    {
        public ProjectTaskImportException() { }
        public ProjectTaskImportException(string message) : base(message) { }
        public ProjectTaskImportException(string message, Exception inner) : base(message, inner) { }
    }

    [Serializable]
    class BuildReleaseRecordException : Exception
    {
        public BuildReleaseRecordException() { }
        public BuildReleaseRecordException(string message) : base(message) { }
        public BuildReleaseRecordException(string message, Exception inner) : base(message, inner) { }
    }

    [Serializable]
    class ChangeRecordProcessingException : Exception
    {
        public ChangeRecordProcessingException() { }
        public ChangeRecordProcessingException(string message) : base(message) { }
        public ChangeRecordProcessingException(string message, Exception inner) : base(message, inner) { }
    }

    [Serializable]
    class DependentActivityProcessingException : Exception
    {
        public DependentActivityProcessingException() { }
        public DependentActivityProcessingException(string message) : base(message) { }
        public DependentActivityProcessingException(string message, Exception inner) : base(message, inner) { }
    }

    [Serializable]
    class ReleaseRecordProcessingException : Exception
    {
        public ReleaseRecordProcessingException() { }
        public ReleaseRecordProcessingException(string message) : base(message) { }
        public ReleaseRecordProcessingException(string message, Exception inner) : base(message, inner) { }
    }

    [Serializable]
    class ProjectCIGroomingException : Exception
    {
        public ProjectCIGroomingException() { }
        public ProjectCIGroomingException(string message) : base(message) { }
        public ProjectCIGroomingException(string message, Exception inner) : base(message, inner) { }
    }
}
