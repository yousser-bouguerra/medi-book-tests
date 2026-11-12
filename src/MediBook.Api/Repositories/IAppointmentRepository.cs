using MediBook.Api.Models;

namespace MediBook.Api.Repositories;

public interface IAppointmentRepository
{
    Task<bool> IsSlotTakenAsync(int doctorId, DateTime startTime);
    Task<Appointment?> GetByIdAsync(int id);
    Task<IReadOnlyList<Appointment>> GetByPatientAsync(int patientId);
    Task AddAsync(Appointment appointment);
    Task UpdateAsync(Appointment appointment);
}
