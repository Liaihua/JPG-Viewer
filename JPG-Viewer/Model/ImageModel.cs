using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPG_Viewer
{
    public class ImageModel
    {
        List<ExifTag> availableTags;
        string Name { get; }
        string Path { get; }
        DateTime WhenShot { get; }
        string DeviceName { get; }
        string Artist { get; }
        public ImageModel(string pathname)
        {
            FileInfo info = new FileInfo(pathname);
            Name = info.Name;
            Path = info.DirectoryName;
            availableTags = new EXIFViewer().ReadExifInFile(pathname);
            WhenShot = DateTime.Parse(availableTags.First(tag => tag.TagDescription == "DateTime").TagValue);
            DeviceName = availableTags.First(tag => tag.TagDescription == "Model").TagValue;
            Artist = availableTags.First(tag => tag.TagDescription == "Artist").TagValue;
        }
    }
}
