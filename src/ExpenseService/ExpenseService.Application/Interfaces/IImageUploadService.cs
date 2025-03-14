using SharedLibrary.Models;
using System.IO;
using System.Threading.Tasks;

namespace ExpenseService.Application.Interfaces;

public interface IImageUploadService
{
    Task<Result<string>> UploadReceiptAsync(int expenseId, Stream imageStream, string fileName);
    Task<Result<byte[]>> GetReceiptAsync(int expenseId, string fileName);
    Task<Result<bool>> DeleteReceiptAsync(int expenseId, string fileName);
}