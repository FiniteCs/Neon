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