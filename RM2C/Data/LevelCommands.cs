using System;

namespace RM2ExCoop.RM2C
{
    /*
     * https://hack64.net/wiki/doku.php?id=super_mario_64:level_commands
     */
    internal class LevelCommands
    {
        public delegate uint? LevelCommand(Rom rom, Command cmd, Script script);

        /* 
         * 0x00
         * Load Raw Data and Jump
         *   Loads raw data to RAM segment and jumps to segment offset.
         *   Used for loading level scripts and then jumping to an offset in them.
         */
        public static uint? LoadRawJumpPush(Rom _, Command cmd, Script script)
        {
            cmd.Args.Pad(8);
            byte segment = cmd.Args.ReadByte();
            uint start = cmd.Args.ReadUInt32();
            uint end = cmd.Args.ReadUInt32();
            uint jumpTarget = cmd.Args.ReadUInt32();

            script.Banks[segment] = new AddressRange(start, end);
            script.Stack.Push(cmd.Start);
            ++script.Top;
            script.Stack.Push(script.Base);
            ++script.Top;
            script.Base = script.Top;

            return script.B2P(jumpTarget);
        }

        /*
         * 0x01
         * Load Raw Data and Jump
         *   Load raw data to RAM segment and jump to segment offset.
         *   The only difference between this command and 0x00 is a call to 0x80278498.
         */
        public static uint? LoadRawJump(Rom _, Command cmd, Script script)
        {
            cmd.Args.Pad(8);
            byte segment = cmd.Args.ReadByte();
            uint start = cmd.Args.ReadUInt32();
            uint end = cmd.Args.ReadUInt32();
            uint jumpTarget = cmd.Args.ReadUInt32();

            script.Banks[segment] = new AddressRange(start, end);
            script.Top = script.Base;

            return script.B2P(jumpTarget);
        }

        /*
         * 0x02
         * End Level Data
         *   End of level layout data.
         *   Typically followed by null padding.
         */
        public static uint? Exit(Rom _0, Command _1, Script script)
        {
            script.Top = script.Base;
            script.Base = script.Stack.Pop();
            script.Top -= 2;

            return script.Stack.Pop();
        }

        /*
         * 0x05
         * Jump to Address
         *   Jump to level script at segmented address.
         */
        public static uint? JumpRaw(Rom _, Command cmd, Script script)
        {
            cmd.Args.Pad(16);
            uint start = cmd.Args.ReadUInt32();

            return script.B2P(start);
        }

        /* 
         * 0x06
         * Push Stack and Jump
         *   Push script stack and jump to level script at segmented address.
         *   Similar to 0x05, but also pushes current place on script stack so it can be popped later.
         */
        public static uint? JumpPush(Rom _, Command cmd, Script script)
        {
            cmd.Args.Pad(16);
            uint start = cmd.Args.ReadUInt32();

            ++script.Top;
            script.Stack.Push(cmd.Start);

            return script.B2P(start);
        }

        /*
         * 0x07
         * Pop Stack
         *   Pop script stack, returns to where previous 0x06 or 0x0C pushed from.
         */
        public static uint? Pop(Rom _0, Command _1, Script script)
        {
            --script.Top;

            return script.Stack.Pop();
        }

        /*
         * 0x0C
         * Conditional Jump
         *   If result of operation is true, jumps to segmented address.
         */
        public static uint? CondJump(Rom _, Command cmd, Script script)
        {
            cmd.Args.Pad(16);
            uint level = cmd.Args.ReadUInt32();
            uint jumpTarget = cmd.Args.ReadUInt32();

            return script.CurrLevel.Id == level ? script.B2P(jumpTarget) : cmd.Start;
        }

        /*
         * 0x10
         * No Operation
         *   Doesn't do anything, nop. This command is never used in any of the level scripts.
         */
        public static uint? Nop(Rom _0, Command _1, Script _2)
        {
            return null;
        }

        // Doesn't map to an actual Level command. Placeholder while waiting to implement other commands.
        public static uint? DoNothing(Rom _0, Command cmd, Script _1) => cmd.Start;

        /*
         * 0x16
         * Load ROM to RAM
         *   Load raw data from ROM to absolute RAM address.
         *   Call 0x802786F0 (A0 = X, A1 = Y, A2 = Z)
         */
        public static uint? LoadAsm(Rom _, Command cmd, Script script)
        {
            cmd.Args.Pad(16);
            uint ram = cmd.Args.ReadUInt32();
            uint start = cmd.Args.ReadUInt32();
            uint end = cmd.Args.ReadUInt32();

            script.Asm.Add((ram, start, end));

            return cmd.Start;
        }

        /*
         * 0x17
         * Load ROM to Segment
         *   Load raw data from ROM to RAM segment.
         */
        public static uint? LoadData(Rom _, Command cmd, Script script)
        {
            cmd.Args.Pad(8);
            byte segment = cmd.Args.ReadByte();
            uint start = cmd.Args.ReadUInt32();
            uint end = cmd.Args.ReadUInt32();

            script.Banks[segment] = new AddressRange(start, end);

            return cmd.Start;
        }

