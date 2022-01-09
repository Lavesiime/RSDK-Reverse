﻿using System.Text;
using System.IO;

namespace RSDKv1
{
    public class Writer : BinaryWriter
    {
        public Writer(Stream stream) : base(stream) { }

        public Writer(string file) : base(File.Open(file, FileMode.Create, FileAccess.Write, FileShare.ReadWrite)) { }

        public void seek(long position, SeekOrigin org)
        {
            BaseStream.Seek(position, org);
        }

        public void writeRSDKString(string val)
        {
            base.Write((byte)val.Length);
            base.Write(new UTF8Encoding().GetBytes(val));
        }
    }
}
