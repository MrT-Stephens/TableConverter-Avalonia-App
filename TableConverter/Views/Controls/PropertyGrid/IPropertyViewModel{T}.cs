namespace TableConverter.Views.Controls.PropertyGrid
{
    public interface IPropertyViewModel<T> : IPropertyViewModel
    {
        new T Value { get; set; }
    }
}