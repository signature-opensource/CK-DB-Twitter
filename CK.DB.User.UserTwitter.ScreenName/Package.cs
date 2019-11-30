using CK.Core;

namespace CK.DB.User.UserTwitter.ScreenName
{
    /// <summary>
    /// Package that adds ScreeName column support to Twitter authentication.
    /// </summary>
    [SqlPackage( Schema = "CK", ResourcePath = "Res" )]
    [Versions("1.0.0")]
    [SqlObjectItem( "transform:CK.sUserTwitterUCL" )]
    public abstract class Package : SqlPackage
    {
        void StObjConstruct( UserTwitter.UserTwitterTable table )
        {
        }
    }
}
