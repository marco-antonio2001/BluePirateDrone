namespace BluePirate.Desktop.WindowsApp.Contracts.Activation;

public interface IActivationHandler
{
    bool CanHandle();

    Task HandleAsync();
}
