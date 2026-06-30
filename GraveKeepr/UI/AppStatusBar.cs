using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace GraveKeeper.UI;

public class AppStatusBar : View
{
    private readonly Label _label;

    public AppStatusBar()
    {
        Height = 1;
        CanFocus = true;

        _label = new Label { X = 0, Y = 0, Text = "Series: none | Type: - | Flow: -" };
        var helpHint = new Label { X = Pos.AnchorEnd(7), Y = 0, Text = "? /help" };
        Add(_label, helpHint);
    }
    
    public void Update(string message) => _label.Text = message;
}