using qrschool_mobil.Models;
using qrschool_mobil.Services;

namespace qrschool_mobil;

public partial class AddEquipmentPage : ContentPage
{
    private readonly IInventoryRepository _repository = InventoryRepositoryProvider.Current;

    public AddEquipmentPage()
    {
        InitializeComponent();
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(QrEntry.Text) || string.IsNullOrWhiteSpace(TypeEntry.Text))
        {
            await DisplayAlert("Инвентаризация", "Заполните QR-код и тип техники.", "ОК");
            return;
        }

        var item = new InventoryItem(
            string.Empty,
            QrEntry.Text,
            TypeEntry.Text,
            CabinetEntry.Text ?? "-",
            StatusEntry.Text ?? "-",
            DescriptionEditor.Text ?? "-");

        try
        {
            await _repository.AddEquipmentAsync(item);
            await DisplayAlert("Инвентаризация", "Техника добавлена в Neon.", "ОК");
            await Shell.Current.GoToAsync(nameof(EquipmentListPage));
        }
        catch (Exception ex)
        {
            await DisplayAlert("Neon", $"Не удалось добавить технику: {ex.Message}", "ОК");
        }
    }

    private async void OnBackTapped(object? sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}
