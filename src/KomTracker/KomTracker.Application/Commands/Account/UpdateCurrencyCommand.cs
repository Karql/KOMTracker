using FluentResults;
using FluentValidation;
using MediatR;
using IIdentityUserService = KomTracker.Application.Interfaces.Services.Identity.IUserService;

namespace KomTracker.Application.Commands.Account;

public class UpdateCurrencyCommand : IRequest<Result>
{
    // Server-owned (set by the controller from the route/claims).
    public int AthleteId { get; set; }
    public string Currency { get; set; } = default!;
}

public class UpdateCurrencyCommandValidator : AbstractValidator<UpdateCurrencyCommand>
{
    // The supported set (mirrors the WEB Currencies registry). Kept here so the server validates independently.
    public static readonly string[] AllowedCurrencies = { "CHF", "EUR", "GBP", "USD", "PLN" };

    public UpdateCurrencyCommandValidator()
    {
        RuleFor(x => x.Currency)
            .NotEmpty()
            .Must(c => AllowedCurrencies.Contains(c))
            .WithMessage($"Currency must be one of: {string.Join(", ", AllowedCurrencies)}.");
    }
}

public class UpdateCurrencyCommandHandler : IRequestHandler<UpdateCurrencyCommand, Result>
{
    private readonly IIdentityUserService _userService;

    public UpdateCurrencyCommandHandler(IIdentityUserService userService)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
    }

    public Task<Result> Handle(UpdateCurrencyCommand request, CancellationToken cancellationToken)
        => _userService.UpdateCurrencyAsync(request.AthleteId, request.Currency);
}
