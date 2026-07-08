using System.Windows;
using SensorDeck.Wpf.ViewModels;

namespace SensorDeck.Wpf;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel = new();

    public MainWindow()
    {
        InitializeComponent();
        DataContext = _viewModel;
        Closing += async (_, _) => await _viewModel.DisposeAsync();
    }
}
