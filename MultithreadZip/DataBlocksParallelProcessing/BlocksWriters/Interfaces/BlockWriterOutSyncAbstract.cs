using System;
using System.Collections.Generic;
using System.IO;

namespace ZipVeeamTest.DataBlocksParallelProcessing.BlockWriters.Interfaces
{
    public abstract class  BlockWriterOutSyncAbstract : BlockWriterAbstract
    {
        protected abstract void WriteBlock(FileStream fileStream, byte[] buffer, int countToWrite);

        protected override void WriteBlocks(string filePath, EndTaskEvent processingTaskEndEvent, BlocksPreparedToWrite blocksPreparedToWrite, EndTaskEvent endWriteEvent)
        {
            const int defaultBufferSize = 4096;

            using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, defaultBufferSize, false))
            {
                while (true)
                {
                    int awaitedKey = blocksPreparedToWrite.AwaitedKey;
                   
                    if (!blocksPreparedToWrite.ProcessedBlocksTable.ContainsKey(awaitedKey) && !processingTaskEndEvent.IsEnded())
                        blocksPreparedToWrite.WaitOne(awaitedKey);

                    if (!blocksPreparedToWrite.ProcessedBlocksTable.ContainsKey(awaitedKey) && processingTaskEndEvent.IsEnded())
                        break;

                    var buffer = blocksPreparedToWrite.ProcessedBlocksTable[awaitedKey] as byte[];

                    if (buffer == null)
                    {
                        throw new Exception("В очереди должны быть массивы byte []");
                    }
                        

                    blocksPreparedToWrite.Reset();
                    blocksPreparedToWrite.AwaitedKey = awaitedKey + 1;

                    Console.WriteLine(awaitedKey);
                    WriteBlock(fileStream, buffer, buffer.Length);
                    blocksPreparedToWrite.ProcessedBlocksTable.Remove(awaitedKey);
                }
            }

            endWriteEvent.SetEndTask();
        }
    }
}
