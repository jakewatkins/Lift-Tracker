# LiftTracker API Documentation

## Overview

The LiftTracker API is a comprehensive RESTful service for workout tracking, built with ASP.NET Core 8 and following OpenAPI 3.0 specifications.

**Base URL**: 
- Development: `https://localhost:7001`
- Production: `https://api.lifttracker.com`

**API Version**: 1.0.0

**Interactive Documentation**: Visit `/swagger` for the Swagger UI interface

## Authentication

### JWT Bearer Token Authentication

All endpoints (except authentication and health checks) require a valid JWT token in the Authorization header.

**Header Format**:
```
Authorization: Bearer <your-jwt-token>
```

### Authentication Flow

1. **Google OAuth Login**
   ```http
   GET /api/auth/google
   ```
   Redirects to Google OAuth consent screen

2. **OAuth Callback**
   ```http
   GET /api/auth/callback?code=<authorization-code>
   ```
   Exchanges authorization code for JWT token

3. **Token Response**
   ```json
   {
     "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
     "expiration": "2025-10-03T10:30:00Z",
     "user": {
       "id": "123e4567-e89b-12d3-a456-426614174000",
       "email": "user@example.com",
       "name": "John Doe"
     }
   }
   ```

## API Endpoints

### Authentication Endpoints

#### Google OAuth Login
```http
GET /api/auth/google
```

**Description**: Initiates Google OAuth authentication flow

**Response**: Redirects to Google OAuth consent screen

**Example**:
```bash
curl -X GET "https://localhost:7001/api/auth/google"
```

---

#### OAuth Callback
```http
GET /api/auth/callback
```

**Description**: Handles OAuth callback and issues JWT token

**Query Parameters**:
- `code` (string, required): Authorization code from Google
- `state` (string, optional): State parameter for CSRF protection

**Response**: JWT token and user information

---

#### Token Refresh
```http
POST /api/auth/refresh
```

**Description**: Refreshes an expired JWT token

**Request Body**:
```json
{
  "refreshToken": "string"
}
```

**Response**: New JWT token

---

### User Management Endpoints

#### Get Current User
```http
GET /api/users/me
```

**Description**: Retrieves the authenticated user's profile

**Authorization**: Required

**Response**:
```json
{
  "id": "123e4567-e89b-12d3-a456-426614174000",
  "email": "user@example.com",
  "name": "John Doe",
  "createdAt": "2025-01-01T00:00:00Z",
  "updatedAt": "2025-10-02T10:30:00Z"
}
```

---

#### Update User Profile
```http
PUT /api/users/me
```

**Description**: Updates the authenticated user's profile

**Authorization**: Required

**Request Body**:
```json
{
  "name": "John Smith",
  "email": "johnsmith@example.com"
}
```

**Response**: Updated user object

---

#### Get User by ID
```http
GET /api/users/{id}
```

**Description**: Retrieves a specific user by ID (admin only)

**Authorization**: Required (Admin role)

**Path Parameters**:
- `id` (UUID, required): User ID

**Response**: User object

---

### Workout Session Endpoints

#### Get User's Workout Sessions
```http
GET /api/workoutsessions
```

**Description**: Retrieves all workout sessions for the authenticated user

**Authorization**: Required

**Query Parameters**:
- `page` (integer, optional, default: 1): Page number for pagination
- `pageSize` (integer, optional, default: 20): Number of items per page
- `startDate` (date, optional): Filter sessions from this date
- `endDate` (date, optional): Filter sessions to this date

**Response**:
```json
{
  "data": [
    {
      "id": "123e4567-e89b-12d3-a456-426614174000",
      "userId": "123e4567-e89b-12d3-a456-426614174000",
      "startTime": "2025-10-02T09:00:00Z",
      "endTime": "2025-10-02T10:30:00Z",
      "notes": "Great workout today!",
      "strengthLifts": [],
      "metconWorkouts": []
    }
  ],
  "totalCount": 1,
  "page": 1,
  "pageSize": 20,
  "totalPages": 1
}
```

---

#### Create Workout Session
```http
POST /api/workoutsessions
```

**Description**: Creates a new workout session

**Authorization**: Required

**Request Body**:
```json
{
  "startTime": "2025-10-02T09:00:00Z",
  "notes": "Morning workout session"
}
```

**Response**: Created workout session object (HTTP 201)

---

#### Get Workout Session by ID
```http
GET /api/workoutsessions/{id}
```

**Description**: Retrieves a specific workout session

**Authorization**: Required

**Path Parameters**:
- `id` (UUID, required): Workout session ID

**Response**: Workout session object with all exercises

---

#### Update Workout Session
```http
PUT /api/workoutsessions/{id}
```

**Description**: Updates an existing workout session

