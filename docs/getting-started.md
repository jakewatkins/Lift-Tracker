# Getting Started with LiftTracker

This guide will help you set up and start using LiftTracker for the first time.

## Prerequisites

Before you begin, ensure you have the following installed:

- **.NET 8 SDK** - [Download here](https://dotnet.microsoft.com/download/dotnet/8.0)
- **SQL Server** or **SQL Server LocalDB** for development
- **Git** for version control
- **Visual Studio 2022** or **VS Code** (recommended)

## Installation

### 1. Clone the Repository

```bash
git clone https://github.com/your-username/lift-tracker.git
cd lift-tracker
```

### 2. Restore Dependencies

```bash
dotnet restore
```

### 3. Configure Database

#### Option A: SQL Server LocalDB (Recommended for Development)

The application is pre-configured to use LocalDB. No additional setup required.

#### Option B: SQL Server Instance

Update the connection string in `src/LiftTracker.API/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=your-server;Database=LiftTrackerDb;Trusted_Connection=true;TrustServerCertificate=true;"
  }
}
```

### 4. Run Database Migrations

```bash
cd src/LiftTracker.API
dotnet ef database update
```

### 5. Configure Google OAuth (Optional)

For authentication features, set up Google OAuth:

1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Create a new project or select existing one
3. Enable Google+ API
4. Create OAuth 2.0 credentials
5. Update `appsettings.json`:

```json
{
  "Authentication": {
    "Google": {
      "ClientId": "your-client-id",
      "ClientSecret": "your-client-secret"
    }
  }
}
```

## Running the Application

### Development Mode

#### Terminal Method

**Start the API (Backend):**
```bash
cd src/LiftTracker.API
dotnet run
```

**Start the Client (Frontend) in a new terminal:**
```bash
cd src/LiftTracker.Client
dotnet run
```

#### Visual Studio Method

1. Set `LiftTracker.API` as the startup project
2. Press F5 to run
3. The Blazor client will be served alongside the API

### Access the Application

- **Client Application**: https://localhost:5001
- **API Documentation**: https://localhost:7001/swagger
- **Performance Metrics**: https://localhost:7001/api/performance/metrics

## First Workout Setup

### 1. Create Your Account

1. Navigate to https://localhost:5001
2. Click "Sign in with Google" or use the registration form
3. Complete your profile information

### 2. Set Up Exercise Types

Before logging workouts, you'll need some exercise types:

1. Go to **Settings** → **Exercise Types**
2. Add common exercises like:
   - **Strength**: Squats, Deadlifts, Bench Press, Overhead Press
   - **Metcon**: Burpees, Mountain Climbers, Box Jumps
   - **Cardio**: Running, Rowing, Biking

### 3. Log Your First Workout

#### Strength Training Workout

1. Click **"New Workout"**
2. Select **"Strength Training"**
3. Add exercises:
   ```
   Exercise: Back Squat
   Set 1: 135 lbs × 10 reps
   Set 2: 155 lbs × 8 reps
   Set 3: 175 lbs × 6 reps
   ```
4. Click **"Save Workout"**

#### Metcon Workout

1. Click **"New Workout"**
2. Select **"Metcon"**
3. Choose or create a workout type:
   ```
   Workout: "Cindy"
   Type: AMRAP (As Many Rounds As Possible)
   Time Cap: 20 minutes
   
   Movements:
   - 5 Pull-ups
   - 10 Push-ups  
   - 15 Air Squats
   
   Result: 12 rounds + 8 reps
   ```
4. Click **"Save Workout"**

## Understanding the Interface

### Dashboard

Your main hub showing:
- **Recent Workouts**: Last 5 workout sessions
- **Progress Charts**: Visual representation of your improvements
- **Quick Actions**: Fast access to common tasks
- **Personal Records**: Your current PRs across different exercises

### Workout Session View

- **Session Details**: Date, duration, and notes
- **Exercise List**: All exercises performed in the session
- **Performance Metrics**: Volume, intensity, and comparative data
- **Edit Controls**: Modify or delete entries

### Progress Tracking

- **Charts & Graphs**: Visual progress over time
- **Personal Records**: Automatic PR tracking
- **Volume Analysis**: Total weight moved, reps performed
- **Trend Analysis**: Identify patterns and improvements

## Common Workflows

### Planning a Workout

1. **Review Previous Sessions**: Check your workout history
2. **Set Goals**: Decide on target weights/reps/times
3. **Plan Progression**: Gradually increase intensity
4. **Log as You Go**: Real-time entry during workout

### Tracking Progress

1. **Regular Check-ins**: Review progress weekly/monthly
2. **Analyze Trends**: Use charts to identify patterns
3. **Adjust Programs**: Modify based on progress data
4. **Set New Goals**: Progressive overload and new challenges

### Data Management

1. **Regular Backups**: Export data periodically
2. **Clean Up**: Remove duplicate or incorrect entries
3. **Organize**: Use tags and categories effectively
4. **Share**: Export achievements and share progress

## Tips for Success

### Data Quality
- **Be Consistent**: Log workouts immediately after completing them
- **Be Accurate**: Record exact weights, reps, and times
- **Add Context**: Use notes to record how the workout felt
- **Regular Review**: Check and clean up data weekly

### Performance Optimization
- **Use Caching**: The app automatically caches frequently accessed data
- **Offline Mode**: Continue logging even without internet
- **Sync Regularly**: Ensure data is backed up to the server
- **Update Regularly**: Keep the application updated for best performance

### Best Practices
- **Start Simple**: Begin with basic exercises and progress gradually
- **Be Patient**: Consistent tracking yields better insights over time
- **Use Analytics**: Leverage progress charts to guide training decisions
- **Stay Organized**: Use consistent naming and categorization

## Troubleshooting

### Common Issues

**Cannot Connect to Database**
- Verify SQL Server is running
- Check connection string in appsettings.json
- Ensure database has been created with `dotnet ef database update`

**Authentication Not Working**
- Verify Google OAuth credentials are correct
- Check that redirect URLs are properly configured
- Ensure Google+ API is enabled in Google Cloud Console

**Slow Performance**
- Check network connection
- Clear browser cache
- Review Performance Metrics at `/api/performance/metrics`
- Restart the application

**Data Not Saving**
- Check browser console for JavaScript errors
- Verify API connectivity
- Check application logs for server errors
- Ensure proper authentication

### Getting Help

1. **Check Logs**: Review application logs for error details
2. **Documentation**: Refer to the [User Manual](./user-manual.md) for detailed feature explanations
3. **Performance**: See [Performance Optimization](./performance-optimization.md) for tuning guidance
4. **Issues**: Report bugs via GitHub Issues with detailed reproduction steps

## Next Steps

Once you're comfortable with the basics:

1. **Explore Advanced Features**: Custom workout templates, advanced analytics
2. **Customize Settings**: Personalize the interface and preferences
3. **Data Analysis**: Dive deeper into progress analytics and trends
4. **Integration**: Explore API endpoints for custom integrations
5. **Community**: Share your progress and learn from other users

## Additional Resources

### Development Documentation
- **[API Documentation](./api-documentation.md)**: Complete REST API reference with examples
- **[Code Refactoring Guide](./refactoring-guide.md)**: Best practices and utility patterns
- **[Testing Guide](./testing-guide.md)**: Comprehensive testing strategies (49 tests)
- **[Performance Optimization](./performance-optimization.md)**: Caching and efficiency improvements

### Quality & Validation
- **[Quickstart Validation Report](./quickstart-validation-report.md)**: Implementation validation
- **[Architecture Documentation](./architecture.md)**: System design and patterns
- **[Deployment Guide](./deployment-guide.md)**: Production deployment instructions

---

**Ready to start tracking?** Head to https://localhost:5001 and log your first workout!

*Last updated: October 2025*