        /*
         * 0x1F
         * Start Area
         *   Start of an area.
         */
        public static uint? StartArea(Rom rom, Command cmd, Script script)
        {
            byte area = cmd.Args.ReadByte();
            cmd.Args.Pad(8);
            uint geo = cmd.Args.ReadUInt32();

            // Ignore stuff in bank 0x14 because thats star select/file select and messes up export
            if ((geo & 0xFF00_0000) == 0x1400_0000) return cmd.Start;

            script.CurrArea = (uint)(area + script.Aoffset);

            if (script.CurrArea.HasValue)
                script.CurrLevel.Areas[script.CurrArea.Value] = new Area(script.CurrArea.Value, script.CurrLevel, script.B2P(geo), rom);

            return cmd.Start;
        }

        /*
         * 0x20
         * End Area
         *   End of an area.
         */
        public static uint? EndArea(Rom _0, Command cmd, Script script)
        {
            script.CurrArea = null;

            return cmd.Start;
        }

        /*
         * 0x21
         * Load Polygon Without Geo
         *   Load polygon data without geometry layout.
         */
        public static uint? LoadPolyF3d(Rom _, Command cmd, Script script)
        {
            int layer = cmd.Args.ReadByte(4);
            cmd.Args.Pad(4);
            byte id = cmd.Args.ReadByte();
            uint f3dSegment = cmd.Args.ReadUInt32();

            try
            {
                script.Models[id] = new Model(f3dSegment, "f3d", layer, script);
            }
            catch
            {
                throw new InvalidOperationException("Unreachable ROM Bank (B2P function error) while loading model");
            }

            return cmd.Start;
        }

        /*
         * 0x22
         * Load Polygon With Geo
         *   Load polygon data with geometry layout.
         */
        public static uint? LoadPolyGeo(Rom _, Command cmd, Script script)
        {
            cmd.Args.Pad(8);
            byte id = cmd.Args.ReadByte();
            uint geoSegment = cmd.Args.ReadUInt32();

            script.Models[id] = new Model(geoSegment, "geo", null, script);

            return cmd.Start;
        }

        /*
         * 0x24
         * Place Object
         *   Places a 3D object in the level.
         */
        public static uint? PlaceObject(Rom rom, Command cmd, Script script)
        {
            Area? area = script.Area;
            if (area == null) return cmd.Start;

            byte actMask = cmd.Args.ReadByte();
            if (actMask == 0) return cmd.Start;

            byte modelId = cmd.Args.ReadByte();

            // Efficiency
            short x = cmd.Args.ReadInt16();
            short y = cmd.Args.ReadInt16();
            short z = cmd.Args.ReadInt16();
            short rx = cmd.Args.ReadInt16();
            short ry = cmd.Args.ReadInt16();
            short rz = cmd.Args.ReadInt16();
            string bparam = Utils.Hex(cmd.Args.ReadUInt32());
            uint bhv = cmd.Args.ReadUInt32();
            Obj? PO = null;

            // Check for MOP stuff first
            foreach (var pair in Data.MOPObjAddr)
            {
                if (pair.Key == (modelId, bhv))
                {
                    string bhvName = string.Format(" bhv{0}", pair.Value.Item1);
                    PO = new Obj(modelId, x, y, z, rx, ry, rz, bparam, bhvName, actMask);
                    break;
                }
            }

            if (PO == null)
            {
                string fmtBhv = bhv.ToString("X8");
                string bhvName = RomMap.GetLabel(fmtBhv);
                if (bhvName.Contains("0x" + fmtBhv))
                {
                    bhvName = " Bhv_Custom_0x" + fmtBhv;
                    // Log.UnkObject(script.Currlevel,script.CurrArea,bhv)
                }

                PO = new Obj(modelId, x, y, z, rx, ry, rz, bparam, bhvName, actMask);

                if (bhvName.Contains("editor_Scroll_Texture") || bhvName.Contains("RM_Scroll_Texture"))
                    PO = TexScroll.Convert(script, PO, rom);
            }

            area.Objects.Add(PO);

            // For parsing later at the end
            script.Objects.Add((PO, script.CurrArea.GetValueOrDefault(), bhv));

            return cmd.Start;
        }

        /*
         * 0x26
         * Connect Warps
         *   Connect warps defined by previous 0x24 (Place Object) commands.
         */
        public static uint? ConnectWarp(Rom _, Command cmd, Script script)
        {
            Area area = script.Area;
            if (area == null) return cmd.Start;

            byte id = cmd.Args.ReadByte();
            byte destLevelId = cmd.Args.ReadByte();
            byte destAreaId = cmd.Args.ReadByte();
            byte destWarpId = cmd.Args.ReadByte();
            byte flags = cmd.Args.ReadByte();

            area.Warps.Add(new WarpConnected(id, destLevelId, (byte)(destAreaId + script.Aoffset), destWarpId, flags));

            return cmd.Start;
        }

