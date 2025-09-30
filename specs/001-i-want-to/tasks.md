843237# Tasks: Workout Tracking System

**Input**: Design documents from `/specs/001-i-want-to/`
**Prerequisites**: plan.md (required), research.md, data-model.md, contracts/, quickstart.md

## Execution Flow (main)
```
1. Load plan.md from feature directory
   → Extract: C# (.NET 8), ASP.NET Core, Blazor WebAssembly, Entity Framework Core, SeriLog, Google OAuth
2. Load design documents:
   → data-model.md: 7 entities (User, WorkoutSession, ExerciseType, StrengthLift, MetconType, MetconWorkout, MovementType, MetconMovement)
   → contracts/: 2 contract test files + api-spec.yaml (25+ endpoints)
   → quickstart.md: 10 user stories for integration testing
3. Generate tasks by category:
   → Setup: .NET project structure, NuGet dependencies, tooling
   → Tests: contract tests, integration tests (TDD approach)
   → Core: domain entities, application services, API controllers
   → Integration: EF Core, Google OAuth, logging, middleware
   → Polish: unit tests, performance, documentation
4. Apply task rules:
   → Different projects/files = mark [P] for parallel
   → Same file = sequential (no [P])
   → Tests before implementation (TDD)
5. Number tasks sequentially (T001, T002...)
6. Generate dependency graph
7. Create parallel execution examples
8. Validate task completeness:
   → All contracts have tests? ✓
   → All entities have models? ✓
   → All endpoints implemented? ✓
9. Return: SUCCESS (tasks ready for execution)
```

## Format: `[ID] [P?] Description`
- **[P]**: Can run in parallel (different files, no dependencies)
- Include exact file paths in descriptions

## Path Conventions
Web application structure with clean architecture:
- **Domain**: `src/LiftTracker.Domain/`
- **Application**: `src/LiftTracker.Application/`
- **Infrastructure**: `src/LiftTracker.Infrastructure/`
- **API**: `src/LiftTracker.API/`
- **Client**: `src/LiftTracker.Client/`
- **Tests**: `tests/LiftTracker.*.Tests/`

## Phase 3.1: Setup
- [x] T001 Create .NET solution and project structure per implementation plan
- [x] T002 Initialize Domain project with core entities and interfaces
- [x] T003 [P] Initialize Application project with services and DTOs
- [x] T004 [P] Initialize Infrastructure project with EF Core and repositories
- [x] T005 [P] Initialize API project with ASP.NET Core controllers
- [x] T006 [P] Initialize Client project with Blazor WebAssembly
- [x] T007 [P] Initialize test projects for each layer
- [x] T008 [P] Configure NuGet packages: EF Core, Google OAuth, SeriLog, xUnit, bUnit
- [x] T009 [P] Configure linting and formatting tools (EditorConfig, StyleCop)
- [x] T010 [P] Setup Terraform infrastructure as code for Azure deployment

## Phase 3.2: Tests First (TDD) ⚠️ MUST COMPLETE BEFORE 3.3
**CRITICAL: These tests MUST be written and MUST FAIL before ANY implementation**
- [ ] T011 [P] Contract test for workout session endpoints in tests/LiftTracker.API.Tests/Contracts/WorkoutSessionContractTests.cs
- [ ] T012 [P] Contract test for strength lift endpoints in tests/LiftTracker.API.Tests/Contracts/StrengthLiftContractTests.cs
- [ ] T013 [P] Contract test for metcon workout endpoints in tests/LiftTracker.API.Tests/Contracts/MetconWorkoutContractTests.cs
- [ ] T014 [P] Contract test for user authentication endpoints in tests/LiftTracker.API.Tests/Contracts/AuthContractTests.cs
- [ ] T015 [P] Contract test for progress tracking endpoints in tests/LiftTracker.API.Tests/Contracts/ProgressContractTests.cs
- [ ] T016 [P] Integration test for user account creation (Story 1) in tests/LiftTracker.IntegrationTests/UserAccountTests.cs
- [ ] T017 [P] Integration test for workout session creation (Story 2) in tests/LiftTracker.IntegrationTests/WorkoutSessionTests.cs
- [ ] T018 [P] Integration test for strength lift logging (Stories 3-4) in tests/LiftTracker.IntegrationTests/StrengthLiftTests.cs
- [ ] T019 [P] Integration test for metcon workout logging (Stories 5-6) in tests/LiftTracker.IntegrationTests/MetconWorkoutTests.cs
- [ ] T020 [P] Integration test for data editing (Story 7) in tests/LiftTracker.IntegrationTests/DataEditingTests.cs
- [ ] T021 [P] Integration test for progress charts (Stories 8-9) in tests/LiftTracker.IntegrationTests/ProgressTrackingTests.cs
- [ ] T022 [P] Integration test for data isolation (Story 10) in tests/LiftTracker.IntegrationTests/DataIsolationTests.cs

