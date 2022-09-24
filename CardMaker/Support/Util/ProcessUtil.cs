using System.Diagnostics;

namespace Support.Util
{
    internal static class ProcessUtil
    {
        public static void StartProcess(string sFile, string sVerb = "")
        {
            new Process
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = sFile,
                    UseShellExecute = true,
                    Verb = sVerb
                }
            }.Start();
        }
    }
}