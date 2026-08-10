using FluentResults;
using FluentValidation;
using KomTracker.Application.Errors;
using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Domain.Entities.Bike;
using MediatR;

namespace KomTracker.Application.Commands.Bike;

/// <summary>Creates a bike when <see cref="Id"/> is null, otherwise updates the existing one.</summary>
public class SaveBikeCommand : IRequest<Result<BikeEntity>>
{
    // Server-owned (set by the controller from route/claims), not part of the request body.
    public int? Id { get; set; }
    public string UserId { get; set; } = default!;

    // Editable fields — mirror SaveBikeViewModel (parity test guards drift).
    public string Name { get; set; } = default!;
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public BikeType Type { get; set; }
    public decimal? WeightKg { get; set; }
    public string? Notes { get; set; }
    public decimal? Price { get; set; }
    public string? PurchasePlace { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public decimal InitialDistanceKm { get; set; }
    public decimal? InitialMovingHours { get; set; }
    public decimal? InitialElevationM { get; set; }
}

public class SaveBikeCommandValidator : AbstractValidator<SaveBikeCommand>
{
    public SaveBikeCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Brand).MaximumLength(200);
        RuleFor(x => x.Model).MaximumLength(200);
        RuleFor(x => x.PurchasePlace).MaximumLength(200);
        RuleFor(x => x.Notes).MaximumLength(2000);
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.WeightKg).GreaterThan(0).When(x => x.WeightKg.HasValue);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0).When(x => x.Price.HasValue);
        RuleFor(x => x.InitialDistanceKm).GreaterThanOrEqualTo(0);
        RuleFor(x => x.InitialMovingHours).GreaterThanOrEqualTo(0).When(x => x.InitialMovingHours.HasValue);
        RuleFor(x => x.InitialElevationM).GreaterThanOrEqualTo(0).When(x => x.InitialElevationM.HasValue);
    }
}

public class SaveBikeCommandHandler : IRequestHandler<SaveBikeCommand, Result<BikeEntity>>
{
    private readonly IKOMUnitOfWork _komUoW;

    public SaveBikeCommandHandler(IKOMUnitOfWork komUoW)
    {
        _komUoW = komUoW ?? throw new ArgumentNullException(nameof(komUoW));
    }

    public async Task<Result<BikeEntity>> Handle(SaveBikeCommand request, CancellationToken cancellationToken)
    {
        var repo = _komUoW.GetRepository<IBikeRepository>();

        BikeEntity bike;

        if (request.Id is null)
        {
            bike = new BikeEntity { UserId = request.UserId, Lifecycle = BikeLifecycle.Active };
            Apply(request, bike);
            repo.AddBike(bike);
        }
        else
        {
            var existing = await repo.GetBikeAsync(request.Id.Value);

            if (existing is null)
            {
                return Result.Fail(new NotFoundError($"Bike {request.Id} not found."));
            }

            if (existing.UserId != request.UserId)
            {
                return Result.Fail(new ForbiddenError("Bike does not belong to the current user."));
            }

            Apply(request, existing);
            repo.UpdateBike(existing);
            bike = existing;
        }

        await _komUoW.SaveChangesAsync();

        return Result.Ok(bike);
    }

    private static void Apply(SaveBikeCommand request, BikeEntity bike)
    {
        bike.Name = request.Name;
        bike.Brand = request.Brand;
        bike.Model = request.Model;
        bike.Type = request.Type;
        bike.WeightKg = request.WeightKg;
        bike.Notes = request.Notes;
        bike.Price = request.Price;
        bike.PurchasePlace = request.PurchasePlace;
        bike.PurchaseDate = BikeDateHelper.EnsureUtc(request.PurchaseDate);
        bike.InitialDistanceKm = request.InitialDistanceKm;
        bike.InitialMovingHours = request.InitialMovingHours;
        bike.InitialElevationM = request.InitialElevationM;
    }
}
