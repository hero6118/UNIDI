using Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OfficeOpenXml;
using static Core.Models.Request.InfoCoinRequest;
using System.Drawing;
using OfficeOpenXml.Style;
using Core.Models.Response;
using System.Text.RegularExpressions;

namespace UniDi.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly string CoinMarketCapApiKey = "d2d5659b-3f41-4c6f-9a33-f5b5f037fb24";

        private readonly IHttpContextAccessor _httpContextAccessor;

        public ValuesController(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet("{symbol}")]
        // USING ON COINCAPMARKET
        public async Task<ActionResult<decimal>> GetCryptoPrice(string symbol)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    // Thêm API key vào header của HTTP request
                    httpClient.DefaultRequestHeaders.Add("X-CMC_PRO_API_KEY", CoinMarketCapApiKey);

                    // Đường dẫn API của CoinMarketCap để lấy giá thị trường
                    // string apiUrle = $"https://pro-api.coinmarketcap.com/v1/cryptocurrency/map";   GET ALL INFO COIN

                    // Gửi HTTP GET request đến CoinMarketCap API

                    string apiUrl = $"https://pro-api.coinmarketcap.com/v1/cryptocurrency/quotes/latest?symbol={symbol}&convert=USD";
                    string api = $"https://pro-api.coinmarketcap.com/v2/tools/price-conversion";
                    HttpResponseMessage response = await httpClient.GetAsync(apiUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        // Đọc dữ liệu JSON từ phản hồi
                        string jsonResponse = await response.Content.ReadAsStringAsync();
                        var data = JsonConvert.DeserializeObject<CryptoApiResponse>(jsonResponse);

                        // Kiểm tra xem có dữ liệu về giá không
                        if (data != null && data.Data.ContainsKey(symbol))
                        {
                            // Trả về giá thị trường của tiền điện tử
                            decimal price = data.Data[symbol].Quote.USD.Price;

                            var cryptoData = data.Data[symbol];



                            // Tạo đối tượng CryptoInfo và trả về
                            var cryptoInfo = new CryptoInfo
                            {
                                Symbol = symbol,
                                Price = price,
                            };


                        }
                        return Ok(new { data });

                    }
                }

                // Trả về lỗi nếu không lấy được dữ liệu hoặc có lỗi trong quá trình lấy dữ liệu
                return BadRequest("Unable to retrieve cryptocurrency price.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("[action]")]
        public  ActionResult mail(string Email)
        {
            var content = $"Your project has been Cancelled \n\n " +
                        "\n" +
                       "" +
                       "\n + " +
                        "------------------------------------------------\n" +
                        "";

                    Tool.SendMail("[UNIDI] MESSAGE", content, Email);
            return Ok("send mail success!");
        }

        [HttpGet("[action]")]
        public ActionResult HashPassword(string? thingtohash = "")
        {
            var user = C_User.Auth(Request.Headers["Authorization"]);
            if (user == null)
                return Ok(new { check = false, ms = "Your login session has expired, please login again!", redirect = "/Login" });
            using (var de = new DataEntities())
            {
                var hasher = new PasswordHasher<string>();
                var hash = hasher.HashPassword(user.UserName, thingtohash);

                return Ok(new { check = true, ms = "Hash success!", data = hash });
            }
        }
        [HttpGet("[action]")]

        public ActionResult TestIp(string? sss = "")  // Test check role
        {
            if (User.Identity!.IsAuthenticated)
            {
                if (User.IsInRole("Member"))
                {
                    // Người dùng đã xác thực và thuộc role "Admin"
                    var price = C_BlockChain.CoinMarketCapGetPriceToken("BTCPAY");
                    return Ok("Welcome !");
                }
                else
                {
                    // Người dùng đã xác thực nhưng không thuộc role "Admin"
                    return Unauthorized(new { ms = "You do not have permission to access this resource." });
                }
            }
            else
            {
                // Người dùng chưa Đăng nhập
                return Unauthorized(new { message = "You need to login." });
            }
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            //  var remoteIpAddressV4 = HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString();

            // return Ok(new { check = true, data = remoteIpAddressV4 });
        }

        [HttpGet("[action]")]

        public ActionResult fixproduct(string? sss = "")
        {
            using var de = new DataEntities();
            var product = de.Products.ToList();
            foreach (var item in product)
            {
                var productlist = de.ProductImages.FirstOrDefault(p => p.ProductId == item.Id)?.Link;
                if (item.Image == null)
                    item.Image = productlist;

            }
            de.SaveChanges();
            return Ok(new { check = true, });
        }

        [HttpGet("[action]")]
        public ActionResult ExportToExcel(DateTime? to, DateTime ? from)
        {

            // Tạo một Excel Package
            using var de = new DataEntities();
            var user = C_User.Auth(Request.Headers["Authorization"]);
            if (user == null)
                return Ok(new { check = false, ms = "Your login session has expired, please login again!", redirect = "/Login" });

            var invoice = de.Invoices.AsNoTracking().Where(p=>p.ShopId == user.Id).ToList();
           

            using (var excelPackage = new ExcelPackage())
            {
                // Tạo một Worksheet
                var worksheet = excelPackage.Workbook.Worksheets.Add("Sheet1");

                // Thêm tiêu đề cho các cột
                worksheet.Cells["A2"].Value = "ToTal Invoice";
                worksheet.Cells["B2"].Value = "ToTal money USD";

                var row = worksheet.Row(1);

                // Thiết lập đường viền cho hàng
                row.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                row.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                row.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                row.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
             
                row.Style.Font.Bold = true;

                // Thêm dữ liệu từ danh sách vào Worksheet
                var data = new List<TestClass>
            {
                new TestClass { Colum1e = $"{invoice.Count}", Colum2e = $"{invoice.Sum(p=>p.TotalUSD ) ?? 0}" },
               // new TestClass { Colum1e = "Value3-1e", Colum2e = "Value4-2e" },
                // Thêm dữ liệu từ danh sách LINQ vào đây
                // ...
            };

                worksheet.Cells["A3"].LoadFromCollection(data);

                // Lưu Excel Package vào một mảng byte
                var fileBytes = excelPackage.GetAsByteArray();

                // Trả về tệp Excel như một phản hồi để tải xuống
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "MyExcelFile.xlsx");
            }
        }
        [HttpGet("export-excel")]
        public IActionResult ExportExcels()
        {
            // Đường dẫn đến tệp Excel đã tạo
            var filePath = "MyExcelFile.xlsx";

            // Đọc nội dung tệp Excel và chuyển đổi thành mảng byte
            var fileBytes = System.IO.File.ReadAllBytes(filePath);

            // Xóa tệp Excel sau khi đã xuất
            System.IO.File.Delete(filePath);

            // Trả về tệp Excel như một phản hồi
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "MyExcelFile.xlsx");
        }

        [HttpGet("[action]")]
        public IActionResult ExportToExcelDynamic(string? ShopId)
        {


            // Tạo một danh sách LINQ với các đối tượng không biết trước cấu trúc
            using var de = new DataEntities();
            if (!de.AspNetUsers.Any(p => p.Id == ShopId))
                return NotFound("Không tìm thấy shop.");
            var dataList = de.Invoices.Where(p => p.ShopId == ShopId).ToList(); // Hãy thay thế hàm này bằng lấy dữ liệu từ LINQ của bạn

            if (dataList == null || !dataList.Any())
            {
                // Trả về một thông báo nếu danh sách rỗng
                return NotFound("Danh sách rỗng.");
            }

            // Tạo một Excel Package
            using (var excelPackage = new ExcelPackage())
            {
                // Tạo một Worksheet
                var worksheet = excelPackage.Workbook.Worksheets.Add("Sheet1");

                // Xác định cấu trúc cột dựa trên đối tượng đầu tiên trong danh sách
                var firstItem = dataList.First();
                var columns = firstItem.GetType().GetProperties();

                // Đặt tiêu đề cho các cột từ cấu trúc đối tượng
                for (int i = 0; i < columns.Length; i++)
                {
                    worksheet.Cells[1, i + 1].Value = columns[i].Name;
                }

                // Điền dữ liệu từ danh sách vào Worksheet
                for (int row = 0; row < dataList.Count; row++)
                {
                    for (int col = 0; col < columns.Length; col++)
                    {
                        var propertyValue = columns[col].GetValue(dataList[row]);
                        worksheet.Cells[row + 2, col + 1].Value = propertyValue;
                    }
                }

                // Lưu Excel Package vào một mảng byte
                var fileBytes = excelPackage.GetAsByteArray();

                // Trả về tệp Excel như một phản hồi để tải xuống
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "DynamicData.xlsx");
            }
        }

        [HttpGet("update-and-download-excel")]
        public IActionResult UpdateAndDownloadExcelFile(string? NameFile,DateTime? from, DateTime? To)
        {
            try
            {
                var user = C_User.Auth(Request.Headers["Authorization"]);
                if (user == null)
                    return Ok(new { check = false, ms = "Your login session has expired, please login again!", redirect = "/Login" });

                if (string.IsNullOrEmpty(NameFile))
                    return Ok(new {check = false, ms = "please Enter Name File"});

                using var de = new DataEntities();
                var namebuss = de.BusinessLicenses.AsNoTracking().FirstOrDefault(p => p.ShopId == user.Id);
                if (namebuss == null)
                    return Ok(new {check =false, ms = "Don't found shop"});

                var inv = de.Invoices.AsNoTracking().Where(p => p.ShopId == user.Id
                && (from == null || p.DateCreate >= from)
                && (To == null || p.DateCreate <= To)
                ).ToList();

                var trl = inv.Sum(p => p.TotalUSD) ?? 0; // Giả sử trl là kiểu double
                double roundedValue = Math.Round(trl, 2); // Làm tròn trl đến hai chữ số thập phân và lưu vào roundedValue

              
                // Đường dẫn đến tệp Excel hiện có
                string existingFilePath = "Excelfile/UnidiExcel.xlsx";

                // Tạo một FileStream để đọc tệp Excel
                using (var existingFile = new FileStream(existingFilePath, FileMode.Open, FileAccess.Read))
                {
                    // Tạo một ExcelPackage từ FileStream
                    using (var package = new ExcelPackage(existingFile))
                    {
                        // Lấy bảng tính (Worksheet) cần cập nhật (ví dụ: "Sheet1")
                        var worksheet = package.Workbook.Worksheets["Sheet1"];

                        // Thêm dữ liệu mới vào bảng tính
                        // Ví dụ: Đặt giá trị vào ô A2
                        if(from != null && To == null)
                            worksheet.Cells["G4:L4"].Value = $"THIS IS THE REPORT FROM {from}";
                        else if(To != null && from == null)
                            worksheet.Cells["G4:L4"].Value = $"THIS IS THE REPORT TO {To}";
                        else if (To != null && from != null)
                            worksheet.Cells["G4:L4"].Value = $"THIS IS THE REPORT FROM {from} TO {To}";
                        else
                            worksheet.Cells["G4:L4"].Value = $"THERE ARE ALL REPORT";

                        worksheet.Cells["H5:L5"].Value = $"{user.UserName}";

                        worksheet.Cells["H6:L6"].Value = $"{namebuss.Name}";
                        worksheet.Cells["H13:L13"].Value = $"{inv.Count} ORDER";
                        worksheet.Cells["H14:L14"].Value = $"$ {roundedValue}";
                        // Lưu lại các thay đổi vào một tệp Excel tạm thời
                        string tempFilePath = "path_to_temp_excel_file.xlsx";
                        package.SaveAs(new FileInfo(tempFilePath));

                        // Đóng FileStream và ExcelPackage
                        existingFile.Close();
                        package.Dispose();

                        // Đọc lại tệp đã cập nhật và trả về cho người dùng
                        var updatedFileBytes = System.IO.File.ReadAllBytes(tempFilePath);
                        System.IO.File.Delete(tempFilePath); // Xóa tệp Excel tạm thời


                        var locdau = Tool.LocDau(NameFile);
                        var patem = "^[a-zA-Z0-9]*$";
                        var colCDN = new Regex(patem);
                        var a = colCDN.IsMatch(NameFile);
                        if (locdau != NameFile.ToLower() || NameFile.Contains('.') || !a)
                            return Ok(new {check = false, ms = "filename without accents" });


                        var fileName = NameFile + ".xlsx";

                        return File(updatedFileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                    }
                }
            }
            catch (Exception ex)
            {
                // Xử lý lỗi nếu cần thiết
                return BadRequest($"An error occurred: {ex.Message}");
            }
        }
    }
}
