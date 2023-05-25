# SortPhotosWithXmpByExifDate
Sort your Photos and Videos by exif time. If you have .xmp files, this tool will place them accordingly, preserving the Image<->Xmp locality.

## TODO-Application wise
* Tests
* Implement the other commands-stubs
 * Add command to print files whose filenames contain a timestamp and for which the xmp time information differs. Also allow to specify the format for the time, like yyyy/MM/dd
* Allow to specify the date format that describes how the files are structured yyyy/MM/dd
* move videos out of image directory? Currently it will be sorted like the images
* Currently we delete the "other file" for a collision
* WHY do we have a collision when the exif is different? 
* Move NoTimeError files into its own NoTimeError directory
* IDeployService that will move or copy depending on users choice

## TODO-Compiler wise
* The logging message template should not vary between calls to 'LoggerExtensions.LogInformation(ILogger, string?, params object?[])' 

## TODO-Action wise
* run for Pictures, sorted-Pictures, and all other unprocessed sources
