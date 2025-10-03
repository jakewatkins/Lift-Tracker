
# Implementation Plan: Workout Tracking System

**Branch**: `001-i-want-to` | **Date**: 2025-09-29 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/001-i-want-to/spec.md`

## Execution Flow (/plan command scope)
```
1. Load feature spec from Input path
   → If not found: ERROR "No feature spec at {path}"
2. Fill Technical Context (scan for NEEDS CLARIFICATION)
   → Detect Project Type from file system structure or context (web=frontend+backend, mobile=app+api)
   → Set Structure Decision based on project type
3. Fill the Constitution Check section based on the content of the constitution document.
4. Evaluate Constitution Check section below
   → If violations exist: Document in Complexity Tracking
   → If no justification possible: ERROR "Simplify approach first"
   → Update Progress Tracking: Initial Constitution Check
5. Execute Phase 0 → research.md
   → If NEEDS CLARIFICATION remain: ERROR "Resolve unknowns"
6. Execute Phase 1 → contracts, data-model.md, quickstart.md, agent-specific template file (e.g., `CLAUDE.md` for Claude Code, `.github/copilot-instructions.md` for GitHub Copilot, `GEMINI.md` for Gemini CLI, `QWEN.md` for Qwen Code or `AGENTS.md` for opencode).
7. Re-evaluate Constitution Check section
   → If new violations: Refactor design, return to Phase 1
   → Update Progress Tracking: Post-Design Constitution Check
