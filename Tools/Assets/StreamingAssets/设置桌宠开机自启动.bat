@echo off
set current_dir=%~dp0
reg add HKCU\Software\Microsoft\Windows\CurrentVersion\Run /v PalCat /t REG_SZ /d %current_dir%DesktopPet_PinkCat.exe
echo "�������������·��:" %current_dir%DesktopPet_PinkCat.exe
pause
#reg add HKCU\Software\Microsoft\Windows\CurrentVersion\Run /v PalCat /t REG_SZ /d "D:\Work\UnityProjects\DesktopPet\PalCat\PalCat.exe"

