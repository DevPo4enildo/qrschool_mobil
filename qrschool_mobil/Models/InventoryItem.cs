namespace qrschool_mobil.Models;

public sealed record InventoryItem(
    string Id,
    string QrCode,
    string Type,
    string Cabinet,
    string Status,
    string Description);
