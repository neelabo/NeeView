# テーマファイル仕様

テーマのコントロール配色を定義します。JSONフォーマットです。

ZIP版であれば、既定のテーマの定義ファイルは "Libraries/Themes" に配置されています。カスタムテーマを作る際の参考にしてください。

### 要素 : Format

テーマファイルであることを示す固定値です。

    "Format": "NeeView.Theme/1.0.0",

### 要素 : FormatManual

このページのURLです。アプリはこの項目を無視します。

    "FormatManual": "https://neelabo.github.io/NeeView/ja-jp/theme.html",

### 要素 : BasedOn

定義を継承します。この設定は省略可能です。

`themes://` を指定した場合は既定のテーマファイルを参照します。そのままのファイル名の場合は同じフォルダーのファイルを参照します。

この継承を使用して、変更したい部分だけの色定義をするといった使用ができます。

例：既定のダークテーマを継承

    "BasedOn": "themes://DarkTheme.json",

### 要素 : Colors

「コントロールの色定義」の連想配列です。

    "Colors": {
        :
    }


## コントロールの色定義

### キー

    例：Button.MouseOver.Background

キーはアルファベットとピリオドで定義されます。
コントロール名、コントロールの状態、コントロール部位などで表現されており、適用する場所を示しています。
最後のトークンは「属性」であり、フォールバックで重要な意味を持ちます。

### 値

フォーマット：

    [色|参照キー](/不透明度)

#### 色

「色」は #AARRGGBB, #RRGGBB, #ARGB, #RGB, 色文字列(例:"Red") といった .NET の色指定フォーマットで色指定します。

例： Window.Background は黒色

    "Window.Background": "#FF000000",

#### 参照キー

色の変わりに他のキーを指定し、その色を使用します。

例： IconButton.Pressed.Border の色は Control.Accent と同じ

    "IconButton.Pressed.Border": "Control.Accent",

#### 不透明度

「不透明度」は "/少数" で指定します。"/0.0"で完全に透明になります。
省略した場合は "/1.0" を指定するのと同じで不透明になります。


例： Window.InactiveTitle の色は Window.ActiveTitle の 50% の半透明色

    "Window.InactiveTitle": "Window.ActiveTitle/0.5",


## フォールバック

キーもしくは値が未定義の場合に代用の色を自動で決定します。

- AAA の属性 "Foreground" が定義されていない場合、"Window.Foreground" を参照します。
- AAA の属性 "Background" が定義されていない場合、"Window.Background" を参照します。
- AAA のそれ以外の属性が定義されていない場合、"AAA.Background" を参照します。

例： SideBar.Border の値が未定義なので SideBar.Background を参照する。この定義自体が存在しない場合も同様。

    "SideBar.Border": "",



## テーマで使用されるキーのリスト

