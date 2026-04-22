namespace RPCMAS.Core.Exceptions;

public class BusinessRuleException : Exception
{
    public string Code { get; }

    public BusinessRuleException(string code, string message) : base(message)
    {
        Code = code;
    }
}