**Authorization**: Required

**Path Parameters**:
- `id` (UUID, required): Workout session ID

**Request Body**:
```json
{
  "endTime": "2025-10-02T10:30:00Z",
  "notes": "Updated notes"
}
```

**Response**: Updated workout session object

---

#### Delete Workout Session
```http
DELETE /api/workoutsessions/{id}
```

**Description**: Deletes a workout session and all associated exercises

**Authorization**: Required

**Path Parameters**:
- `id` (UUID, required): Workout session ID

**Response**: HTTP 204 No Content

---

### Strength Training Endpoints

#### Get Strength Lifts for Session
```http
GET /api/strengthlifts/session/{sessionId}
```

**Description**: Retrieves all strength lifts for a workout session

**Authorization**: Required

**Path Parameters**:
- `sessionId` (UUID, required): Workout session ID

**Response**:
```json
[
  {
    "id": "123e4567-e89b-12d3-a456-426614174000",
    "workoutSessionId": "123e4567-e89b-12d3-a456-426614174000",
    "exerciseTypeId": "123e4567-e89b-12d3-a456-426614174000",
    "exerciseType": {
      "name": "Back Squat",
      "category": "Compound",
      "muscleGroups": ["Quadriceps", "Glutes"]
    },
    "sets": [
      {
        "setNumber": 1,
        "weight": 135.0,
        "reps": 10,
        "rpe": 7.0
      },
      {
        "setNumber": 2,
        "weight": 155.0,
        "reps": 8,
        "rpe": 8.0
      }
    ],
    "notes": "Felt strong today"
  }
]
```

---

#### Create Strength Lift
```http
POST /api/strengthlifts
```

**Description**: Creates a new strength lift with sets

**Authorization**: Required

**Request Body**:
```json
{
  "workoutSessionId": "123e4567-e89b-12d3-a456-426614174000",
  "exerciseTypeId": "123e4567-e89b-12d3-a456-426614174000",
  "sets": [
    {
      "setNumber": 1,
      "weight": 135.0,
      "reps": 10,
      "rpe": 7.0
    }
  ],
  "notes": "First set felt easy"
}
```

**Response**: Created strength lift object (HTTP 201)

---

#### Update Strength Lift
```http
PUT /api/strengthlifts/{id}
```

**Description**: Updates an existing strength lift

**Authorization**: Required

**Path Parameters**:
- `id` (UUID, required): Strength lift ID

**Request Body**: Same as create, with updated values

**Response**: Updated strength lift object

---

### Metcon Workout Endpoints

#### Get Metcon Workouts for Session
```http
GET /api/metconworkouts/session/{sessionId}
```

**Description**: Retrieves all metcon workouts for a workout session

**Authorization**: Required

**Path Parameters**:
- `sessionId` (UUID, required): Workout session ID

**Response**:
```json
[
  {
    "id": "123e4567-e89b-12d3-a456-426614174000",
    "workoutSessionId": "123e4567-e89b-12d3-a456-426614174000",
    "metconTypeId": "123e4567-e89b-12d3-a456-426614174000",
    "metconType": {
      "name": "Cindy",
      "description": "AMRAP 20 minutes",
      "timeCapMinutes": 20
    },
    "rounds": 12,
    "actualTimeMinutes": 20.0,
    "movements": [
      {
        "movementType": "Pull-ups",
        "reps": 5
      },
      {
        "movementType": "Push-ups", 
        "reps": 10
      },
      {
        "movementType": "Air Squats",
        "reps": 15
      }
    ],
    "notes": "Great workout, felt strong"
  }
]
```

---

#### Create Metcon Workout
```http
POST /api/metconworkouts
```

**Description**: Creates a new metcon workout

**Authorization**: Required

**Request Body**:
```json
{
  "workoutSessionId": "123e4567-e89b-12d3-a456-426614174000",
  "metconTypeId": "123e4567-e89b-12d3-a456-426614174000",
  "rounds": 12,
  "actualTimeMinutes": 20.0,
  "movements": [
    {
      "movementTypeId": "123e4567-e89b-12d3-a456-426614174000",
      "reps": 5
    }
  ],
  "notes": "Pushed hard today"
}
```

**Response**: Created metcon workout object (HTTP 201)

---

### Exercise Type Management

#### Get All Exercise Types
```http
GET /api/exercisetypes
```

**Description**: Retrieves all available exercise types

**Authorization**: Required

**Query Parameters**:
- `category` (string, optional): Filter by exercise category
- `muscleGroup` (string, optional): Filter by muscle group

