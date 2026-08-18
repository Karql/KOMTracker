using System;
using System.Collections.Generic;

namespace KomTracker.Application.Models;

/// <summary>A single page of results plus the total row count (for server-side paging).</summary>
public class PagedResultModel<T>
{
    public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();
    public int TotalCount { get; set; }
}
