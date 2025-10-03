# LiftTracker Documentation

Welcome to LiftTracker - a comprehensive workout tracking system built with .NET 8, Blazor WebAssembly, and ASP.NET Core.

## 📚 Documentation Index

### User Guides
- [Getting Started](./getting-started.md) - Quick setup and first workout
- [User Manual](./user-manual.md) - Complete feature guide
- [API Documentation](./api-documentation.md) - REST API reference

### Developer Documentation
- [Architecture Overview](./architecture.md) - System design and structure
- [Development Setup](./development-setup.md) - Local development environment
- [Performance Optimization](./performance-optimization.md) - Caching and optimization strategies
- [Testing Guide](./testing-guide.md) - Unit, integration, and component testing
- [Code Refactoring Guide](./refactoring-guide.md) - Refactoring patterns and best practices

### Deployment & Operations
- [Deployment Guide](./deployment-guide.md) - Production deployment instructions
- [Configuration Reference](./configuration.md) - Environment variables and settings
- [Monitoring & Logging](./monitoring.md) - Observability and troubleshooting

### Quality Assurance
- [Quickstart Validation Report](./quickstart-validation-report.md) - Comprehensive scenario validation
- [Testing Coverage Analysis](./testing-guide.md) - 49 test implementations and strategies

## 🏗️ System Architecture

LiftTracker follows Clean Architecture principles with the following layers:

```
┌─────────────────────────────────────┐
│           Presentation Layer        │
│      (Blazor WebAssembly Client)    │
├─────────────────────────────────────┤
│             API Layer               │
│        (ASP.NET Core Web API)       │
├─────────────────────────────────────┤
│          Application Layer          │
│    (Services, DTOs, Interfaces)     │
├─────────────────────────────────────┤
│           Domain Layer              │
│     (Entities, Value Objects)       │
├─────────────────────────────────────┤
│        Infrastructure Layer         │
│  (EF Core, Repositories, Caching)   │
└─────────────────────────────────────┘
```

## 🚀 Quick Start

1. **Prerequisites**: .NET 8 SDK, Node.js (for client tooling)
2. **Clone**: `git clone [repository-url]`
3. **Setup**: Run `dotnet restore` in the solution root
4. **Database**: Run `dotnet ef database update` in the API project
5. **Run**: Execute `dotnet run` in both API and Client projects
6. **Access**: Navigate to `https://localhost:5001`

## 🔧 Technology Stack

- **Backend**: ASP.NET Core 8, Entity Framework Core
- **Frontend**: Blazor WebAssembly, Tailwind CSS
- **Database**: SQL Server (or SQL Server LocalDB for development)
- **Authentication**: Google OAuth 2.0
- **Caching**: In-Memory Caching with Redis support
- **Logging**: Serilog with structured logging
- **Testing**: xUnit, Moq, bUnit
- **DevOps**: Terraform, Azure deployment ready

## 🏋️ Key Features

### Workout Management
- **Strength Training**: Track sets, reps, and weights for traditional lifts
- **Metcon Workouts**: Log high-intensity metabolic conditioning workouts
- **Exercise Library**: Comprehensive database of movements and exercises
- **Session Management**: Organize workouts into sessions with timestamps

### Progress Tracking
- **Performance Analytics**: Visual charts and progress metrics
- **Personal Records**: Automatic tracking of personal bests
- **Historical Data**: Complete workout history with search and filtering
- **Goal Setting**: Set and track fitness goals over time

### User Experience
- **Responsive Design**: Works seamlessly on desktop and mobile devices
- **Offline Capability**: Continue tracking workouts without internet connection
- **Data Export**: Export workout data in various formats
- **Social Features**: Share achievements and connect with other users

## 🔍 Performance Highlights

- **Sub-second API response times** with intelligent caching
- **Optimized database queries** with Entity Framework performance tuning
- **Client-side state management** for smooth user interactions
- **Real-time performance monitoring** with detailed metrics
- **Scalable architecture** designed for high-traffic scenarios

## 🛡️ Security Features

- **OAuth 2.0 Authentication** with Google integration
- **JWT Token Management** with secure refresh mechanisms
- **Data Isolation** ensuring users only access their own data
- **HTTPS Enforcement** with security headers
- **Input Validation** at all layers to prevent security vulnerabilities

## 📖 Development Philosophy

LiftTracker is built following these core principles:

1. **Clean Architecture**: Clear separation of concerns across layers
2. **Test-Driven Development**: Comprehensive test coverage at all levels
3. **Performance First**: Optimized for speed and scalability
4. **User-Centric Design**: Intuitive interface focused on user experience
5. **Maintainable Code**: SOLID principles and clean code practices

## 🤝 Contributing

For development guidelines, testing standards, and contribution workflows, see our [Development Setup Guide](./development-setup.md).

## 📞 Support

- **Issues**: Report bugs and feature requests via GitHub Issues
- **Documentation**: This documentation is continuously updated
- **Community**: Join our community discussions for tips and best practices

---

*Last updated: October 2025*