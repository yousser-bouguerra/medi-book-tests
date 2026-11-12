using MediBook.Api.Models;

namespace MediBook.Api.Services;

public interface IAppointmentService
{
    Task<Appointment> CreateAsync(int doctorId, int patientId, DateTime startTime);
    Task CancelAsync(int appointmentId, int patientId);
    Task<IReadOnlyList<Appointment>> GetForPatientAsync(int patientId);
}
