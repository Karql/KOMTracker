using System.Threading;
using System.Threading.Tasks;

namespace KomTracker.Application.Services;

public interface IKomService
{
    Task TrackKomsAsync(CancellationToken cancellationToken);
}
