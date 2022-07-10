using Neon.CodeAnalysis.Text;

namespace Neon.CodeAnalysis.Syntax;

public sealed class SyntaxToken : SyntaxNode
{
    public override SyntaxKind Kind { get; }
    public int Position { get; }
    public string Text { get; }
    public object Value { get; }

    public SyntaxToken(SyntaxKind kind, int position, string text, object value)
    {
        Kind = kind;
        Position = position;
        Text = text;
        Value = value;
    }

    public override TextSpan Span
    {
        get
        {
            return new TextSpan(Position, Text.Length);
        }
    }
}