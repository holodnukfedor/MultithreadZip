using System;
using System.Collections.Generic;
using ZipVeeamTest.DataBlocksParallelProcessing.BlockReadres.Params;
using System.Threading;
using ZipVeeamTest.DataBlocksParallelProcessing.BlockReadres.ProcessingThreadChooseAlg.Interfaces;

namespace ZipVeeamTest.DataBlocksParallelProcessing.BlockReadres.Interfaces
{
    public abstract class BlocksReaderAbstract
    {
        private Thread _readThread;

        protected IProcessingThreadChooseAlg _processingThreadChooseAlg;

        protected abstract void ReadBlocks(string filePath, List<ProcessingThreadData> processingThreadDataList, int blockSize, EndTaskEvent endReadEvent);

        protected virtual void ReadBlocks(object obj)
        {
            var parameters = obj as ReadBlocksParams;

            if (parameters == null)
                throw new ArgumentException("параметр функции должен быть типом ReadBlocksParams");

            ReadBlocks(parameters.FilePath, parameters.ProcessingThreadDataList, parameters.BlockSize, parameters.EndReadEvent);
        }

        public void StartReadBlocks(Object obj)
        {
            _readThread.Start(obj);
        }

        public BlocksReaderAbstract(IProcessingThreadChooseAlg processingThreadChooseAlg)
        {
            if (processingThreadChooseAlg == null)
                throw new ArgumentNullException("processingThreadChooseAlg");

            _processingThreadChooseAlg = processingThreadChooseAlg;
            _readThread = new Thread(ReadBlocks);
        }
    }
}
