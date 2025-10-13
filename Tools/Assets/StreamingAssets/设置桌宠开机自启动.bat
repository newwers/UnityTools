@echo off
set current_dir=%~dp0
reg add HKCU\Software\Microsoft\Windows\CurrentVersion\Run /v PalCat /t REG_SZ /d %current_dir%DesktopPet_PinkCat.exe
echo "设置软件自启动路径:" %current_dir%DesktopPet_PinkCat.exe
pause
#reg add HKCU\Software\Microsoft\Windows\CurrentVersion\Run /v PalCat /t REG_SZ /d "D:\Work\UnityProjects\DesktopPet\PalCat\PalCat.exe"

