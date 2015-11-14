REM Post-Build script that cleans up binaries (dll, mdb, reeper, reloadable) in specified directories
REM
REM Usage [directory]... 

set TargetExtensions=dll,mdb,reeper,reloadable,pdb
echo Cleaning solution GameData...

:NextParameter

if [%1]==[] (
	echo Finished cleaning
	goto:Finished
)

for %%i in (%TargetExtensions%) do (
	Call:DeleteFilesWithExtensionInDirectory %1 %%i
)

shift
goto:NextParameter


goto:eof


REM
REM Delete files that match wildcard in second argument from directory in first
REM 
:DeleteFilesWithExtensionInDirectory
if "%~1"=="" (
	echo ERROR: requires a directory parameter
	exit -22
)

if "%~2"=="" (
	echo ERROR: requires an extension parameter
	exit -23
)

if not exist "%~1" goto:eof


cd %1
for %%f in (*.%~2) DO (
echo deleting %%f
del %%f
)

goto:eof

:Finished