namespace TechUI.Mvc.Inputs
{
    /// <summary>
    /// Implemented by builders that can receive server-side validation data.
    /// </summary>
    public interface IValidationBuilder
    {
        void ApplyValidation(
            IEnumerable<KeyValuePair<string, string>> attributes,
            string? errorMessage);
    }
}