8. Plan Phase 2 → Describe task generation approach (DO NOT create tasks.md)
9. STOP - Ready for /tasks command
```

**IMPORTANT**: The /plan command STOPS at step 7. Phases 2-4 are executed by other commands:
- Phase 2: /tasks command creates tasks.md
- Phase 3-4: Implementation execution (manual or via tools)

## Summary
A multi-user workout tracking application that allows fitness enthusiasts to log strength training and metabolic conditioning workouts with comprehensive progress tracking. Users create accounts with name/email and record detailed workout data including lifts (sets, reps, weights), metcons (types, movements, times), and view progress charts over 30/60/90-day periods. Technical approach uses Blazor WebAssembly frontend with ASP.NET Core backend, SQL Server database, and Google OAuth authentication following clean architecture principles.

## Technical Context
**Language/Version**: C# (.NET 8)  
**Primary Dependencies**: ASP.NET Core, Blazor WebAssembly, Entity Framework Core, SeriLog, Google OAuth  
**Storage**: Microsoft SQL Server with Entity Framework Core (code-first)  
**Testing**: xUnit, bUnit (Blazor components), integration tests  
**Target Platform**: Web application (responsive design for desktop, tablet, mobile)
**Project Type**: web - frontend (Blazor WASM) + backend (ASP.NET Core API)  
**Performance Goals**: <2s initial page load on mobile, <500ms API response (95th percentile)  
**Constraints**: WCAG 2.1 AA accessibility, HTTPS enforced, 80% unit test coverage  
**Scale/Scope**: Multi-user application with user data isolation, progress tracking, Azure deployment

## Constitution Check
*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

**Clean Architecture & SOLID Principles**: ✅ PASS - Planned architecture follows clean architecture with Domain layer (entities), Application layer (services), Infrastructure layer (EF Core, external APIs), and Presentation layer (Blazor components). SOLID principles maintained through dependency injection, interface segregation, and single responsibility design.

**Test-First Development**: ✅ PASS - TDD approach planned with xUnit for unit tests, bUnit for Blazor component tests, and integration tests. Tasks ordered as test creation → implementation. 80% unit test coverage and 70% integration test coverage requirements addressed.

**User Experience Consistency**: ✅ PASS - Responsive design with Tailwind CSS, WCAG 2.1 AA accessibility compliance planned, consistent design patterns across desktop/tablet/mobile. Mobile-first approach with defined breakpoints.

**Security-First Implementation**: ✅ PASS - Google OAuth2 authentication via ASP.NET Identity, HTTPS enforcement, input validation, output encoding, user data isolation. Snyk vulnerability scanning and dependency audits included in CI/CD.

**Performance Excellence**: ✅ PASS - Performance benchmarks defined (<2s page load on mobile, <500ms API response for 95th percentile). New Relic monitoring for backend performance tracking and profiling during development.

**Quality Assurance**: ✅ PASS - Roslyn analyzers, StyleCop for naming conventions, SonarAnalyzer.CSharp for code quality. GitHub Actions CI/CD with automated code review gates and coverage enforcement.

## Project Structure

### Documentation (this feature)
```
specs/[###-feature]/
├── plan.md              # This file (/plan command output)
├── research.md          # Phase 0 output (/plan command)
├── data-model.md        # Phase 1 output (/plan command)
├── quickstart.md        # Phase 1 output (/plan command)
├── contracts/           # Phase 1 output (/plan command)
└── tasks.md             # Phase 2 output (/tasks command - NOT created by /plan)
```

### Source Code (repository root)
```
# Web application structure (Blazor WebAssembly + ASP.NET Core)
src/
├── LiftTracker.Domain/           # Core business logic and entities
│   ├── Entities/
│   ├── ValueObjects/
│   ├── Interfaces/
│   └── Services/
├── LiftTracker.Application/      # Application services and use cases
│   ├── Services/
│   ├── DTOs/
│   ├── Interfaces/
│   └── Mappers/
├── LiftTracker.Infrastructure/   # Data access and external services
│   ├── Data/
│   ├── Repositories/
│   ├── Authentication/
│   └── Logging/
├── LiftTracker.API/             # ASP.NET Core Web API
│   ├── Controllers/
│   ├── Middleware/
│   ├── Configuration/
│   └── Program.cs
└── LiftTracker.Client/          # Blazor WebAssembly frontend
    ├── Components/
    ├── Pages/
    ├── Services/
    ├── Models/
    └── wwwroot/

tests/
├── LiftTracker.Domain.Tests/     # Unit tests for domain logic
├── LiftTracker.Application.Tests/ # Unit tests for application services
├── LiftTracker.API.Tests/        # Integration tests for API
├── LiftTracker.Client.Tests/     # Component tests for Blazor UI
└── LiftTracker.IntegrationTests/ # End-to-end integration tests

infrastructure/
└── terraform/                   # Azure infrastructure as code
    ├── main.tf
    ├── variables.tf
    └── outputs.tf
```

**Structure Decision**: Selected web application structure with clean architecture layers. Domain layer contains core business entities and interfaces. Application layer handles use cases and business logic. Infrastructure layer manages data persistence, authentication, and external services. API project provides REST endpoints. Client project delivers Blazor WebAssembly frontend. Separate test projects for each layer following TDD principles.

## Phase 0: Outline & Research
1. **Extract unknowns from Technical Context** above:
   - For each NEEDS CLARIFICATION → research task
   - For each dependency → best practices task
   - For each integration → patterns task

2. **Generate and dispatch research agents**:
   ```
   For each unknown in Technical Context:
     Task: "Research {unknown} for {feature context}"
   For each technology choice:
     Task: "Find best practices for {tech} in {domain}"
   ```

3. **Consolidate findings** in `research.md` using format:
   - Decision: [what was chosen]
   - Rationale: [why chosen]
   - Alternatives considered: [what else evaluated]

**Output**: research.md with all NEEDS CLARIFICATION resolved

## Phase 1: Design & Contracts
*Prerequisites: research.md complete*

1. **Extract entities from feature spec** → `data-model.md`:
   - Entity name, fields, relationships
   - Validation rules from requirements
   - State transitions if applicable

2. **Generate API contracts** from functional requirements:
   - For each user action → endpoint
   - Use standard REST/GraphQL patterns
   - Output OpenAPI/GraphQL schema to `/contracts/`

3. **Generate contract tests** from contracts:
   - One test file per endpoint
   - Assert request/response schemas
   - Tests must fail (no implementation yet)

4. **Extract test scenarios** from user stories:
   - Each story → integration test scenario
   - Quickstart test = story validation steps

5. **Update agent file incrementally** (O(1) operation):
   - Run `.specify/scripts/bash/update-agent-context.sh copilot`
     **IMPORTANT**: Execute it exactly as specified above. Do not add or remove any arguments.
   - If exists: Add only NEW tech from current plan
   - Preserve manual additions between markers
   - Update recent changes (keep last 3)
   - Keep under 150 lines for token efficiency
   - Output to repository root

**Output**: data-model.md, /contracts/*, failing tests, quickstart.md, agent-specific file

## Phase 2: Task Planning Approach
*This section describes what the /tasks command will do - DO NOT execute during /plan*

**Task Generation Strategy**:
- Load `.specify/templates/tasks-template.md` as base
- Generate tasks from Phase 1 design docs (contracts, data model, quickstart)
- Each contract → contract test task [P]
- Each entity → model creation task [P] 
- Each user story → integration test task
- Implementation tasks to make tests pass

**Ordering Strategy**:
- TDD order: Tests before implementation 
- Dependency order: Models before services before UI
- Mark [P] for parallel execution (independent files)

**Estimated Output**: 25-30 numbered, ordered tasks in tasks.md

**IMPORTANT**: This phase is executed by the /tasks command, NOT by /plan

## Phase 3+: Future Implementation
*These phases are beyond the scope of the /plan command*

**Phase 3**: Task execution (/tasks command creates tasks.md)  
**Phase 4**: Implementation (execute tasks.md following constitutional principles)  
**Phase 5**: Validation (run tests, execute quickstart.md, performance validation)

## Complexity Tracking
*Fill ONLY if Constitution Check has violations that must be justified*

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| [e.g., 4th project] | [current need] | [why 3 projects insufficient] |
| [e.g., Repository pattern] | [specific problem] | [why direct DB access insufficient] |


## Progress Tracking
*This checklist is updated during execution flow*

**Phase Status**:
- [x] Phase 0: Research complete (/plan command)
- [x] Phase 1: Design complete (/plan command)
- [x] Phase 2: Task planning complete (/plan command - describe approach only)
- [ ] Phase 3: Tasks generated (/tasks command)
- [ ] Phase 4: Implementation complete
- [ ] Phase 5: Validation passed

**Gate Status**:
- [x] Initial Constitution Check: PASS
- [x] Post-Design Constitution Check: PASS
- [x] All NEEDS CLARIFICATION resolved
- [x] Complexity deviations documented

---
*Based on Constitution v1.0.0 - See `.specify/memory/constitution.md`*
