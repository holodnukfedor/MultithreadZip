using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace ZipVeeamTest.DataBlocksParallelProcessing.BlockReadres.Params
{
    public class ReadBlocksParams
    {
        public readonly string FilePath;

        public readonly List<ProcessingThreadDataQueue> ProcessingThreadDataQueueList;

        public readonly int BlockSize;

        public readonly EndTaskEvent EndReadEvent;

        private void CheckCtorArguments(string filePath, List<ProcessingThreadDataQueue> processingThreadDataQueueList, int blockSize, EndTaskEvent endReadEvent)
        {
            if (String.IsNullOrEmpty(filePath.Trim()))
                throw new ArgumentException("должен быть задан путь к файлу");

            if (processingThreadDataQueueList == null)
                throw new ArgumentNullException("processingThreadDataList");

            if (processingThreadDataQueueList.Count == 0)
                throw new ArgumentException("список данных процессов обработки должен иметь хотя бы один элемент");

            if (blockSize <= 0)
                throw new ArgumentException("Размер блока для чтения должен быть больше нуля");

            if (endReadEvent == null)
                throw new ArgumentNullException("endReadEvent");

            if (!File.Exists(filePath))
                throw new FileNotFoundException(String.Format("По указанному пути '{0}' не обнаружен файл", filePath));
        }

        public ReadBlocksParams(string filePath, List<ProcessingThreadDataQueue> processingThreadDataList, int blockSize, EndTaskEvent endReadEvent)
        {
            CheckCtorArguments(filePath, processingThreadDataList, blockSize, endReadEvent);
            FilePath = filePath;
            ProcessingThreadDataQueueList = processingThreadDataList;
            BlockSize = blockSize;
            EndReadEvent = endReadEvent;
        }
    }
}
