using PdfSharpCore.Fonts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Infrastructure.Helper
{
    public class CustomFontResolver : IFontResolver
    {
        private readonly string _fontPath;
        public CustomFontResolver(string fontPath)
        {
            _fontPath = fontPath;
        }

        // Bắt buộc: Default font name cho fallback
        public string DefaultFontName => "CustomFont";

        public byte[] GetFont(string faceName)
        {
            // faceName phải match tên trong FontResolverInfo
            return File.ReadAllBytes(_fontPath);
        }

        public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            if (familyName.Equals("LibertinusSerif-Regular", StringComparison.OrdinalIgnoreCase))
                return new FontResolverInfo("CustomFont");

            // fallback sang default font
            return new FontResolverInfo(DefaultFontName);
        }
    }
}
