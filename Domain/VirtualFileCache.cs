using System;
using System.IO;
using System.Runtime.InteropServices;

namespace PsVDecrypt.Domain
{
    public class VirtualFileCache : IDisposable
    {
        private readonly IPsStream _encryptedVideoFile;

        public long Length
        {
            get
            {
                return _encryptedVideoFile.Length;
            }
        }

        public VirtualFileCache(string encryptedVideoFilePath)
        {
            _encryptedVideoFile = (IPsStream)new PsStream(encryptedVideoFilePath);
        }

        public VirtualFileCache(IPsStream stream)
        {
            _encryptedVideoFile = stream;
        }

        public void Read(byte[] pv, int offset, int count, IntPtr pcbRead)
        {
            if (Length == 0L)
                return;
            _encryptedVideoFile.Seek(offset, SeekOrigin.Begin);
            int length = _encryptedVideoFile.Read(pv, 0, count);
            VideoEncryption.XorBuffer(pv, length, (long)offset);
            if (!(IntPtr.Zero != pcbRead))
                return;
            Marshal.WriteIntPtr(pcbRead, new IntPtr(length));
        }

        public void Dispose()
        {
            _encryptedVideoFile.Dispose();
        }
    }
}
