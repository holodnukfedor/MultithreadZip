using System;
using System.IO;
using System.Diagnostics;

namespace ZipVeeamTest
{
    class Program
    {
        private const int _megabyteCoefficient = 1048576;

        private static bool ContinueDialog()
        {
            Console.WriteLine("Вы уверены, что хотите продолжить? y - да, любой другой символ либо последовательность символов - нет");

            string answer = Console.ReadLine();
            if (answer.Trim().ToLower() == "y")
                return true;

            return false;
        }

        static void Main(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("В программу необходимо послать 3 аргумента по шаблону: compress/decompress [имя исходного файла] [имя результирующего файла]");
                return;
            }

            string command = args[0].Trim().ToLower();
            string sourceFileName = args[1].Trim();
            string destinationFileName = args[2].Trim();
            bool needZip;

            if (command == "compress")
            {
                needZip = true;
            }
            else if (command == "decompress")
            {
                needZip = false;
            }
            else
            {
                Console.WriteLine("Первым аргументом введена неизвестная команда. ");
                Console.WriteLine("В программу необходимо послать 3 аргумента по шаблону: compress/decompress [имя исходного файла] [имя результирующего файла].");
                Console.WriteLine("программа принимает две команды compress (сжатие исходного файла) или decompress (распаковка исходного файла)");
                return;
            }

            var sourceFileInfo = new FileInfo(sourceFileName);

            if (!sourceFileInfo.Exists)
            {
                Console.WriteLine("Не существует исходного файла с заданным именем: {0}", sourceFileName);
                return;
            }

            try
            {
                using (var testSourceFilePermission = sourceFileInfo.OpenRead())
                {

                }
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine("У вас нет прав для чтения исходного файла: {0}", sourceFileName);
                Console.WriteLine("Обратитест к администратору компьютера за получением прав или используйте файл для которого у вас есть права на чтение");
                return;
            }

            if (sourceFileName.Length == 0)
            {
                Console.WriteLine("Не имеет смысла производить какие либо операции над пустым входным файлом: {0}", sourceFileName);
                return;
            }

            var destinationFileInfo = new FileInfo(destinationFileName);
            if (destinationFileInfo.Exists)
            {
                Console.WriteLine("Существует файл с именем аналогичным имени выходного файла {0}, он будет перезаписан", destinationFileName);

                if (!ContinueDialog())
                    return;
            }

            try
            {
                using (var testDestFilePermission = destinationFileInfo.OpenWrite())
                {

                }
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine("У вас нет прав для записи в выходной файл: {0}", destinationFileName);
                Console.WriteLine("Обратитест к администратору компьютера за получением прав или используйте файл для которого у вас есть права на запись");
                return;
            }

            string destinationFileDriveName = Path.GetPathRoot(destinationFileInfo.FullName);
            var destinatinoDriveInfo = new DriveInfo(destinationFileDriveName);

            if (destinatinoDriveInfo.AvailableFreeSpace <= sourceFileInfo.Length * 1.5)
            {
                Console.WriteLine("Размер исходного файла {0} - {1} байт ({2} Мб)", sourceFileInfo.Name, sourceFileInfo.Length, sourceFileInfo.Length / _megabyteCoefficient);

                Console.WriteLine("Размер свободного доступного пространства на указанном жестком диске {0} - {1} байт ({2} Мб)", destinationFileDriveName, destinatinoDriveInfo.AvailableFreeSpace, destinatinoDriveInfo.AvailableFreeSpace / _megabyteCoefficient);

                Console.WriteLine("Размер свободного пространства на жестком диске менее чем в полтора раза меньше исходного файла есть вероятность, что не хватит места для выполнения операции. ");

                Console.WriteLine("При компресии если файл уже имеет сжатую структуру, размер файла может увеличиться. Поскольку так работает GZipStream и он был обозначен в техническом задании.");

                Console.WriteLine("А при декомпресии логично, что файл может увеличиться также.");

                if (!ContinueDialog())
                    return;
            }

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            try
            {
                if (needZip)
                    GZipMultithreadProcessor.Zip(sourceFileName, destinationFileName);
                else
                    GZipMultithreadProcessor.UnZip(sourceFileName, destinationFileName);

                stopWatch.Stop();
                TimeSpan ts = stopWatch.Elapsed;
                Console.WriteLine("Алгоритм отработал за {0} секунд", ts.TotalSeconds);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Произошла ошибка: {0}", ex.Message);
            }
        }
    }
}
