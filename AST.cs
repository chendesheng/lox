abstract class Expr {
    abstract public R accept<R>(Visitor<R> visitor);
}

class AssignExpr(Token name, Expr value) : Expr {
    public readonly Token name = name;
    public readonly Expr value = value;

    public override R accept<R>(Visitor<R> visitor) {
        return visitor.visit_assign_expr(this);
    }
}

class BinaryExpr(Expr left, Token op, Expr right) : Expr {
    public readonly Expr left = left;
    public readonly Token op = op;
    public readonly Expr right = right;

    public override R accept<R>(Visitor<R> visitor) {
        return visitor.visit_binary_expr(this);
    }
}

class GroupingExpr(Expr expression) : Expr {
    public readonly Expr expression = expression;

    public override R accept<R>(Visitor<R> visitor) {
        return visitor.visit_grouping_expr(this);
    }
}

class LiteralExpr(object? value) : Expr {
    public readonly object? value = value;

    public override R accept<R>(Visitor<R> visitor) {
        return visitor.visit_literal_expr(this);
    }
}

class LogicalExpr(Expr left, Token op, Expr right) : Expr {
    public readonly Expr left = left;
    public readonly Token op = op;
    public readonly Expr right = right;

    public override R accept<R>(Visitor<R> visitor) {
        return visitor.visit_logical_expr(this);
    }
}

class UnaryExpr(Token op, Expr right) : Expr {
    public readonly Token op = op;
    public readonly Expr right = right;

    public override R accept<R>(Visitor<R> visitor) {
        return visitor.visit_unary_expr(this);
    }
}

class VariableExpr(Token name) : Expr {
    public readonly Token name = name;

    public override R accept<R>(Visitor<R> visitor) {
        return visitor.visit_variable_expr(this);
    }
}

abstract class Stmt {
    abstract public R accept<R>(Visitor<R> visitor);
}

class ExpressionStmt(Expr expression) : Stmt {
    public readonly Expr expression = expression;

    public override R accept<R>(Visitor<R> visitor) {
        return visitor.visit_expression_stmt(this);
    }
}

class BlockStmt(List<Stmt> statements) : Stmt {
    public readonly List<Stmt> statements = statements;

    public override R accept<R>(Visitor<R> visitor) {
        return visitor.visit_block_stmt(this);
    }
}

class ForStmt(Stmt? initializer, Expr? condition, Expr? increment, Stmt body) : Stmt {
    public readonly Stmt? initializer = initializer;
    public readonly Expr? condition = condition;
    public readonly Expr? increment = increment;
    public readonly Stmt body = body;

    public override R accept<R>(Visitor<R> visitor) {
        return visitor.visit_for_stmt(this);
    }
}

class IfStmt(Expr condition, Stmt then_branch, Stmt? else_branch) : Stmt {
    public readonly Expr condition = condition;
    public readonly Stmt then_branch = then_branch;
    public readonly Stmt? else_branch = else_branch;

    public override R accept<R>(Visitor<R> visitor) {
        return visitor.visit_if_stmt(this);
    }
}

class WhileStmt(Expr condition, Stmt body) : Stmt {
    public readonly Expr condition = condition;
    public readonly Stmt body = body;

    public override R accept<R>(Visitor<R> visitor) {
        return visitor.visit_while_stmt(this);
    }
}

class PrintStmt(Expr expression) : Stmt {
    public readonly Expr expression = expression;

    public override R accept<R>(Visitor<R> visitor) {
        return visitor.visit_print_stmt(this);
    }
}

class VarStmt(Token name, Expr? initializer) : Stmt {
    public readonly Token name = name;
    public readonly Expr? initializer = initializer;

    public override R accept<R>(Visitor<R> visitor) {
        return visitor.visit_var_stmt(this);
    }
}

class Program {
    public readonly List<Stmt> statements;

    public Program(List<Stmt> statements) {
        this.statements = statements;
    }

    public R accept<R>(Visitor<R> visitor) {
        return visitor.visit_program(this);
    }
}