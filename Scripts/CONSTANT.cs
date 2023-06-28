
namespace DVAH
{
    public static class CONSTANT
    {
#if UNITY_EDITOR
        public const string Prefix = "<color=cyan>[Huynn3rdLib]</color>";
#else
        public const string Prefix = "[Huynn3rdLib]";
#endif
        #region Observer Key
        public const string LAN_1ST = "LAN_1ST";
        #endregion

        #region IAP product ID
        #endregion

        #region RemoteConfig key
        public const string FORCE_UPDATE = "FORCE_UPDATE";
        #endregion

        #region LibKey
        public const string LANGUAGE_ID = "LAN";
        public const string RATE_CHECK = "RATE";
        public const string COUNT_OPEN_APP = "COUNT_OPEN_APP";
        #endregion

        #region Custom
        #endregion
    }
}