        /*
         * 0x27
         * Painting Warp
         *   Define level warps for paintings inside the Castle.
         *   Same as 0x26.
         */
        public static uint? PaintingWarp(Rom _, Command cmd, Script script)
        {
            Area area = script.Area;
            if (area == null) return cmd.Start;

            byte id = cmd.Args.ReadByte();
            byte destLevelId = cmd.Args.ReadByte();
            byte destAreaId = cmd.Args.ReadByte();
            byte destWarpId = cmd.Args.ReadByte();
            byte flags = cmd.Args.ReadByte();

            area.Warps.Add(new WarpPainting(id, destLevelId, (byte)(destAreaId + script.Aoffset), destWarpId, flags));

            return cmd.Start;
        }

        /*
         * 0x28
         * Setup instant area warp
         *   Allows Mario to be instantly teleported to another area within the current level.
         *   The teleport to an area is triggered by the collision IDs 0x1B to 0x1E.
         */
        public static uint? InstantWarp(Rom _, Command cmd, Script script)
        {
            Area area = script.Area;
            if (area == null) return cmd.Start;

            // Determines which collision to use starting with 0x1B. (00 = 0x1B, 01 = 0x1C, etc.)
            byte collisionId = cmd.Args.ReadByte();
            byte destAreaId = cmd.Args.ReadByte();
            short destX = cmd.Args.ReadInt16();
            short destY = cmd.Args.ReadInt16();
            short destZ = cmd.Args.ReadInt16();

            area.Warps.Add(new WarpInstant(collisionId, destAreaId, destX, destY, destZ));

            return cmd.Start;
        }

        /*
         * 0x2B
         * Set Mario's default position in the level
         */
        public static uint? SetMarioDefault(Rom _, Command cmd, Script script)
        {
            byte area = cmd.Args.ReadByte();
            cmd.Args.Pad(8);
            short yaw = cmd.Args.ReadInt16();
            short x = cmd.Args.ReadInt16();
            short y = cmd.Args.ReadInt16();
            short z = cmd.Args.ReadInt16();

            script.MStart = (area, yaw, x, y, z);

            return cmd.Start;
        }

        /*
         * 0x2E
         * Load Collision
         *   Loads terrain collision data for level, and other special models to be placed in the level.
         */
        public static uint? LoadCol(Rom _, Command cmd, Script script)
        {
            Area area = script.Area;
            if (area == null) return cmd.Start;

            cmd.Args.Pad(16);
            uint segment = cmd.Args.ReadUInt32();
            area.Col = segment;

            return cmd.Start;
        }

        /*
         * 0x31
         * Set Default Terrain
         */
        public static uint? SetTerrain(Rom _, Command cmd, Script script)
        {
            Area area = script.Area;
            if (area == null) return cmd.Start;

            cmd.Args.Pad(8);
            area.Terrain = cmd.Args.ReadByte();

            return cmd.Start;
        }

        /*
         * 0x36
         * Set Music
         */
        public static uint? SetMusic(Rom _, Command cmd, Script script)
        {
            Area area = script.Area;
            if (area == null) return cmd.Start;

            cmd.Args.Pad(24);
            area.Music = cmd.Args.ReadByte();

            return cmd.Start;
        }

        /*
         * 0x37
         * Set Music
         *   Used only in screen levels, like select file, select star and mario face and demo screen.
         */
        public static uint? SetMusic2(Rom _, Command cmd, Script script)
        {
            Area area = script.Area;
            if (area == null) return cmd.Start;

            cmd.Args.Pad(8);
            area.Music = cmd.Args.ReadByte();

            return cmd.Start;
        }

        /*
         * 0x39
         * Place Macro Objects
         *   Place macro objects defined in segmented address.
         */
        public static uint? MacroObjects(Rom rom, Command cmd, Script script)
        {
            cmd.Args.Pad(16);
            uint segment = cmd.Args.ReadUInt32();

            uint macros = script.B2P(segment);

            Area area = script.Area;
            area.Macros.Clear();
            int offset = 0;

            while (true)
            {
                BitStream bin = new(rom.GetBytes(macros + offset, 10));
                byte yRot = bin.ReadByte(7);
                ushort preset = bin.ReadUInt16(9);

                if (preset < 0x1F) break;

                ushort x = bin.ReadUInt16();
                ushort y = bin.ReadUInt16();
                ushort z = bin.ReadUInt16();
                ushort bParam = bin.ReadUInt16();

                area.Macros.Add(new Macro(yRot, preset, x, y, z, bParam));

                offset += 10;
            }

            return cmd.Start;
        }
    }
}
