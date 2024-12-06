using System;
using System.IO;

namespace X975.Tools
{
    public class Pathfinder
    {
        public static string mainFolder
        {
            get
            {
                string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\DEATHEYE";

                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                return path;
            }
        }
    }
}
