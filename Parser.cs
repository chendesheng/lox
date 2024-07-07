

// expression     → assignment ;
// assignment     → IDENTIFIER "=" assignment | equality ;
// equality       → comparison ( ( "!=" | "==" ) comparison )* ;
// comparison     → term ( ( ">" | ">=" | "<" | "<=" ) term )* ;
// term           → factor ( ( "-" | "+" ) factor )* ;
// factor         → unary ( ( "/" | "*" ) unary )* ;
// unary          → ( "!" | "-" ) unary | primary ;
// primary        → NUMBER | STRING | "true" | "false" | "nil" | "(" expression ")" | IDENTIFIER ;

// program        → declaration* EOF ;
// declaration    → varDecl | statement ;
// varDecl        → "var" IDENTIFIER ( "=" expression )? ";" ;
// statement      → exprStmt | printStmt ;
// exprStmt       → expression ";" ;
// printStmt      → "print" expression ";" ;
class Parser(List<Token> tokens) {
    private readonly List<Token> _tokens = tokens;
    private int _current = 0;

    public Program parse() {
        List<Stmt> statements = [];
        while (!is_at_end()) {
            statements.Add(declaration());
        }
        return new Program(statements);
    }

    private Stmt declaration() {
        try {
            if (match(TokenType.VAR)) return var_declaration();
            return statement();
        } catch (ParseErrorException) {
            synchronize();
            return null!;
        }
    }

    private VarStmt var_declaration() {
        Token name = consume(TokenType.IDENTIFIER, "Expect variable name.");

        Expr? initializer = null;
        if (match(TokenType.EQUAL)) {
            initializer = expression();
        }

        consume(TokenType.SEMICOLON, "Expect ';' after variable declaration.");
        return new VarStmt(name, initializer);
    }

    private Stmt statement() {
        if (match(TokenType.PRINT)) return print_statement();
        return expression_statement();
    }

    private ExpressionStmt expression_statement() {
        Expr expr = expression();
        consume(TokenType.SEMICOLON, "Expect ';' after expression.");
        return new ExpressionStmt(expr);
    }

    private PrintStmt print_statement() {
        Expr expr = expression();
        consume(TokenType.SEMICOLON, "Expect ';' after value.");
        return new PrintStmt(expr);
    }

    private Expr expression() {
        return assignment();
    }

    private Expr assignment() {
        Expr expr = equality();

        if (match(TokenType.EQUAL)) {
            Token equals = previous();
            Expr value = assignment();

            if (expr is VariableExpr variable) {
                Token name = variable.name;
                return new AssignExpr(name, value);
            }

            throw parse_error(equals, "Invalid assignment target.");
        }

        return expr;
    }

    private Expr equality() {
        Expr expr = comparison();

        while (match(TokenType.BANG_EQUAL, TokenType.EQUAL_EQUAL)) {
            Token op = previous();
            Expr right = comparison();
            expr = new BinaryExpr(expr, op, right);
        }

        return expr;
    }

    private Expr comparison() {
        Expr expr = term();

        while (match(TokenType.GREATER, TokenType.GREATER_EQUAL, TokenType.LESS, TokenType.LESS_EQUAL)) {
            Token op = previous();
            Expr right = term();
            expr = new BinaryExpr(expr, op, right);
        }

        return expr;
    }

    private Expr term() {
        Expr expr = factor();

        while (match(TokenType.MINUS, TokenType.PLUS)) {
            Token op = previous();
            Expr right = factor();
            expr = new BinaryExpr(expr, op, right);
        }

        return expr;
    }

    private Expr factor() {
        Expr expr = unary();

        while (match(TokenType.SLASH, TokenType.STAR)) {
            Token op = previous();
            Expr right = unary();
            expr = new BinaryExpr(expr, op, right);
        }

        return expr;
    }

    private Expr unary() {
        if (match(TokenType.BANG, TokenType.MINUS)) {
            Token op = previous();
            Expr right = unary();
            return new UnaryExpr(op, right);
        }

        return primary();
    }

    private Expr primary() {
        if (match(TokenType.FALSE)) return new LiteralExpr(false);
        if (match(TokenType.TRUE)) return new LiteralExpr(true);
        if (match(TokenType.NIL)) return new LiteralExpr(null);

        if (match(TokenType.NUMBER, TokenType.STRING)) {
            return new LiteralExpr(previous().literal);
        }

        if (match(TokenType.LEFT_PAREN)) {
            Expr expr = expression();
            consume(TokenType.RIGHT_PAREN, "Expect ')' after expression.");
            return new GroupingExpr(expr);
        }

        if (match(TokenType.IDENTIFIER)) {
            return new VariableExpr(previous());
        }

        throw parse_error(peek(), "Expect expression.");
    }

    private static ParseErrorException parse_error(Token token, string message) {
        Lox.error(token, message);
        return new ParseErrorException();
    }

    private Token consume(TokenType type, string message) {
        if (check(type)) {
            var token = peek();
            advance();
            return token;
        } else throw parse_error(peek(), message);
    }

    private void advance() {
        if (!is_at_end()) _current++;
    }

    private bool check(TokenType type) {
        if (is_at_end()) return false;
        return peek().type == type;
    }

    private Token peek() {
        return _tokens[_current];
    }

    private Token previous() {
        return _tokens[_current - 1];
    }

    private bool is_at_end() {
        return peek().type == TokenType.EOF;
    }

    private bool match(params TokenType[] types) {
        foreach (TokenType type in types) {
            if (check(type)) {
                advance();
                return true;
            }
        }

        return false;
    }

    private void synchronize() {
        advance();

        while (!is_at_end()) {
            if (previous().type == TokenType.SEMICOLON) return;

            switch (peek().type) {
                case TokenType.CLASS:
                case TokenType.FUN:
                case TokenType.VAR:
                case TokenType.FOR:
                case TokenType.IF:
                case TokenType.WHILE:
                case TokenType.PRINT:
                case TokenType.RETURN:
                    return;
            }

            advance();
        }
    }
}

class ParseErrorException : Exception {
}
