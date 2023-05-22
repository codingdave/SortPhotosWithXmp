using System.CommandLine;

namespace SortPhotosWithXmpByExifDateCli;

internal static class OptionsHelper 
{
    internal static Option<bool> GetForceOption()
    {
        return new Option<bool>(
            name: "--force",
            description: "Allow possibly destructive operations.",
            getDefaultValue: () => false
        );
    }

    internal static Option<bool> GetMoveOption()
    {
        return new Option<bool>(
            name: "--move",
            description: "Operation on files, move if true. Defaults to copy.",
            getDefaultValue: () => false
        );
    }

    internal static Option<object?> GetOffsetOption()
    {
        // To workaround the following issue we return an object instead of a struct 
        // "resource": "/home/david/projects/SortPhotosWithXmpByExifDate/SortPhotosWithXmpByExifDateCli/CommandLine.cs",
        // "message": "Argument 4: cannot convert from 'System.CommandLine.Option<System.TimeSpan?>' to 'System.CommandLine.Binding.IValueDescriptor<System.TimeSpan>' [SortPhotosWithXmpByExifDateCli]",
        // "startLineNumber": 148,
        return new Option<object?>(
            name: "--offset",
            description: "The offset that should be added to the images.",
            isDefault: true,
            parseArgument: result =>
            {
                TimeSpan? ret = null;
                var offset = result.Tokens.SingleOrDefault()?.Value;
                if (offset == null)
                {
                    result.ErrorMessage = "No argument given";
                } 
                else if (TimeSpan.TryParse(offset, out var parsed))
                {
                    ret = parsed;
                }
                else 
                {
                    result.ErrorMessage = $"cannot parse TimeSpan '{offset}'";
                }

                return ret;
            }
        );
    }
    
    internal static Option<DirectoryInfo?> GetDestinationOption()
    {
        return new Option<DirectoryInfo?>(
            name: "--destination",
            description: "The destination directory that contains the data.",
            isDefault: true,
            parseArgument: result =>
            {
                DirectoryInfo? ret = null;
                var filePath = result.Tokens.SingleOrDefault()?.Value;
                if (filePath == null)
                {
                    result.ErrorMessage = "No argument given";
                } 
                else 
                {
                    filePath = Helpers.FixPath(filePath);

                    if (!Directory.Exists(filePath))
                    {
                        Directory.CreateDirectory(filePath);
                    }

                    ret = new DirectoryInfo(filePath);
                }
               
                return ret;
            }
        );
    }

    internal static Option<DirectoryInfo?> GetSourceOption()
    {
        return new Option<DirectoryInfo?>(
            name: "--source",
            description: "The source directory that contains the data.",
            isDefault: true,
            parseArgument: result =>
            {
                DirectoryInfo? ret = null;
                var filePath = result.Tokens.SingleOrDefault()?.Value;
                if (filePath == null)
                {
                    result.ErrorMessage = "No argument given";
                } 
                else 
                {
                    filePath = Helpers.FixPath(filePath);

                    if (!Directory.Exists(filePath))
                    {
                        result.ErrorMessage = "Source directory does not exist";
                    }
                    else
                    {
                        ret = new DirectoryInfo(filePath);
                    }
                }

                return ret;
            }
        );
    }
}
