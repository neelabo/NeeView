# About Store App

Install from  [Microsoft Store](https://www.microsoft.com/store/apps/9p24z53hc1jr).

* The “Open Configuration File Location” command cannot be used because the configuration file is saved in a special location. Please use the “Import/Export all settings” command instead
* Uninstalling will delete all settings, etc.
* Updates are done through the store's functionality.
* There is no setting for “Allow network access” in Settings. This is because updates are performed by the store function, but we have no control over it.
* File extension associations will be made. There is no addition to the Explorer context menu.

## How to start from the command line

Starts with `NeeView.exe`. The complete path is not required. Parameters are the same as for the regular version.

## Protocol Activation

Protocol activation is also possible.
Currently, only the `neeview-open:` protocol is supported, and only the file path (without URI escaping) is valid as a parameter.

    > neeview-open:E:\Pictures\Image001.jpg
