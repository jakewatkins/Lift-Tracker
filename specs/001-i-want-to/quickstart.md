# Quickstart: Workout Tracking System

This quickstart guide validates the core user stories through step-by-step scenarios. These steps should be executable once the implementation is complete.

## Prerequisites

- .NET 8 SDK installed
- SQL Server LocalDB or SQL Server instance
- Visual Studio 2022 or VS Code with C# extension
- Google Developer Console OAuth2 client configured

## Setup

1. **Clone and setup project**:
   ```bash
   git clone https://github.com/jakewatkins/Lift-Tracker.git
   cd Lift-Tracker
   dotnet restore
   ```

2. **Configure database**:
   ```bash
   cd src/LiftTracker.Infrastructure
   dotnet ef database update
   ```

3. **Configure authentication**:
   - Create `appsettings.Development.json` in `src/LiftTracker.API/`
   - Add Google OAuth client ID and secret:
   ```json
   {
     "Authentication": {
       "Google": {
         "ClientId": "your-google-client-id",
         "ClientSecret": "your-google-client-secret"
       }
     }
   }
   ```

4. **Run the application**:
   ```bash
   cd src/LiftTracker.API
   dotnet run
   ```

5. **Access the application**:
   - API: https://localhost:7001
   - Frontend: https://localhost:7002

## User Story Validation

### Story 1: User Account Creation and Login

**Scenario**: New user creates account and logs in

1. **Navigate to application**: Open https://localhost:7002
2. **Initiate login**: Click "Sign in with Google" button
3. **Complete OAuth flow**: 
   - Redirected to Google login
   - Grant permissions to the application
   - Redirected back to application
4. **Verify account creation**:
   - User should see personalized dashboard
   - Name should display from Google profile
   - Session should persist on page refresh

**Expected Result**: User successfully authenticated and account created with Google profile data

### Story 2: Create Basic Workout Session

**Scenario**: User creates a new workout session for today

1. **Access workout creation**: Click "New Workout" button on dashboard
2. **Set workout date**: 
   - Date should default to today
   - Verify date picker allows past dates but not future dates
3. **Add session notes**: Enter "Morning strength training session"
4. **Save workout session**: Click "Create Workout"
5. **Verify creation**:
   - Session should appear in workout list
   - Session date should be correctly displayed
   - Notes should be preserved

**Expected Result**: Workout session successfully created and displayed in user's workout list

### Story 3: Add Strength Lift to Session

**Scenario**: User logs a back squat lift with sets and reps

1. **Open workout session**: Click on the workout session created in Story 2
2. **Add strength lift**: Click "Add Lift" button
3. **Select exercise**: Choose "Back Squat" from exercise dropdown
4. **Set lift parameters**:
   - Set Structure: "Sets/Reps"
   - Sets: 3
   - Reps: 5
   - Weight: 225.0 lbs
   - Rest Period: 3.0 minutes
   - Comments: "Working on form"
5. **Save lift**: Click "Add Lift"
6. **Verify lift data**:
   - Lift should appear in session
   - All parameters should be correctly displayed
   - Weight should show "225.0 lbs"

**Expected Result**: Strength lift successfully added to workout session with all parameters preserved

### Story 4: Add Multiple Lift Variations

**Scenario**: User adds different set structures and bodyweight exercises

1. **Add EMOM lift**:
   - Exercise: "Overhead Press"
   - Set Structure: "EMOM"
   - Duration: 10.0 minutes
   - Weight: 135.0 lbs
   - Comments: "Every minute on the minute"

2. **Add bodyweight exercise**:
   - Exercise: "Pull-ups"
   - Set Structure: "Sets/Reps"
   - Sets: 5
   - Reps: 8
   - Weight: 0.0 lbs
   - Additional Weight: 25.0 lbs (weighted vest)
   - Comments: "Using weighted vest"

3. **Verify order and display**:
   - Lifts should appear in the order added
   - Different set structures should be clearly indicated
   - Bodyweight + additional weight should be properly displayed

**Expected Result**: Multiple lifts with different structures successfully added and properly displayed

### Story 5: Add Metcon Workout

**Scenario**: User logs an AMRAP metcon workout

1. **Add metcon workout**: Click "Add Metcon" in the workout session
2. **Set metcon parameters**:
   - Type: "AMRAP"
   - Total Time: 15.0 minutes
   - Notes: "15-minute AMRAP"
3. **Add movements**:
   - Movement 1: "Burpees", 10 reps
   - Movement 2: "Air Squats", 15 reps
   - Movement 3: "Push-ups", 20 reps
4. **Set completion data**:
   - Rounds Completed: 8
5. **Save metcon**: Click "Add Metcon"
6. **Verify metcon data**:
   - Metcon should appear in session
   - All movements should be listed with reps
   - Time and rounds completed should be displayed

**Expected Result**: Metcon workout successfully added with movements and completion data

### Story 6: Add Distance-Based Metcon

**Scenario**: User logs a "For Time" metcon with distance movements

1. **Add metcon workout**:
   - Type: "For Time"
   - Total Time: 12.5 minutes
   - Notes: "Cardio and strength combo"
2. **Add movements**:
   - Movement 1: "Running", 400 meters
   - Movement 2: "Thrusters", 21 reps, 95.0 lbs
   - Movement 3: "Running", 400 meters
   - Movement 4: "Thrusters", 15 reps, 95.0 lbs
   - Movement 5: "Running", 400 meters
   - Movement 6: "Thrusters", 9 reps, 95.0 lbs
