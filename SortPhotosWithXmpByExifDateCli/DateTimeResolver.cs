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
using System.Diagnostics.CodeAnalysis;
using SortPhotosWithXmpByExifDateCli.Statistics;
using SortPhotosWithXmpByExifDateCli.ErrorCollection;

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
        // var format = "yyyy-MM-ddTHH:mm:ss+00:00";
        var format = "yyyy-MM-dd'T'HH:mm:sszzz";
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

                            try
                            {
                                var parsedDateTime = DateTime.ParseExact(property.Value, format, CultureInfo.InvariantCulture);
                                ret = parsedDateTime;
                            }
                            catch (Exception e)
                            {
                                _logger.LogExceptionWarning($"using format {format}:", e);

                                try
                                {
                                    var parsedDateTime = DateTime.Parse(property.Value);
                                    ret = parsedDateTime;
                                }
                                catch (Exception e2)
                                {
                                    _logger.LogExceptionWarning("DateTime.Parse failed: {exception}", e2);
                                }
                            }
                            finally
                            {
                                if (ret != null)
                                {
                                    _logger.LogTrace($"{ret} was found in XMP: '{xmpString}:{property.Value}' with {format}");
                                }
                                else
                                {
                                    _logger.LogWarning($"DateTime.Parse failed in XMP: '{xmpString}:{property.Value}' with {format}");
                                }
                            }
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
            if (found.Any())
            {
                // inform if more than 1 match exists
                if (found.Count > 1)
                {
                    var distinctDates = found.Select(d => d.dateTime).Distinct().ToList();
                    Debug.Assert(distinctDates.Count > 0);

                    if (distinctDates.Count == 1)
                    {
                        logger.LogTrace("Several identical tags have been found");
                    }
                    else
                    {
                        var messages = new List<string>() { $"Found several different entries for tag {tag}:" };

                        foreach (var t in distinctDates)
                        {
                            messages.Add(t.ToString(CultureInfo.InvariantCulture));
                        }

                        logger.LogError(string.Join(Environment.NewLine, messages));
                    }
                }

                ret = found.First().dateTime;
                break;
            }
        }

        return ret;
    }
}
