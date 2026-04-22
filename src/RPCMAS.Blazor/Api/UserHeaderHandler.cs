using RPCMAS.Blazor.State;

namespace RPCMAS.Blazor.Api;

public class UserHeaderHandler : DelegatingHandler
{
    public const string HeaderName = "X-User-Id";

    private readonly CurrentUserState _state;

    public UserHeaderHandler(CurrentUserState state) => _state = state;

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        if (_state.UserId.HasValue)
        {
            request.Headers.Remove(HeaderName);
            request.Headers.Add(HeaderName, _state.UserId.Value.ToString());
        }
        return base.SendAsync(request, ct);
    }
}
