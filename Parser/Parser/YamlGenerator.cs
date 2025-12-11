using System;
using System.Collections.Generic;
using System.Text;

namespace Parser
{
    public class YamlGenerator
    {
        private readonly Evaluator _evaluator;
        private readonly StringBuilder _output;
        private int _indentLevel;

        public YamlGenerator(Evaluator evaluator)
        {
            _evaluator = evaluator;
            _output = new StringBuilder();
            _indentLevel = 0;
        }

        public string Generate(ProgramNode ast)
        {
            _output.Clear();
            _indentLevel = 0;

            
            bool firstDict = true;
            foreach (var dict in ast.Dictionaries)
            {
                if (!firstDict)
                {
                    _output.AppendLine();
                    _output.AppendLine("---"); 
                }

                GenerateDictionary(dict);
                firstDict = false;
            }

            return _output.ToString();
        }

        private void GenerateDictionary(DictNode dict)
        {
            var evaluatedDict = _evaluator.EvaluateToDictionary(dict); 
            if (evaluatedDict == null) return;

            foreach (var kvp in evaluatedDict)
            {
                GenerateKeyValue(kvp.Key, kvp.Value, isRoot: true);
            }
        }

        private void GenerateKeyValue(string key, object value, bool isRoot = false)
        {
            var indent = new string(' ', _indentLevel * 2);

            if (value is Dictionary<string, object> nestedDict)
            {
                _output.AppendLine($"{indent}{key}:");
                _indentLevel++;

                foreach (var nestedKvp in nestedDict)
                {
                    GenerateKeyValue(nestedKvp.Key, nestedKvp.Value);
                }

                _indentLevel--;
            }
            else if (value is List<object> list)
            {
                _output.AppendLine($"{indent}{key}:");
                _indentLevel++;

                foreach (var item in list)
                {
                    _output.AppendLine($"{new string(' ', _indentLevel * 2)}- {FormatValue(item)}");
                }

                _indentLevel--;
            }
            else
            {
                _output.AppendLine($"{indent}{key}: {FormatValue(value)}");
            }
        }

        private string FormatValue(object value)
        {
            if (value == null) return "null";

            if (value is string str)
            {
                
                if (NeedsQuotes(str))
                {
                    return $"\"{EscapeString(str)}\"";
                }
                return str;
            }

            if (value is bool b) return b.ToString().ToLower();
            if (value is int i) return i.ToString();

            
            return value.ToString();
        }

        private bool NeedsQuotes(string str)
        {
            
            return string.IsNullOrEmpty(str) ||
                   str.Contains(":") ||
                   str.Contains("#") ||
                   str.Contains("[") ||
                   str.Contains("]") ||
                   str.Contains("{") ||
                   str.Contains("}") ||
                   str.Contains(",") ||
                   str.Contains("&") ||
                   str.Contains("*") ||
                   str.Contains("!") ||
                   str.Contains("|") ||
                   str.Contains(">") ||
                   str.Contains("'") ||
                   str.Contains("\"") ||
                   str.StartsWith(" ") ||
                   str.EndsWith(" ") ||
                   str == "true" ||
                   str == "false" ||
                   str == "null" ||
                   str == "yes" ||
                   str == "no" ||
                   str == "on" ||
                   str == "off";
        }

        private string EscapeString(string str)
        {
           
            return str.Replace("\"", "\\\"");
        }
    }
}