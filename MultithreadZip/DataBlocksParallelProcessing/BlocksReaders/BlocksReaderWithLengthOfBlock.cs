using System;
using System.Collections.Generic;
using ZipVeeamTest.DataBlocksParallelProcessing.BlockReadres.Interfaces;
using System.IO;
using ZipVeeamTest.DataBlocksParallelProcessing.BlockReadres.ProcessingThreadChooseAlg.Interfaces;

namespace ZipVeeamTest.DataBlocksParallelProcessing.BlockReadres
{
    public class BlocksReaderWithLength : BlocksReaderInSyncAbstract
    {
        protected override byte[] ReadBlock(FileStream filestream, out int readCount)
        {
            var lengthBuffer = new byte[4];
            filestream.Read(lengthBuffer, 0, lengthBuffer.Length);
            var blockLength = BitConverter.ToInt32(lengthBuffer, 0);

            var buffer = new byte[blockLength];
            readCount = filestream.Read(buffer, 0, blockLength);
            return buffer;
        }

        public BlocksReaderWithLength(IProcessingThreadChooseAlg processingThreadChooseAlg)
            : base(processingThreadChooseAlg)
	    {

	    }
    }
}
