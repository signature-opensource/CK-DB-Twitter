using CK.Core;
using CK.SqlServer;

namespace CK.DB.User.UserTwitter.AccessToken;

/// <summary>
/// Package that adds AccessToken support to Twitter authentication.
/// </summary>
[SqlPackage( Schema = "CK", ResourcePath = "Res" )]
[Versions( "1.0.0" )]
[SqlObjectItem( "transform:CK.sUserTwitterUCL" )]
public abstract class Package : SqlPackage
{
    void StObjConstruct( UserTwitter.UserTwitterTable table )
    {
    }

    /// <summary>
    /// Sets whether the tokens storage is enabled for the user: the <see cref="UserTwitterInfoExtensions.NoAccessTokenMarker"/> is set
    /// in AccessToken and AccessTokenSecret.
    /// </summary>
    /// <param name="ctx">The context to use.</param>
    /// <param name="actorId">The acting actor identifier.</param>
    /// <param name="userId">The user identifier for which access token storage must be allowed or not.</param>
    /// <param name="allowTokenStorage">Whether access token storage must be allowed.</param>
    [SqlProcedure( "sUserTwitterAllowTokenStorage" )]
    public abstract void SetAllowTokenStorage( ISqlCallContext ctx, int actorId, int userId, bool allowTokenStorage );
}
