﻿using System.Threading.Tasks;

namespace Plus.Core.Language;

public interface ILanguageManager
{
    string TryGetValue(string value);
    Task Reload();
}