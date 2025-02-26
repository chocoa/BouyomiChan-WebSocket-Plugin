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
            if (File.Exists(_path)) _settings.Load(_path);

            // 現在のポート番号を記憶
            _currentPort = _settings.Port;

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

                // マスク解除
                Int64 payloadLen = request[1] & 0x7F;
                bool masked = ((request[1] & 0x80) == 0x80);
                int hp = 2;
                switch (payloadLen)
                {
                    case 126: payloadLen = request[2] * 0x100 + request[3]; hp += 2; break;
                    case 127: payloadLen = request[2] * 0x100000000000000 + request[3] * 0x1000000000000 + request[4] * 0x10000000000 + request[5] * 0x100000000 + request[6] * 0x1000000 + request[7] * 0x10000 + request[8] * 0x100 + request[9]; hp += 8; break;
                    default:  break;
                }
                if (masked)
                {
                    for (int i = 0; i < payloadLen; i++)
                    {
                        request[hp + 4 + i] ^= request[hp + (i % 4)];
                        //Console.WriteLine(buffer[6 + i]);
                    }
                    hp += 4;
                }

                // 受け取ったリクエストの解析
                String fromClient = Encoding.UTF8.GetString(request, hp, (int)payloadLen);
                
                String[] delim = { "<bouyomi>" };
                String[] param = fromClient.Split(delim, 5, StringSplitOptions.None);
                VoiceType vt = VoiceType.Default;
                if (param.Length == 5)
                {
                    switch (int.Parse(param[3])) {
                        case 0: vt = VoiceType.Default; break;
                        case 1: vt = VoiceType.Female1; break;
                        case 2: vt = VoiceType.Female2; break;
                        case 3: vt = VoiceType.Male1; break;
                        case 4: vt = VoiceType.Male2; break;
                        case 5: vt = VoiceType.Imd1; break;
                        case 6: vt = VoiceType.Robot1; break;
                        case 7: vt = VoiceType.Machine1; break;
                        case 8: vt = VoiceType.Machine2; break;
                        default: vt = (VoiceType)int.Parse(param[3]); break;
                    }
                }

                // 読み上げ
                Pub.AddTalkTask(param[4], int.Parse(param[0]), int.Parse(param[1]), int.Parse(param[2]), vt);
                
            }
        }


    }
}
