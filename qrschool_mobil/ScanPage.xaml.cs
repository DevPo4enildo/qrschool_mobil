using qrschool_mobil.Models;
using qrschool_mobil.Services;
using ZXing.Net.Maui;

namespace qrschool_mobil;

public partial class ScanPage : ContentPage
{
    private readonly IInventoryRepository _repository = InventoryRepositoryProvider.Current;
    private bool _isProcessing;

    public ScanPage()
    {
        InitializeComponent();
    }

    private async void OnBarcodesDetected(object? sender, BarcodeDetectionEventArgs e)
    {
        if (_isProcessing)
        {
            return;
        }

        var code = e.Results?.FirstOrDefault()?.Value;
        if (string.IsNullOrWhiteSpace(code))
        {
            return;
        }

        _isProcessing = true;
        await MainThread.InvokeOnMainThreadAsync(async () => await LoadByQrCodeAsync(code));
        _isProcessing = false;
    }

    private async Task LoadByQrCodeAsync(string code)
    {
        ScanStatusLabel.Text = code;

        try
        {
            var item = await _repository.FindByQrCodeAsync(code);
            ShowItem(item);
        }
        catch (Exception ex)
        {
            ScanStatusLabel.Text = "Ошибка Neon";
            DescriptionLabel.Text = $"Описание: {ex.Message}";
        }
    }

    private void ShowItem(InventoryItem? item)
    {
        TypeLabel.Text = $"Тип: {item?.Type ?? "-"}";
        CabinetLabel.Text = $"Кабинет: {item?.Cabinet ?? "-"}";
        StatusLabel.Text = $"Статус: {item?.Status ?? "-"}";
        DescriptionLabel.Text = $"Описание: {item?.Description ?? "-"}";

        if (item is null)
        {
            ScanStatusLabel.Text = "Техника не найдена";
        }
    }

    private async void OnBackTapped(object? sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}
