# 💡📽️🎬 Stagefright: (tagline here)

Introducing: Stagefright! A lightning-fast DMX input manager for [Resonite](https://resonite.com) via [ResoniteModLoader](https://github.com/resonite-modding-group/ResoniteModLoader) utilizing ArtNet to let you make and control virtual DMX fixtures and stages!

- Quickly make new devices without complicated math, bit-shifting or even impulses!
- Utilizes quantized ValueStreams for very quick and performant updates over the network - won't get bogged down by queued packets!
- Exposes DMX channels as 0 -> 1 floating-point numbers for ease-of-use!


# Installation
- Download the latest [ArtfullySimple](https://github.com/RileyGuy/ArtfullySimple) release and place it into `rml_libs`
- Place `Stagefright.dll` from the latest release of this repository into `rml_mods`


# Usage
Firstly, make sure your DMX software is outputting to either `127.0.0.1` Unicast, or whatever IP you're targetting. Then, Simply spawn out your favorite Developer Tool, click 'Create New' in your context menu, navigate to the 'Stagefright' directory, and choose one of the few self-explanatory options to set the stage(s) up for DMX input.

No additional setup after this point is required if you're simply using this mod to control a ready-built DMX stage in Resonite. If you're interested
in building your own stage or devices, you can find instructions on how to make them [here](STAGES.md).

