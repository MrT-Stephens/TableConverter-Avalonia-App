namespace TableConverter.Utilities.Interfaces;

public interface ITransaction : IDisposable
{
    public void Commit();
}