//プラグインのファイル名は、「Plugin_*.dll」という形式にして下さい。
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Drawing;
using System.Threading;
using System.ComponentModel;
using System.Windows.Forms;
using FNF.Utility;
using FNF.Controls;
using FNF.XmlSerializerSetting;
using FNF.BouyomiChanApp;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;

namespace Plugin_WebSocket {
    public class Plugin_WebSocket : IPlugin {

        #region ■フィールド
        private string _path = Base.CallAsmPath + Base.CallAsmName + ".setting";
        private PluginSettings _settings;
        private SettingFormData _settingFormData;
        private int _currentPort;
        private Accept _wsAccept;
        #endregion


        #region ■IPluginメンバの実装

        public string           Name            { get { return "WebSocketサーバー"; } }
        public string           Version         { get { return "2013/05/15版"; } }
        public string           Caption         { get { return "WebSocketからの読み上げリクエストを受け付けます。"; } } 
        public ISettingFormData SettingFormData { get { return _settingFormData; } }

        //プラグイン開始時処理
        public void Begin() {
            // 設定ファイルを読み込む
            _settings = new PluginSettings();
            if (File.Exists(_path)) {
                _settings.Load(_path);
            }

            // 現在のポート番号を記憶
            _currentPort = _settings.Port;

            // ポート番号変更イベントを登録
            _settings.PortChanged += new EventHandler(Settings_PortChanged);

            // 設定画面の初期化
            _settingFormData = new SettingFormData(_settings);

            // サーバー起動
            StartServer();
        }

        //プラグイン終了時処理
        public void End() {
            if (_wsAccept != null) {
                _wsAccept.Stop();
                _wsAccept = null;
            }
            _settings.Save(this._path);
        }

        // ポート番号変更イベントハンドラ
        private void Settings_PortChanged(object sender, EventArgs e) {
            // ポート番号が変更されていたらサーバーを再起動
            if (_settings.Port != _currentPort) {
                Pub.AddTalkTask("ポート番号が" + _currentPort + "から" + _settings.Port + "に変更されました。サーバーを再起動します。", -1, -1, VoiceType.Default);
                _currentPort = _settings.Port;
                RestartServer();
            }
        }

        // サーバーを起動
        private void StartServer() {
            if (_wsAccept != null) {
                _wsAccept.Stop();
                _wsAccept = null;
            }

            _wsAccept = new Accept(_settings.Port);
            _wsAccept.Start();
        }

        // サーバーを再起動
        private void RestartServer() {
            StartServer();
        }

        #endregion

        // 受付クラス
        class Accept {
            private int mPort;
            public bool active = true;
            Thread thread;
            Socket server;

            // コンストラクタ
            public Accept(int port) {
                mPort = port;
            }

            public void Start() {
                thread = new Thread(new ThreadStart(Run));
                Pub.AddTalkTask("ポート" + mPort + "でソケット受付を開始しました。", -1, -1, VoiceType.Default);
                thread.Start();
            }

            public void Stop() {
                active = false;
                if (server != null) {
                    try {
                        server.Close();
                    }
                    catch {
                        // エラーは無視
                    }
                }
                if (thread != null && thread.IsAlive) {
                    try {
                        thread.Abort();
                    }
                    catch {
                        // エラーは無視
                    }
                }
                Pub.AddTalkTask("ソケット受付を終了しました。", -1, -1, VoiceType.Default);
            }

            private void Run() {
                try {
                    // ポートが使用可能かチェック
                    using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)) {
                        socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                        socket.Bind(new IPEndPoint(IPAddress.Any, mPort));
                        socket.Close();
                    }

