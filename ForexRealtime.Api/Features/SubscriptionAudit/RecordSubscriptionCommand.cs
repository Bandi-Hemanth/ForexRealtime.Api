using MediatR;

namespace ForexRealtime.Api.Features.SubscriptionAudit;

public record RecordSubscriptionCommand(string UserId, string Symbol, string Action) : IRequest<Unit>;
