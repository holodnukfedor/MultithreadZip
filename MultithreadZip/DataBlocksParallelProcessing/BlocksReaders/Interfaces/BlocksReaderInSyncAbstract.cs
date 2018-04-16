﻿using System;
using System.Collections.Generic;
using System.IO;
using ZipVeeamTest.DataBlocksParallelProcessing.BlockReadres.ProcessingThreadChooseAlg.Interfaces;


namespace ZipVeeamTest.DataBlocksParallelProcessing.BlockReadres.Interfaces
{
    public abstract class BlocksReaderInSyncAbstract : BlocksReaderAbstract
    {
        protected abstract byte[] ReadBlock(FileStream filestream, out int readCount);

        protected override void ReadBlocks(string filePath, List<ProcessingThreadDataQueue> processingThreadDataQueueList, int blockSize, EndTaskEvent readEndEvent)
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
                        ProcessingThreadDataQueue processingThreadDataQueue = _processingThreadChooseAlg.ChooseThread(processingThreadDataQueueList);

                        byte[] bufferCopy = new byte[readCount];
                        Array.Copy(buffer, bufferCopy, readCount);
                        DataBlock readBlock = new DataBlock(bufferCopy, returnedReadBlockNumber++);

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
                processingThreadData.AwakeTheWaitings();
        }

        public BlocksReaderInSyncAbstract(IProcessingThreadChooseAlg processingThreadChooseAlg)
            : base(processingThreadChooseAlg)
	    {

	    }
    }

    
}
