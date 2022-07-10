using System;
using Neon.CodeAnalysis.Syntax;
using Neon.CodeAnalysis.Text;

public static class Program
{
    public static void Main()
    {
        while (true)
        {
            var input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input))
            {
                continue;
            }

            var sourceText = SourceText.From(input);
            var lexer = new Lexer(sourceText);
            SyntaxToken token;
            do
            {
                token = lexer.NextToken();
                Console.WriteLine($"{token.Span} {token.Kind} {token.Text} {token.Value}");
            }while (token.Kind != SyntaxKind.EndOfFileToken);
        }
    }
}