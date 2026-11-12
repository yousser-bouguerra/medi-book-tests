using MediBook.Api.Repositories;
using MediBook.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
// Replace with your EF Core repository: AddScoped<IAppointmentRepository, EfAppointmentRepository>()
builder.Services.AddSingleton<IAppointmentRepository, InMemoryAppointmentRepository>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();

// TODO: plug in your existing JWT authentication here (AddAuthentication().AddJwtBearer(...))
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthorization();
app.MapControllers();
app.Run();

public partial class Program { }
