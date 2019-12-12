using CK.SqlServer;
using CK.Core;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using CK.DB.Auth;
using CK.Text;

namespace CK.DB.User.UserTwitter
{
    /// <summary>
    /// Twitter authentication provider.
    /// </summary>
    [SqlTable( "tUserTwitter", Package = typeof( Package ) )]
    [Versions( "2.0.1" )]
    [SqlObjectItem( "transform:sUserDestroy" )]
    public abstract partial class UserTwitterTable : SqlTable, IGenericAuthenticationProvider<IUserTwitterInfo>
    {
        IPocoFactory<IUserTwitterInfo> _infoFactory;

        /// <summary>
        /// Gets "Twitter" that is the name of the Twitter provider.
        /// </summary>
        public string ProviderName => "Twitter";

        void StObjConstruct( IPocoFactory<IUserTwitterInfo> infoFactory )
        {
            _infoFactory = infoFactory;
        }

        IUserTwitterInfo IGenericAuthenticationProvider<IUserTwitterInfo>.CreatePayload() => _infoFactory.Create();

        /// <summary>
        /// Creates a <see cref="IUserTwitterInfo"/> poco.
        /// </summary>
        /// <returns>A new instance.</returns>
        public T CreateUserInfo<T>() where T : IUserTwitterInfo => (T)_infoFactory.Create();

        /// <summary>
        /// Creates or updates a user entry for this provider. 
        /// This is the "binding account" feature since it binds an external identity to 
        /// an already existing user that may already be registered into other authencation providers.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="userId">The user identifier that must be registered.</param>
        /// <param name="info">Provider specific data: the <see cref="IUserTwitterInfo"/> poco.</param>
        /// <param name="mode">Optionnaly configures Create, Update only or WithLogin behavior.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The result.</returns>
        public async Task<UCLResult> CreateOrUpdateTwitterUserAsync( ISqlCallContext ctx, int actorId, int userId, IUserTwitterInfo info, UCLMode mode = UCLMode.CreateOrUpdate, CancellationToken cancellationToken = default( CancellationToken ) )
        {
            var r = await TwitterUserUCLAsync( ctx, actorId, userId, info, mode, cancellationToken ).ConfigureAwait( false );
            return r;
        }

        /// <summary>
        /// Challenges <see cref="IUserTwitterInfo"/> data to identify a user.
        /// Note that a successful challenge may have side effects such as updating claims, access tokens or other data
        /// related to the user and this provider.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="info">The payload to challenge.</param>
        /// <param name="actualLogin">Set it to false to avoid login side-effect (such as updating the LastLoginTime) on success.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The <see cref="LoginResult"/>.</returns>
        public async Task<LoginResult> LoginUserAsync( ISqlCallContext ctx, IUserTwitterInfo info, bool actualLogin = true, CancellationToken cancellationToken = default( CancellationToken ) )
        {
            var mode = actualLogin
                        ? UCLMode.UpdateOnly | UCLMode.WithActualLogin
                        : UCLMode.UpdateOnly | UCLMode.WithCheckLogin;
            var r = await TwitterUserUCLAsync( ctx, 1, 0, info, mode, cancellationToken ).ConfigureAwait( false );
            return r.LoginResult;
        }

        /// <summary>
        /// Destroys a TwitterUser for a user.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="userId">The user identifier for which Twitter account information must be destroyed.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The awaitable.</returns>
        [SqlProcedure( "sUserTwitterDestroy" )]
        public abstract Task DestroyTwitterUserAsync( ISqlCallContext ctx, int actorId, int userId, CancellationToken cancellationToken = default( CancellationToken ) );

        /// <summary>
        /// Raw call to manage TwitterUser. Since this should not be used directly, it is protected.
        /// Actual implementation of the centralized update, create or login procedure.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="userId">The user identifier for which a Twitter account must be created or updated.</param>
        /// <param name="info">User information to create or update.</param>
        /// <param name="mode">Configures Create, Update only or WithCheck/ActualLogin behavior.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The result.</returns>
        [SqlProcedure( "sUserTwitterUCL" )]
        protected abstract Task<UCLResult> TwitterUserUCLAsync(
            ISqlCallContext ctx,
            int actorId,
            int userId,
            [ParameterSource]IUserTwitterInfo info,
            UCLMode mode,
            CancellationToken cancellationToken );

        /// <summary>
        /// Finds a user by its Twitter account identifier.
        /// Returns null if no such user exists.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="twitterAccountId">The Twitter account identifier.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>A <see cref="IdentifiedUserInfo{T}"/> object or null if not found.</returns>
        public Task<IdentifiedUserInfo<IUserTwitterInfo>> FindKnownUserInfoAsync( ISqlCallContext ctx, string twitterAccountId, CancellationToken cancellationToken = default( CancellationToken ) )
        {
            using( var c = CreateReaderCommand( twitterAccountId ) )
            {
                return ctx[Database].ExecuteSingleRowAsync( c, r => r == null
                                                                    ? null
                                                                    : DoCreateUserUnfo( twitterAccountId, r ) );
            }
        }

