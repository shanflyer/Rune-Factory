using VoxelBusters.CoreLibrary;

namespace VoxelBusters.EssentialKit
{
    /// <summary>
    /// Provides a cross-platform interface to access information about a player playing your game.
    /// </summary>
    /// @ingroup GameServices
    public interface IPlayer
    {
        #region Properties

        /// <summary>
        /// A string assigned by game service to uniquely identify a player. (read-only)
        /// </summary>
        string Id { get; }


        /// <summary>
        /// A unique identifier for a player of all the games that you distribute using your developer account.
        /// </summary>
        /// <remarks>
        /// This property is used to identify a player across all the games that you develop and distribute with the same developer account.
        /// </remarks>
        /// @note
        /// This is currently available for iOS only.
        string DeveloperScopeId { get; }


        /// <summary>
        /// Legacy identifier for backward compatibility.
        /// Use <see cref="Id"/> or <see cref="DeveloperScopeId"/> instead.
        /// </summary>
        /// <remarks>
        /// This property is used for legacy purposes and should not be used in new code.
        /// Please use <see cref="Id"/> or <see cref="DeveloperScopeId"/> instead.
        /// </remarks>
        string LegacyId { get; }

        /// <summary>
        /// A string chosen by the player to identify themselves to others. (read-only)
        /// </summary>
        /// <description>
        /// This property is used when a player is not a friend of the local player. For displaying name on user interface, use the <see cref="DisplayName"/> property.
        /// </description>
        string Alias { get; }

        /// <summary>
        /// A string to display for the player. (read-only)
        /// </summary>
        /// <description>
        /// If the player is a friend of the local player, then the value returned is the actual name of the player. 
        /// And incase if he is not a friend, then players alias will be returned.
        /// </description>
        string DisplayName { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Loads the player profile image.
        /// </summary>
        /// <param name="callback">Callback that will be called after operation is completed.</param>
        void LoadImage(EventCallback<TextureData> callback);

        #endregion
    }
}