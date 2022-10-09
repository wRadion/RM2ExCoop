# RM2ExCoop

A program that "transform" a .z64 SM64 Romhack into a sm64ex-coop mod.

## Requirements

You will need to install .NET Desktop Runtime 6.0: https://dotnet.microsoft.com/en-us/download/dotnet/6.0

## How to use

1. Open a .z64 Rom
2. Configure the settings
3. Click "Run RM2C"
4. Configure the output Mod settings
5. Click "Run C2ExCoop"
6. You will find your mod folder inside the RM2ExCoop folder, called "mod". Copy this folder in the "mods" directory of sm64ex-coop.

## Notes

This program is still in development. If you have any issues with it, please send me the detail of the issue to me on Discord.
If you don't have my Discord, you can contact me at this email address: wradion@gmail.com.
Please send me the Romhack **name** (or patch download link) you trying to port so I can reproduce the error myself.

## Romhack type

The program works best with Romhack **entirely** made with ROM Manager or SM64 Editor (make sure to check the corresponding box).

This program work even better if the Romhack doesn't use custom asm code or custom objects behaviors.

## What does it do?

This program uses [rom-manager-2-c](https://gitlab.com/scuttlebugraiser/rom-manger-2-c/) (well, a new version recoded entirely in C# for this case).

It uses RM2C to export all the content of the .z64 Rom into C files.

It then modifies those files, and create Lua files to make the sm64ex-coop Lua mod.

### Manual Romhack port

If you want to manually port hacks for sm64ex-coop, you would have to follow [this guide](https://docs.google.com/document/d/1iQW043U51wDIU-xnyZvorHg9Dnio_N70iMPVn3JFRuY).

If you already know this guide, here is the list of steps that is done automagically by the program:

![rm2excoop_compare](https://user-images.githubusercontent.com/7728178/194631372-4023440d-264c-48ba-8705-83471d2dfbe5.jpg)

# Credits

Thanks a lot to [jesusyoshi54](https://github.com/jesusyoshi54) ([scuttlebugraiser](https://gitlab.com/scuttlebugraiser)), the developer of RM2C who made this possible.

Thanks to [EmeraldLockdown](https://github.com/EmeraldLoc) for all the help provided when I first got into Romhack porting.

Thanks to [Sunk](https://www.youtube.com/channel/UCBT9x3fRcOqbDTATXVgptWg) for the creation of his Romhack port tutorial.

Thanks to [Leonitz](https://www.youtube.com/c/Leonitz) for the program icon.

Thanks to Woissil for the testing.
