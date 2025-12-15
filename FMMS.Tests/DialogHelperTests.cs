using System;
using System.Threading.Tasks;
using FMMS.Helpers;
using Microsoft.Maui.Controls;
using Xunit;

namespace FMMS.Tests;

/// <summary>
/// Unit tests for the DialogHelper class.
/// Tests dialog display functionality, null handling, and error scenarios.
/// </summary>
public class DialogHelperTests
{
    [Fact]
    public async Task ShowAlertAsync_WithNullPage_ReturnsFalse()
    {
        // Act
        Page? nullPage = null;
        var result = await DialogHelper.ShowAlertAsync(nullPage, "Test Title", "Test Message");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ShowAlertAsync_WithNullPage_DoesNotThrow()
    {
        // Act & Assert - should not throw
        Page? nullPage = null;
        var exception = await Record.ExceptionAsync(async () =>
            await DialogHelper.ShowAlertAsync(nullPage, "Test Title", "Test Message"));

        Assert.Null(exception);
    }

    [Fact]
    public async Task ShowConfirmationAsync_WithNullPage_ReturnsNull()
    {
        // Act
        Page? nullPage = null;
        var result = await DialogHelper.ShowConfirmationAsync(nullPage, "Test Title", "Test Message");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task ShowConfirmationAsync_WithNullPage_DoesNotThrow()
    {
        // Act & Assert - should not throw
        Page? nullPage = null;
        var exception = await Record.ExceptionAsync(async () =>
            await DialogHelper.ShowConfirmationAsync(nullPage, "Test Title", "Test Message"));

        Assert.Null(exception);
    }

    [Theory]
    [InlineData("OK")]
    [InlineData("Close")]
    [InlineData("")]
    public async Task ShowAlertAsync_WithNullPage_UsesCustomCancelText(string cancelText)
    {
        // Act
        Page? nullPage = null;
        var result = await DialogHelper.ShowAlertAsync(nullPage, "Test Title", "Test Message", cancelText);

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData("Yes", "No")]
    [InlineData("Confirm", "Cancel")]
    [InlineData("Accept", "Reject")]
    public async Task ShowConfirmationAsync_WithNullPage_UsesCustomButtonText(string accept, string cancel)
    {
        // Act
        Page? nullPage = null;
        var result = await DialogHelper.ShowConfirmationAsync(nullPage, "Test Title", "Test Message", accept, cancel);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task ShowAlertAsync_WithNullTitle_DoesNotThrow()
    {
        // Act & Assert - should not throw even with null title
        Page? nullPage = null;
        var exception = await Record.ExceptionAsync(async () =>
            await DialogHelper.ShowAlertAsync(nullPage, null!, "Test Message"));

        Assert.Null(exception);
    }

    [Fact]
    public async Task ShowAlertAsync_WithNullMessage_DoesNotThrow()
    {
        // Act & Assert - should not throw even with null message
        Page? nullPage = null;
        var exception = await Record.ExceptionAsync(async () =>
            await DialogHelper.ShowAlertAsync(nullPage, "Test Title", null!));

        Assert.Null(exception);
    }

    [Fact]
    public async Task ShowConfirmationAsync_WithNullTitle_DoesNotThrow()
    {
        // Act & Assert - should not throw even with null title
        Page? nullPage = null;
        var exception = await Record.ExceptionAsync(async () =>
            await DialogHelper.ShowConfirmationAsync(nullPage, null!, "Test Message"));

        Assert.Null(exception);
    }

    [Fact]
    public async Task ShowConfirmationAsync_WithNullMessage_DoesNotThrow()
    {
        // Act & Assert - should not throw even with null message
        Page? nullPage = null;
        var exception = await Record.ExceptionAsync(async () =>
            await DialogHelper.ShowConfirmationAsync(nullPage, "Test Title", null!));

        Assert.Null(exception);
    }

    [Fact]
    public async Task ShowAlertAsync_WithDefaultParameters_HandlesNullPage()
    {
        // Act - this will try to get the current page from Application.Current
        // which will be null in unit test context
        var result = await DialogHelper.ShowAlertAsync("Test Title", "Test Message");

        // Assert - should return false when no page is available
        Assert.False(result);
    }

    [Fact]
    public async Task ShowConfirmationAsync_WithDefaultParameters_HandlesNullPage()
    {
        // Act - this will try to get the current page from Application.Current
        // which will be null in unit test context
        var result = await DialogHelper.ShowConfirmationAsync("Test Title", "Test Message");

        // Assert - should return null when no page is available
        Assert.Null(result);
    }
}

