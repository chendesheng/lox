

// expression     → assignment ;
// assignment     → IDENTIFIER "=" assignment | logic_or ;
// logic_or       → logic_and ( "or" logic_and )* ;
// logic_and      → equality ( "and" equality )*
// equality       → comparison ( ( "!=" | "==" ) comparison )* ;
// comparison     → term ( ( ">" | ">=" | "<" | "<=" ) term )* ;
// term           → factor ( ( "-" | "+" ) factor )* ;
// factor         → unary ( ( "/" | "*" ) unary )* ;
// unary          → ( "!" | "-" ) unary | call ;
// call           → primary ( "(" arguments? ")" )* ;
// arguments      → expression ( "," expression )* ;
// primary        → NUMBER | STRING | "true" | "false" | "nil" | "(" expression ")" | IDENTIFIER ;

// program        → declaration* EOF ;
// declaration    → funDecl | varDecl | statement ;
// funDecl        → "fun" function ;
// function       → IDENTIFIER "(" parameters? ")" block ;
// parameters     → IDENTIFIER ( "," IDENTIFIER )* ;
// varDecl        → "var" IDENTIFIER ( "=" expression )? ";" ;
// statement      → exprStmt | forStmt | ifStmt | whileStmt | printStmt | returnStmt | block ;
// forStmt        → "for" "(" ( varDecl | exprStmt | ";" ) expression? ";" expression? ")" statement ;
// ifStmt         → "if" "(" expression ")" statement ( "else" statement )? ;
// whileStmt      → "while" "(" expression ")" statement ;
// block          → "{" declaration* "}" ;
// exprStmt       → expression ";" ;
// printStmt      → "print" expression ";" ;
// returnStmt     → "return" expression? ";" ;
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
            if (match(TokenType.FUN)) return function("function");
            if (match(TokenType.VAR)) return var_declaration();
            return statement();
        } catch (ParseErrorException) {
            synchronize();
            return null!;
        }
    }

    private FunctionStmt function(string kind) {
        Token name = consume(TokenType.IDENTIFIER, $"Expect {kind} name.");
        consume(TokenType.LEFT_PAREN, $"Expect '(' after {kind} name.");

        List<Token> parameters = [];
        if (!check(TokenType.RIGHT_PAREN)) {
            do {
                if (parameters.Count >= 255) {
                    throw parse_error(peek(), "Cannot have more than 255 parameters.");
                }
                parameters.Add(consume(TokenType.IDENTIFIER, "Expect parameter name."));
            } while (match(TokenType.COMMA));
        }

        consume(TokenType.RIGHT_PAREN, "Expect ')' after parameters.");
        consume(TokenType.LEFT_BRACE, $"Expect '{{' before {kind} body.");
        List<Stmt> body = block().statements;
        return new FunctionStmt(name, parameters, body);
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
        if (match(TokenType.IF)) return if_statement();
        if (match(TokenType.FOR)) return for_statement();
        if (match(TokenType.WHILE)) return while_statement();
        if (match(TokenType.RETURN)) return return_statement();
        if (match(TokenType.PRINT)) return print_statement();
        if (match(TokenType.LEFT_BRACE)) return block();
        return expression_statement();
    }

    private ReturnStmt return_statement() {
        Token keyword = previous();
        Expr? value = null;
        if (!check(TokenType.SEMICOLON)) {
            value = expression();
        }

        consume(TokenType.SEMICOLON, "Expect ';' after return value.");
        return new ReturnStmt(keyword, value);
    }

    private ExpressionStmt expression_statement() {
        Expr expr = expression();
        consume(TokenType.SEMICOLON, "Expect ';' after expression.");
        return new ExpressionStmt(expr);
    }

    private IfStmt if_statement() {
        consume(TokenType.LEFT_PAREN, "Expect '(' after 'if'.");
        Expr condition = expression();
        consume(TokenType.RIGHT_PAREN, "Expect ')' after if condition.");

        Stmt then_branch = statement();
        Stmt? else_branch = null;
        if (match(TokenType.ELSE)) {
            else_branch = statement();
        }

        return new IfStmt(condition, then_branch, else_branch);
    }

    private ForStmt for_statement() {
        consume(TokenType.LEFT_PAREN, "Expect '(' after 'for'.");

        Stmt initializer;
        if (match(TokenType.SEMICOLON)) {
            initializer = null!;
        } else if (match(TokenType.VAR)) {
            initializer = var_declaration();
        } else {
            initializer = expression_statement();
        }

        Expr? condition = null;
        if (!check(TokenType.SEMICOLON)) {
            condition = expression();
        }
        consume(TokenType.SEMICOLON, "Expect ';' after loop condition.");

        Expr? increment = null;
        if (!check(TokenType.RIGHT_PAREN)) {
            increment = expression();
        }
        consume(TokenType.RIGHT_PAREN, "Expect ')' after for clauses.");

        Stmt body = statement();

        if (increment != null) {
            body = new BlockStmt(new List<Stmt> { body, new ExpressionStmt(increment) });
        }

        condition ??= new LiteralExpr(true);
        body = new WhileStmt(condition, body);

        if (initializer != null) {
            body = new BlockStmt([initializer, body]);
        }

        return new ForStmt(initializer, condition, increment, body);
    }

    private WhileStmt while_statement() {
        consume(TokenType.LEFT_PAREN, "Expect '(' after 'while'.");
        Expr condition = expression();
        consume(TokenType.RIGHT_PAREN, "Expect ')' after while condition.");

        Stmt body = statement();
        return new WhileStmt(condition, body);
    }

    private BlockStmt block() {
        List<Stmt> statements = [];

        while (!check(TokenType.RIGHT_BRACE) && !is_at_end()) {
            statements.Add(declaration());
        }

        consume(TokenType.RIGHT_BRACE, "Expect '}' after block.");
        return new BlockStmt(statements);
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
        Expr expr = or_expr();

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

    private Expr or_expr() {
        Expr expr = and_expr();

        while (match(TokenType.OR)) {
            Token op = previous();
            Expr right = and_expr();
            expr = new LogicalExpr(expr, op, right);
        }

        return expr;
    }

    private Expr and_expr() {
        Expr expr = equality();

        while (match(TokenType.AND)) {
            Token op = previous();
            Expr right = equality();
            expr = new LogicalExpr(expr, op, right);
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

        return call();
    }

    private Expr call() {
        Expr expr = primary();

        while (true) {
            if (match(TokenType.LEFT_PAREN)) {
                expr = finish_call(expr);
            } else {
                break;
            }
        }

        return expr;
    }

    private CallExpr finish_call(Expr callee) {
        List<Expr> arguments = [];

        if (!check(TokenType.RIGHT_PAREN)) {
            do {
                if (arguments.Count >= 255) {
                    throw parse_error(peek(), "Cannot have more than 255 arguments.");
                }
                arguments.Add(expression());
            } while (match(TokenType.COMMA));
        }

        Token paren = consume(TokenType.RIGHT_PAREN, "Expect ')' after arguments.");

        return new CallExpr(callee, paren, arguments);
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
