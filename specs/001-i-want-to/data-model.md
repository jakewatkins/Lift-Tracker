# Data Model: Workout Tracking System

## Entity Relationship Overview

```
User ||--o{ WorkoutSession : "has many"
WorkoutSession ||--o{ StrengthLift : "contains"
WorkoutSession ||--o{ MetconWorkout : "contains"
StrengthLift }o--|| ExerciseType : "references"
MetconWorkout }o--|| MetconType : "references"
MetconWorkout ||--o{ MetconMovement : "contains"
MetconMovement }o--|| MovementType : "references"
```

## Core Entities

### User
**Purpose**: Represents individual users with authentication and profile data
**Domain Rules**: Email must be unique, name required, data isolation enforced

| Field | Type | Rules | Description |
|-------|------|-------|-------------|
| Id | Guid | PK, Required | Unique user identifier |
| Email | string(255) | Required, Unique, Email format | Google OAuth email |
| Name | string(100) | Required | Display name from Google profile |
| CreatedDate | DateTime | Required, UTC | Account creation timestamp |
| LastLoginDate | DateTime? | Optional, UTC | Last authentication timestamp |

**Validation Rules**:
- Email: Valid email format, maximum 255 characters
- Name: Required, 1-100 characters, no special characters except spaces, hyphens, apostrophes

### WorkoutSession
**Purpose**: Groups related exercises performed on a specific date
**Domain Rules**: One session per user per date, can contain multiple lifts and metcons

