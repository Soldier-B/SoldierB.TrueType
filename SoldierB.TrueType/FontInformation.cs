using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace SoldierB.TrueType
{
    public sealed class FontInformation
    {
        private const string HEADER_EXP = @"\x00\x01\x00\x00|OTTO";

        public enum FontStyle { Regular, Bold, Italic, BoldItalic }
        public enum Platform : ushort { Unicode, Macintosh, ISO, Windows, Custom }

        public string FamilyName { get; set; }
        public FontStyle Style { get; set; }

        private FontInformation() { }

        public override string ToString()
        {
            return $"{FamilyName} {Style}";
        }

        public static FontInformation Read(byte[] fontData)
        {
            FontInformation info = new FontInformation();
            Encoding UTF16BE = Encoding.BigEndianUnicode;
            Encoding ASCII = Encoding.ASCII;

            using (MemoryStream ms = new MemoryStream(fontData))
            using (FontReader reader = new FontReader(ms))
            {
                uint nameTableOffset = 0;
                uint headTableOffset = 0;
                uint os_2TableOffset = 0;

                // sanity check for fonts
                string sfnt = reader.ReadTag();
                if (!Regex.IsMatch(sfnt, HEADER_EXP))
                    throw new InvalidFontException();

                // read number of tables
                ushort tableCount = reader.ReadUInt16();

                // skip to beginning of table record
                reader.BaseStream.Seek(12, SeekOrigin.Begin);

                // read tables
                for (int i = 0; i < tableCount; i++)
                {
                    string tag = reader.ReadTag();
                    uint checksum = reader.ReadUInt32();
                    uint offset = reader.ReadUInt32();
                    uint length = reader.ReadUInt32();

                    if (tag == "name") nameTableOffset = offset;
                    if (tag == "head") headTableOffset = offset;
                    if (tag == "OS/2") os_2TableOffset = offset;
                }

                // make sure head and name tables exist
                if (headTableOffset == 0) throw new InvalidFontException("'head' table does not exist");
                if (nameTableOffset == 0) throw new InvalidFontException("'name' table does not exist");

                // find family name
                reader.BaseStream.Seek(nameTableOffset, SeekOrigin.Begin);
                ushort nameFormat = reader.ReadUInt16();
                ushort nameCount = reader.ReadUInt16();
                long nameOffset = reader.BaseStream.Position - 4 + reader.ReadUInt16();

                for (int i = 0; i < nameCount; i++)
                {
                    ushort platformId = reader.ReadUInt16();
                    ushort encodingId = reader.ReadUInt16();
                    ushort languageId = reader.ReadUInt16();
                    ushort nameId = reader.ReadUInt16();
                    ushort length = reader.ReadUInt16();
                    ushort offset = reader.ReadUInt16();

                    if (nameId == 1)
                    {
                        long position = reader.BaseStream.Position;
                        bool isWindows = platformId == (ushort)Platform.Windows;

                        reader.BaseStream.Seek(nameOffset + offset, SeekOrigin.Begin);

                        if (isWindows || String.IsNullOrWhiteSpace(info.FamilyName))
                            info.FamilyName = (isWindows ? UTF16BE : ASCII).GetString(reader.ReadBytes(length));

                        reader.BaseStream.Seek(position, SeekOrigin.Begin);
                    }
                }

                // read macStyle
                reader.BaseStream.Seek(headTableOffset + 44, SeekOrigin.Begin);
                info.Style = ParseMacStyle(reader.ReadUInt16() & 0x3);
                //switch (reader.ReadUInt16() & 3)
                //{
                //    case 0:
                //        info.Style = FontStyle.Regular;
                //        break;
                //    case 1:
                //        info.Style = FontStyle.Bold;
                //        break;
                //    case 2:
                //        info.Style = FontStyle.Italic;
                //        break;
                //    case 3:
                //        info.Style = FontStyle.BoldItalic;
                //        break;
                //}

                // read fsSelection
                if (os_2TableOffset > 0)
                {
                    reader.BaseStream.Seek(os_2TableOffset + 62, SeekOrigin.Begin);
                    info.Style = ParseFsSelection(reader.ReadUInt16() & 0x61);
                    //switch (reader.ReadUInt16() & 97)
                    //{
                    //    case 1:
                    //        info.Style = FontStyle.Italic;
                    //        break;
                    //    case 32:
                    //        info.Style = FontStyle.Bold;
                    //        break;
                    //    case 33:
                    //        info.Style = FontStyle.BoldItalic;
                    //        break;
                    //    default:
                    //        info.Style = FontStyle.Regular;
                    //        break;
                    //}
                }
            }

            return info;
        }

        public static FontInformation Read(string fontFile)
        {
            return Read(File.ReadAllBytes(fontFile));
        }

        private static FontStyle ParseMacStyle(int macStyle)
        {
            if (macStyle == 1) return FontStyle.Bold;
            if (macStyle == 2) return FontStyle.Italic;
            if (macStyle == 3) return FontStyle.BoldItalic;
            return FontStyle.Regular;
        }

        private static FontStyle ParseFsSelection(int fsSelection)
        {
            if (fsSelection == 1) return FontStyle.Italic;
            if (fsSelection == 32) return FontStyle.Bold;
            if (fsSelection == 33) return FontStyle.BoldItalic;
            return FontStyle.Regular;
        }

    }
}
