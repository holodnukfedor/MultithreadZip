using System;

namespace ZipVeeamTest
{
    public class DataBlock
    {
        public readonly int Number;

        public readonly byte[] Block;

        public DataBlock(byte[] block, int number)
        {
            Block = block;
            Number = number;
        }
    }
}
