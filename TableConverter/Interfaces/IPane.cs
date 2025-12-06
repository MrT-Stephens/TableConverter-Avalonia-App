namespace TableConverter.Interfaces
{
    public interface IPane
    {
        public string Title { get; set; }
        
        public bool IsEnabled { get; set; }
        
        public object Workspace { get; set; }
        
        public void OnActivate();
        
        public void OnDeactivate();
    }
}
