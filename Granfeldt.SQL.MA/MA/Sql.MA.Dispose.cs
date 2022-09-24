using Microsoft.MetadirectoryServices;
using System;

namespace Granfeldt
{

    public partial class SQLManagementAgent : IDisposable, IMAExtensible2GetCapabilities, IMAExtensible2GetSchema, IMAExtensible2GetParameters, IMAExtensible2CallImport, IMAExtensible2CallExport
    {
        private bool disposedValue = false; // To detect redundant calls
        protected virtual void Dispose(bool disposing)
        {
            Tracer.Enter(nameof(Dispose));
            if (!disposedValue)
            {
                if (disposing)
                {
                    Tracer.TraceInformation("disposing-managed-objects");
                }
                disposedValue = true;
            }
            Tracer.Exit(nameof(Dispose));
        }
        ~SQLManagementAgent()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }
        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

}
