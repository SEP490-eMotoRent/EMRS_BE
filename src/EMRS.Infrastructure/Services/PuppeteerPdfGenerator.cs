using EMRS.Application.Abstractions;
using EMRS.Application.Common;
using HandlebarsDotNet;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Infrastructure.Services;

public class PuppeteerPdfGenerator : IPuppeteerPdfGenerator
{
    private readonly IHandlebars _handlebars;

    public PuppeteerPdfGenerator()
    {
        _handlebars = Handlebars.Create();
    }

    public async Task<byte[]> GeneratePdfAsync(ContractData data)
    {
        const string htmlTemplate = @"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <style>
        body { font-family: 'Times New Roman', serif; font-size: 13pt; line-height: 150%; margin: 0; padding: 20mm 15mm; }
        .center { text-align: center; }
        .bold { font-weight: bold; }
        .underline { text-decoration: underline; }
        .italic { font-style: italic; }
        p { margin: 6pt 0; }
        .indent { margin-left: 36pt; }
        table { width: 100%; margin-top: 30pt; border-collapse: collapse; }
        td { vertical-align: top; }
        .signature { text-align: center; margin-top: 40pt; }
        .spacer { height: 60pt; }
    </style>
</head>
<body>

    <p class='center bold'>CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM</p>
    <p class='center bold'>Độc lập – Tự do – Hạnh phúc</p>
    <br>
    <p class='center bold'>HỢP ĐỒNG THUÊ XE</p>

    <p class='italic'>- Căn cứ Bộ luật Dân sự 2015;</p>
    <p class='italic'>- Căn cứ Luật Giao thông đường bộ 2008;</p>
    <p class='italic'>- Căn cứ vào nhu cầu và khả năng cung ứng của các bên.</p>

    <p class='italic'>Hôm nay, ngày {{ContractDate}} tại {{ContractLocation}}, chúng tôi gồm:</p>

    <p class='bold underline'>1. ĐẠI DIỆN BÊN GIAO XE:</p>
    <p><strong>{{LessorCompany}}</strong></p>
    <p>Đại diện GSM:</p>

    <p>&nbsp;&nbsp;&nbsp;&nbsp;Nhân viên giao xe: <strong>{{LessorDeliveryStaffName}}</strong> – Chức vụ: <strong>{{LessorDeliveryStaffPosition}}</strong></p>
    <p>&nbsp;&nbsp;&nbsp;&nbsp;Địa điểm giao xe: <strong>{{DeliveryLocation}}</strong></p>

    <p class='bold underline'>2. BÊN NHẬN XE:</p>
    <p>&nbsp;&nbsp;&nbsp;&nbsp;Họ tên tài xế: <strong>{{LesseeDriverName}}</strong></p>
    <p>&nbsp;&nbsp;&nbsp;&nbsp;Số CCCD: <strong>{{LesseeDriverId}}</strong></p>
    <p>&nbsp;&nbsp;&nbsp;&nbsp;Số điện thoại: <strong>{{LesseeDriverPhone}}</strong></p>

    <p class='italic'>Hai bên thống nhất ký kết hợp đồng với các điều khoản sau:</p>

    <p class='bold'>Điều 1. Đặc điểm xe thuê</p>
    <p>Nhãn hiệu: {{CarBrand}}&nbsp;&nbsp;&nbsp;&nbsp;Số loại: {{CarModel}}</p>
    <p>Loại xe: {{CarType}}&nbsp;&nbsp;&nbsp;&nbsp;Màu sơn: {{CarColor}}</p>
    <p>Biển số: {{LicensePlate}}&nbsp;&nbsp;&nbsp;&nbsp;Số chỗ: {{Seats}}</p>

    <p class='bold'>Điều 2. Thời hạn thuê</p>
    <p>Thời hạn: <strong>{{RentalDay}} ngày</strong> kể từ ngày ký.</p>



    <p class='bold'>Điều 3. Giá thuê và thanh toán</p>
    <p>Giá thuê: <strong>{{RentalPrice}} VNĐ/ngày</strong></p>

    <p class='bold'>Điều 4. Giao nhận xe</p>
    <p>Bàn giao tại: <strong>{{DeliveryLocation}}</strong></p>

    <p class='bold'>Điều 5. Nghĩa vụ Bên giao xe</p>
    <p>a) Bàn giao xe đúng tình trạng; b) Hỗ trợ sự cố.</p>

    <p class='bold'>Điều 6. Nghĩa vụ Bên nhận xe</p>
    <p>a) Bảo quản xe; b) Thanh toán đúng hạn; c) Chịu trách nhiệm hư hỏng.</p>

    <p class='bold'>Điều 7. Điều khoản chung</p>
    <p>Hợp đồng lập thành 02 bản, mỗi bên giữ 01 bản. Có hiệu lực từ ngày ký.</p>

    <table>
        <tr>
            <td class='signature'>
                <p class='bold'>ĐẠI DIỆN BÊN GIAO XE</p>
                <p class='italic'>(Ký, đóng dấu)</p>
                <div class='spacer'></div>
                <p><strong>{{LessorRepresentativeName}}</strong></p>
                <p>Trưởng Phòng Vận Hành Miền Nam</p>
            </td>
            <td class='signature'>
                <p class='bold'>BÊN NHẬN XE</p>
                <p class='italic'>(Ký tên)</p>
                <div class='spacer'></div>
                <p><strong>{{LesseeDriverName}}</strong></p>
            </td>
        </tr>
    </table>

</body>
</html>";

        var template = _handlebars.Compile(htmlTemplate);
        var html = template(data);

        await new BrowserFetcher().DownloadAsync();
        await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
        {
            Headless = true,
            Args = new[] { "--no-sandbox", "--disable-setuid-sandbox" }
        });
        await using var page = await browser.NewPageAsync();
        await page.SetContentAsync(html);

        return await page.PdfDataAsync(new PdfOptions
        {
            Format = PaperFormat.A4,
            PrintBackground = true,
            MarginOptions = new MarginOptions { Top = "20mm", Bottom = "20mm", Left = "15mm", Right = "15mm" }
        });
    }
}
