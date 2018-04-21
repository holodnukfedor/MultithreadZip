using System;
using System.Collections.Generic;
using System.IO;
using ZipVeeamTest.DataBlocksParallelProcessing.BlockReadres.Interfaces;
using ZipVeeamTest.DataBlocksParallelProcessing.BlockReadres.ProcessingThreadChooseAlg.Interfaces;

namespace ZipVeeamTest.DataBlocksParallelProcessing.BlockReadres
{
    public class SimpleBlocksReader : BlocksReaderInSyncAbstract
    {
        private int _blockSize;

        protected override byte[] ReadBlock(FileStream filestream, out int readCount)
        {
            var buffer = new byte[_blockSize];
            readCount = filestream.Read(buffer, 0, buffer.Length);
            return buffer;
        }

        protected override void ReadBlocks(string filePath, List<ProcessingThreadDataQueue> processingThreadDataQueueList, int blockSize, EndTaskEvent readEndEvent)
        {
            if (blockSize <= 0)
                throw new ArgumentException("Размер блока должен быть больше 0");

            _blockSize = blockSize;
            base.ReadBlocks(filePath, processingThreadDataQueueList, blockSize, readEndEvent);
        }

        public SimpleBlocksReader(IProcessingThreadChooseAlg processingThreadChooseAlg)
            : base(processingThreadChooseAlg)
	    {
           
	    }
    }
}
