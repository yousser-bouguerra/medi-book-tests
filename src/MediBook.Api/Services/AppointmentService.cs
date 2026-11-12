using MediBook.Api.Models;
using MediBook.Api.Repositories;

namespace MediBook.Api.Services;

public class AppointmentService : IAppointmentService
{
    private readonly IAppointmentRepository _repository;

    public AppointmentService(IAppointmentRepository repository)
    {
        _repository = repository;
    }

    public async Task<Appointment> CreateAsync(int doctorId, int patientId, DateTime startTime)
    {
        if (startTime <= DateTime.UtcNow)
            throw new ArgumentException("The appointment must be in the future.");

        if (await _repository.IsSlotTakenAsync(doctorId, startTime))
            throw new InvalidOperationException("This time slot is already taken.");

        var appointment = new Appointment
        {
            DoctorId = doctorId,
            PatientId = patientId,
            StartTime = startTime,
            Status = AppointmentStatus.Scheduled
        };

        await _repository.AddAsync(appointment);
        return appointment;
    }

    public async Task CancelAsync(int appointmentId, int patientId)
    {
        var appointment = await _repository.GetByIdAsync(appointmentId)
            ?? throw new KeyNotFoundException("Appointment not found.");

        if (appointment.PatientId != patientId)
            throw new UnauthorizedAccessException("You cannot cancel another patient's appointment.");

        if (appointment.Status == AppointmentStatus.Cancelled)
            throw new InvalidOperationException("The appointment is already cancelled.");

        appointment.Status = AppointmentStatus.Cancelled;
        await _repository.UpdateAsync(appointment);
    }

    public Task<IReadOnlyList<Appointment>> GetForPatientAsync(int patientId) =>
        _repository.GetByPatientAsync(patientId);
}
