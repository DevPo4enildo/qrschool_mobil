using qrschool_mobil.Models;

namespace qrschool_mobil.Services;

public interface IAttendanceRepository
{
    Task<IReadOnlyList<Student>> GetStudentsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AttendanceEntry>> GetTodayAttendanceAsync(CancellationToken cancellationToken = default);

    Task<AttendanceSummary> GetSummaryAsync(CancellationToken cancellationToken = default);

    Task<AttendanceEntry> RegisterQrAsync(string qrPayload, CancellationToken cancellationToken = default);
}
