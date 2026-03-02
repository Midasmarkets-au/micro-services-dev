namespace Bacera.Gateway.Interfaces;

public interface IRecaptchaService
{
    Task<bool> CreateAssessment(string token, string recaptchaAction);
}