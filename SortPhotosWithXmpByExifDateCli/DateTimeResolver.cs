using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.Iptc;
using MetadataExtractor.Formats.QuickTime;
using MetadataExtractor.Formats.Xmp;
using DirectoryExtensions = MetadataExtractor.DirectoryExtensions;
using System.Globalization;
using Microsoft.Extensions.Logging;

namespace SortPhotosWithXmpByExifDateCli;

public class DateTimeResolver
{
    private readonly ILogger _logger;

    public DateTimeResolver(ILogger staticLogger)
    {
        _logger = staticLogger;
    }

    public DateTime GetDateTimeFromImage(IReadOnlyList<MetadataExtractor.Directory> directories)
    {
        // Exif IFD0 - Date/Time = 2023:01:18 10:54:28
        // Exif SubIFD - Date/Time Digitized = 2023:01:18 10:17:32
        // Exif SubIFD - Date/Time Original = 2023:01:18 10:17:32
        // IPTC - Date Created = 2023:01:18
        // IPTC - Digital Date Created = 2023:01:18
        // IPTC - Digital Time Created = 10:17:32+0100
        // IPTC - Time Created = 10:17:32+0100

        DateTime ret = DateTime.MinValue;

        Func<IReadOnlyList<MetadataExtractor.Directory>, DateTime>[] functions = new[]
        {
            DateTimeFromExif,
            DateTimeFromExifSubIf,
            DateTimeFromIptc,
            DateTimeFromQuicktime,
            DateTimeFromXmp
        };

        foreach (var f in functions)
        {
            if (ret == DateTime.MinValue)
            {
                ret = f(directories);
            }
        }

        return ret;
    }

    private DateTime DateTimeFromXmp(IReadOnlyList<MetadataExtractor.Directory> directories)
    {
        var ret = DateTime.MinValue;
        // my own modifications are like: exif:DateTimeOriginal: 2015-12-06T00:00:01+00:00
        var format = "yyyy-MM-ddTHH:mm:ss+00:00";
        var xmpString = "exif:DateTimeOriginal";
        foreach (var xmpDirectory in directories.OfType<XmpDirectory>())
        {
            if (xmpDirectory.XmpMeta != null)
            {
                foreach (var property in xmpDirectory.XmpMeta.Properties)
                {
                    var exifProperty = xmpString.Equals(property.Path);
                    if (exifProperty)
                    {
                        var parsedDateTime = DateTime.ParseExact(property.Value, format, CultureInfo.InvariantCulture);
                        _logger.LogTrace($"{parsedDateTime} was found in XMP: '{xmpString}:{property.Value}'.");
                        ret = parsedDateTime;
                    }
                }
            }
        }

        return ret;
    }

    private DateTime DateTimeFromQuicktime(IReadOnlyList<MetadataExtractor.Directory> directories)
    {
        var ret = DateTime.MinValue;
        var quickTimeMovieHeaderDirectory = directories.OfType<QuickTimeMovieHeaderDirectory>().FirstOrDefault();
        if (quickTimeMovieHeaderDirectory != null)
        {
            if (DirectoryExtensions.TryGetDateTime(quickTimeMovieHeaderDirectory, QuickTimeMovieHeaderDirectory.TagCreated, out var tagCreated))
            {
                ret = tagCreated;
            }
        }

        return ret;
    }

    private DateTime DateTimeFromIptc(IReadOnlyList<MetadataExtractor.Directory> directories)
    {
        var ret = DateTime.MinValue;
        var iptcDirectory = directories.OfType<IptcDirectory>().FirstOrDefault();
        if (iptcDirectory != null)
        {
            if (DirectoryExtensions.TryGetDateTime(iptcDirectory, IptcDirectory.TagDateCreated, out var tagDateCreated))
            {
                ret = tagDateCreated;
            }
            else if (DirectoryExtensions.TryGetDateTime(iptcDirectory, IptcDirectory.TagTimeCreated, out var tagTimeCreated))
            {
                ret = tagTimeCreated;
            }
            else if (DirectoryExtensions.TryGetDateTime(iptcDirectory, IptcDirectory.TagDigitalDateCreated, out var tagDigitalDateCreated))
            {
                ret = tagDigitalDateCreated;
            }
            else if (DirectoryExtensions.TryGetDateTime(iptcDirectory, IptcDirectory.TagDigitalTimeCreated, out var tagDigitalTimeCreated))
            {
                ret = tagDigitalTimeCreated;
            }
        }

        return ret;
    }

    private DateTime DateTimeFromExifSubIf(IReadOnlyList<MetadataExtractor.Directory> directories)
    {
        var ret = DateTime.MinValue;
        var subIfdDirectory = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();
        if (subIfdDirectory != null)
        {
            if (DirectoryExtensions.TryGetDateTime(subIfdDirectory, ExifDirectoryBase.TagDateTimeOriginal, out var tagDateTimeOriginal))
            {
                ret = tagDateTimeOriginal;
            }
        }

        return ret;
    }

    private DateTime DateTimeFromExif(IReadOnlyList<MetadataExtractor.Directory> directories)
    {
        var ret = DateTime.MinValue;
        var exifDirectoryBase = directories.OfType<ExifDirectoryBase>().FirstOrDefault();
        if (exifDirectoryBase != null)
        {
            if (DirectoryExtensions.TryGetDateTime(exifDirectoryBase, ExifDirectoryBase.TagDateTime, out var tagDateTime))
            {
                ret = tagDateTime;
            }
            else if (DirectoryExtensions.TryGetDateTime(exifDirectoryBase, ExifDirectoryBase.TagDateTimeDigitized, out var tagDateTimeDigitized))
            {
                ret = tagDateTimeDigitized;
            }
            else if (DirectoryExtensions.TryGetDateTime(exifDirectoryBase, ExifDirectoryBase.TagDateTimeOriginal, out var tagDateTimeOriginal))
            {
                ret = tagDateTimeOriginal;
            }
        }

        return ret;
    }


}
