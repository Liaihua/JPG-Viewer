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
        long Size { get; }
        DateTime WhenShot { get; }
        string DeviceName { get; }
        string Artist { get; }
        public ImageModel(string pathname)
        {
            Name = pathname;
            Size = new FileInfo(pathname).Length / 1024;
            //Может, пусть этим занимается DirectoryViewModel?
            //availableTags = new EXIFViewer().ReadExifInFile(pathname);
            //WhenShot = DateTime.Parse(availableTags.First(tag => tag.TagDescription == "DateTime").TagValue);
            //DeviceName = availableTags.First(tag => tag.TagDescription == "Model").TagValue;
            //Artist = availableTags.First(tag => tag.TagDescription == "Artist").TagValue;
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
