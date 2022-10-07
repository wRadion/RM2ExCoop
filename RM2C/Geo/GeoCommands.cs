using g3;
using System;
using System.Collections.Generic;

namespace RM2ExCoop.RM2C
{
    internal class GeoCommands
    {
        public delegate List<dynamic> GeoCommand(BitStream x, string id, Script script);

        /*
         * 0x00
         * Branch and Store
         *   Branches to segmented address and stores 2 return addresses.
         */
        public static List<dynamic> BranchAndLink(BitStream x, string idPrefix, Script _)
        {
            x.Pad(24);
            uint segment = x.ReadUInt32();

            return new() { $"GEO_BRANCH_AND_LINK(Geo_{idPrefix}{Utils.Hex(segment)})", 8, "PUSH", segment, "ext" };
        }

        /*
         * 0x01
         * Terminate Geometry Layout
         *   The ending command to close.
         */
        public static List<dynamic> End(BitStream _0, string _1, Script _2) =>
            new() { "GEO_END()", 4 };

        /*
         * 0x02
         * Branch Geometry Layout
         *   Branches the current geo layout to another area within a RAM bank.
         */
        public static List<dynamic> Branch(BitStream x, string idPrefix, Script script)
        {
            // 0 = Jump, 1 = Jump and store return address
            byte type = x.ReadByte();
            x.Pad(16);
            uint segment = x.ReadUInt32();

            return new() { $"GEO_BRANCH({type},Geo_{idPrefix}{Utils.Hex(script.B2P(segment))})", 8, "PUSH", segment, "ext" };
        }

        /*
         * 0x03
         * Return From Branch
         *   Ends the current geometry layout branch and returns to the area following the 0x02 command.
         */
        public static List<dynamic> Return(BitStream _0, string _1, Script _2) =>
            new() { "GEO_RETURN()", 4, "POP", 0, "ext" };

        /*
         * 0x04
         * Open Node
         *   If you don't understand nodes, this is basically like a sub-folder.
         */
        public static List<dynamic> OpenNode(BitStream _0, string _1, Script _2) =>
            new() { "GEO_OPEN_NODE()", 4 };

        /*
         * 0x05
         * Close Node
         *   Used at the end of the data within the opened node.
         */
        public static List<dynamic> CloseNode(BitStream _0, string _1, Script _2) =>
            new() { "GEO_CLOSE_NODE()", 4 };

        /*
         * 0x06
         * Store Current Node Pointer To Table (unused)
         *   Stores the pointer of the last node into the table at address (*0x8038BCAC).
         *   This table is usually empty except for the first entry (index 0), which holds
         *   the pointer to the node created by the geo layout 0x0F command.
         *   The size of the table is defined by the geo layout 0x08 command.
         */
        public static List<dynamic> AssignAsView(BitStream x, string _0, Script _1)
        {
            x.Pad(8);
            ushort index = x.ReadUInt16();

            return new() { $"GEO_ASSIGN_AS_VIEW({index})", 4 };
        }

        /*
         * 0x07
         * Set/OR/AND Node Flags (unused)
         *   Does an operation on the flags of the current node.
         *   (Flags are at offset 0x02 of the GraphNode structure)
         */
        public static List<dynamic> UpdateNodeFlags(BitStream x, string _0, Script _1)
        {
            byte op = x.ReadByte();
            ushort value = x.ReadUInt16();

            return new() { $"GEO_UPDATE_NODE_FLAGS({op},{value})", 4 };
        }

        /*
         * 0x08
         * Set Screen Render Area
         *   Only used in geo layout of levels.
         *   Sets graph node type 0x0001.
         */
        public static List<dynamic> NodeScreenArea(BitStream x, string _0, Script _1)
        {
            x.Pad(16);
            byte entries = x.ReadByte();
            ushort xPos = x.ReadUInt16();
            ushort yPos = x.ReadUInt16();
            ushort widPrefixth = x.ReadUInt16();
            ushort height = x.ReadUInt16();

            return new() { $"GEO_NODE_SCREEN_AREA({entries},{xPos},{yPos},{widPrefixth},{height})", 12 };
        }

        /*
         * 0x09
         * Create Ortho Matrix
         *   Sets ortho matrix for level backgrounds.
         *   It is unknown if this command is actually necessary or not.
         *   You can remove this command from a level's geometry layout and it won't affect the background,
         *   and changing the scale value has no visible effect either.
         */
        public static List<dynamic> NodeOrtho(BitStream x, string _0, Script _1)
        {
            x.Pad(8);
            ushort scale = x.ReadUInt16();

            return new() { $"GEO_NODE_ORTHO({scale})", 4 };
        }

