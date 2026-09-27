using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace DynamicProperty.SourceGen.Tests;

internal sealed class GeneratorTestResult
{
    public GeneratorDriverRunResult RunResult { get; }

    public Compilation OutputCompilation { get; }

    public ImmutableArray<Diagnostic> DriverDiagnostics { get; }

    public GeneratorTestResult(
        GeneratorDriverRunResult runResult,
        Compilation outputCompilation,
        ImmutableArray<Diagnostic> driverDiagnostics)
    {
        RunResult = runResult;
        OutputCompilation = outputCompilation;
        DriverDiagnostics = driverDiagnostics;
    }

    public string GeneratedSource =>
        string.Join(
            "\n\n",
            RunResult.Results
                .SelectMany(x => x.GeneratedSources)
                .Select(x => x.SourceText.ToString()));

    public Diagnostic[] GeneratorDiagnostics =>
        RunResult.Results
            .SelectMany(x => x.Diagnostics)
            .ToArray();

    public Diagnostic[] CompilationErrors =>
        OutputCompilation
            .GetDiagnostics()
            .Where(x => x.Severity == DiagnosticSeverity.Error)
            .ToArray();
}