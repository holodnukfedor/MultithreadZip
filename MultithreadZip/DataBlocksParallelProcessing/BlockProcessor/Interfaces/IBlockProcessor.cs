using System;
using ZipVeeamTest;

namespace ZipVeeamTest.DataBlocksParallelProcessing.BlockProcessor.Interfaces
{
    public interface IBlockProcessor
    {
        byte[] Process(DataBlock dataBlock);
    }
}
