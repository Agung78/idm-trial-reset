using IdmDiag.Core.Reporting;

namespace IdmDiag.Core.Checks;

public interface IDiagnosticCheck
{
    string Id { get; }

    string Name { get; }

    CheckResult Run(DiagnosticContext context);
}
