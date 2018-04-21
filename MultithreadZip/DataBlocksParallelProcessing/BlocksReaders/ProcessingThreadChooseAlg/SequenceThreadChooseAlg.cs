using System;
using System.Collections.Generic;
using ZipVeeamTest.DataBlocksParallelProcessing.BlockReadres.ProcessingThreadChooseAlg.Interfaces;

namespace ZipVeeamTest.DataBlocksParallelProcessing.BlockReadres.ProcessingThreadChooseAlg
{
    public class SequenceThreadChooseAlg : IProcessingThreadChooseAlg
    {
        private int _chosenThreadIndex = -1;

        public ProcessingThreadBlocksQueue ChooseThread(List<ProcessingThreadBlocksQueue> processingThreadDataQueueList)
        {
            if (++_chosenThreadIndex == processingThreadDataQueueList.Count)
                _chosenThreadIndex = 0;

            return processingThreadDataQueueList[_chosenThreadIndex];
        }
    }
}
