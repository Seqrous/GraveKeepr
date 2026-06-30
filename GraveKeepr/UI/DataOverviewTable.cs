using GraveKeeper.Domain;
using Terminal.Gui.Drawing;
using Terminal.Gui.Drivers;
using Terminal.Gui.Input;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace GraveKeeper.UI;

public class DataOverviewTable : TableView
{
    private SeriesRowViewCollection? _collection;

    public Action<IReadOnlySet<SeriesRow>>? TombstoneMarkedRowsChanged { get; set; }

    public DataOverviewTable()
    {
        FullRowSelect = true;
        MultiSelect = true;
        CanFocus = false; // disable focus until data is loaded
        Style.ShowHeaders = true;
        Style.AlwaysShowHeaders = true;

        Style.RowColorGetter = rowArgs =>
            _collection?.GetColorScheme(rowArgs.RowIndex, GetAttributeForRole(VisualRole.Normal));
    }

    public void LoadData(IReadOnlyList<SeriesRowViewModel> rows)
    {
        _collection = new SeriesRowViewCollection(rows);
        Table = _collection.ToTableSource();
        CanFocus = true;
        SetNeedsDraw();
    }

    protected override bool OnKeyDown(Key key)
    {
        switch (key.KeyCode)
        {
            case KeyCode.X:
                ToggleTombstoneMark();
                return true;
            case KeyCode.Tab:
                SuperView?.AdvanceFocus(NavigationDirection.Forward, null);
                return true;
            case KeyCode.Tab | KeyCode.ShiftMask:
                SuperView?.AdvanceFocus(NavigationDirection.Backward, null);
                return true;
        }

        return base.OnKeyDown(key);
    }

    private void ToggleTombstoneMark()
    {
        var selectedRows = GetAllSelectedCells().Select(c => c.Y).Distinct();
        _collection!.ToggleMark(selectedRows);
        SetNeedsDraw();
        TombstoneMarkedRowsChanged?.Invoke(_collection.MarkedRows);
    }
}