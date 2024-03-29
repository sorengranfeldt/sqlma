﻿using System;

namespace Granfeldt
{

    public partial class SqlMethods : IDisposable
    {

        // disposability
        private bool disposedValue = false; // To detect redundant calls
        protected virtual void Dispose(bool disposing)
        {
            Tracer.Enter(nameof(Dispose));
            if (!disposedValue)
            {
                if (disposing)
                {
                    Tracer.TraceInformation("disposing-managed-objects");
                    this.CloseConnection();
                }
                disposedValue = true;
            }
            Tracer.Exit(nameof(Dispose));
        }
        ~SqlMethods()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}