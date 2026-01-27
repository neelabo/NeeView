using System;
using System.Reflection;

namespace NeeView.Effects
{
    public static class Tools
    {
        // MakePackUri is a utility method for computing a pack uri
        // for the given resource. 
        public static Uri MakePackUri(Assembly asm, string relativeFile)
        {
            //Assembly a = typeof(GrayscaleEffect).Assembly;

            if (asm != null)
            {
                // Extract the short name.
                string assemblyShortName = asm.ToString().Split(',')[0];

                string uriString = "pack://application:,,,/" +
                    assemblyShortName +
                    ";component/" +
                    relativeFile;

                return new Uri(uriString);
            }
            else
            {
                return new Uri("pack://application:,,,/" + relativeFile);
            }
        }
    }
}
