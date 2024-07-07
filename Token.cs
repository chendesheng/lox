class Token(TokenType type, string lexeme, object? literal, int line) {
    public TokenType type = type;
    public string lexeme = lexeme;
    public object? literal = literal;
    public int line = line;

    public override string ToString() {
        return $"{type} {lexeme} {literal}";
    }
}