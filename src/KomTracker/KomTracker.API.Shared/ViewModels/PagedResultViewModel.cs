using System;

namespace KomTracker.API.Shared.ViewModels;

/// <summary>A page of items plus the total row count (server-side paging).</summary>
public class PagedResultViewModel<T>
{
    public T[] Items { get; set; } = Array.Empty<T>();
    public int TotalCount { get; set; }
}
