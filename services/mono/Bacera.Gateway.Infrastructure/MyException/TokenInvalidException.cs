namespace Bacera.Gateway.MyException;

public class TokenInvalidException(string message = "") : Exception(message);