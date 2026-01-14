using ClosedXML.Excel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.Data;
using System.Drawing;
using System.Reflection;

namespace Tools.Common
{
    public class ExcelHelper
    {
        private readonly IWebHostEnvironment _env;
        private const string ExcelFolder = "ExcelList";

        public ExcelHelper(IWebHostEnvironment env)
        {
            _env = env;
        }

        #region ========== 导出 Excel ==========

        /// <summary>
        /// 导出 Excel 到 wwwroot/ExcelList 并返回访问 URL
        /// </summary>
        public string ExportToWebRoot<T>(
            IEnumerable<T> data,
            string? fileName = null,
            string sheetName = "Sheet1")
        {
            if (data == null || !data.Any())
                throw new ArgumentException("导出数据不能为空");

            fileName ??= $"export_{DateTime.Now:yyyyMMddHHmmss}.xlsx";

            var folderPath = Path.Combine(_env.WebRootPath, ExcelFolder);
            Directory.CreateDirectory(folderPath);

            var filePath = Path.Combine(folderPath, fileName);

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add(sheetName);

            var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            // 表头
            for (int i = 0; i < props.Length; i++)
            {
                ws.Cell(1, i + 1).Value = props[i].Name;
            }

            // 数据
            int row = 2;
            foreach (var item in data)
            {
                for (int col = 0; col < props.Length; col++)
                {
                    var value = props[col].GetValue(item);
                    ws.Cell(row, col + 1).Value = value?.ToString() ?? "";
                }
                row++;
            }

            ws.Columns().AdjustToContents();

            workbook.SaveAs(filePath);

            return $"/{ExcelFolder}/{fileName}";
        }

        #endregion

        #region ========== 导入 Excel ==========

        /// <summary>
        /// 从 Excel 读取数据
        /// </summary>
        public List<T> Import<T>(IFormFile file) where T : new()
        {
            var list = new List<T>();

            using var stream = file.OpenReadStream();
            using var workbook = new XLWorkbook(stream);
            var ws = workbook.Worksheets.First();

            var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            var rows = ws.RowsUsed().Skip(1); // 跳过表头

            foreach (var row in rows)
            {
                var obj = new T();

                for (int i = 0; i < props.Length; i++)
                {
                    var cellValue = row.Cell(i + 1).GetString();
                    if (string.IsNullOrWhiteSpace(cellValue)) continue;

                    var propType = Nullable.GetUnderlyingType(props[i].PropertyType)
                                   ?? props[i].PropertyType;

                    var value = Convert.ChangeType(cellValue, propType);
                    props[i].SetValue(obj, value);
                }

                list.Add(obj);
            }

            return list;
        }

        #endregion

        #region ========== 下载 Excel ==========

        /// <summary>
        /// 根据文件名返回下载流
        /// </summary>
        public  FileStream Download(string fileName)
        {
            var path = Path.Combine(_env.WebRootPath, ExcelFolder, fileName);

            if (!File.Exists(path))
                throw new FileNotFoundException("Excel 文件不存在");

            return new FileStream(path, FileMode.Open, FileAccess.Read);
        }

        #endregion
    }
}
