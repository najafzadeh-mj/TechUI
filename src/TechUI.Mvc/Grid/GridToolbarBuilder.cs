namespace TechUI.Mvc.Grid;

public sealed class GridToolbarBuilder
{
    private readonly IList<GridToolbarItem> _items;

    internal GridToolbarBuilder(IList<GridToolbarItem> items)
    {
        _items = items;
    }

    public GridToolbarBuilder Create(
        string text = "Create",
        string? handler = null,
        string? icon = "plus")
    {
        _items.Add(new GridToolbarItem
        {
            Type = GridToolbarItemType.Create,
            Text = text,
            Handler = handler,
            Icon = icon
        });

        return this;
    }

    public GridToolbarBuilder Refresh(
        string text = "Refresh",
        string? icon = "arrow-clockwise")
    {
        _items.Add(new GridToolbarItem
        {
            Type = GridToolbarItemType.Refresh,
            Text = text,
            Icon = icon
        });

        return this;
    }

    public GridToolbarBuilder ColumnChooser(
        string text = "ستون‌ها",
        string? icon = "layout-three-columns")
    {
        _items.Add(new GridToolbarItem
        {
            Type = GridToolbarItemType.ColumnChooser,
            Text = text,
            Icon = icon
        });

        return this;
    }

    /// <summary>
    /// Exports the current page (visible columns only) as a CSV file.
    /// </summary>
    public GridToolbarBuilder Export(
        string text = "خروجی CSV",
        string? icon = "download",
        string? fileName = null)
    {
        _items.Add(new GridToolbarItem
        {
            Type = GridToolbarItemType.Export,
            Text = text,
            Icon = icon,
            FileName = fileName
        });

        return this;
    }

    public GridToolbarBuilder Custom(
        string text,
        string handler,
        string? icon = null)
    {
        _items.Add(new GridToolbarItem
        {
            Type = GridToolbarItemType.Custom,
            Text = text,
            Handler = handler,
            Icon = icon
        });

        return this;
    }
}