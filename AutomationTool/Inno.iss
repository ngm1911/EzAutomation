; ---------------- Cáº¥u hÃ¬nh chÃ­nh ----------------
[Setup]
AppName=AutomationTool
AppVersion=1.0.0.13
DefaultDirName={pf}\AutomationTool
DefaultGroupName=AutomationTool
UninstallDisplayIcon={app}\AutomationTool.exe
OutputDir=Output
OutputBaseFilename=AutomationTool
Compression=lzma2
SolidCompression=yes
PrivilegesRequired=admin
RestartIfNeededByRun=yes

; ---------------- ThÃªm file vÃ o bá»™ cÃ i Ä‘áº·t ----------------
[Files]
Source: "C:\Users\NguonMan\source\repos\EzAutomation\AutomationTool\bin\Release\net8.0-windows\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs
Source: "C:\Users\NguonMan\source\repos\EzAutomation\AutomationTool\bin\Release\net8.0-windows\*"; Excludes: "*.json,*.xml"; DestDir: "{app}"; Flags: recursesubdirs

; ---------------- Táº¡o shortcut ----------------
[Icons]
Name: "{group}\AutomationTool"; Filename: "{app}\AutomationTool.exe";
Name: "{userdesktop}\AutomationTool"; Filename: "{app}\AutomationTool.exe"; Tasks: desktopicon; WorkingDir: "{app}";

; ---------------- Tuá»³ chá»n ----------------
[Tasks]
Name: "desktopicon"; Description: "Create a desktop icon"; GroupDescription: "Additional icons:"

[Run]  
Filename: "{app}\AutomationTool.exe"; Description: "Launch AutomationTool"; Flags: nowait postinstall skipifsilent

; ---------------- Äiá»u kiá»‡n há»‡ Ä‘iá»u hÃ nh ----------------
[Code]
const
  faDirectory = $10;

procedure KillAppIfRunning(const exeName: string);
var
  ResultCode: Integer;
begin
  // taskkill để đóng tiến trình nếu đang chạy
  Exec('taskkill', '/IM ' + exeName + ' /F', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
end;

procedure DeleteFilesExceptDb(const folder: string);
var
  FindResult: TFindRec;
  FilePath: string;
begin
  if FindFirst(folder + '\*', FindResult) then begin
    repeat
      FilePath := folder + '\' + FindResult.Name;
      if (FindResult.Attributes and faDirectory = 0) and
         (Pos('.json', LowerCase(FindResult.Name)) = 0) then begin
        DeleteFile(FilePath);
      end;
    until not FindNext(FindResult);
    FindClose(FindResult);
  end;
end;

function InitializeSetup(): Boolean;
var
  InstallDir: string;
begin
  InstallDir := ExpandConstant('{pf}\AutomationTool'); // hoặc dùng {app} nếu đã set ở Setup

  // 1. Đóng app nếu đang chạy
  KillAppIfRunning('AutomationTool.exe'); // đổi tên đúng exe của bạn

  // 2. Xoá các file trừ *.db
  DeleteFilesExceptDb(InstallDir);

  Result := True;
end;

procedure CurStepChanged(CurStep: TSetupStep);
var
  AppPath: string;
  FindRec: TFindRec;
  FileName: string;
begin
  if CurStep = ssInstall then
  begin
    // Lấy đường dẫn cài đặt
    AppPath := ExpandConstant('{app}');
    
    // Xóa tất cả các tệp .exe trong thư mục cài đặt
    if FindFirst(AppPath + '\*.exe', FindRec) then
    begin
      try
        repeat
          FileName := AppPath + '\' + FindRec.Name;
          if not DeleteFile(FileName) then
            Log('Không thể xóa tệp: ' + FileName);
        until not FindNext(FindRec);
      finally
        FindClose(FindRec);
      end;
    end;
  end;
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
var
  AppPath: string;
begin
  if CurUninstallStep = usPostUninstall then
  begin
    AppPath := ExpandConstant('{app}');
    if DirExists(AppPath) then
    begin
      // Xóa thư mục ứng dụng sau khi gỡ cài đặt
      DelTree(AppPath, True, True, True);
    end;
  end;
end;