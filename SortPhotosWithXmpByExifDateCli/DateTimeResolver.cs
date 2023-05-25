using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.Iptc;
using MetadataExtractor.Formats.QuickTime;
using MetadataExtractor.Formats.Xmp;
using DirectoryExtensions = MetadataExtractor.DirectoryExtensions;
using System.Globalization;
using Microsoft.Extensions.Logging;
using MetadataExtractor;
using System.Net;
using MetadataExtractor.Formats.FileSystem;
using System.Diagnostics;

namespace SortPhotosWithXmpByExifDateCli;

public class DateTimeResolver
{
    private readonly ILogger _logger;

    public DateTimeResolver(ILogger staticLogger)
    {
        _logger = staticLogger;
    }

    public DateTime? GetDateTimeFromImage(ILogger logger, IReadOnlyList<MetadataExtractor.Directory> directories)
    {
        // Exif IFD0 - Date/Time = 2023:01:18 10:54:28
        // Exif SubIFD - Date/Time Digitized = 2023:01:18 10:17:32
        // Exif SubIFD - Date/Time Original = 2023:01:18 10:17:32
        // IPTC - Date Created = 2023:01:18
        // IPTC - Digital Date Created = 2023:01:18
        // IPTC - Digital Time Created = 10:17:32+0100
        // IPTC - Time Created = 10:17:32+0100

        DateTime? ret = null;

        Func<ILogger, IReadOnlyList<MetadataExtractor.Directory>, DateTime?>[] functions = new[]
        {
            DateTimeFromExif,
            DateTimeFromIptc,
            DateTimeFromQuicktime,
            DateTimeFromXmp
        };

        foreach (var f in functions)
        {
            if (ret == null)
            {
                ret = f(logger, directories);
            }
        }

        return ret;
    }

    private DateTime? DateTimeFromXmp(ILogger logger, IReadOnlyList<MetadataExtractor.Directory> directories)
    {
        DateTime? ret = null;
        // my own modifications are like: exif:DateTimeOriginal: 2015-12-06T00:00:01+00:00
        var format = "yyyy-MM-ddTHH:mm:ss+00:00";
        var xmpString = "exif:DateTimeOriginal";

        var typedDirectories = directories.OfType<XmpDirectory>();
        if (typedDirectories != null)
        {
            foreach (var directory in typedDirectories)
            {
                if (directory.XmpMeta != null)
                {
                    foreach (var property in directory.XmpMeta.Properties)
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
        }

        return ret;
    }

    private DateTime? DateTimeFromQuicktime(ILogger logger, IReadOnlyList<MetadataExtractor.Directory> directories)
    {
        var tags = new int[]
        {
            QuickTimeMovieHeaderDirectory.TagCreated,
        };
        return DateTimeFrom<QuickTimeMovieHeaderDirectory>(logger, directories, tags);
    }

    private static DateTime? DateTimeFromIptc(ILogger logger, IReadOnlyList<MetadataExtractor.Directory> directories)
    {
        DateTime? ret = null;

        var exifDates = new Dictionary<int, DateTime>();
        var tags = new int[] { 1, 2 };

        var typedDirectories = directories.OfType<IptcDirectory>();
        if (typedDirectories != null)
        {
            foreach (var directory in typedDirectories)
            {
                DateTimeOffset? date = directory.GetDateCreated();
                if (date is DateTimeOffset dto)
                {
                    exifDates.Add(1, dto.DateTime);
                }

                date = directory.GetDigitalDateCreated();
                if (date is DateTimeOffset dto2)
                {
                    exifDates.Add(2, dto2.DateTime);
                }
            }
        }

        foreach (var tag in tags)
        {
            var isFound = exifDates.TryGetValue(tag, out var dateTime);
            if (isFound)
            {
                ret = dateTime;
                break;
            }
        }

        return ret;
    }

    private static DateTime? DateTimeFromExif(ILogger logger, IReadOnlyList<MetadataExtractor.Directory> directories)
    {
        var tags = new int[]
        {
            ExifDirectoryBase.TagDateTimeOriginal,
            ExifDirectoryBase.TagDateTime,
            ExifDirectoryBase.TagDateTimeDigitized,
        };
        return DateTimeFrom<ExifDirectoryBase>(logger, directories, tags);
    }

    private static DateTime? DateTimeFrom<T>(ILogger logger, IReadOnlyList<MetadataExtractor.Directory> directories, int[] tags) where T : MetadataExtractor.Directory
    {
        DateTime? ret = null;

        var exifDates = new List<(int tag, DateTime dateTime)>();

        var typedDirectories = directories.OfType<T>().ToList();
        if (typedDirectories != null)
        {
            foreach (var directory in typedDirectories)
            {
                foreach (var tag in tags)
                {
                    if (DirectoryExtensions.TryGetDateTime(directory, tag, out var dateTime))
                    {
                        exifDates.Add((tag, dateTime));
                    }
                }
            }
        }

        foreach (var tag in tags)
        {
            var found = exifDates.Where(t => t.tag == tag).ToList();
            if (found.Count > 1)
            {
                var foundDates = found.Select(d => d.dateTime).Distinct().ToList();
                Debug.Assert(foundDates.Count > 0);

                if (foundDates.Count == 1)
                {
                    logger.LogDebug("Several identical tags have been found");
                }
                else
                {
                    var message = $"Found several different entries for tag {tag}:" + Environment.NewLine;

                    foreach (var t in foundDates)
                    {
                        message += t.ToString(CultureInfo.InvariantCulture) + Environment.NewLine;
                    }

                    logger.LogError(message);
                }

                ret = foundDates.First();
                break;
            }
        }

        return ret;
    }
}
