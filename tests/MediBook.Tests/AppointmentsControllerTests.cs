using System.Security.Claims;
using MediBook.Api.Controllers;
using MediBook.Api.Models;
using MediBook.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace MediBook.Tests;

public class AppointmentsControllerTests
{
    private readonly Mock<IAppointmentService> _service = new();

    private AppointmentsController BuildController(int userId)
    {
        var controller = new AppointmentsController(_service.Object);
        var identity = new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) }, "test");

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };
        return controller;
    }

    [Fact]
    public async Task Create_ReturnsCreated_WhenServiceSucceeds()
    {
        var start = DateTime.UtcNow.AddDays(1);
        _service.Setup(s => s.CreateAsync(1, 10, start))
                .ReturnsAsync(new Appointment { Id = 1, DoctorId = 1, PatientId = 10, StartTime = start });
        var controller = BuildController(10);

        var result = await controller.Create(new CreateAppointmentRequest(1, start));

        Assert.IsType<CreatedAtActionResult>(result);
    }

    [Fact]
    public async Task Create_ReturnsConflict_WhenSlotIsTaken()
    {
        var start = DateTime.UtcNow.AddDays(1);
        _service.Setup(s => s.CreateAsync(1, 10, start))
                .ThrowsAsync(new InvalidOperationException("taken"));
        var controller = BuildController(10);

        var result = await controller.Create(new CreateAppointmentRequest(1, start));

        Assert.IsType<ConflictObjectResult>(result);
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenDateIsInvalid()
    {
        var start = DateTime.UtcNow.AddDays(-1);
        _service.Setup(s => s.CreateAsync(1, 10, start))
                .ThrowsAsync(new ArgumentException("past"));
        var controller = BuildController(10);

        var result = await controller.Create(new CreateAppointmentRequest(1, start));

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Cancel_ReturnsNoContent_WhenSuccessful()
    {
        var controller = BuildController(10);

        var result = await controller.Cancel(5);

        Assert.IsType<NoContentResult>(result);
        _service.Verify(s => s.CancelAsync(5, 10), Times.Once);
    }

    [Fact]
    public async Task Cancel_ReturnsNotFound_WhenAppointmentDoesNotExist()
    {
        _service.Setup(s => s.CancelAsync(99, 10)).ThrowsAsync(new KeyNotFoundException());
        var controller = BuildController(10);

        var result = await controller.Cancel(99);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Cancel_ReturnsForbid_WhenNotTheOwner()
    {
        _service.Setup(s => s.CancelAsync(5, 11)).ThrowsAsync(new UnauthorizedAccessException());
        var controller = BuildController(11);

        var result = await controller.Cancel(5);

        Assert.IsType<ForbidResult>(result);
    }
}
