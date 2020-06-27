using System;

namespace PsVDecrypt.Domain
{
    internal class VirtualFileStream : IDisposable
    {
        // private readonly object _lock = new object();
        private long _position;
        private VirtualFileCache _cache;

        public VirtualFileStream(string encryptedVideoFilePath)
        {
            _cache = new VirtualFileCache(encryptedVideoFilePath);
        }

        private VirtualFileStream(VirtualFileCache cache)
        {
            _cache = cache;
        }

        public byte[] ReadAll()
        {
            unsafe
            {
                int pcbReadSign = 1;
                IntPtr pcbRead = new IntPtr(&pcbReadSign);
                var length = _cache.Length;
                var pv = new byte[length];
                _cache.Read(pv, (int) _position, (int) length, pcbRead);
                return pv;
            }
        }

        public void Dispose()
        {
            _cache.Dispose();
        }
    }
}