キー|説明
--|--
Window.Background | ウィンドウ背景色
Window.Foreground | ウィンドウ文字色
Window.Border | ウィンドウボーダー色
Window.ActiveTitle | ウィンドウアクティブ時のタイトル文字色
Window.InactiveTitle | ウィンドウ非アクティブ時のタイトル文字色
Window.Dialog.Border | ダイアログウィンドウのボーダー色
Control.Background | 汎用：コントロールの背景色
Control.Foreground | 汎用：コントロールの文字色
Control.Border | 汎用：コントロールのボーダー色
Control.GrayText | 汎用：コントロールの灰色文字色
Control.Accent | 汎用：コントロールのアクセントカラー
Control.AccentText | 汎用：コントロールのアクセントカラー上の文字色
Control.Focus | 汎用：コントロールのフォーカス色
Item.Separator | リスト項目の区切りの色
Item.MouseOver.Background | リスト項目上にマウスカーソルがあるときの背景色
Item.MouseOver.Border | リスト項目にマウスカーソルがあるときのボーダー色
Item.Selected.Background | 選択されたリスト項目の背景色
Item.Selected.Border | 選択されたリスト項目のボーダー色
Item.Inactive.Background | 選択されたリスト項目が非アクティブなときの背景色
Item.Inactive.Border | 選択されたリスト項目が非アクティブなときのボーダー色
Button.Background | ボタンの背景色
Button.Foreground | ボタンの文字色
Button.Border | ボタンのボーダー色
Button.MouseOver.Background | ボタンにマウスカーソルがあるときの背景色
Button.MouseOver.Border | ボタンにマウスカーソルがあるときのボーダー色
Button.Checked.Background | トグルボタンがチェックされているときの背景色
Button.Checked.Border |トグルボタンがチェックされているときのボーダー色
Button.Pressed.Background | ボタンが押されているときの背景色
Button.Pressed.Border | ボタンが押されているときのボーダー色
IconButton.Background | アイコンボタンの背景色
IconButton.Foreground | アイコンボタンの文字色
IconButton.Border | アイコンボタンのボーダー色
IconButton.MouseOver.Background | アイコンボタンにマウスカーソルがあるときの背景色
IconButton.MouseOver.Border | アイコンボタンにマウスカーソルがあるときのボーダー色
IconButton.Checked.Background | トグルアイコンボタンがチェックされているときの背景色
IconButton.Checked.Border | トグルアイコンボタンがチェックされているときのボーダー色
IconButton.Pressed.Background | アイコンボタンが押されているときの背景色
IconButton.Pressed.Border | アイコンボタンが押されているときのボーダー色
Slider.Background | スライダーコントロールの背景色
Slider.Foreground | スライダーコントロールの文字色
Slider.Border | スライダーコントロールのボーダー色
Slider.Thumb | スライダーコントロールのつまみ部分の色
Slider.Track | スライダーコントロールのトラック部分の色
ScrollBar.Background | スクロールバーの背景色
ScrollBar.Foreground | スクロールバーの文字色
ScrollBar.Border | スクロールバーのボーダー色
ScrollBar.MouseOver | スクロールバーにマウスカーソルがあるときの色
ScrollBar.Pressed | スクロールバーが押されたときの色
TextBox.Background | テキストボックスの背景色
TextBox.Foreground | テキストボックスの文字色
TextBox.Border | テキストボックスのボーダー色
Menu.Background | メニューの背景色
Menu.Foreground | メニューの文字色
Menu.Border | メニューのボーダー色
Menu.Separator | メニューの区切りの色
SideBar.Background | サイドバーの背景色
SideBar.Foreground | サイドバーの文字色
SideBar.Border | サイドバーのボーダー色
Panel.Background | パネルの背景色
Panel.Foreground | パネルの文字色
Panel.Border | パネルのボーダー色
Panel.Header | パネルのサブタイトルの文字色
Panel.Note | パネルの補足説明の文字色
Panel.Separator | パネルでの区切りの色
Panel.Splitter | パネルとパネルの区切りの色
MenuBar.Background | メニューバーの背景色
MenuBar.Foreground | メニューバーの文字色
MenuBar.Border | メニューバーのボーダー色
MenuBar.Address.Background | メニューバーのアドレス領域の背景色
MenuBar.Address.Border | メニューバーのアドレス領域のボーダー色
BottomBar.Background | ページスライダー部の背景色
BottomBar.Foreground | ページスライダー部の文字色
BottomBar.Border | ページスライダー部のボーダー色
BottomBar.Slider.Background | ページスライダーの背景色
BottomBar.Slider.Foreground | ページスライダーの文字色
BottomBar.Slider.Border | ページスライダーのボーダー色
BottomBar.Slider.Thumb | ページスライダーのつまみ部の色
BottomBar.Slider.Track | ページスライダーのトラック部の色
Toast.Background | トースト通知の背景色
Toast.Foreground | トースト通知の文字色
Toast.Border | トースト通知のボーダー色
Notification.Background | メインビュー通知の背景色
Notification.Foreground | メインビュー通知の文字色
Thumbnail.Background | サムネイルの背景色
Thumbnail.Foreground | サムネイルの文字色
SelectedMark.Foreground | 現在項目を示すアイコンの色
CheckIcon.Foreground | 履歴登録済アイコンの色
BookmarkIcon.Foreground | ブックマークアイコンの色
PlaylistItemIcon.Foreground | プレイリスト項目アイコンの色