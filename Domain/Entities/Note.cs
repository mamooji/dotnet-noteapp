using Domain.Common;

namespace Domain.Entities;

public class Note : AuditableEntity
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Body { get; set; }

    public User User { get; set; }
    public string UserId { get; set; }
}