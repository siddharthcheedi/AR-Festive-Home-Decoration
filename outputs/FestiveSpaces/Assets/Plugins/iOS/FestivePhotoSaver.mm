#import <UIKit/UIKit.h>

extern "C" void SaveFestiveSpacesImage(const char *imagePath)
{
    if (imagePath == nullptr) return;
    NSString *path = [NSString stringWithUTF8String:imagePath];
    UIImage *image = [UIImage imageWithContentsOfFile:path];
    if (image != nil)
        UIImageWriteToSavedPhotosAlbum(image, nil, nil, nil);
}
