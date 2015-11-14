set ksp=D:\For New Computer\Kerbal Space Program\GameData\
set unity=C:\Program Files (x86)\Unity464\

if not exist "%ksp%" (
	echo ERROR: KSP directory not correctly set. Edit SetEnvironment.bat
	exit -1
)

if not exist "%unity%" (
	echo ERROR: Unity directory not correctly set. Edit SetEnvironment.bat
	exit -2
)

if not exist "%unity%\Editor\Data\MonoBleedingEdge\bin\cli.bat" (
	echo ERROR: cli.bat not found; check Unity installation
	exit -3
)

if not exist "%unity%\Editor\Data\MonoBleedingEdge\lib\mono\4.0\pdb2mdb.exe" (
	echo ERROR: pdb2mdb.exe not found; check Unity installation
	exit -4
)

echo Environment conditions set