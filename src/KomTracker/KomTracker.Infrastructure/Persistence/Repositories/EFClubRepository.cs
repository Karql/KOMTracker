using EFCore.BulkExtensions;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Domain.Entities.Club;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MoreLinq.Extensions.FullGroupJoinExtension;
using static MoreLinq.Extensions.ForEachExtension;
using Microsoft.EntityFrameworkCore;
using KomTracker.Domain.Entities.Athlete;

namespace KomTracker.Infrastructure.Persistence.Repositories;
public class EFClubRepository : EFBaseRepository, IClubRepository
{
    // Ugly fest
    // Columns from: ClubEntityTypeConfiguration
    private static readonly IEnumerable<string> ClubEntityColumnsToCompare = new[] {
        "name",
        "profile_medium",
        "profile",
        "cover_photo",
        "cover_photo_small",
        "activity_types_icon",
        "sport_type",
        "city",
        "state",
        "country",
        "private",
        "member_count",
        "featured",
        "verified",
        "url"
    };

    public async Task AddOrUpdateClubsAsync(IEnumerable<ClubEntity> clubs)
    {
        SetAuidtCD(clubs);
        SetAuidtMD(clubs);

        await _context.BulkInsertOrUpdateAsync(clubs.ToList(), new BulkConfig
        {            
            PropertiesToIncludeOnCompare = new List<string> { nameof(ClubEntity.Name) },
            PropertiesToExcludeOnUpdate = new List<string> { nameof(ClubEntity.AuditCD) },

            // PropertiesToExcludeOnCompare not working for postgresql 
            // PropertiesToExcludeOnCompare = new List<string> { nameof(ClubEntity.AuditCD), nameof(ClubEntity.AuditMD) },
            OnConflictUpdateWhereSql = (string existingTablePrefix, string insertedTablePrefix) =>
                string.Join(" or ", ClubEntityColumnsToCompare.Select(x => $"{insertedTablePrefix}.\"{x}\" != {existingTablePrefix}.\"{x}\""))

        });
    }

    public async Task SyncAthleteClubsAsync(int athleteId, IEnumerable<ClubEntity> clubs)
    {
        var dbClubs = await _context.AthleteClub.Where(x => x.AthleteId == athleteId).ToListAsync();

        dbClubs.FullGroupJoin(clubs,
            x => x.ClubId,
            y => y.Id,
            (key, x, y) => new { dbClub = x.FirstOrDefault(), club = y.FirstOrDefault() }
        ).ForEach(x =>
        {
            if (x.dbClub == null)
            {
                _context.AthleteClub.Add(new AthleteClubEntity
                {
                    AthleteId = athleteId,
                    ClubId = x.club.Id
                });
            }

            else if (x.club == null)
            {
                _context.AthleteClub.Remove(x.dbClub);
            }
        });
    }

    public async Task<IEnumerable<ClubEntity>> GetAthleteClubsAsync(int athleteId)
    {
        return await _context
            .Athlete
            .Where(x => x.AthleteId == athleteId)
            .SelectMany(x => x.Clubs)
            .OrderBy(x => x.Name)
            .ToListAsync();

    }
}
