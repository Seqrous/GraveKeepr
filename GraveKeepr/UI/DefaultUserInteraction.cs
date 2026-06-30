using System.Text;
using Terminal.Gui.App;
using Terminal.Gui.Drawing;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace GraveKeeper.UI;

public class DefaultUserInteraction(IApplication app) : IUserInteraction
{
    public async Task<bool> ConfirmAsync(string message)
    {
        var tcs = new TaskCompletionSource<int>();
        app.Invoke(() =>
        {
            var confirm = MessageBox.Query(app, "Info", message, "Yes", "No")!.Value;
            tcs.SetResult(confirm);
        });
        return await tcs.Task == 0;
    }

    public void InformAsync(string title, string message)
    {
        app.Invoke(() => MessageBox.Query(app, title, message, "OK"));
    }

    public void ErrorAsync(string title, string message)
    {
        app.Invoke(() => MessageBox.ErrorQuery(app, title, message, "OK"));
    }

    public void ShowTextAsync(string title, string message)
    {
        app.Invoke(() =>
        {
            var dialog = new Dialog
            {
                Title = title,
                Width = Dim.Percent(80),
                Height = Dim.Percent(80),
                ShadowStyle = ShadowStyle.None
            };
            dialog.Padding!.Thickness = new Thickness(1, 1, 1, 0);

            var okButton = new Button { X = Pos.AnchorEnd(), Y = Pos.AnchorEnd(), Title = "OK" };
            okButton.Accepting += (_, _) => app.RequestStop(dialog);

            var label = new Label
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill(1),
                Text = message,
                HotKeySpecifier = new Rune('\xFFFF'), // prevent _ from being consumed as hotkey prefix
                CanFocus = false
            };

            dialog.Add(label, okButton);
            app.Run(dialog);
        });
    }
}