using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDDReader_0._4._0_renewed
{   //This class would be used for any additional functions, that are not listed in above
    class AnyAdditionalAndSupportiveFunctions
    {
        internal struct ByteConvert
        {
            public uint ToUInt(byte[] b)
            {
                int pos = 0;
                uint res = 0;
                foreach (byte by in b)
                {
                    res |= ((uint)by) << pos;
                    pos += 8;
                }
                return res;
            }

            public int ToInt(byte[] b)
            {
                int pos = 8 * (b.Length - 1);
                int res = 0;
                foreach (byte by in b)
                {
                    res |= by << pos;
                    pos -= 8;
                }
                return res;
            }

            public char ToChar(byte b)
            {
                return (char)b;
            }

            public char[] ToCharArray(byte[] b)
            {
                int pos = 0;
                char[] res = new char[b.Length];
                foreach (byte by in b)
                {
                    res[pos] = (char)by;
                    pos++;
                }
                return res;
            }

            public string ToString(byte[] b)
            {
                return new string(ToCharArray(b));
            }

            //returns a formated string of hex
            //Parameters:
            //{format} - {0 for no dashes, 1 for dashed text}
            public string ToStringOfHex(byte[] b, int format)
            {
                switch (format)
                {
                    case 0: return BitConverter.ToString(b).Replace("-", string.Empty);
                    case 1:
                        return BitConverter.ToString(b);

                    default:
                        return null;
                }
            }

            public string ToBitsString(byte b)
            {
                return Convert.ToString(b, 2).PadLeft(8, '0');
            }

            public byte GetByte(char c)
            {
                return (byte)c;
            }

            public byte GetByte(int i)
            {
                if (i < 256) return (byte)i;
                else return Byte.MinValue;
            }

            public byte[] GetBytes(int i)
            {
                if (i < 256) { byte[] temp = new byte[1]; temp[0] = (byte)i; return temp; }
                else
                {
                    byte[] intBytes = BitConverter.GetBytes(i);
                    if (BitConverter.IsLittleEndian)
                        Array.Reverse(intBytes);
                    return intBytes;
                }
            }

            public byte[] GetByte(char[] i)
            {
                int c = 0;
                byte[] t = new byte[i.Length];
                foreach (char ch in i)
                {
                    t[c] = Convert.ToByte(ch);
                    c++;
                }
                return t;
            }

            public byte[] GetByte(string i)
            {
                return System.Text.Encoding.Default.GetBytes(i);
            }

            public byte[] GetByteFromHexString(string i)
            {
                int NumberChars = i.Length;
                byte[] bytes = new byte[NumberChars / 2];
                for (int j = 0; j < NumberChars; j += 2)
                    bytes[j / 2] = Convert.ToByte(i.Substring(j, 2), 16);
                return bytes;
            }

            internal string ToStringOfHex(byte[] b)
            {
                return BitConverter.ToString(b);
            }
        }
    }
}
