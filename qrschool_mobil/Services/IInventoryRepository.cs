using qrschool_mobil.Models;

namespace qrschool_mobil.Services;

public interface IInventoryRepository
{
    Task<IReadOnlyList<InventoryItem>> GetEquipmentAsync(CancellationToken cancellationToken = default);

    Task<InventoryItem?> FindByQrCodeAsync(string qrCode, CancellationToken cancellationToken = default);

    Task AddEquipmentAsync(InventoryItem item, CancellationToken cancellationToken = default);
}
