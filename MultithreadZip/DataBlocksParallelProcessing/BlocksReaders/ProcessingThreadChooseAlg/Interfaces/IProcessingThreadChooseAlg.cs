using System;
using System.Collections.Generic;
using ZipVeeamTest;

namespace ZipVeeamTest.DataBlocksParallelProcessing.BlockReadres.ProcessingThreadChooseAlg.Interfaces
{
    public interface IProcessingThreadChooseAlg
    {
        ProcessingThreadData ChooseThread(List<ProcessingThreadData> threadDataList);
    }
}
