# アプリ設定ファイル

アプリの挙動を定義する設定ファイル **NeeView.settings.json** について説明します。

> [!WARNING]
> 設定はパッケージに最適化されています。 通常は編集する必要はありません。

### PackageType

パッケージの種類。  
アプリは指定されたモードで実行されます。

### Revision

表示用。  
アプリ ソースコードのリポジトリのリビジョン番号です。

### SelfContained

表示用。  
自己完結型アプリかどうか。  
false のときはフレームワーク依存型 (-fd) であることを意味します。

### UseLocalApplicationData

**LocalApplicationData** にプロファイルを保存します。  
false の場合は実行ファイルの場所にプロファイルを保存します。  
Msi, Appx は true です。

### TemporaryFilesInProfileFolder

既定のテンポラリ フォルダーをプロファイル内に作ります。  
false の場合、システムのテンポラリ フォルダーを使用します。

### PathProcessGroup

多重起動制限の判定にアプリのパスを使用します。
false のときはプロセス名のみで判別します。

### Watermark

パッケージ種類のウォーターマークを表示します。  
Alpha, Beta は true です。

### LogFile

開発用。  
ログファイルのパスを指定します。  
null の場合はログファイルを生成しません。
