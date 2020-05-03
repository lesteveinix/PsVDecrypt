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
                return this._encryptedVideoFile.Length;
            }
        }

        public VirtualFileCache(string encryptedVideoFilePath)
        {
            this._encryptedVideoFile = (IPsStream)new PsStream(encryptedVideoFilePath);
        }

        public VirtualFileCache(IPsStream stream)
        {
            this._encryptedVideoFile = stream;
        }

        public void Read(byte[] pv, int offset, int count, IntPtr pcbRead)
        {
            if (this.Length == 0L)
                return;
            this._encryptedVideoFile.Seek(offset, SeekOrigin.Begin);
            int length = this._encryptedVideoFile.Read(pv, 0, count);
            VideoEncryption.XorBuffer(pv, length, (long)offset);
            if (IntPtr.Zero == pcbRead)
                return;
            Marshal.WriteIntPtr(pcbRead, new IntPtr(length));
        }

        public void Dispose()
        {
            this._encryptedVideoFile.Dispose();
        }
    }
}
