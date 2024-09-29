using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynoAI.Core.Models.Results
{
    public class UpdateResult<T>
    {
        private UpdateResult(bool isSuccess, string? error)
        {
            if (isSuccess && error is not null)
                throw new ArgumentNullException(nameof(error), "Cannot have a value for a successful result.");
            if (!isSuccess && string.IsNullOrWhiteSpace(error))
                throw new ArgumentNullException(nameof(error), "Error cannot be null or empty for a failed result.");

            IsSuccess = isSuccess;
            Error = error;
        }

        public bool IsSuccess { get; }
        public string? Error { get; }

        public static UpdateResult<T> Success(T result) => new(true, null);
        public static UpdateResult<T> Failure(string error) => new(false, error);
    }
}
