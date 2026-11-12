# MediBook

![CI](https://github.com/<ton-username>/<ton-repo>/actions/workflows/ci.yml/badge.svg)

Clinic appointment management: ASP.NET Core REST API (EF Core, PostgreSQL, JWT roles) with an Angular front end.

## Run the tests
```bash
dotnet test tests/MediBook.Tests/MediBook.Tests.csproj
```

## Tests
- `AppointmentServiceTests`: business rules (free slot, taken slot, past date, cancel, ownership, already cancelled) using xUnit + Moq
- `AppointmentsControllerTests`: HTTP results (201, 204, 400, 403, 404, 409)
- CI: GitHub Actions builds and runs the tests on every push and pull request
