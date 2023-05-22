# SortPhotosWithXmpByExifDate
Sort your Photos and Videos by exif time. If you have .xmp files, this tool will place them accordingly, preserving the Image<->Xmp locality.

## TODO
* ErrorFiles: Better copy with subdirectories. Or what if there is a collision?
* Tests
* Add all the other commands
* How to handle collisions?
* The logging message template should not vary between calls to 'LoggerExtensions.LogInformation(ILogger, string?, params object?[])' 