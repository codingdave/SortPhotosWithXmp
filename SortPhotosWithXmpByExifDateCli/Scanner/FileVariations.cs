using System.IO.Enumeration;

namespace SortPhotosWithXmpByExifDateCli.Scanner;

public record struct FileVariations(string? Filename, List<string> SidecarFiles);