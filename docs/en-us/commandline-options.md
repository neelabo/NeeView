# NeeView Startup options

    > Usage: NeeView.exe [Options...] [File or Folder...]

## Options

Name|Description
--|--
-h, --help|This help is displayed
-v, --version|Display version information
-x, --setting=\<string\>|Specify the path of the setting file "UserSetting.json"
-b, --blank|Start up without opening the image file
-r, --reset-placement|Reset window coordinates
-n, --new-window[=off\|on]|Specify whether to start in a new window
-s, --slideshow[=off\|on]|Specify whether to start a slideshow
-o, --folderlist=\<string\>|Specify the bookshelf location.
--window=\<normal\|min\|max\|full\>|Start with the specified window state
--script=\<string\>|Executes the specified script file at startup. You can specify a file in the scripts folder by specifying 'script:\foobar.nvjs'.
--clear-registry|Clear registries for NeeView
--|Indicates the end of option list. Subsequent arguments are considered file names.

## Examples

    > NeeView.exe -s E:\Pictures

    > NeeView.exe -o "E:\Pictures?search=foobar"

    > NeeView.exe --window=full

    > NeeView.exe --setting="C:\MySetting.json" --new-window=off


## Other

Hold down `SHIFT` to start the program in a new window.

これ、ここじゃないな。