**Response**:
```json
[
  {
    "id": "123e4567-e89b-12d3-a456-426614174000",
    "name": "Back Squat",
    "category": "Compound",
    "muscleGroups": ["Quadriceps", "Glutes", "Hamstrings"],
    "description": "A compound lower body exercise",
    "instructions": "Stand with feet shoulder-width apart..."
  }
]
```

---

#### Create Exercise Type
```http
POST /api/exercisetypes
```

**Description**: Creates a new exercise type (admin only)

**Authorization**: Required (Admin role)

**Request Body**:
```json
{
  "name": "Bulgarian Split Squat",
  "category": "Compound",
  "muscleGroups": ["Quadriceps", "Glutes"],
  "description": "Single-leg compound exercise",
  "instructions": "Place rear foot on bench..."
}
```

**Response**: Created exercise type object (HTTP 201)

---

### Progress Tracking Endpoints

#### Get Progress Analytics
```http
GET /api/progress/analytics
```

**Description**: Retrieves progress analytics for the authenticated user

**Authorization**: Required

**Query Parameters**:
- `startDate` (date, optional): Analytics start date
- `endDate` (date, optional): Analytics end date
- `exerciseTypeId` (UUID, optional): Filter by specific exercise

**Response**:
```json
{
  "totalWorkouts": 45,
  "totalVolume": 25650.0,
  "averageWorkoutDuration": 75.5,
  "personalRecords": [
    {
      "exerciseType": "Back Squat",
      "maxWeight": 225.0,
      "achievedDate": "2025-09-15T00:00:00Z"
    }
  ],
  "progressCharts": {
    "volumeOverTime": [
      {
        "date": "2025-10-01",
        "volume": 5250.0
      }
    ],
    "strengthProgress": [
      {
        "exerciseType": "Back Squat",
        "data": [
          {
            "date": "2025-10-01",
            "maxWeight": 215.0
          }
        ]
      }
    ]
  }
}
```

---

#### Get Personal Records
```http
GET /api/progress/records
```

**Description**: Retrieves all personal records for the authenticated user

**Authorization**: Required

**Response**:
```json
[
  {
    "exerciseTypeId": "123e4567-e89b-12d3-a456-426614174000",
    "exerciseTypeName": "Back Squat",
    "maxWeight": 225.0,
    "maxReps": 15,
    "maxVolume": 2250.0,
    "achievedDate": "2025-09-15T00:00:00Z"
  }
]
```

---

### Performance Monitoring Endpoints

#### Get Performance Metrics
```http
GET /api/performance/metrics
```

**Description**: Retrieves API performance metrics (admin only)

**Authorization**: Required (Admin role)

**Response**:
```json
{
  "requestCount": 1250,
  "averageResponseTime": 245.5,
  "slowRequests": 12,
  "errorRate": 0.8,
  "cacheHitRate": 85.2,
  "memoryUsage": {
    "allocated": 52428800,
    "gen0Collections": 5,
    "gen1Collections": 2,
    "gen2Collections": 1
  },
  "timestamp": "2025-10-02T10:30:00Z"
}
```

---

#### Clear Cache
```http
DELETE /api/performance/cache
```

**Description**: Clears application cache (admin only)

**Authorization**: Required (Admin role)

**Response**: HTTP 204 No Content

---

## Data Models

### User
```json
{
  "id": "UUID",
  "email": "string",
  "name": "string", 
  "createdAt": "datetime",
  "updatedAt": "datetime"
}
```

### WorkoutSession
```json
{
  "id": "UUID",
  "userId": "UUID",
  "startTime": "datetime",
  "endTime": "datetime?",
  "notes": "string?",
  "strengthLifts": "StrengthLift[]",
  "metconWorkouts": "MetconWorkout[]"
}
```

### StrengthLift
```json
{
  "id": "UUID",
  "workoutSessionId": "UUID",
  "exerciseTypeId": "UUID",
  "exerciseType": "ExerciseType",
  "sets": "Set[]",
  "notes": "string?"
}
```

### Set
```json
{
  "setNumber": "integer",
  "weight": "decimal",
  "reps": "integer",
  "rpe": "decimal?",
  "notes": "string?"
}
```

### MetconWorkout
```json
{
  "id": "UUID",
  "workoutSessionId": "UUID", 
  "metconTypeId": "UUID",
  "metconType": "MetconType",
  "rounds": "integer?",
  "actualTimeMinutes": "decimal?",
  "movements": "MetconMovement[]",
  "notes": "string?"
}
```

### ExerciseType
```json
{
  "id": "UUID",
  "name": "string",
  "category": "string",
  "muscleGroups": "string[]",
  "description": "string?",
  "instructions": "string?"
}
```

## Response Format

### Success Response
```json
{
  "data": "object | array",
  "meta": {
    "timestamp": "datetime",
    "requestId": "string"
  }
}
```

