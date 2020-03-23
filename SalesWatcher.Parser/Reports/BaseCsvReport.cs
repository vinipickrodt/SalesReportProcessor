using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SalesWatcher.Business
{
    public abstract class BaseCsvReport
    {
        public Stream Stream { get; protected set; }
        public string Separator { get; protected set; } = "ç";
        public object[] Result { get; protected set; }

        public BaseCsvReport(Stream stream, string separator = ";")
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (string.IsNullOrEmpty(separator))
                throw new ArgumentNullException(nameof(separator));

            this.Stream = stream;
            this.Separator = separator;
        }

        public abstract void Process();
    }
}
