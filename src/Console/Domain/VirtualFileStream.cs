using System;

namespace PsVDecrypt.Domain
{
    internal class VirtualFileStream : IDisposable
    {
        private readonly long _position = 0;
        private readonly VirtualFileCache _cache;

        public VirtualFileStream(string encryptedVideoFilePath)
        {
            this._cache = new VirtualFileCache(encryptedVideoFilePath);
        }

        private VirtualFileStream(VirtualFileCache cache)
        {
            this._cache = cache;
        }

        public byte[] ReadAll()
        {
            unsafe
            {
                int pcbReadSign = 1;
                IntPtr pcbRead = new IntPtr(&pcbReadSign);
                var length = this._cache.Length;
                var pv = new byte[length];
                this._cache.Read(pv, (int)this._position, (int)length, pcbRead);
                return pv;
            }
        }

        public void Dispose()
        {
            this._cache.Dispose();
        }
    }
}