using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Pokedex.Util
{
    static class Files
    {
        public static Stream GetResourceStream(string resource)
        {
            return Assembly.GetExecutingAssembly().GetManifestResourceStream(resource);
        }
    }
}
