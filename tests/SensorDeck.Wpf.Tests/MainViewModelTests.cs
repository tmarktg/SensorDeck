using SensorDeck.Wpf.ViewModels;
using Xunit;

namespace SensorDeck.Wpf.Tests;

public class MainViewModelTests
{
    [Fact]
    public void ConnectCommand_CanExecute_WhenNotConnected()
    {
        var vm = new MainViewModel();

        Assert.True(vm.ConnectCommand.CanExecute(null));
        Assert.False(vm.ToggleHeaterCommand.CanExecute(null));
        Assert.False(vm.ApplySampleRateCommand.CanExecute(null));
    }

    [Fact]
    public void Commands_CanExecute_Toggles_With_IsConnected()
    {
        var vm = new MainViewModel();

        vm.IsConnected = true;

        Assert.False(vm.ConnectCommand.CanExecute(null));
        Assert.True(vm.ToggleHeaterCommand.CanExecute(null));
        Assert.True(vm.ApplySampleRateCommand.CanExecute(null));

        vm.IsConnected = false;

        Assert.True(vm.ConnectCommand.CanExecute(null));
        Assert.False(vm.ToggleHeaterCommand.CanExecute(null));
        Assert.False(vm.ApplySampleRateCommand.CanExecute(null));
    }

    [Fact]
    public void PropertyChanged_Raised_When_IsConnected_Changes()
    {
        var vm = new MainViewModel();
        var raisedProperties = new List<string?>();
        vm.PropertyChanged += (_, e) => raisedProperties.Add(e.PropertyName);

        vm.IsConnected = true;

        Assert.Contains(nameof(MainViewModel.IsConnected), raisedProperties);
    }
}
