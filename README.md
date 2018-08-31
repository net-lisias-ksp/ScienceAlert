# Science Alert Unofficial

When is is it time for science?  Unofficial fork by Lisias.

## In a Hurry

* [Releases](https://github.com/net-lisias-kspu/KerbalWindTunnel/tree/Archive)
* [Source](https://github.com/net-lisias-kspu/KerbalWindTunnel)
* [Latest Release](https://github.com/net-lisias-kspu/KerbalWindTunnel/releases)
* [Change Log](./CHANGE_LOG.md)


## Description

When is is it time for science?  ﻿

Who knows! The game provides no feedback when you've crossed a biome barrier, a new crew report is available or when you've f﻿orgotten to run a goo experiment. Wouldn't it be cool to stop guessing?

Features:

* Audio and visual cue when science is available
* Configurable science filters for experiments
* Configurable option to stop warp when an experiment becomes available
* science threshold to ignore low value reports
* SCANsat v6.1+ support! Optional SCANsat integration
* Stock toolbar support

### Usage

Actual use in-game is simple. When the button is lit, at least one experiment onboard matches your criteria. If the flask is animating, then a new experiment just became available (animation will stop when you have viewed the new experiment list). There are lots of per-experiment settings to choose from.

* to open the list of currently available experiments, **left-click** the ScienceAlert button
* to open the options menu, **right-click** the button. You can also change which button type (stock or Blizzy's) here, within the additional options submenu

#### SCANsat support

Enable SCANsat integration in the additional options menu
With this option enabled, you will only receive biome-associated alerts if you have mapped that portion of the planet's surface
Non-biome specific alerts will still occur as normal

### Known Bugs:

* if you transmit science report(s) using a transmitter's action menu, the reports queued for it won't be taken into account until transmission is complete
* [not a bug] Available experiments and report values may suddenly change when going on EVA. This is because your Kerbal may be in a different "situation" -- for example, sitting on the pad might trigger an EVA alert for LaunchPad but you go on EVA and the alert stops. This is because the game considers the Kerbal to be "flying" while holding onto a ladder while in the air. 


## Installation

In your KSP GameData folder, delete any existing EditorExtensions folder. Download the zip file to your KSP GameData folder and unzip.﻿

### Dependencies

* [KSP API Extensions/L](https://github.com/net-lisias-ksp/KSPAPIExtensions)

### License:

Licensed under LGPLv3. See [here](./LICENSE).


## UPSTREAM

* [DennyTX](https://forum.kerbalspaceprogram.com/index.php?/profile/92389-dennytx/) CURRENT MAINTAINER (?)
	+ [Forum](https://forum.kerbalspaceprogram.com/index.php?/topic/170748-131-sciencealert-191-experiment-availability-feedback-10feb18/)
	+ [GitHub](https://github.com/DennyTX/ScienceAlert)
* [jefftimlin](https://github.com/jefftimlin) PARALLEL
	+ [GitHib](https://github.com/jefftimlin/ScienceAlert)
* [xEvilReeperx](https://forum.kerbalspaceprogram.com/index.php?/profile/75857-xevilreeperx/)
	+ [Forum](https://forum.kerbalspaceprogram.com/index.php?/topic/69538-104-sciencealert-189-experiment-availability-feedback-july-13/&) 
	+ [BitBucket](https://bitbucket.org/xEvilReeperx/ksp_sciencealert)
