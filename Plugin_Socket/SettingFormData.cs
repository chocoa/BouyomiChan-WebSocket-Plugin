using FNF.XmlSerializerSetting;

namespace Plugin_WebSocket {
    /// <summary>
    /// 設定画面を管理するクラス
    /// </summary>
    public class SettingFormData : ISettingFormData {
        private PluginSettings _settings = null;
        public SettingPropertyGrid _propertyGrid = null;

        // ISettingFormDataの実装
        public string Title { get { return "WebSocketサーバー設定"; } }
        public bool ExpandAll { get { return false; } }
        public SettingsBase Setting { get { return _settings; } }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="settings">設定オブジェクト</param>
        public SettingFormData(PluginSettings settings) {
            _settings = settings;
            _propertyGrid = new SettingPropertyGrid(_settings);

        }

    }
} 