## Phase 3.3: Domain Layer (ONLY after tests are failing)
- [ ] T023 [P] User entity in src/LiftTracker.Domain/Entities/User.cs
- [ ] T024 [P] WorkoutSession entity in src/LiftTracker.Domain/Entities/WorkoutSession.cs
- [ ] T025 [P] ExerciseType entity in src/LiftTracker.Domain/Entities/ExerciseType.cs
- [ ] T026 [P] StrengthLift entity in src/LiftTracker.Domain/Entities/StrengthLift.cs
- [ ] T027 [P] MetconType entity in src/LiftTracker.Domain/Entities/MetconType.cs
- [ ] T028 [P] MetconWorkout entity in src/LiftTracker.Domain/Entities/MetconWorkout.cs
- [ ] T029 [P] MovementType entity in src/LiftTracker.Domain/Entities/MovementType.cs
- [ ] T030 [P] MetconMovement entity in src/LiftTracker.Domain/Entities/MetconMovement.cs
- [ ] T031 [P] Domain interfaces in src/LiftTracker.Domain/Interfaces/
- [ ] T032 [P] Value objects (Weight, Duration) in src/LiftTracker.Domain/ValueObjects/

## Phase 3.4: Infrastructure Layer
- [ ] T033 EF Core DbContext configuration in src/LiftTracker.Infrastructure/Data/LiftTrackerDbContext.cs
- [ ] T034 [P] User repository in src/LiftTracker.Infrastructure/Repositories/UserRepository.cs
- [ ] T035 [P] WorkoutSession repository in src/LiftTracker.Infrastructure/Repositories/WorkoutSessionRepository.cs
- [ ] T036 [P] StrengthLift repository in src/LiftTracker.Infrastructure/Repositories/StrengthLiftRepository.cs
- [ ] T037 [P] MetconWorkout repository in src/LiftTracker.Infrastructure/Repositories/MetconWorkoutRepository.cs
- [ ] T038 [P] ExerciseType repository in src/LiftTracker.Infrastructure/Repositories/ExerciseTypeRepository.cs
- [ ] T039 Database migrations and seed data configuration
- [ ] T040 [P] Google OAuth authentication configuration in src/LiftTracker.Infrastructure/Authentication/
- [ ] T041 [P] SeriLog logging configuration in src/LiftTracker.Infrastructure/Logging/

## Phase 3.5: Application Layer
- [ ] T042 [P] User service in src/LiftTracker.Application/Services/UserService.cs
- [ ] T043 [P] WorkoutSession service in src/LiftTracker.Application/Services/WorkoutSessionService.cs
- [ ] T044 [P] StrengthLift service in src/LiftTracker.Application/Services/StrengthLiftService.cs
- [ ] T045 [P] MetconWorkout service in src/LiftTracker.Application/Services/MetconWorkoutService.cs
- [ ] T046 [P] Progress tracking service in src/LiftTracker.Application/Services/ProgressService.cs
- [ ] T047 [P] DTOs for all entities in src/LiftTracker.Application/DTOs/
- [ ] T048 [P] AutoMapper configuration in src/LiftTracker.Application/Mappers/
- [ ] T049 [P] Application service interfaces in src/LiftTracker.Application/Interfaces/

