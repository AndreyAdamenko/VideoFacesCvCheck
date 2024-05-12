using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoCheck
{
    class VideoInfo
    {
        public string FileName { get; }
        public int NumberOfFaces { get; set; }
        public long Size { get; }

        public VideoInfo(string fileName)
        {
            FileName = fileName;
            NumberOfFaces = 0; // Изначально количество лиц устанавливаем в 0
            Size = GetFileSize(fileName);
        }

        private long GetFileSize(string fileName)
        {
            try
            {
                // Получаем размер файла в байтах
                FileInfo fileInfo = new FileInfo(fileName);
                return fileInfo.Length;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении размера файла {fileName}: {ex.Message}");
                return 0;
            }
        }
    }
}
