using FluentValidation;
using BadPig.Administration.Core;
using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Administration.Features.CreateSeason;

/// <summary>Creates a season. The end date and the name have defaults; seasons cannot overlap and their names are unique.</summary>
internal sealed class CreateSeasonHandler(IValidator<CreateSeasonRequest> validator, IDateTimeProvider clock, AppDbContext db, IAdministrationAudit audit)
    : IHandler<CreateSeasonRequest, CreatedResponse>
{
    public async Task<Result<CreatedResponse>> HandleAsync(CreateSeasonRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        var startsOn = request.StartsOn!.Value;
        var endsOn = request.EndsOn ?? SeasonRules.DefaultEnd(startsOn);
        var name = string.IsNullOrWhiteSpace(request.Name) ? SeasonRules.DefaultName(startsOn, endsOn) : request.Name.Trim();

        var conflict = SeasonConflicts.Find(await db.Seasons.AsNoTracking().ToListAsync(ct), name, startsOn, endsOn);
        if (conflict is not null)
            return Error.Conflict(conflict);

        var season = new Season
        {
            Id = Guid.NewGuid(),
            Name = name,
            StartsOn = startsOn,
            EndsOn = endsOn,
            GraceDays = request.GraceDays ?? SeasonRules.DefaultGraceDays,
            InternalFee = request.InternalFee!.Value,
            ExternalFee = request.ExternalFee!.Value,
            CreatedAt = clock.UtcNow,
        };
        db.Seasons.Add(season);
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            // Another request created the same name at the same time (unique index).
            return Error.Conflict($"Une saison « {name} » existe déjà.");
        }

        await audit.RecordAsync($"season.created.{name}", null, ct);
        return new CreatedResponse(season.Id.ToString());
    }
}
