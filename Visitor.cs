
using System.Text;

interface Visitor<R> {
    R visit_binary_expr(BinaryExpr expr);
    R visit_grouping_expr(GroupingExpr expr);
    R visit_literal_expr(LiteralExpr expr);
    R visit_unary_expr(UnaryExpr expr);

    R visit_expression_stmt(ExpressionStmt expressionStmt);
    R visit_print_stmt(PrintStmt printStmt);
    R visit_variable_expr(VariableExpr variableExpr);
    R visit_var_stmt(VarStmt varStmt);
    R visit_assign_expr(AssignExpr assignExpr);
    R visit_program(Program program);
    R visit_logical_expr(LogicalExpr logicalExpr);
    R visit_if_stmt(IfStmt ifStmt);
    R visit_block_stmt(BlockStmt blockStmt);
    R visit_while_stmt(WhileStmt whileStmt);
    R visit_for_stmt(ForStmt forStmt);
    R visit_function_stmt(FunctionStmt functionStmt);
    R visit_call_expr(CallExpr callExpr);
    R visit_return_stmt(ReturnStmt returnStmt);
}

class AstPrinter : Visitor<StringBuilder> {
    public static string print(Expr expr) {
        AstPrinter printer = new();
        return expr.accept(printer).ToString();
    }

    public static string print(Program program) {
        AstPrinter printer = new();
        return program.accept(printer).ToString();
    }

    public StringBuilder visit_assign_expr(AssignExpr assignExpr) {
        StringBuilder builder = new();
        builder
            .Append(assignExpr.name.lexeme)
            .Append(" = ")
            .Append(assignExpr.value.accept(this));
        return builder;
    }

    public StringBuilder visit_binary_expr(BinaryExpr expr) {
        return parenthesize(expr.op.lexeme, expr.left, expr.right);
    }

    public StringBuilder visit_grouping_expr(GroupingExpr expr) {
        return parenthesize("group", expr.expression);
    }

    public StringBuilder visit_literal_expr(LiteralExpr expr) {
        if (expr.value == null) return new StringBuilder("nil");
        return new StringBuilder(expr.value.ToString());
    }

    public StringBuilder visit_unary_expr(UnaryExpr expr) {
        return parenthesize(expr.op.lexeme, expr.right);
    }

    private StringBuilder parenthesize(string name, params Expr[] exprs) {
        StringBuilder builder = new();
        builder.Append('(').Append(name);
        foreach (Expr expr in exprs) {
            builder.Append(' ');
            builder.Append(expr.accept(this));
        }
        builder.Append(')');
        return builder;
    }

    public StringBuilder visit_variable_expr(VariableExpr variableExpr) {
        return new StringBuilder(variableExpr.name.lexeme);
    }

    public StringBuilder visit_expression_stmt(ExpressionStmt expressionStmt) {
        StringBuilder builder = expressionStmt.expression.accept(this);
        builder.Append(';').AppendLine();
        return builder;
    }

    public StringBuilder visit_print_stmt(PrintStmt printStmt) {
        StringBuilder builder = new();
        builder
            .Append("print ")
            .Append(printStmt.expression.accept(this))
            .Append(';')
            .AppendLine();
        return builder;
    }

    public StringBuilder visit_var_stmt(VarStmt varStmt) {
        StringBuilder builder = new();
        builder
            .Append("var ")
            .Append(varStmt.name.lexeme);
        if (varStmt.initializer != null) {
            builder
                .Append(" = ")
                .Append(varStmt.initializer.accept(this));
        }
        builder.Append(';').AppendLine();
        return builder;
    }

    public StringBuilder visit_program(Program program) {
        StringBuilder builder = new();
        foreach (Stmt stmt in program.statements) {
            builder.Append(stmt.accept(this));
        }
        return builder;
    }

    public StringBuilder visit_if_stmt(IfStmt ifStmt) {
        StringBuilder builder = new();
        builder.Append("if (")
            .Append(ifStmt.condition.accept(this))
            .Append(") ")
            .AppendLine()
            .Append('\t')
            .Append(ifStmt.then_branch.accept(this))
            .AppendLine();
        if (ifStmt.else_branch != null) {
            builder.AppendLine(" else ")
                .Append(ifStmt.else_branch.accept(this))
                .AppendLine();
        }
        return builder;
    }

    public StringBuilder visit_block_stmt(BlockStmt blockStmt) {
        StringBuilder builder = new();
        builder.AppendLine("{");
        foreach (Stmt stmt in blockStmt.statements) {
            builder.Append('\t').Append(stmt.accept(this));
        }
        builder.AppendLine("}");
        return builder;
    }

    public StringBuilder visit_while_stmt(WhileStmt whileStmt) {
        StringBuilder builder = new();
        builder.Append("while (")
            .Append(whileStmt.condition.accept(this))
            .Append(") {")
            .Append(whileStmt.body.accept(this))
            .AppendLine("}");
        return builder;
    }

    public StringBuilder visit_for_stmt(ForStmt forStmt) {
        StringBuilder builder = new();
        builder.Append("for (")
            .Append(forStmt.initializer?.accept(this) ?? new())
            .Append("; ")
            .Append(forStmt.condition?.accept(this) ?? new())
            .Append("; ")
            .Append(forStmt.increment?.accept(this) ?? new())
            .Append(") {")
            .Append(forStmt.body.accept(this))
            .AppendLine("}");
        return builder;
    }

    public StringBuilder visit_logical_expr(LogicalExpr logicalExpr) {
        return parenthesize(logicalExpr.op.lexeme, logicalExpr.left, logicalExpr.right);
    }

    public StringBuilder visit_function_stmt(FunctionStmt functionStmt) {
        StringBuilder builder = new();
        builder.Append("fun ")
            .Append(functionStmt.name.lexeme)
            .Append('(');
        foreach (Token param in functionStmt.parameters) {
            builder.Append(param.lexeme).Append(", ");
        }
        if (functionStmt.parameters.Count > 0) {
            builder.Length -= 2;
        }
        builder.Append(") {");
        foreach (Stmt stmt in functionStmt.body) {
            builder.Append(stmt.accept(this));
        }
        builder.AppendLine("}");
        return builder;
    }

    public StringBuilder visit_call_expr(CallExpr callExpr) {
        StringBuilder builder = new();
        builder.Append(callExpr.callee.accept(this))
            .Append('(');
        foreach (Expr arg in callExpr.arguments) {
            builder.Append(arg.accept(this)).Append(", ");
        }
        if (callExpr.arguments.Count > 0) {
            builder.Length -= 2;
        }
        builder.Append(')');
        return builder;
    }

    public StringBuilder visit_return_stmt(ReturnStmt returnStmt) {
        StringBuilder builder = new();
        builder.Append("return");
        if (returnStmt.value != null) {
            builder.Append(' ');
            builder.Append(returnStmt.value.accept(this));
        }
        builder.AppendLine(";");
        return builder;
    }
}