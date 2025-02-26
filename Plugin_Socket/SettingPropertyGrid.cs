using System;
using System.Windows.Forms;
using System.ComponentModel;
using FNF.XmlSerializerSetting;

namespace Plugin_WebSocket {
    /// <summary>
    /// 設定用プロパティグリッド
    /// </summary>
    public class SettingPropertyGrid : ISettingPropertyGrid {
        private PluginSettings _settings;

        // コンストラクタ
        public SettingPropertyGrid(PluginSettings setting) {
            _settings = setting;
        }

        //シート名
        public string GetName() { return "WebSocketサーバー設定"; }

        // ポート番号
        [Category("サーバー設定")]
        [DisplayName("ポート番号")]
        [Description("WebSocketサーバーが使用するポート番号を指定します。(1024～65535)")]
        [DefaultValue(55000)]
        public int Port {
            get { return _settings.Port; }
            set { _settings.Port = value; }
        }
    }
} 