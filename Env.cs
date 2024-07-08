
class Env(Env? enclosing = null) {
    private readonly Dictionary<string, object?> _values = [];
    private readonly Env? _enclosing = enclosing;

    public void define(string name, object? value) {
        _values[name] = value;
    }

    public object? get(Token name) {
        if (_values.TryGetValue(name.lexeme, out object? value)) {
            return value;
        } else if (_enclosing != null) {
            return _enclosing.get(name);
        }

        throw new RuntimeErrorException(name, $"Undefined variable '{name.lexeme}'.");
    }

    public void assign(Token name, object? value) {
        if (_values.ContainsKey(name.lexeme)) {
            _values[name.lexeme] = value;
            return;
        } else if (_enclosing != null) {
            _enclosing.assign(name, value);
            return;
        }

        throw new RuntimeErrorException(name, $"Undefined variable '{name.lexeme}'.");
    }
}