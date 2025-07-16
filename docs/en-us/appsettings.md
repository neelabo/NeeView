# App Settings File

This section describes the configuration file **NeeView.settings.json**, which defines the behavior of the app.

> [!WARNING]
> The settings are optimized for the package. Normally, there is no need to edit them.

### PackageType

Types of packages.    
The app will run in the specified mode.

### Revision

For display.  
This is the revision number of the app source code repository.

### DateVersion

For display.  
This is the date the app was created. It is a supplementary version of Canary and Beta.

### SelfContained

For display.  
Whether it was built to be self-contained.  
false means that it is framework-dependent (-fd).

### UseLocalApplicationData

Save the profile in **LocalApplicationData.**  
If false, save the profile in the location of the executable file.  
Msi and Appx are true.

### TemporaryFilesInProfileFolder

Create a default temporary folder in the profile folder.  
If false, the system's temporary folder is used.

### PathProcessGroup

The app path is used to determine multiple launch restrictions.  
When false, only the process name is used for identification.

### Watermark

Displays the watermark for the package type.    
Canary and Beta are true.

### LogFile

For development.  
Specify the path to the log file.    
If null, no log file will be generated.
