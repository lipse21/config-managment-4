using System;
using System.IO;
using Parser.Parser;

namespace Parser
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
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
                    return;
                }

                string inputText = File.ReadAllText(arguments.InputFile);
                Console.WriteLine("Файл прочитан: " + arguments.InputFile);

                Lexer lexer = new Lexer(inputText);
                var tokens = lexer.Tokenize();

                Console.WriteLine("Найдено " + tokens.Count + " токенов");

                Console.WriteLine("\n=== Все токены ===");
                foreach (var token in tokens)
                {
                    if (token.Type == TokenType.Error)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                    }
                    Console.WriteLine("  " + token);
                    Console.ResetColor();

                    if (token.Type == TokenType.Error)
                    {
                        break;
                    }
                }

                // ФИКС: Проверяем что путь не пустой
                string outputDir = Path.GetDirectoryName(arguments.OutputFile);
                if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
                {
                    Directory.CreateDirectory(outputDir);
                }

                File.WriteAllText(arguments.OutputFile, "# Заглушка - YAML будет на этапе 3\n");
                Console.WriteLine("\nФайл создан: " + arguments.OutputFile);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка: " + ex.Message);
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
        }

        static void RunInteractiveMode()
        {
            Console.WriteLine("=== Интерактивный режим ===");
            Console.Write("Входной файл: ");
            string inputFile = Console.ReadLine();

            Console.Write("Выходной файл: ");
            string outputFile = Console.ReadLine();

            string[] args = { "-i", inputFile, "-o", outputFile };
            Main(args);
        }
    }
}