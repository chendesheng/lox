class Scanner(string source) {
    private readonly string _source = source;
    private readonly List<Token> _tokens = [];
    private int _start = 0;
    private int _current = 0;
    private int _line = 1;

    public List<Token> scan_tokens() {
        while (!is_at_end()) {
            _start = _current;
            scan_token();
        }

        _tokens.Add(new Token(TokenType.EOF, "", null, _line));
        return _tokens;
    }

    private void scan_token() {
        var c = advance();
        switch (c) {
            case '(': add_token(TokenType.LEFT_PAREN, null); break;
            case ')': add_token(TokenType.RIGHT_PAREN, null); break;
            case '{': add_token(TokenType.LEFT_BRACE, null); break;
            case '}': add_token(TokenType.RIGHT_BRACE, null); break;
            case ',': add_token(TokenType.COMMA, null); break;
            case '.': add_token(TokenType.DOT, null); break;
            case '-': add_token(TokenType.MINUS, null); break;
            case '+': add_token(TokenType.PLUS, null); break;
            case ';': add_token(TokenType.SEMICOLON, null); break;
            case '*': add_token(TokenType.STAR, null); break;
            case '!': add_token(match('=') ? TokenType.BANG_EQUAL : TokenType.BANG, null); break;
            case '=': add_token(match('=') ? TokenType.EQUAL_EQUAL : TokenType.EQUAL, null); break;
            case '<': add_token(match('=') ? TokenType.LESS_EQUAL : TokenType.LESS, null); break;
            case '>': add_token(match('=') ? TokenType.GREATER_EQUAL : TokenType.GREATER, null); break;
            case '/':
                if (match('/')) {
                    while (peek() != '\n' && !is_at_end()) advance();
                } else {
                    add_token(TokenType.SLASH, null);
                }
                break;
            case ' ':
            case '\r':
            case '\t':
                break;
            case '\n':
                _line++;
                break;
            case '"': str(); break;
            default:
                if (is_digit(c)) {
                    number();
                } else if (is_alpha(c)) {
                    identifier();
                } else {
                    Lox.error(_line, "Unexpected character.");
                }
                break;
        }
    }

    private void identifier() {
        while (is_alphanumeric(peek())) advance();

        string text = _source[_start.._current];
        TokenType type = _keywords.GetValueOrDefault(text, TokenType.IDENTIFIER);
        add_token(type);
    }

    private void number() {
        while (is_digit(peek())) advance();

        if (peek() == '.' && is_digit(peek_next())) {
            advance();
            while (is_digit(peek())) advance();
        }

        add_token(TokenType.NUMBER, double.Parse(_source[_start.._current]));
    }

    private void str() {
        while (peek() != '"' && !is_at_end()) {
            if (peek() == '\n') _line++;
            advance();
        }

        if (is_at_end()) {
            Lox.error(_line, "Unterminated string.");
            return;
        }

        advance();

        string value = _source[(_start + 1)..(_current - 1)];
        add_token(TokenType.STRING, value);
    }

    private bool match(char expected) {
        if (_current >= _source.Length) return false;
        if (_source[_current] != expected) return false;

        _current++;
        return true;
    }

    private bool is_at_end() {
        return _current >= _source.Length;
    }

    private char peek() {
        if (is_at_end()) return '\0';
        return _source[_current];
    }

    private char peek_next() {
        if (_current + 1 >= _source.Length) return '\0';
        return _source[_current + 1];
    }

    private char advance() {
        return _source[_current++];
    }

    private void add_token(TokenType type, object? literal = null) {
        string text = _source[_start.._current];
        _tokens.Add(new Token(type, text, literal, _line));
    }

    private static bool is_alpha(char c) {
        return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '_';
    }

    private static bool is_alphanumeric(char c) {
        return is_alpha(c) || is_digit(c);
    }

    private static bool is_digit(char c) {
        return c >= '0' && c <= '9';
    }

    private static readonly Dictionary<string, TokenType> _keywords = new() {
        { "and", TokenType.AND },
        { "class", TokenType.CLASS },
        { "else", TokenType.ELSE },
        { "false", TokenType.FALSE },
        { "for", TokenType.FOR },
        { "fun", TokenType.FUN },
        { "if", TokenType.IF },
        { "nil", TokenType.NIL },
        { "or", TokenType.OR },
        { "print", TokenType.PRINT },
        { "return", TokenType.RETURN },
        { "super", TokenType.SUPER },
        { "this", TokenType.THIS },
        { "true", TokenType.TRUE },
        { "var", TokenType.VAR },
        { "while", TokenType.WHILE }
    };
}