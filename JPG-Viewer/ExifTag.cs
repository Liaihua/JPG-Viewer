using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPG_Viewer
{
    [Serializable]
    class ExifTag
    {
        ushort Tag { get; set; }
        ushort Type { get; set; }
        string TagDescription { get; }
        
        public ExifTag(ushort tag, ushort type, string tagDescription)
        {
            Tag = tag;
            Type = type;
            TagDescription = tagDescription;
        }
    }
}
