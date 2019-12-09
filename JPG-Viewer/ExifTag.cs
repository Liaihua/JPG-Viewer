using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPG_Viewer
{
    class ExifTag
    {
        ushort Tag { get; set; }
        ushort Type { get; set; }
        ushort Length { get; set; }
        byte[] Content { get; set; }
        string TagDescription { get; }
        
        public ExifTag(ushort tag, ushort type, ushort length, string tagDescription)
        {
            Tag = tag;
            Type = type;
            Length = length;
            TagDescription = tagDescription;
        }
    }
}
