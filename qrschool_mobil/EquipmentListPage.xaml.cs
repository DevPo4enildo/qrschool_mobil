using qrschool_mobil.Services;

namespace qrschool_mobil;

public partial class EquipmentListPage : ContentPage
{
    private readonly IInventoryRepository _repository = InventoryRepositoryProvider.Current;

    public EquipmentListPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadEquipmentAsync();
    }

    private async Task LoadEquipmentAsync()
    {
        try
        {
            EquipmentView.ItemsSource = await _repository.GetEquipmentAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Neon", $"Не удалось загрузить список техники: {ex.Message}", "ОК");
        }
        finally
        {
            RefreshHost.IsRefreshing = false;
        }
    }

    private async void OnRefreshing(object? sender, EventArgs e)
    {
        await LoadEquipmentAsync();
    }

    private async void OnBackTapped(object? sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}
