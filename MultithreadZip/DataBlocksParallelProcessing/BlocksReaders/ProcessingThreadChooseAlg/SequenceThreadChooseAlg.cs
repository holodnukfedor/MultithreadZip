using System;
using System.Collections.Generic;
using ZipVeeamTest.DataBlocksParallelProcessing.BlockReadres.ProcessingThreadChooseAlg.Interfaces;

namespace ZipVeeamTest.DataBlocksParallelProcessing.BlockReadres.ProcessingThreadChooseAlg
{
    public class SequenceThreadChooseAlg : IProcessingThreadChooseAlg
    {
        private int _chosenThreadIndex = -1;

        public ProcessingThreadData ChooseThread(List<ProcessingThreadData> threadDataList)
        {
            if (++_chosenThreadIndex == threadDataList.Count)
                _chosenThreadIndex = 0;

            return threadDataList[_chosenThreadIndex];
        }
    }
}
