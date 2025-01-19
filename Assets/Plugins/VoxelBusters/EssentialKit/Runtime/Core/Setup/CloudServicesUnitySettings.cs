using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using VoxelBusters.CoreLibrary;

namespace VoxelBusters.EssentialKit
{
    [Serializable]
    public partial class CloudServicesUnitySettings : SettingsPropertyGroup
    {
        #region Fields

        [SerializeField]
        [Tooltip("Android specific settings.")]
        private     AndroidPlatformProperties   m_androidProperties;

        #endregion

        #region Properties

        public AndroidPlatformProperties AndroidProperties { get { return m_androidProperties; } }

        #endregion

        #region Constructors

        public CloudServicesUnitySettings(bool isEnabled = true, AndroidPlatformProperties androidProperties = null) 
            : base(isEnabled: isEnabled, name: NativeFeatureType.kCloudServices)
        {
            m_androidProperties = androidProperties;
        }

        #endregion
    }
}