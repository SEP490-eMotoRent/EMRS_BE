using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UglyToad.PdfPig.Content;

namespace EMRS.Infrastructure.Helper
{
    #region Enums & Models

    public enum TextAlign { Left, Center, Right }

    public class OverlaySettings
    {
        public double BoxOffsetX { get; set; } = 0;
        public double BoxOffsetY { get; set; } = 0;
        public double TextOffsetX { get; set; } = 0;
        public double TextOffsetY { get; set; } = 0;

        public double PaddingLeft { get; set; } = 0;
        public double PaddingRight { get; set; } = 0;
        public double PaddingTop { get; set; } = 0;
        public double PaddingBottom { get; set; } = 0;

        public TextAlign Align { get; set; } = TextAlign.Center;
        public double FontSize { get; set; } = 10;
        public double MinFontSize { get; set; } = 6;
        public bool AutoScaleFont { get; set; } = true;
        public bool DrawBackground { get; set; } = true;

        public OverlaySettings Clone() => (OverlaySettings)MemberwiseClone();
    }

    #endregion

    #region Presets

    public static class OverlayPresets
    {
        public static OverlaySettings Default => new()
        {
            BoxOffsetX = 0,
            BoxOffsetY = 0,
            TextOffsetX = 0,
            TextOffsetY = 0,
            PaddingLeft = 0,
            PaddingRight = 0,
            PaddingTop = 0,
            PaddingBottom = 0,
            Align = TextAlign.Center,
            FontSize = 10,
            MinFontSize = 6,
            AutoScaleFont = true,
            DrawBackground = true
        };

        public static OverlaySettings ShortText => new()
        {
            PaddingLeft = 2,
            PaddingRight = 2,
            PaddingTop = 1,
            PaddingBottom = 1,
            Align = TextAlign.Center,
            FontSize = 10,
            AutoScaleFont = true,
            DrawBackground = true
        };

        public static OverlaySettings MediumText => new()
        {
            PaddingLeft = 3,
            PaddingRight = 3,
            PaddingTop = 1,
            PaddingBottom = 1,
            Align = TextAlign.Left,
            FontSize = 10,
            AutoScaleFont = true,
            DrawBackground = true
        };

        public static OverlaySettings LongText => new()
        {
            PaddingLeft = 2,
            PaddingRight = 2,
            PaddingTop = 1,
            PaddingBottom = 1,
            Align = TextAlign.Left,
            FontSize = 9,
            MinFontSize = 6,
            AutoScaleFont = true,
            DrawBackground = true
        };

        public static OverlaySettings AllUppercase => new()
        {
            PaddingLeft = 5,
            PaddingRight = 5,
            PaddingTop = 2,
            PaddingBottom = 2,
            Align = TextAlign.Center,
            FontSize = 10,
            AutoScaleFont = true,
            DrawBackground = true
        };
    }

    #endregion

    #region Helper

    public static class PdfGeneratorHelper
    {
        private const string FontFamily = "LibertinusSerif-Regular";

        public static OverlaySettings DetectSettings(string text)
        {
            if (string.IsNullOrEmpty(text))
                return OverlayPresets.Default;

            var letters = text.Where(char.IsLetter).ToList();
            int upperCount = letters.Count(c =>
            {
                var cat = CharUnicodeInfo.GetUnicodeCategory(c);
                return cat == UnicodeCategory.UppercaseLetter ||
                       cat == UnicodeCategory.TitlecaseLetter;
            });

            bool isAllUpper = letters.Count > 0 && upperCount == letters.Count;

            if (isAllUpper && text.Length > 10)
                return OverlayPresets.AllUppercase;

            return text.Length switch
            {
                < 20 => OverlayPresets.ShortText,
                < 50 => OverlayPresets.MediumText,
                _ => OverlayPresets.LongText
            };
        }

        public static (double x, double y, double w, double h)? FindPlaceholder(
            Page page, string placeholder)
        {
            var letters = page.Letters;
            var buffer = new System.Text.StringBuilder();

            for (int i = 0; i < letters.Count; i++)
            {
                buffer.Append(letters[i].Value);

                if (buffer.ToString().EndsWith(placeholder))
                {
                    int start = i - placeholder.Length + 1;
                    var first = letters[start].GlyphRectangle;
                    var last = letters[i].GlyphRectangle;

                    return (first.Left, first.Bottom, last.Right - first.Left, first.Height);
                }
            }
            return null;
        }

        public static void DrawOverlay(
            XGraphics gfx,
            string text,
            (double x, double y, double w, double h) rect,
            double pageHeight,
            OverlaySettings settings = null)
        {
            settings ??= DetectSettings(text);

            var font = new XFont(FontFamily, settings.FontSize, XFontStyle.Regular,
                new XPdfFontOptions(PdfFontEncoding.Unicode));

            // Auto-scale font
            if (settings.AutoScaleFont)
            {
                double textW = gfx.MeasureString(text, font).Width;
                while (textW > rect.w && font.Size > settings.MinFontSize)
                {
                    font = new XFont(FontFamily, font.Size - 0.5, XFontStyle.Regular,
                        new XPdfFontOptions(PdfFontEncoding.Unicode));
                    textW = gfx.MeasureString(text, font).Width;
                }
            }

            // === COORDINATE CONVERSION ===
            // PdfPig: origin ở bottom-left, Y tăng lên trên
            // PdfSharp: origin ở top-left, Y tăng xuống dưới
            // rect.y là bottom của placeholder trong PdfPig coordinate

            // Chuyển đổi Y từ PdfPig sang PdfSharp
            // Top của placeholder trong PdfSharp = pageHeight - (rect.y + rect.h)
            double placeholderTopY = pageHeight - (rect.y + rect.h);

            // === BOX POSITION ===
            double boxX = rect.x + settings.BoxOffsetX - settings.PaddingLeft;
            double boxY = placeholderTopY + settings.BoxOffsetY - settings.PaddingTop;
            double boxW = rect.w + settings.PaddingLeft + settings.PaddingRight;
            double boxH = rect.h + settings.PaddingTop + settings.PaddingBottom;

            if (settings.DrawBackground)
                gfx.DrawRectangle(XBrushes.White, boxX, boxY, boxW, boxH);

            // === TEXT POSITION ===
            double textWidth = gfx.MeasureString(text, font).Width;

            // Text X theo alignment
            double textX = settings.Align switch
            {
                TextAlign.Left => boxX + settings.PaddingLeft,
                TextAlign.Right => boxX + boxW - textWidth - settings.PaddingRight,
                _ => boxX + (boxW - textWidth) / 2  // Center
            } + settings.TextOffsetX;

            // Text Y: baseline nằm trong box
            // PdfSharp DrawString vẽ từ baseline
            double textY = boxY + settings.PaddingTop + font.GetHeight() * 0.8 + settings.TextOffsetY;

            gfx.DrawString(text, font, XBrushes.Black, new XPoint(textX, textY));
        }
    }

    #endregion
}
