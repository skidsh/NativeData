namespace NativeData.Tests;

/// <summary>
/// Runs the test as a normal fact when the specified environment variable is set;
/// skips it otherwise. Used to gate live integration tests in CI.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class FactIfEnvAttribute : FactAttribute
{
    public FactIfEnvAttribute(string variableName)
    {
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(variableName)))
            Skip = $"Skipped: environment variable '{variableName}' is not set";
    }
}
