using System;
using System.Collections.Generic;

namespace Parser
{
    public class Lexer
    {
        private readonly string _input;
        private int _position;
        private int _line = 1;
        private int _column = 1;

        public Lexer(string input)
        {
            _input = input;
        }

        public List<Token> Tokenize()
        {
            List<Token> tokens = new List<Token>();
            Token token;

            do
            {
                token = NextToken();
                tokens.Add(token);
            } while (token.Type != TokenType.EOF && token.Type != TokenType.Error);

            return tokens;
        }

        private Token NextToken()
        {
            SkipWhitespace();

            if (_position >= _input.Length)
            {
                return new Token(TokenType.EOF, "", _line, _column);
            }

            char current = _input[_position];

            if (current == 'q')
            {
                return ReadString();
            }

           
            if (current == '0' && _position + 1 < _input.Length)
            {
                char nextChar = _input[_position + 1];
                if (nextChar == 'x' || nextChar == 'X')
                {
                    return ReadHexNumber();
                }
            }

            if (char.IsDigit(current))
            {
                return ReadNumber();
            }

            if (char.IsLetter(current) || current == '_')
            {
                return ReadIdentifier();
            }

            return ReadSymbol();
        }

        private Token ReadNumber()
        {
            int start = _position;
            int line = _line;
            int col = _column;

            while (_position < _input.Length && char.IsDigit(_input[_position]))
            {
                _position++;
                _column++;
            }

            string value = _input.Substring(start, _position - start);
            return new Token(TokenType.Number, value, line, col);
        }

        private Token ReadHexNumber()
        {
            int start = _position;
            int line = _line;
            int col = _column;

            // Пропускаем '0x' или '0X'
            _position += 2;
            _column += 2;

            while (_position < _input.Length)
            {
                char c = _input[_position];
                if ((c >= '0' && c <= '9') ||
                    (c >= 'a' && c <= 'f') ||
                    (c >= 'A' && c <= 'F'))
                {
                    _position++;
                    _column++;
                }
                else
                {
                    break;
                }
            }

            string value = _input.Substring(start, _position - start);
            return new Token(TokenType.HexNumber, value, line, col);
        }

        private Token ReadIdentifier()
        {
            int start = _position;
            int line = _line;
            int col = _column;

            while (_position < _input.Length &&
                   (char.IsLetterOrDigit(_input[_position]) || _input[_position] == '_'))
            {
                _position++;
                _column++;
            }

            string value = _input.Substring(start, _position - start);

            if (value.ToLower() == "var")
            {
                return new Token(TokenType.Var, value, line, col);
            }
            else if (value.ToLower() == "min")
            {
                return new Token(TokenType.MinFunc, value, line, col);
            }
            else if (IsAllUpperCase(value))
            {
                return new Token(TokenType.Word, value, line, col);
            }
            else
            {
                return new Token(TokenType.Identifier, value, line, col);
            }
        }

        private bool IsAllUpperCase(string value)
        {
            foreach (char c in value)
            {
                if (char.IsLetter(c) && !char.IsUpper(c))
                    return false;
            }
            return value.Length > 0 && char.IsLetter(value[0]);
        }

        private Token ReadString()
        {
            int line = _line;
            int col = _column;

            if (_position + 1 >= _input.Length || _input[_position + 1] != '(')
            {
                _position++;
                _column++;
                return new Token(TokenType.Error, "Ожидается '(' после q", line, col);
            }

            _position += 2;
            _column += 2;

            int start = _position;
            int parenCount = 1;

            while (_position < _input.Length && parenCount > 0)
            {
                if (_input[_position] == '(')
                {
                    parenCount++;
                }
                else if (_input[_position] == ')')
                {
                    parenCount--;
                }

                _position++;
                _column++;
            }

            if (parenCount > 0)
            {
                return new Token(TokenType.Error, "Незакрытая строка", line, col);
            }

            string value = _input.Substring(start, _position - start - 1);
            return new Token(TokenType.String, value, line, col);
        }

        private Token ReadSymbol()
        {
            char current = _input[_position];
            int line = _line;
            int col = _column;

            _position++;
            _column++;

            switch (current)
            {
                case '@':
                    if (_position < _input.Length && _input[_position] == '{')
                    {
                        _position++;
                        _column++;
                        return new Token(TokenType.DictStart, "@{", line, col);
                    }
                    return new Token(TokenType.Error, "Ожидается '{' после '@'", line, col);

                case '{':
                    return new Token(TokenType.Error, "Словарь должен начинаться с '@{'", line, col);

                case '}':
                    return new Token(TokenType.DictEnd, "}", line, col);

                case ';':
                    return new Token(TokenType.Semicolon, ";", line, col);

                case '$':
                    return new Token(TokenType.Dollar, "$", line, col);

                case ':':
                    if (_position < _input.Length && _input[_position] == '=')
                    {
                        _position++;
                        _column++;
                        return new Token(TokenType.Assign, ":=", line, col);
                    }
                    return new Token(TokenType.Error, "Ожидается '=' после ':'", line, col);

                case '=':
                    return new Token(TokenType.Equals, "=", line, col);

                case '+':
                    return new Token(TokenType.Plus, "+", line, col);

                case '-':
                    return new Token(TokenType.Minus, "-", line, col);

                case '*':
                    return new Token(TokenType.Multiply, "*", line, col);

                case '(':
                    return new Token(TokenType.ParenOpen, "(", line, col);

                case ')':
                    return new Token(TokenType.ParenClose, ")", line, col);

                case ',':
                    return new Token(TokenType.Comma, ",", line, col);

                default:
                    return new Token(TokenType.Error, "Неизвестный символ: " + current, line, col);
            }
        }

        private void SkipWhitespace()
        {
            while (_position < _input.Length && char.IsWhiteSpace(_input[_position]))
            {
                if (_input[_position] == '\n')
                {
                    _line++;
                    _column = 1;
                }
                else
                {
                    _column++;
                }
                _position++;
            }
        }
    }
}