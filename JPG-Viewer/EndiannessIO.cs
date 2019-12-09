using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPG_Viewer
{
    static class EndiannessIO
    {
        public static uint ReadUInt32(uint param, bool littleEndian)
        {
            if (littleEndian)
                return param;
            else
                return BitConverter.ToUInt32(BitConverter.GetBytes(param).Reverse().ToArray(), 0);
        }
        public static ushort ReadUInt16(ushort param, bool littleEndian)
        {
            if (littleEndian)
                return param;
            else
                return BitConverter.ToUInt16(BitConverter.GetBytes(param).Reverse().ToArray(), 0);
        }
        public static int ReadInt32(int param, bool littleEndian)
        {
            if (littleEndian)
                return param;
            else
                return BitConverter.ToInt32(BitConverter.GetBytes(param).Reverse().ToArray(), 0);
        }
        public static short ReadInt16(short param, bool littleEndian)
        {
            if (littleEndian)
                return param;
            else
                return BitConverter.ToInt16(BitConverter.GetBytes(param).Reverse().ToArray(), 0);
        }
        public static float ReadSingle(float param, bool littleEndian)
        {
            if (littleEndian)
                return param;
            else
                return BitConverter.ToSingle(BitConverter.GetBytes(param).Reverse().ToArray(), 0);
        }
        public static double ReadDouble(double param, bool littleEndian)
        {
            if (littleEndian)
                return param;
            else
                return BitConverter.ToDouble(BitConverter.GetBytes(param).Reverse().ToArray(), 0);
        }
    }
}
