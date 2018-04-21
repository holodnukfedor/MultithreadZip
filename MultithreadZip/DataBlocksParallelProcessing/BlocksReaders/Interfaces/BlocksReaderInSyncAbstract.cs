using System;
using System.Collections.Generic;
using System.IO;
using ZipVeeamTest.DataBlocksParallelProcessing.BlockReadres.ProcessingThreadChooseAlg.Interfaces;


namespace ZipVeeamTest.DataBlocksParallelProcessing.BlockReadres.Interfaces
{
    public abstract class BlocksReaderInSyncAbstract : BlocksReaderAbstract
    {
        protected abstract byte[] ReadBlock(FileStream filestream, out int readCount);

        protected override void ReadBlocks(string filePath, List<ProcessingThreadBlocksQueue> processingThreadDataQueueList, int blockSize, EndTaskEvent readEndEvent)
        {
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None, blockSize, false))
            {
                if (fileStream.Length == 0)
                    throw new InvalidOperationException("Нет смысла проводить обработку файла нулевой длины");

                int returnedReadBlockNumber = 1;
                byte[] buffer;

                while (true)
                {
                    int readCount;
                    buffer = ReadBlock(fileStream, out readCount);

                    if (readCount > 0)
                    {
                        ProcessingThreadBlocksQueue processingThreadDataQueue = _processingThreadChooseAlg.ChooseThread(processingThreadDataQueueList);

                        DataBlock readBlock = new DataBlock(buffer, returnedReadBlockNumber++, readCount);

                        processingThreadDataQueue.Enqueue(readBlock);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            readEndEvent.SetEndTask();
            foreach (var processingThreadData in processingThreadDataQueueList)
                processingThreadData.AwakeWaitingsForAddition();
        }

        public BlocksReaderInSyncAbstract(IProcessingThreadChooseAlg processingThreadChooseAlg)
            : base(processingThreadChooseAlg)
	    {

	    }
    }

    
}
