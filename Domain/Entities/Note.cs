using Domain.Common;

namespace Domain.Entities;

public class Note : AuditableEntity
{
    public int Id { get; set; }
    public string? ApplicationUserId { get; set; }
    public string Title { get; set; }
    public string Body { get; set; }

    public ApplicationUser? ApplicationUser { get; set; }
}