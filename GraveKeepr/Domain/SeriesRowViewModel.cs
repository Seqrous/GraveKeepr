namespace GraveKeeper.Domain;

public class SeriesRowViewModel(SeriesRow row, RowKind kind)
{
    public SeriesRow Row { get; } = row;
    public RowKind Kind { get; } = kind;
    public bool IsMarkedForTombstone { get; private set; }

    public void ToggleTombstoneMark()
    {
        if (Kind == RowKind.Normal)
            IsMarkedForTombstone = !IsMarkedForTombstone;
        else
            IsMarkedForTombstone = false;
    }
}
