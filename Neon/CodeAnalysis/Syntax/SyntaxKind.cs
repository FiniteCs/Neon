namespace Neon.CodeAnalysis.Syntax;

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