using Microsoft.Extensions.Logging;

using SortPhotosWithXmpByExifDate.Cli.ErrorCollection;
using SortPhotosWithXmpByExifDate.Cli.Repository;

using SystemInterface.IO;

namespace SortPhotosWithXmpByExifDate.Cli.Operation;

public abstract class FileOperationBase : IOperation
{
    protected readonly ILogger _logger;
    protected readonly IDirectory _directory;
    protected readonly Action<FileAlreadyExistsError> _handleError;

    protected FileOperationBase(ILogger logger, IDirectory directory, Action<FileAlreadyExistsError> handleError, bool isForce)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _directory = directory ?? throw new ArgumentNullException(nameof(directory));
        _handleError = handleError ?? throw new ArgumentNullException(nameof(handleError));
        IsForce = isForce;
        _directorySeparator = Path.DirectorySeparatorChar.ToString();
    }

    public bool IsForce { get; private set; }
    private readonly string _directorySeparator;

    public void CreateDirectory(string targetPath)
    {
        if (!Path.EndsInDirectorySeparator(targetPath))
        {
            throw new ArgumentException("The directory parameter is not a directory!", nameof(targetPath));
            // Apparently Path.Join generates a path without ending separator, 
            // but Path.GetDirectoryName will only work correctly, if the string does end with one.
        }

        var directoryPath = Path.GetDirectoryName(targetPath) ?? throw new InvalidOperationException($"Path.GetFullPath({targetPath})");

        if (IsForce && !_directory.Exists(directoryPath))
        {
            _ = _directory.CreateDirectory(directoryPath);
        }
    }

    public abstract void ChangeFiles(IEnumerable<IImageFile> files, string targetPath);

    protected void ChangeFiles(IEnumerable<IImageFile> files, string targetPath, Action<string, string> action)
    {
        CreateDirectory(targetPath);
        
        foreach (var file in files)
        {
            var targetName = JoinFile(targetPath, Path.GetFileName(file.CurrentFilename));

            if (File.Exists(targetName))
            {
                var error = new FileAlreadyExistsError(targetName, file.CurrentFilename, $"File {file.CurrentFilename} already exists at {targetName}");
                _handleError(error);
            }
            else
            {
                if (IsForce)
                {
                    try
                    {
                        action(file.CurrentFilename, targetName);
                    }
                    catch (Exception e)
                    {
                        _logger.LogExceptionError(e);
                    }
                }
                else
                {
                    _logger.LogTrace($"Ignoring IFile.Move({file.CurrentFilename}, {targetName});");
                }
                file.NewFilename = targetName;
            }
        }
    }

    internal string JoinDirectory(string dir1, string dir2)
    {
        // Pathes have to end with a separator for other Path.Functionalities to work correctly.
        // But Path.Join does create pathes without by default
        // Path.Combine does not work with relative pathes:
        // Path.Combine("/home/user/", "/") yields "/"
        // Path.Join("/home/user/", "/") yields "/home/user//"
        return Path.Join(dir1, dir2, _directorySeparator);
    }

    internal string JoinFile(string directory, string filenameWithExtension)
    {
        return Path.Join(directory, filenameWithExtension);
    }

    public string GetDirectory(string path)
    {
        return Path.Join(Path.GetDirectoryName(path), _directorySeparator);
    }


    public override string ToString()
    {
        return GetType().Name + ", IsForce: " + IsForce;
    }
}

/*
using System;
using System.IO;
					
public class Program
{
	public static void Main()
	{
		string fileNameWithPath = @"/home/david/projects/SortPhotosWithXmpByExifDate/resources/Photos-processed/MetaDataError/03-10-24_Opa_Lan_bis_03-10-26_175/03-10-24_Opa_Lan_bis_03-10-26_175.JPG";
		Console.WriteLine($"{Path.GetFullPath(fileNameWithPath)}");
		Console.WriteLine($"{Path.GetDirectoryName(fileNameWithPath)}");
		
		var path = "/home/user/foo";
		var pathWithSlash = path + "/";
		Console.WriteLine($"path: {Path.GetDirectoryName(path)}");
		Console.WriteLine($"path with slash: {Path.GetDirectoryName(pathWithSlash)}");
		
		// var pathargs = new string [] { "resources", "Photos-processed", "03-10-24_Opa_Lan_bis_03-10-26_175", "03-10-24_Opa_Lan_bis_03-10-26_175.JPG" };
		Console.WriteLine($"{Path.EndsInDirectorySeparator(fileNameWithPath)}");
		var a = "/home/david/projects/SortPhotosWithXmpByExifDate/resources/Photos-processed";
		var b = "2007/03/04/";
		var c = "/";
		var combinedPath = Path.Combine(a, b, c);
		var joinedPath = Path.Join(a, b, c);
	Console.WriteLine($"{combinedPath}");	
	Console.WriteLine($"{joinedPath}");	
		var u2 = Path.Join("/home/user/", "/");
		Console.WriteLine($"{u2}");	
		
	}
}
*/