﻿using System;
using System.Collections;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using ZipVeeamTest.DataBlocksParallelProcessing.BlockWriters.Interfaces;
using ZipVeeamTest.DataBlocksParallelProcessing.BlockWriters.Params;
using ZipVeeamTest.DataBlocksParallelProcessing.BlockReadres.Interfaces;
using ZipVeeamTest.DataBlocksParallelProcessing.BlockReadres.Params;
using ZipVeeamTest.DataBlocksParallelProcessing.BlockReadres.ProcessingThreadChooseAlg.Interfaces;
using ZipVeeamTest.DataBlocksParallelProcessing.BlockProcessor.Interfaces;
using System.Diagnostics;

namespace ZipVeeamTest
{
    public class DataBlockParallelProcessor
    {
        private const  int _operationMemoryReserveForOS = 209715200;

        private const int _minimumSizeOfCollections = 5;

        private ProcessedBlocksQueue _blocksPreparedToWrite;

        private List<ProcessingThreadBlocksQueue> _processingThreadDataQueueList = new List<ProcessingThreadBlocksQueue>();

        private List<Thread> _processThreadsList = new List<Thread>();

        private EndTaskEvent _readEndEvent = new EndTaskEvent();

        private EndTaskEvent _endProcessingEvent = new EndTaskEvent();

        private EndTaskEvent _writeEndEvent = new EndTaskEvent();

        private BlocksReaderAbstract _blocksReader;

        private IBlockProcessor _blockHandler;

        private BlockWriterAbstract _blocksWriter;

        private string _sourcePath;

        private string _destinationPath;

        private volatile int _countOfProcessingThread;

        public readonly int BlockSize;

        public readonly int ProcessorsCount;

        private int GetDataBlockCollectionSize()
        {
            long collectionSize;
            using (var peformanceCounter = new PerformanceCounter("Memory", "Available Bytes"))
            {
                var availiableMemory = Convert.ToInt64(peformanceCounter.NextValue()) - _operationMemoryReserveForOS;
                collectionSize = availiableMemory / (ProcessorsCount + 1);
            }

            // Это значение можно было бы вычислять, оно нужно чтобы память выделялась непрырывно не было
            // OutOfMemoryException и память не лезла в виртуальную, обращение к виртуальной памяти тормозит процессор и он
            // не выполняет работу. В общем как правильно вычислить быстро не разобрался. При 50 в виртуальное память алгоритм не лезет
            int limitOfContinuosMemory = 50;//(int.MaxValue / (BlockSize * 2 + sizeof(int)) - 10) / 2;

            if (collectionSize > limitOfContinuosMemory)
                collectionSize = limitOfContinuosMemory;

            if (collectionSize < _minimumSizeOfCollections)
            {
                Console.WriteLine("Недостаточно оперативной физической памяти для выполнения процесса. Будет использована вирутальная, что скажется на производительности. Закройте остальные приложения для максимальной производительности");
                collectionSize = _minimumSizeOfCollections;
            }

            return (int) collectionSize;
        }

        private void ProcessDataBlocksQueue(object threadIndexObject)
        {
            try
            {
                int threadIndex = (int)threadIndexObject;
                ProcessingThreadBlocksQueue processingThreadDataQueue = _processingThreadDataQueueList[threadIndex];

                while (true)
                {
                    DataBlock readBlock = processingThreadDataQueue.DequeueWait();

                    if (readBlock == null)
                        break;

                    ProcessDataBlock(readBlock);
                }

                --_countOfProcessingThread;
                if (_countOfProcessingThread == 0)
                {
                    _endProcessingEvent.SetEndTask();
                    _blocksPreparedToWrite.AwakeWaitingsForAddition();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Произошла ошибка при обработке: {0}. Программа завершается", ex.Message);
                Environment.Exit(0);
            }
        }

        private void ProcessDataBlock(DataBlock readBlock)
        {
            var buffer = _blockHandler.Process(readBlock);
            _blocksPreparedToWrite.Enqueue(new DataBlock(buffer, readBlock.Number, buffer.Length));
        }

        public void StartProcessing()
        {
            var readParameters = new ReadBlocksParams(_sourcePath, _processingThreadDataQueueList, BlockSize, _readEndEvent);
            _blocksReader.StartReadBlocks(readParameters);

            for (int i = 0; i < ProcessorsCount; ++i)
                _processThreadsList[i].Start(i);

            var writeParameters = new WriteBlocksParams(_destinationPath, _blocksPreparedToWrite, _writeEndEvent);
            _blocksWriter.StartWriteBlocks(writeParameters);
        }

        public void Wait()
        {
            if (!_writeEndEvent.IsEnded())
                _writeEndEvent.WaitOne();
        }

        public DataBlockParallelProcessor(
            BlocksReaderAbstract blocksReader, 
            IBlockProcessor blockProcessor,
            BlockWriterAbstract blockWriter,
            string sourcePath,
            string destinationPath,
            int blockSize)
        {
            if (blocksReader == null)
                throw new ArgumentNullException("blocksReader");

            if (blockProcessor == null)
                throw new ArgumentNullException("blockHandler");

            if (blockWriter == null)
                throw new ArgumentNullException("blockWriter");

            if (String.IsNullOrEmpty(sourcePath.Trim()))
                throw new ArgumentException("Путь к файлу для чтения не должен быть null или содержать только пробелы");

            if (String.IsNullOrEmpty(destinationPath.Trim()))
                throw new ArgumentException("Путь к файлу для записи не должен быть null или содержать только пробелы");

            _sourcePath = sourcePath;
            _destinationPath = destinationPath;

            ProcessorsCount = Environment.ProcessorCount;
            _countOfProcessingThread = ProcessorsCount;

            BlockSize = blockSize;

            _blocksReader = blocksReader;
            _blockHandler = blockProcessor;
            _blocksWriter = blockWriter;

            int dataBlockCollectionsSize = GetDataBlockCollectionSize();

            for (int i = 0; i < ProcessorsCount; ++i)
            {
                _processingThreadDataQueueList.Add(new ProcessingThreadBlocksQueue(_readEndEvent, dataBlockCollectionsSize));
                _processThreadsList.Add(new Thread(ProcessDataBlocksQueue));
            }

            _blocksPreparedToWrite = new ProcessedBlocksQueue(_endProcessingEvent, dataBlockCollectionsSize);
        }
    }
}
