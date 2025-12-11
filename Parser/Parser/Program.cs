using System;
using System.IO;

namespace Parser
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            if (args.Length == 0)
            {
                PrintUsage();
                RunInteractiveMode();
                return;
            }

            try
            {
                Arguments arguments = ParseArguments(args);
                if (arguments == null)
                {
                    PrintUsage();
                    return;
                }

                if (!File.Exists(arguments.InputFile))
                {
                    Console.WriteLine("Ошибка: Входной файл не существует - " + arguments.InputFile);
                    Console.WriteLine("Текущая директория: " + Directory.GetCurrentDirectory());
                    return;
                }

                string inputText = File.ReadAllText(arguments.InputFile);
                Console.WriteLine("Файл прочитан: " + arguments.InputFile);

               
                Console.WriteLine("\n=== Лексический анализ ===");
                Lexer lexer = new Lexer(inputText);
                var tokens = lexer.Tokenize();

                Console.WriteLine("Найдено " + tokens.Count + " токенов");

                
                bool hasLexerErrors = false;
                foreach (var token in tokens)
                {
                    if (token.Type == TokenType.Error)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("  Лексическая ошибка: " + token.Value + " в строке " + token.Line);
                        Console.ResetColor();
                        hasLexerErrors = true;
                    }
                }

                if (hasLexerErrors)
                {
                    Console.WriteLine("\nПрограмма остановлена из-за лексических ошибок");
                    return;
                }

               
                Console.WriteLine("\nПервые 20 токенов:");
                for (int i = 0; i < Math.Min(20, tokens.Count); i++)
                {
                    var token = tokens[i];
                    Console.WriteLine($"  {i,2}: {token.Type,-15} '{token.Value}' at {token.Line}:{token.Column}");
                }
                if (tokens.Count > 20)
                {
                    Console.WriteLine($"  ... и еще {tokens.Count - 20} токенов");
                }

                
                Console.WriteLine("\n=== Синтаксический анализ (парсинг) ===");
                Parser parser = new Parser(tokens);
                ProgramNode ast;

                try
                {
                    ast = parser.Parse();
                    Console.WriteLine("   AST успешно построен");
                    Console.WriteLine($"  Объявлено переменных: {ast.Variables.Count}");
                    Console.WriteLine($"  Найдено словарей: {ast.Dictionaries.Count}");

                   
                    if (ast.Variables.Count > 0)
                    {
                        Console.WriteLine("\n  Переменные:");
                        foreach (var varDecl in ast.Variables)
                        {
                            Console.WriteLine($"    {varDecl.Name} := ... (строка {varDecl.Line})");
                        }
                    }
                }
                catch (ParseException ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\n   Ошибка парсинга: " + ex.Message);
                    Console.ResetColor();
                    return;
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\n   Неожиданная ошибка при парсинге: " + ex.Message);
                    Console.ResetColor();
                    return;
                }

               
                Console.WriteLine("\n=== Вычисление константных выражений ===");
                Evaluator evaluator = new Evaluator();

               
                Console.WriteLine("  Вычисление переменных:");
                foreach (var varDecl in ast.Variables)
                {
                    try
                    {
                        var value = evaluator.Evaluate(varDecl.Value);
                        evaluator.SetVariable(varDecl.Name, value);
                        Console.WriteLine($"     {varDecl.Name} = {FormatValue(value)}");
                    }
                    catch (EvaluationException ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"      Ошибка вычисления переменной {varDecl.Name}: {ex.Message}");
                        Console.ResetColor();
                        return;
                    }
                }

               
                Console.WriteLine("\n  Вычисление словарей:");
                int dictNumber = 1;
                foreach (var dict in ast.Dictionaries)
                {
                    try
                    {
                        var dictValue = evaluator.Evaluate(dict);
                        var dictAsDict = dictValue as System.Collections.IDictionary;
                        if (dictAsDict != null)
                        {
                            Console.WriteLine($"     Словарь #{dictNumber}: {dictAsDict.Count} элементов");

                            
                            foreach (System.Collections.DictionaryEntry entry in dictAsDict)
                            {
                                Console.WriteLine($"        {entry.Key} = {FormatValue(entry.Value)}");
                            }
                        }
                        dictNumber++;
                    }
                    catch (EvaluationException ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"      Ошибка вычисления словаря: {ex.Message}");
                        Console.ResetColor();
                        return;
                    }
                }

                
                string outputDir = Path.GetDirectoryName(arguments.OutputFile);
                if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
                {
                    Directory.CreateDirectory(outputDir);
                }

                
                string yamlContent = "# Результат парсинга (YAML будет потом)\n";
                yamlContent += $"# Файл: {Path.GetFileName(arguments.InputFile)}\n";
                yamlContent += $"# Время обработки: {DateTime.Now}\n\n";

               
                if (ast.Variables.Count > 0)
                {
                    yamlContent += "# Объявленные переменные:\n";
                    foreach (var varDecl in ast.Variables)
                    {
                        yamlContent += $"#   {varDecl.Name}\n";
                    }
                    yamlContent += "\n";
                }

                File.WriteAllText(arguments.OutputFile, yamlContent);
                Console.WriteLine("\n  Файл создан: " + arguments.OutputFile);
                Console.WriteLine("\n  Этап 2 завершен успешно! Парсер работает корректно.");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n  Критическая ошибка: " + ex.Message);
                Console.WriteLine("StackTrace: " + ex.StackTrace);
                Console.ResetColor();
            }
        }

        static Arguments ParseArguments(string[] args)
        {
            Arguments arguments = new Arguments();

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-i" && i + 1 < args.Length)
                {
                    arguments.InputFile = args[++i];
                }
                else if (args[i] == "-o" && i + 1 < args.Length)
                {
                    arguments.OutputFile = args[++i];
                }
                else
                {
                    return null;
                }
            }

            if (string.IsNullOrEmpty(arguments.InputFile) ||
                string.IsNullOrEmpty(arguments.OutputFile))
            {
                return null;
            }

            return arguments;
        }

        static void PrintUsage()
        {
            Console.WriteLine("Parser - конвертер учебного конфигурационного языка в YAML");
            Console.WriteLine("Использование: Parser -i <input_file> -o <output_file>");
            Console.WriteLine("Пример: Parser -i test.txt -o output.yaml");
            Console.WriteLine("\nПримеры тестовых файлов:");
            Console.WriteLine("  test_inputs\\test_basic.txt      - базовый тест");
            Console.WriteLine("  test_inputs\\test_parser1.txt    - простой парсер тест");
            Console.WriteLine("  test_inputs\\test_parser2.txt    - вложенные словари");
            Console.WriteLine("  test_inputs\\test_parser3.txt    - выражения");
            Console.WriteLine("  test_inputs\\test_errors.txt     - тест ошибок");
            Console.WriteLine();
        }

        static void RunInteractiveMode()
        {
            Console.WriteLine("\n=== Интерактивный режим ===");
            Console.WriteLine("Введите пути к файлам (или нажмите Enter для значений по умолчанию)");

            Console.Write("Входной файл [test_inputs\\test_basic.txt]: ");
            string inputFile = Console.ReadLine();
            if (string.IsNullOrEmpty(inputFile))
                inputFile = "test_inputs\\test_basic.txt";

            Console.Write("Выходной файл [output.yaml]: ");
            string outputFile = Console.ReadLine();
            if (string.IsNullOrEmpty(outputFile))
                outputFile = "output.yaml";

            string[] args = { "-i", inputFile, "-o", outputFile };
            Main(args);
        }

        static string FormatValue(object value)
        {
            if (value == null) return "null";
            if (value is string s) return "\"" + s + "\"";
            if (value is int i) return i.ToString();
            if (value is System.Collections.IDictionary dict)
                return $"{{ словарь, {dict.Count} элементов }}";

            return value.ToString();
        }
    }
}