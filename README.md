棒読みちゃん用のWebSocket受付プラグイン
---------------------------------------

ビルド
------

コンパイル前に参照設定ですでに設定されているBouyomichanを削除し、
手持ちのBouyomiChan.exeを指定して追加してください。


インストール
------------
コンパイルして作成したPlugin_WebSocket.dllをBouyomichan.exeと同じフォルダに入れるだけです。

棒読みちゃんを起動し、その他タブのプラグインでWebSocketサーバーにチェックを入れて有効化してください。


JavaScriptからプラグインへ喋らせる方法
---------------------------------------

### JSON形式での送信方法
```javascript
// WebSocketを使用して棒読みちゃんに接続
var socket = new WebSocket('ws://localhost:55000/');

// 読み上げリクエスト
socket.onopen = function() {
    // 読み上げコマンド
    var talkData = {
        command: "talk",
        speed: 100,    // 速度50-200。-1を指定すると本体設定
        pitch: 100,    // ピッチ50-200。-1を指定すると本体設定
        volume: 100,   // ボリューム0-100。-1を指定すると本体設定
        voiceType: 0,  // 声質(0.本体設定/1.女性1/2.女性2/3.男性1/4.男性2/5.中性/6.ロボット/7.機械1/8.機械2)
        text: "こんにちは、棒読みちゃんです"
    };
    
    socket.send(JSON.stringify(talkData));
};
```

### 制御コマンド
JSON形式では、読み上げ以外にも以下の制御コマンドが使用できます：

```javascript
// 読み上げ停止
socket.send(JSON.stringify({ command: "stop" }));

// 読み上げ一時停止
socket.send(JSON.stringify({ command: "pause" }));

// 読み上げ再開
socket.send(JSON.stringify({ command: "resume" }));

// 現在の読み上げをスキップ
socket.send(JSON.stringify({ command: "skip" }));
```
