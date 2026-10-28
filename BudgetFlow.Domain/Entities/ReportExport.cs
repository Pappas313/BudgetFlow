using BudgetFlow.Domain.Common;

namespace BudgetFlow.Domain.Entities;

/// <summary>
/// Represents a generated report export file requested by an application user.
/// </summary>
public class ReportExport : Entity, IAuditableEntity
{
    public Guid ApplicationUserId { get; private set; }

    public string FileName { get; private set; } = string.Empty;

    public string Format { get; private set; } = string.Empty;

    public string? StoragePath { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset? LastModifiedAtUtc { get; private set; }

    private ReportExport()
    {
    }

    public ReportExport(Guid applicationUserId, string fileName, string format, string? storagePath = null)
    {
        ApplicationUserId = applicationUserId;
        FileName = fileName;
        Format = format;
        StoragePath = storagePath;
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }
}
