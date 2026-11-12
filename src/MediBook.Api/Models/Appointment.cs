namespace MediBook.Api.Models;

public enum AppointmentStatus
{
    Scheduled,
    Cancelled
}

public class Appointment
{
    public int Id { get; set; }
    public int DoctorId { get; set; }
    public int PatientId { get; set; }
    public DateTime StartTime { get; set; }
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;
}

public record CreateAppointmentRequest(int DoctorId, DateTime StartTime);
