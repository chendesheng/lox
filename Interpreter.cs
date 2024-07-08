
class Interpreter : Visitor<object?> {
    public readonly Env globals = new();
    private Env _env;

    public Interpreter() {
        _env = globals;
    }

    private object? evaluate(Expr expr) {
        return expr.accept(this);
    }

    private void evaluate(Stmt stmt) {
        stmt.accept(this);
    }

    static void check_number_operands(Token op, object? left, object? right) {
        if (left is double && right is double) return;
        throw new RuntimeErrorException(op, "Operands must be numbers.");
    }

    static void check_number_operand(Token op, object? operand) {
        if (operand is double) return;
        throw new RuntimeErrorException(op, "Operand must be a number.");
    }

    static bool is_truthy(object? obj) {
        if (obj == null) return false;
        if (obj is bool v) return v;
        return true;
    }

    static bool is_equal(object? a, object? b) {
        return Equals(a, b);
    }

    private static string stringify(object? obj) {
        if (obj == null) return "nil";
        if (obj is double v) {
            string text = v.ToString();
            if (text.EndsWith(".0")) {
                text = text[..^2];
            }
            return text;
        }
        return obj.ToString()!;
    }

    public void interpret(Program program) {
        try {
            List<Stmt> statements = program.statements;
            foreach (Stmt statement in statements) {
                evaluate(statement);
            }
        }
        catch (RuntimeErrorException e) {
            Lox.runtime_error(e);
        }
    }

    public object? visit_binary_expr(BinaryExpr expr) {
        object? left = evaluate(expr.left);
        object? right = evaluate(expr.right);

        switch (expr.op.type) {
            case TokenType.MINUS:
                check_number_operands(expr.op, left, right);
                return (double)left! - (double)right!;
            case TokenType.SLASH:
                check_number_operands(expr.op, left, right);
                return (double)left! / (double)right!;
            case TokenType.STAR:
                check_number_operands(expr.op, left, right);
                return (double)left! * (double)right!;
            case TokenType.PLUS:
                if (left is double && right is double) {
                    return (double)left! + (double)right!;
                }
                if (left is string && right is string) {
                    return (string)left! + (string)right!;
                }
                throw new RuntimeErrorException(expr.op, "Operands must be two numbers or two strings.");
            case TokenType.GREATER:
                check_number_operands(expr.op, left, right);
                return (double)left! > (double)right!;
            case TokenType.GREATER_EQUAL:
                check_number_operands(expr.op, left, right);
                return (double)left! >= (double)right!;
            case TokenType.LESS:
                check_number_operands(expr.op, left, right);
                return (double)left! < (double)right!;
            case TokenType.LESS_EQUAL:
                check_number_operands(expr.op, left, right);
                return (double)left! <= (double)right!;
            case TokenType.BANG_EQUAL:
                return !is_equal(left, right);
            case TokenType.EQUAL_EQUAL:
                return is_equal(left, right);
            default:
                return null;
        }
    }

    public object? visit_grouping_expr(GroupingExpr expr) {
        return evaluate(expr.expression);
    }

    public object? visit_literal_expr(LiteralExpr expr) {
        return expr.value;
    }

    public object? visit_unary_expr(UnaryExpr expr) {
        object? right = evaluate(expr.right);
        switch (expr.op.type) {
            case TokenType.MINUS:
                check_number_operand(expr.op, right);
                return -(double)right!;
            case TokenType.BANG:
                return !is_truthy(right);
            default:
                return null;
        }
    }

    public object? visit_expression_stmt(ExpressionStmt stmt) {
        return evaluate(stmt.expression);
    }

    public object? visit_print_stmt(PrintStmt printStmt) {
        object? value = evaluate(printStmt.expression);
        Console.WriteLine(stringify(value));
        return null;
    }

    public object? visit_variable_expr(VariableExpr variableExpr) {
        return _env.get(variableExpr.name);
    }

    public object? visit_var_stmt(VarStmt varStmt) {
        object? value = null;
        if (varStmt.initializer != null) {
            value = evaluate(varStmt.initializer);
        }
        _env.define(varStmt.name.lexeme, value);
        return null;
    }

    public object? visit_assign_expr(AssignExpr assignExpr) {
        object? value = evaluate(assignExpr.value);
        _env.assign(assignExpr.name, value);
        return null;
    }

    public object? visit_program(Program program) {
        foreach (Stmt statement in program.statements) {
            evaluate(statement);
        }
        return null;
    }

    public object? visit_if_stmt(IfStmt if_stmt) {
        if (is_truthy(evaluate(if_stmt.condition))) {
            evaluate(if_stmt.then_branch);
        } else if (if_stmt.else_branch != null) {
            evaluate(if_stmt.else_branch);
        }
        return null;
    }

    public object? visit_block_stmt(BlockStmt block) {
        foreach (Stmt statement in block.statements) {
            evaluate(statement);
        }
        return null;
    }

    public object? visit_while_stmt(WhileStmt while_stmt) {
        while (is_truthy(evaluate(while_stmt.condition))) {
            evaluate(while_stmt.body);
        }
        return null;
    }

    public object? visit_for_stmt(ForStmt for_stmt) {
        if (for_stmt.initializer != null) {
            evaluate(for_stmt.initializer);
        }
        while (for_stmt.condition == null || is_truthy(evaluate(for_stmt.condition))) {
            evaluate(for_stmt.body);
            if (for_stmt.increment != null) {
                evaluate(for_stmt.increment);
            }
        }
        return null;
    }

    public object? visit_logical_expr(LogicalExpr logicalExpr) {
        object? left = evaluate(logicalExpr.left);
        if (logicalExpr.op.type == TokenType.OR) {
            if (is_truthy(left)) return left;
        } else {
            if (!is_truthy(left)) return left;
        }
        return evaluate(logicalExpr.right);
    }

    public object? visit_function_stmt(FunctionStmt stmt) {
        var fun = new LoxFunction(stmt, _env);
        _env.define(stmt.name.lexeme, fun);
        return null;
    }

    public void execute_block(List<Stmt> body, Env env) {
        var previous_env = _env;
        try {
            _env = env;
            foreach (Stmt statement in body) {
                evaluate(statement);
            }
        } finally {
            _env = previous_env;
        }
    }

    public object? visit_call_expr(CallExpr callExpr) {
        object? callee = evaluate(callExpr.callee);
        List<object?> arguments = [];
        foreach (Expr argument in callExpr.arguments) {
            arguments.Add(evaluate(argument));
        }
        if (callee is not LoxCallable) {
            throw new RuntimeErrorException(callExpr.paren, "Can only call functions and classes.");
        }
        LoxCallable function = (LoxCallable)callee;
        if (arguments.Count != function.arity()) {
            throw new RuntimeErrorException(callExpr.paren, $"Expected {function.arity()} arguments but got {arguments.Count}.");
        }
        return function.call(this, arguments);
    }

    public object? visit_return_stmt(ReturnStmt returnStmt) {
        object? value = null;
        if (returnStmt.value != null) {
            value = evaluate(returnStmt.value);
        }
        throw new ReturnException(value);
    }
}

class RuntimeErrorException(Token token, string message) : Exception(message) {
    public readonly Token token = token;
}

class ReturnException(object? value) : Exception {
    public readonly object? value = value;
}