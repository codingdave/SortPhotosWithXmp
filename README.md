# SortPhotosWithXmpByExifDate
Sort your Photos and Videos by exif time. If you have .xmp files, this tool will place them accordingly, preserving the Image<->Xmp locality.

## TODO-Code wise
* Tests
* Add all the other commands
* Add command to print files whose filenames contain a timestamp and for which the xmp time information differs. Also allow to specify the format for the time, like yyyy/MM/dd
* Allow to specify the format for the time, like yyyy/MM/dd
* The logging message template should not vary between calls to 'LoggerExtensions.LogInformation(ILogger, string?, params object?[])' 
* duplicate detection: If file already exists and is the same, ignore it
* move videos out of image directory

## TODO-Action wise
* run for Pictures, sorted-Pictures, and all other unprocessed sources
