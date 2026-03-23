using FluentAssertions;
using Microsoft.Extensions.Options;
using TestFirstProject.Services.Logging;
using TestFirstProject.Services.Logging.Models;
using Xunit;

namespace TestFirstProject.Tests.Services;

public class PiiMaskingServiceTests
{
    private readonly IPiiMaskingService _service;

    public PiiMaskingServiceTests()
    {
        var config = new PiiMaskingConfiguration
        {
            EnableEmailMasking = true,
            EnableCreditCardMasking = true,
            EnablePhoneMasking = true,
            EnableSsnMasking = true,
            SensitiveFieldNames = new List<string>
            {
                "password", "pwd", "secret", "token", "apikey", "api_key", "ssn", "creditcard"
            }
        };
        _service = new PiiMaskingService(Options.Create(config));
    }

    [Theory]
    [InlineData("john.doe@example.com")]
    [InlineData("test@test.com")]
    [InlineData("a.b.c@domain.org")]
    public void MaskSensitiveData_ShouldMaskEmails(string email)
    {
        var input = $"User email is {email}";

        var result = _service.MaskSensitiveData(input);

        result.Should().NotContain(email);
        result.Should().Contain("@");
        result.Should().Contain("***");
    }

    [Theory]
    [InlineData("4111111111111111")]
    [InlineData("4111-1111-1111-1111")]
    [InlineData("5500 0000 0000 0004")]
    public void MaskSensitiveData_ShouldMaskCreditCards(string creditCard)
    {
        var input = $"Card number: {creditCard}";

        var result = _service.MaskSensitiveData(input);

        result.Should().NotContain(creditCard.Replace("-", "").Replace(" ", "").Substring(0, 12));
        result.Should().Contain("****");
    }

    [Theory]
    [InlineData("123-45-6789")]
    [InlineData("123 45 6789")]
    public void MaskSensitiveData_ShouldMaskSsn(string ssn)
    {
        var input = $"SSN: {ssn}";

        var result = _service.MaskSensitiveData(input);

        result.Should().Contain("***-**-****");
    }

    [Theory]
    [InlineData("555-123-4567")]
    [InlineData("(555) 123-4567")]
    [InlineData("+1 555 123 4567")]
    public void MaskSensitiveData_ShouldMaskPhoneNumbers(string phone)
    {
        var input = $"Phone: {phone}";

        var result = _service.MaskSensitiveData(input);

        result.Should().Contain("***");
    }

    [Theory]
    [InlineData("password", true)]
    [InlineData("user_password", true)]
    [InlineData("PASSWORD", true)]
    [InlineData("apiKey", true)]
    [InlineData("api_key", true)]
    [InlineData("secret", true)]
    [InlineData("token", true)]
    [InlineData("ssn", true)]
    [InlineData("username", false)]
    [InlineData("email", false)]
    [InlineData("name", false)]
    public void ShouldMaskProperty_DetectsSensitiveFields(string propertyName, bool expected)
    {
        var result = _service.ShouldMaskProperty(propertyName);

        result.Should().Be(expected);
    }

    [Fact]
    public void MaskJsonProperties_ShouldMaskSpecifiedFields()
    {
        var json = """{"password":"secret123","email":"test@example.com","name":"John"}""";
        var propertiesToMask = new[] { "password" };

        var result = _service.MaskJsonProperties(json, propertiesToMask);

        result.Should().Contain("[REDACTED]");
        result.Should().Contain("name");
    }

    [Fact]
    public void MaskSensitiveData_ShouldMaskMultiplePatterns()
    {
        var text = "User email: john@example.com, SSN: 123-45-6789, Card: 4111111111111111";

        var result = _service.MaskSensitiveData(text);

        result.Should().NotContain("john@example.com");
        result.Should().NotContain("123-45-6789");
        result.Should().NotContain("4111111111111111");
    }

    [Fact]
    public void MaskSensitiveData_EmptyString_ShouldReturnEmpty()
    {
        var result = _service.MaskSensitiveData(string.Empty);

        result.Should().BeEmpty();
    }

    [Fact]
    public void MaskSensitiveData_NullInput_ShouldHandleGracefully()
    {
        var result = _service.MaskSensitiveData(null!);

        result.Should().BeNull();
    }

    [Fact]
    public void MaskSensitiveData_NoSensitiveData_ShouldReturnOriginal()
    {
        var input = "This is a normal message with no sensitive data";

        var result = _service.MaskSensitiveData(input);

        result.Should().Be(input);
    }
}
