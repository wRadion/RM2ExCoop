# RM2ExCoop

A program that "transform" a .z64 SM64 Romhack into a sm64ex-coop mod.

## Notes

This program is still in development. If you have any issues with it, please send me the detail of the issue to me on Discord.
If you don't have my Discord, you can contact me at this email address: wradion@gmail.com.
Please send me the Romhack **name** (or patch download link) you trying to port so I can reproduce the error myself.

## Romhack type

The program works best with Romhack **entirely** made with ROM Manager or SM64 Editor (make sure to check the corresponding box).

This program work even better if the Romhack doesn't use custom asm code or custom objects behaviors.

## TODO

- [ ] **Add proper logging/status/feedback** (most important right now)
- [ ] Handle Skyboxes automatically
- [ ] Handle Actors automatically
- [ ] Fully generate scroll targets Lua code
- [ ] Add option for WaterBoxes type
- [ ] Generate behavior table

## What does it do?

This program uses [rom-manager-2-c](https://gitlab.com/scuttlebugraiser/rom-manger-2-c/) (well, a new version recoded entirely in C# for this case).

Thanks a lot to [jesusyoshi54](https://github.com/jesusyoshi54) ([scuttlebugraiser](https://gitlab.com/scuttlebugraiser)) who made this possible.

It uses RM2C to export all the content of the .z64 Rom into C files.

It then modifies those files, and create Lua files to make the sm64ex-coop Lua mod.

### Manual Romhack port

If you want to manually port hacks for sm64ex-coop, you would have to follow [this guide](https://docs.google.com/document/d/1iQW043U51wDIU-xnyZvorHg9Dnio_N70iMPVn3JFRuY).

If you already know this guide, here is the list of steps that is done automagically by the program:

![rm2excoop_compare](https://user-images.githubusercontent.com/7728178/194631372-4023440d-264c-48ba-8705-83471d2dfbe5.jpg)
