
class Env {
    private readonly Dictionary<string, object?> _values = [];

    public void define(string name, object? value) {
        _values[name] = value;
    }

    public object? get(Token name) {
        if (_values.TryGetValue(name.lexeme, out object? value)) {
            return value;
        }

        throw new RuntimeErrorException(name, $"Undefined variable '{name.lexeme}'.");
    }

    public void assign(Token name, object? value) {
        if (_values.ContainsKey(name.lexeme)) {
            _values[name.lexeme] = value;
            return;
        }

        throw new RuntimeErrorException(name, $"Undefined variable '{name.lexeme}'.");
    }
}