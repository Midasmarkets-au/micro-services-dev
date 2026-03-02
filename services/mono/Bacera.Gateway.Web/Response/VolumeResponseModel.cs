namespace Bacera.Gateway.Web.Response;

public class VolumeResponseModel
{
    public double Volume { get; set; }
    public static VolumeResponseModel Of(double volume) => new() { Volume = volume };
}