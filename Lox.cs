class Lox {
    static private bool _has_error = false;
    static private bool _has_runtime_error = false;
    static readonly Interperter _interpreter = new();

    static void Main(string[] args) {
        if (args.Length > 1) {
            Console.WriteLine("Usage: lox [script]");
            Environment.Exit(64);
        } else if (args.Length == 1) {
            run_file(args[0]);
        } else {
            run_prompt();
        }
    }

    static void run_file(string path) {
        string source = File.ReadAllText(path);
        run(source);
    }

    static void run_prompt() {
        while (true) {
            Console.Write("> ");
            string? line = Console.ReadLine();
            if (line == null) break;
            run(line);
            if (_has_error) Environment.Exit(65);
            if (_has_runtime_error) Environment.Exit(70);
        }
    }

    static void run(string source) {
        Scanner scanner = new(source);
        List<Token> tokens = scanner.scan_tokens();
        Parser parser = new(tokens);
        Program program = parser.parse();
        if (_has_error) return;
        Console.WriteLine(AstPrinter.print(program));
        _interpreter.interpret(program);
    }

    public static void error(int line, string message) {
        report(line, "", message);
    }

    public static void runtime_error(RuntimeErrorException e) {
        Console.WriteLine($"{e.Message}\n[line {e.token.line}]");
        _has_runtime_error = true;
    }

    public static void error(Token token, string message) {
        if (token.type == TokenType.EOF) {
            report(token.line, " at end", message);
        } else {
            report(token.line, $" at '{token.lexeme}'", message);
        }
    }

    static void report(int line, string where, string message) {
        Console.WriteLine($"[line {line}] Error{where}: {message}");
        _has_error = true;
    }
}

