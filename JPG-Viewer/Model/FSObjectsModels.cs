using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPG_Viewer.Model
{
    public class ImageModel : AbstractFSObject
    {
        public long Size { get; }
        public string WhenShot { get; }
        public string DeviceName { get; }
        public string ImageResolution { get; }
        //public string Artist { get; }
        public ImageModel(string pathname)
        {
            Name = pathname;
            Size = new FileInfo(pathname).Length / 1024;
            //Может, пусть этим занимается DirectoryViewModel?
            try
            {
                List<ExifTag> availableTags = new EXIFViewer().ReadExifInFile(pathname);
                WhenShot = availableTags?.Find(tag => tag.TagDescription.Contains("DateTime"))?.TagValue;
                DeviceName = availableTags?.Find(tag => tag.TagDescription == "Model")?.TagValue;
                ImageResolution = $"{availableTags?.Find(tag => tag.TagDescription == "PixelXDimension")?.TagValue}" +
                                 $"x{availableTags?.Find(tag => tag.TagDescription == "PixelYDimension")?.TagValue}";
                //Artist = availableTags.First(tag => tag.TagDescription == "Artist").TagValue;
                WhenShot ??= "";
                DeviceName ??= "";
                ImageResolution ??= "";
            }
            catch { WhenShot = ""; DeviceName = ""; ImageResolution = ""; }
        }
    }

    public class DirectoryModel : AbstractFSObject
    {
        public DirectoryModel() : base()
        {

        }
        public DirectoryModel(string name)
        {
            Name = name;
        }
    }
}
