@echo off
set current_dir=%~dp0
reg add HKCU\Software\Microsoft\Windows\CurrentVersion\Run /v PalCat /t REG_SZ /d %current_dir%延迟启动脚本.bat
echo "设置开机软件延迟自启动路径:" %current_dir%延迟启动脚本.bat
pause
#reg add HKCU\Software\Microsoft\Windows\CurrentVersion\Run /v PalCat /t REG_SZ /d "D:\Work\UnityProjects\DesktopPet\PalCat\PalCat.exe"

