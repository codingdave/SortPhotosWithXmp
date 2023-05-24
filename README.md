# SortPhotosWithXmpByExifDate
Sort your Photos and Videos by exif time. If you have .xmp files, this tool will place them accordingly, preserving the Image<->Xmp locality.

## TODO
* ErrorFiles: Better copy with subdirectories. Or what if there is a collision?
* Tests
* Add all the other commands
* How to handle collisions?
* The logging message template should not vary between calls to 'LoggerExtensions.LogInformation(ILogger, string?, params object?[])' 
* If file already exists and is the same, ignore it
* run for Pictures and sorted-Pictures
* FileAlreadyExistsError
* move videos out of image directory
* TODO: Add command to print files whose filenames contain a timestamp and for which the xmp time information differs. Also allow to specify the format for the time, like yyyy/MM/dd
* TODO Allow to specify the format for the time, like yyyy/MM/dd