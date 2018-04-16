using System;
using System.Collections.Generic;
using ZipVeeamTest;

namespace ZipVeeamTest.DataBlocksParallelProcessing.BlockReadres.ProcessingThreadChooseAlg.Interfaces
{
    public interface IProcessingThreadChooseAlg
    {
        ProcessingThreadDataQueue ChooseThread(List<ProcessingThreadDataQueue> processingThreadDataQueueList);
    }
}
