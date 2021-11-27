using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KomTracker.Infrastructure.Identity.Validation;

/// <summary>
/// Based on: StrictRedirectUriValidator 
/// </summary>
public class StartWithRedirectUriValidator : IRedirectUriValidator
{
    /// <summary>
    /// Checks if any string in collection start with a given URI (using ordinal ignore case comparison)
    /// </summary>
    /// <param name="uris">The uris.</param>
    /// <param name="requestedUri">The requested URI.</param>
    /// <returns></returns>
    protected bool AnyItemInCollectionStartWithString(IEnumerable<string> uris, string requestedUri)
    {
        if (uris.IsNullOrEmpty()) return false;

        return uris.Any(x => requestedUri.StartsWith(x, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Determines whether a redirect URI is valid for a client.
    /// </summary>
    /// <param name="requestedUri">The requested URI.</param>
    /// <param name="client">The client.</param>
    /// <returns>
    ///   <c>true</c> is the URI is valid; <c>false</c> otherwise.
    /// </returns>
    public virtual Task<bool> IsRedirectUriValidAsync(string requestedUri, Client client)
    {
        return Task.FromResult(AnyItemInCollectionStartWithString(client.RedirectUris, requestedUri));
    }

    /// <summary>
    /// Determines whether a post logout URI is valid for a client.
    /// </summary>
    /// <param name="requestedUri">The requested URI.</param>
    /// <param name="client">The client.</param>
    /// <returns>
    ///   <c>true</c> is the URI is valid; <c>false</c> otherwise.
    /// </returns>
    public virtual Task<bool> IsPostLogoutRedirectUriValidAsync(string requestedUri, Client client)
    {
        return Task.FromResult(AnyItemInCollectionStartWithString(client.PostLogoutRedirectUris, requestedUri));
    }
}
