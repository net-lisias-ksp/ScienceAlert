set ksp=D:\For New Computer\Kerbal Space Program\GameData\
set unitydir=F:\Program Files\Unity
set unityproject=D:\For New Computer\KSPCustomMods\KSPUnityProject

if not exist "%ksp%" (
	echo ERROR: KSP directory not correctly set. Edit SetEnvironment.bat
	exit -1
)

if not exist "%unitydir%" (
	echo ERROR: Unity directory not correctly set. Edit SetEnvironment.bat
	exit -2
)

if not exist "%unityproject%" (
	echo ERROR: Unity project directory not correctly set. Edit SetEnvironment.bat
	exit -4
)

if not exist "%unitydir%\Editor\Data\MonoBleedingEdge\bin\cli.bat" (
	echo ERROR: cli.bat not found; check Unity installation
	exit -3
)

echo Environment conditions set