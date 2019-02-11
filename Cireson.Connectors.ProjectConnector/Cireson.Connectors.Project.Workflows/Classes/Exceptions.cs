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
