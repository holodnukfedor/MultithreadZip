using System;

namespace ZipVeeamTest
{
    public class DataBlock
    {
        public readonly int Number;

        public readonly int Size;

        public readonly byte[] Block;

        public DataBlock(byte[] block, int number, int size)
        {
            if (block == null)
                throw new ArgumentException("Не имеет смысла использовать блок данных null");

            if (block.Length <= 0)
                throw new ArgumentException("Размер блока должен быть больше либо равен 0");

            if (size <= 0)
                throw new ArgumentException("Не имеет смысла использовать размер меньше либо равный 0");

            if (number < 0)
                throw new ArgumentException("Номер не должен быть отрицательным");

            Block = block;
            Number = number;
            Size = size;
        }
    }
}
