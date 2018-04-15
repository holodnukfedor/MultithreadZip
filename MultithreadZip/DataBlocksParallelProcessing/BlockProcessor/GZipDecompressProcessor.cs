using System;
using ZipVeeamTest.DataBlocksParallelProcessing.BlockProcessor.Interfaces;
using ZipVeeamTest;
using System.IO;
using System.IO.Compression;

namespace ZipVeeamTest.DataBlocksParallelProcessing.BlockProcessor
{
    public class GZipDecompressProcessor : IBlockProcessor
    {
        private int _blockSize;

        public byte[] Process(DataBlock readBlock)
        {
            byte[] buffer = new byte[_blockSize];
            int readCount;
            using (var memStream = new MemoryStream(readBlock.Block))
            {
                using (var gzipStream = new GZipStream(memStream, CompressionMode.Decompress))
                {
                    readCount = gzipStream.Read(buffer, 0, _blockSize);
                }
            }

            byte[] bufferCopy = new byte[readCount];
            Array.Copy(buffer, bufferCopy, bufferCopy.Length);
            return bufferCopy;
        }

        public GZipDecompressProcessor(int blockSize)
        {
            if (blockSize <= 0)
                throw new ArgumentException("Размер блока должен быть положительным");

            _blockSize = blockSize;
        }
    }
}
