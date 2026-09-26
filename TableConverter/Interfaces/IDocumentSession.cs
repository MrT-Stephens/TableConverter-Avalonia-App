using System;
using System.Collections.Generic;
using System.Text.Json;

namespace TableConverter.Interfaces;

/// <summary>
/// One document that was open, tagged with the document type that wrote its state.
/// </summary>
/// <param name="DocumentType">
/// The key of the document type that wrote <paramref name="State" />. It keeps the state of different
/// document types apart inside the shared session.
/// </param>
/// <param name="State">
/// The state needed to reopen the document, as captured by its type. Its shape belongs to that
/// document type and is never interpreted by the session.
/// </param>
public sealed record DocumentSessionEntry(string DocumentType, JsonElement State);

/// <summary>
/// Remembers which documents were open, so they can be reopened on the next start.
/// </summary>
/// <remarks>
/// A document type decides for itself what it needs to store and how it reopens itself: the session
/// only files the state under the type's own key. Document types which store their data differently
/// are therefore independent of one another and share this one session file.
/// </remarks>
public interface IDocumentSession
{
    /// <summary>
    /// Reads the entries written by the previous run. An empty result means there is nothing to
    /// restore.
    /// </summary>
    IReadOnlyList<DocumentSessionEntry> Load();

    /// <summary>
    /// Replaces the entries stored for <paramref name="documentType" /> with <paramref name="states" />.
    /// The entries of every other document type are kept, so a workspace only ever rewrites its own
    /// documents.
    /// </summary>
    /// <param name="documentType">The key of the document type the states belong to.</param>
    /// <param name="states">The captured state of every open document of that type.</param>
    void Save(string documentType, IEnumerable<JsonElement> states);
}

