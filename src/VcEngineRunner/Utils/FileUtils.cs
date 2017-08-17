using System;
using System.IO;

namespace Fldt.Runner.Utils
{
    public static class FileUtils
    {
        public static string RootFilename(string filename)
        {
            if(!Path.IsPathRooted(filename))
            {
                return Path.Combine(Environment.CurrentDirectory, filename);
            }
            return filename;
        }

        public static string ReplaceExtension(string filename, string newExtension)
        {
            return Path.Combine(Path.GetDirectoryName(filename), Path.GetFileNameWithoutExtension(filename), $".{newExtension}");
        }
    }
}
