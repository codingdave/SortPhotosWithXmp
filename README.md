# SortPhotosWithXmpByExifDate
Sort your Photos and Videos by exif time. If you have .xmp files, this tool will place them accordingly, preserving the Image<->Xmp locality.

## TODO-Application wise
* Add Tests
* Implement the other commands-stubs
* Add command to print files whose filenames contain a timestamp and for which the xmp time information differs. Also allow to specify the format for the time, like yyyy/MM/dd
* Allow to specify the date format that describes how the files are structured yyyy/MM/dd
* move videos out of image directory? Currently it will be sorted like the images
* 2015/01/06/dsc_1491.JPG and 2015/06/01/dsc_1491.JPG are the same image but exif got broken: 
  * First scan all images with ImageMagick to get image hash
  * with a HashSet look up for duplicates?
  * Maybe store the HashSet database on disk to speed up further iterations?
* 0/0 duplicates found. Does it even work?

## TODO-Compiler wise
* The logging message template should not vary between calls to 'LoggerExtensions.LogInformation(ILogger, string?, params object?[])' 

## TODO-Action wise
* run for Pictures, sorted-Pictures, and all other unprocessed sources
