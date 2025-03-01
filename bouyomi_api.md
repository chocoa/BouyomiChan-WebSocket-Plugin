# 棒読みちゃん プラグイン開発 API リファレンス
このドキュメントは棒読みちゃんプラグイン開発で使用される主要なクラスとインターフェースの定義をまとめたものです。.NET Framework 2.0 に準拠しています。

## 目次

- プラグイン開発の基本
- FNF.BouyomiChanApp 名前空間
- FNF.XmlSerializerSetting 名前空間
- FNF.Utility 名前空間
- FNF.Controls 名前空間

## プラグイン開発の基本

### プラグインの命名規則
- プラグインのファイル名は「Plugin_.dll」という形式にする必要があります（例：「Plugin_WebSocket.dll」）
- 設定ファイルは「Plugin_.config」または「.setting」という形式で保存されます

### プラグイン開発の基本手順

1. `IPlugin` インターフェースを実装したクラスを作成する
2. 設定が必要な場合は `SettingsBase` を継承した設定クラスを作成する
3. 設定画面が必要な場合は `ISettingFormData` と `ISettingPropertyGrid` を実装したクラスを作成する
4. `Begin()` メソッドで初期化処理を行い、`End()` メソッドで終了処理を行う
5. 読み上げ機能を使用する場合は `Pub.AddTalkTask()` メソッドを呼び出す

### 開発上の制約

- .NET Framework 2.0 の機能のみ使用可能
- C# 2.0 の文法のみ使用可能
- 以下の機能は使用不可：
  - auto-implemented プロパティ (get; set;)
  - var キーワード
  - LINQ
  - ラムダ式
  - 文字列補間($"...")
  - null 条件演算子(?.)
  - null 合体演算子(??)

## FNF.BouyomiChanApp 名前空間

### IPlugin インターフェース

プラグインの基本インターフェース。すべてのプラグインはこのインターフェースを実装する必要があります。

```csharp
public interface IPlugin {
    string Name { get; }                // プラグインの名前
    string Version { get; }             // プラグインのバージョン
    string Caption { get; }             // プラグインの説明
    ISettingFormData SettingFormData { get; } // プラグインの設定画面データ
    void Begin();                       // プラグイン開始時処理
    void End();                         // プラグイン終了時処理
}
```

### Pub クラス
棒読みちゃんの主要な機能を提供する静的クラス。

```csharp
public static class Pub {
    // 読み上げタスクを追加
    public static void AddTalkTask(string text, int speaker, int speed, VoiceType voiceType);
    public static void AddTalkTask(string text, int speaker, int volume, int speed, VoiceType voiceType);
    
    // 読み上げを停止
    public static void Stop();
    
    // 設定データ
    public static Settings Data { get; }
    
    // アプリケーション終了
    public static void ExitApplication(bool bReboot, bool bCloseLog);
    
    // メインフォーム
    public static FormMain FormMain { get; }
}
```

### Base クラス
棒読みちゃんの基本情報を提供する静的クラス。

```csharp
public static class Base {
    public static string CallAsmPath { get; }  // 呼び出し元アセンブリのパス
    public static string CallAsmName { get; }  // 呼び出し元アセンブリの名前
}
```

### VoiceType 列挙型
音声の種類を定義する列挙型。

```csharp
public enum VoiceType {
    Default,    // デフォルト
    Female1,    // 女性1
    Female2,    // 女性2
    Male1,      // 男性1
    Male2,      // 男性2
    Imd1,       // 中性
    Robot1,     // ロボット
    Machine1,   // 機械1
    Machine2    // 機械2
}
```

### FormMain クラス
棒読みちゃんのメインフォームクラス。

```csharp
public class FormMain : Form {
    // プラグインの読み込み
    public void LoadPlugins();
    
    // プラグインの解放
    public void UnloadPlugins();
    
    // 読み上げタスクを追加
    public void AddTalkTask(string text, int speaker, int volume, int speed, VoiceType voiceType);
    
    // 読み上げを停止
    public void Stop();
}
```

### BouyomiChanHttpServer クラス
棒読みちゃんのHTTPサーバークラス。

```csharp
public class BouyomiChanHttpServer : IDisposable {
    // コンストラクタ
    public BouyomiChanHttpServer(int port, FormMain owner);
    
    // ポート番号
    public int Port { get; }
    
    // 実行中かどうか
    public bool IsRunning { get; }
    
    // リソースの解放
    public void Dispose();
}
```

### BouyomiChanTcpServer クラス
棒読みちゃんのTCPサーバークラス。

```csharp
public class BouyomiChanTcpServer : IDisposable {
    // コンストラクタ
    public BouyomiChanTcpServer(int port, FormMain owner);
    
    // ポート番号
    public int Port { get; }
    
    // 実行中かどうか
    public bool IsRunning { get; }
    
    // リソースの解放
    public void Dispose();
}
```

