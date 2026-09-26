using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace TableConverter.Interfaces;

/// <summary>
/// A document whose open state is remembered, so it can be reopened on the next start.
/// </summary>
/// <remarks>
/// The document type owns the shape of its own state, so a type that stores its data differently - in
/// another file format, in another database, or in no file at all - only has to describe that
/// difference here. Nothing else has to know how the data is kept.
/// </remarks>
public interface ISessionDocument : IPaneDocument
{
    /// <summary>
    /// The key this document's type is filed under in the session. Entries are grouped by key, so a
    /// document type never disturbs the documents of another.
    /// </summary>
    string DocumentType { get; }

    /// <summary>
    /// Captures whatever is needed to reopen this document on the next start. The captured JSON is
    /// stored as it is, so its shape belongs entirely to this document type.
    /// </summary>
    JsonElement CaptureSessionState();

    /// <summary>
    /// Reopens this document from state previously produced by <see cref="CaptureSessionState" />.
    /// </summary>
    /// <param name="state">The state this document's type captured.</param>
    /// <exception cref="Exception">
    /// Thrown when the document cannot be reopened. The caller skips the entry rather than failing the
    /// whole restore.
    /// </exception>
    Task RestoreSessionStateAsync(JsonElement state);
}

