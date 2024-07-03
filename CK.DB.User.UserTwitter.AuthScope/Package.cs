using CK.DB.Auth.AuthScope;
using CK.Setup;
using CK.SqlServer;
using CK.Core;
using System;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;

namespace CK.DB.User.UserTwitter.AuthScope
{
    /// <summary>
    /// Package that adds AuthScope support to Twitter authentication. 
    /// </summary>
    [SqlPackage( Schema = "CK", ResourcePath = "Res" )]
    [Versions("1.0.0")]
    [SqlObjectItem( "transform:sUserTwitterUCL, transform:sUserTwitterDestroy" )]
    public class Package : SqlPackage
    {
        [AllowNull]
        AuthScopeSetTable _scopeSetTable;
        [AllowNull]
        UserTwitterTable _twitterTable;

        void StObjConstruct( AuthScopeSetTable scopeSetTable, UserTwitterTable TwitterTable )
        {
            _scopeSetTable = scopeSetTable;
            _twitterTable = TwitterTable;
        }

        /// <summary>
        /// Gets the <see cref="UserTwitterTable"/>.
        /// </summary>
        public UserTwitterTable UserTwitterTable => _twitterTable;

        /// <summary>
        /// Gets the <see cref="AuthScopeSetTable"/>.
        /// </summary>
        public AuthScopeSetTable AuthScopeSetTable => _scopeSetTable;

        /// <summary>
        /// Reads the <see cref="AuthScopeSet"/> of a user.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns>The scope set or null if the user is not a Twitter user.</returns>
        public Task<AuthScopeSet> ReadScopeSetAsync( ISqlCallContext ctx, int userId )
        {
            Throw.CheckArgument( userId > 0 );
            var cmd = _scopeSetTable.CreateReadCommand( $"select ScopeSetId from CK.tUserTwitter where UserId = {userId}" );
            return _scopeSetTable.RawReadAuthScopeSetAsync( ctx, cmd );
        }

        /// <summary>
        /// Reads the default <see cref="AuthScopeSet"/> that is the template for new users.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <returns>The default scope set.</returns>
        public Task<AuthScopeSet> ReadDefaultScopeSetAsync( ISqlCallContext ctx )
        {
            var cmd = _scopeSetTable.CreateReadCommand( "select ScopeSetId from CK.tUserTwitter where UserId = 0" );
            return _scopeSetTable.RawReadAuthScopeSetAsync( ctx, cmd );
        }

    }
}
