@echo off
title 删除注册表项
color 0A

echo ==============================
echo      开始删除注册表项
echo ==============================
echo.

:: 定义要删除的注册表路径
set "regPath=HKEY_CURRENT_USER\Software\Z\DesktopBalloon_BuildIn"

:: 检查注册表项是否存在
reg query "%regPath%" >nul 2>nul
if %errorLevel% neq 0 (
    echo 注册表项 "%regPath%" 不存在，无需删除！
) else (
    echo 正在删除注册表项 "%regPath%"...
    
    :: 删除注册表项（/f 参数表示强制删除，无需确认）
    reg delete "%regPath%" /f
    
    :: 检查删除结果
    reg query "%regPath%" >nul 2>nul
    if %errorLevel% neq 0 (
        echo 注册表项删除成功！
    ) else (
        echo 注册表项删除失败，请检查权限或手动删除。
    )
)

echo.
echo ==============================
echo      操作完成，按任意键退出
echo ==============================
pause >nul