using System;

namespace ZipVeeamTest.DataBlocksParallelProcessing.BlockWriters.Params
{
    public class WriteBlocksParams
    {
        public readonly string FilePath;

        public readonly ProcessedBlocksQueue ProcessedBlocks;

        public readonly EndTaskEvent WriteEndEvent;

        private void CheckCtorArguments(string filePath, ProcessedBlocksQueue processedBlocks, EndTaskEvent writeEndEvent)
        {
            if (String.IsNullOrEmpty(filePath.Trim()))
                throw new ArgumentException("Должен быть задан путь к файлу ");

            if (processedBlocks == null)
                throw new ArgumentNullException("processedBlocks");

            if (writeEndEvent == null)
                throw new ArgumentNullException("writeEndEvent");

            if (writeEndEvent.IsEnded())
                throw new ArgumentException("Нельзя посылать событие окончании записи с флагом законченного задания");
        }

        public WriteBlocksParams(string filePath, ProcessedBlocksQueue processedBlocks, EndTaskEvent writeEndEvent)
        {
            CheckCtorArguments(filePath, processedBlocks, writeEndEvent);
            FilePath = filePath;
            ProcessedBlocks = processedBlocks;
            WriteEndEvent = writeEndEvent;
        }
    }
}
