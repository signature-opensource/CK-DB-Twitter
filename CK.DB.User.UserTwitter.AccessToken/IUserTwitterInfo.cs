
using System;

namespace CK.DB.User.UserTwitter.AccessToken;

/// <summary>
/// Extends <see cref="UserTwitter.IUserTwitterInfo"/> with tokens storage.
/// </summary>
public interface IUserTwitterInfo : UserTwitter.IUserTwitterInfo
{
    /// <summary>
    /// Gets or sets the last time the tokens have been updated.
    /// This is meaningful only when reading from the database and is ignored when writing.
    /// </summary>
    DateTime LastWriteTime { get; set; }

    /// <summary>
    /// Gets or sets the access token.
    /// </summary>
    string? AccessToken { get; set; }

    /// <summary>
    /// Gets or sets the access token secret.
    /// </summary>
    string? AccessTokenSecret { get; set; }
}

/// <summary>
/// Extends <see cref="IUserTwitterInfo"/>.
/// </summary>
public static class UserTwitterInfoExtensions
{
    /// <summary>
    /// The string "<NoAccessToken>" is used as a marker in AccessToken and AccessTokenSecret to
    /// disallow the token storage for the user.
    /// </summary>
    public readonly static string NoAccessTokenMarker = "<NoStorage>";

    /// <summary>
    /// Sets whether the tokens storage is enabled for the user:
    /// the <see cref="NoAccessTokenMarker"/> is set in <see cref="IUserTwitterInfo.AccessToken"/> and <see cref="IUserTwitterInfo.AccessTokenSecret"/>.
    /// </summary>
    /// <param name="this">This twitter info.</param>
    /// <param name="allow">Whether token storage is allowed or not.</param>
    public static void SetAllowAccessToken( this IUserTwitterInfo @this, bool allow )
    {
        if( allow )
        {
            if( !AllowAccessToken( @this ) )
            {
                @this.AccessToken = @this.AccessTokenSecret = String.Empty;
            }
        }
        else
        {
            @this.AccessToken = @this.AccessTokenSecret = NoAccessTokenMarker;
        }
    }

    /// <summary>
    /// Gets whether the tokens storage is enabled for the user.
    /// </summary>
    /// <param name="this">This twitter info.</param>
    /// <returns>True if the tokens can be stored.</returns>
    public static bool AllowAccessToken( this IUserTwitterInfo @this )
    {
        return @this.AccessToken != NoAccessTokenMarker;
    }

}
