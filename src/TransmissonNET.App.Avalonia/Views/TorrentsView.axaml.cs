using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.VisualTree;
using TransmissonNET.App.Avalonia.Features.TorrentTable;
using TransmissonNET.App.Avalonia.ViewModels;
using TransmissonNET.Domain;

namespace TransmissonNET.App.Avalonia.Views;

public partial class TorrentsView : UserControl
{
    private TorrentsViewModel? _viewModel;
    private string? _draggingColumnId;

    public TorrentsView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
        TorrentsGrid.SelectionChanged += OnTorrentsGridSelectionChanged;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (_viewModel is not null)
        {
            _viewModel.TableLayoutChanged -= RebuildColumns;
            _viewModel.SortStateChanged -= UpdateSortHeaders;
        }

        _viewModel = DataContext as TorrentsViewModel;
        if (_viewModel is not null)
        {
            _viewModel.TableLayoutChanged += RebuildColumns;
            _viewModel.SortStateChanged += UpdateSortHeaders;
            RebuildColumns();
            SyncGridSelection();
        }
    }

    private void OnTorrentsGridSelectionChanged(object? sender, SelectionChangedEventArgs e) =>
        SyncGridSelection();

    private void SyncGridSelection()
    {
        if (_viewModel is null)
            return;

        var selected = TorrentsGrid.SelectedItems
            .OfType<TorrentRowViewModel>()
            .ToList();
        _viewModel.SetSelectedTorrents(selected);
    }

    private async void OnRowDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (DataContext is TorrentsViewModel vm && vm.OpenDetailsCommand.CanExecute(null))
            await vm.OpenDetailsCommand.ExecuteAsync(null);
    }

    private void OnDataGridSorting(object? sender, DataGridColumnEventArgs e)
    {
        if (_viewModel is null || e.Column?.Tag is not string columnId)
            return;

        var definition = TorrentTableColumns.Find(columnId);
        if (definition is null || !definition.Sortable)
            return;

        _viewModel.ToggleSort(columnId);
        e.Handled = true;
    }

    private void OnColumnDragHandlePressed(object? sender, PointerPressedEventArgs e)
    {
        if (_viewModel is null || sender is not TextBlock handle)
            return;

        if (handle.DataContext is not TorrentColumnItemViewModel column)
            return;

        if (!e.GetCurrentPoint(handle).Properties.IsLeftButtonPressed)
            return;

        _draggingColumnId = column.Id;
        e.Pointer.Capture(handle);
        e.Handled = true;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        if (_draggingColumnId is null || _viewModel is null || !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            return;

        var target = FindColumnItemAt(e.GetPosition(ColumnPickerList));
        if (target is not null && target.Id != _draggingColumnId)
            _viewModel.MoveColumn(_draggingColumnId, target.Id);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);

        if (_draggingColumnId is null)
            return;

        _draggingColumnId = null;
        e.Pointer.Capture(null);
    }

    private TorrentColumnItemViewModel? FindColumnItemAt(Point position)
    {
        foreach (var border in ColumnPickerList.GetVisualDescendants().OfType<Border>())
        {
            if (!border.Classes.Contains("column-picker-row"))
                continue;

            var topLeft = border.TranslatePoint(new Point(0, 0), ColumnPickerList);
            if (topLeft is null)
                continue;

            var bounds = new Rect(topLeft.Value, border.Bounds.Size);
            if (bounds.Contains(position) && border.DataContext is TorrentColumnItemViewModel column)
                return column;
        }

        return null;
    }

    private void RebuildColumns()
    {
        if (_viewModel is null)
            return;

        TorrentsGrid.Columns.Clear();

        var visibleColumns = _viewModel.GetVisibleColumnsInOrder();
        for (var index = 0; index < visibleColumns.Count; index++)
        {
            var setting = visibleColumns[index];
            var definition = TorrentTableColumns.Find(setting.Id);
            if (definition is null)
                continue;

            var width = setting.WidthPx ?? definition.DefaultWidthPx;
            var columnWidth = index == 0
                ? new DataGridLength(1, DataGridLengthUnitType.Star)
                : new DataGridLength(width, DataGridLengthUnitType.Pixel);

            DataGridColumn column = setting.Id == TorrentTableColumnIds.Progress
                ? new DataGridTemplateColumn
                {
                    Header = CreateColumnHeader(setting.Id),
                    SortMemberPath = definition.BindingPath,
                    Width = columnWidth,
                    Tag = setting.Id,
                    CanUserSort = definition.Sortable,
                    CellTemplate = new FuncDataTemplate<TorrentRowViewModel>(
                        (row, _) => row is null
                            ? new Panel()
                            : new TorrentProgressBar { DataContext = row }),
                }
                : new DataGridTextColumn
                {
                    Header = CreateColumnHeader(setting.Id),
                    Binding = new Binding(definition.BindingPath),
                    SortMemberPath = definition.BindingPath,
                    Width = columnWidth,
                    Tag = setting.Id,
                    CanUserSort = definition.Sortable,
                };

            TorrentsGrid.Columns.Add(column);
        }
    }

    private void UpdateSortHeaders()
    {
        foreach (var column in TorrentsGrid.Columns)
        {
            if (column.Tag is string columnId)
                column.Header = CreateColumnHeader(columnId);
        }
    }

    private Control CreateColumnHeader(string columnId)
    {
        var sortable = TorrentTableColumns.Find(columnId)?.Sortable == true;

        return new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 4,
            Cursor = sortable ? new Cursor(StandardCursorType.Hand) : null,
            Children =
            {
                new TextBlock
                {
                    Text = _viewModel?.GetColumnLabel(columnId) ?? columnId,
                    VerticalAlignment = VerticalAlignment.Center,
                },
                new TextBlock
                {
                    Text = _viewModel?.GetSortMark(columnId) ?? string.Empty,
                    Opacity = 0.7,
                    VerticalAlignment = VerticalAlignment.Center,
                },
            },
        };
    }
}
