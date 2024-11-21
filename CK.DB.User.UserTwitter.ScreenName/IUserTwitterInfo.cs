
namespace CK.DB.User.UserTwitter.ScreenName;

/// <summary>
/// Extends <see cref="UserTwitter.IUserTwitterInfo"/> with the screen name.
/// </summary>
public interface IUserTwitterInfo : UserTwitter.IUserTwitterInfo
{
    /// <summary>
    /// Gets or sets the scren name.
    /// Note that since 2017, this can be up to 50 characters.
    /// </summary>
    string? ScreenName { get; set; }

}
