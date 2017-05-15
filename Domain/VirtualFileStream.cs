using PsVDecrypt.Domain;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace PsVDecrypt.Domain
{
    internal class VirtualFileStream : IDisposable
    {
        private readonly object _Lock = new object();
        private long position;
        private VirtualFileCache _Cache;

        public VirtualFileStream(string EncryptedVideoFilePath)
        {
            this._Cache = new VirtualFileCache(EncryptedVideoFilePath);
        }

        private VirtualFileStream(VirtualFileCache Cache)
        {
            this._Cache = Cache;
        }

        public byte[] ReadAll()
        {
            unsafe
            {
                int pcbReadSign = 1;
                IntPtr pcbRead = new IntPtr(&pcbReadSign);
                var length = this._Cache.Length;
                var pv = new byte[length];
                this._Cache.Read(pv, (int) this.position, (int) length, pcbRead);
                return pv;
            }
        }

        public void Dispose()
        {
            this._Cache.Dispose();
        }
    }
}