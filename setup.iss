; Script generated for Tabito's Works RedfishViewer

#define MyAppName "RedfishViewer"
#define MyAppVersion "2.0.1"
#define MyAppPublisher "Tabito's Works"
#define MyAppURL "https://github.com/tabitos-atelier/redfish-viewer"
#define MyAppBlogURL "https://tabitos-voyage.com/"
#define MyAppExeName "RedfishViewer.exe"

[Setup]
; --- アプリケーション基本情報 ---
AppId={{83CA612F-3F90-4CF2-A6A0-005EC83DDEB7}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}/issues
AppUpdatesURL={#MyAppURL}/releases
AppCopyright=© 2023-2026 Tabito's Works

; --- インストール先・権限設定 ---
DefaultDirName={localappdata}\TabitosWorks\{#MyAppName}
PrivilegesRequired=lowest
DisableProgramGroupPage=yes
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible

; --- ビジュアル・ブランディング設定 ---
WizardStyle=modern
SetupIconFile=RedfishViewer\RedfishViewer.ico
UninstallDisplayIcon={app}\{#MyAppExeName}
WizardImageFile=Images\SideImage.bmp
WizardSmallImageFile=Images\SmallImage.bmp
WizardImageStretch=yes

; --- コンパイル出力設定 ---
OutputBaseFilename=RedfishViewer_Setup
OutputDir=Setup
SolidCompression=yes

; --- ファイルバージョン情報 (プロパティ詳細) ---
VersionInfoVersion={#MyAppVersion}
VersionInfoCompany={#MyAppPublisher}
VersionInfoDescription={#MyAppName} Installer
VersionInfoProductVersion={#MyAppVersion}

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "japanese"; MessagesFile: "compiler:Languages\Japanese.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "RedfishViewer\bin\Release\publish\win-x64\*"; DestDir: "{app}"; Flags: recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent
Filename: "{#MyAppBlogURL}"; Description: "Tabito's Works の活動拠点『たびとの旅路』を訪れる"; Flags: shellexec nowait postinstall runasoriginaluser