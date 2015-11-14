REM Post-Build script that generates PDB to MDB
REM
REM Usage: [targetFolder]

echo off

if "%unity%"=="" (
	echo ERROR: Unity path not set. Make sure SetEnvironment.bat has run and is configured correctly.
	exit -1
)

if [%1]=="" (
	echo ERROR: Must specify a target folder for PDB2MDB
)
call:CheckUnity

SETLOCAL ENABLEDELAYEDEXPANSION

for %%f in ("%~1\*.dll") DO (
	IF exist "%%~nf.pdb" (
		call:CreateMdb "%%f"
		echo deleting %%~nf.pdb
		del "%~1\%%~nf.pdb"
	)
)

goto:eof

REM
REM CreateMdb - Given a fully qualified path to an assembly, generate MDB with same name + appended with .mdb
REM
:CreateMdb
if exist %~1 (
call "%unity%Editor\Data\MonoBleedingEdge\bin\cli.bat" "%unity%Editor\Data\MonoBleedingEdge\lib\mono\4.0\pdb2mdb.exe" "%~1"

IF NOT EXIST "%~1".mdb (
echo ERROR: failed to generate %~1.mdb
exit -4
)
echo Successfully created %~nx1.mdb
goto:eof
) else (
echo CreateMdb: Error - %~1 not found!
exit -5
)

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

:CheckUnity
echo Verifying Unity directory set...
IF NOT EXIST "%unity%Editor\Data\MonoBleedingEdge\lib\mono\4.0\pdb2mdb.exe" (
echo ERROR: Failed to find pdb2mdb.exe; check Unity path
exit -1
)