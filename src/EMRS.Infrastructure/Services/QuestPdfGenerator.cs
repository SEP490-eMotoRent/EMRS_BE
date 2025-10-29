using EMRS.Application.Abstractions;
using EMRS.Application.Common;
using HandlebarsDotNet;
using HTMLQuestPDF.Extensions;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QuestPDF.Infrastructure;
using System.Threading.Tasks;

namespace EMRS.Infrastructure.Services;

public class QuestPdfGenerator : IQuestPdfGenerator
{
    public QuestPdfGenerator()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }
    public async Task<byte[]> GeneratePdfAsync(ContractData data)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontFamily("Times New Roman").FontSize(13));
                page.Content().Column(col =>
                {
                    // --- Header ---
                    col.Item().AlignCenter().Text("CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM").Bold();
                    col.Item().AlignCenter().Text("Độc lập – Tự do – Hạnh phúc").Bold();
                    col.Item().PaddingVertical(10);
                    col.Item().AlignCenter().Text("HỢP ĐỒNG THUÊ XE").Bold().FontSize(15);

                    col.Item().PaddingVertical(10);
                    col.Item().Text("- Căn cứ Bộ luật Dân sự 2015;").Italic();
                    col.Item().Text("- Căn cứ Luật Giao thông đường bộ 2008;").Italic();
                    col.Item().Text("- Căn cứ vào nhu cầu và khả năng cung ứng của các bên.").Italic();

                    col.Item().PaddingVertical(10);
                    col.Item().Text($"Hôm nay, ngày {data.ContractDate} tại {data.ContractLocation}, chúng tôi gồm:").Italic();

                    // --- Section 1: Bên giao xe ---
                    col.Item().PaddingTop(10).Text("1. ĐẠI DIỆN BÊN GIAO XE:").Bold().Underline();
                    col.Item().Text($"{data.LessorCompany}").Bold();
                    col.Item().Text($"Nhân viên giao xe: {data.LessorDeliveryStaffName} – Chức vụ: {data.LessorDeliveryStaffPosition}");
                    col.Item().Text($"Địa điểm giao xe: {data.DeliveryLocationName}");

                    // --- Section 2: Bên nhận xe ---
                    col.Item().PaddingTop(10).Text("2. BÊN NHẬN XE:").Bold().Underline();
                    col.Item().Text($"Họ tên tài xế: {data.LesseeDriverName}");
                    col.Item().Text($"Số CCCD: {data.LesseeDriverId}");
                    col.Item().Text($"Số điện thoại: {data.LesseeDriverPhone}");

                    // --- Agreement intro ---
                    col.Item().PaddingVertical(10).Text("Hai bên thống nhất ký kết hợp đồng với các điều khoản sau:").Italic();

                    // --- Điều 1 ---
                    col.Item().PaddingTop(10).Text("Điều 1. Đặc điểm xe thuê").Bold();
                    col.Item().Text($"Nhãn hiệu: {data.VehicleModelName}    Màu sơn: {data.VehicleColor}");
                    col.Item().Text($"Biển số: {data.LicensePlate}");

                    // --- Điều 2 ---
                    col.Item().PaddingTop(10).Text("Điều 2. Thời hạn thuê").Bold();
                    col.Item().Text($"Ngày bắt đầu thuê: {data.RentalDay}");

                    // --- Điều 3 ---
                    col.Item().PaddingTop(10).Text("Điều 3. Giá thuê và thanh toán").Bold();
                    col.Item().Text($"Giá thuê: {data.RentalPrice} VNĐ/ngày");

                    // --- Điều 4 ---
                    col.Item().PaddingTop(10).Text("Điều 4. Giao nhận xe").Bold();
                    col.Item().Text($"Bàn giao tại: {data.DeliveryLocationName}");

                    // --- Điều 5 ---
                    col.Item().PaddingTop(10).Text("Điều 5. Nghĩa vụ Bên giao xe").Bold();
                    col.Item().Text("a) Bàn giao xe đúng tình trạng;");
                    col.Item().Text("b) Hỗ trợ sự cố khi có yêu cầu từ Bên nhận xe.");

                    // --- Điều 6 ---
                    col.Item().PaddingTop(10).Text("Điều 6. Nghĩa vụ Bên nhận xe").Bold();
                    col.Item().Text("a) Bảo quản xe trong thời gian thuê;");
                    col.Item().Text("b) Thanh toán đầy đủ, đúng hạn;");
                    col.Item().Text("c) Chịu trách nhiệm nếu gây hư hỏng hoặc mất mát tài sản.");

                    // --- Điều 7 ---
                    col.Item().PaddingTop(10).Text("Điều 7. Điều khoản chung").Bold();
                    col.Item().Text("Hợp đồng lập thành 02 bản có giá trị pháp lý như nhau, mỗi bên giữ 01 bản. Có hiệu lực kể từ ngày ký.");

                    // --- Signatures ---
                    col.Item().PaddingTop(40).Row(row =>
                    {
                        row.RelativeColumn().AlignCenter().Column(inner =>
                        {
                            inner.Item().Text("ĐẠI DIỆN BÊN GIAO XE").Bold();
                            inner.Item().Text("(Ký, đóng dấu)").Italic();
                            inner.Item().Height(60);
                            inner.Item().Text(data.LessorDeliveryStaffName).Bold();
                            inner.Item().Text(data.LessorDeliveryStaffPosition);
                        });

                        row.RelativeColumn().AlignCenter().Column(inner =>
                        {
                            inner.Item().Text("BÊN NHẬN XE").Bold();
                            inner.Item().Text("(Ký tên)").Italic();
                            inner.Item().Height(60);
                            inner.Item().Text(data.LesseeDriverName).Bold();
                        });
                    });
                });
            });
        });

        byte[] pdfBytes = document.GeneratePdf();
        return await Task.FromResult(pdfBytes);
    }
}