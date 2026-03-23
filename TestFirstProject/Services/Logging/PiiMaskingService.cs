using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using TestFirstProject.Services.Logging.Models;

namespace TestFirstProject.Services.Logging;

/// <summary>
/// Service for masking PII (Personally Identifiable Information) in strings and JSON data.
/// </summary>
public class PiiMaskingService : IPiiMaskingService
{
    private readonly PiiMaskingConfiguration _config;
    private readonly Regex _emailRegex;
    private readonly Regex _creditCardRegex;
    private readonly Regex _ssnRegex;
    private readonly Regex _phoneRegex;
    private readonly HashSet<string> _sensitiveFieldNames;

    /// <summary>
    /// Initializes a new instance of the PiiMaskingService.
    /// </summary>
    /// <param name="options">PII masking configuration options.</param>
    public PiiMaskingService(IOptions<PiiMaskingConfiguration> options)
    {
        _config = options.Value;
        _emailRegex = new Regex(_config.EmailPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
        _creditCardRegex = new Regex(_config.CreditCardPattern, RegexOptions.Compiled);
        _ssnRegex = new Regex(_config.SsnPattern, RegexOptions.Compiled);
        _phoneRegex = new Regex(_config.PhonePattern, RegexOptions.Compiled);
        _sensitiveFieldNames = new HashSet<string>(
            _config.SensitiveFieldNames.Select(f => f.ToLowerInvariant()),
            StringComparer.OrdinalIgnoreCase
        );
    }

    /// <inheritdoc />
    public string MaskSensitiveData(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        var result = input;

        if (_config.EnableEmailMasking)
        {
            result = MaskEmails(result);
        }

        if (_config.EnableCreditCardMasking)
        {
            result = MaskCreditCards(result);
        }

        if (_config.EnableSsnMasking)
        {
            result = MaskSsns(result);
        }

        if (_config.EnablePhoneMasking)
        {
            result = MaskPhones(result);
        }

        return result;
    }

    /// <inheritdoc />
    public string MaskJsonProperties(string json, IEnumerable<string> propertyNames)
    {
        if (string.IsNullOrEmpty(json))
        {
            return json;
        }

        try
        {
            using var doc = JsonDocument.Parse(json);
            var maskedJson = MaskJsonElement(doc.RootElement, propertyNames.ToHashSet(StringComparer.OrdinalIgnoreCase));
            return JsonSerializer.Serialize(maskedJson);
        }
        catch (JsonException)
        {
            return MaskSensitiveData(json);
        }
    }

    /// <inheritdoc />
    public bool ShouldMaskProperty(string propertyName)
    {
        if (string.IsNullOrEmpty(propertyName))
        {
            return false;
        }

        var lowerName = propertyName.ToLowerInvariant();
        return _sensitiveFieldNames.Any(sf => lowerName.Contains(sf, StringComparison.OrdinalIgnoreCase));
    }

    private string MaskEmails(string input)
    {
        return _emailRegex.Replace(input, match =>
        {
            var email = match.Value;
            var atIndex = email.IndexOf('@');
            if (atIndex <= 1)
            {
                return "***@" + email.Substring(atIndex + 1);
            }

            var localPart = email.Substring(0, atIndex);
            var domain = email.Substring(atIndex);
            var maskedLocal = localPart[0] + new string('*', Math.Min(3, localPart.Length - 1));
            return maskedLocal + domain;
        });
    }

    private string MaskCreditCards(string input)
    {
        return _creditCardRegex.Replace(input, match =>
        {
            var digits = new string(match.Value.Where(char.IsDigit).ToArray());
            if (digits.Length >= 4)
            {
                return "****-****-****-" + digits.Substring(digits.Length - 4);
            }
            return "[REDACTED]";
        });
    }

    private string MaskSsns(string input)
    {
        return _ssnRegex.Replace(input, "***-**-****");
    }

    private string MaskPhones(string input)
    {
        return _phoneRegex.Replace(input, match =>
        {
            var digits = new string(match.Value.Where(char.IsDigit).ToArray());
            if (digits.Length >= 4)
            {
                return "***-***-" + digits.Substring(digits.Length - 4);
            }
            return "[REDACTED]";
        });
    }

    private object? MaskJsonElement(JsonElement element, HashSet<string> propertiesToMask)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                var dict = new Dictionary<string, object?>();
                foreach (var prop in element.EnumerateObject())
                {
                    if (ShouldMaskProperty(prop.Name) || propertiesToMask.Contains(prop.Name))
                    {
                        dict[prop.Name] = "[REDACTED]";
                    }
                    else
                    {
                        dict[prop.Name] = MaskJsonElement(prop.Value, propertiesToMask);
                    }
                }
                return dict;

            case JsonValueKind.Array:
                return element.EnumerateArray()
                    .Select(e => MaskJsonElement(e, propertiesToMask))
                    .ToList();

            case JsonValueKind.String:
                return MaskSensitiveData(element.GetString() ?? string.Empty);

            case JsonValueKind.Number:
                return element.GetDecimal();

            case JsonValueKind.True:
                return true;

            case JsonValueKind.False:
                return false;

            case JsonValueKind.Null:
            default:
                return null;
        }
    }
}
