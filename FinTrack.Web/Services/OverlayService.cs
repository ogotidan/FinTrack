namespace FinTrack.Web.Services;

public class OverlayService
{
    public event Action? OnChange;

    public bool Visible { get; private set; }
    public string State { get; private set; } = "spinner";
    public string Message { get; private set; } = "";

    public void ShowSaving(string message = "Working…") => Set(true, "spinner", message);

    public async Task ShowSuccess(string message, int holdMs = 1100)
    {
        Set(true, "success", message);
        await Task.Delay(holdMs);
        Hide();
    }

    public async Task ShowError(string message, int holdMs = 1900)
    {
        Set(true, "error", message);
        await Task.Delay(holdMs);
        Hide();
    }

    public void Hide()
    {
        Visible = false;
        OnChange?.Invoke();
    }

    private void Set(bool visible, string state, string message)
    {
        Visible = visible;
        State = state;
        Message = message;
        OnChange?.Invoke();
    }
}