### Error Response
```json
{
  "error": {
    "code": "string",
    "message": "string",
    "details": "object?",
    "timestamp": "datetime",
    "requestId": "string"
  }
}
```

### Validation Error Response
```json
{
  "error": {
    "code": "VALIDATION_FAILED",
    "message": "Validation failed",
    "details": {
      "fieldName": ["error message 1", "error message 2"]
    },
    "timestamp": "datetime",
    "requestId": "string"
  }
}
```

## HTTP Status Codes

- **200 OK**: Successful GET, PUT requests
- **201 Created**: Successful POST requests
- **204 No Content**: Successful DELETE requests
- **400 Bad Request**: Invalid request data or validation errors
- **401 Unauthorized**: Missing or invalid authentication token
- **403 Forbidden**: Insufficient permissions
- **404 Not Found**: Resource not found
- **409 Conflict**: Resource conflict (e.g., duplicate email)
- **429 Too Many Requests**: Rate limit exceeded
- **500 Internal Server Error**: Server error
- **503 Service Unavailable**: Service temporarily unavailable

## Rate Limiting

The API implements rate limiting to ensure fair usage:

- **Authenticated Users**: 1000 requests per hour
- **Performance Endpoints**: 100 requests per hour  
- **Authentication Endpoints**: 10 requests per minute

Rate limit headers are included in responses:
```
X-RateLimit-Limit: 1000
X-RateLimit-Remaining: 999
X-RateLimit-Reset: 1696248000
```

## Caching

The API implements intelligent caching for improved performance:

- **User Data**: Cached for 1 hour
- **Exercise Types**: Cached for 24 hours
- **Progress Analytics**: Cached for 15 minutes
- **Static Data**: Cached indefinitely until updated

Cache-related headers:
```
Cache-Control: public, max-age=3600
ETag: "abc123"
Last-Modified: Tue, 02 Oct 2025 10:30:00 GMT
```

## Pagination

List endpoints support pagination:

**Query Parameters**:
- `page` (integer, default: 1): Page number
- `pageSize` (integer, default: 20, max: 100): Items per page

**Response Metadata**:
```json
{
  "data": [],
  "pagination": {
    "page": 1,
    "pageSize": 20,
    "totalCount": 150,
    "totalPages": 8,
    "hasNext": true,
    "hasPrevious": false
  }
}
```

## Filtering and Sorting

Many endpoints support filtering and sorting:

**Common Filters**:
- `startDate`, `endDate`: Date range filters
- `category`: Category-based filtering
- `search`: Text search across relevant fields

**Sorting**:
- `sortBy`: Field to sort by
- `sortDirection`: `asc` or `desc`

Example:
```
GET /api/workoutsessions?startDate=2025-10-01&sortBy=startTime&sortDirection=desc
```

## WebSocket Support (Future)

The API is designed to support real-time features via SignalR:

- **Workout Updates**: Real-time workout progress updates
- **Social Features**: Live workout sharing and comments
- **Notifications**: Real-time notifications for achievements

WebSocket endpoint: `/hubs/workout`

## SDK and Client Libraries

Official SDKs are available for:

- **JavaScript/TypeScript**: `@lifttracker/js-sdk`
- **.NET**: `LiftTracker.SDK`
- **Python**: `lifttracker-python` (planned)

## Error Handling Best Practices

### Client-Side Error Handling

```javascript
try {
  const response = await fetch('/api/users/me', {
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    }
  });
  
  if (!response.ok) {
    const error = await response.json();
    throw new Error(error.message);
  }
  
  const user = await response.json();
  return user;
} catch (error) {
  console.error('API Error:', error.message);
  // Handle error appropriately
}
```

### Common Error Scenarios

1. **Token Expiration**: Redirect to login or refresh token
2. **Network Errors**: Retry with exponential backoff
3. **Validation Errors**: Display field-specific error messages
4. **Server Errors**: Show generic error message and log details

## Testing

### Using curl

```bash
# Get user profile
curl -X GET "https://localhost:7001/api/users/me" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json"

# Create workout session
curl -X POST "https://localhost:7001/api/workoutsessions" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "startTime": "2025-10-02T09:00:00Z",
    "notes": "Morning workout"
  }'
```

### Using Postman

1. Import the OpenAPI specification from `/swagger/v1/swagger.json`
2. Set up environment variables for base URL and auth token
3. Use the pre-configured requests for testing

## Support and Feedback

- **Documentation Issues**: Report via GitHub Issues
- **API Questions**: Contact support@lifttracker.com
- **Feature Requests**: Submit via GitHub Discussions
- **Bug Reports**: Use GitHub Issues with detailed reproduction steps

---

*This documentation is automatically generated from the OpenAPI specification and is continuously updated.*
