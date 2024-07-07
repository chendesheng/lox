
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
}