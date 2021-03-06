﻿// -----------------------------------------------------------------------
// <copyright file="CompilerParameters.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Mizu3.Compiler
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class CompilerParameters
    {
        public string AssemblyName { get; set; }
        public string OutputFilename { get; set; }
        public string[] SourceCodeFiles { get; set; }
        public string[] References { get; set; }
        public string MainClass { get; set; }
        public bool IsDebugMode { get; set; }
    }
}
