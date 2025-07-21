using PdfSharp.Fonts;
using System.Reflection;

namespace OnionArchitectureAPI.Services.Print
{
    public class CustomFontResolver : IFontResolver
    {
        public byte[] GetFont(string faceName)
        {
            var resources = Assembly.GetExecutingAssembly().GetManifestResourceNames();
            foreach (var r in resources)
                Console.WriteLine("Found resource: " + r);
            var resource = "OnionArchitectureAPI.Logo.Roboto_Regular.ttf";  // ✅ Adjusted to match your actual project structure

            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource);
            if (stream == null)
            {
                // Helpful debug info
                var all = string.Join("\n", Assembly.GetExecutingAssembly().GetManifestResourceNames());
                throw new InvalidOperationException($"Could not find embedded font: {resource}\nAvailable:\n{all}");
            }

            using var ms = new MemoryStream();
            stream.CopyTo(ms);
            return ms.ToArray();
        }

        public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            return new FontResolverInfo("Roboto#");
        }

        public static void Register()
        {
            GlobalFontSettings.FontResolver ??= new CustomFontResolver();
        }
    }
}
