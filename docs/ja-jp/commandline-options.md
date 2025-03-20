# NeeView 起動オプション

Version 43.0

### Usage

    > NeeView.exe [Options...] [File or Folder...]

### Options

Option|Description
--|--
\-b, \-\-blank|画像ファイルを開かずに起動します
\-h, \-\-help|このヘルプを表示します
\-l, \-\-language=\<string\>|一時的な言語 (例：'en-US')
\-n, \-\-new\-window[=off\|on]|新しいウィンドウで起動するかを指定します
\-o, \-\-folderlist=\<string\>|本棚の場所を指定します
\-r, \-\-reset\-placement|ウィンドウ座標をリセットします
\-s, \-\-slideshow[=off\|on]|スライドショウを開始するかを指定します
\-v, \-\-version|バージョン情報を表示します
\-x, \-\-setting=\<string\>|設定ファイル "UserSetting.json" のパスを指定します
\-\-clear\-registry|NeeView 用のレジストリをクリアする
\-\-script=\<string\>|指定されたスクリプト ファイルを起動時に実行します。'script:\foobar.nvjs' と指定することでスクリプト フォルダーのファイルを指定できます。
\-\-window=\<normal\|min\|max\|full\>|指定されたウィンドウ状態で起動します
\-\-|オプション リストの終りを示す。これ以後の引数はファイル名とみなされます

### Examples

`> NeeView.exe -s E:\Pictures`

`> NeeView.exe -o "E:\Pictures?search=foobar"`

`> NeeView.exe --window=full`

`> NeeView.exe --setting="C:\MySetting.json" --new-window=off`

