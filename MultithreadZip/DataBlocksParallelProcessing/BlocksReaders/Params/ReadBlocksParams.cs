using System;
using System.Collections.Generic;
using System.IO;
using System.Collections;
using System.Threading;

namespace ZipVeeamTest.DataBlocksParallelProcessing.BlockReadres.Params
{
    public class ReadBlocksParams
    {
        public readonly string FilePath;

        public readonly List<ProcessingThreadData> ProcessingThreadDataList;

        public readonly int BlockSize;

        public readonly EndTaskEvent EndReadEvent;

        private void CheckCtorArguments(string filePath, List<ProcessingThreadData> processingThreadDataList, int blockSize, EndTaskEvent endReadEvent)
        {
            if (String.IsNullOrEmpty(filePath.Trim()))
                throw new ArgumentException("должен быть задан путь к файлу");

            if (processingThreadDataList == null)
                throw new ArgumentNullException("processingThreadDataList");

            if (processingThreadDataList.Count == 0)
                throw new ArgumentException("список данных процессов обработки должен иметь хотя бы один элемент");

            if (blockSize <= 0)
                throw new ArgumentException("Размер блока для чтения должен быть больше нуля");

            if (endReadEvent == null || endReadEvent.IsEnded())
                throw new ArgumentException("параметр endReadEvent не должен быть равен null или уже иметь флаг завершения");

            if (!File.Exists(filePath))
                throw new FileNotFoundException(String.Format("По указанному пути '{0}' не обнаружен файл", filePath));

            // проверить на права на чтение
        }

        public ReadBlocksParams(string filePath, List<ProcessingThreadData> processingThreadDataList, int blockSize, EndTaskEvent endReadEvent)
        {
            CheckCtorArguments(filePath, processingThreadDataList, blockSize, endReadEvent);
            FilePath = filePath;
            ProcessingThreadDataList = processingThreadDataList;
            BlockSize = blockSize;
            EndReadEvent = endReadEvent;
        }
    }
}
