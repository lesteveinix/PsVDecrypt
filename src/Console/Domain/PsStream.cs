using System.IO;

namespace PsVDecrypt.Domain
{
    public class PsStream : IPsStream
    {
        private readonly Stream _fileStream;
        private long _length;

        public int BlockSize
        {
            get
            {
                return 262144;
            }
        }

        public long Length
        {
            get
            {
                return this._length;
            }
        }

        public PsStream(string filenamePath)
        {
            this._fileStream = (Stream)File.Open(filenamePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            this._length = new FileInfo(filenamePath).Length;
        }

        public void Seek(int offset, SeekOrigin begin)
        {
            if (this._length <= 0L)
                return;
            this._fileStream.Seek((long)offset, begin);
        }

        public int Read(byte[] pv, int i, int count)
        {
            if (this._length <= 0L)
                return 0;
            return this._fileStream.Read(pv, i, count);
        }

        public void Dispose()
        {
            this._length = 0L;
            this._fileStream.Dispose();
        }
    }
}
