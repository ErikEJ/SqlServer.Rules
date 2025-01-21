using System;
using System.Collections.Generic;

namespace SqlServer.Rules.Tests.Utils;

/// <summary>
/// Utility class for tracking and disposing of objects that implement IDisposable.
///
/// Original Source: https://github.com/microsoft/DACExtensions/blob/master/Samples/DisposableList.cs
/// </summary>
public sealed class DisposableList : List<IDisposable>, IDisposable
{
    /// <summary>
    /// Disposes of all elements of list.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
    }

    /// <summary>
    /// Internal implementation of Dispose logic.
    /// </summary>
    private void Dispose(bool isDisposing)
    {
        foreach (var disposable in this)
        {
            disposable.Dispose();
        }
    }

    public T Add<T>(T item)
        where T : IDisposable
    {
        base.Add(item);

        return item;
    }
}