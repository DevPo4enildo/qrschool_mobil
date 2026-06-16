using qrschool_mobil.Services;

namespace qrschool_mobil;

public partial class MainPage : ContentPage
{
    private readonly IAttendanceRepository _attendanceRepository = new NeonAttendanceRepository();

    public MainPage()
    {
        InitializeComponent();
        ConnectionStringEntry.Text = NeonConnectionSettings.ConnectionString;
        UpdateConnectionStatus();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadDashboardAsync();
    }

    private async Task LoadDashboardAsync()
    {
        try
        {
            var students = await _attendanceRepository.GetStudentsAsync();
            var attendance = await _attendanceRepository.GetTodayAttendanceAsync();
            var summary = await _attendanceRepository.GetSummaryAsync();

            StudentsView.ItemsSource = students;
            AttendanceView.ItemsSource = attendance;
            PresentLabel.Text = summary.PresentToday.ToString();
            LateLabel.Text = summary.LateToday.ToString();
            SyncLabel.Text = summary.WaitingForSync.ToString();
        }
        catch (Exception ex)
        {
            ConnectionStatusLabel.Text = $"Не удалось прочитать Neon: {ex.Message}";
        }
    }

    private async void OnSaveConnectionClicked(object? sender, EventArgs e)
    {
        NeonConnectionSettings.ConnectionString = ConnectionStringEntry.Text ?? string.Empty;
        UpdateConnectionStatus();
        await LoadDashboardAsync();
    }

    private async void OnRegisterQrClicked(object? sender, EventArgs e)
    {
        var payload = QrPayloadEntry.Text;
        if (string.IsNullOrWhiteSpace(payload))
        {
            await DisplayAlert("QR School", "Введите или отсканируйте QR-код ученика.", "ОК");
            return;
        }

        try
        {
            var entry = await _attendanceRepository.RegisterQrAsync(payload);
            LastScanLabel.Text = $"{entry.StudentName} — {entry.CheckedAt:HH:mm} ({(entry.Synced ? "Neon" : "локально")})";
            QrPayloadEntry.Text = string.Empty;
            await LoadDashboardAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("QR School", $"Не удалось сохранить отметку: {ex.Message}", "ОК");
        }
    }

    private void UpdateConnectionStatus()
    {
        ConnectionStatusLabel.Text = NeonConnectionSettings.HasConnectionString
            ? "Облачная база Neon подключена."
            : "Демо-режим: добавьте строку Neon для синхронизации с облаком.";
    }
}
