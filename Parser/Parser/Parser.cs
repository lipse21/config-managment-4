using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser
{
    public class Parser
    {
        private readonly List<Token> _tokens;
        private int _position;
        private readonly Dictionary<string, ValueNode> _variables = new();

        public Parser(List<Token> tokens)
        {
            _tokens = tokens;
        }

        public ProgramNode Parse()
        {
            var program = new ProgramNode();

            while (!IsAtEnd())
            {
                if (Match(TokenType.Var))
                {
                    var varDecl = ParseVarDeclaration();
                    program.Variables.Add(varDecl);
                    _variables[varDecl.Name] = varDecl.Value;
                }
                else if (Match(TokenType.DictStart))
                {
                    var dict = ParseDictionary();
                    program.Dictionaries.Add(dict);
                }
                else if (Match(TokenType.Semicolon))
                {
                    
                }
                else
                {
                    if (Current().Type != TokenType.EOF)
                        throw new ParseException($"Неожиданный токен: {Current().Type} '{Current().Value}'",
                            Current().Line, Current().Column);
                }
            }

            return program;
        }

        private VarDeclarationNode ParseVarDeclaration()
        {
            var nameToken = Consume(TokenType.Identifier, "Ожидалось имя переменной после 'var'");
            Consume(TokenType.Assign, "Ожидалось ':=' после имени переменной");

            var value = ParseValue();
            Consume(TokenType.Semicolon, "Ожидалось ';' после значения переменной");

            return new VarDeclarationNode(nameToken.Value, value)
            {
                Line = nameToken.Line,
                Column = nameToken.Column
            };
        }

        private DictNode ParseDictionary()
        {
            var dict = new DictNode
            {
                Line = Current(-1).Line,
                Column = Current(-1).Column
            };

            while (!Check(TokenType.DictEnd) && !IsAtEnd())
            {
                var keyToken = Consume(TokenType.Word, "Ожидалось имя в словаре (заглавными буквами)");
                Consume(TokenType.Equals, "Ожидалось '=' после имени в словаре");

                var value = ParseValue();
                Consume(TokenType.Semicolon, "Ожидалось ';' после значения в словаре");

                dict.Items[keyToken.Value] = value;
            }

            Consume(TokenType.DictEnd, "Ожидалось '}' в конце словаря");
            return dict;
        }

        private ValueNode ParseValue()
        {
            if (Match(TokenType.Number))
            {
                return new NumberNode(int.Parse(Previous().Value))
                {
                    Line = Previous().Line,
                    Column = Previous().Column
                };
            }
            else if (Match(TokenType.HexNumber))
            {
                return new HexNumberNode(Previous().Value)
                {
                    Line = Previous().Line,
                    Column = Previous().Column
                };
            }
            else if (Match(TokenType.String))
            {
                return new StringNode(Previous().Value)
                {
                    Line = Previous().Line,
                    Column = Previous().Column
                };
            }
            else if (Match(TokenType.Identifier))
            {
                return new VariableNode(Previous().Value)
                {
                    Line = Previous().Line,
                    Column = Previous().Column
                };
            }
            else if (Match(TokenType.DictStart))
            {
                return ParseDictionary();
            }
            else if (Match(TokenType.Dollar))
            {
                return ParseExpression();
            }
            else
            {
                throw new ParseException($"Ожидалось значение, получено: {Current().Type}",
                    Current().Line, Current().Column);
            }
        }

        private ExpressionNode ParseExpression()
        {
            var line = Current(-1).Line;
            var column = Current(-1).Column;

            
            ExpressionNode.OperationType operation;
            if (Match(TokenType.Plus))
                operation = ExpressionNode.OperationType.Add;
            else if (Match(TokenType.Minus))
                operation = ExpressionNode.OperationType.Subtract;
            else if (Match(TokenType.Multiply))
                operation = ExpressionNode.OperationType.Multiply;
            else if (Match(TokenType.MinFunc))
                operation = ExpressionNode.OperationType.Min;
            else
                throw new ParseException($"Ожидалась операция (+, -, *, min), получено: {Current().Type}",
                    Current().Line, Current().Column);

            
            var left = ParseValue();

            
            var right = ParseValue();

            Consume(TokenType.Dollar, "Ожидалось '$' в конце выражения");

            return new ExpressionNode(operation, left, right)
            {
                Line = line,
                Column = column
            };
        }

        #region Вспомогательные методы

        private bool Match(params TokenType[] types)
        {
            foreach (var type in types)
            {
                if (Check(type))
                {
                    Advance();
                    return true;
                }
            }
            return false;
        }

        private bool Check(TokenType type)
        {
            if (IsAtEnd()) return false;
            return Current().Type == type;
        }

        private Token Advance()
        {
            if (!IsAtEnd()) _position++;
            return Previous();
        }

        private bool IsAtEnd() => Current().Type == TokenType.EOF;

        private Token Current(int offset = 0)
        {
            int index = _position + offset;
            if (index >= _tokens.Count) return _tokens[^1];
            return _tokens[index];
        }

        private Token Previous() => Current(-1);

        private Token Consume(TokenType type, string message)
        {
            if (Check(type)) return Advance();
            throw new ParseException(message, Current().Line, Current().Column);
        }

        #endregion
    }

    public class ParseException : Exception
    {
        public int Line { get; }
        public int Column { get; }

        public ParseException(string message, int line, int column)
            : base($"{message} в строке {line}, позиция {column}")
        {
            Line = line;
            Column = column;
        }
    }
}
