using System;
using ZipVeeamTest.DataBlocksParallelProcessing.BlockWriters.Interfaces;
using ZipVeeamTest.DataBlocksParallelProcessing.BlockWriters.Params;
using System.IO;

namespace ZipVeeamTest.DataBlocksParallelProcessing.BlockWriters
{
    public class SimpleBlocksWriter : BlockWriterOutSyncAbstract
    {
        protected override void WriteBlock(FileStream fileStream, byte[] buffer, int countToWrite)
        {
            fileStream.Write(buffer, 0, countToWrite);
        }
    }
}
