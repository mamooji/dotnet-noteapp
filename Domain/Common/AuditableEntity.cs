namespace Domain.Common;

public interface IAuditableEntity
{
    string CreatedBy { get; set; }
    DateTime Created { get; set; }
    string LastModifiedBy { get; set; }
    DateTime LastModified { get; set; }
}

public abstract class AuditableEntity : IAuditableEntity
{
    public string CreatedBy { get; set; }
    public DateTime Created { get; set; }
    public string LastModifiedBy { get; set; }
    public DateTime LastModified { get; set; }
}