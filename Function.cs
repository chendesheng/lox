interface LoxCallable {
    int arity();
    object? call(Interpreter interpreter, List<object?> arguments);
}

class LoxFunction(FunctionStmt declaration, Env closure) : LoxCallable {
    private readonly FunctionStmt _declaration = declaration;
    private readonly Env _closure = closure;

    public int arity() {
        return _declaration.parameters.Count;
    }

    public object? call(Interpreter interpreter, List<object?> arguments) {
        var env = new Env(_closure);
        for (int i = 0; i < _declaration.parameters.Count; i++) {
            env.define(_declaration.parameters[i].lexeme, arguments[i]);
        }

        try {
            interpreter.execute_block(_declaration.body, env);
        } catch (ReturnException return_value) {
            return return_value.value;
        }

        return null;
    }
}