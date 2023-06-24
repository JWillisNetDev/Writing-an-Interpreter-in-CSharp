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
            for (Token token = lexer.MoveNext();
                 token.Type != TokenType.EndOfFile;
                 token = lexer.MoveNext())
            {
                writer.WriteLine($"{token}");
            }
        }
    }
}