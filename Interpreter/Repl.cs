namespace Interpreter;

public class Repl
{
    public const string Prompt = ">> ";
    
    public Repl Attach(TextReader reader, TextWriter writer, Environment env)
    {
        while (true)
        {
            writer.Write(Prompt);
            string? line = reader.ReadLine(); 
            if (line is null) { return this; }

            Lexer lexer = new(line);
            Parser parser = new(lexer);

            var program = parser.ParseProgram();
            if (parser.Errors.Count > 0)
            {
                PrintParserErrors(parser, writer);
                continue;
            }
            
            if (Evaluator.Evaluate(program, env) is { } evaluated)
            {
                writer.WriteLine(evaluated.Inspect());
            }
        }
    }
    
    public Repl Start(TextReader reader, TextWriter writer, out Environment env)
    {
        env = new();
        Attach(reader, writer, env);
        return this;
    }
    
    private static void PrintParserErrors(Parser parser, TextWriter writer)
    {
        foreach (var error in parser.Errors)
        {
            writer.WriteLine($"\t{error}");
        }
    }
}