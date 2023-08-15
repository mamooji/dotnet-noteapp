using Application.Common.Interfaces;
using MediatR;

namespace Application.Weather.Queries;

public class GetWeatherQuery : IRequest<List<string>>
{
}

public class GetWeatherQueryHandler : IRequestHandler<GetWeatherQuery, List<string>>
{
    private readonly IApplicationDbContext _context;

    public GetWeatherQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<string>> Handle(GetWeatherQuery request, CancellationToken cancellationToken)
    {
        // var note = _context.Note.FindAsync()
        var weather = new List<string> { "Hello", "Muhammad's", "World" };
        List<bool> list = _context.Note.Select(n => n.Title == "test").ToList();

        return weather;
    }
}