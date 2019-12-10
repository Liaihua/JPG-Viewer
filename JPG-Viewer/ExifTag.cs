using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPG_Viewer
{
    class ExifTag
    {
        public string TagDescription { get; }
        public string TagValue { get; set; }
        
        public ExifTag(string tagDescription, string tagValue)
        {
            TagValue = tagValue;
            TagDescription = tagDescription;
        }
    }
}
