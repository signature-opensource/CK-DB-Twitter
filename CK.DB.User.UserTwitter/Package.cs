using CK.Core;

namespace CK.DB.User.UserTwitter
{
    /// <summary>
    /// Package that adds Twitter authentication support for users. 
    /// </summary>
    [SqlPackage( Schema = "CK", ResourcePath = "Res" )]
    [Versions("1.0.0")]
    [SqlObjectItem( "transform:vUserAuthProvider" )]
    public class Package : SqlPackage
    {
        void StObjConstruct( Actor.Package actorPackage, Auth.Package authPackage )
        {
        }

        /// <summary>
        /// Gets the user Twitter table.
        /// </summary>
        [InjectObject]
        public UserTwitterTable UserTwitterTable { get; protected set; }

    }
}
