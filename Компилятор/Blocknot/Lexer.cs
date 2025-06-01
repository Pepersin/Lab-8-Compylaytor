using System.Collections.Generic;

public class Lexer
{
    private readonly string _text;
    private int _pos;

    public List<string> LexicalErrors { get; } = new List<string>();

    public Lexer(string text)
    {
        _text = text;
        _pos = 0;
    }

    public List<Token> Tokenize()
    {
        var tokens = new List<Token>();

        while (_pos < _text.Length)
        {
            char current = _text[_pos];

            if (char.IsWhiteSpace(current))
            {
                _pos++;
                continue;
            }

            // Скобки
            if (current == '(')
            {
                tokens.Add(new Token(TokenType.LParen, "(", _pos));
                _pos++;
                continue;
            }
            if (current == ')')
            {
                tokens.Add(new Token(TokenType.RParen, ")", _pos));
                _pos++;
                continue;
            }

            // Операторы сложения
            if (current == '+')
            {
                tokens.Add(new Token(TokenType.Plus, "+", _pos));
                _pos++;
                continue;
            }
            if (current == '-')
            {
                tokens.Add(new Token(TokenType.Minus, "-", _pos));
                _pos++;
                continue;
            }

            // Операторы умножения
            if (current == '*')
            {
                tokens.Add(new Token(TokenType.Multiply, "*", _pos));
                _pos++;
                continue;
            }
            if (current == '/')
            {
                tokens.Add(new Token(TokenType.Divide, "/", _pos));
                _pos++;
                continue;
            }
            if (current == '%')
            {
                tokens.Add(new Token(TokenType.Modulo, "%", _pos));
                _pos++;
                continue;
            }

            // Операции отношения: =, !=, <, <=, >, >=
            if (current == '=')
            {
                tokens.Add(new Token(TokenType.RelOp, "=", _pos));
                _pos++;
                continue;
            }
            if (current == '!' && PeekNext() == '=')
            {
                tokens.Add(new Token(TokenType.RelOp, "!=", _pos));
                _pos += 2;
                continue;
            }
            if (current == '<')
            {
                if (PeekNext() == '=')
                {
                    tokens.Add(new Token(TokenType.RelOp, "<=", _pos));
                    _pos += 2;
                }
                else
                {
                    tokens.Add(new Token(TokenType.RelOp, "<", _pos));
                    _pos++;
                }
                continue;
            }
            if (current == '>')
            {
                if (PeekNext() == '=')
                {
                    tokens.Add(new Token(TokenType.RelOp, ">=", _pos));
                    _pos += 2;
                }
                else
                {
                    tokens.Add(new Token(TokenType.RelOp, ">", _pos));
                    _pos++;
                }
                continue;
            }

            // Идентификаторы и ключевые слова (not, or)
            if (char.IsLetter(current))
            {
                int start = _pos;
                while (_pos < _text.Length && (char.IsLetterOrDigit(_text[_pos])))
                {
                    _pos++;
                }
                string value = _text.Substring(start, _pos - start);

                if (value == "not")
                    tokens.Add(new Token(TokenType.Not, value, start));
                else if (value == "or")
                    tokens.Add(new Token(TokenType.Or, value, start));
                else
                    tokens.Add(new Token(TokenType.Identifier, value, start));

                continue;
            }

            tokens.Add(new Token(TokenType.Unknown, current.ToString(), _pos));
            LexicalErrors.Add($"Неизвестный символ '{current}' на позиции {_pos}");
            _pos++;
            continue;
        }

            tokens.Add(new Token(TokenType.EOF, "", _pos));
        return tokens;
    }

    private char PeekNext()
    {
        return (_pos + 1) < _text.Length ? _text[_pos + 1] : '\0';
    }
}
