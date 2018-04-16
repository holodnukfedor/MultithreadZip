using System;
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

namespace ZipVeeamTest
{
    public class DataBlockParallelProcessor
    {
        private ProcessedBlocksCollection _blocksPreparedToWrite;

        private List<ProcessingThreadDataQueue> _processingThreadDataQueueList = new List<ProcessingThreadDataQueue>();

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

        private void ProcessDataBlocksQueue(object threadIndexObject)
        {
            int threadIndex = (int)threadIndexObject;
            ProcessingThreadDataQueue processingThreadDataQueue = _processingThreadDataQueueList[threadIndex];

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
                _blocksPreparedToWrite.AwakeTheWaitings();
            }
        }

        private void ProcessDataBlock(DataBlock readBlock)
        {
            var buffer = _blockHandler.Process(readBlock);
            _blocksPreparedToWrite.Add(readBlock.Number, buffer);
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

            for (int i = 0; i < ProcessorsCount; ++i)
            {
                _processingThreadDataQueueList.Add(new ProcessingThreadDataQueue(_readEndEvent));
                _processThreadsList.Add(new Thread(ProcessDataBlocksQueue));
            }

            _blocksPreparedToWrite = new ProcessedBlocksCollection(_endProcessingEvent);
        }
    }
}