## FNF.XmlSerializerSetting 名前空間

### SettingsBase クラス
設定ファイルの基本クラス。XMLシリアライズを使用して設定を保存・読み込みます。

```csharp
public class SettingsBase {
    // 設定の読み込み
    public void Load(string path);
    
    // 設定の保存
    public void Save(string path);
    
    // 設定読み込み時に呼ばれる（オーバーライド用）
    public virtual void ReadSettings();
    
    // 設定保存時に呼ばれる（オーバーライド用）
    public virtual void WriteSettings();
}
```

### ISettingFormData インターフェース
設定画面のデータを管理するインターフェース。

```csharp
public interface ISettingFormData {
    string Title { get; }           // 設定画面のタイトル
    bool ExpandAll { get; }         // すべての項目を展開するかどうか
    SettingsBase Setting { get; }   // 設定オブジェクト
    
    // 設定画面のコントロールを作成
    void CreateControls(Form form);
    
    // リソースの解放
    void Dispose();
}
```

### ISettingPropertyGrid インターフェース
設定プロパティグリッドのインターフェース。

```csharp
public interface ISettingPropertyGrid {
    // シート名を取得
    string GetName();
}   
```

### XSSBase<T> クラス
設定の基本クラス。ジェネリック型を使用して様々な設定を管理します。

```csharp
public class XSSBase<T> {
    // 設定の読み込み
    public void ReadSetting(T obj);
    
    // 設定の保存
    public void WriteSetting(T obj);
}
```

### XSSForm クラス
設定の基本クラス。ジェネリック型を使用して様々な設定を管理します。

```csharp
public class XSSForm : XSSBase<Form> {
    public int Width { get; set; }
    public int Height { get; set; }
    public int Left { get; set; }
    public int Top { get; set; }
    public FormWindowState WindowState { get; set; }
    
    // 設定の読み込み
    public void ReadSetting(Form form);
    
    // 設定の保存
    public void WriteSetting(Form form);
}
```

### XSSDataGridView クラス
DataGridViewの設定を管理するクラス。

```csharp
public class XSSDataGridView : XSSBase<DataGridView> {
    public class DgvColumns {
        public class DgvColumn {
            public int DisplayIndex { get; set; }
            public int Width { get; set; }
        }
        
        public DgvColumn[] Items { get; set; }
    }
    
    public DgvColumns ColumnItems { get; set; }
    
    // 設定の読み込み
    public void ReadSetting(DataGridView dgv);
    
    // 設定の保存
    public void WriteSetting(DataGridView dgv);
}
```

### XSSListView クラス
ListViewの設定を管理するクラス。

```csharp
public class XSSListView : XSSBase<ListView> {
    public class Columns {
        public class Column {
            public int DisplayIndex { get; set; }
            public int Width { get; set; }
        }
        
        public Column[] Items { get; set; }
    }
    
    public Columns ColumnItems { get; set; }
    
    // 設定の読み込み
    public void ReadSetting(ListView lv);
    
    // 設定の保存
    public void WriteSetting(ListView lv);
}
```

## FNF.Utility 名前空間

### BouyomiChan クラス
棒読みちゃんの主要な機能を提供するクラス。

```csharp
public class BouyomiChan {
    // 読み上げタスクを追加
    public void AddTalkTask(string text, int speaker, int volume, int speed, VoiceType voiceType);
    
    // 読み上げを停止
    public void Stop();
    
    // 読み上げタスク開始イベント
    public event EventHandler<TalkTaskStartedEventArgs> TalkTaskStarted;
    
    // 読み上げタスク開始イベント引数
    public class TalkTaskStartedEventArgs : EventArgs {
        public int TaskId { get; }
        public string Text { get; }
        public int Speaker { get; }
        public int Volume { get; }
        public int Speed { get; }
        public VoiceType VoiceType { get; }
    }
}
```

### BouyomiChanRemoting クラス
リモート操作用のクラス。

```csharp
public class BouyomiChanRemoting {
    // 読み上げタスクを追加
    public void AddTalkTask(string text, int speaker, int volume, int speed, VoiceType voiceType);
    
    // 読み上げを停止
    public void Stop();
}
```

### IpcRemotingServer クラス
IPCリモーティングサーバークラス。

```csharp
public class IpcRemotingServer {
    // サーバーを開始
    public void Start(string channelName, object obj);
    
    // サーバーを停止
    public void Stop();
}
```

### MciSound クラス
MCIを使用したサウンド再生クラス。

```csharp
public class MciSound {
    // サウンドを開く
    public bool Open(string fileName);
    
    // サウンドを再生
    public bool Play();
    
    // サウンドを停止
    public bool Stop();
    
    // サウンドを閉じる
    public bool Close();
}
```

