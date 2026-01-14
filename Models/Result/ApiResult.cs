using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Result
{
    public class ApiResult<T>
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }

        public static ApiResult<T> Ok(T data) =>
            new() { Success = true, Data = data };

        public static ApiResult<T> Fail(string msg) =>
            new() { Success = false, Message = msg };
    }
}
