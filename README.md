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


JavaScriptからプラグインへ喋らせる方法。
---------------------------------------

// 区切りは<bouyomi>になります。
var delim = "<bouyomi>";
var speed = 100; // 速度50-200。-1を指定すると本体設定
var pitch = 100; // ピッチ50-200。-1を指定すると本体設定
var volume = 100; // ボリューム0-100。-1を指定すると本体設定
var type = 0; // 声質(0.本体設定/1.女性1/2.女性2/3.男性1/4.男性2/5.中性/6.ロボット/7.機械1/8.機械2)
var text = "喋らせる内容のテキスト";

// 設定を区切りでつないで送信文字列を作る。
var sends = "" + speed + delim + pitch + delim + volume + delim + type + delim + text;

// 棒読みちゃんに送信　ポートは50002です。
var socket = new WebSocket('ws://localhost:50002/');
socket.onopen = function() {
	socket.send(sends);
}
