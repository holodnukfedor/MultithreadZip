using System;

namespace ZipVeeamTest.DataBlocksParallelProcessing.BlockWriters.Params
{
    public class WriteBlocksParams
    {
        public readonly string FilePath;

        public readonly EndTaskEvent ProcessingEndEvent;

        public readonly BlocksPreparedToWrite BlockPreparedToWrite;

        public readonly EndTaskEvent WriteEndEvent;

        private void CheckCtorArguments(string filePath, EndTaskEvent processingTaskEndEvent, BlocksPreparedToWrite blocksPreparedToWrite, EndTaskEvent writeEndEvent)
        {
            if (String.IsNullOrEmpty(filePath.Trim()))
                throw new ArgumentException("Должен быть задан путь к файлу ");

            if (processingTaskEndEvent == null)
                throw new ArgumentNullException("processingTaskEndEvent");

            if (blocksPreparedToWrite == null)
                throw new ArgumentNullException("blocksPreparedToWrite");

            if (blocksPreparedToWrite.AwaitedKey < 1)
                throw new ArgumentException("Ожидаемый ключ сигнализатора должен быть более либо равен 1");

            if (writeEndEvent == null)
                throw new ArgumentNullException("writeEndEvent");

            if (writeEndEvent.IsEnded())
                throw new ArgumentException("Нельзя посылать событие окончании записи с флагом законченного задания");
        }

        public WriteBlocksParams(string filePath, EndTaskEvent processingEndEvent, BlocksPreparedToWrite blocksPreparedToWrite, EndTaskEvent writeEndEvent)
        {
            CheckCtorArguments(filePath, processingEndEvent, blocksPreparedToWrite, writeEndEvent);
            FilePath = filePath;
            ProcessingEndEvent = processingEndEvent;
            BlockPreparedToWrite = blocksPreparedToWrite;
            WriteEndEvent = writeEndEvent;
        }
    }
}
