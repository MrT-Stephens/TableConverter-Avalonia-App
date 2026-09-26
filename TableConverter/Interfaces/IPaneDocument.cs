using System;

namespace TableConverter.Interfaces
{
    public interface IPaneDocument : IPane
    {
        public Guid ID { get; }

        /// <summary>
        /// Whether the document can be closed. Documents whose changes are written straight to their
        /// backing store are always closable, because there is never unsaved state to lose.
        /// </summary>
        public bool CanClose { get; }
    }
}