| Field | Type | Rules | Description |
|-------|------|-------|-------------|
| Id | Guid | PK, Required | Unique session identifier |
| UserId | Guid | FK, Required | Reference to User |
| Date | DateOnly | Required | Workout date (user's local timezone) |
| Notes | string(1000) | Optional | General session notes |
| CreatedDate | DateTime | Required, UTC | Session creation timestamp |
| ModifiedDate | DateTime | Required, UTC | Last modification timestamp |

**Validation Rules**:
- Date: Cannot be future date, required
- Notes: Maximum 1000 characters
- User association: Must belong to authenticated user

### ExerciseType
**Purpose**: Predefined catalog of strength exercises
**Domain Rules**: System-managed reference data, extensible

| Field | Type | Rules | Description |
|-------|------|-------|-------------|
| Id | int | PK, Identity | Unique exercise identifier |
| Name | string(100) | Required, Unique | Exercise name (e.g., "Back Squat") |
| Category | string(50) | Required | Exercise category (e.g., "Squat", "Press") |
| IsActive | bool | Required, Default: true | Available for selection |

**Initial Data**:
```
Squat Category: Back Squat, Front Squat, Overhead Squat, Box Squat
Press Category: Bench Press, Overhead Press, Incline Press, Dumbbell Press
Deadlift Category: Conventional Deadlift, Sumo Deadlift, Romanian Deadlift
Pull Category: Pull-ups, Chin-ups, Rows, Lat Pulldowns
Olympic Category: Clean, Jerk, Snatch, Clean & Jerk
```

### StrengthLift
**Purpose**: Records individual strength training sets within a workout session
**Domain Rules**: Supports various set structures, fractional weights, rest tracking

| Field | Type | Rules | Description |
|-------|------|-------|-------------|
| Id | Guid | PK, Required | Unique lift identifier |
| WorkoutSessionId | Guid | FK, Required | Reference to WorkoutSession |
| ExerciseTypeId | int | FK, Required | Reference to ExerciseType |
| SetStructure | string(20) | Required | Set type: "SetsReps", "EMOM", "AMRAP", "TimeBased" |
| Sets | int? | Optional | Number of sets (null for AMRAP/TimeBased) |
| Reps | int? | Optional | Reps per set (null for time-based) |
| Weight | decimal(5,2) | Required, ≥0 | Weight in pounds (0 for bodyweight) |
| AdditionalWeight | decimal(5,2) | Optional, ≥0 | Extra weight for bodyweight exercises |
| Duration | decimal(4,2) | Optional, ≥0 | Set duration in minutes |
| RestPeriod | decimal(4,2) | Optional, ≥0 | Rest between sets in minutes |
| Comments | string(500) | Optional | Set-specific notes |
| Order | int | Required, ≥1 | Order within workout session |

**Validation Rules**:
- Weight: Fractional increments of 0.25 only (2.25, 2.5, 2.75, 3.0)
- Duration/RestPeriod: Fractional increments of 0.25 only
- Sets: 1-50 range when applicable
- Reps: 1-500 range when applicable
- Comments: Maximum 500 characters

### MetconType
**Purpose**: Predefined catalog of metabolic conditioning workout types
**Domain Rules**: System-managed reference data

| Field | Type | Rules | Description |
|-------|------|-------|-------------|
| Id | int | PK, Identity | Unique metcon type identifier |
| Name | string(50) | Required, Unique | Metcon type name |
| Description | string(200) | Optional | Type description |
| IsActive | bool | Required, Default: true | Available for selection |

**Initial Data**:
```
AMRAP: As Many Rounds/Reps As Possible
For Time: Complete prescribed work as fast as possible
EMOM: Every Minute On the Minute
TABATA: 20 seconds work, 10 seconds rest, 8 rounds
Intervals: Timed work and rest periods
Chipper: Complete list of movements in order
```

### MetconWorkout
**Purpose**: Records metabolic conditioning workouts within a session
**Domain Rules**: Contains multiple movements, tracks time and notes

| Field | Type | Rules | Description |
|-------|------|-------|-------------|
| Id | Guid | PK, Required | Unique metcon identifier |
| WorkoutSessionId | Guid | FK, Required | Reference to WorkoutSession |
| MetconTypeId | int | FK, Required | Reference to MetconType |
| TotalTime | decimal(6,2) | Optional, ≥0 | Total workout time in minutes |
| RoundsCompleted | int? | Optional, ≥0 | Rounds completed (AMRAP) |
| Notes | string(1000) | Optional | Workout notes |
| Order | int | Required, ≥1 | Order within workout session |

**Validation Rules**:
- TotalTime: Fractional increments of 0.25 only
- RoundsCompleted: 0-1000 range
- Notes: Maximum 1000 characters

### MovementType
**Purpose**: Catalog of movements used in metcon workouts
**Domain Rules**: Supports both rep-based and distance-based movements

| Field | Type | Rules | Description |
|-------|------|-------|-------------|
| Id | int | PK, Identity | Unique movement identifier |
| Name | string(100) | Required, Unique | Movement name |
| Category | string(50) | Required | Movement category |
| MeasurementType | string(20) | Required | "Reps" or "Distance" |
| IsActive | bool | Required, Default: true | Available for selection |

**Initial Data**:
```
Bodyweight Category (Reps): Burpees, Push-ups, Air Squats, Mountain Climbers
Cardio Category (Distance): Running, Rowing, Biking, Swimming
Gymnastics Category (Reps): Pull-ups, Handstand Push-ups, Muscle-ups
Weightlifting Category (Reps): Thrusters, Deadlifts, Box Jumps, Wall Balls
```

### MetconMovement
**Purpose**: Individual movements within a metcon workout
**Domain Rules**: Supports reps or distance measurement, optional weight

| Field | Type | Rules | Description |
|-------|------|-------|-------------|
| Id | Guid | PK, Required | Unique movement identifier |
| MetconWorkoutId | Guid | FK, Required | Reference to MetconWorkout |
| MovementTypeId | int | FK, Required | Reference to MovementType |
| Reps | int? | Optional, ≥0 | Number of reps (for rep-based movements) |
| Distance | decimal(8,2) | Optional, ≥0 | Distance in meters (for distance-based) |
| Weight | decimal(5,2) | Optional, ≥0 | Weight used in pounds |
| Order | int | Required, ≥1 | Order within metcon |

**Validation Rules**:
- Reps: 1-10000 range for rep-based movements
- Distance: Positive decimal for distance-based movements (meters)
- Weight: Fractional increments of 0.25 only when applicable
- Must have either Reps or Distance based on MovementType.MeasurementType

## Database Indexes

### Performance Indexes
```sql
-- User session lookups
CREATE INDEX IX_WorkoutSession_UserId_Date ON WorkoutSession (UserId, Date DESC);

-- Session content queries
CREATE INDEX IX_StrengthLift_WorkoutSessionId_Order ON StrengthLift (WorkoutSessionId, Order);
CREATE INDEX IX_MetconWorkout_WorkoutSessionId_Order ON MetconWorkout (WorkoutSessionId, Order);
CREATE INDEX IX_MetconMovement_MetconWorkoutId_Order ON MetconMovement (MetconWorkoutId, Order);

-- Progress tracking queries
CREATE INDEX IX_StrengthLift_UserId_ExerciseTypeId_Date ON StrengthLift (UserId, ExerciseTypeId, Date DESC);
CREATE INDEX IX_MetconWorkout_UserId_MetconTypeId_Date ON MetconWorkout (UserId, MetconTypeId, Date DESC);
```

### Unique Constraints
```sql
-- One session per user per date
ALTER TABLE WorkoutSession ADD CONSTRAINT UQ_WorkoutSession_UserId_Date UNIQUE (UserId, Date);

-- Unique exercise types
ALTER TABLE ExerciseType ADD CONSTRAINT UQ_ExerciseType_Name UNIQUE (Name);

-- Unique movement types
ALTER TABLE MovementType ADD CONSTRAINT UQ_MovementType_Name UNIQUE (Name);
```

## Data Migration Strategy

### Seed Data
- ExerciseType: 25+ common strength exercises
- MetconType: 6 standard metcon formats
- MovementType: 50+ common metcon movements

### User Data Migration
- No existing data to migrate (new application)
- Google OAuth profile data imported on first login
- User timezone handling via client-side detection

## State Transitions

### WorkoutSession Lifecycle
1. **Draft**: User creates session, adds exercises
2. **Active**: User logging data during workout
3. **Complete**: Session finalized with all data

### Data Modification Rules
- Sessions: Full CRUD for session owner
- Historical data: Edit allowed for data corrections
- Reference data: Admin-only modifications

## Data Validation Summary

### Input Validation
- **Fractional weights**: Only .0, .25, .5, .75 increments allowed
- **Time values**: Only .0, .25, .5, .75 increments in minutes
- **Required fields**: All FK relationships, user associations
- **Positive numbers**: All numeric fitness metrics ≥ 0

### Business Rules
- **User isolation**: All queries filtered by authenticated UserId
- **Date constraints**: Workout dates cannot be in the future
- **Measurement consistency**: MovementType determines Reps vs Distance requirement
- **Order integrity**: Sequential ordering within sessions and metcons