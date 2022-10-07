using RM2ExCoop.RM2C.BehaviorCommands;
using System;
using System.Collections.Generic;

namespace RM2ExCoop.RM2C
{
    public enum BehaviorParamType
    {
        NONE, LIST, JUMP, COL, CALL, FIELD, FIELD3
    }

    internal abstract class BehaviorCommand
    {
        public readonly int MSB;
        public readonly string Name;
        public readonly BehaviorParamType Func;
        public dynamic[] Args;

        public BehaviorCommand(int msb, string name, BehaviorParamType func = BehaviorParamType.NONE)
        {
            MSB = msb;
            Name = name;
            Func = func;
            Args = Array.Empty<dynamic>();
        }

        public abstract dynamic[] GetArgs(BitStream bin);

        public static BehaviorCommand Get(int id)
        {
            return id switch
            {
                0x00 => new BhvBegin(),
                0x01 => new BhvDelay(),
                0x02 => new BhvCall(),
                0x03 => new BhvReturn(),
                0x04 => new BhvGoto(),
                0x05 => new BhvBeginRepeat(),
                0x06 => new BhvEndRepeat(),
                0x07 => new BhvEndRepeatContinue(),
                0x08 => new BhvBeginLoop(),
                0x09 => new BhvEndLoop(),
                0x0A => new BhvBreak(),
                0x0B => new BhvBreakUnused(),
                0x0C => new BhvCallNative(),
                0x0D => new BhvAddFloat(),
                0x0E => new BhvSetFloat(),
                0x0F => new BhvAddInt(),
                0x10 => new BhvSetInt(),
                0x11 => new BhvOrInt(),
                0x12 => new BhvBitClear(),
                0x13 => new BhvSetIntRandRshift(),
                0x14 => new BhvSetRandomFloat(),
                0x15 => new BhvSetRandomInt(),
                0x16 => new BhvAddRandomFloat(),
                0x17 => new BhvAddIntRandRshift(),
                0x18 => new BhvCmdNop1(),
                0x19 => new BhvCmdNop2(),
                0x1A => new BhvCmdNop3(),
                0x1B => new BhvSetModel(),
                0x1C => new BhvSpawnChild(),
                0x1D => new BhvDeactivate(),
                0x1E => new BhvDropToFloor(),
                0x1F => new BhvSumFloat(),
                0x20 => new BhvSumInt(),
                0x21 => new BhvBillboard(),
                0x22 => new BhvHide(),
                0x23 => new BhvSetHitbox(),
                0x24 => new BhvCmdNop4(),
                0x25 => new BhvDelayVar(),
                0x26 => new BhvBeginRepeatUnused(),
                0x27 => new BhvLoadAnimations(),
                0x28 => new BhvAnimate(),
                0x29 => new BhvSpawnChildWithParam(),
                0x2A => new BhvLoadCollisionData(),
                0x2B => new BhvSetHitboxWithOffset(),
                0x2C => new BhvSpawnObj(),
                0x2D => new BhvSetHome(),
                0x2E => new BhvSetHurtbox(),
                0x2F => new BhvSetInteractType(),
                0x30 => new BhvSetObjPhysics(),
                0x31 => new BhvSetInteractionSubtype(),
                0x32 => new BhvScale(),
                0x33 => new BhvParentBitClear(),
                0x34 => new BhvAnimateTexture(),
                0x35 => new BhvDisableRendering(),
                0x36 => new BhvSetIntUnused(),
                0x37 => new BhvSpawnWaterDroplet(),
                0x38 => new BhvCylboard(),
                _ => throw new ArgumentOutOfRangeException($"No behavior command found with id {id:X2}."),
            };
        }

        public static List<string> ObjectList = new()
        {
             "OBJ_LIST_PLAYER",
             "OBJ_LIST_UNUSED_1",
             "OBJ_LIST_DESTRUCTIVE",
             "OBJ_LIST_UNUSED_3",
             "OBJ_LIST_GENACTOR",
             "OBJ_LIST_PUSHABLE",
             "OBJ_LIST_LEVEL",
             "OBJ_LIST_UNUSED_7",
             "OBJ_LIST_DEFAULT",
             "OBJ_LIST_SURFACE",
             "OBJ_LIST_POLELIKE",
             "OBJ_LIST_SPAWNER",
             "OBJ_LIST_UNIMPORTANT",
             "NUM_OBJ_LISTS"
        };

