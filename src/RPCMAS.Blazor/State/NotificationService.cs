namespace RPCMAS.Blazor.State;

public enum NotificationKind { Success, Error, Info }

public record Notification(Guid Id, NotificationKind Kind, string Message);

public class NotificationService
{
    private readonly List<Notification> _items = new();

    public IReadOnlyList<Notification> Items => _items;

    public event Action? Changed;

    public void Show(NotificationKind kind, string message)
    {
        var n = new Notification(Guid.NewGuid(), kind, message);
        _items.Add(n);
        Changed?.Invoke();
        _ = AutoDismissAsync(n.Id);
    }

    public void Success(string message) => Show(NotificationKind.Success, message);
    public void Error(string message) => Show(NotificationKind.Error, message);
    public void Info(string message) => Show(NotificationKind.Info, message);

    public void Dismiss(Guid id)
    {
        if (_items.RemoveAll(x => x.Id == id) > 0) Changed?.Invoke();
    }

    private async Task AutoDismissAsync(Guid id)
    {
        await Task.Delay(TimeSpan.FromSeconds(4));
        Dismiss(id);
    }
}
