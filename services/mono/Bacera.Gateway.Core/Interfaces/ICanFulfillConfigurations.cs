namespace Bacera.Gateway.Interfaces;

public interface ICanFulfillConfigurations
{
    long Id { get; set; }

    List<Configuration> Configurations { get; set; }
}