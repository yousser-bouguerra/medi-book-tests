using MediBook.Api.Models;

namespace MediBook.Api.Repositories;

// Simple implementation for local runs. Replace it with your EF Core + PostgreSQL repository.
public class InMemoryAppointmentRepository : IAppointmentRepository
{
    private readonly List<Appointment> _items = new();
    private int _nextId = 1;

    public Task<bool> IsSlotTakenAsync(int doctorId, DateTime startTime) =>
        Task.FromResult(_items.Any(a =>
            a.DoctorId == doctorId &&
            a.StartTime == startTime &&
            a.Status == AppointmentStatus.Scheduled));

    public Task<Appointment?> GetByIdAsync(int id) =>
        Task.FromResult(_items.FirstOrDefault(a => a.Id == id));

    public Task<IReadOnlyList<Appointment>> GetByPatientAsync(int patientId) =>
        Task.FromResult<IReadOnlyList<Appointment>>(_items.Where(a => a.PatientId == patientId).ToList());

    public Task AddAsync(Appointment appointment)
    {
        appointment.Id = _nextId++;
        _items.Add(appointment);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Appointment appointment) => Task.CompletedTask;
}