        public static Dictionary<int, string> Fields = new()
        {
            { 1, "oFlags" },
            { 2, "oDialogResponse" },
            { 2, "oDialogState" },
            { 3, "oUnk94" },
            { 5, "oIntangibleTimer" },
            { 6, "oPosX" },
            { 7, "oPosY" },
            { 8, "oPosZ" },
            { 9, "oVelX" },
            { 10, "oVelY" },
            { 11, "oVelZ" },
            { 12, "oForwardVelS32" },
            { 13, "oUnkBC" },
            { 14, "oUnkC0" },
            { 15, "oMoveAnglePitch" },
            { 16, "oMoveAngleYaw" },
            { 17, "oMoveAngleRoll" },
            { 18, "oFaceAnglePitch" },
            { 19, "oFaceAngleYaw" },
            { 20, "oFaceAngleRoll" },
            { 21, "oGraphYOffset" },
            { 22, "oActiveParticleFlags" },
            { 23, "oGravity" },
            { 24, "oFloorHeight" },
            { 25, "oMoveFlags" },
            { 26, "oAnimState" },
            { 35, "oAngleVelPitch" },
            { 36, "oAngleVelYaw" },
            { 37, "oAngleVelRoll" },
            { 38, "oAnimations" },
            { 39, "oHeldState" },
            { 40, "oWallHitboxRadius" },
            { 41, "oDragStrength" },
            { 42, "oInteractType" },
            { 43, "oInteractStatus" },
            { 47, "oBehParams2ndByte" },
            { 49, "oAction" },
            { 50, "oSubAction" },
            { 51, "oTimer" },
            { 52, "oBounciness" },
            { 53, "oDistanceToMario" },
            { 54, "oAngleToMario" },
            { 55, "oHomeX" },
            { 56, "oHomeY" },
            { 57, "oHomeZ" },
            { 58, "oFriction" },
            { 59, "oBuoyancy" },
            { 60, "oSoundStateID" },
            { 61, "oOpacity" },
            { 62, "oDamageOrCoinValue" },
            { 63, "oHealth" },
            { 64, "oBehParams" },
            { 65, "oPrevAction" },
            { 66, "oInteractionSubtype" },
            { 67, "oCollisionDistance" },
            { 68, "oNumLootCoins" },
            { 69, "oDrawingDistance" },
            { 70, "oRoom" },
            { 71, "oUnk1A4" },
            { 72, "oUnk1A8" },
            { 75, "oWallAngle" },
            { 76, "oFloorType" },
            { 76, "oFloorRoom" },
            { 77, "oAngleToHome" },
            { 78, "oFloor" },
            { 79, "oDeathSound" },
            { 29, "oYoshiChosenHome" },
            { 30, "oYoshiTargetYaw" },
            { 31, "oWoodenPostOffsetY" },
            { 32, "oWigglerTimeUntilRandomTurn" },
            { 33, "oWigglerTargetYaw" },
            { 34, "oWigglerWalkAwayFromWallTimer" },
            { 27, "oYoshiBlinkTimer" },
            { 28, "oWoodenPostPrevAngleToMario" },
            { 0, "oUkikiCageNextAction" },
            { 74, "oUnagiUnk1B0" },
            { 73, "oWigglerUnused" },
            { 31, "oBowserUnk106" },
            { 32, "oBowserHeldAnglePitch" },
            { 33, "oBowserHeldAngleVelYaw" },
            { 33, "oBowserUnk10E" },
            { 34, "oBowserAngleToCentre" },
            { 73, "oWigglerTextStatus" },
            { 74, "oUnagiUnk1B2" },
            { 27, "oUkikiTauntsToBeDone" },
        };
    }
}
