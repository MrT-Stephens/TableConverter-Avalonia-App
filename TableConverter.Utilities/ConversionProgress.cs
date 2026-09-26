namespace TableConverter.Utilities;

/// <summary>
///     Reports how far a conversion has got.
/// </summary>
/// <remarks>
///     <para>
///         The unit is whatever the reporter is counting: an importer that reads a file as it arrives
///         counts bytes, an exporter that pulls rows out of a store counts rows. A consumer only reads
///         <see cref="Processed" /> against <see cref="Total" />, so it never has to know which it is
///         being told about.
///     </para>
///     <para>
///         <see cref="Total" /> is <see langword="null" /> while the extent of the job is not known,
///         which is the normal state for an import: how many rows a file holds is only known once it
///         has been read to the end. A consumer shows an indeterminate indicator until it is told
///         otherwise, and a determinate one from then on.
///     </para>
/// </remarks>
/// <param name="Processed">The number of units handled so far.</param>
/// <param name="Total">The number of units in the whole job, or <see langword="null" /> if it is not known.</param>
public readonly record struct ConversionProgress(long Processed, long? Total)
{
    /// <summary>
    ///     Gets a value indicating whether the extent of the job is known, and so whether the progress
    ///     can be shown as a proportion rather than as an indeterminate indicator.
    /// </summary>
    public bool IsDeterminate => Total is > 0;

    /// <summary>
    ///     Gets how much of the job is done, between 0 and 1, or <see langword="null" /> when
    ///     <see cref="Total" /> is not known.
    /// </summary>
    /// <remarks>
    ///     Clamped, because a reporter that counts in a different unit from the one it totalled (or that
    ///     overshoots on the last report) must not hand a consumer a proportion it cannot draw.
    /// </remarks>
    public double? Fraction => Total is > 0 ? Math.Clamp((double)Processed / Total.Value, 0d, 1d) : null;

    /// <summary>
    ///     Gets how much of the job is done as a percentage, or <see langword="null" /> when
    ///     <see cref="Total" /> is not known.
    /// </summary>
    public double? Percent => Fraction * 100d;
}

