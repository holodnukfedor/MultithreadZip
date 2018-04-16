using System;
using System.Collections.Generic;
using System.IO;
using ZipVeeamTest.DataBlocksParallelProcessing.BlockReadres.Interfaces;
using ZipVeeamTest.DataBlocksParallelProcessing.BlockReadres.ProcessingThreadChooseAlg.Interfaces;

namespace ZipVeeamTest.DataBlocksParallelProcessing.BlockReadres
{
    public class SimpleBlocksReader : BlocksReaderInSyncAbstract
    {
        private byte[] _buffer;

        protected override byte[] ReadBlock(FileStream filestream, out int readCount)
        {
            readCount = filestream.Read(_buffer, 0, _buffer.Length);
            return _buffer;
        }

        protected override void ReadBlocks(string filePath, List<ProcessingThreadDataQueue> processingThreadDataQueueList, int blockSize, EndTaskEvent readEndEvent)
        {
            _buffer = new byte[blockSize];
            base.ReadBlocks(filePath, processingThreadDataQueueList, blockSize, readEndEvent);
        }

        public SimpleBlocksReader(IProcessingThreadChooseAlg processingThreadChooseAlg)
            : base(processingThreadChooseAlg)
	    {
           
	    }
    }
}
