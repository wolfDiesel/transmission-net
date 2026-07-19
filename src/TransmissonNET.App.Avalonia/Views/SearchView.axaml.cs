using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using TransmissonNET.App.Avalonia.ViewModels;

namespace TransmissonNET.App.Avalonia.Views;

public partial class SearchView : UserControl
{
    public SearchView()
    {
        InitializeComponent();
        DataContextChanged += (_, _) => ApplyHeaders();
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    private void OnQueryKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
            return;

        e.Handled = true;
        if (DataContext is not SearchViewModel vm)
            return;

        if (sender is TextBox box)
            vm.Query = box.Text ?? string.Empty;

        if (vm.SearchCommand.CanExecute(null))
            vm.SearchCommand.Execute(null);
    }

    private void ApplyHeaders()
    {
        if (DataContext is not SearchViewModel vm)
            return;

        var grid = this.FindControl<DataGrid>("ResultsGrid");
        if (grid?.Columns is null || grid.Columns.Count < 5)
            return;

        grid.Columns[0].Header = vm.ColumnName;
        grid.Columns[1].Header = vm.ColumnSource;
        grid.Columns[2].Header = vm.ColumnSize;
        grid.Columns[3].Header = vm.ColumnLink;
        grid.Columns[4].Header = vm.ColumnActions;
    }
}
