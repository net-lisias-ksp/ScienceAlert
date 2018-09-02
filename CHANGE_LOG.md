# Science Alert :: Change Log

* 2018-0210: 1.9.1 (DennyTX) for KSP 1.3.1
	+ No changelog provided
* 2015-0714: 1.8.9 (xEvilReeperx) for KSP 1.0.4
	+ Bugfix: Fixed an issue in StorageCache that could sometimes prevent it from properly handling OnVesselWasModified events, preventing ScienceAlert from issuing alerts
* 2015-0504: 1.8.7 (xEvilReeperx) for KSP 1.0
	+ Bugfix: Fixed an issue that could result in wasteful use of memory because Mono sucks at GC
	+ Bugfix: Fixed a small memory leak caused every time your vessel switched dominant bodies until you changed scenes
	+ Bugfix: Semi-fixed an issue that sometimes caused transmissions to fail when RemoteTech is installed. Full fix requires RemoteTech 4.7+
	+ Bugfix: should no longer spam log with exceptions when Impact! is installed
	+ Bugfix: options window filter settings weren't refreshed when loading a new profile, making it appear as though the wrong filters were being used
	+ Bugfix: "not maxed" changed from <98% collected to "at least 0.1 science remains"
	+ Bugfix: Deploying an experiment from a command seat in certain conditions should no longer turn the seat into a cloning device
	+ Bugfix: Fixed a small issue that could ScienceAlert to stop alerting if conditions are just right when rapid unplanned disassembly is occurring yet you still have control of half a ship
	+ Improvement: should now warn on "incorrect" biomes more reliably (specifically, if you're near the surface)
	+ Improvement: preference will be given to Kerbals in command seats unless there's a better qualified scientist available
* 2015-0429: 1.8.6 (xEvilReeperx) for KSP 1.0
	+ Bugfix: Taking surface samples or EVA reports from a vessel with command chair should no longer result in clones
	+ Bugfix: SCANsat science transmission fixed
	+ Bugfix: EVA reports (while flying) now trigger while in EVA on kerbin
	+ Bugfix: UI transparency now applies to experiment list window
* 2015-0428: 1.8.5 (xEvilReeperx) for KSP 1.0.0
	+ Bugfix: science transmission fixed
	+ Bugfix: tourists are no longer eligible to take EVA reports
	+ Feature: the most experienced scientist on board will be sent to collect EVA reports
* 2014-1223: 1.8.4 (xEvilReeperx) for KSP 0.90
	+ Re-fixed compatibility bug with EPL/Hangar
	+ Stock toolbar button should no longer get duplicated every time you switch to blizzy's toolbar option back
* 2014-1219: 1.8.3 (xEvilReeperx) for KSP 0.90
	+ Prevent EVA or surface sample experiments from being deployed while TimeWarp is active
	+ Re-added biome filtering; this should result in fewer "phantom alerts" that vanish before you can react
* 2014-1216: 1.8.2 (xEvilReeperx) for KSP 0.90
	+ Updated for KSP 0.90
	+ Flask animation now stops when experiment list is opened (or re-opened)
	+ Confirmation dialog when overwriting existing profiles no longer unreasonably large
	+ Correctly accounts for your facility tech level; for instance, if you are unable to go on EVA no alerts for eva reports will occur
* 2014-1201: 1.8.1 (xEvilReeperx) for KSP 0.25.0
	+ Fixed a compatability issue with EPL/Hangar (thanks taniwha for pointing out issue & fix)
	+ DLL version is now correct
* 2014-1020: 1.8 (xEvilReeperx) for KSP 0.25.0 PRE-RELEASE
	+ Updated for KSP 0.25
	+ Draggable, pinnable windows
	+ Per-vessel settings via profiles
	+ Lots of small bugfixes and improvements
* 2014-0724: 1.7.1 (xEvilReeperx) for KSP 0.24.1
	+ Updated for KSP 0.24.1
	+ Removed some debug code that was accidentally left in
* 2014-0722: 1.7 (xEvilReeperx) for KSP 0.24.0
	+ Updated for KSP 0.24
	+ Added feature: Application Launcher support (and enabled by default)
	+ Added feature: Blizzy's toolbar made optional
	+ Added feature: (optional) EVA report always listed first
	+ Added feature: (optional) Re-open experiment list when going on EVA
	+ Added feature: (optional) Surface samples can be tracked from in-vessel
	+ Added feature: (optional) Experiment list can display current biome
	+ Bugfix: ScienceAlert GUI windows should now do much less flickering
	+ Update: SCANsat interface updated to support 6.1; 6.1+ is now required for SCANsat integration
	+ Note: Per-sound settings removed
	+ Note: Lots of refactoring went on
* 2014-0711: 1.6.1 (xEvilReeperx) for KSP 0.23.5
	+ New feature: SCANsat integration
	+ Fixed an issue where alerts could appear for experiments worth no science
	+ Biome map filter could, in rare circumstances, return an incorrect result
	+ ScienceAlert should no longer stop reporting science in some circumstances while switching vessels
	+ Fixed an issue that caused KeyNotFound exceptions to spam the game, degrading performance
* 2014-0426: 1.5 (xEvilReeperx) for KSP 0.23.5
	+ New feature added: science report threshold to ignore reports worth less than specified value
	+ Fixed a camera issue that could occur when switching to EVA from map view
	+ Fixed a small issue that could rarely cause ScienceAlert to ignore biomes
	+ A small UI improvement to make room for future options
* 2014-0421: 1.4.1 (xEvilReeperx) for KSP 0.23.5
	+ Fixed a camera issue that could occur when switching to EVA from an IVA view
* 2014-0416: 1.4 (xEvilReeperx) for KSP 0.23.5
	+ No changelog provided
* 2014-0407: 1.3 (xEvilReeperx) for KSP 0.23.5 PRE-RELEASE
	+ No changelog provided
* 2014-0404: 1.2.1 (xEvilReeperx) for KSP 0.23.5 PRE-RELEASE
	+ No changelog provided
* 2014-0401: 1.1.1 (xEvilReeperx) for KSP 0.23 PRE-RELEASE
	+ No changelog provided
* 2014-0328: 1.0 (xEvilReeperx) for KSP 0.23 PRE-RELEASE
	+ No changelog provided
