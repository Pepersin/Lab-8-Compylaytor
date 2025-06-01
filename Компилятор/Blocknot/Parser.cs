using System;
using System.Collections.Generic;
using System.Text;

public class Parser
{
    private readonly List<Token> _tokens;
    private int _pos;
    private StringBuilder _output = new StringBuilder();

    public Parser(List<Token> tokens)
    {
        _tokens = tokens;
        _pos = 0;
        SkipUnknownTokens(); // на случай, если в начале unknown
    }

    private Token CurrentToken => _pos < _tokens.Count ? _tokens[_pos] : null;

    private void Advance()
    {
        _pos++;
        SkipUnknownTokens();
    }

    private void SkipUnknownTokens()
    {
        while (_pos < _tokens.Count && _tokens[_pos].Type == TokenType.Unknown)
        {
            // Можно добавить логирование, если нужно
            _pos++;
        }
    }

    public string Parse()
    {
        _output.Clear();
        _output.AppendLine("Начало разбора <Выражение>");
        SkipUnknownTokens();

        try
        {
            ParseExpression();

            if (CurrentToken == null || CurrentToken.Type != TokenType.EOF)
                throw new Exception($"Ожидался конец ввода, найден '{CurrentToken?.Value}' на позиции {CurrentToken?.Position}");

            _output.AppendLine("Разбор завершён успешно");
            _output.AppendLine("Синтаксических ошибок не найдено.");
        }
        catch (Exception ex)
        {
            _output.AppendLine("Обнаружены синтаксические ошибки");
            _output.AppendLine($"Синтаксические ошибки:\n{ex.Message}");
        }

        return _output.ToString();
    }

    private void ParseExpression()
    {
        SkipUnknownTokens();
        _output.AppendLine("Разбор <Выражение>");

        ParseSimpleExpression();

        SkipUnknownTokens();

        if (IsRelOp(CurrentToken))
        {
            _output.AppendLine($"Найдена операция отношения '{CurrentToken.Value}'");
            Advance();
            ParseSimpleExpression();
        }
    }

    private void ParseSimpleExpression()
    {
        SkipUnknownTokens();
        _output.AppendLine("Разбор <Простое выражение>");

        ParseTerm();

        SkipUnknownTokens();

        while (IsAddOp(CurrentToken))
        {
            _output.AppendLine($"Найдена аддитивная операция '{CurrentToken.Value}'");
            Advance();
            SkipUnknownTokens();

            ParseTerm();

            SkipUnknownTokens();
        }
    }

    private void ParseTerm()
    {
        SkipUnknownTokens();
        _output.AppendLine("Разбор <Терм>");

        ParseFactor();

        SkipUnknownTokens();

        while (IsMulOp(CurrentToken))
        {
            _output.AppendLine($"Найдена мультипликативная операция '{CurrentToken.Value}'");
            Advance();
            SkipUnknownTokens();

            ParseFactor();

            SkipUnknownTokens();
        }
    }

    private void ParseFactor()
    {
        SkipUnknownTokens();
        _output.AppendLine("Разбор <Фактор>");

        var token = CurrentToken;

        if (token == null)
            throw new Exception("Ожидался фактор, но достигнут конец ввода");

        if (token.Type == TokenType.Identifier)
        {
            _output.AppendLine($"Найдена переменная '{token.Value}'");
            Advance();
        }
        else if (token.Type == TokenType.Not)
        {
            _output.AppendLine("Найден оператор 'not'");
            Advance();
            ParseFactor();
        }
        else if (token.Type == TokenType.LParen)
        {
            _output.AppendLine("Найдена '('");
            Advance();
            ParseExpression();

            if (CurrentToken == null || CurrentToken.Type != TokenType.RParen)
                throw new Exception($"Ожидалась ')' на позиции {CurrentToken?.Position}");

            _output.AppendLine("Найдена ')'");
            Advance();
        }
        else
        {
            throw new Exception($"Ожидался фактор на позиции {token.Position}");
        }
    }

    private bool IsRelOp(Token token)
    {
        return token != null && token.Type == TokenType.RelOp;
    }

    private bool IsAddOp(Token token)
    {
        if (token == null) return false;
        return token.Type == TokenType.Plus || token.Type == TokenType.Minus || token.Type == TokenType.Or;
    }

    private bool IsMulOp(Token token)
    {
        if (token == null) return false;
        return token.Type == TokenType.Multiply || token.Type == TokenType.Divide || token.Type == TokenType.Modulo;
    }
}
