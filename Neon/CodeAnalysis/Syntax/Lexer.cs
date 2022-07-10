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