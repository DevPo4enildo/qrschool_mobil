namespace qrschool_mobil.Services;

public static class InventoryRepositoryProvider
{
    public static IInventoryRepository Current { get; } = new NeonInventoryRepository();
}
