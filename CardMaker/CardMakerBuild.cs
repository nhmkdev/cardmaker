#define UNSTABLE

namespace CardMaker
{
    public static class CardMakerBuild
    {
        public static string GetBuildSuffix()
        {
#if UNSTABLE
            return "[UNSTABLE] V.A2";
#else
            return string.Empty;
#endif
        }

        public static bool IsUnstable()
        {
#if UNSTABLE
            return true;
#else
            return false;
#endif
        }
    }
}