        /// <summary>
        /// Creates a the reader command parametrized with the Twitter account identifier.
        /// Single-row returned columns are defined by <see cref="AppendUserInfoColumns(StringBuilder)"/>.
        /// </summary>
        /// <param name="twitterAccountId">Twitter account identifier to look for.</param>
        /// <returns>A ready to use reader command.</returns>
        SqlCommand CreateReaderCommand( string twitterAccountId )
        {
            StringBuilder b = new StringBuilder( "select " );
            AppendUserInfoColumns( b ).Append( " from CK.tUserTwitter where TwitterAccountId=@A" );
            var c = new SqlCommand( b.ToString() );
            c.Parameters.Add( new SqlParameter( "@A", twitterAccountId ) );
            return c;
        }

        IdentifiedUserInfo<IUserTwitterInfo> DoCreateUserUnfo( string TwitterAccountId, SqlDataRow r )
        {
            var info = _infoFactory.Create();
            info.TwitterAccountId = TwitterAccountId;
            FillUserTwitterInfo( info, r, 1 );
            return new IdentifiedUserInfo<IUserTwitterInfo>( r.GetInt32( 0 ), info );
        }

        /// <summary>
        /// Adds the columns name to read.
        /// </summary>
        /// <param name="b">The string builder.</param>
        /// <returns>The string builder.</returns>
        protected virtual StringBuilder AppendUserInfoColumns( StringBuilder b )
        {
            var props = _infoFactory.PocoClassType.GetProperties().Where( p => p.Name != nameof( IUserTwitterInfo.TwitterAccountId ) );
            return props.Any() ? b.Append( "UserId, " ).AppendStrings( props.Select( p => p.Name ) ) : b.Append( "UserId " );
        }

        /// <summary>
        /// Fill UserInfo properties from reader.
        /// </summary>
        /// <param name="info">The info to fill.</param>
        /// <param name="r">The record.</param>
        /// <param name="idx">The index of the first column.</param>
        /// <returns>The updated index.</returns>
        protected virtual int FillUserTwitterInfo( IUserTwitterInfo info, SqlDataRow r, int idx )
        {
            var props = _infoFactory.PocoClassType.GetProperties().Where( p => p.Name != nameof( IUserTwitterInfo.TwitterAccountId ) );
            foreach( var p in props )
            {
                p.SetValue( info, r.GetValue( idx++ ) );
            }
            return idx;
        }

        #region IGenericAuthenticationProvider explicit implementation.

        UCLResult IGenericAuthenticationProvider.CreateOrUpdateUser( ISqlCallContext ctx, int actorId, int userId, object payload, UCLMode mode )
        {
            IUserTwitterInfo info = _infoFactory.ExtractPayload( payload );
            return CreateOrUpdateTwitterUser( ctx, actorId, userId, info, mode );
        }

        LoginResult IGenericAuthenticationProvider.LoginUser( ISqlCallContext ctx, object payload, bool actualLogin )
        {
            IUserTwitterInfo info = _infoFactory.ExtractPayload( payload );
            return LoginUser( ctx, info, actualLogin );
        }

        Task<UCLResult> IGenericAuthenticationProvider.CreateOrUpdateUserAsync( ISqlCallContext ctx, int actorId, int userId, object payload, UCLMode mode, CancellationToken cancellationToken )
        {
            IUserTwitterInfo info = _infoFactory.ExtractPayload( payload );
            return CreateOrUpdateTwitterUserAsync( ctx, actorId, userId, info, mode, cancellationToken );
        }

        Task<LoginResult> IGenericAuthenticationProvider.LoginUserAsync( ISqlCallContext ctx, object payload, bool actualLogin, CancellationToken cancellationToken )
        {
            IUserTwitterInfo info = _infoFactory.ExtractPayload( payload );
            return LoginUserAsync( ctx, info, actualLogin, cancellationToken );
        }

        void IGenericAuthenticationProvider.DestroyUser( ISqlCallContext ctx, int actorId, int userId, string schemeSuffix )
        {
            DestroyTwitterUser( ctx, actorId, userId );
        }

        Task IGenericAuthenticationProvider.DestroyUserAsync( ISqlCallContext ctx, int actorId, int userId, string schemeSuffix, CancellationToken cancellationToken )
        {
            return DestroyTwitterUserAsync( ctx, actorId, userId, cancellationToken );
        }

        #endregion
    }
}
