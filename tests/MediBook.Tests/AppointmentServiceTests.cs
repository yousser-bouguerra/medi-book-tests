using MediBook.Api.Models;
using MediBook.Api.Repositories;
using MediBook.Api.Services;
using Moq;

namespace MediBook.Tests;

public class AppointmentServiceTests
{
    private readonly Mock<IAppointmentRepository> _repo = new();
    private readonly AppointmentService _service;

    public AppointmentServiceTests()
    {
        _service = new AppointmentService(_repo.Object);
    }

    private static DateTime Tomorrow => DateTime.UtcNow.AddDays(1);

    [Fact]
    public async Task Create_SavesAppointment_WhenSlotIsFree()
    {
        var start = Tomorrow;
        _repo.Setup(r => r.IsSlotTakenAsync(1, start)).ReturnsAsync(false);

        var result = await _service.CreateAsync(doctorId: 1, patientId: 10, startTime: start);

        Assert.Equal(AppointmentStatus.Scheduled, result.Status);
        Assert.Equal(10, result.PatientId);
        _repo.Verify(r => r.AddAsync(It.IsAny<Appointment>()), Times.Once);
    }

    [Fact]
    public async Task Create_Throws_WhenSlotIsTaken()
    {
        var start = Tomorrow;
        _repo.Setup(r => r.IsSlotTakenAsync(1, start)).ReturnsAsync(true);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CreateAsync(1, 10, start));

        _repo.Verify(r => r.AddAsync(It.IsAny<Appointment>()), Times.Never);
    }

    [Fact]
    public async Task Create_Throws_WhenDateIsInThePast()
    {
        var past = DateTime.UtcNow.AddDays(-1);

        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateAsync(1, 10, past));

        _repo.Verify(r => r.AddAsync(It.IsAny<Appointment>()), Times.Never);
    }

    [Fact]
    public async Task Cancel_SetsStatusToCancelled()
    {
        var appointment = new Appointment { Id = 5, PatientId = 10, Status = AppointmentStatus.Scheduled };
        _repo.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(appointment);

        await _service.CancelAsync(5, 10);

        Assert.Equal(AppointmentStatus.Cancelled, appointment.Status);
        _repo.Verify(r => r.UpdateAsync(appointment), Times.Once);
    }

    [Fact]
    public async Task Cancel_Throws_WhenAppointmentNotFound()
    {
        _repo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Appointment?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.CancelAsync(99, 10));
    }

    [Fact]
    public async Task Cancel_Throws_WhenPatientIsNotTheOwner()
    {
        var appointment = new Appointment { Id = 5, PatientId = 10 };
        _repo.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(appointment);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.CancelAsync(5, 11));

        _repo.Verify(r => r.UpdateAsync(It.IsAny<Appointment>()), Times.Never);
    }

    [Fact]
    public async Task Cancel_Throws_WhenAlreadyCancelled()
    {
        var appointment = new Appointment { Id = 5, PatientId = 10, Status = AppointmentStatus.Cancelled };
        _repo.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(appointment);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CancelAsync(5, 10));
    }

    [Fact]
    public async Task GetForPatient_ReturnsAppointmentsFromRepository()
    {
        var list = new List<Appointment>
        {
            new() { Id = 1, PatientId = 10 },
            new() { Id = 2, PatientId = 10 }
        };
        _repo.Setup(r => r.GetByPatientAsync(10)).ReturnsAsync(list);

        var result = await _service.GetForPatientAsync(10);

        Assert.Equal(2, result.Count);
    }
}