### MultiReplacerRegex クラス
正規表現による複数置換を行うクラス。   

```csharp
public class MultiReplacerRegex {
    // 置換を追加
    public void Add(string pattern, string replacement);
    
    // 置換を実行
    public string Replace(string input);
}
```

## FNF.Controls 名前空間

### KeyValueList クラス
キーと値のリストを表示するコントロール。

```csharp
public class KeyValueList : Control {
    // アイテムを追加
    public KeyValueListItem Add(string key, string value);
    
    // アイテムを取得
    public KeyValueListItem GetItem(string key);
    
    // アイテムをクリア
    public void Clear();
    
    // ダブルクリックイベント
    public event EventHandler DoubleClick;
}
```

### KeyValueListItem クラス
KeyValueListのアイテムクラス。

```csharp
public class KeyValueListItem {
    // キー
    public string Key { get; set; }
    
    // 値
    public string Value { get; set; }
    
    // 表示テキスト
    public string Text { get; set; }
    
    // タグ
    public object Tag { get; set; }
}
```

### KeyValueListUpdater クラス
KeyValueListを更新するためのクラス。

```csharp
public class KeyValueListUpdater {
    // アイテムを更新
    public void Update(string key, string value);
    
    // アイテムをクリア
    public void Clear();
}
```


### WebBrowserForm クラス
Webブラウザを表示するフォーム。

```csharp
public class WebBrowserForm : Form {
    // URLを開く
    public void Navigate(string url);
    
    // HTMLを表示
    public void NavigateToString(string html);
}
```

# プラグイン実装例
以下は基本的なプラグインの実装例です：

```csharp
using System;
using System.IO;
using FNF.BouyomiChanApp;
using FNF.XmlSerializerSetting;

namespace Plugin_Sample {
    public class Plugin_Sample : IPlugin {
        private string _path = Base.CallAsmPath + Base.CallAsmName + ".setting";
        private SampleSettings _settings;
        private SettingFormData _settingFormData;
        
        // IPluginメンバの実装
        public string Name { get { return "サンプルプラグイン"; } }
        public string Version { get { return "1.0.0"; } }
        public string Caption { get { return "サンプルプラグインです。"; } }
        public ISettingFormData SettingFormData { get { return _settingFormData; } }
        
        // プラグイン開始時処理
        public void Begin() {
            // 設定ファイルを読み込む
            _settings = new SampleSettings();
            if (File.Exists(_path)) _settings.Load(_path);
            
            // 設定画面の初期化
            _settingFormData = new SettingFormData(_settings);
            
            // 初期化処理
            Pub.AddTalkTask("サンプルプラグインを開始しました。", -1, -1, VoiceType.Default);
        }
        
        // プラグイン終了時処理
        public void End() {
            // 終了処理
            _settings.Save(_path);
            Pub.AddTalkTask("サンプルプラグインを終了しました。", -1, -1, VoiceType.Default);
        }
    }
    
    // 設定クラス
    public class SampleSettings : SettingsBase {
        public int SampleValue;
        
        public SampleSettings() {
            SampleValue = 100; // デフォルト値
        }
        
        public override void ReadSettings() {
            // 必要に応じて追加の処理
        }
        
        public override void WriteSettings() {
            // 必要に応じて追加の処理
        }
    }
    
    // 設定画面データクラス
    public class SettingFormData : ISettingFormData {
        private SampleSettings _settings = null;
        public SettingPropertyGrid _propertyGrid = null;
        
        public string Title { get { return "サンプル設定"; } }
        public bool ExpandAll { get { return false; } }
        public SettingsBase Setting { get { return _settings; } }
        
        public SettingFormData(SampleSettings settings) {
            _settings = settings;
            _propertyGrid = new SettingPropertyGrid(_settings);
        }
        
        public void CreateControls(Form form) {
            // 必要に応じて追加のコントロールを作成
        }
        
        public void Dispose() {
            // リソースの解放が必要な場合はここで行う
        }
    }
    
    // 設定プロパティグリッドクラス
    public class SettingPropertyGrid : ISettingPropertyGrid {
        private SampleSettings _settings;
        
        public SettingPropertyGrid(SampleSettings setting) {
            _settings = setting;
        }
        
        public string GetName() { return "サンプル設定"; }
        
        [Category("基本設定")]
        [DisplayName("サンプル値")]
        [Description("サンプルの値を指定します。")]
        [DefaultValue(100)]
        public int SampleValue {
            get { return _settings.SampleValue; }
            set { _settings.SampleValue = value; }
        }
    }
}
```

このプラグインは、棒読みちゃんの設定画面にサンプルの値を表示し、その値を変更することができます。


