using MediatR;
using Microsoft.EntityFrameworkCore;
using ForexRealtime.Api.Domain;
using ForexRealtime.Api.Infrastructure.Persistence;

namespace ForexRealtime.Api.Features.Ticks;

public class GetRecentTicksQueryHandler : IRequestHandler<GetRecentTicksQuery, IReadOnlyList<PriceTickDto>>
{
    private readonly ForexDbContext _db;

    public GetRecentTicksQueryHandler(ForexDbContext db) => _db = db;

    public async Task<IReadOnlyList<PriceTickDto>> Handle(GetRecentTicksQuery request, CancellationToken cancellationToken)
    {
        IQueryable<PriceTick> query = _db.PriceTicks.AsNoTracking().OrderByDescending(x => x.Ts);
        if (!string.IsNullOrWhiteSpace(request.Symbol))
            query = query.Where(x => x.Symbol == request.Symbol);

        var list = await query.Take(request.Limit).ToListAsync(cancellationToken);
        return list.Select(x => new PriceTickDto(x.Id, x.Symbol, x.Price, x.Bid, x.Ask, x.Ts)).ToList();
    }
}
