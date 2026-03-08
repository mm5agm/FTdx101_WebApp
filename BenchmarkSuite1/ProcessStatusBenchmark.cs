using BenchmarkDotNet.Attributes;
using System;
using System.Diagnostics;
using Microsoft.VSDiagnostics;

namespace FTdx101_WebApp.Benchmarks
{
    [CPUUsageDiagnoser]
    public class ProcessStatusBenchmark
    {
        private readonly string[] _processNames =
        {
            "wsjtx",
            "JTAlertV2",
            "L4ONG"
        };
        [Benchmark(Baseline = true)]
        public bool[] CheckProcessStatus_Current()
        {
            // Current implementation: 3 separate GetProcessesByName calls
            var results = new bool[3];
            results[0] = Process.GetProcessesByName("wsjtx").Length > 0;
            results[1] = Process.GetProcessesByName("JTAlertV2").Length > 0;
            results[2] = Process.GetProcessesByName("L4ONG").Length > 0;
            return results;
        }

        [Benchmark]
        public bool[] CheckProcessStatus_SingleSnapshot()
        {
            // Optimized: Get all processes once, then filter
            var allProcesses = Process.GetProcesses();
            var results = new bool[3];
            try
            {
                foreach (var proc in allProcesses)
                {
                    try
                    {
                        var name = proc.ProcessName;
                        if (name.Equals("wsjtx", StringComparison.OrdinalIgnoreCase))
                            results[0] = true;
                        else if (name.Equals("JTAlertV2", StringComparison.OrdinalIgnoreCase))
                            results[1] = true;
                        else if (name.Equals("L4ONG", StringComparison.OrdinalIgnoreCase))
                            results[2] = true;
                    }
                    catch
                    {
                    // Process may have exited
                    }
                }
            }
            finally
            {
                foreach (var proc in allProcesses)
                {
                    proc.Dispose();
                }
            }

            return results;
        }
    }
}