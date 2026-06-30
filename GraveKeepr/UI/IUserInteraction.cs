namespace GraveKeeper.UI;

public interface IUserInteraction
{
    Task<bool> ConfirmAsync(string message);
    void InformAsync(string title, string message);
    void ErrorAsync(string title, string message);
    void ShowTextAsync(string title, string message);
}