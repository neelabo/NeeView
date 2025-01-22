# NeeView 起動オプション

    > Usage: NeeView.exe [Options...] [File or Folder...]

## Options

Name|Description
--|--
-h, --help|このヘルプを表示します
-v, --version|バージョン情報を表示します
-x, --setting=\<string\>|設定ファイル "UserSetting.json" のパスを指定します
-b, --blank|画像ファイルを開かずに起動します
-r, --reset-placement|ウィンドウ座標をリセットします
-n, --new-window[=off\|on]|新しいウィンドウで起動するかを指定します
-s, --slideshow[=off\|on]|スライドショウを開始するかを指定します
-o, --folderlist=\<string\>|本棚の場所を指定します
--window=\<normal\|min\|max\|full\>|指定されたウィンドウ状態で起動します
--script=\<string\>|指定されたスクリプト ファイルを起動時に実行します。'script:\foobar.nvjs' と指定することでスクリプト フォルダーのファイルを指定できます。
--clear-registry|NeeView 用のレジストリをクリアする
--|オプション リストの終りを示す。これ以後の引数はファイル名とみなされます

## Examples

    > NeeView.exe -s E:\Pictures

    > NeeView.exe -o "E:\Pictures?search=foobar"

    > NeeView.exe --window=full

    > NeeView.exe --setting="C:\MySetting.json" --new-window=off


## Other

Hold down `SHIFT` to start the program in a new window.

これ、ここじゃないな。
