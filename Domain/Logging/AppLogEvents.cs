using Microsoft.Extensions.Logging;

namespace Domain.Logging;

public static class AppLogEvents
{
    public static EventId SecurityAudit = new(1000, "Security Audit");
}