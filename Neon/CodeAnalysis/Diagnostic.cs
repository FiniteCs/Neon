using System.Collections;
using System.Collections.Generic;
using Neon.CodeAnalysis.Text;

namespace Neon.CodeAnalysis;

public sealed class Diagnostic
{
    public string Message { get; }
    public TextSpan Span { get; }

    public Diagnostic(string message, TextSpan span)
    {
        Message = message;
        Span = span;
    }
}

public sealed class DiagnosticBag : IEnumerable<Diagnostic>
{
    private List<Diagnostic> _diagnostics = new List<Diagnostic>();
    public DiagnosticBag()
    {
        _diagnostics = new List<Diagnostic>();
    }
    
    private void Report(string message, TextSpan span)
    {
        _diagnostics.Add(new Diagnostic(message, span));
    }
    
    public void ReportBadNumber(string number, TextSpan span)
    {
        Report($"Bad number: {number}", span);
    }

    public IEnumerator<Diagnostic> GetEnumerator()
    {
        return _diagnostics.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
