
class ClockFunction : LoxCallable {
    public int arity() {
        return 0;
    }

    public object? call(Interpreter interpreter, List<object?> arguments) {
        return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }

    public override string ToString() {
        return "<native fn>";
    }
}