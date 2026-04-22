using System.Net.Http.Json;
using System.Text.Json;
using RPCMAS.Core.Enums;

namespace RPCMAS.Blazor.Api;

public class ApiClient
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web)
    {
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    private readonly HttpClient _http;

    public ApiClient(HttpClient http) => _http = http;

    public Task<PagedResponse<ItemDto>?> ListItemsAsync(string? search, int? departmentId, int page, int pageSize, CancellationToken ct = default)
    {
        var qs = $"?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(search)) qs += $"&search={Uri.EscapeDataString(search)}";
        if (departmentId.HasValue) qs += $"&departmentId={departmentId.Value}";
        return GetAsync<PagedResponse<ItemDto>>($"api/v1/items{qs}", ct);
    }

    public Task<ItemDto?> GetItemAsync(int id, CancellationToken ct = default)
        => GetAsync<ItemDto>($"api/v1/items/{id}", ct);

    public Task<PagedResponse<PriceChangeRequestSummaryDto>?> ListRequestsAsync(
        string? requestNumber, RequestStatus? status, int? departmentId, ChangeType? changeType,
        DateTime? fromDate, DateTime? toDate, int page, int pageSize, CancellationToken ct = default)
    {
        var parts = new List<string> { $"page={page}", $"pageSize={pageSize}" };
        if (!string.IsNullOrWhiteSpace(requestNumber)) parts.Add($"requestNumber={Uri.EscapeDataString(requestNumber)}");
        if (status.HasValue) parts.Add($"status={status}");
        if (departmentId.HasValue) parts.Add($"departmentId={departmentId}");
        if (changeType.HasValue) parts.Add($"changeType={changeType}");
        if (fromDate.HasValue) parts.Add($"fromDate={fromDate:O}");
        if (toDate.HasValue) parts.Add($"toDate={toDate:O}");
        return GetAsync<PagedResponse<PriceChangeRequestSummaryDto>>(
            $"api/v1/price-change-requests?{string.Join('&', parts)}", ct);
    }

    public Task<PriceChangeRequestDto?> GetRequestAsync(int id, CancellationToken ct = default)
        => GetAsync<PriceChangeRequestDto>($"api/v1/price-change-requests/{id}", ct);

    public Task<PriceChangeRequestDto?> CreateRequestAsync(CreateRequestDto dto, CancellationToken ct = default)
        => PostAsync<CreateRequestDto, PriceChangeRequestDto>("api/v1/price-change-requests", dto, ct);

    public Task<PriceChangeRequestDto?> UpdateRequestAsync(int id, UpdateRequestDto dto, CancellationToken ct = default)
        => PutAsync<UpdateRequestDto, PriceChangeRequestDto>($"api/v1/price-change-requests/{id}", dto, ct);

    public Task SubmitAsync(int id, string rowVersion, CancellationToken ct = default)
        => PostNoContentAsync($"api/v1/price-change-requests/{id}/submit", new WorkflowActionDto { RowVersion = rowVersion }, ct);

    public Task ApproveAsync(int id, string rowVersion, CancellationToken ct = default)
        => PostNoContentAsync($"api/v1/price-change-requests/{id}/approve", new WorkflowActionDto { RowVersion = rowVersion }, ct);

    public Task RejectAsync(int id, string rowVersion, string reason, CancellationToken ct = default)
        => PostNoContentAsync($"api/v1/price-change-requests/{id}/reject", new RejectActionDto { RowVersion = rowVersion, Reason = reason }, ct);

    public Task ApplyAsync(int id, string rowVersion, CancellationToken ct = default)
        => PostNoContentAsync($"api/v1/price-change-requests/{id}/apply", new WorkflowActionDto { RowVersion = rowVersion }, ct);

    public Task CancelAsync(int id, string rowVersion, CancellationToken ct = default)
        => PostNoContentAsync($"api/v1/price-change-requests/{id}/cancel", new WorkflowActionDto { RowVersion = rowVersion }, ct);

    public Task<List<DepartmentLookup>?> GetDepartmentsAsync(CancellationToken ct = default)
        => GetAsync<List<DepartmentLookup>>("api/v1/lookups/departments", ct);

    public Task<List<UserLookup>?> GetUsersAsync(CancellationToken ct = default)
        => GetAsync<List<UserLookup>>("api/v1/lookups/users", ct);

    public Task<List<EnumLookup>?> GetChangeTypesAsync(CancellationToken ct = default)
        => GetAsync<List<EnumLookup>>("api/v1/lookups/change-types", ct);

    public Task<List<EnumLookup>?> GetStatusesAsync(CancellationToken ct = default)
        => GetAsync<List<EnumLookup>>("api/v1/lookups/statuses", ct);

    private async Task<T?> GetAsync<T>(string url, CancellationToken ct)
    {
        var res = await _http.GetAsync(url, ct);
        await EnsureSuccessAsync(res, ct);
        return await res.Content.ReadFromJsonAsync<T>(JsonOpts, ct);
    }

    private async Task<TRes?> PostAsync<TReq, TRes>(string url, TReq body, CancellationToken ct)
    {
        var res = await _http.PostAsJsonAsync(url, body, JsonOpts, ct);
        await EnsureSuccessAsync(res, ct);
        return await res.Content.ReadFromJsonAsync<TRes>(JsonOpts, ct);
    }

    private async Task<TRes?> PutAsync<TReq, TRes>(string url, TReq body, CancellationToken ct)
    {
        var res = await _http.PutAsJsonAsync(url, body, JsonOpts, ct);
        await EnsureSuccessAsync(res, ct);
        return await res.Content.ReadFromJsonAsync<TRes>(JsonOpts, ct);
    }

    private async Task PostNoContentAsync<TReq>(string url, TReq body, CancellationToken ct)
    {
        var res = await _http.PostAsJsonAsync(url, body, JsonOpts, ct);
        await EnsureSuccessAsync(res, ct);
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage res, CancellationToken ct)
    {
        if (res.IsSuccessStatusCode) return;

        string? message = null;
        string? code = null;
        var fieldErrors = new List<string>();

        try
        {
            using var doc = JsonDocument.Parse(await res.Content.ReadAsStringAsync(ct));
            var root = doc.RootElement;
            if (root.TryGetProperty("detail", out var d)) message = d.GetString();
            if (root.TryGetProperty("code", out var c)) code = c.GetString();
            if (root.TryGetProperty("errors", out var errs) && errs.ValueKind == JsonValueKind.Array)
            {
                foreach (var e in errs.EnumerateArray())
                {
                    if (e.TryGetProperty("errorMessage", out var em)) fieldErrors.Add(em.GetString() ?? "");
                }
            }
        }
        catch { /* leave message null */ }

        throw new ApiException(
            (int)res.StatusCode,
            message ?? res.ReasonPhrase ?? "Request failed",
            code,
            fieldErrors);
    }
}
