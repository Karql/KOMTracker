using MediatR;
using IIdentityUserService = KomTracker.Application.Interfaces.Services.Identity.IUserService;

namespace KomTracker.Application.Queries.Account;

/// <summary>The user's preferred display currency (ISO code); falls back to PLN if the user is missing.</summary>
public class GetCurrencyQuery : IRequest<string>
{
    public int AthleteId { get; set; }
}

public class GetCurrencyQueryHandler : IRequestHandler<GetCurrencyQuery, string>
{
    private readonly IIdentityUserService _userService;

    public GetCurrencyQueryHandler(IIdentityUserService userService)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
    }

    public async Task<string> Handle(GetCurrencyQuery request, CancellationToken cancellationToken)
    {
        var user = await _userService.GetUserAsync(request.AthleteId);
        return string.IsNullOrWhiteSpace(user?.Currency) ? "PLN" : user!.Currency;
    }
}
