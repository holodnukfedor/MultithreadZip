using System;
using System.Collections.Generic;
using ZipVeeamTest.DataBlocksParallelProcessing.BlockReadres.ProcessingThreadChooseAlg.Interfaces;

namespace ZipVeeamTest.DataBlocksParallelProcessing.BlockReadres.ProcessingThreadChooseAlg
{
    public class MinLoadedThreadChooseAlg : IProcessingThreadChooseAlg
    {
        public ProcessingThreadData ChooseThread(List<ProcessingThreadData> threadDataList)
        {
            int minCountInQueueThreadIndex = 0;

            for (int i = 1; i < threadDataList.Count; ++i)
            {
                if (threadDataList[i].SynchronizedQueue.Count < threadDataList[minCountInQueueThreadIndex].SynchronizedQueue.Count)
                    minCountInQueueThreadIndex = i;
            }

            return threadDataList[minCountInQueueThreadIndex];
        }
    }
}
