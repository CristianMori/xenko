﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Diagnostics;
using SiliconStudio.Core.Diagnostics;

namespace SiliconStudio.Xenko.Shaders.Parser.Performance
{
    public static class GenerateShaderPerformance
    {
        internal static Logger Logger = GlobalLogger.GetLogger("XenkoShaderPerformance"); // Global logger for shader profiling

        private static Stopwatch Global = new Stopwatch();
        private static Stopwatch GroupByConstantBuffer = new Stopwatch();
        private static Stopwatch StreamCreator = new Stopwatch();
        private static Stopwatch ExpandForEachStatements = new Stopwatch();
        private static Stopwatch RemoveUselessVariables = new Stopwatch();

        public static void Start(GenerateShaderStage stage)
        {
            switch (stage)
            {
                case GenerateShaderStage.Global:
                    Global.Start();
                    break;
                case GenerateShaderStage.GroupByConstantBuffer:
                    GroupByConstantBuffer.Start();
                    break;
                case GenerateShaderStage.StreamCreator:
                    StreamCreator.Start();
                    break;
                case GenerateShaderStage.ExpandForEachStatements:
                    ExpandForEachStatements.Start();
                    break;
                case GenerateShaderStage.RemoveUselessVariables:
                    RemoveUselessVariables.Start();
                    break;
            }
        }

        public static void Pause(GenerateShaderStage stage)
        {
            switch (stage)
            {
                case GenerateShaderStage.Global:
                    Global.Stop();
                    break;
                case GenerateShaderStage.GroupByConstantBuffer:
                    GroupByConstantBuffer.Stop();
                    break;
                case GenerateShaderStage.StreamCreator:
                    StreamCreator.Stop();
                    break;
                case GenerateShaderStage.ExpandForEachStatements:
                    ExpandForEachStatements.Stop();
                    break;
                case GenerateShaderStage.RemoveUselessVariables:
                    RemoveUselessVariables.Start();
                    break;
            }
        }

        public static void Reset()
        {
            Global.Reset();
            GroupByConstantBuffer.Reset();
            StreamCreator.Reset();
            ExpandForEachStatements.Reset();
            RemoveUselessVariables.Reset();
        }

        public static void PrintResult()
        {
            Logger.Info(@"----------------------------GENERATE SHADER ANALYZER-----------------------------");
            Logger.Info(@"Whole generation took {0} ms", Global.ElapsedMilliseconds);
            Logger.Info(@"GroupByConstantBuffer took {0} ms", GroupByConstantBuffer.ElapsedMilliseconds);
            Logger.Info(@"StreamCreator took {0} ms", StreamCreator.ElapsedMilliseconds);
            Logger.Info(@"ExpandForEachStatements took {0} ms", ExpandForEachStatements.ElapsedMilliseconds);
            Logger.Info(@"RemoveUselessVariables took {0} ms", RemoveUselessVariables.ElapsedMilliseconds);
            Logger.Info(@"-------------------------------------------------------------------------------");
        }
    }

    public enum GenerateShaderStage
    {
        Global,
        GroupByConstantBuffer,
        StreamCreator,
        ExpandForEachStatements,
        RemoveUselessVariables
    }
}