        /*
         * 0x0A
         * Set Camera Frustum
         *   Set camera frustum properties.
         *   Only used in geo layout of levels.
         *   GraphNode type 0x03
         */
        public static List<dynamic> CameraFrustum(BitStream x, string _0, Script _1)
        {
            bool useASM = x.ReadByte() > 0;
            ushort fov = x.ReadUInt16();
            ushort near = x.ReadUInt16();
            ushort far = x.ReadUInt16();
            int asmFunc = x.ReadInt32();

            if (useASM)
                return new() { $"GEO_CAMERA_FRUSTUM_WITH_FUNC({fov},{near},{far},{asmFunc})", 12, "CVASM", asmFunc, "ext" };
            else
                return new() { $"GEO_CAMERA_FRUSTUM({fov},{near},{far})", 8, "CVASM", asmFunc, "ext" };
        }

        /*
         * 0x0B
         * Start Geo Layout
         *   Starts geometry layout with no parameters whatsoever.
         *   Seems to use a pre-set render area.
         */
        public static List<dynamic> NodeStart(BitStream _0, string _1, Script _2) =>
            new() { "GEO_NODE_START()", 4 };

        /*
         * 0x0C
         * Enable/Disable Z-Buffer
         *   This command is used in level geometry layouts.
         *   Z-Buffering is disabled when rendering the skybox, and re-enabled when rendering level geometry.
         *   GraphNode type 0x04
         */
        public static List<dynamic> ZBuffer(BitStream x, string _0, Script _1)
        {
            // 0x00 = Disable Z-buffer, 0x01 = Enable Z-buffer
            byte enable = x.ReadByte();

            return new() { $"GEO_ZBUFFER({enable})", 4 };
        }

        /*
         * 0x0D
         * Set Render Range
         *   Used in WF, CCM, TTM, SSL levels and some geo layouts.
         *   This command will make the following node only render
         *   in a certain distance range away from the camera.
         *   GraphNode type 0x0B
         */
        public static List<dynamic> RenderRange(BitStream x, string _0, Script _1)
        {
            x.Pad(24);
            ushort minDistance = x.ReadUInt16();
            ushort maxDistance = x.ReadUInt16();

            return new() { $"GEO_RENDER_RANGE({minDistance},{maxDistance})", 8 };
        }

        /*
         * 0x0E
         * Switch Case
         *   Loads ASM in RAM that switches between the receding display lists within the node.
         *   GraphNode type 0x0C
         */
        public static List<dynamic> SwitchCase(BitStream x, string _0, Script _1)
        {
            x.Pad(16);
            byte numCases = x.ReadByte();
            int asmFunc = x.ReadInt32();

            return new() { $"GEO_SWITCH_CASE({numCases},{asmFunc})", 8, "CVASM", asmFunc, "ext" };
        }

        /*
         * 0x0F
         * Create Camera Graph Node
         *   GraphNode type 0x14
         */
        public static List<dynamic> Camera(BitStream x, string _0, Script _1)
        {
            x.Pad(8);
            ushort camType = x.ReadUInt16();
            Vector3i location = new(x.ReadUInt16(), x.ReadUInt16(), x.ReadUInt16());
            Vector3i focus = new(x.ReadUInt16(), x.ReadUInt16(), x.ReadUInt16());
            int asmFunc = x.ReadInt32();

            return new() { $"GEO_CAMERA({camType},{location.x},{location.y},{location.z},{focus.x},{focus.y},{focus.z},{asmFunc})", 20, "CVASM", asmFunc, "ext" };

        }

        /*
         * 0x10
         * Translate and Rotate (unused?)
         *   Applies translation & rotation to the following node.
         *   GraphNode type 0x15
         */
        public static List<dynamic> TranslateRotate(BitStream x, string idPrefix, Script _1)
        {
            byte branchingFlag = x.ReadByte();
            ushort usedIfFlag = x.ReadUInt16(); // rename this please
            Vector3i translation = new(x.ReadUInt16(), x.ReadUInt16(), x.ReadUInt16());
            Vector3i rotation = new(x.ReadUInt16(), x.ReadUInt16(), x.ReadUInt16());

            if (branchingFlag >> 4 == 8)
                return new() { $"GEO_TRANSLATE_ROTATE_WITH_DL({usedIfFlag},{translation.x},{translation.y},{translation.z},{rotation.x},{rotation.y},{rotation.z},DL_{idPrefix}{Utils.Hex((translation.z << 16) + rotation.x)})", 20 };
            else
                return new() { $"GEO_TRANSLATE_ROTATE({usedIfFlag},{translation.x},{translation.y},{translation.z},{rotation.x},{rotation.y},{rotation.z})", 16 };
        }

