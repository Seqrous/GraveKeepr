using Terminal.Gui.Drawing;
using Terminal.Gui.Input;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace GraveKeeper.UI;

public class CommandBar : View
{
    private readonly TextField _inputField;

    public event EventHandler<string>? CommandEntered;

    public CommandBar()
    {
        Height = 3;
        CanFocus = true;
        TabStop = TabBehavior.TabStop;
        BorderStyle = LineStyle.Single;

        Label prompt = new() { X = 0, Y = 0, Text = " >" };

        _inputField = new TextField()
        {
            X = Pos.Right(prompt) + 1,
            Y = 0,
            Width = Dim.Fill(),
            TabStop = TabBehavior.TabStop
        };

        _inputField.KeyDown += OnInputKeyDown;

        Add(prompt, _inputField);
    }

    private void OnInputKeyDown(object? _, Key e)
    {
        if (e.KeyCode != Key.Enter) return;
        if (string.IsNullOrWhiteSpace(_inputField.Text)) return;

        CommandEntered?.Invoke(this, _inputField.Text);
    }
}