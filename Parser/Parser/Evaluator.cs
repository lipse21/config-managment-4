using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser
{
    public class Evaluator
    {
        private readonly Dictionary<string, object> _variableValues = new();

        public object Evaluate(ValueNode node)
        {
            return node switch
            {
                NumberNode n => n.Value,
                HexNumberNode h => h.Value,
                StringNode s => s.Value,
                VariableNode v => EvaluateVariable(v),
                DictNode d => EvaluateDictionary(d),
                ExpressionNode e => EvaluateExpression(e),
                _ => throw new InvalidOperationException($"Неизвестный тип узла: {node.GetType().Name}")
            };
        }

        private object EvaluateVariable(VariableNode node)
        {
            if (_variableValues.TryGetValue(node.Name, out var value))
                return value;

            throw new EvaluationException($"Необъявленная переменная: {node.Name}", node.Line, node.Column);
        }

        private Dictionary<string, object> EvaluateDictionary(DictNode node)
        {
            var result = new Dictionary<string, object>();

            foreach (var kvp in node.Items)
            {
                result[kvp.Key] = Evaluate(kvp.Value);
            }

            return result;
        }

        private object EvaluateExpression(ExpressionNode node)
        {
            var leftVal = GetNumericValue(node.Left);
            var rightVal = GetNumericValue(node.Right);

            return node.Operation switch
            {
                ExpressionNode.OperationType.Add => leftVal + rightVal,
                ExpressionNode.OperationType.Subtract => leftVal - rightVal,
                ExpressionNode.OperationType.Multiply => leftVal * rightVal,
                ExpressionNode.OperationType.Min => Math.Min(leftVal, rightVal),
                _ => throw new EvaluationException($"Неизвестная операция: {node.Operation}", node.Line, node.Column)
            };
        }

        private int GetNumericValue(ValueNode node)
        {
            var value = Evaluate(node);

            return value switch
            {
                int i => i,
                _ => throw new EvaluationException($"Ожидалось число, получено: {value} ({value.GetType().Name})",
                    node.Line, node.Column)
            };
        }

        public void SetVariable(string name, object value)
        {
            _variableValues[name] = value;
        }
    }

    public class EvaluationException : Exception
    {
        public int Line { get; }
        public int Column { get; }

        public EvaluationException(string message, int line, int column)
            : base($"{message} в строке {line}, позиция {column}")
        {
            Line = line;
            Column = column;
        }
    }
}