        /*
         * 0x11
         * Translate Node (and Load Display List or Start Geo Layout)
         *   Applies translation to the child nodes, and a display list if one is specified.
         *   You can start a geometry layout with this command to set the offset for the entire model.
         *   You cannot start a geometry layout and load a display list at the same time,
         *   as it will cause the game to freeze (white-screen).
         *   GraphNode type 0x16
         */
        public static List<dynamic> TranslateNode(BitStream x, string idPrefix, Script _)
        {
            byte flag = x.ReadByte(4);
            byte layer = x.ReadByte(4);
            Vector3i translation = new(x.ReadUInt16(), x.ReadUInt16(), x.ReadUInt16());
            uint segment = x.ReadUInt32();

            if (flag == 8)
                return new() { $"GEO_TRANSLATE_NODE_WITH_DL({layer},{translation.x},{translation.y},{translation.y},DL_{idPrefix}{Utils.Hex(segment)})", 12 };
            else
                return new() { $"GEO_TRANSLATE_NODE(0,{translation.x},{translation.y},{translation.z})", 8 };
        }

        /*
         * 0x12
         * Rotate Node (and Load Display List or Start Geo Layout)
         *   Applies rotation to the child nodes, and a display list if one is specified.
         *   You can start a geometry layout with this command to set the rotation for the entire model.
         *   You cannot start a geometry layout and load a display list at the same time,
         *   as it will cause the game to freeze (white-screen).
         *   Same as 0x11 but for rotation.
         *   GraphNode type 0x17
         */
        public static List<dynamic> RotationNode(BitStream x, string idPrefix, Script _)
        {
            byte flag = x.ReadByte(4);
            byte layer = x.ReadByte(4);
            Vector3i translation = new(x.ReadUInt16(), x.ReadUInt16(), x.ReadUInt16());
            uint segment = x.ReadUInt32();

            if (flag == 8)
                return new() { $"GEO_ROTATION_NODE_WITH_DL({layer},{translation.x},{translation.y},{translation.y},DL_{idPrefix}{Utils.Hex(segment)})", 12 };
            else
                return new() { $"GEO_ROTATION_NODE(0,{translation.x},{translation.y},{translation.z})", 8 };
        }

        /*
         * 0x13
         * Load Display List With Offset
         *   Loads display list with drawing layer and offsets the model and the node's children on X/Y/Z axis.
         *   GraphNode type 0x19
         */
        public static List<dynamic> AnimatedPart(BitStream x, string idPrefix, Script _)
        {
            byte layer = x.ReadByte();
            Vector3i offset = new(x.ReadUInt16(), x.ReadUInt16(), x.ReadUInt16());
            uint segment = x.ReadUInt32();

            string arg = segment > 0 ? $"DL_{idPrefix}{Utils.Hex(segment)}" : "0";
 
            return new() { $"GEO_ANIMATED_PART({layer},{offset.x},{offset.y},{offset.z},{arg})", 12, "STOREDL", segment, "ext" };
        }

        /*
         * 0x14
         * Billboard Model and Translate (and Load Display List or Start Geo Layout)
         *   Almost identical with the geometry layout 0x11 command, except that it
         *   will billboard the node and it's children without needing the use of 0x21 in the behavior script.
         *   You can start a geometry layout with this command to billboard and set the offset for the entire model.
         *   You cannot start a geometry layout and load a display list at the same time,
         *   as it will cause the game to freeze (white-screen).
         *   GraphNode type 0x1A
         */
        public static List<dynamic> BillboardWithParams(BitStream x, string idPrefix, Script _)
        {
            byte flag = x.ReadByte(4);
            byte layer = x.ReadByte(4);
            Vector3i translation = new(x.ReadUInt16(), x.ReadUInt16(), x.ReadUInt16());
            uint segment = x.ReadUInt32();

            if (flag == 8)
                return new() { $"GEO_BILLBOARD_WITH_PARAMS_WITH_DL({layer},{translation.x},{translation.y},{translation.y},DL_{idPrefix}{Utils.Hex(segment)})", 12 };
            else
                return new() { $"GEO_BILLBOARD_WITH_PARAMS(0,{translation.x},{translation.y},{translation.z})", 8 };
        }

        /*
         * 0x15
         * Load Display List
         *   Loads display list with drawing layer and no other properties.
         *   GraphNode type 0x1B
         */
        public static List<dynamic> DisplayList(BitStream x, string idPrefix, Script _)
        {
            byte layer = x.ReadByte();
            x.Pad(16);
            uint segment = x.ReadUInt32();

            string arg = segment > 0 ? $"DL_{idPrefix}{Utils.Hex(segment)}" : "0";

            return new() { $"GEO_DISPLAY_LIST({layer},{arg})", 8, "STOREDL", segment, "ext" };
        }

