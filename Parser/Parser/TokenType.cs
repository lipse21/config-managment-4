namespace Parser
{
    public enum TokenType
    {
        EOF,
        Error,
        Var,
        Assign,
        DictStart,
        DictEnd,
        Semicolon,
        Dollar,
        Comma,
        Plus,
        Minus,
        Multiply,
        MinFunc,
        Number,
        HexNumber,
        String,
        Identifier,
        Word,
        ParenOpen,
        ParenClose,
        Equals
    }
}