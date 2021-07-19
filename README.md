# shole's uboat mods
This is the source repository for my mods for the game UBOAT.<br/>
https://store.steampowered.com/app/494840/UBOAT/<br/>
The mods are available on steam workshop.<br/>
https://steamcommunity.com/sharedfiles/filedetails/?id=2549989463<br/>

This repository contains the following mods:
* No Discipline - disables the discipline feature from the game
* No Discipline (Task Version) - same as above, implemented as a background task, for redundancy in case other fails
* Show Decimal Distances on Map - show decimals on map for <5km or <3nmi ranges
* Decimal Distances (global) - same as above, but global. may cause issues somewhere(?) so not recommended
# usage
1. using unity 2019.4.15f1
2. create new unity mod from uboat launcher
3. open it and hit "tools/update game assemblies"
4. git clone this repo (or download zip)
5. drop into the unity mod folder, overwriting things (i have deleted the packages/uboat/shaders folder)
6. hope stuff still works
# license
You're free to use this code as you please, but please don't just publish the same mod, unless I have abandoned it.
I have a paid license for the No Discipline sailor image (https://thenounproject.com/term/sailor/2051600/), but if you redistribute it, you have to give credit to Alice Noir, under CreativeCommons license.
