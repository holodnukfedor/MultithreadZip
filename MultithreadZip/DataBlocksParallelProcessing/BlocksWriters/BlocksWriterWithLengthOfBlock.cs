using System;
using ZipVeeamTest.DataBlocksParallelProcessing.BlockWriters.Interfaces;
using ZipVeeamTest.DataBlocksParallelProcessing.BlockWriters.Params;
using System.IO;

namespace ZipVeeamTest.DataBlocksParallelProcessing.BlockWriters
{
    public class BlocksWriterWithLength : BlockWriterOutSyncAbstract
    {
        protected override void WriteBlock(FileStream fileStream, byte[] buffer, int countToWrite)
        {
            byte[] lengthByteArr = BitConverter.GetBytes(countToWrite);
            fileStream.Write(lengthByteArr, 0, lengthByteArr.Length);
            fileStream.Write(buffer, 0, buffer.Length);
        }
    }
}
