using CK.Core;

namespace CK.DB.User.UserTwitter;

/// <summary>
/// Holds information stored for a Twitter user.
/// </summary>
public interface IUserTwitterInfo : IPoco
{
    /// <summary>
    /// Gets or sets the Twitter account identifier.
    /// </summary>
    string TwitterAccountId { get; set; }
}
