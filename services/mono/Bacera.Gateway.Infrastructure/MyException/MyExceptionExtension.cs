using System.Text;

namespace Bacera.Gateway.MyException;

public static class MyExceptionExtension
{
    public static string GetFullMessage(this Exception e)
    {
        var current = e;
        var logStr = new StringBuilder();
        while (current != null)
        {
            logStr.AppendLine($"{current.Message} \n");
            current = current.InnerException;
        }

        return logStr.ToString();
    }
}