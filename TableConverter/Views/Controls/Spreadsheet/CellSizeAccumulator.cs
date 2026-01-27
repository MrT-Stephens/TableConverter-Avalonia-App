using System;
using System.Collections.Generic;
using System.Linq;

namespace TableConverter.Views.Controls.Spreadsheet;

/*
Why implement this? Because the height and width of the cell are additive due to layout, the original implementation:
        // Calculate the cumulative height to the top of the specified row
        for (int i = 0; i < rowId; i++){
            position += SheetData.GetRowHeightInUI((uint)i);
        }
This kind of calculation exists all the time due to real-time rendering, which is very unfriendly, so space is exchanged for time:
Cell size cumulative calculator (unified processing of row height/column width)
Features:
1. Initialize and build prefix and array (O(n))
2. Record the difference when modifying (O(1))
3. Dynamically merge differences during query (O(k), k=number of unmerged modifications)
4. Automatically rebuild the cache after the modifications accumulate to the threshold (O(n))

The calculation is completed in one go during initialization. The notebook CPU takes about 17ms (1048576), one frame.
Single modification interface: word fine-tuning only requires simple recording. If it accumulates to a certain number of times, it will be recalculated.
Query location

| Scenario | Traditional Plan | This Plan |
|---------------------|-----------------------|----------------------|
| **Initialization** | No preprocessing | O(n) build prefix sum |
| **Single row height modification** | Full recalculation (O(n)) | Record difference (O(1)) |
| **Frequent position calculation** | O(n) accumulation each time | O(k) in most cases (k=number of modified rows) |
| **Memory usage** | No additional overhead | Additional O(n)+O(k) space |

Core advantage: amortize the cost of row height changes from O(n) for each modification to O(k) for multiple queries
 */

public sealed class CellSizeAccumulator
{
    private readonly Func<int, double> _getSizeFunc;
    private double[] _PrefixSum; 
    private readonly Dictionary<int, double> _sizeDeltas = new(); 
    private bool _RequiresRebuild;
    private const int RebuildThreshold = 50; 

    public CellSizeAccumulator(Func<int, double> getSizeFunc)
    {
        _getSizeFunc = getSizeFunc ?? throw new ArgumentNullException(nameof(getSizeFunc));
    }

    public void Initialize(int totalCount)
    {
        _PrefixSum = new double[totalCount];
        double sum = 0;
        for (int i = 0; i < totalCount; i++){
            sum += _getSizeFunc(i);
            _PrefixSum[i] = sum;
        }

        _sizeDeltas.Clear();
        _RequiresRebuild = false;
    }

    public void UpdateSize(int index, double newSize)
    {
        double delta = newSize - _getSizeFunc(index);
        _sizeDeltas[index] = _sizeDeltas.TryGetValue(index, out var existing) ? existing + delta : delta;
        _RequiresRebuild = _sizeDeltas.Count >= RebuildThreshold;
    }

    public double GetAccumulated(int endIndex)
    {
        if (endIndex < 0) return 0;
        if (_RequiresRebuild) RebuildCache();

        double baseSum = endIndex == 0 ? 0 : _PrefixSum[endIndex - 1];

        foreach (var kv in _sizeDeltas.Where(x => x.Key < endIndex)){
            baseSum += kv.Value;
        }

        return baseSum;
    }

    private void RebuildCache()
    {
        foreach (var (index, delta) in _sizeDeltas.OrderBy(x => x.Key)){
            for (int i = index; i < _PrefixSum.Length; i++){
                _PrefixSum[i] += delta;
            }
        }

        _sizeDeltas.Clear();
        _RequiresRebuild = false;
    }
}