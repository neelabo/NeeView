# 最新版での不具合

こちらの [課題リスト](https://github.com/neelabo/NeeView/issues?q=is%3Aissue%20state%3Aopen%20label%3Abug) から確認できます。


# 確認されている問題

以下の問題は現状では仕様となります


## "CopyTrans HEIC for Windows" を使用するとクラッシュする

HEIC画像を表示可能にする"CopyTrans HEIC for Windows"というソフトがありますが、NeeViewでは動作が不安定になり、NeeView自体が起動できないこともあります。
このため、"CopyTrans HEIC for Windows"はサポート外といたします。

Windows10でHEIC画像を表示させるためには、ストアから [HEIF画像拡張機能](https://www.microsoft.com/store/apps/9pmmsr1cgpwg) をインストールしてお試しください。

## Susieプラグインの問題

Susieプラグインとの相性問題があります。使用するプラグインを限定して使用されることをお薦めします。

* 機能しないプラグインがある
* 圧縮ファイル中の画像を開こうとすると落ちる
* 1回目は読み込めるが2回目に読み込もうとすると強制終了する（同時に使用するプラグインとの組み合わせ？）