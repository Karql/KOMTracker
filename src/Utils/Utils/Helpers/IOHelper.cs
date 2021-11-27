using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils.Helpers;

public static class IOHelper
{
    private static char WindowsDirectorySeparatorChar = '\\';
    private static char UnixDirectorySeparatorChar = '/';
    private static char WindowsVolumeSeparator = ':';

    private static IFileSystem GetDefaultFileSystem()
    {
        return new FileSystem();
    }

    public static string ResolvePath(string path)
    {
        return ResolvePath(GetDefaultFileSystem(), path);
    }

    public static string ResolvePath(this IFileSystem fileSystem, string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return path;

        if (fileSystem.Path.IsPathRooted(path)
            || (path.Length >= 2 && path[1] == WindowsVolumeSeparator))
        {
            return path;
        }

        path = path.Replace("~/", "");
        path = fileSystem.Path.Combine(AppContext.BaseDirectory, path);
        return path;
    }

    /// <summary>
    /// Create the folder if not existing for a full file name
    /// </summary>
    /// <param name="filePath">full path of the file</param>
    public static void CreateDirectoryForFile(string filePath)
    {
        CreateDirectoryForFile(GetDefaultFileSystem(), filePath);
    }

    public static void CreateDirectoryForFile(this IFileSystem fileSystem, string filePath)
    {
        string path = fileSystem.Path.GetDirectoryName(filePath);
        CreateDirectory(fileSystem, path);
    }

    public static void CreateDirectory(string path)
    {
        CreateDirectory(GetDefaultFileSystem(), path);
    }

    public static void CreateDirectory(this IFileSystem fileSystem, string path)
    {
        if (!fileSystem.Directory.Exists(path))
        {
            fileSystem.Directory.CreateDirectory(path);
        }
    }
}