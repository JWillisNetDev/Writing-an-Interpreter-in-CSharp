using Interpreter;
using System.CommandLine;
using Environment = Interpreter.Environment;

var rootCommand = new RootCommand("Interpreter and REPL CLI tool for the Monkey interpreted language.");

var fileOption = new Option<FileInfo>(name: "--file", description: "The file to run using Monkey interpreter")
{
    IsRequired = true,
};

var loadCommand = new Command("load", "Load and execute the Monkey file")
{
    fileOption,
};

var startCommand = new Command("start", "Start the Monkey interpreter REPL");

rootCommand.AddCommand(loadCommand);
rootCommand.AddCommand(startCommand);

Repl repl = new();
loadCommand.SetHandler((file) =>
{
    using Stream fileReadStream = file.OpenRead();
    using StreamReader reader = new StreamReader(fileReadStream);
    using TextReader buffer = new StringReader(reader.ReadToEnd().ReplaceLineEndings(""));
    repl.Start(buffer, Console.Out, out var environment)
        .Attach(Console.In, Console.Out, environment);
}, fileOption);

startCommand.SetHandler(() =>
{
    repl.Start(Console.In, Console.Out, out var environment);
});

await rootCommand.InvokeAsync(args);