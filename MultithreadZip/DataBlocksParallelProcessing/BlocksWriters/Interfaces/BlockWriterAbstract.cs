using System;
using System.Collections.Generic;
using System.Threading;
using ZipVeeamTest.DataBlocksParallelProcessing.BlockWriters.Params;

namespace ZipVeeamTest.DataBlocksParallelProcessing.BlockWriters.Interfaces
{
    public abstract class BlockWriterAbstract
    {
        private Thread _writeThread;

        protected abstract void WriteBlocks(string filePath, ProcessedBlocksCollection processedBlocks, EndTaskEvent endWriteEvent);

        protected virtual void WriteBlocks(Object obj)
        {
            var parameters = obj as WriteBlocksParams;

            if (parameters == null)
                throw new ArgumentException("параметр функции должен быть типом WriteBlockParams");

            WriteBlocks(parameters.FilePath, parameters.ProcessedBlocks, parameters.WriteEndEvent);
        }

        public void StartWriteBlocks(Object obj)
        {
            _writeThread.Start(obj);
        }

        public BlockWriterAbstract()
        {
            _writeThread = new Thread(WriteBlocks);
        }
    }
}
