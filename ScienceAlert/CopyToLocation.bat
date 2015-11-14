REM Post-Build script that copies binaries to a specified location
REM
REM Usage: [BuildDir] [DestinationDir] [args]
@echo off

call:CheckArgs %1 %2

set source=%~1
set dest=%~2

shift
shift
set params=%1
:loop
shift
if [%1]==[] goto afterloop
set params=%params% %1
goto loop
:afterloop

echo Copying files:
echo Source: %source%
echo Destination: %dest% 
echo with params: %params%

robocopy "%source% " "%dest% " %params%
call:CheckRoboCopyCode
echo Finished copying

goto:eof


:CheckArgs
echo Verifying source directory...
IF NOT EXIST "%~1" (
echo ERROR: source directory %~1 does not exist
exit -1
)

echo Verifying target directory...

IF NOT EXIST "%~2" (
echo WARNING: target directory %~2 does not exist
goto:eof
)

echo Targets verified
goto:eof

:CheckRoboCopyCode
    if %ERRORLEVEL% EQU 16 echo ***FATAL ERROR*** & exit 16
    if %ERRORLEVEL% EQU 15 echo OKCOPY + FAIL + MISMATCHES + XTRA & exit 15
    if %ERRORLEVEL% EQU 14 echo FAIL + MISMATCHES + XTRA & exit 14
    if %ERRORLEVEL% EQU 13 echo OKCOPY + FAIL + MISMATCHES & exit 13
    if %ERRORLEVEL% EQU 12 echo FAIL + MISMATCHES& exit 12
    if %ERRORLEVEL% EQU 11 echo OKCOPY + FAIL + XTRA & exit 11
    if %ERRORLEVEL% EQU 10 echo FAIL + XTRA & exit 10
    if %ERRORLEVEL% EQU 9 echo OKCOPY + FAIL & exit 9
    if %ERRORLEVEL% EQU 8 echo FAIL & exit 8
    if %ERRORLEVEL% EQU 7 echo OKCOPY + MISMATCHES + XTRA & goto end
    if %ERRORLEVEL% EQU 6 echo MISMATCHES + XTRA & goto end
    if %ERRORLEVEL% EQU 5 echo OKCOPY + MISMATCHES & goto end
    if %ERRORLEVEL% EQU 4 echo MISMATCHES & goto end
    if %ERRORLEVEL% EQU 3 echo OKCOPY + XTRA & goto end
    if %ERRORLEVEL% EQU 2 echo XTRA & goto end
    if %ERRORLEVEL% EQU 1 echo OKCOPY & goto end
    if %ERRORLEVEL% EQU 0 echo No Change & goto end
    :end  
goto:eof