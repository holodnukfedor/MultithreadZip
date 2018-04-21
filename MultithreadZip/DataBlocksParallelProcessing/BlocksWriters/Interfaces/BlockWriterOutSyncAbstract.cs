using System;
using System.Collections.Generic;
using System.IO;

namespace ZipVeeamTest.DataBlocksParallelProcessing.BlockWriters.Interfaces
{
    public abstract class  BlockWriterOutSyncAbstract : BlockWriterAbstract
    {
        protected abstract void WriteBlock(FileStream fileStream, byte[] buffer, int countToWrite);

        protected override void WriteBlocks(string filePath, ProcessedBlocksQueue processedBlocks, EndTaskEvent endWriteEvent)
        {
            const int defaultBufferSize = 4096;

            using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, defaultBufferSize, false))
            {
                while (true)
                {
                    var buffer = processedBlocks.DequeueWait();

                    if (buffer == null)
                        break;
                        
                    WriteBlock(fileStream, buffer, buffer.Length);
                    fileStream.Flush();
                }
            }

            endWriteEvent.SetEndTask();
        }
    }
}
