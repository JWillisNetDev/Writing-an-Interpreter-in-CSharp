namespace Interpreter;

public class Repl
{
    public const string Prompt = ">> ";

    public void Start(TextReader reader, TextWriter writer)
    {
        while (true)
        {
            writer.Write(Prompt);
            string? line = reader.ReadLine(); 
            if (line is null) { return; }

            Lexer lexer = new(line);
            Parser parser = new(lexer);

            var program = parser.ParseProgram();
            if (parser.Errors.Count > 0)
            {
                PrintParserErrors(parser, writer);
                continue;
            }
            
            if (Evaluator.Evaluate(program) is { } evaluated)
            {
                writer.WriteLine(evaluated.Inspect());
            }
        }
    }

    private static void PrintParserErrors(Parser parser, TextWriter writer)
    {
        foreach (var error in parser.Errors)
        {
            writer.WriteLine($"\t{error}");
        }
    }
}