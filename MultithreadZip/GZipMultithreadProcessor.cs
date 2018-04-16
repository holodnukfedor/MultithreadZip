using System;
using System.IO;
using ZipVeeamTest.DataBlocksParallelProcessing;
using ZipVeeamTest.DataBlocksParallelProcessing.BlockProcessor;
using ZipVeeamTest.DataBlocksParallelProcessing.BlockReadres;
using ZipVeeamTest.DataBlocksParallelProcessing.BlockWriters;
using ZipVeeamTest.DataBlocksParallelProcessing.BlockReadres.ProcessingThreadChooseAlg;

namespace ZipVeeamTest
{
    public class GZipMultithreadProcessor
    {
        public const int BlockSize = 1048576;

        private static void CheckArguments(string sourceFileName, string destinationFileName)
        {
            if (String.IsNullOrEmpty(sourceFileName.Trim()))
                throw new ArgumentException("Путь к файлу для чтения не должен быть null или содержать только пробелы");

            if (String.IsNullOrEmpty(destinationFileName.Trim()))
                throw new ArgumentException("Путь к файлу для записи не должен быть null или содержать только пробелы");

            if (!File.Exists(sourceFileName))
                throw new ArgumentException("Не существует файла источника");
        }

        public static void Zip(string sourceFileName, string destinationFileName)
        {
            CheckArguments(sourceFileName, destinationFileName);

            var dataBlockParallelProcessor =
              new DataBlockParallelProcessor(
                  new SimpleBlocksReader(new MinLoadedThreadChooseAlg()),
                  new GZipCompressProcessor(),
                  new BlocksWriterWithLength(),
                  sourceFileName,
                  destinationFileName,
                  BlockSize
                  );

            dataBlockParallelProcessor.StartProcessing();
            dataBlockParallelProcessor.Wait();
        }

        public static void UnZip(string sourceFileName, string destinationFileName)
        {
            CheckArguments(sourceFileName, destinationFileName);

            var dataBlockParallelProcessor =
              new DataBlockParallelProcessor(
                  new BlocksReaderWithLength(new MinLoadedThreadChooseAlg()),
                  new GZipDecompressProcessor(BlockSize),
                  new SimpleBlocksWriter(),
                  sourceFileName,
                  destinationFileName,
                  BlockSize
                  );

            dataBlockParallelProcessor.StartProcessing();
            dataBlockParallelProcessor.Wait();
        }
    }
}
