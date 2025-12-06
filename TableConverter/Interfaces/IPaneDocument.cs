using System;

namespace TableConverter.Interfaces
{
    public interface IPaneDocument : IPane
    {
        public Guid ID { get; }
        
        public bool IsDirty { get; set; }

        public bool CanClose { get; }
        
        public bool CanUndo { get; }
        
        public bool CanRedo { get; }
    }
}
