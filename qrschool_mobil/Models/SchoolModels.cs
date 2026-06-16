namespace qrschool_mobil.Models;

public sealed record Student(string Id, string FullName, string ClassName, string Status);

public sealed record AttendanceEntry(
    string StudentId,
    string StudentName,
    string ClassName,
    DateTimeOffset CheckedAt,
    string Source,
    bool Synced);

public sealed record AttendanceSummary(int PresentToday, int LateToday, int WaitingForSync);
