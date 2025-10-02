# Architecture Overview

LiftTracker is built using Clean Architecture principles with .NET 8, providing a scalable, maintainable, and testable workout tracking system.

## Table of Contents

- [Architecture Principles](#architecture-principles)
- [System Overview](#system-overview)
- [Layer Architecture](#layer-architecture)
- [Technology Stack](#technology-stack)
- [Data Flow](#data-flow)
- [Security Architecture](#security-architecture)
- [Performance Architecture](#performance-architecture)
- [Deployment Architecture](#deployment-architecture)

## Architecture Principles

### Clean Architecture

LiftTracker follows Uncle Bob's Clean Architecture, ensuring:

- **Independence of Frameworks**: The architecture doesn't depend on external libraries
- **Testability**: Business rules can be tested without UI, database, or external services
- **Independence of UI**: The UI can change without affecting business rules
- **Independence of Database**: Business rules aren't bound to the database
- **Independence of External Agency**: Business rules don't know about the outside world

### SOLID Principles

- **Single Responsibility**: Each class has one reason to change
- **Open/Closed**: Open for extension, closed for modification
- **Liskov Substitution**: Derived classes are substitutable for base classes
- **Interface Segregation**: Clients shouldn't depend on interfaces they don't use
- **Dependency Inversion**: Depend on abstractions, not concretions

## System Overview

```
┌─────────────────────────────────────────────────────────────┐
│                        Client Layer                         │
│                   (Blazor WebAssembly)                     │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────┐ │
│  │   Pages     │  │ Components  │  │      Services       │ │
│  │             │  │             │  │                     │ │
│  │ • Dashboard │  │ • Forms     │  │ • AuthService       │ │
│  │ • Login     │  │ • Charts    │  │ • ApiClient         │ │
│  │ • Workouts  │  │ • Navigation│  │ • StateService      │ │
│  └─────────────┘  └─────────────┘  └─────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
                              │ HTTPS/JSON
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                         API Layer                          │
│                    (ASP.NET Core Web API)                  │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────┐ │
│  │Controllers  │  │ Middleware  │  │     Configuration   │ │
│  │             │  │             │  │                     │ │
│  │ • Auth      │  │ • Auth      │  │ • DI Container      │ │
│  │ • Users     │  │ • Error     │  │ • CORS              │ │
│  │ • Workouts  │  │ • Logging   │  │ • Swagger           │ │
│  │ • Progress  │  │ • Perf Mon  │  │ • Health Checks     │ │
│  └─────────────┘  └─────────────┘  └─────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                    Application Layer                       │
│                  (Business Logic & DTOs)                   │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────┐ │
│  │  Services   │  │    DTOs     │  │     Interfaces      │ │
│  │             │  │             │  │                     │ │
│  │ • UserSvc   │  │ • UserDTO   │  │ • IUserService      │ │
│  │ • WorkoutSvc│  │ • WorkoutDTO│  │ • IProgressService  │ │
│  │ • ProgressSvc│ │ • ProgressDTO│ │ • IWorkoutService   │ │
│  └─────────────┘  └─────────────┘  └─────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                      Domain Layer                          │
│                   (Core Business Rules)                    │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────┐ │
│  │  Entities   │  │Value Objects│  │     Interfaces      │ │
│  │             │  │             │  │                     │ │
│  │ • User      │  │ • Weight    │  │ • IUserRepository   │ │
│  │ • Workout   │  │ • Duration  │  │ • IWorkoutRepo      │ │
│  │ • Exercise  │  │ • Email     │  │ • IUnitOfWork       │ │
│  └─────────────┘  └─────────────┘  └─────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                   Infrastructure Layer                     │
│              (Data Access & External Services)             │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────┐ │
│  │Repositories │  │ Data Access │  │   External Services │ │
│  │             │  │             │  │                     │ │
│  │ • UserRepo  │  │ • DbContext │  │ • Google OAuth      │ │
│  │ • WorkoutRepo│ │ • Migrations│  │ • Email Service     │ │
│  │ • Cache     │  │ • Config    │  │ • Logging           │ │
│  └─────────────┘  └─────────────┘  └─────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
                    ┌─────────────────┐
                    │    Database     │
                    │   (SQL Server)  │
                    └─────────────────┘
```

## Layer Architecture

### 1. Domain Layer (`src/LiftTracker.Domain/`)

**Purpose**: Contains core business logic and rules

**Components**:
- **Entities**: Core business objects with identity
- **Value Objects**: Immutable objects defined by their values
- **Domain Services**: Complex business operations
- **Repository Interfaces**: Data access contracts

**Key Entities**:
```csharp
// User entity with business rules
public class User : BaseEntity
{
    public string Email { get; private set; }
    public string Name { get; private set; }
    public DateTime CreatedAt { get; private set; }
    
    // Business logic methods
    public void UpdateProfile(string name)
    {
        ValidateName(name);
        Name = name;
        UpdatedAt = DateTime.UtcNow;
    }
}

// Workout session with complex business rules
public class WorkoutSession : BaseEntity
{
    public Guid UserId { get; private set; }
    public DateTime StartTime { get; private set; }
    public DateTime? EndTime { get; private set; }
    public List<StrengthLift> StrengthLifts { get; private set; }
    public List<MetconWorkout> MetconWorkouts { get; private set; }
    
    public void CompleteSession()
    {
        if (EndTime.HasValue)
            throw new InvalidOperationException("Session already completed");
            
        EndTime = DateTime.UtcNow;
    }
}
```

**Dependencies**: None (pure business logic)

### 2. Application Layer (`src/LiftTracker.Application/`)

**Purpose**: Orchestrates business operations and defines use cases

**Components**:
- **Services**: Application-specific business logic
- **DTOs**: Data transfer objects for external communication
- **Interfaces**: Service contracts
- **Mappings**: AutoMapper profiles

**Example Service**:
```csharp
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    
    public async Task<UserDto> CreateUserAsync(CreateUserRequest request)
    {
        // Validate business rules
        var existingUser = await _userRepository.GetByEmailAsync(request.Email);
        if (existingUser != null)
            throw new DuplicateEmailException(request.Email);
        
        // Create domain entity
        var user = new User(request.Email, request.Name);
        
        // Persist
        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();
        
        // Return DTO
        return _mapper.Map<UserDto>(user);
    }
}
```

**Dependencies**: Domain Layer only

### 3. Infrastructure Layer (`src/LiftTracker.Infrastructure/`)

**Purpose**: Implements external concerns and data access

**Components**:
- **Repositories**: Data access implementations
- **DbContext**: Entity Framework configuration
- **External Services**: Third-party integrations
- **Caching**: Performance optimization

**Repository Pattern**:
```csharp
public class UserRepository : IUserRepository
{
    private readonly LiftTrackerDbContext _context;
    
    public async Task<User> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }
    
    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await _context.Users.AddAsync(user, cancellationToken);
    }
}
```

**Caching Architecture**:
```csharp
// Decorator pattern for caching
public class CachedUserRepository : IUserRepository
{
    private readonly IUserRepository _repository;
    private readonly ICacheService _cache;
    
    public async Task<User> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cacheKey = CacheKeys.User(id);
        
        return await _cache.GetOrSetAsync(cacheKey, async () =>
        {
            return await _repository.GetByIdAsync(id, cancellationToken);
        }, CacheKeys.UserExpiration);
    }
}
```

**Dependencies**: Application and Domain layers

### 4. API Layer (`src/LiftTracker.API/`)

**Purpose**: Exposes HTTP endpoints and handles web concerns

**Components**:
- **Controllers**: HTTP request/response handling
- **Middleware**: Cross-cutting concerns
- **Configuration**: Dependency injection and startup
- **Authentication**: JWT and OAuth integration

**Controller Example**:
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    
    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUser(Guid id)
    {
        try
        {
            var user = await _userService.GetUserByIdAsync(id);
            return Ok(user);
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
    }
}
```

**Middleware Pipeline**:
```csharp
// Request processing pipeline
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<PerformanceMonitoringMiddleware>();
app.UseMiddleware<ErrorHandlingMiddleware>();
app.MapControllers();
```

**Dependencies**: Application and Infrastructure layers

### 5. Client Layer (`src/LiftTracker.Client/`)

**Purpose**: Blazor WebAssembly frontend application

**Components**:
- **Pages**: Route-able UI components
- **Components**: Reusable UI elements
- **Services**: Client-side business logic
- **State Management**: Application state

**Component Example**:
```razor
@page "/workouts"
@inject IWorkoutService WorkoutService
@inject NavigationManager Navigation

<h1>My Workouts</h1>

@if (workouts == null)
{
    <p>Loading...</p>
}
else
{
    <WorkoutList Workouts="workouts" OnWorkoutSelected="NavigateToWorkout" />
}

@code {
    private List<WorkoutDto> workouts;

    protected override async Task OnInitializedAsync()
    {
        workouts = await WorkoutService.GetUserWorkoutsAsync();
    }

    private void NavigateToWorkout(Guid workoutId)
    {
        Navigation.NavigateTo($"/workouts/{workoutId}");
    }
}
```

**Dependencies**: Communicates with API layer via HTTP

## Technology Stack

### Backend Technologies

- **.NET 8**: Latest LTS version for performance and features
- **ASP.NET Core**: Web API framework with built-in dependency injection
- **Entity Framework Core**: ORM with Code First migrations
- **SQL Server**: Primary database with Azure SQL support
- **AutoMapper**: Object-to-object mapping
- **Serilog**: Structured logging with multiple sinks

### Frontend Technologies

- **Blazor WebAssembly**: C# in the browser with near-native performance
- **Tailwind CSS**: Utility-first CSS framework
- **Chart.js**: Interactive charts and data visualization
- **Progressive Web App (PWA)**: Offline support and mobile experience

### Cross-Cutting Technologies

- **JWT Tokens**: Stateless authentication
- **Google OAuth 2.0**: Social authentication
- **xUnit + Moq + bUnit**: Comprehensive testing stack
- **Docker**: Containerization for deployment
- **Azure**: Cloud hosting and services
- **Terraform**: Infrastructure as Code

## Data Flow

### Request Flow (API)

```
1. HTTP Request → 2. Middleware Pipeline → 3. Controller → 4. Application Service → 5. Domain Logic → 6. Repository → 7. Database
                                                                                                              ↓
8. HTTP Response ← 7. Middleware Pipeline ← 6. Controller ← 5. Application Service ← 4. Domain Entity ← 3. Repository ← 2. Data
```

### Authentication Flow

```
1. User clicks "Login with Google"
2. Redirect to Google OAuth
3. User authenticates with Google
4. Google redirects with authorization code
5. API exchanges code for access token
6. API creates JWT token with user claims
7. Client stores JWT in local storage
8. Subsequent requests include JWT in Authorization header
9. API validates JWT and extracts user context
```

### Caching Flow

```
1. Request for data
2. Check cache for existing data
3. If cache hit: return cached data
4. If cache miss: query database
5. Store result in cache with expiration
6. Return data to requester
```

## Security Architecture

### Authentication & Authorization

- **Google OAuth 2.0**: Primary authentication method
- **JWT Tokens**: Stateless authentication with claims
- **Role-based Authorization**: Admin, User role separation
- **Data Isolation**: Users can only access their own data

### Security Measures

- **HTTPS Everywhere**: TLS 1.2+ for all communication
- **Security Headers**: HSTS, CSP, X-Frame-Options
- **Input Validation**: Server-side validation on all inputs
- **SQL Injection Protection**: Parameterized queries via EF Core
- **XSS Protection**: Blazor's built-in encoding
- **CSRF Protection**: Anti-forgery tokens

### Data Protection

- **Encryption at Rest**: Database encryption
- **Encryption in Transit**: HTTPS/TLS
- **Sensitive Data**: Secure storage of credentials
- **Audit Logging**: Track all data modifications

## Performance Architecture

### Caching Strategy

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Client Cache  │    │  Server Cache   │    │    Database     │
│  (Browser)      │    │ (Memory/Redis)  │    │  (SQL Server)   │
├─────────────────┤    ├─────────────────┤    ├─────────────────┤
│ • Static Assets │    │ • User Data     │    │ • Master Data   │
│ • API Responses │    │ • Query Results │    │ • Transactional │
│ • UI State      │    │ • Computed Data │    │ • Historical    │
└─────────────────┘    └─────────────────┘    └─────────────────┘
```

### Performance Optimizations

- **In-Memory Caching**: Frequently accessed data
- **Query Optimization**: EF Core performance tuning
- **Lazy Loading**: Load data only when needed
- **Pagination**: Limit data transfer
- **Compression**: Gzip compression for responses
- **CDN**: Static asset delivery
- **Database Indexing**: Optimized query performance

### Monitoring & Observability

- **Performance Middleware**: Request timing and metrics
- **Structured Logging**: Searchable, queryable logs
- **Health Checks**: System health monitoring
- **Metrics Collection**: Performance counters
- **Error Tracking**: Exception monitoring
- **User Analytics**: Usage patterns

## Deployment Architecture

### Development Environment

```
Developer Machine
├── .NET 8 SDK
├── SQL Server LocalDB
├── Visual Studio/VS Code
└── Docker Desktop (optional)
```

### Production Environment (Azure)

```
┌─────────────────────────────────────────────────────────────┐
│                        Azure Cloud                         │
│                                                             │
│  ┌─────────────┐    ┌─────────────┐    ┌─────────────────┐  │
│  │   CDN       │    │  App Service│    │   Azure SQL     │  │
│  │ (Static     │    │   (API)     │    │   Database      │  │
│  │  Assets)    │    │             │    │                 │  │
│  └─────────────┘    └─────────────┘    └─────────────────┘  │
│                                                             │
│  ┌─────────────┐    ┌─────────────┐    ┌─────────────────┐  │
│  │Static Web   │    │ Application │    │   Key Vault     │  │
│  │App (Client) │    │  Insights   │    │  (Secrets)      │  │
│  │             │    │(Monitoring) │    │                 │  │
│  └─────────────┘    └─────────────┘    └─────────────────┘  │
└─────────────────────────────────────────────────────────────┘
```

### Container Architecture (Optional)

```
┌─────────────────────────────────────────────────────────────┐
│                     Docker Containers                      │
│                                                             │
│  ┌─────────────┐    ┌─────────────┐    ┌─────────────────┐  │
│  │   Nginx     │    │    API      │    │   SQL Server    │  │
│  │(Reverse     │    │ Container   │    │   Container     │  │
│  │ Proxy)      │    │             │    │                 │  │
│  └─────────────┘    └─────────────┘    └─────────────────┘  │
│                                                             │
│  ┌─────────────┐    ┌─────────────┐    ┌─────────────────┐  │
│  │   Client    │    │   Redis     │    │   Logging       │  │
│  │ Container   │    │   Cache     │    │   Container     │  │
│  │             │    │             │    │                 │  │
│  └─────────────┘    └─────────────┘    └─────────────────┘  │
└─────────────────────────────────────────────────────────────┘
```

## Design Patterns Used

### Repository Pattern
- Abstracts data access logic
- Enables unit testing with mocks
- Provides consistent interface for data operations

### Decorator Pattern
- Used for caching repositories
- Adds behavior without modifying original classes
- Maintains interface compatibility

### Dependency Injection
- Constructor injection throughout
- Interface-based dependencies
- Supports testing and flexibility

### CQRS (Command Query Responsibility Segregation)
- Separate read and write operations
- Optimized query models
- Enhanced performance and scalability

### Mediator Pattern
- Decouples request/response handling
- Centralizes cross-cutting concerns
- Simplifies testing and maintenance

## Scalability Considerations

### Horizontal Scaling
- Stateless API design
- Session data in distributed cache
- Load balancer ready

### Vertical Scaling
- Efficient memory usage
- Optimized database queries
- Resource monitoring

### Database Scaling
- Read replicas for reporting
- Connection pooling
- Query optimization

### Caching Strategy
- Multi-level caching
- Cache invalidation patterns
- Distributed caching support

## Future Architecture Enhancements

### Microservices Migration
- Service decomposition
- API gateway implementation
- Event-driven architecture

### Advanced Caching
- Redis distributed cache
- Cache-aside pattern
- Event-based cache invalidation

### Real-time Features
- SignalR integration
- Live workout tracking
- Social features

### Mobile Support
- MAUI application
- Offline synchronization
- Push notifications

---

*This architecture documentation reflects the current implementation and provides guidance for future enhancements.*