## Phase 3.6: API Layer
- [ ] T050 Authentication controller (login, logout) in src/LiftTracker.API/Controllers/AuthController.cs
- [ ] T051 Users controller (profile management) in src/LiftTracker.API/Controllers/UsersController.cs
- [ ] T052 WorkoutSessions controller (CRUD) in src/LiftTracker.API/Controllers/WorkoutSessionsController.cs
- [ ] T053 StrengthLifts controller (CRUD) in src/LiftTracker.API/Controllers/StrengthLiftsController.cs
- [ ] T054 MetconWorkouts controller (CRUD) in src/LiftTracker.API/Controllers/MetconWorkoutsController.cs
- [ ] T055 Progress controller (charts, analytics) in src/LiftTracker.API/Controllers/ProgressController.cs
- [ ] T056 ExerciseTypes controller (reference data) in src/LiftTracker.API/Controllers/ExerciseTypesController.cs
- [ ] T057 [P] Input validation middleware in src/LiftTracker.API/Middleware/ValidationMiddleware.cs
- [ ] T058 [P] Error handling middleware in src/LiftTracker.API/Middleware/ErrorHandlingMiddleware.cs
- [ ] T059 [P] Authentication middleware configuration in src/LiftTracker.API/Middleware/AuthenticationMiddleware.cs
- [ ] T060 API configuration and startup in src/LiftTracker.API/Program.cs

## Phase 3.7: Client Layer (Blazor WebAssembly)
- [ ] T061 [P] Authentication service in src/LiftTracker.Client/Services/AuthService.cs
- [ ] T062 [P] API client service in src/LiftTracker.Client/Services/ApiClient.cs
- [ ] T063 [P] State management service in src/LiftTracker.Client/Services/StateService.cs
- [ ] T064 [P] Login page component in src/LiftTracker.Client/Pages/Login.razor
- [ ] T065 [P] Dashboard page component in src/LiftTracker.Client/Pages/Dashboard.razor
- [ ] T066 [P] Workout session form component in src/LiftTracker.Client/Components/WorkoutSessionForm.razor
- [ ] T067 [P] Strength lift form component in src/LiftTracker.Client/Components/StrengthLiftForm.razor
- [ ] T068 [P] Metcon workout form component in src/LiftTracker.Client/Components/MetconWorkoutForm.razor
- [ ] T069 [P] Progress charts component in src/LiftTracker.Client/Components/ProgressCharts.razor
- [ ] T070 [P] Navigation component in src/LiftTracker.Client/Components/Navigation.razor
- [ ] T071 [P] Responsive layout with Tailwind CSS in src/LiftTracker.Client/Shared/MainLayout.razor
- [ ] T072 [P] Client-side routing configuration in src/LiftTracker.Client/App.razor

## Phase 3.8: Integration
- [ ] T073 Connect all services to repositories with dependency injection
- [ ] T074 Configure EF Core database connection and migrations
- [ ] T075 Integrate Google OAuth with frontend authentication flow
- [ ] T076 Configure CORS policy for client-server communication
- [ ] T077 Setup structured logging with SeriLog across all layers
- [ ] T078 Configure security headers and HTTPS enforcement
- [ ] T079 Setup health checks and monitoring endpoints
- [ ] T080 Configure application settings and environment variables

## Phase 3.9: Polish
- [ ] T081 [P] Unit tests for domain entities in tests/LiftTracker.Domain.Tests/
- [ ] T082 [P] Unit tests for application services in tests/LiftTracker.Application.Tests/
- [ ] T083 [P] Unit tests for API controllers in tests/LiftTracker.API.Tests/
- [ ] T084 [P] Blazor component tests in tests/LiftTracker.Client.Tests/
- [ ] T085 [P] Performance optimization and caching strategies
- [ ] T086 [P] API documentation with Swagger/OpenAPI
- [ ] T087 [P] User documentation and deployment guides
- [ ] T088 Remove code duplication and refactor for clarity
- [ ] T089 Execute quickstart.md validation scenarios