        /*
         * 0x16
         * Start Geo Layout with Shadow
         *   Used at start of the geo layout, with shadow type, solidity, and size.
         *   GraphNode type 0x28
         */
        public static List<dynamic> Shadow(BitStream x, string _0, Script _1)
        {
            x.Pad(16);
            byte type = x.ReadByte();
            x.Pad(8);
            // 00=invisible, FF=black
            byte solidity = x.ReadByte();
            ushort scale = x.ReadUInt16();

            return new() { $"GEO_SHADOW({type},{solidity},{scale})", 8 };
        }

        /*
         * 0x17
         * Create Object List
         *   Sets up rendering for 3D Objects. Without it, 0x24 objects placed in level do not render.
         *   GraphNode type 0x29
         */
        public static List<dynamic> RenderObj(BitStream _0, string _1, Script _2) =>
            new() { "GEO_RENDER_OBJ()", 4 };

        /*
         * 0x18
         * Load Polygons ASM
         *   Used in some original objects to point to ASM in RAM, for misc. effects such as vertex rippling.
         *   GraphNode type 0x2A
         */
        public static List<dynamic> Asm(BitStream x, string _0, Script _1)
        {
            x.Pad(8);
            ushort parameters = x.ReadUInt16();
            int asmFunc = x.ReadInt32();

            return new() { $"GEO_ASM({parameters},{asmFunc})", 8, "CVASM", asmFunc, "ext" };
        }

        /*
         * 0x19
         * Set Background
         *   Set background image or color.
         *   GraphNode type 0x2C
         */
        public static List<dynamic> Background(BitStream x, string _0, Script _1)
        {
            x.Pad(8);
            ushort rgbaColorOrBackgroundId = x.ReadUInt16();
            int asmFunc = x.ReadInt32();

            if (asmFunc == 0)
                return new() { $"GEO_BACKGROUND_COLOR({rgbaColorOrBackgroundId})", 8, "CVASM", asmFunc, "ext" };
            else
                return new() { $"GEO_BACKGROUND({rgbaColorOrBackgroundId},{asmFunc})", 8, "CVASM", asmFunc, "ext" };
        }

        /*
         * 0x1A
         * No Operation
         *   Doesn't do anything, nop. This command is never used in any of the scripts.
         */
        public static List<dynamic> Nop1A(BitStream _0, string _1, Script _2) =>
            new() { "GEO_NOP_1A()", 8 };

        /*
         * 0x1C
         * ?
         */
        public static List<dynamic> HeldObject(BitStream x, string _0, Script _1)
        {
            byte unk1 = x.ReadByte();
            ushort unk2 = x.ReadUInt16();
            ushort unk3 = x.ReadUInt16();
            ushort unk4 = x.ReadUInt16();
            uint unk5 = x.ReadUInt32();

            return new() { $"GEO_HELD_OBJECT({unk1},{unk2},{unk3},{unk4},{unk5})", 12 };
        }

        /*
         * 0x1D
         * Scale Model
         *   Scales the receding data uniformly.
         *   GraphNode type 0x1c
         */
        public static List<dynamic> Scale(BitStream x, string _0, Script _1)
        {
            byte flag = x.ReadByte(4);
            _ = x.ReadByte(4); // Unused here, "If MSbit of A (flag) is set, use for A2 to 8037B940"
            x.Pad(16);
            uint scale = x.ReadUInt32();
            uint dl = x.ReadUInt32();

            if (flag == 8)
                return new() { $"GEO_SCALE_WITH_DL({flag},{scale},{dl})", 12 };
            else
                return new() { $"GEO_SCALE({flag},{scale})", 8 };
        }

        /*
         * 0x1E
         * No Operation
         *   Doesn't do anything, nop. This command is never used in any of the scripts.
         */
        public static List<dynamic> Nop1E(BitStream _0, string _1, Script _2) =>
            new() { "GEO_NOP_1E()", 8 };

        /*
         * 0x1F
         * No Operation
         *   Doesn't do anything, nop. This command is never used in any of the scripts.
         */
        public static List<dynamic> Nop1F(BitStream _0, string _1, Script _2) =>
            new() { "GEO_NOP_1F()", 16 };

        /*
         * 0x20
         * Start Geo Layout with Render Area
         *   Starts the geometry layout with no shadow and a render area.
         *   If not set, a default of 300 is used.
         *   GraphNode type 0x2F
         */
        public static List<dynamic> CullingRadius(BitStream x, string _0, Script _1)
        {
            x.Pad(8);
            ushort renderDist = x.ReadUInt16();

            return new() { $"GEO_CULLING_RADIUS({renderDist})", 4 };
        }
    }
}
