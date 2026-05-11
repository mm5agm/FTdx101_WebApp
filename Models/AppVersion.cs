namespace FTdx101_WebApp;

public static class AppVersion
{
    public static readonly string Current =
        ((System.Reflection.AssemblyInformationalVersionAttribute?)
            typeof(AppVersion).Assembly
                .GetCustomAttributes(typeof(System.Reflection.AssemblyInformationalVersionAttribute), false)
                .FirstOrDefault())
        ?.InformationalVersion
        ?.Split('+')[0]   // strip git hash suffix if present
        ?? "unknown";
}
