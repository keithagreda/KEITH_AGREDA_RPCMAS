namespace RPCMAS.Core.Interfaces;

public interface IRequestNumberGenerator
{
    Task<string> NextAsync(DateTime requestDate, CancellationToken ct = default);
}
