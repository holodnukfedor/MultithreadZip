using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO.Compression;
using System.IO;
using System.Diagnostics;
using ZipVeeamTest.DataBlocksParallelProcessing.BlockWriters;
using ZipVeeamTest.DataBlocksParallelProcessing.BlockReadres;
using ZipVeeamTest.DataBlocksParallelProcessing.BlockReadres.ProcessingThreadChooseAlg;
using ZipVeeamTest.DataBlocksParallelProcessing.BlockHandler;

namespace ZipVeeamTest
{
    class Program
    {
        private static void ZipFile(string sourceFileName, string destinationFileName)
        {
            using (FileStream originalFile = File.OpenRead(sourceFileName))
            {
                using (FileStream compressedFileStream = File.Create(destinationFileName))
                {
                    using (GZipStream compressionStream = new GZipStream(compressedFileStream, CompressionMode.Compress))
                    {
                        byte[] buffer = new byte[originalFile.Length];
                        int bytesToRead = originalFile.Read(buffer, 0, (int)originalFile.Length);
                        compressionStream.Write(buffer, 0, bytesToRead);
                    }
                }
            }
        }

        private static void UnzipFile(string sourceFileName, string destinationFileName)
        {
            using (FileStream originalFile = File.OpenRead(sourceFileName))
            {
                using (FileStream uncompressedFileStream = File.Create(destinationFileName))
                {
                    using (GZipStream compressedFileStream = new GZipStream(originalFile, CompressionMode.Decompress))
                    { 
                        byte[] buffer = new byte[BlockSize];
                        int bytesToWrite = compressedFileStream.Read(buffer, 0, BlockSize);
                        uncompressedFileStream.Write(buffer, 0, bytesToWrite);
                    }
                }
            }
        }

        private const int BlockSize = 1048576;

        public static void Decompress(string inFileName, string outFileName)
        {
            using (var inFileStream = new FileStream(inFileName, FileMode.Open, FileAccess.Read))
            {
                using (FileStream uncompressedFileStream = File.Create(outFileName))
                {
                    int index = 1;

                    List<byte> buffer = new List<byte>(BlockSize);

                    byte[] firstThreeBytes = new byte[3];
                    inFileStream.Read(firstThreeBytes, 0, 3);
                    for (int i = 0; i < 3; ++i)
                        buffer.Add(firstThreeBytes[i]);

                    while (inFileStream.Position < inFileStream.Length)
                    {
                        byte first = (byte) inFileStream.ReadByte();
                        if (first == 31)
                        {
                            byte second = (byte)inFileStream.ReadByte();
                            if (second == 139)
                            {
                                byte third = (byte)inFileStream.ReadByte();
                                if (third == 8)
                                {
                                    using (var memStream = new MemoryStream(buffer.ToArray()))
                                    {
                                        using (var gzipStream = new GZipStream(memStream, CompressionMode.Decompress))
                                        {
                                            Console.WriteLine(++index);
                                            byte[] buffer2 = new byte[BlockSize];
                                            int bytesToWrite = gzipStream.Read(buffer2, 0, BlockSize);
                                            uncompressedFileStream.Write(buffer2, 0, bytesToWrite);
                                        }
                                    }
                                    buffer.Clear();
                                }
                                buffer.Add(first);
                                buffer.Add(second);
                                buffer.Add(third);
                            }
                            else
                            {
                                buffer.Add(first);
                                buffer.Add(second);
                            }
                        }
                        else
                        {
                            buffer.Add(first);
                        }
                    }
                }
            }
        }

        public static void DecompressWithLengthFirst(string inFileName, string outFileName)
        {
            int index = 1;
            using (var inFileStream = new FileStream(inFileName, FileMode.Open, FileAccess.Read))
            {
                using (FileStream uncompressedFileStream = File.Create(outFileName))
                {
                    while (inFileStream.Position < inFileStream.Length)
                    {
                        Console.WriteLine(index++);
                        var lengthBuffer = new byte[4];
                        inFileStream.Read(lengthBuffer, 0, lengthBuffer.Length);
                        var blockLength = BitConverter.ToInt32(lengthBuffer, 0);
                        var buffer = new byte[blockLength];

                        inFileStream.Read(buffer, 0, buffer.Length);
                        using (var memStream = new MemoryStream(buffer))
                        {
                            using (var gzipStream = new GZipStream(memStream, CompressionMode.Decompress))
                            {
                                byte[] buffer2 = new byte[BlockSize];
                                int bytesToWrite = gzipStream.Read(buffer2, 0, BlockSize);
                                uncompressedFileStream.Write(buffer2, 0, bytesToWrite);
                            }
                        }
                    }
                }
            }
        }

        static void Main(string[] args)
        {
            string sourceFileName = @"Star.Wars.Rebels.S04E16.rus.LostFilm.TV.avi";
            string destinationFileName = @"Star.Wars.Rebels.S04E16.rus.LostFilm.TV.avi.gz";

            var processorUnitCount = Environment.ProcessorCount;
            var availiableMemory = Convert.ToInt64(new PerformanceCounter("Memory", "Available MBytes").NextValue() * BlockSize);

            Stopwatch stopWatch = new Stopwatch();
            Console.WriteLine("Количество ядер процессора: " + processorUnitCount);
            Console.WriteLine("Количество свободной оперативной памяти (Б): " + availiableMemory);
            stopWatch.Start();

            //DecompressWithLengthFirst(destinationFileName, sourceFileName);
            //UnzipFile(destinationFileName, sourceFileName);
            //Decompress(destinationFileName, sourceFileName);

            //var dataBlockParallelProcessor =
            //    new DataBlockParallelProcessor<MinLoadedChoose, SimpleBlocksReader<MinLoadedChoose>, BlocksWriterWithLength>(new GZipCompressHandler(), sourceFileName, destinationFileName, BlockSize);

            var dataBlockParallelProcessor =
              new DataBlockParallelProcessor(
                  new SimpleBlocksReader(new MinLoadedThreadChooseAlg(), BlockSize),
                  new GZipCompressProcessor(),
                  new BlocksWriterWithLength(),
                  sourceFileName,
                  destinationFileName,
                  BlockSize
                  );

            //var dataBlockParallelProcessor =
            //   new DataBlockParallelProcessor(
            //       new BlocksReaderWithLength(new MinLoadedThreadChooseAlg()),
            //       new GZipDecompressProcessor(BlockSize),
            //       new SimpleBlocksWriter(),
            //       destinationFileName,
            //       sourceFileName,
            //       BlockSize
            //       );

            dataBlockParallelProcessor.StartProcessing();
            dataBlockParallelProcessor.Wait();

            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            Console.WriteLine("Алгоритм отработал за {0} секунд", ts.TotalSeconds);
        }
    }
}
