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
        private BlocksPreparedToWrite _blocksPreparedToWrite;

        private List<ProcessingThreadData> _processingThreadDataList = new List<ProcessingThreadData>();

        private Hashtable _processedBlockHashtable = Hashtable.Synchronized(new Hashtable());

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
            ProcessingThreadData processThreadData = _processingThreadDataList[threadIndex];

            while (true)
            {
                if (processThreadData.SynchronizedQueue.Count == 0 && !_readEndEvent.IsEnded())
                    processThreadData.WaitOne();

                if (processThreadData.SynchronizedQueue.Count == 0 && _readEndEvent.IsEnded())
                    break;

                DataBlock readBlock = processThreadData.SynchronizedQueue.Dequeue() as DataBlock;
                processThreadData.Reset();

                if (readBlock == null)
                    throw new ArgumentException("в очереди находится объект не типа DataBlock");

                ProcessDataBlock(readBlock);
            }

            --_countOfProcessingThread;
            if (_countOfProcessingThread == 0)
            {
                _endProcessingEvent.SetEndTask();
                _blocksPreparedToWrite.Set();
            }
        }

        private void ProcessDataBlock(DataBlock readBlock)
        {
            var buffer = _blockHandler.Process(readBlock);
            _processedBlockHashtable.Add(readBlock.Number, buffer);

            if (readBlock.Number == _blocksPreparedToWrite.AwaitedKey)
                _blocksPreparedToWrite.Set();
        }

        public void StartProcessing()
        {
            var readParameters = new ReadBlocksParams(_sourcePath, _processingThreadDataList, BlockSize, _readEndEvent);
            _blocksReader.StartReadBlocks(readParameters);

            for (int i = 0; i < ProcessorsCount; ++i)
                _processThreadsList[i].Start(i);

            var writeParameters = new WriteBlocksParams(_destinationPath, _endProcessingEvent, _blocksPreparedToWrite, _writeEndEvent);
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
                _processingThreadDataList.Add(new ProcessingThreadData());
                _processThreadsList.Add(new Thread(ProcessDataBlocksQueue));
            }

            _blocksPreparedToWrite = new BlocksPreparedToWrite(_processedBlockHashtable);
        }
    }
}
