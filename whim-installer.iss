; This requires the following CLI arguments:
; - \DMyBuildDir
; - \DMyOutputBaseFilename
; - \DMyVersion
;
; See scripts\Create-Installer.ps1

#define MyAppName "Whim"
#define MyAppExeName "Whim.Runner.exe"
#define MyAppPublisher "Isaac Daly"
#define MyAppURL "https://github.com/dalyIsaac/Whim"

[Setup]
AppId={{63D20D07-83ED-453F-8906-BB3216B8EF51}
AppName={#MyAppName}
AppVersion={#MyVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
ArchitecturesInstallIn64BitMode=x64 arm64
DefaultDirName={autopf}\{#MyAppName}
DisableProgramGroupPage=yes
LicenseFile=LICENSE
; Remove the following line to run in administrative install mode (install for all users.)
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog
MinVersion=10.0.19041
OutputDir=bin\
OutputBaseFilename={#MyOutputBaseFilename}
Compression=lzma2
SolidCompression=yes
UninstallDisplayName={#MyAppName}
WizardStyle=modern

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
Source: "{#MyBuildDir}\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#MyBuildDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent
