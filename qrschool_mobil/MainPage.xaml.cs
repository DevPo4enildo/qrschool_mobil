namespace qrschool_mobil;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    private async void OnScanClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(ScanPage));
    }

    private async void OnAddClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(AddEquipmentPage));
    }

    private async void OnListClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(EquipmentListPage));
    }
}
