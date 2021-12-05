using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SingleSignOn.Api.Services
{
    public class FileStorageService : IStorageService
    {
        private readonly string _contentFolder;
        private const string FOLDER_NAME = "multi-media";

        public FileStorageService(IWebHostEnvironment webHostEnvironment)
        {
            _contentFolder = Path.Combine(webHostEnvironment.WebRootPath, FOLDER_NAME);
        }

        public string GetFileUrl(string fileName)
        {
            return $"/{FOLDER_NAME}/{fileName}";
        }

        public async Task SaveFileAsync(Stream mediaBinaryStream, string fileName)
        {
            if (!Directory.Exists(_contentFolder))
                Directory.CreateDirectory(_contentFolder);

            var filePath = Path.Combine(_contentFolder, fileName);
            using var output = new FileStream(filePath, FileMode.Create);
            await mediaBinaryStream.CopyToAsync(output);
        }

        public async Task DeleteFileAsync(string fileName)
        {
            var filePath = Path.Combine(_contentFolder, fileName);
            if (File.Exists(filePath))
            {
                await Task.Run(() => File.Delete(filePath));
            }
        }
    }
}
