using MediatR;
using ForexRealtime.Api.Domain;
using ForexRealtime.Api.Infrastructure.Persistence;

namespace ForexRealtime.Api.Features.SubscriptionAudit;

public class RecordSubscriptionCommandHandler : IRequestHandler<RecordSubscriptionCommand, Unit>
{
    private readonly ForexDbContext _db;

    public RecordSubscriptionCommandHandler(ForexDbContext db) => _db = db;

    public async Task<Unit> Handle(RecordSubscriptionCommand request, CancellationToken cancellationToken)
    {
        _db.SubscriptionAudits.Add(new ForexRealtime.Api.Domain.SubscriptionAudit
        {
            UserId = request.UserId,
            Symbol = request.Symbol,
            Action = request.Action,
            At = DateTime.UtcNow
        });
        await _db.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
