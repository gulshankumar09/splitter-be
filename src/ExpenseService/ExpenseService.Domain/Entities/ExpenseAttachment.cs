using SharedLibrary.Domain;

namespace ExpenseService.Domain.Entities;

public class ExpenseAttachment : BaseEntity
{
    public string FileName { get; private set; }
    public string FileType { get; private set; }
    public string FileUrl { get; private set; }

    // For EF Core
    private ExpenseAttachment() { }

    public ExpenseAttachment(string fileName, string fileType, string fileUrl)
        : base()
    {
        FileName = fileName;
        FileType = fileType;
        FileUrl = fileUrl;
    }
}