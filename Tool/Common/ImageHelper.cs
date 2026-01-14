using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools.Common
{
    public class ImageHelper
    {
        private readonly IWebHostEnvironment _env;

        public ImageHelper(IWebHostEnvironment env)
        {
            _env = env;
        }

        /// <summary>
        /// 确保目录存在，不存在则创建
        /// </summary>
        private string EnsureFolder(string folder)
        {
            var fullPath = Path.Combine(_env.WebRootPath, folder);

            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }

            return fullPath;
        }

        /// <summary>
        /// 保存 Base64 图片
        /// </summary>
        public string SaveBase64Image(
            string base64,
            string folder = "images",
            string? fileName = null)
        {
            if (string.IsNullOrWhiteSpace(base64))
                throw new ArgumentException("Base64 不能为空");

            // 处理 data:image/png;base64,...
            if (base64.Contains(","))
                base64 = base64.Split(',')[1];

            var bytes = Convert.FromBase64String(base64);
            fileName ??= $"{Guid.NewGuid():N}.png";

            var dir = EnsureFolder(folder);
            var filePath = Path.Combine(dir, fileName);

            File.WriteAllBytes(filePath, bytes);

            return $"/{folder}/{fileName}";
        }

        /// <summary>
        /// 保存上传图片
        /// </summary>
        public async Task<string> SaveImageAsync(
            IFormFile file,
            string folder = "images")
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("文件为空");

            var ext = Path.GetExtension(file.FileName);
            var fileName = $"{Guid.NewGuid():N}{ext}";

            var dir = EnsureFolder(folder);
            var filePath = Path.Combine(dir, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"/{folder}/{fileName}";
        }
    }
}
