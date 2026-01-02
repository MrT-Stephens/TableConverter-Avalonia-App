using System;

namespace TableConverter.Interfaces
{
    public interface IPaneDocument : IPane
    {
        public Guid ID { get; }
        
        public bool IsDirty { get; set; }

        public bool CanClose { get; }
    }
}