                    // 元のサーバー起動コード
                    server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    server.Bind(new IPEndPoint(IPAddress.Any, mPort));
                    server.Listen(10);
                }
                catch (SocketException ex) {
                    MessageBox.Show("サーバーの起動に失敗しました。\nポート" + mPort + "が既に使用されているか、アクセス権限がありません。\n\nエラー詳細: " + ex.Message, 
                        "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    throw;
                }

                // 要求待ち（無限ループ）
                while (active) {
                    try {
                        Socket client = server.Accept();
                        Response response = new Response(client);
                        response.Start();
                    }
                    catch (Exception) {
                        if (active) {
                            // アクティブな状態でエラーが発生した場合は少し待機
                            Thread.Sleep(1000);
                        }
                        else {
                            // 非アクティブならループを抜ける
                            break;
                        }
                    }
                }
            }
        }


        // 応答クラス
        class Response {
            enum STATUS {
                CHECKING,   // 調査中
                OK,         // OK
                ERROR,      // ERROR
            };

            private Socket mClient;
            private STATUS mStatus;
            private Thread thread;

            // コンストラクタ
            public Response(Socket client) {
                mClient = client;
                mStatus = STATUS.CHECKING;
            }

            // 開始
            public void Start() {
                thread = new Thread(new ThreadStart(Run));
                thread.Start();
            }

            // 応答実行
            private void Run()
            {
                try
                {
                    // 要求受信
                    int bsize = mClient.ReceiveBufferSize;
                    byte[] buffer = new byte[bsize];
                    int recvLen = mClient.Receive(buffer);

                    if (recvLen <= 0)
                        return;

                    String header = Encoding.ASCII.GetString(buffer, 0, recvLen);
                    Console.WriteLine("【" + System.DateTime.Now + "】\n" + header);

                    // 要求URL確認 ＆ 応答内容生成
                    int pos = header.IndexOf("GET / HTTP/");

                    if (mStatus == STATUS.CHECKING && 0 == pos)
                    {
                        doWebSocketMain(header);
                    }

                }
                catch (System.Net.Sockets.SocketException e)
                {
                    Console.Write(e.Message);
                }
                finally
                {
                    mClient.Close();
                }
            }

