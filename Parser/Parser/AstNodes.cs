

    namespace Parser
    {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    
    public abstract class AstNode
        {
            public int Line { get; set; }
            public int Column { get; set; }
        }

       
        public abstract class ValueNode : AstNode
        {
            public abstract object GetValue();
        }

        
        public class NumberNode : ValueNode
        {
            public int Value { get; }

            public NumberNode(int value)
            {
                Value = value;
            }

            public override object GetValue() => Value;
        }

       
        public class HexNumberNode : ValueNode
        {
            public int Value { get; }

            public HexNumberNode(string hexValue)
            {
                Value = Convert.ToInt32(hexValue, 16);
            }

            public override object GetValue() => Value;
        }

        
        public class StringNode : ValueNode
        {
            public string Value { get; }

            public StringNode(string value)
            {
                Value = value;
            }

            public override object GetValue() => Value;
        }

        
        public class VariableNode : ValueNode
        {
            public string Name { get; }

            public VariableNode(string name)
            {
                Name = name;
            }

            public override object GetValue()
            {
                throw new InvalidOperationException($"Значение переменной '{Name}' должно быть подставлено на этапе вычислений");
            }
        }

       
        public class DictNode : ValueNode
        {
            public Dictionary<string, ValueNode> Items { get; } = new();

            public override object GetValue() => Items;
        }

        
        public class VarDeclarationNode : AstNode
        {
            public string Name { get; }
            public ValueNode Value { get; }

            public VarDeclarationNode(string name, ValueNode value)
            {
                Name = name;
                Value = value;
            }
        }

       
        public class ExpressionNode : ValueNode
        {
            public enum OperationType
            {
                Add,      // +
                Subtract, // -
                Multiply, // *
                Min       // min
            }

            public OperationType Operation { get; }
            public ValueNode Left { get; }
            public ValueNode Right { get; }

            public ExpressionNode(OperationType operation, ValueNode left, ValueNode right)
            {
                Operation = operation;
                Left = left;
                Right = right;
            }

            public override object GetValue()
            {
                var leftVal = GetNumericValue(Left);
                var rightVal = GetNumericValue(Right);

                return Operation switch
                {
                    OperationType.Add => leftVal + rightVal,
                    OperationType.Subtract => leftVal - rightVal,
                    OperationType.Multiply => leftVal * rightVal,
                    OperationType.Min => Math.Min(leftVal, rightVal),
                    _ => throw new InvalidOperationException($"Неизвестная операция: {Operation}")
                };
            }

            private int GetNumericValue(ValueNode node)
            {
                var value = node.GetValue();
                return value switch
                {
                    int i => i,
                    string s when int.TryParse(s, out int num) => num,
                    _ => throw new InvalidOperationException($"Ожидалось число, получено: {value}")
                };
            }
        }

        
        public class ProgramNode : AstNode
        {
            public List<VarDeclarationNode> Variables { get; } = new();
            public List<DictNode> Dictionaries { get; } = new();
        }
    }

