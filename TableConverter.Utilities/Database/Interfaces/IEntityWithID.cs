namespace TableConverter.Utilities.Database.Interfaces;

public interface IEntityWithId<T>
{
    public T Id { get; set; }
}