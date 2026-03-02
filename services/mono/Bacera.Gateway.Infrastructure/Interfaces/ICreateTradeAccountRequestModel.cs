namespace Bacera.Gateway;

public interface ICreateTradeAccountRequestModel
{
    public string Name { get; set; }
    public string Group { get; set; }
    public int Leverage { get; set; }
    public string Password { get; set; }
}