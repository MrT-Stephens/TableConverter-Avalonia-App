using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace TableConverter.Utilities.Logging.Format;

internal ref partial struct ValueStringBuilder
{
	private char[]? _ArrayToReturnToPool;
	private Span<char> _Chars;
	private int _Pos;

	public ValueStringBuilder(Span<char> initialBuffer)
	{
		_ArrayToReturnToPool = null;
		_Chars = initialBuffer;
		_Pos = 0;
	}

	public ValueStringBuilder(int initialCapacity)
	{
		_ArrayToReturnToPool = ArrayPool<char>.Shared.Rent(initialCapacity);
		_Chars = _ArrayToReturnToPool;
		_Pos = 0;
	}

	public int Length
	{
		get => _Pos;
		set
		{
	#if DEBUG
			Debug.Assert(value >= 0);
			Debug.Assert(value <= _Chars.Length);
	#endif
			
			_Pos = value;
		}
	}

	public int Capacity => _Chars.Length;

	public void EnsureCapacity(int capacity)
	{
#if DEBUG
		// This is not expected to be called this with negative capacity
		Debug.Assert(capacity >= 0);
#endif

		// If the caller has a bug and calls this with negative capacity, make sure to call Grow to throw an exception.
		if ((uint)capacity > (uint)_Chars.Length)
			Grow(capacity - _Pos);
	}

	/// <summary>
	/// Get a pinnable reference to the builder.
	/// Does not ensure there is a null char after <see cref="Length"/>
	/// This overload is pattern matched in the C# 7.3+ compiler so you can omit
	/// the explicit method call, and write eg "fixed (char* c = builder)"
	/// </summary>
	public ref char GetPinnableReference()
	{
		return ref MemoryMarshal.GetReference(_Chars);
	}

	/// <summary>
	/// Get a pinnable reference to the builder.
	/// </summary>
	/// <param name="terminate">Ensures that the builder has a null char after <see cref="Length"/></param>
	public ref char GetPinnableReference(bool terminate)
	{
		if (terminate)
		{
			EnsureCapacity(Length + 1);
			_Chars[Length] = '\0';
		}

		return ref MemoryMarshal.GetReference(_Chars);
	}

	public ref char this[int index]
	{
		get
		{
			Debug.Assert(index < _Pos);
			return ref _Chars[index];
		}
	}

	public override string ToString()
	{
		string s = _Chars.Slice(0, _Pos).ToString();
		Dispose();
		return s;
	}

	/// <summary>Returns the underlying storage of the builder.</summary>
	public Span<char> RawChars => _Chars;

	/// <summary>Returns a span representing the remaining space available in the underlying storage of the builder.</summary>
	public Span<char> RemainingRawChars => _Chars.Slice(_Pos);

	/// <summary>
	/// Returns a span around the contents of the builder.
	/// </summary>
	/// <param name="terminate">Ensures that the builder has a null char after <see cref="Length"/></param>
	public ReadOnlySpan<char> AsSpan(bool terminate)
	{
		if (terminate)
		{
			EnsureCapacity(Length + 1);
			_Chars[Length] = '\0';
		}

		return _Chars.Slice(0, _Pos);
	}

	public ReadOnlySpan<char> AsSpan() => _Chars.Slice(0, _Pos);
	public ReadOnlySpan<char> AsSpan(int start) => _Chars.Slice(start, _Pos - start);
	public ReadOnlySpan<char> AsSpan(int start, int length) => _Chars.Slice(start, length);

	public bool TryCopyTo(Span<char> destination, out int charsWritten)
	{
		if (_Chars.Slice(0, _Pos).TryCopyTo(destination))
		{
			charsWritten = _Pos;
			Dispose();
			return true;
		}
		else
		{
			charsWritten = 0;
			Dispose();
			return false;
		}
	}

	public void Insert(int index, char value, int count)
	{
		if (_Pos > _Chars.Length - count)
		{
			Grow(count);
		}

		int remaining = _Pos - index;
		_Chars.Slice(index, remaining).CopyTo(_Chars.Slice(index + count));
		_Chars.Slice(index, count).Fill(value);
		_Pos += count;
	}

	public void Insert(int index, string? s)
	{
		if (s == null)
		{
			return;
		}

		int count = s.Length;

		if (_Pos > (_Chars.Length - count))
		{
			Grow(count);
		}

		int remaining = _Pos - index;
		_Chars.Slice(index, remaining).CopyTo(_Chars.Slice(index + count));
		s
#if !NET
				.AsSpan()
#endif
			.CopyTo(_Chars.Slice(index));
		_Pos += count;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Append(char c)
	{
		int pos = _Pos;
		Span<char> chars = _Chars;
		if ((uint)pos < (uint)chars.Length)
		{
			chars[pos] = c;
			_Pos = pos + 1;
		}
		else
		{
			GrowAndAppend(c);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Append(string? s)
	{
		if (s == null)
		{
			return;
		}

		int pos = _Pos;
		if (s.Length == 1 &&
		    (uint)pos < (uint)_Chars
			    .Length) // very common case, e.g. appending strings from NumberFormatInfo like separators, percent symbols, etc.
		{
			_Chars[pos] = s[0];
			_Pos = pos + 1;
		}
		else
		{
			AppendSlow(s);
		}
	}

	private void AppendSlow(string s)
	{
		int pos = _Pos;
		if (pos > _Chars.Length - s.Length)
		{
			Grow(s.Length);
		}

		s
#if !NET
				.AsSpan()
#endif
			.CopyTo(_Chars.Slice(pos));
		_Pos += s.Length;
	}

	public void Append(char c, int count)
	{
		if (_Pos > _Chars.Length - count)
		{
			Grow(count);
		}

		Span<char> dst = _Chars.Slice(_Pos, count);
		for (int i = 0; i < dst.Length; i++)
		{
			dst[i] = c;
		}

		_Pos += count;
	}

	public void Append(scoped ReadOnlySpan<char> value)
	{
		int pos = _Pos;
		if (pos > _Chars.Length - value.Length)
		{
			Grow(value.Length);
		}

		value.CopyTo(_Chars.Slice(_Pos));
		_Pos += value.Length;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Span<char> AppendSpan(int length)
	{
		int origPos = _Pos;
		if (origPos > _Chars.Length - length)
		{
			Grow(length);
		}

		_Pos = origPos + length;
		return _Chars.Slice(origPos, length);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void GrowAndAppend(char c)
	{
		Grow(1);
		Append(c);
	}

	/// <summary>
	/// Resize the internal buffer either by doubling current buffer size or
	/// by adding <paramref name="additionalCapacityBeyondPos"/> to
	/// <see cref="_Pos"/> whichever is greater.
	/// </summary>
	/// <param name="additionalCapacityBeyondPos">
	/// Number of chars requested beyond current position.
	/// </param>
	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Grow(int additionalCapacityBeyondPos)
	{
#if DEBUG
		Debug.Assert(additionalCapacityBeyondPos > 0);
		Debug.Assert(_Pos > _Chars.Length - additionalCapacityBeyondPos,
			"Grow called incorrectly, no resize is needed.");
#endif

		const uint arrayMaxLength = 0x7FFFFFC7; // same as Array.MaxLength

		// Increase to at least the required size (_pos + additionalCapacityBeyondPos), but try
		// to double the size if possible, bounding the doubling to not go beyond the max array length.
		int newCapacity = (int)Math.Max(
			(uint)(_Pos + additionalCapacityBeyondPos),
			Math.Min((uint)_Chars.Length * 2, arrayMaxLength));

		// Make sure to let Rent throw an exception if the caller has a bug and the desired capacity is negative.
		// This could also go negative if the actual required length wraps around.
		char[] poolArray = ArrayPool<char>.Shared.Rent(newCapacity);

		_Chars.Slice(0, _Pos).CopyTo(poolArray);

		char[]? toReturn = _ArrayToReturnToPool;
		_Chars = _ArrayToReturnToPool = poolArray;
		if (toReturn != null)
		{
			ArrayPool<char>.Shared.Return(toReturn);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Dispose()
	{
		char[]? toReturn = _ArrayToReturnToPool;
		this = default; // for safety, to avoid using pooled array if this instance is erroneously appended to again
		
		if (toReturn != null)
		{
			ArrayPool<char>.Shared.Return(toReturn);
		}
	}
}