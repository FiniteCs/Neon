using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using Neon.CodeAnalysis.Text;

namespace Neon.CodeAnalysis.Syntax;

public sealed class Lexer
{
    private readonly DiagnosticBag _diagnostics;
    private readonly SourceText _text;
    private int _position;
    private int _start;
    private SyntaxKind _kind;
    private string _textValue;
    private object _value;

    public Lexer(SourceText text)
    {
        _text = text;
        _diagnostics = new DiagnosticBag();
    }

    private char PeekChar(int offset)
    {
        var index = _position + offset;
        if (index >= _text.Length)
        {
            return '\0';
        }

        return _text[index];
    }

    public SyntaxToken NextToken()
    {
        _start = _position;
        _kind = SyntaxKind.BadToken;
        _textValue = string.Empty;
        _value = null;

        switch (PeekChar(0))
        {
            case '\0':
                _kind = SyntaxKind.EndOfFileToken;
                _position++;
                break;
            case '+':
                _kind = SyntaxKind.PlusToken;
                _position++;
                break;
            case '-':
                _kind = SyntaxKind.MinusToken;
                _position++;
                break;
            case '*':
                _kind = SyntaxKind.StarToken;
                _position++;
                break;
            case '/':
                _kind = SyntaxKind.SlashToken;
                _position++;
                break;
            case '0':
            case '1':
            case '2':
            case '3':
            case '4':
            case '5':
            case '6':
            case '7':
            case '8':
            case '9':
                ScanNumber();
                break;
            case ' ':
            case '\t':
            case '\r':
            case '\n':
                ScanWhitespace();
                break;
        }
        
        return new SyntaxToken(_kind, _start, _textValue, _value);
    }
    
    private void ScanNumber()
    {
        _kind = SyntaxKind.NumberToken;
        _textValue = string.Empty;
        _value = 0;
        
        while (char.IsDigit(PeekChar(0)))
        {
            _position++;
        }
        
        _textValue = _text.ToString(_start, _position - _start);
        if (!int.TryParse(_textValue, out var value))
        {
            _diagnostics.ReportBadNumber(_textValue, TextSpan.FromBounds(_start, _position));
        }
        else
        {
            _value = value;
        }
    }
    
    private void ScanWhitespace()
    {
        _kind = SyntaxKind.WhitespaceToken;
        while (char.IsWhiteSpace(PeekChar(0)))
        {
            _position++;
        }

        _textValue = _text.ToString(_start, _position - _start);
    }
}

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

public abstract class SyntaxNode
{
    public abstract SyntaxKind Kind { get; }

    public virtual TextSpan Span
    {
        get
        {
            var first = GetChildren().First().Span;
            var last = GetChildren().Last().Span;
            return TextSpan.FromBounds(first.Start, last.End);
        }
    }

    public IEnumerable<SyntaxNode> GetChildren()
    {
        var properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var property in properties)
        {
            if (typeof(SyntaxNode).IsAssignableFrom(property.PropertyType))
            {
                var child = (SyntaxNode)property.GetValue(this);
                if (child != null)
                {
                    yield return child;
                }
            }
            else if (typeof(IEnumerable<SyntaxNode>).IsAssignableFrom(property.PropertyType))
            {
                var children = (IEnumerable<SyntaxNode>)property.GetValue(this);
                if (children != null)
                {
                    foreach (var child in children)
                    {
                        if (child != null)
                        {
                            yield return child;
                        }
                    }
                }
            }
        }
    }
    
    public SyntaxToken GetFirstToken()
    {
        if (this is SyntaxToken token)
        {
            return token;
        }

        return GetChildren().First().GetFirstToken();
    }
    
    public SyntaxToken GetLastToken()
    {
        if (this is SyntaxToken token)
        {
            return token;
        }

        return GetChildren().Last().GetLastToken();
    }

    public void WriteTo(TextWriter writer)
    {
        PrettyPrint(writer, this);
    }
    
    private static void PrettyPrint(TextWriter writer, SyntaxNode node, string indent = "", bool isLast = true)
    {
        var isToConsole = writer == Console.Out;
        var marker = isLast ? "└──" : "├──";

        writer.Write(indent);
        writer.Write(marker);

        if (isToConsole)
        {
            Console.ForegroundColor = node is SyntaxToken ? ConsoleColor.Blue : ConsoleColor.Cyan;
        }

        writer.Write(node.Kind);

        if (node is SyntaxToken t && t.Value != null)
        {
            writer.Write(" ");
            writer.Write(t.Value);
        }

        if (isToConsole)
        {
            Console.ResetColor();
        }

        writer.WriteLine();

        indent += isLast ? "   " : "│  ";

        SyntaxNode lastChild = node.GetChildren().LastOrDefault();

        foreach (SyntaxNode child in node.GetChildren())
        {
            PrettyPrint(writer, child, indent, child == lastChild);
        }
    }
}

public enum SyntaxKind : byte
{
    // Special tokens
    EndOfFileToken,
    BadToken,
    
    // Ranged tokens
    WhitespaceToken,
    NumberToken,
    
    // Tokens
    PlusToken,
    MinusToken,
    StarToken,
    SlashToken,
}
