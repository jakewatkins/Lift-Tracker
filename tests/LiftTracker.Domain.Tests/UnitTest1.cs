using LiftTracker.Domain.Entities;

namespace LiftTracker.Domain.Tests.Entities;

public class UserTests
{
    [Fact]
    public void User_DefaultConstructor_ShouldSetIdAndCreatedDate()
    {
        // Act
        var user = new User();

        // Assert
        Assert.NotEqual(Guid.Empty, user.Id);
        Assert.True(user.CreatedDate <= DateTime.UtcNow);
        Assert.True(user.CreatedDate > DateTime.UtcNow.AddSeconds(-1)); // Created within last second
        Assert.Null(user.LastLoginDate);
        Assert.Empty(user.Email);
        Assert.Empty(user.Name);
        Assert.NotNull(user.WorkoutSessions);
        Assert.Empty(user.WorkoutSessions);
    }

    [Fact]
    public void User_ParameterizedConstructor_ShouldSetEmailAndName()
    {
        // Arrange
        var email = "test@example.com";
        var name = "Test User";

        // Act
        var user = new User(email, name);

        // Assert
        Assert.Equal(email, user.Email);
        Assert.Equal(name, user.Name);
        Assert.NotEqual(Guid.Empty, user.Id);
        Assert.True(user.CreatedDate <= DateTime.UtcNow);
        Assert.Null(user.LastLoginDate);
    }

    [Fact]
    public void User_ParameterizedConstructor_WithNullEmail_ShouldThrowArgumentNullException()
    {
        // Arrange
        string? email = null;
        var name = "Test User";

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new User(email!, name));
    }

    [Fact]
    public void User_ParameterizedConstructor_WithNullName_ShouldThrowArgumentNullException()
    {
        // Arrange
        var email = "test@example.com";
        string? name = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new User(email, name!));
    }

    [Fact]
    public void UpdateLastLogin_ShouldSetLastLoginDateToCurrentTime()
    {
        // Arrange
        var user = new User("test@example.com", "Test User");
        var beforeUpdate = DateTime.UtcNow;

        // Act
        user.UpdateLastLogin();

        // Assert
        Assert.NotNull(user.LastLoginDate);
        Assert.True(user.LastLoginDate >= beforeUpdate);
        Assert.True(user.LastLoginDate <= DateTime.UtcNow);
    }

    [Fact]
    public void UpdateLastLogin_CalledMultipleTimes_ShouldUpdateToLatestTime()
    {
        // Arrange
        var user = new User("test@example.com", "Test User");
        user.UpdateLastLogin();
        var firstLogin = user.LastLoginDate;

        // Wait a small amount to ensure time difference
        Thread.Sleep(1);

        // Act
        user.UpdateLastLogin();

        // Assert
        Assert.NotNull(user.LastLoginDate);
        Assert.True(user.LastLoginDate > firstLogin);
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalid-email")]
    [InlineData("@example.com")]
    [InlineData("test@")]
    public void User_Email_ValidationAttributes_ShouldRequireValidEmail(string email)
    {
        // Arrange
        var user = new User();
        user.Email = email;

        // Act
        var validationResults = ValidateModel(user);

        // Assert
        if (string.IsNullOrEmpty(email))
        {
            Assert.Contains(validationResults, vr => vr.MemberNames.Contains(nameof(User.Email)) &&
                                                      vr.ErrorMessage!.Contains("required"));
        }
        else
        {
            Assert.Contains(validationResults, vr => vr.MemberNames.Contains(nameof(User.Email)) &&
                                                      vr.ErrorMessage!.Contains("valid"));
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void User_Name_ValidationAttributes_ShouldRequireNonEmptyName(string? name)
    {
        // Arrange
        var user = new User();
        user.Name = name!;

        // Act
        var validationResults = ValidateModel(user);

        // Assert
        Assert.Contains(validationResults, vr => vr.MemberNames.Contains(nameof(User.Name)) &&
                                                  vr.ErrorMessage!.Contains("required"));
    }

    [Fact]
    public void User_Email_ShouldNotExceedMaxLength()
    {
        // Arrange
        var user = new User();
        user.Email = new string('a', 255) + "@example.com"; // 267 characters total

        // Act
        var validationResults = ValidateModel(user);

        // Assert
        Assert.Contains(validationResults, vr => vr.MemberNames.Contains(nameof(User.Email)) &&
                                                  vr.ErrorMessage!.Contains("length"));
    }

    [Fact]
    public void User_Name_ShouldNotExceedMaxLength()
    {
        // Arrange
        var user = new User();
        user.Name = new string('A', 101); // 101 characters

        // Act
        var validationResults = ValidateModel(user);

        // Assert
        Assert.Contains(validationResults, vr => vr.MemberNames.Contains(nameof(User.Name)) &&
                                                  vr.ErrorMessage!.Contains("length"));
    }

    [Fact]
    public void User_ValidEmail_ShouldPassValidation()
    {
        // Arrange
        var user = new User("test@example.com", "Test User");

        // Act
        var validationResults = ValidateModel(user);

        // Assert
        Assert.DoesNotContain(validationResults, vr => vr.MemberNames.Contains(nameof(User.Email)));
    }

    [Fact]
    public void User_ValidName_ShouldPassValidation()
    {
        // Arrange
        var user = new User("test@example.com", "Test User");

        // Act
        var validationResults = ValidateModel(user);

        // Assert
        Assert.DoesNotContain(validationResults, vr => vr.MemberNames.Contains(nameof(User.Name)));
    }

    private static List<System.ComponentModel.DataAnnotations.ValidationResult> ValidateModel(object model)
    {
        var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
        var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(model);
        System.ComponentModel.DataAnnotations.Validator.TryValidateObject(model, validationContext, validationResults, true);
        return validationResults;
    }
}
