using System;
using System.Collections.Generic;
using ZipVeeamTest.DataBlocksParallelProcessing.BlockReadres.ProcessingThreadChooseAlg.Interfaces;
using System.IO;

namespace ZipVeeamTest.DataBlocksParallelProcessing.BlockReadres.Interfaces
{
    public abstract class BlocksReaderInSyncAbstract : BlocksReaderAbstract
    {
        protected abstract byte[] ReadBlock(FileStream filestream, out int readCount);

        protected override void ReadBlocks(string filePath, List<ProcessingThreadData> processingThreadDataList, int blockSize, EndTaskEvent readEndEvent)
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
                        ProcessingThreadData processingThreadData = _processingThreadChooseAlg.ChooseThread(processingThreadDataList);

                        byte[] bufferCopy = new byte[readCount];
                        Array.Copy(buffer, bufferCopy, readCount);
                        DataBlock readBlock = new DataBlock(bufferCopy, returnedReadBlockNumber++);

                        processingThreadData.SynchronizedQueue.Enqueue(readBlock);

                        if (processingThreadData.SynchronizedQueue.Count == 1)
                            processingThreadData.Set();
                    }
                    else
                    {
                        break;
                    }
                }
            }

            readEndEvent.SetEndTask();
            foreach (var processingThreadData in processingThreadDataList)
                processingThreadData.Set();
        }

        public BlocksReaderInSyncAbstract(IProcessingThreadChooseAlg processingThreadChooseAlg)
            : base(processingThreadChooseAlg)
	    {

	    }
    }

    
}
