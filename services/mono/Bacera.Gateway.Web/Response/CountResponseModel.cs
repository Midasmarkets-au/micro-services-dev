namespace Bacera.Gateway.Web.Response;

public class CountResponseModel
{
    public int Count { get; set; }
    public static CountResponseModel Of(int count)
        => new() { Count = count };
}