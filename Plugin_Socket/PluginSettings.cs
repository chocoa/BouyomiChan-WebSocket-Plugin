using System;
using System.IO;
using FNF.XmlSerializerSetting;

namespace Plugin_WebSocket {
    /// <summary>
    /// WebSocketプラグインの設定を管理するクラス
    /// </summary>
    public class PluginSettings : SettingsBase {
        private int _port;
        
        // ポート番号が変更されたときに発生するイベント
        public event EventHandler PortChanged;
        
        // ポート番号
        public int Port {
            get { return _port; }
            set { 
                if (_port != value) {
                    int oldPort = _port;
                    _port = value;
                    // ポート番号が変更されたらイベントを発生
                    if (PortChanged != null) {
                        PortChanged(this, EventArgs.Empty);
                    }
                }
            }
        }

        // コンストラクタ
        public PluginSettings() {
            _port = 55000; // デフォルトポート
        }

        /// <summary>
        /// 設定フォームが閉じられたときに呼ばれる
        /// </summary>
        public override void OnSettingFormClosed(FNF.XmlSerializerSetting.SettingForm.CloseKind closeKind) {
            base.OnSettingFormClosed(closeKind);
            // OKボタンで閉じられた場合のみ処理
            if (closeKind == FNF.XmlSerializerSetting.SettingForm.CloseKind.OK) {
                // ポート番号が変更されていたらイベントを発生
                if (PortChanged != null) {
                    PortChanged(this, EventArgs.Empty);
                }
            }
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