            // WebSocketメイン
            private void doWebSocketMain(String header) {
                String key = "Sec-WebSocket-Key: ";
                int pos = header.IndexOf(key);
                if (pos < 0) return;

                // "Sec-WebSocket-Accept"に設定する文字列を生成
                String value = header.Substring(pos + key.Length, (header.IndexOf("\r\n", pos) - (pos + key.Length)));
                byte[] byteValue = Encoding.UTF8.GetBytes(value + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11");
                SHA1 crypto = new SHA1CryptoServiceProvider();
                byte[] hash = crypto.ComputeHash(byteValue);
                String resValue = Convert.ToBase64String(hash);

                // 応答内容送信
                byte[] buffer = Encoding.UTF8.GetBytes(
                    "HTTP/1.1 101 OK\r\n" +
                    "Upgrade: websocket\r\n" +
                    "Connection: Upgrade\r\n" +
                    "Sec-WebSocket-Accept: " + resValue + "\r\n" +
                    "\r\n");

                mClient.Send(buffer);

                // クライアントからテキストを受信
                int bsize = mClient.ReceiveBufferSize;
                byte[] request = new byte[bsize];
                mClient.Receive(request);

                // WebSocketデータをデコード
                string jsonText = DecodeWebSocketData(request, bsize);
                if (string.IsNullOrEmpty(jsonText)) {
                    return;
                }

                try {
                    // JSONデータをパース
                    Dictionary<string, object> jsonData = ParseJson(jsonText);

                    // コマンドタイプを取得
                    string command = "talk"; // デフォルトは読み上げ
                    if (jsonData.ContainsKey("command")) {
                        command = jsonData["command"].ToString().ToLower();
                    }

                    // コマンドに応じた処理
                    switch (command) {
                        case "talk":
                            // 読み上げコマンド
                            ProcessTalkCommand(jsonData);
                            break;
                        case "stop":
                            // 停止コマンド
                            Pub.Stop();
                            Console.WriteLine("読み上げを停止しました");
                            break;
                        case "pause":
                            // 一時停止コマンド
                            Pub.Pause = true;
                            Console.WriteLine("読み上げを一時停止しました");
                            break;
                        case "resume":
                            // 再開コマンド
                            Pub.Pause = false;
                            Console.WriteLine("読み上げを再開しました");
                            break;
                        case "skip":
                            // スキップコマンド
                            Pub.SkipTalkTask();
                            Console.WriteLine("現在の読み上げをスキップしました");
                            break;
                        default:
                            // 不明なコマンドの場合は読み上げコマンドとして処理
                            ProcessTalkCommand(jsonData);
                            break;
                    }
                }
                catch (Exception ex) {
                    // エラーログ
                    Console.WriteLine("JSONパースエラー: " + ex.Message);
                }
            }

            // 読み上げコマンドの処理
            private void ProcessTalkCommand(Dictionary<string, object> jsonData) {
                // 各パラメータを取得
                int speed = -1;
                int pitch = -1;
                int volume = -1;
                int voiceType = 0;
                string text = string.Empty;

                if (jsonData.ContainsKey("speed")) {
                    speed = Convert.ToInt32(jsonData["speed"]);
                }
                if (jsonData.ContainsKey("pitch")) {
                    pitch = Convert.ToInt32(jsonData["pitch"]);
                }
                if (jsonData.ContainsKey("volume")) {
                    volume = Convert.ToInt32(jsonData["volume"]);
                }
                if (jsonData.ContainsKey("voiceType")) {
                    voiceType = Convert.ToInt32(jsonData["voiceType"]);
                }
                if (jsonData.ContainsKey("text")) {
                    text = jsonData["text"].ToString();
                }

                // VoiceTypeを設定
                VoiceType vt = VoiceType.Default;
                switch (voiceType) {
                    case 0: vt = VoiceType.Default; break;
                    case 1: vt = VoiceType.Female1; break;
                    case 2: vt = VoiceType.Female2; break;
                    case 3: vt = VoiceType.Male1; break;
                    case 4: vt = VoiceType.Male2; break;
                    case 5: vt = VoiceType.Imd1; break;
                    case 6: vt = VoiceType.Robot1; break;
                    case 7: vt = VoiceType.Machine1; break;
                    case 8: vt = VoiceType.Machine2; break;
                    default: vt = (VoiceType)voiceType; break;
                }

                // 読み上げ
                Pub.AddTalkTask(text, pitch, volume, speed, vt);
            }

            // WebSocketのデータをデコード
            private string DecodeWebSocketData(byte[] buffer, int size) {
                try {
                    if (size < 2) {
                        return "";
                    }

                    // 基本情報取得
                    bool fin = (buffer[0] & 0x80) != 0;    // 終了フレームかどうか
                    int opcode = buffer[0] & 0x0F;         // opcode
                    bool mask = (buffer[1] & 0x80) != 0;   // マスクされているかどうか
                    int len = buffer[1] & 0x7F;            // ペイロード長
                    int pos = 2;                           // ヘッダサイズ

                    // 長さが126以上の場合
                    if (len == 126) {
                        len = (buffer[2] << 8) + buffer[3];
                        pos = 4;
                    }
                    else if (len == 127) {
                        // 64ビット長の処理
                        long longLen = 0;
                        for (int i = 0; i < 8; i++) {
                            longLen = (longLen << 8) | buffer[2 + i];
                        }
                        
                        // int.MaxValueを超える場合は処理できない
                        if (longLen > Int32.MaxValue) {
                            Console.WriteLine("メッセージが大きすぎます: " + longLen + " バイト");
                            return "";
                        }
                        
                        len = (int)longLen;
                        pos = 10; // 2 + 8バイト
                    }

                    // マスクキー取得
                    byte[] maskKey = null;
                    if (mask) {
                        maskKey = new byte[4];
                        for (int i = 0; i < 4; i++) {
                            maskKey[i] = buffer[pos + i];
                        }
                        pos += 4;
                    }

                    // ペイロード取得
                    byte[] payload = new byte[len];
                    for (int i = 0; i < len; i++) {
                        if (pos + i < size) {
                            payload[i] = buffer[pos + i];
                        }
                        else {
                            // サイズが足りない
                            Console.WriteLine("不完全なメッセージ: 必要なサイズ " + len + " バイト, 実際のサイズ " + (size - pos) + " バイト");
                            return "";
                        }
                    }

                    // マスク解除
                    if (mask) {
                        for (int i = 0; i < len; i++) {
                            payload[i] = (byte)(payload[i] ^ maskKey[i % 4]);
                        }
                    }

                    // テキストの場合のみ処理
                    if (opcode == 1) {
                        return Encoding.UTF8.GetString(payload);
                    }
                }
                catch (Exception ex) {
                    // エラーログ
                    Console.WriteLine("WebSocketデータのデコードエラー: " + ex.Message);
                }
                return "";
            }

            // JSONデータをパースする簡易メソッド（.NET 2.0互換）
            private Dictionary<string, object> ParseJson(string jsonText) {
                Dictionary<string, object> result = new Dictionary<string, object>();
                
                // 最も単純なJSONパース（完全なJSONパーサーではありません）
                try {
                    // 中括弧を削除
                    jsonText = jsonText.Trim();
                    if (jsonText.StartsWith("{") && jsonText.EndsWith("}")) {
                        jsonText = jsonText.Substring(1, jsonText.Length - 2);
                    } else {
                        return result;
                    }
                    
                    // キーと値のペアを解析
                    bool inQuotes = false;
                    bool escaped = false;
                    StringBuilder keyBuilder = new StringBuilder();
                    StringBuilder valueBuilder = new StringBuilder();
                    bool buildingKey = true;
                    
                    for (int i = 0; i < jsonText.Length; i++) {
                        char c = jsonText[i];
                        
                        // エスケープシーケンスの処理
                        if (escaped) {
                            if (buildingKey) {
                                keyBuilder.Append(c);
                            } else {
                                valueBuilder.Append(c);
                            }
                            escaped = false;
                            continue;
                        }
                        
                        // バックスラッシュはエスケープ文字
                        if (c == '\\') {
                            escaped = true;
                            continue;
                        }
                        
                        // 引用符の処理
                        if (c == '"') {
                            inQuotes = !inQuotes;
                            continue;
                        }
                        
                        // キーと値の区切り
                        if (!inQuotes && c == ':' && buildingKey) {
                            buildingKey = false;
                            continue;
                        }
                        
                        // 次のキーと値のペアへ
                        if (!inQuotes && c == ',') {
                            string key = keyBuilder.ToString().Trim().Trim('"');
                            string value = valueBuilder.ToString().Trim().Trim('"');
                            
                            // 値を適切な型に変換
                            if (value == "true") {
                                result[key] = true;
                            } else if (value == "false") {
                                result[key] = false;
                            } else if (value == "null") {
                                result[key] = null;
                            } else {
                                // 数値かどうか確認
                                int intValue;
                                if (int.TryParse(value, out intValue)) {
                                    result[key] = intValue;
                                } else {
                                    result[key] = value;
                                }
                            }
                            
                            keyBuilder.Length = 0;
                            valueBuilder.Length = 0;
                            buildingKey = true;
                            continue;
                        }
                        
                        // 文字を追加
                        if (buildingKey) {
                            keyBuilder.Append(c);
                        } else {
                            valueBuilder.Append(c);
                        }
                    }
                    
                    // 最後のキーと値のペアを処理
                    if (keyBuilder.Length > 0) {
                        string key = keyBuilder.ToString().Trim().Trim('"');
                        string value = valueBuilder.ToString().Trim().Trim('"');
                        
                        // 値を適切な型に変換
                        if (value == "true") {
                            result[key] = true;
                        } else if (value == "false") {
                            result[key] = false;
                        } else if (value == "null") {
                            result[key] = null;
                        } else {
                            // 数値かどうか確認
                            int intValue;
                            if (int.TryParse(value, out intValue)) {
                                result[key] = intValue;
                            } else {
                                result[key] = value;
                            }
                        }
                    }
                } catch (Exception ex) {
                    Console.WriteLine("JSONパースエラー: " + ex.Message);
                }
                
                return result;
            }
        }


    }
}
