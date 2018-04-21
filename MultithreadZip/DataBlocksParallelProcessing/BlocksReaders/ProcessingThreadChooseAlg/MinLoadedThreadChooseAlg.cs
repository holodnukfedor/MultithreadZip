using System;
using System.Collections.Generic;
using ZipVeeamTest.DataBlocksParallelProcessing.BlockReadres.ProcessingThreadChooseAlg.Interfaces;

namespace ZipVeeamTest.DataBlocksParallelProcessing.BlockReadres.ProcessingThreadChooseAlg
{
    public class MinLoadedThreadChooseAlg : IProcessingThreadChooseAlg
    {
        public ProcessingThreadBlocksQueue ChooseThread(List<ProcessingThreadBlocksQueue> processingThreadDataQueueList)
        {
            int minCountInThreadQueueIndex = 0;

            for (int i = 1; i < processingThreadDataQueueList.Count; ++i)
            {
                if (processingThreadDataQueueList[i].Count < processingThreadDataQueueList[minCountInThreadQueueIndex].Count)
                    minCountInThreadQueueIndex = i;
            }

            return processingThreadDataQueueList[minCountInThreadQueueIndex];
        }
    }
}
