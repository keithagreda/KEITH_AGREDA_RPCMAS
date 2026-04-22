using RPCMAS.Core.Interfaces;

namespace RPCMAS.Infrastructure.Services;

public class RequestNumberGenerator : IRequestNumberGenerator
{
    private readonly IPriceChangeRequestRepository _repo;

    public RequestNumberGenerator(IPriceChangeRequestRepository repo) => _repo = repo;

    public async Task<string> NextAsync(DateTime requestDate, CancellationToken ct = default)
    {
        var count = await _repo.CountForDateAsync(requestDate, ct);
        var seq = (count + 1).ToString("D4");
        return $"PCR-{requestDate:yyyyMMdd}-{seq}";
    }
}
