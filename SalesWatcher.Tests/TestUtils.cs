using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SalesWatcher.Tests
{
    public static class TestUtils
    {
        public static Stream GetStreamFromString(string text)
        {
            var ms = new MemoryStream();
            var sw = new StreamWriter(ms);
            sw.AutoFlush = true;
            sw.Write(text);
            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }
    }
}
