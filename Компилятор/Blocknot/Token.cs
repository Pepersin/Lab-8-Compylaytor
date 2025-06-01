public enum TokenType
{
    Identifier,     // a, b, abc
    Number,         // цифры (если нужно)
    Plus, 
    Minus, 
    Or,
    Multiply, 
    Divide, 
    Modulo,
    Not,
    LParen, 
    RParen,
    RelOp,          // =, !=, <, <=, >, >=
    EOF,
    Unknown
}

public class Token
{
    public TokenType Type { get; set; }
    public string Value { get; set; }
    public int Position { get; set; }

    public Token(TokenType type, string value, int position)
    {
        Type = type;
        Value = value;
        Position = position;
    }
}
