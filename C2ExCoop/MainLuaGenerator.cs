﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace RM2ExCoop.C2ExCoop
{
    internal class MainLuaGenerator
    {
        readonly string _modName;
        readonly string _modDesc;
        readonly string _seqPath;
        readonly string _starPosPath;
        readonly List<(string, string)> _movTexs;
        readonly bool _dontUseCameraSpecific;
        readonly string _entryLevel;

        public MainLuaGenerator(string modName, string modDesc, string seqPath, string starPosPath, List<(string, string)> movTexs, bool dontUseCameraSpecific, string entryLevel)
        {
            _modName = modName;
            _modDesc = modDesc;
            _seqPath = seqPath;
            _starPosPath = starPosPath;
            _movTexs = movTexs;
            _dontUseCameraSpecific = dontUseCameraSpecific;
            _entryLevel = entryLevel;
        }

        public void Generate(string outputDir)
        {
            using StreamWriter writer = new(File.Open(Path.Join(outputDir, "main.lua"), FileMode.Create));

            writer.WriteLine("-- name: " + _modName);
            writer.WriteLine("-- description: " + _modDesc);
            writer.WriteLine("-- incompatible: romhack");

            writer.WriteLine();
            writer.WriteLine($"gLevelValues.entryLevel = LEVEL_{_entryLevel}");

            if (_dontUseCameraSpecific)
            {
                writer.WriteLine();
                writer.WriteLine("camera_set_use_course_specific_settings(false)");
            }

            writer.WriteLine();
            if (!File.Exists(_seqPath))
            {
                Logger.Warn("There were no sequences.json file generated. Skipping it.");
            }
            else
            {
                foreach (string line in File.ReadAllLines(_seqPath))
                {
                    if (!line.Contains("_custom")) continue;

                    string seqId = line.Split('"')[1].Split('_')[0];

                    if (seqId == "00")
                        continue;

                    string soundBankId = line.Split("[\"")[1].Split("\"]")[0].Split('_')[0];

                    writer.WriteLine($"smlua_audio_utils_replace_sequence(0x{seqId}, 0x{soundBankId}, 75, \"{seqId}_Seq_custom\")");
                }
            }

            writer.WriteLine();
            if (!File.Exists(_starPosPath))
            {
                Logger.Warn("There were no Star_Pos.inc.c file generated. Skipping it.");
            }
            else
            {
                foreach (string line in File.ReadAllLines(_starPosPath))
                {
                    string star = line.Split(' ')[1].Replace("BoB", "Bob").Replace("THI", "Thi").Replace("Omb", "omb");
                    string pos = line.Split("Pos ")[1].Replace("f", "").Replace("{ ", "").Replace(" }", "");

                    writer.WriteLine($"vec3f_set(gLevelValues.starPositions.{star}, {pos})");
                }
            }
        }
    }
}
