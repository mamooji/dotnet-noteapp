using Domain.Common;

namespace Domain.Entities;

public class Note : AuditableEntity
{
    public int Id { get; set; }
    public int ApplicationUserId { get; set; }
    public ApplicationUser ApplicationUser { get; set; } = null!;
    public string Title { get; set; }
    public string Body { get; set; }
}