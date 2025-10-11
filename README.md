# CleanSolution

Clean Architecture (სუფთა არქიტექტურა) - მიდგომით შექმნილი პროექტის ნიმუში

## 📋 მიმოხილვა / Overview

CleanSolution არის .NET 9 პროექტის შაბლონი, რომელიც აგებულია Clean Architecture პრინციპების გამოყენებით. პროექტი უზრუნველყოფს მკაფიო განაწილებას ბიზნეს ლოგიკას, ინფრასტრუქტურას და პრეზენტაციის ფენებს შორის.

This is a .NET 9 project template built using Clean Architecture principles, ensuring clear separation between business logic, infrastructure, and presentation layers.

## 🏗️ არქიტექტურა / Architecture

პროექტი დაყოფილია სამ ძირითად ფენად:

### Core (ბირთვი)
- **Core.Domain** - დომეინის მოდელები, ენთითები და ენამები
  - Models: `Employee`, `Position`, `Address`
  - Enums: `Gender`, `Language`
  - Base classes: `BaseEntity`, `AuditableEntity`, `Aggregate`

- **Core.Application** - აპლიკაციის ბიზნეს ლოგიკა და CQRS პატერნები
  - Commands: Create, Update, Delete ოპერაციები
  - Queries: მონაცემების მოძიება და ფილტრაცია
  - Notifications: Event handling
  - DTOs: Data Transfer Objects
  - Behaviors: Validation behaviors
  - Technologies: MediatR, FluentValidation, Mapster, Bogus

- **Core.Shared** - საერთო უტილიტები და გაფართოებები
  - Extensions, helpers და shared utilities

### Infrastructure (ინფრასტრუქტურა)
- **Infrastructure.Persistence** - მონაცემთა ბაზასთან მუშაობა (Entity Framework Core)
- **Infrastructure.Messaging** - შეტყობინებების სერვისები და message queues
- **Infrastructure.Documents** - დოკუმენტების გენერაცია და მართვა

### Presentation (პრეზენტაცია)
- **Presentation.WebApi** - REST API (ASP.NET Core)
  - JWT Authentication
  - API Versioning
  - Swagger/OpenAPI documentation
  - Health Checks
  - Rate Limiting
  - Serilog logging (Graylog, Seq)
  - Response Compression
  - CORS support

- **Presentation.Worker** - Background services და scheduled tasks

## 🔧 ტექნოლოგიები / Technologies

- **.NET 9.0**
- **Entity Framework Core 9.0**
- **MediatR** - CQRS pattern implementation
- **FluentValidation** - Input validation
- **Mapster** - Object mapping
- **Serilog** - Structured logging
- **Swagger** - API documentation
- **JWT Bearer** - Authentication
- **AspNetCoreRateLimit** - Rate limiting
- **Health Checks** - Application monitoring
- **Bogus** - Fake data generation for testing

## 🚀 დაწყება / Getting Started

### წინაპირობები / Prerequisites

- .NET 9.0 SDK
- SQL Server (ან სხვა supported database)
- Visual Studio 2022 / VS Code / Rider

### ინსტალაცია / Installation

1. Clone the repository:
```bash
git clone https://github.com/yourusername/CleanSolution.git
cd CleanSolution
```

2. Restore dependencies:
```bash
dotnet restore
```

3. Update connection string in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "your-connection-string"
  }
}
```

4. Run migrations:
```bash
dotnet ef database update --project Infrastructure/Infrastructure.Persistence --startup-project Presentation/Presentation.WebApi
```

5. Run the application:
```bash
dotnet run --project Presentation/Presentation.WebApi
```

### API Documentation

WebApi გაშვების შემდეგ, Swagger UI ხელმისაწვდომია აქ:
- Development: `https://localhost:5001/swagger`

### Health Checks

აპლიკაციის სტატუსის შესამოწმებლად:
- Health endpoint: `https://localhost:5001/health`

## 📁 პროექტის სტრუქტურა / Project Structure

```
CleanSolution/
├── Core/
│   ├── Core.Domain/          # Domain entities and value objects
│   ├── Core.Application/     # Business logic and use cases
│   └── Core.Shared/          # Shared utilities
├── Infrastructure/
│   ├── Infrastructure.Persistence/   # Database and EF Core
│   ├── Infrastructure.Messaging/     # Message queue services
│   └── Infrastructure.Documents/     # Document generation
└── Presentation/
    ├── Presentation.WebApi/  # REST API endpoints
    └── Presentation.Worker/  # Background workers
```

## 🎯 ძირითადი ფუნქციები / Key Features

- ✅ Clean Architecture პრინციპები
- ✅ CQRS pattern with MediatR
- ✅ Repository pattern
- ✅ Validation pipeline
- ✅ API versioning
- ✅ JWT authentication
- ✅ Structured logging
- ✅ Health monitoring
- ✅ Rate limiting
- ✅ Response compression
- ✅ Localization support
- ✅ Correlation ID tracking
- ✅ Exception handling middleware

## 🔐 Security Features

- JWT Bearer token authentication
- Rate limiting per IP
- HTTPS redirection
- CORS configuration
- Input validation with FluentValidation

## 📊 Logging & Monitoring

- Serilog structured logging
- Graylog integration
- Seq integration
- Health checks with UI client
- SQL Server health monitoring
- URI health checks

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## 📝 License

This project is licensed under the MIT License.

## 👤 Author

Your Name / Company

## 📞 Support

For support, email your-email@example.com or create an issue in the repository.
