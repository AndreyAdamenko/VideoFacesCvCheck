using System;
using System.IO;
using System.Diagnostics;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing;

class Program
{
    static void Main(string[] args)
    {
        string directory = "F:\\_Все_фото\\20210906_Video";

        string screenDir = Path.Combine(directory, "screens");

        // Получение списка видеофайлов
        string[] videoFiles = GetVideoFiles(directory);

        // Списки для хранения имен видеофайлов с лицами и без
        var filesWithFaces = new List<string>();
        var filesWithoutFaces = new List<string>();

        // Создание скриншотов и определение лиц для каждого видеофайла
        foreach (string videoFile in videoFiles)
        {
            int maxFacesCount;
            string screenshotsDir = CreateScreenshots(videoFile, screenDir);
            bool facesDetected = DetectFacesOnScreenshots(screenshotsDir, out maxFacesCount);

            if (facesDetected)
            {
                filesWithFaces.Add($"{videoFile} -- {maxFacesCount}" );
                Console.WriteLine($"Лица обнаружены на видео: {Path.GetFileName(videoFile)}");
            }
            else
            {
                filesWithoutFaces.Add(videoFile);
                Console.WriteLine($"Лица не обнаружены на видео: {Path.GetFileName(videoFile)}");
            }
        }

        var resultFacesFile = Path.Combine(screenDir, "faces.txt");
        var resultNpFacesFile = Path.Combine(screenDir, "noFaces.txt");

        File.WriteAllLines(resultFacesFile, filesWithFaces);
        File.WriteAllLines(resultNpFacesFile, filesWithoutFaces);
    }

    static string[] GetVideoFiles(string directory)
    {
        // Получаем список видеофайлов в текущей директории с расширением .mp4
        return Directory.GetFiles(directory, "*.mp4");
    }

    static string CreateScreenshots(string videoFile, string screenshotsDir)
    {
        // Создаем папку для сохранения скриншотов, если она не существует
        Directory.CreateDirectory(screenshotsDir);

        // Получаем имя видеофайла без расширения
        string videoFileName = Path.GetFileNameWithoutExtension(videoFile);

        // Создаем папку для скриншотов текущего видеофайла
        string screenshotsSubDir = Path.Combine(screenshotsDir, videoFileName);
        Directory.CreateDirectory(screenshotsSubDir);

        // Интервал между скриншотами (в секундах)
        int interval = 1;

        // Выполняем команду ffmpeg для извлечения скриншотов
        string arguments = $"-i \"{videoFile}\" -vf fps=1/{interval} \"{screenshotsSubDir}\\screenshot_%03d.jpg\"";
        Process.Start("ffmpeg", arguments)?.WaitForExit();

        Console.WriteLine($"Скриншоты извлечены и сохранены в папке {screenshotsSubDir}.");

        return screenshotsSubDir;
    }

    static bool DetectFacesOnScreenshots(string screenshotsDir, out int maxFacesCount)
    {
        bool facesDetected = false;

        maxFacesCount = 0;

        // Подключение каскадного классификатора для детекции лиц
        CascadeClassifier faceCascade = new CascadeClassifier("haarcascade_frontalface_default.xml");

        // Проверяем каждый скриншот в папке
        foreach (string imagePath in Directory.GetFiles(screenshotsDir, "*.jpg"))
        {
            // Загружаем изображение
            using (Image<Bgr, byte> image = new Image<Bgr, byte>(imagePath))
            {
                // Преобразуем изображение в черно-белое
                using (Image<Gray, byte> gray = image.Convert<Gray, byte>())
                {
                    // Детекция лиц
                    Rectangle[] faces = faceCascade.DetectMultiScale(gray, 1.1, 5, new System.Drawing.Size(30, 30));

                    maxFacesCount = maxFacesCount > faces.Length ? maxFacesCount : faces.Length;

                    // Если обнаружены лица, устанавливаем флаг и выходим из цикла
                    if (faces.Length > 0)
                    {
                        facesDetected = true;
                        break;
                    }
                }
            }
        }

        return facesDetected;
    }
}
