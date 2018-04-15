using System;
using ZipVeeamTest.DataBlocksParallelProcessing.BlockProcessor.Interfaces;
using ZipVeeamTest;
using System.IO;
using System.IO.Compression;

namespace ZipVeeamTest.DataBlocksParallelProcessing.BlockProcessor
{
    public class GZipCompressProcessor : IBlockProcessor
    {
        public byte[] Process(DataBlock readBlock)
        {
            byte [] buffer;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (var zipStream = new GZipStream(memoryStream, CompressionMode.Compress, false))
                {
                    zipStream.Write(readBlock.Block, 0, readBlock.Block.Length);
                }

                buffer = memoryStream.ToArray();
            }
            return buffer;
        }
    }
}
