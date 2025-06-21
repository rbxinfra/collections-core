namespace Roblox.Collections;

/// <summary>
/// Represents the policy for an expirable collection.
/// </summary>
public enum ExpirationPolicy
{
    /// <summary>
    /// Never renew the collection.
    /// </summary>
    NeverRenew,

    /// <summary>
    /// Renew on each read.
    /// </summary>
    RenewOnRead
}
