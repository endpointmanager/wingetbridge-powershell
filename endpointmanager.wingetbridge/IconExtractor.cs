using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace endpointmanager.wingetbridge
{
    internal static class IconExtractor
    {
        [UnmanagedFunctionPointer(CallingConvention.Winapi, SetLastError = true, CharSet = CharSet.Unicode)]
        [SuppressUnmanagedCodeSecurity]
        internal delegate bool ENUMRESNAMEPROC(IntPtr hModule, IntPtr lpszType, IntPtr lpszName, IntPtr lParam);
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr LoadLibraryEx(string lpFileName, IntPtr hFile, uint dwFlags);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool FreeLibrary(IntPtr hModule);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr FindResource(IntPtr hModule, IntPtr lpName, IntPtr lpType);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr LoadResource(IntPtr hModule, IntPtr hResInfo);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr LockResource(IntPtr hResData);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern uint SizeofResource(IntPtr hModule, IntPtr hResInfo);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [SuppressUnmanagedCodeSecurity]
        public static extern bool EnumResourceNames(IntPtr hModule, IntPtr lpszType, ENUMRESNAMEPROC lpEnumFunc, IntPtr lParam);


        private const uint LOAD_LIBRARY_AS_DATAFILE = 0x00000002;
        private readonly static IntPtr RT_ICON = (IntPtr)3;
        private readonly static IntPtr RT_GROUP_ICON = (IntPtr)14;

        public class IconResolution
        {
            public int Height { get; set; }
            public int Width { get; set; }
            public short Bits { get; set; }
            public string ImageFormat { get; set; }
        }

        private static int ReadLittleEndianInt32(byte[] Block, int Offset)
        {
            byte[] bytes = new byte[sizeof(int)];
            for (int i = 0; i < sizeof(int); i += 1)
            {
                bytes[sizeof(int) - 1 - i] = Block[Offset + i];
            }
            return BitConverter.ToInt32(bytes, 0);
        }

        public static List<IconResolution> GetIconInformation(Icon icoIcon)
        {
            List<IconResolution> iconResolutions = new List<IconResolution>();

            byte[] srcBuf = null;
            using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
            {
                icoIcon.Save(stream);
                srcBuf = stream.ToArray();
            }
            const int SizeICONDIR = 6; //ICON-Header
            const int SizeICONDIRENTRY = 16;
            int iCount = BitConverter.ToInt16(srcBuf, 4);

            for (int iIndex = 0; iIndex < iCount; iIndex++)
            {
                int iWidth = srcBuf[SizeICONDIR + SizeICONDIRENTRY * iIndex];
                int iHeight = srcBuf[SizeICONDIR + SizeICONDIRENTRY * iIndex + 1];
                short iBitCount = BitConverter.ToInt16(srcBuf, SizeICONDIR + SizeICONDIRENTRY * iIndex + 6);

                int iImageOffset = BitConverter.ToInt32(srcBuf, SizeICONDIR + SizeICONDIRENTRY * iIndex + 12);
                int iImageSize = BitConverter.ToInt32(srcBuf, SizeICONDIR + SizeICONDIRENTRY * iIndex + 8);
                string imageFormat = "";

                using (var sr = new MemoryStream(srcBuf))
                {
                    sr.Seek(iImageOffset, SeekOrigin.Begin);
                    var data = new byte[8];
                    sr.Read(data, 0, data.Length);
                    var pngHeader = new byte[8] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };

                    if (pngHeader.SequenceEqual(data))
                    {
                        imageFormat = "Png";
                        sr.Seek(iImageOffset, SeekOrigin.Begin);
                        var pngheader = new byte[24];
                        sr.Read(pngheader, 0, pngheader.Length);

                        iWidth = ReadLittleEndianInt32(pngheader, 16);
                        iHeight = ReadLittleEndianInt32(pngheader, 20);
                    }
                    else
                    {
                        int biSize = BitConverter.ToInt32(data, 0); //size of the structure (12->BITMAPCOREHEADER(DIB), 40->BITMAPINFOHEADE(Expanded DIB), 120->PBITMAPV4HEADER/BITMAPV5HEADER)
                        if ((biSize == 12) || (biSize == 40) || (biSize == 120))
                        {
                            imageFormat = "Bmp";
                            sr.Seek(iImageOffset, SeekOrigin.Begin);
                            var bitmapv5header = new byte[40];
                            sr.Read(bitmapv5header, 0, bitmapv5header.Length);

                            iWidth = BitConverter.ToInt32(bitmapv5header, 4); //bV5Width
                            iHeight = BitConverter.ToInt32(bitmapv5header, 8); //bV5Height
                            iHeight = iHeight / 2;
                            iBitCount = BitConverter.ToInt16(bitmapv5header, 14); //bV5BitCount
                        }
                    }
                }
                iconResolutions.Add(new IconResolution { Width = iWidth, Height = iHeight, Bits = iBitCount, ImageFormat = imageFormat });
            }
            return iconResolutions.OrderByDescending(o => o.Width).OrderByDescending(o => o.Bits).ToList();
        }

        public static Icon ExtractIconFromExecutable(string path)
        {
            IntPtr hModule = LoadLibraryEx(path, IntPtr.Zero, LOAD_LIBRARY_AS_DATAFILE);
            if (hModule != (IntPtr)0x0)
            {
                var tmpData = new List<byte[]>();

                ENUMRESNAMEPROC callback = (h, t, name, l) =>
                {
                    var dir = GetDataFromResource(hModule, RT_GROUP_ICON, name);

                // Calculate the size of an entire .icon file.

                int count = BitConverter.ToUInt16(dir, 4);  // GRPICONDIR.idCount
                int len = 6 + 16 * count;                   // sizeof(ICONDIR) + sizeof(ICONDIRENTRY) * count
                for (int i = 0; i < count; ++i)
                        len += BitConverter.ToInt32(dir, 6 + 14 * i + 8);   // GRPICONDIRENTRY.dwBytesInRes

                using (var dst = new BinaryWriter(new MemoryStream(len)))
                    {
                    // Copy GRPICONDIR to ICONDIR.

                    dst.Write(dir, 0, 6);

                        int picOffset = 6 + 16 * count; // sizeof(ICONDIR) + sizeof(ICONDIRENTRY) * count

                    for (int i = 0; i < count; ++i)
                        {
                        // Load the picture.

                        ushort id = BitConverter.ToUInt16(dir, 6 + 14 * i + 12);    // GRPICONDIRENTRY.nID
                        var pic = GetDataFromResource(hModule, RT_ICON, (IntPtr)id);

                        // Copy GRPICONDIRENTRY to ICONDIRENTRY.

                        dst.Seek(6 + 16 * i, 0);

                            dst.Write(dir, 6 + 14 * i, 8);  // First 8bytes are identical.
                        dst.Write(pic.Length);          // ICONDIRENTRY.dwBytesInRes
                        dst.Write(picOffset);           // ICONDIRENTRY.dwImageOffset

                        // Copy a picture.

                        dst.Seek(picOffset, 0);
                            dst.Write(pic, 0, pic.Length);

                            picOffset += pic.Length;
                        }

                        tmpData.Add(((MemoryStream)dst.BaseStream).ToArray());
                    }
                    return true;
                };
                EnumResourceNames(hModule, RT_GROUP_ICON, callback, IntPtr.Zero);
                FreeLibrary(hModule);
                byte[][] iconData = tmpData.ToArray();
                if (iconData.Count() > 0)
                {
                    using (var ms = new MemoryStream(iconData[0]))
                    {
                        return new Icon(ms);
                    }
                }
                else { return null; }
            } else
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }

        private static byte[] GetDataFromResource(IntPtr hModule, IntPtr type, IntPtr name)
        {
            // Load the binary data from the specified resource.
            IntPtr hResInfo = FindResource(hModule, name, type);
            IntPtr hResData = LoadResource(hModule, hResInfo);
            IntPtr pResData = LockResource(hResData);
            uint size = SizeofResource(hModule, hResInfo);
            byte[] buf = new byte[size];
            Marshal.Copy(pResData, buf, 0, buf.Length);

            return buf;
        }
    }
}
