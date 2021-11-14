using System.Threading;
using System.Threading.Tasks;

namespace KOMTracker.API.Infrastructure.Services;

public interface IKomService
{
    Task TrackKomsAsync(CancellationToken cancellationToken);
}
