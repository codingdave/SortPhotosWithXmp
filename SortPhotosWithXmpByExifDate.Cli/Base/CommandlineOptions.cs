using System.CommandLine;

namespace SortPhotosWithXmpByExifDate.Cli.Commands;

internal class CommandlineOptions
{
    internal CommandlineOptions()
    {
        SourceOption = GetSourceOption();
        DestinationOption = GetDestinationOption();
        OffsetOption = GetOffsetOption();
        ForceOption = GetForceOption();
        MoveOption = GetMoveOption();
        SimilarityOption = GetSimilarityOption();
    }

    internal Option<string?> SourceOption { get; }
    internal Option<string?> DestinationOption { get; }
    internal Option<object?> OffsetOption { get; }
    internal Option<bool> ForceOption { get; }
    internal Option<bool> MoveOption { get; }
    internal Option<int> SimilarityOption { get; }

    internal Option<bool> GetForceOption()
    {
        return new Option<bool>(
            name: "--force",
            description: "Allow possibly destructive operations.",
            getDefaultValue: () => false
        );
    }

    internal Option<bool> GetMoveOption()
    {
        return new Option<bool>(
            name: "--move",
            description: "Operation on files, move if true. Defaults to copy.",
            getDefaultValue: () => false
        );
    }

    internal Option<int> GetSimilarityOption()
    {
        return new Option<int>
        (name: "--similarity",
        description: "Number to indicate similarity to detect an image as a duplicate of anothe. Defaults to 100%.",
        getDefaultValue: () => 100);
    }

    internal Option<object?> GetOffsetOption()
    {
        // To workaround the following issue we return an object instead of a struct 
        // "resource": "~/projects/SortPhotosWithXmpByExifDate/SortPhotosWithXmpByExifDate.Cli/CommandLine.cs",
        // "message": "Argument 4: cannot convert from 'System.CommandLine.Option<System.TimeSpan?>' to 'System.CommandLine.Binding.IValueDescriptor<System.TimeSpan>' [SortPhotosWithXmpByExifDate.Cli]",
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

    internal Option<string?> GetDestinationOption()
    {
        return new Option<string?>(
            name: "--destination",
            description: "The destination directory that contains the data.",
            isDefault: true,
            parseArgument: result =>
            {
                string? ret = null;
                var filePath = result.Tokens.SingleOrDefault()?.Value;
                if (filePath == null)
                {
                    result.ErrorMessage = "No argument given";
                }
                else
                {
                    filePath = Path.GetFullPath(Helpers.FixPath(filePath));

                    if (!Directory.Exists(filePath))
                    {
                        _ = Directory.CreateDirectory(filePath);
                    }

                    ret = filePath;
                }

                return ret;
            }
        );
    }

    internal Option<string?> GetSourceOption()
    {
        return new Option<string?>(
            name: "--source",
            description: "The source directory that contains the data.",
            isDefault: true,
            parseArgument: result =>
            {
                string? ret = null;
                var filePath = result.Tokens.SingleOrDefault()?.Value;
                if (filePath == null)
                {
                    result.ErrorMessage = "No argument given";
                }
                else
                {
                    filePath = Path.GetFullPath(Helpers.FixPath(filePath));

                    if (!Directory.Exists(filePath))
                    {
                        result.ErrorMessage = "Source directory does not exist";
                    }
                    else
                    {
                        ret = filePath;
                    }
                }

                return ret;
            }
        );
    }
}