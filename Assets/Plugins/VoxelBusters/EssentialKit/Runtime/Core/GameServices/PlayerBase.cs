using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using VoxelBusters.CoreLibrary;
using VoxelBusters.CoreLibrary.NativePlugins;

namespace VoxelBusters.EssentialKit.GameServicesCore
{
    public abstract class PlayerBase : NativeObjectBase, IPlayer
    {
        #region Abstract methods

        protected abstract string GetIdInternal();

        protected abstract string GetDeveloperScopeIdInternal();

        protected abstract string GetLegacyIdInternal();

        protected abstract string GetAliasInternal();
        
        protected abstract string GetDisplayNameInternal();
        
        protected abstract void LoadImageInternal(LoadImageInternalCallback callback);

        #endregion

        #region Base class methods

        public override string ToString()
        {
            return $"[Id={Id}, Alias={Alias}, DisplayName={DisplayName}, DeveloperScopeId={DeveloperScopeId}, LegacyId={LegacyId}]";
        }

        #endregion

        #region IGameServicesPlayer implementation

        public string Id => GetIdInternal();

        public string Alias => GetAliasInternal();

        public string DisplayName => GetDisplayNameInternal();

        public string DeveloperScopeId => GetDeveloperScopeIdInternal();

        public string LegacyId => GetLegacyIdInternal();


        public void LoadImage(EventCallback<TextureData> callback)
        {
            LoadImageInternal((imageData, error) =>
            {
                // send result to caller object
                var     data    = (imageData == null) ? null : new TextureData(imageData);
                CallbackDispatcher.InvokeOnMainThread(callback, data, error);
            });
        }

        #endregion

        #region Utilities

        protected static void SendLoadPlayerFriendsResult(EventCallback<GameServicesLoadPlayerFriendsResult> callback, IPlayer[] players, Error error)
        {
            // send result to caller object
            var     result      = new GameServicesLoadPlayerFriendsResult(players ?? new IPlayer[0]);
            CallbackDispatcher.InvokeOnMainThread(callback, result, error);
        }

        protected static void SendViewClosedResult(EventCallback<bool> callback, Error error)
        {
            // send result to caller object
            CallbackDispatcher.InvokeOnMainThread(callback, error != null, error);
        }

        #endregion
    }
}