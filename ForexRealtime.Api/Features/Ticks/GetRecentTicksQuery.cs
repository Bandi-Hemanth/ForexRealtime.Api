using MediatR;
using ForexRealtime.Api.Domain;

namespace ForexRealtime.Api.Features.Ticks;

public record GetRecentTicksQuery(string? Symbol, int Limit = 100) : IRequest<IReadOnlyList<PriceTickDto>>;

public record PriceTickDto(long Id, string Symbol, decimal Price, decimal Bid, decimal Ask, DateTime Ts);
