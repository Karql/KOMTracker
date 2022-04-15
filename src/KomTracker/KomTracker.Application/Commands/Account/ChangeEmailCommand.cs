using FluentResults;
using KomTracker.Application.Interfaces.Services.Mail;
using KomTracker.Application.Models.Mail;
using KomTracker.Application.Services;
using MediatR;
using IIdentityUserService = KomTracker.Application.Interfaces.Services.Identity.IUserService;

namespace KomTracker.Application.Commands.Account;
public class ChangeEmailCommand : IRequest<Result>
{
    public int AthleteId { get; set; }
    public string NewEmail { get; set; } = default!;
}

public class ChangeEmailCommandHandler : IRequestHandler<ChangeEmailCommand, Result>
{
    private readonly IIdentityUserService _userService;
    private readonly IAthleteService _athleteService;
    private readonly IMailService _mailService;

    public ChangeEmailCommandHandler(IIdentityUserService userService, IAthleteService athleteService, IMailService mailService)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _athleteService = athleteService ?? throw new ArgumentNullException(nameof(athleteService));
        _mailService = mailService ?? throw new ArgumentNullException(nameof(mailService));
    }

    public async Task<Result> Handle(ChangeEmailCommand request, CancellationToken cancellationToken)
    {
        var athlete = await _athleteService.GetAthleteAsync(request.AthleteId);

        if (athlete == null)
        {
            return Result.Fail("Failed"); // todo: error handling
        }

        var res = await _userService.GenerateChangeEmailUrlAsync(request.AthleteId, request.NewEmail);

        if (!res.IsSuccess)
        {
            return Result.Fail("Failed"); // todo: error handling
        }

        var url = res.Value;

        var p = new SendChangeEmailConfirmationParamsModel
        {
            To = request.NewEmail,
            FirstName = athlete.FirstName,
            Url = url
        };

        await _mailService.SendChangeEmailConfirmationAsync(p);

        return Result.Ok();
    }
}

