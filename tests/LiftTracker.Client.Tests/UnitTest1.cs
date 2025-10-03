using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace LiftTracker.Client.Tests;

/// <summary>
/// Basic Blazor component tests using bUnit framework
/// </summary>
public class BlazorComponentTests : TestContext
{
    public BlazorComponentTests()
    {
        // Configure test context for Blazor components
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    /// <summary>
    /// Test that verifies bUnit framework is working correctly
    /// </summary>
    [Fact]
    public void BunitFramework_IsConfiguredCorrectly()
    {
        // Arrange & Act
        var component = RenderComponent<SimpleTestComponent>();

        // Assert
        Assert.NotNull(component);
        Assert.Contains("Default Title", component.Markup);
    }

    /// <summary>
    /// Test basic component rendering functionality
    /// </summary>
    [Fact]
    public void SimpleComponent_RendersCorrectly()
    {
        // Arrange & Act
        var component = RenderComponent<SimpleTestComponent>();

        // Assert
        var element = component.Find("div");
        Assert.NotNull(element);
        Assert.Equal("test-component", element.GetAttribute("class"));
    }

    /// <summary>
    /// Test component parameter binding
    /// </summary>
    [Fact]
    public void SimpleComponent_WithParameters_RendersCorrectly()
    {
        // Arrange
        var testTitle = "Custom Title";

        // Act
        var component = RenderComponent<SimpleTestComponent>(parameters => parameters
            .Add(p => p.Title, testTitle));

        // Assert
        Assert.Contains(testTitle, component.Markup);
    }

    /// <summary>
    /// Test component event handling
    /// </summary>
    [Fact]
    public void SimpleComponent_ButtonClick_UpdatesState()
    {
        // Arrange
        var component = RenderComponent<SimpleTestComponent>();
        var button = component.Find("button");

        // Act
        button.Click();

        // Assert
        Assert.Contains("Clicked!", component.Markup);
    }

    /// <summary>
    /// Test component with CSS classes
    /// </summary>
    [Fact]
    public void SimpleComponent_HasCorrectCssClasses()
    {
        // Arrange & Act
        var component = RenderComponent<SimpleTestComponent>();

        // Assert
        var divElement = component.Find("div");
        Assert.Contains("test-component", divElement.GetAttribute("class"));

        var buttonElement = component.Find("button");
        Assert.Contains("btn", buttonElement.GetAttribute("class"));
    }

    /// <summary>
    /// Test component lifecycle methods
    /// </summary>
    [Fact]
    public void SimpleComponent_InitializesWithDefaultValues()
    {
        // Arrange & Act
        var component = RenderComponent<SimpleTestComponent>();

        // Assert
        Assert.Contains("Default Title", component.Markup);
        Assert.DoesNotContain("Clicked!", component.Markup);
    }

    /// <summary>
    /// Test multiple parameter combinations
    /// </summary>
    [Theory]
    [InlineData("Test 1")]
    [InlineData("Test 2")]
    [InlineData("Custom Component Name")]
    public void SimpleComponent_WithDifferentTitles_RendersCorrectly(string title)
    {
        // Arrange & Act
        var component = RenderComponent<SimpleTestComponent>(parameters => parameters
            .Add(p => p.Title, title));

        // Assert
        Assert.Contains(title, component.Markup);
    }

    /// <summary>
    /// Test component disposal and cleanup
    /// </summary>
    [Fact]
    public void SimpleComponent_CanBeDisposed()
    {
        // Arrange
        var component = RenderComponent<SimpleTestComponent>();

        // Act & Assert - Should not throw
        component.Dispose();
        Assert.NotNull(component);
    }

    /// <summary>
    /// Test component state management
    /// </summary>
    [Fact]
    public void SimpleComponent_StateChanges_TriggerRerender()
    {
        // Arrange
        var component = RenderComponent<SimpleTestComponent>();
        var initialMarkup = component.Markup;

        // Act
        var button = component.Find("button");
        button.Click();

        // Assert
        var updatedMarkup = component.Markup;
        Assert.NotEqual(initialMarkup, updatedMarkup);
        Assert.Contains("Clicked!", updatedMarkup);
    }

    /// <summary>
    /// Test component with null parameters
    /// </summary>
    [Fact]
    public void SimpleComponent_WithNullTitle_HandlesSafely()
    {
        // Arrange & Act
        var component = RenderComponent<SimpleTestComponent>(parameters => parameters
            .Add(p => p.Title, (string?)null));

        // Assert
        Assert.NotNull(component.Markup);
        // Should fallback to default title or handle null gracefully
    }
}

/// <summary>
/// Simple test component for bUnit testing
/// </summary>
public class SimpleTestComponent : ComponentBase
{
    [Parameter] public string Title { get; set; } = "Default Title";

    private bool hasBeenClicked = false;

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "class", "test-component");

        builder.OpenElement(2, "h1");
        builder.AddContent(3, Title);
        builder.CloseElement();

        builder.OpenElement(4, "button");
        builder.AddAttribute(5, "class", "btn btn-primary");
        builder.AddAttribute(6, "onclick", EventCallback.Factory.Create(this, HandleClick));
        builder.AddContent(7, "Click Me");
        builder.CloseElement();

        if (hasBeenClicked)
        {
            builder.OpenElement(8, "p");
            builder.AddContent(9, "Clicked!");
            builder.CloseElement();
        }

        builder.CloseElement();
    }

    private void HandleClick()
    {
        hasBeenClicked = true;
        StateHasChanged();
    }
}
