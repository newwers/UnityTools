@echo off
setlocal

:: 定义正确的路径（包含LocalLow文件夹）
set "TARGET_FILE=%USERPROFILE%\AppData\LocalLow\z\DesktopBalloon_BuildIn\saveData.sd"

:: 检查文件是否存在
if not exist "%TARGET_FILE%" (
    echo 文件 "%TARGET_FILE%" 不存在，无需删除。
    goto :END
)

:: 提示用户确认（可选）
echo 警告：即将删除文件 "%TARGET_FILE%"，此操作不可撤销！
choice /C YN /M "是否继续？(Y/N)"
if errorlevel 2 (
    echo 操作已取消。
    goto :END
)

:: 删除文件（/Q 参数表示安静模式，不提示确认）
del /Q /F "%TARGET_FILE%"

if exist "%TARGET_FILE%" (
    echo 删除失败，可能缺少权限或文件被占用。
    echo 请尝试以管理员身份运行此脚本。
) else (
    echo 成功删除文件 "%TARGET_FILE%"。
)

:END
endlocal
pause