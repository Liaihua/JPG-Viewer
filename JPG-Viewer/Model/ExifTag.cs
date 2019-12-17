namespace JPG_Viewer
{
    class ExifTag
    {
        public ushort Tag { get; }
        public string TagDescription { get; }
        public string TagValue { get; set; }

        public ExifTag(ushort tag, string tagDescription, string tagValue)
        {
            Tag = tag;
            TagValue = tagValue;
            TagDescription = tagDescription;
        }
    }
}
