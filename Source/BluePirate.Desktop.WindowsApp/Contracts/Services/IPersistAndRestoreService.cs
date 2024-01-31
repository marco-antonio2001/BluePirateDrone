namespace BluePirate.Desktop.WindowsApp.Contracts.Services;

public interface IPersistAndRestoreService
{
    void RestoreData();

    void PersistData();
}
