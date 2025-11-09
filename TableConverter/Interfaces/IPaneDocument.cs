namespace TableConverter.Interfaces
{
    public interface IPaneDocument : IPane
    {
        public bool IsDirty { get; set; }

        public bool CanClose { get; }
    }
}
