using System;
using System.IO;
using FNF.XmlSerializerSetting;

namespace Plugin_WebSocket {
    /// <summary>
    /// WebSocketプラグインの設定を管理するクラス
    /// </summary>
    public class PluginSettings : SettingsBase {
        public int Port;

        // コンストラクタ
        public PluginSettings() {
            Port = 55000; // デフォルトポート
        }

        /// <summary>
        /// 設定ファイルから設定を読み込む際に呼ばれる
        /// </summary>
        public override void ReadSettings() {
            return;
        }

        /// <summary>
        /// 設定ファイルに設定を保存する際に呼ばれる
        /// </summary>
        public override void WriteSettings() {
            return;
        }
    }
} 