3. **Verify mixed measurements**:
   - Distance movements should show meters
   - Rep movements should show rep count
   - Weights should display for weighted movements

**Expected Result**: Metcon with mixed measurement types (distance and reps) successfully recorded

### Story 7: Edit Workout Data

**Scenario**: User corrects weight entry in a previously logged lift

1. **Access existing lift**: Click on the back squat lift from Story 3
2. **Edit lift data**:
   - Change weight from 225.0 to 235.0 lbs
   - Update comments to "Increased weight, good form"
3. **Save changes**: Click "Update Lift"
4. **Verify changes**:
   - Weight should display 235.0 lbs
   - Comments should show updated text
   - Other parameters should remain unchanged

**Expected Result**: Lift data successfully updated with new weight and comments

### Story 8: View Progress Charts

**Scenario**: User views progress trends for back squat over 30 days

1. **Navigate to progress**: Click "Progress" tab or button
2. **Select exercise**: Choose "Back Squat" from exercise dropdown
3. **Set time range**: Select "30 days" option
4. **View chart**:
   - Chart should display weight progression over time
   - Data points should correspond to logged workouts
   - X-axis should show dates, Y-axis should show weights
5. **Verify chart interactivity**:
   - Hover over data points should show details
   - Chart should be responsive on mobile devices

**Expected Result**: Progress chart successfully displays back squat weight progression over 30-day period

### Story 9: Multi-Exercise Progress Analysis

**Scenario**: User compares progress across different time ranges

1. **View 60-day progress**:
   - Select "Back Squat", 60-day range
   - Note trend and data density
2. **View 90-day progress**:
   - Select "Back Squat", 90-day range
   - Compare with shorter time frames
3. **View metcon progress**:
   - Select metcon type "AMRAP"
   - View time/rounds completed trends
4. **Verify data consistency**:
   - All data points should align across time ranges
   - No duplicate or missing data
   - Performance trends should be logical

**Expected Result**: Progress charts work correctly across different time ranges and exercise types

### Story 10: Data Isolation Verification

**Scenario**: Verify user data privacy and isolation

1. **Create second user account**:
   - Use different Google account to sign in
   - Verify separate user profile created
2. **Verify data isolation**:
   - New user should see empty dashboard
   - No workout data from first user should be visible
   - Progress charts should show no data
3. **Create workout for second user**:
   - Add workout session with different exercises
   - Verify data saved correctly
4. **Switch back to first user**:
   - Log out and log back in with first account
   - Verify original user's data still present
   - Verify second user's data not visible

**Expected Result**: Complete data isolation between users, no cross-contamination of workout data

## Performance Validation

### Page Load Performance

1. **Measure initial page load**:
   - Open browser developer tools
   - Navigate to application
   - Verify page loads within 2 seconds on mobile connection
2. **Measure API response times**:
   - Monitor network tab during workout operations
   - Verify API responses under 500ms for 95th percentile
3. **Test mobile responsiveness**:
   - Test on various screen sizes (320px, 768px, 1024px, 1440px)
   - Verify all functionality works on touch devices

**Expected Result**: Application meets performance benchmarks across devices

## Accessibility Validation

1. **Keyboard navigation**:
   - Navigate entire application using only keyboard
   - Verify all interactive elements accessible via Tab key
   - Verify logical tab order
2. **Screen reader compatibility**:
   - Test with screen reader software
   - Verify all form labels and headings properly announced
   - Verify progress charts have alternative text descriptions
3. **WCAG 2.1 AA compliance**:
   - Run automated accessibility testing tools
   - Verify color contrast ratios meet standards
   - Verify form validation messages are accessible

**Expected Result**: Application fully accessible according to WCAG 2.1 AA standards

## Error Handling Validation

### Data Validation

1. **Test invalid weight entries**:
   - Enter weight with invalid fractional increment (e.g., 225.33)
   - Verify system rounds up to nearest 0.25 increment
2. **Test future date restriction**:
   - Attempt to create workout for future date
   - Verify error message and prevention
3. **Test field length limits**:
   - Enter comments exceeding character limits
   - Verify truncation or validation error

**Expected Result**: Robust data validation with clear error messages

### Authentication Error Handling

1. **Test network interruption**:
   - Simulate network disconnection during OAuth flow
   - Verify graceful error handling and retry mechanisms
2. **Test session expiration**:
   - Simulate expired authentication token
   - Verify automatic re-authentication prompt

**Expected Result**: Reliable authentication with proper error recovery

## Success Criteria

- ✅ All 10 user stories complete successfully
- ✅ Performance benchmarks met (<2s page load, <500ms API)
- ✅ WCAG 2.1 AA accessibility compliance verified
- ✅ Data validation and error handling working correctly
- ✅ Complete user data isolation confirmed
- ✅ Cross-device functionality verified

## Troubleshooting

### Common Issues

**Database connection errors**:
- Verify SQL Server is running
- Check connection string in appsettings
- Ensure database migrations applied

**Authentication failures**:
- Verify Google OAuth client configuration
- Check redirect URLs match exactly
- Ensure client secrets are correctly set

**Chart rendering issues**:
- Verify JavaScript libraries loaded correctly
- Check browser console for JavaScript errors
- Test chart responsiveness on different screen sizes

### Getting Help

- Check application logs in console output
- Review browser developer tools for client-side errors
- Verify all NuGet packages properly restored
- Ensure .NET 8 SDK is correctly installed