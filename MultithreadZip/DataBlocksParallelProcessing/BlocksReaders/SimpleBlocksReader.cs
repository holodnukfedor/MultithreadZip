using System;
using System.Collections.Generic;
using System.Linq;
using ZipVeeamTest.DataBlocksParallelProcessing.BlockReadres.Interfaces;
using System.IO;
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

        public SimpleBlocksReader(IProcessingThreadChooseAlg processingThreadChooseAlg, int blockSize)
            : base(processingThreadChooseAlg)
	    {
            if (blockSize <= 0)
                throw new ArgumentException("Размер блока должен быть положительным");

            _buffer = new byte[blockSize];
	    }
    }
}