## Phase 3.10: Constitutional Compliance
- [ ] T090 [P] Run architecture tests for clean architecture compliance
- [ ] T091 [P] Verify test coverage thresholds (80% unit, 70% integration)
- [ ] T092 [P] Security scan with SAST and dependency vulnerability checking
- [ ] T093 [P] Performance benchmark validation (<2s page load, <500ms API)
- [ ] T094 [P] WCAG 2.1 AA accessibility compliance check for Blazor components
- [ ] T095 [P] Code style and naming convention validation with StyleCop
- [ ] T096 [P] Constitutional principle compliance verification
- [ ] T097 [P] Azure deployment and infrastructure validation

## Dependencies
- Setup (T001-T010) before everything else
- Tests (T011-T022) before any implementation (T023+)
- Domain entities (T023-T032) before repositories (T034-T038)
- Repositories (T034-T038) before services (T042-T046)
- Services (T042-T049) before controllers (T050-T056)
- API controllers (T050-T060) before client services (T061-T063)
- Authentication (T040, T050, T061) before protected endpoints
- All core implementation before integration (T073-T080)
- Integration complete before polish (T081-T089)
- Everything complete before constitutional compliance (T090-T097)

## Parallel Execution Examples

### Phase 3.1 Setup (can run T003-T007 in parallel):
```
Task: "Initialize Application project with services and DTOs"
Task: "Initialize Infrastructure project with EF Core and repositories"
Task: "Initialize API project with ASP.NET Core controllers"
Task: "Initialize Client project with Blazor WebAssembly"
Task: "Initialize test projects for each layer"
```

### Phase 3.2 Contract Tests (can run T011-T015 in parallel):
```
Task: "Contract test for workout session endpoints in tests/LiftTracker.API.Tests/Contracts/WorkoutSessionContractTests.cs"
Task: "Contract test for strength lift endpoints in tests/LiftTracker.API.Tests/Contracts/StrengthLiftContractTests.cs"
Task: "Contract test for metcon workout endpoints in tests/LiftTracker.API.Tests/Contracts/MetconWorkoutContractTests.cs"
Task: "Contract test for user authentication endpoints in tests/LiftTracker.API.Tests/Contracts/AuthContractTests.cs"
Task: "Contract test for progress tracking endpoints in tests/LiftTracker.API.Tests/Contracts/ProgressContractTests.cs"
```

### Phase 3.3 Domain Entities (can run T023-T030 in parallel):
```
Task: "User entity in src/LiftTracker.Domain/Entities/User.cs"
Task: "WorkoutSession entity in src/LiftTracker.Domain/Entities/WorkoutSession.cs"
Task: "ExerciseType entity in src/LiftTracker.Domain/Entities/ExerciseType.cs"
Task: "StrengthLift entity in src/LiftTracker.Domain/Entities/StrengthLift.cs"
Task: "MetconType entity in src/LiftTracker.Domain/Entities/MetconType.cs"
Task: "MetconWorkout entity in src/LiftTracker.Domain/Entities/MetconWorkout.cs"
Task: "MovementType entity in src/LiftTracker.Domain/Entities/MovementType.cs"
Task: "MetconMovement entity in src/LiftTracker.Domain/Entities/MetconMovement.cs"
```

### Phase 3.4 Repositories (can run T034-T038 in parallel after T033):
```
Task: "User repository in src/LiftTracker.Infrastructure/Repositories/UserRepository.cs"
Task: "WorkoutSession repository in src/LiftTracker.Infrastructure/Repositories/WorkoutSessionRepository.cs"
Task: "StrengthLift repository in src/LiftTracker.Infrastructure/Repositories/StrengthLiftRepository.cs"
Task: "MetconWorkout repository in src/LiftTracker.Infrastructure/Repositories/MetconWorkoutRepository.cs"
Task: "ExerciseType repository in src/LiftTracker.Infrastructure/Repositories/ExerciseTypeRepository.cs"
```

## Task Validation Checklist
- ✅ All 7 entities from data-model.md have creation tasks
- ✅ All API endpoints from contracts/ have implementation tasks
- ✅ All 10 user stories from quickstart.md have integration tests
- ✅ Clean architecture layers properly separated
- ✅ TDD approach with tests before implementation
- ✅ Constitutional compliance verification included
- ✅ Parallel execution optimized for independent work
- ✅ Dependencies clearly mapped and enforced