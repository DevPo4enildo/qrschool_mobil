using Npgsql;
using qrschool_mobil.Models;

namespace qrschool_mobil.Services;

public sealed class NeonAttendanceRepository : IAttendanceRepository
{
    private static readonly Student[] DemoStudents =
    [
        new("1001", "Алина Иванова", "7А", "На уроке"),
        new("1002", "Даниил Смирнов", "8Б", "Ожидает отметки"),
        new("1003", "Мария Петрова", "9В", "Опоздание")
    ];

    private readonly List<AttendanceEntry> _offlineEntries = [];

    public async Task<IReadOnlyList<Student>> GetStudentsAsync(CancellationToken cancellationToken = default)
    {
        if (!NeonConnectionSettings.HasConnectionString)
        {
            return DemoStudents;
        }

        await using var connection = await OpenConnectionAsync(cancellationToken);
        const string sql = """
            select id::text, full_name, class_name, coalesce(status, 'Активен')
            from students
            order by class_name, full_name
            limit 100
            """;
        await using var command = new NpgsqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var students = new List<Student>();
        while (await reader.ReadAsync(cancellationToken))
        {
            students.Add(new Student(reader.GetString(0), reader.GetString(1), reader.GetString(2), reader.GetString(3)));
        }

        return students.Count > 0 ? students : DemoStudents;
    }

    public async Task<IReadOnlyList<AttendanceEntry>> GetTodayAttendanceAsync(CancellationToken cancellationToken = default)
    {
        if (!NeonConnectionSettings.HasConnectionString)
        {
            return BuildDemoAttendance();
        }

        await using var connection = await OpenConnectionAsync(cancellationToken);
        const string sql = """
            select a.student_id::text, s.full_name, s.class_name, a.checked_at, coalesce(a.source, 'qr')
            from attendance a
            join students s on s.id = a.student_id
            where a.checked_at::date = current_date
            order by a.checked_at desc
            limit 100
            """;
        await using var command = new NpgsqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var entries = new List<AttendanceEntry>();
        while (await reader.ReadAsync(cancellationToken))
        {
            entries.Add(new AttendanceEntry(
                reader.GetString(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetFieldValue<DateTimeOffset>(3),
                reader.GetString(4),
                true));
        }

        return entries.Count > 0 ? entries : BuildDemoAttendance();
    }

    public async Task<AttendanceSummary> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var entries = await GetTodayAttendanceAsync(cancellationToken);
        var late = entries.Count(entry => entry.CheckedAt.LocalDateTime.TimeOfDay > new TimeSpan(8, 30, 0));
        return new AttendanceSummary(entries.Count, late, _offlineEntries.Count(entry => !entry.Synced));
    }

    public async Task<AttendanceEntry> RegisterQrAsync(string qrPayload, CancellationToken cancellationToken = default)
    {
        var studentId = ParseStudentId(qrPayload);
        var now = DateTimeOffset.Now;

        if (!NeonConnectionSettings.HasConnectionString)
        {
            var student = DemoStudents.FirstOrDefault(item => item.Id == studentId) ?? DemoStudents[0];
            var offlineEntry = new AttendanceEntry(student.Id, student.FullName, student.ClassName, now, "offline-qr", false);
            _offlineEntries.Insert(0, offlineEntry);
            return offlineEntry;
        }

        await using var connection = await OpenConnectionAsync(cancellationToken);
        const string sql = """
            insert into attendance (student_id, checked_at, source)
            values (@student_id, @checked_at, 'mobile-qr')
            returning student_id::text,
                (select full_name from students where id = @student_id),
                (select class_name from students where id = @student_id),
                checked_at,
                source
            """;
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("student_id", studentId);
        command.Parameters.AddWithValue("checked_at", now);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (await reader.ReadAsync(cancellationToken))
        {
            return new AttendanceEntry(
                reader.GetString(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetFieldValue<DateTimeOffset>(3),
                reader.GetString(4),
                true);
        }

        throw new InvalidOperationException("Neon не вернул созданную отметку посещаемости.");
    }

    private static async Task<NpgsqlConnection> OpenConnectionAsync(CancellationToken cancellationToken)
    {
        var connection = new NpgsqlConnection(NeonConnectionSettings.ConnectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }

    private IReadOnlyList<AttendanceEntry> BuildDemoAttendance()
    {
        var demo = DemoStudents.Select((student, index) => new AttendanceEntry(
            student.Id,
            student.FullName,
            student.ClassName,
            DateTimeOffset.Now.AddMinutes(-15 * index),
            "demo",
            true));

        return _offlineEntries.Concat(demo).ToList();
    }

    private static string ParseStudentId(string qrPayload)
    {
        var payload = qrPayload.Trim();
        const string prefix = "student:";
        return payload.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
            ? payload[prefix.Length..]
            : payload;
    }
}
