using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace JPG_Viewer
{
    // У меня есть 2 основных варианта по созданию приложения:
    //  1. Продолжать разрабатывать функционал для чтения JPEG, затем п.2
    //  2. Временно перейти к попытке связать между собой БД и приложение (тут тоже есть свои проблемы), затем п.1

    // Есть также вариант по созданию функции, которая могла бы искать инфу по определенному тегу
    // (так то у меня есть ф-ии ReadExifTagsInStream и ReadExifInFile, но тут-то все выглядит довольно громоздко: 
    // они отвечают за выдачу **всех** тегов, а не отдельного)
    class JPEGWalker
    {
        bool? LittleEndian;
        Dictionary<int, string> LoadedExifTags;

        public JPEGWalker()
        {
            LoadedExifTags = GetExifTagsFromDictionary();
        }
        public List<string> FindJPEGInDirectory(string dir)
        {
            if (string.IsNullOrWhiteSpace(dir))
                return null;
            List<string> JPEGPaths = new List<string>();
            DirectoryInfo directory = new DirectoryInfo(dir);
            try
            {
                foreach (FileInfo file in directory.GetFiles("*", SearchOption.AllDirectories))
                {
                    if (file.Extension.Contains("jpg") ||
                        file.Extension.Contains("jpeg"))
                        JPEGPaths.Add(file.FullName);
                }
            }
            catch (Exception ex) { System.Windows.MessageBox.Show(ex.Message); }
            return JPEGPaths;
        }

        public List<string> FindAllPathsInDirectory(string dir) // Данный метод не предусматривает вложенность каталогов
        {
            if (string.IsNullOrWhiteSpace(dir))
                return null;
            List<string> Dirs = new List<string>();
            DirectoryInfo directory = new DirectoryInfo(dir);
            try
            {
                foreach (var d in directory.GetDirectories("*", SearchOption.AllDirectories))
                {
                    Dirs.Add(d.Parent + "/" + d.Name);
                }
            }
            catch (Exception ex) { System.Windows.MessageBox.Show(ex.Message); }
            return Dirs;
        }

        public float GetFileLength(string path)
        {
            return new FileInfo(path).Length / 1024;
        }

        private bool? CheckEndiannessInStream(BinaryReader reader) // true - для прямого порядка (LE), false - для обратного (BE)
        {
            reader.BaseStream.Seek(8, SeekOrigin.Current);
            ushort endianTag = reader.ReadUInt16();
            ushort Tag42 = reader.ReadUInt16();
            reader.BaseStream.Seek(-4, SeekOrigin.Current);
            if (endianTag == 0x4949 && Tag42 == 0x002A)     // "II"
                return true;
            else if (endianTag == 0x4D4D && Tag42 == 0x2A00) // "MM"
                return false;
            else
                return null;
        }

        // Я решил возвращать не просто строку, но пару ключ-значение. Я думаю, это пригодится, когда я изменю дизайн
        // Exif-окна (вместо textbox - listview с двумя столбиками)
        public ExifTag ReadTag(BinaryReader reader, long tiffHeader, long dirEntryTag)
        {
            reader.BaseStream.Seek(dirEntryTag, SeekOrigin.Begin);
            ushort tag = EndiannessIO.ReadUInt16(reader.ReadUInt16(), LittleEndian.Value);
            ushort type = EndiannessIO.ReadUInt16(reader.ReadUInt16(), LittleEndian.Value);
            uint count = EndiannessIO.ReadUInt32(reader.ReadUInt32(), LittleEndian.Value);
            byte[] valueOrOffset = BitConverter.GetBytes(EndiannessIO.ReadUInt32(reader.ReadUInt32(), !LittleEndian.Value));
            ExifTag currentExifTag = new ExifTag(tag,
                    LoadedExifTags.First(item => item.Key == tag).Value, "");

            switch (type)
            {
                case (int)IFDTypeEnum.Byte:
                case (int)IFDTypeEnum.Undefined:
                    if (count == 1)
                        currentExifTag.TagValue = Encoding.ASCII.GetString(reader.ReadBytes(2));

                    reader.BaseStream.Seek(
                    (count > 4) ? EndiannessIO.ReadUInt32(
                        BitConverter.ToUInt32(valueOrOffset, 0), false) + tiffHeader :
                        dirEntryTag + 8,
                    SeekOrigin.Begin
                    );
                    currentExifTag.TagValue = Encoding.ASCII.GetString(
                        reader.ReadBytes((int)count)
                        );
                    break;
                case (int)IFDTypeEnum.ASCII: // Полностью рабочий кейс
                    reader.BaseStream.Seek(
                        (count > 4) ? EndiannessIO.ReadUInt32(
                            BitConverter.ToUInt32(valueOrOffset, 0), false) + tiffHeader :
                            dirEntryTag + 8,
                        SeekOrigin.Begin
                        );
                    currentExifTag.TagValue = Encoding.ASCII.GetString(
                        reader.ReadBytes((int)count)
                        );
                    break;
                case (int)IFDTypeEnum.Short:

                    reader.BaseStream.Seek(
                        (count > 2) ? EndiannessIO.ReadUInt32(
                            BitConverter.ToUInt32(valueOrOffset, 0), false) + tiffHeader :
                            dirEntryTag + 8,
                        SeekOrigin.Begin
                        );
                    for (int i = 0; i < count; i++)
                        currentExifTag.TagValue +=
                            EndiannessIO.ReadUInt16(reader.ReadUInt16(), LittleEndian.Value);

                    break;
                case (int)IFDTypeEnum.Long:

                    reader.BaseStream.Seek(
                        (count > 1) ? EndiannessIO.ReadUInt32(
                            BitConverter.ToUInt32(valueOrOffset, 0), false) + tiffHeader :
                            dirEntryTag + 8,
                        SeekOrigin.Begin
                        );
                    for (int i = 0; i < count; i++)
                        currentExifTag.TagValue +=
                            EndiannessIO.ReadUInt32(reader.ReadUInt32(), LittleEndian.Value).ToString();

                    break;
                case (int)IFDTypeEnum.Rational: // TODO
                    
                        reader.BaseStream.Seek(
                            
                            EndiannessIO.ReadUInt32(
                                BitConverter.ToUInt32(valueOrOffset, 0), false) + tiffHeader,
                            SeekOrigin.Begin);
                        uint numerator = EndiannessIO.ReadUInt32(reader.ReadUInt32(), LittleEndian.Value);
                        uint denominator = EndiannessIO.ReadUInt32(reader.ReadUInt32(), LittleEndian.Value);
                        currentExifTag.TagValue = $"{numerator / (denominator * 1.0)} [{numerator}/{denominator}]";
                    
                    break;
                case (int)IFDTypeEnum.SLong: // TODO
                    if (count > 1)
                    {
                        reader.BaseStream.Seek(
                            EndiannessIO.ReadInt32(
                                BitConverter.ToInt32(valueOrOffset, 0), false) + tiffHeader,
                            SeekOrigin.Begin);
                        currentExifTag.TagValue =
                            EndiannessIO.ReadInt32(reader.ReadInt32(), LittleEndian.Value).ToString();
                    }
                    else
                        currentExifTag.TagValue =
                            EndiannessIO.ReadInt32(
                                BitConverter.ToInt32(valueOrOffset, 0), LittleEndian.Value).ToString();
                    break;
                case (int)IFDTypeEnum.SRational: // TODO
                    if (count > 1) { }
                    else
                    {
                        reader.BaseStream.Seek(
                            EndiannessIO.ReadInt32(
                                BitConverter.ToInt32(valueOrOffset, 0), false) + tiffHeader,
                            SeekOrigin.Begin);
                        int numerator = EndiannessIO.ReadInt32(reader.ReadInt32(), LittleEndian.Value);
                        int denominator = EndiannessIO.ReadInt32(reader.ReadInt32(), LittleEndian.Value);
                        currentExifTag.TagValue = $"{numerator / (denominator * 1.0)} [{numerator}/{denominator}]";
                    }
                    break;
            }
            reader.BaseStream.Seek(tiffHeader, SeekOrigin.Begin);
            return currentExifTag;
        }

        private List<ExifTag> ReadTagsInFileStream(BinaryReader reader) // Поиск тегов в файле
        {
            List<ExifTag> foundTags = new List<ExifTag>();
            LittleEndian = CheckEndiannessInStream(reader);
            long tiffPosition = reader.BaseStream.Position;
            reader.BaseStream.Seek(4, SeekOrigin.Current);
            uint firstIFDOffset = reader.ReadUInt32();
            ushort entries = reader.ReadUInt16();
            if (LittleEndian.HasValue)
            {
                if (!LittleEndian.Value)
                {
                    firstIFDOffset = EndiannessIO.ReadUInt32(firstIFDOffset, LittleEndian.Value);
                    entries = EndiannessIO.ReadUInt16(entries, LittleEndian.Value);
                }
            }
            else
            {
                foundTags.Add(new ExifTag(0, null, "Неверные данные TIFF (Отсутствует порядок байтов)"));
                return foundTags;
            }
            if (firstIFDOffset < 0x00000008)
            {
                foundTags.Add(new ExifTag(0, null, "Неверные данные TIFF (Неправильное смещение IFD)"));
                return foundTags;
            }

            long dirPosition = tiffPosition + firstIFDOffset;

            // вначале читаем 0 IFD, в котором лежат данные TIFF
            for (int i = 0; i < entries; i++)
            {
                foundTags.Add(ReadTag(reader, tiffPosition, 2 + dirPosition + i * 12)); // набросок
            }

            // Указатель на Exif IFD
            ExifTag exifPointerTag = foundTags.Find((tag) => tag.Tag == 0x8769);
            if (exifPointerTag != null)
            {
                // Я думаю, что здесь и в условии с GPS надо повторить все колдунство сверху в пределах функции
                long exifPointerPosition = tiffPosition + long.Parse(exifPointerTag.TagValue);
                reader.BaseStream.Seek(exifPointerPosition, SeekOrigin.Begin);
                entries = EndiannessIO.ReadUInt16(reader.ReadUInt16(), LittleEndian.Value);
                for (int i = 0; i < entries; i++)
                {
                    foundTags.Add(ReadTag(reader, tiffPosition, 2 + exifPointerPosition + i * 12));
                }
            }

            // Указатель на GPS IFD
            ExifTag gpsPointerTag = foundTags.Find((tag) => tag.Tag == 0x8825);
            if (gpsPointerTag != null)
            {
                long gpsPointerPosition = tiffPosition + long.Parse(gpsPointerTag.TagValue);
                reader.BaseStream.Seek(gpsPointerPosition, SeekOrigin.Begin);
                entries = EndiannessIO.ReadUInt16(reader.ReadUInt16(), LittleEndian.Value);
                for(int i = 0; i < entries; i++)
                {
                    foundTags.Add(ReadTag(reader, tiffPosition, 2 + gpsPointerPosition + i * 12));
                }
            }

            return foundTags;
        }

        public string ReadExifInFile(string path) // Поиск APP1 в файле
        {
            // У нас есть вариант, позволяющий не просматривать каждые два байта, а "прыгать" по основным тегам,
            // что по идее позволит тратить меньше времени на чтение файла

            string kindaExif = "";

            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                using (BinaryReader reader = new BinaryReader(fs))
                {
                    if (reader.ReadByte() != 0xFF || reader.ReadByte() != 0xD8)
                        return "Испорченный файл";

                    long offset = 2;
                    byte mark = 0;
                    while (offset < fs.Length - 1)
                    {
                        fs.Seek(offset, SeekOrigin.Begin);
                        mark = reader.ReadByte();
                        if (mark != 0xFF)
                            return "Нет данных";
                        mark = reader.ReadByte();

                        if (mark == 0xE1)
                        {
                            ReadTagsInFileStream(reader).ForEach(tag =>
                            {
                                kindaExif += tag.TagDescription + " : " + tag.TagValue + '\n';
                            });
                            break;
                        }
                        else
                        {
                            if (mark == 0xDB || // DQT
                                mark == 0xC0 || // SOF
                                mark == 0xC4 || // DHT
                                mark == 0xDA)    // SOS
                                kindaExif = "Нет данных";
                            offset += 2 + EndiannessIO.ReadUInt16(reader.ReadUInt16(), false);
                        }
                    }

                    if (kindaExif == "")
                        kindaExif = "Нет данных";
                    if (fs.Position == fs.Length - 1) // Если тег отсутствует
                        kindaExif = "Информация недоступна";
                }
            }
            return kindaExif;
        }
        public Dictionary<int, string> GetExifTagsFromDictionary()
        {
            Dictionary<int, string> exifTags = new Dictionary<int, string>();
            using (FileStream fs = new FileStream("exif_tags.json", FileMode.Open, FileAccess.Read))
            {
                System.Runtime.Serialization.Json.DataContractJsonSerializer jsonSerializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(typeof(Dictionary<string, string>));
                foreach (var tag in (Dictionary<string, string>)jsonSerializer.ReadObject(fs))
                    exifTags.Add(int.Parse(tag.Key, System.Globalization.NumberStyles.HexNumber), tag.Value);
            }
            if (exifTags.Count < 1)
                return null;
            return exifTags;
        }
    }
}
