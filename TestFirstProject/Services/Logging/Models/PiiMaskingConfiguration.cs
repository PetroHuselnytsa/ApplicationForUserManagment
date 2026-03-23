namespace TestFirstProject.Services.Logging.Models;

/// <summary>
/// Configuration settings for PII (Personally Identifiable Information) masking.
/// </summary>
public class PiiMaskingConfiguration
{
    /// <summary>
    /// Whether to enable email address masking. Default is true.
    /// </summary>
    public bool EnableEmailMasking { get; set; } = true;

    /// <summary>
    /// Whether to enable credit card number masking. Default is true.
    /// </summary>
    public bool EnableCreditCardMasking { get; set; } = true;

    /// <summary>
    /// Whether to enable phone number masking. Default is true.
    /// </summary>
    public bool EnablePhoneMasking { get; set; } = true;

    /// <summary>
    /// Whether to enable SSN masking. Default is true.
    /// </summary>
    public bool EnableSsnMasking { get; set; } = true;

    /// <summary>
    /// List of field names that should be fully masked (e.g., password, token).
    /// </summary>
    public List<string> SensitiveFieldNames { get; set; } = new()
    {
        "password",
        "pwd",
        "secret",
        "token",
        "apikey",
        "api_key",
        "authorization",
        "bearer",
        "credit_card",
        "creditcard",
        "ssn",
        "social_security",
        "cvv",
        "pin"
    };

    /// <summary>
    /// Regex pattern for matching email addresses.
    /// </summary>
    public string EmailPattern { get; set; } = @"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}";

    /// <summary>
    /// Regex pattern for matching credit card numbers (various formats).
    /// </summary>
    public string CreditCardPattern { get; set; } = @"\b(?:\d{4}[-\s]?){3}\d{4}\b";

    /// <summary>
    /// Regex pattern for matching SSN (US Social Security Numbers).
    /// </summary>
    public string SsnPattern { get; set; } = @"\b\d{3}[-\s]?\d{2}[-\s]?\d{4}\b";

    /// <summary>
    /// Regex pattern for matching phone numbers (various formats).
    /// </summary>
    public string PhonePattern { get; set; } = @"\b(?:\+?1[-.\s]?)?(?:\(?\d{3}\)?[-.\s]?)?\d{3}[-.\s]?\d{4}\b";
}
