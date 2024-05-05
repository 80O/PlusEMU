using Plus.Utilities.FigureData.Types;
using System;
using System.Collections.Concurrent;

namespace Plus.Utilities.FigureData;
public interface IFigureDataUtility
{
    FigureData FigureData { get; set; }
    Task<FigureData> InitFromJsonAsync();
    Task UpdateFromJsonAsync();
    Dictionary<string, SetType> IndexedSetTypes { get; set; }
    Dictionary<int, Palette> IndexedPalettes { get; set; }
}