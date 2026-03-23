namespace TestFirstProject.Services.Logging;

/// <summary>
/// Interface for PII masking service to protect sensitive data in logs.
/// </summary>
public interface IPiiMaskingService
{
    /// <summary>
    /// Masks sensitive data patterns in the input string (email, credit card, SSN, phone).
    /// </summary>
    /// <param name="input">The input string to mask.</param>
    /// <returns>The masked string.</returns>
    string MaskSensitiveData(string input);

    /// <summary>
    /// Masks specific JSON properties by name.
    /// </summary>
    /// <param name="json">The JSON string to process.</param>
    /// <param name="propertyNames">The property names to mask.</param>
    /// <returns>The JSON string with masked properties.</returns>
    string MaskJsonProperties(string json, IEnumerable<string> propertyNames);

    /// <summary>
    /// Checks if a property name indicates sensitive data.
    /// </summary>
    /// <param name="propertyName">The property name to check.</param>
    /// <returns>True if the property should be masked.</returns>
    bool ShouldMaskProperty(string propertyName);
}
