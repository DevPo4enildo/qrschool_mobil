using Npgsql;
using qrschool_mobil.Models;

namespace qrschool_mobil.Services;

public sealed class NeonInventoryRepository : IInventoryRepository
{
    // Подключение должно совпадать с qrschool_deckstop. В текущем workspace проекта qrschool_deckstop нет,
    // поэтому значение оставлено в одном месте для прямой вставки desktop connection string.
    private const string NeonConnectionString = "Host=ep-example.neon.tech;Port=5432;Database=qrschool;Username=qrschool_owner;Password=CHANGE_ME;SSL Mode=Require;Trust Server Certificate=true";

    public async Task<IReadOnlyList<InventoryItem>> GetEquipmentAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        const string sql = """
            select id::text,
                   coalesce(qr_code, id::text) as qr_code,
                   coalesce(type, name, 'Техника') as type,
                   coalesce(cabinet, room, '-') as cabinet,
                   coalesce(status, '-') as status,
                   coalesce(description, '-') as description
            from equipment
            order by type, cabinet, id
            """;

        await using var command = new NpgsqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var items = new List<InventoryItem>();

        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(ReadInventoryItem(reader));
        }

        return items;
    }

    public async Task<InventoryItem?> FindByQrCodeAsync(string qrCode, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        const string sql = """
            select id::text,
                   coalesce(qr_code, id::text) as qr_code,
                   coalesce(type, name, 'Техника') as type,
                   coalesce(cabinet, room, '-') as cabinet,
                   coalesce(status, '-') as status,
                   coalesce(description, '-') as description
            from equipment
            where qr_code = @qr_code or id::text = @qr_code
            limit 1
            """;

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("qr_code", qrCode.Trim());
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        return await reader.ReadAsync(cancellationToken) ? ReadInventoryItem(reader) : null;
    }

    public async Task AddEquipmentAsync(InventoryItem item, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        const string sql = """
            insert into equipment (qr_code, type, cabinet, status, description)
            values (@qr_code, @type, @cabinet, @status, @description)
            """;

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("qr_code", item.QrCode.Trim());
        command.Parameters.AddWithValue("type", item.Type.Trim());
        command.Parameters.AddWithValue("cabinet", item.Cabinet.Trim());
        command.Parameters.AddWithValue("status", item.Status.Trim());
        command.Parameters.AddWithValue("description", item.Description.Trim());
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task<NpgsqlConnection> OpenConnectionAsync(CancellationToken cancellationToken)
    {
        var connection = new NpgsqlConnection(NeonConnectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }

    private static InventoryItem ReadInventoryItem(NpgsqlDataReader reader)
    {
        return new InventoryItem(
            reader.GetString(0),
            reader.GetString(1),
            reader.GetString(2),
            reader.GetString(3),
            reader.GetString(4),
            reader.GetString(5));
    }
}
