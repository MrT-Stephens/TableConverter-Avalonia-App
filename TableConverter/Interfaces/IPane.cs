namespace TableConverter.Interfaces
{
    public interface IPane
    {
        public string Title { get; set; }

        public bool CanClose { get; set; }
    }

    public interface IPane<TViewModel> : IPane where TViewModel : IWorkspace
    {
    }
}
