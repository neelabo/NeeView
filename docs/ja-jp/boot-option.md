## :pushpin: 起動オプション

> NeeView.exe [Options...] [File or Folder]

|name|detail
|----|------
|-h, --help | ヘルプを表示します
|-v, --version | バージョン情報を表示します
|-x, --setting=<string> | 設定ファイル(UserSetting.xml)のパスを指定します
|-b, --blank | 画像ファイルを開かずに起動します
|-r, --reset-placement | ウィンドウ座標を初期化します
|-n, --new-window[=on/off] | 新しいウィンドウで起動するかを指定します
|-s, --slideshow[=on/off] | スライドショウを開始するかを指定します
|-o, --folderlist=<string> | 指定された場所の本棚を開きます。"?search="を追加することで検索キーワードも指定できます
|--window=<normal\|min\|max\|full> | 指定されたウィンドウ状態で起動します
|--script=<string> | 指定されたスクリプトファイルを起動時に実行します
|-- | オプションリストの終わりを示す。これ以降の引数はファイル名とみなされます

:information_source: コマンドラインオプションは`設定`よりも優先されます。

### 使用例

`NeeView.exe -s "E:\Pictures"`  
`NeeView.exe -o "E:\Pictures?search=foobar"`
`NeeView.exe --window=full`  
`NeeView.exe --setting="C:\Sample\CustomUserSetting.xml" --new-window=off`

----

## :pushpin: その他

* `SHIFTキー` を押しながら起動すると新しいウィンドウで起動します。