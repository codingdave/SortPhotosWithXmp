# SortPhotosWithXmpByExifDate

Sort your Photos and Videos by exif time. If you have .xmp files, this tool will place them accordingly, preserving the Image<->Xmp relation.

For all desctructive (possibly dangerous) commands the safe (dry run) mode is the default. Performing the operation (move, copy) needs to be requested.

The default operation is copy, but move can be enforced.

Challenges it can deal with:

* No timestamp found: We try different fields of metadata (exif, iptc, ...). Still a datetime might be missing. Images for which we do not find the datetime will be placed in NoTimeFound/image/image.jpg
* In the future we can try to extract the date from the filename. Maybe with a pattern.
* duplicates: This might happen if you have the same image (so also same metadata) at 2 different directories, as they will be copied next to each other. Since this might be a duplicate that you want to resolve first, they are placed in FileAlreadyExistsError/image/image.jpg and FileAlreadyExistsError/image/image_1.jpg, FileAlreadyExistsError/image/image_2.jpg ...
* Metadata issues: If reading the metadata fails or there is an unknown medatata entry the image is placed at MetaDataError/image/image.jpg

## TODO-Application wise

* Add Tests
  * Fix finding xmps
* Implement the other commands-stubs
* Add command to print files whose filenames contain a timestamp and for which the xmp time information differs. Also allow to specify the format for the time, like yyyy/MM/dd
* Allow to specify the date format that describes how the files are structured yyyy/MM/dd
* move videos out of image directory? Currently it will be sorted like the images
* Duplicates: 2015/01/06/dsc_1491.JPG and 2015/06/01/dsc_1491.JPG are the same image but exif got broken:
  * First scan all images with ImageMagick to get image hash
  * with a HashSet look up for duplicates?
  * Maybe store the HashSet database on disk to speed up further iterations?

* 0/0 duplicates found. Does it even work?

* Duplicate command needs to move duplicates into directory structure if force is given. Currently it only logs to console that it calculates hashes.

## Issues

* Hashing dies, thats why we save the hashes