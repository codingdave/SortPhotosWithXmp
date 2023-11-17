# SortPhotosWithXmpByExifDate

Sort your Photos and Videos by exif time. If you have .xmp files, this tool will preserve the Image<->Xmp relation by keeping them adjoint.

For all desctructive (possibly dangerous) actions the safe (dry run) mode is the default. Performing the destructive operation (move, copy, delete) needs to be requested (--force).

The default operation is copy, but move can be used insted (--move).

## Operations

This application can perform an operation on a set of images with optional sidecar files

### Rearrange images by exif information

This operation will recursively browse through a directory and scan for images and movies. Their EXIF/IPTC information will be extracted and based on the date and time the image was shot a directory structure is created and the image with its optional sidecard files copied (default) or moved (--move) if enforced (--force). If you do not perform the operation it will notify you about the changes that will be performed when enforced.

Possible future updates: 

* Allow to specify the date format that describes how the files are structured like "yyyy/MM/dd".
* move videos out of image directory? Currently it will be sorted like the images
  
### Extract the date from a pattern in the filename

Not yet implemented

### Find images where the filenames date mismatches the metadata

Not yet implemented

## We can solve the following issues

### duplicates by filename

This might happen if you have the same image (so also same metadata) at 2 different directories, as they will be copied next to each other. Since this might be a duplicate that you want to resolve first, they are placed in FileAlreadyExistsError/image/image.jpg and FileAlreadyExistsError/image/image_1.jpg, FileAlreadyExistsError/image/image_2.jpg ...

### No timestamp found

We try different fields of EXIF/IPTC metadata. Still a datetime might be missing. Images for which we do not find the datetime will be copied in NoTimeFound/image/image.jpg for further investigation.

### Metadata issues

If reading the metadata fails or there is an unknown medatata entry the image is copied at MetaDataError/image/image.jpg for further investigation.

## TODO-Application wise

* Duplicates: 2015/01/06/dsc_1491.JPG and 2015/06/01/dsc_1491.JPG are the same image but exif got broken:
  * First scan all images with ImageMagick to get image hash
  * with a HashSet look up for duplicates?
  * Maybe store the HashSet database on disk to speed up further iterations?

* 0/0 duplicates found. Does it even work?

* Duplicate command needs to move duplicates into directory structure if force is given. Currently it only logs to console that it calculates hashes.

* MoveFileOperation just logs FileAlreadyExistsErrors. They are not handled well, yet. Not stored for further processing, logging

* Does Hashing still die? (thats why we were saving the hashes)