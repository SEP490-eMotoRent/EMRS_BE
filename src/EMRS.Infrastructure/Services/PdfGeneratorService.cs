using EMRS.Application.Abstractions;
using EMRS.Infrastructure.Helper;
using PdfSharpCore.Drawing;
using PdfSharpCore.Fonts;
using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;


namespace EMRS.Infrastructure.Services
{
    public class PdfGeneratorService : IPdfGeneratorService
    {
        private readonly Regex _dottedPlaceholderRegex = new(@"\.{3,}\((\d+)\)\.{3,}", RegexOptions.Compiled);

        // Font TTF có hỗ trợ Unicode (Arial, Roboto, NotoSans,…)

        public byte[] GeneratePdf(byte[] templateBytes, List<string> parameters)
        {
            using var pdfStream = new MemoryStream(templateBytes);
            using var output = new MemoryStream();

            // PdfPig để extract text
            using var document = UglyToad.PdfPig.PdfDocument.Open(pdfStream);

            // PdfSharpCore để ghi overlay
            var pdf = PdfReader.Open(new MemoryStream(templateBytes), PdfDocumentOpenMode.Modify);
            var font = new XFont("LibertinusSerif", 10, XFontStyle.Regular, new XPdfFontOptions(PdfFontEncoding.Unicode));
            for (int pageIndex = 0; pageIndex < document.NumberOfPages; pageIndex++)
            {
                var pigPage = document.GetPage(pageIndex + 1);
                var sharpPage = pdf.Pages[pageIndex];

                string text = pigPage.Text;
                var matches = _dottedPlaceholderRegex.Matches(text);

                if (matches.Count == 0) continue;

                // Tạo XGraphics 1 lần / page
                using (var gfx = XGraphics.FromPdfPage(sharpPage))
                {



                    foreach (Match m in matches)
                    {
                        string number = m.Groups[1].Value;
                        string placeholderFull = m.Value;
                        int idx = int.Parse(number) - 1;

                        // Lấy value, nếu null hoặc empty thì bỏ qua
                        string value = idx < parameters.Count ? parameters[idx] : null;
                        if (string.IsNullOrWhiteSpace(value))
                            continue; // không thay thế, không vẽ overlay

                        var position = FindPlaceholderPosition(pigPage, placeholderFull);
                        if (position == null) continue;

                        DrawOverlay(gfx, font, value, position.Value, sharpPage.Height);
                    }

                }
            }

            pdf.Save(output);
            return output.ToArray();
        }

        /// <summary>
        /// Tìm bounding box placeholder trong PdfPig Page
        /// </summary>
        private (double x, double y, double width, double height)? FindPlaceholderPosition(Page page, string placeholder)
        {
            var letters = page.Letters;
            var buffer = new StringBuilder();
            int start = -1;

            for (int i = 0; i < letters.Count; i++)
            {
                buffer.Append(letters[i].Value);

                if (buffer.ToString().EndsWith(placeholder))
                {
                    start = i - placeholder.Length + 1;

                    var first = letters[start].GlyphRectangle;
                    var last = letters[i].GlyphRectangle;

                    return (
                        first.Left,
                        first.Bottom,
                        last.Right - first.Left,
                        first.Height
                    );
                }
            }

            return null;
        }

        
        private enum TextAlign
        {
            Left,
            Center,
            Right
        }

        private class OverlayLayout
        {
            public double OffsetX = 0;
            public double OffsetY = 0;

            public double PaddingLeft = 2;
            public double PaddingRight = 2;
            public double PaddingTop = 2;
            public double PaddingBottom = 2;

            public TextAlign Align = TextAlign.Left;
        }
        private bool IsVietnameseUpper(char c)
        {
            var cat = CharUnicodeInfo.GetUnicodeCategory(c);

            return cat == UnicodeCategory.UppercaseLetter ||
                   cat == UnicodeCategory.TitlecaseLetter;
        }

        private OverlayLayout GetLayoutForText(string text)
        {
            var letters = text.Where(char.IsLetter).ToList();

            int uppercaseCount = letters.Count(c => IsVietnameseUpper(c));
            bool isAllUpper = letters.Count > 0 && uppercaseCount == letters.Count;

            if (isAllUpper && text.Length > 10)
            {
                return new OverlayLayout
                {
                    OffsetX = 0,
                    OffsetY = 10,
                    PaddingLeft = 20,
                    PaddingRight = 5,
                    PaddingTop = 10,
                    PaddingBottom = 5,
                    Align = TextAlign.Right
                };
            }
            if (text.Length < 20) // text ngắn
            {
                return new OverlayLayout
                {
                    OffsetX = 0,
                    OffsetY = 13,
                    PaddingLeft = 0,
                    PaddingRight = 0,
                    PaddingTop = 10,
                    PaddingBottom = 10,
                    Align = TextAlign.Center
                };
            }
            else if (text.Length < 50) // text trung bình
            {
                return new OverlayLayout
                {
                    OffsetX = 0,
                    OffsetY = 13,
                    PaddingLeft = 5,
                    PaddingRight = 5,
                    PaddingTop = 10,
                    PaddingBottom = 10,
                    Align = TextAlign.Left
                };
            }
            else // text dài
            {
                return new OverlayLayout
                {
                    OffsetX = -20,
                    OffsetY = 20,
                    PaddingLeft = 5,
                    PaddingRight = 5,
                    PaddingTop = 10,
                    PaddingBottom = 10,
                    Align = TextAlign.Center
                };
            }
        }

        private void DrawOverlay(
            XGraphics gfx,
            XFont font,
            string newText,
            (double x, double y, double width, double height) rect,
            double pageHeight)
        {
            // Bạn tùy chỉnh tại đây
            var layout = GetLayoutForText(newText); // <-- thay đổi ở đây
            // --- Tính box overlay --- //
            double boxX = rect.x + layout.OffsetX - layout.PaddingLeft;
            double boxY = pageHeight - rect.y + layout.OffsetY - layout.PaddingTop;

            double boxW = rect.width + layout.PaddingLeft + layout.PaddingRight;
            double boxH = rect.height + layout.PaddingTop + layout.PaddingBottom;

            gfx.DrawRectangle(
                XBrushes.White,
                boxX,
                boxY - boxH,
                boxW,
                boxH
            );

            double textWidth = gfx.MeasureString(newText, font).Width;

            double textX;

            switch (layout.Align)
            {
                case TextAlign.Left:
                    textX = boxX + layout.PaddingLeft;
                    break;

                case TextAlign.Right:
                    textX = boxX + boxW - textWidth - layout.PaddingRight;
                    break;

                default: // Center
                    textX = boxX + (boxW - textWidth) / 2;
                    break;
            }

            double baselineY = boxY - (boxH - font.Height) / 2;

            gfx.DrawString(newText, font, XBrushes.Black, new XPoint(textX, baselineY));
        }




    }
}
