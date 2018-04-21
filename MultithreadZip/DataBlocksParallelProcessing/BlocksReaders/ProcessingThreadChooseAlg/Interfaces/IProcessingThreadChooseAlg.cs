using System;
using System.Collections.Generic;
using ZipVeeamTest;

namespace ZipVeeamTest.DataBlocksParallelProcessing.BlockReadres.ProcessingThreadChooseAlg.Interfaces
{
    public interface IProcessingThreadChooseAlg
    {
        ProcessingThreadBlocksQueue ChooseThread(List<ProcessingThreadBlocksQueue> processingThreadDataQueueList);
    }
}
