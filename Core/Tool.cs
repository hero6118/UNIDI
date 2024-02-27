using Core.Models.Response;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Net.Sockets;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace Core
{
    public static class Tool
    {
        static Regex MobileCheck = new Regex(@"(android|bb\d+|meego).+mobile|avantgo|bada\/|blackberry|blazer|compal|elaine|fennec|hiptop|iemobile|ip(hone|od)|iris|kindle|lge |maemo|midp|mmp|mobile.+firefox|netfront|opera m(ob|in)i|palm( os)?|phone|p(ixi|re)\/|plucker|pocket|psp|series(4|6)0|symbian|treo|up\.(browser|link)|vodafone|wap|windows ce|xda|xiino", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);
        static Regex MobileVersionCheck = new Regex(@"1207|6310|6590|3gso|4thp|50[1-6]i|770s|802s|a wa|abac|ac(er|oo|s\-)|ai(ko|rn)|al(av|ca|co)|amoi|an(ex|ny|yw)|aptu|ar(ch|go)|as(te|us)|attw|au(di|\-m|r |s )|avan|be(ck|ll|nq)|bi(lb|rd)|bl(ac|az)|br(e|v)w|bumb|bw\-(n|u)|c55\/|capi|ccwa|cdm\-|cell|chtm|cldc|cmd\-|co(mp|nd)|craw|da(it|ll|ng)|dbte|dc\-s|devi|dica|dmob|do(c|p)o|ds(12|\-d)|el(49|ai)|em(l2|ul)|er(ic|k0)|esl8|ez([4-7]0|os|wa|ze)|fetc|fly(\-|_)|g1 u|g560|gene|gf\-5|g\-mo|go(\.w|od)|gr(ad|un)|haie|hcit|hd\-(m|p|t)|hei\-|hi(pt|ta)|hp( i|ip)|hs\-c|ht(c(\-| |_|a|g|p|s|t)|tp)|hu(aw|tc)|i\-(20|go|ma)|i230|iac( |\-|\/)|ibro|idea|ig01|ikom|im1k|inno|ipaq|iris|ja(t|v)a|jbro|jemu|jigs|kddi|keji|kgt( |\/)|klon|kpt |kwc\-|kyo(c|k)|le(no|xi)|lg( g|\/(k|l|u)|50|54|\-[a-w])|libw|lynx|m1\-w|m3ga|m50\/|ma(te|ui|xo)|mc(01|21|ca)|m\-cr|me(rc|ri)|mi(o8|oa|ts)|mmef|mo(01|02|bi|de|do|t(\-| |o|v)|zz)|mt(50|p1|v )|mwbp|mywa|n10[0-2]|n20[2-3]|n30(0|2)|n50(0|2|5)|n7(0(0|1)|10)|ne((c|m)\-|on|tf|wf|wg|wt)|nok(6|i)|nzph|o2im|op(ti|wv)|oran|owg1|p800|pan(a|d|t)|pdxg|pg(13|\-([1-8]|c))|phil|pire|pl(ay|uc)|pn\-2|po(ck|rt|se)|prox|psio|pt\-g|qa\-a|qc(07|12|21|32|60|\-[2-7]|i\-)|qtek|r380|r600|raks|rim9|ro(ve|zo)|s55\/|sa(ge|ma|mm|ms|ny|va)|sc(01|h\-|oo|p\-)|sdk\/|se(c(\-|0|1)|47|mc|nd|ri)|sgh\-|shar|sie(\-|m)|sk\-0|sl(45|id)|sm(al|ar|b3|it|t5)|so(ft|ny)|sp(01|h\-|v\-|v )|sy(01|mb)|t2(18|50)|t6(00|10|18)|ta(gt|lk)|tcl\-|tdg\-|tel(i|m)|tim\-|t\-mo|to(pl|sh)|ts(70|m\-|m3|m5)|tx\-9|up(\.b|g1|si)|utst|v400|v750|veri|vi(rg|te)|vk(40|5[0-3]|\-v)|vm40|voda|vulc|vx(52|53|60|61|70|80|81|83|85|98)|w3c(\-| )|webc|whit|wi(g |nc|nw)|wmlb|wonu|x700|yas\-|your|zeto|zte\-", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);
        public static bool IsMobile()
        {
            if (HttpContext.Current.Request != null && HttpContext.Current.Request.ServerVariables["HTTP_USER_AGENT"] != null)
            {
                var u = HttpContext.Current.Request.ServerVariables["HTTP_USER_AGENT"].ToString();
                if (u.Length < 4)
                    return false;
                if (MobileCheck.IsMatch(u) || MobileVersionCheck.IsMatch(u.Substring(0, 4)))
                    return true;
            }

            return false;
        }
        public static double NextDouble(double minimum, double maximum)
        {
            Random random = new Random();
            return random.NextDouble() * (maximum - minimum) + minimum;
        }
        public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
            return dt.AddDays(-1 * diff).Date;
        }
        public static string GetHash256(String text, String key)
        {
            // change according to your needs, an UTF8Encoding
            // could be more suitable in certain situations
            ASCIIEncoding encoding = new ASCIIEncoding();

            Byte[] textBytes = encoding.GetBytes(text);
            Byte[] keyBytes = encoding.GetBytes(key);

            Byte[] hashBytes;

            using (var hash = new HMACSHA256(keyBytes))
                hashBytes = hash.ComputeHash(textBytes);

            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }
        public static string GetHash512(String text, String key)
        {
            // change according to your needs, an UTF8Encoding
            // could be more suitable in certain situations
            ASCIIEncoding encoding = new ASCIIEncoding();

            Byte[] textBytes = encoding.GetBytes(text);
            Byte[] keyBytes = encoding.GetBytes(key);

            Byte[] hashBytes;

            using (var hash = new HMACSHA512(keyBytes))
                hashBytes = hash.ComputeHash(textBytes);

            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }
        public static int TotalMonth(DateTime date1, DateTime date2)
        {
            int month;
            if (date1 > date2)
                month = ((date1.Year - date2.Year) * 12) + date1.Month - date2.Month;
            else
                month = ((date2.Year - date1.Year) * 12) + date2.Month - date1.Month;
            return month;
        }
        public static IEnumerable<T> QueryInChunksOf<T>(this IQueryable<T> queryable, int chunkSize)
        {
            return queryable.QueryChunksOfSize(chunkSize).SelectMany(chunk => chunk);
        }
        public static IEnumerable<T[]> QueryChunksOfSize<T>(this IQueryable<T> queryable, int chunkSize)
        {
            int chunkNumber = 0;
            while (true)
            {
                var query = (chunkNumber == 0) ? queryable : queryable.Skip(chunkNumber * chunkSize);
                var chunk = query.Take(chunkSize).ToArray();
                if (chunk.Length == 0)
                    yield break;
                yield return chunk;
                chunkNumber++;
            }
        }
        public static void WriteLog(string tieude, string path)
        {
            FileStream fs = new FileStream(path, FileMode.Append, FileAccess.Write);
            StreamWriter swr = new StreamWriter(fs, Encoding.UTF8);
            swr.WriteLine(tieude);
            swr.WriteLine("");
            swr.Close();
        }
        public static Guid NewGuid(string input)
        {
            Guid id = Guid.Empty;
            using (MD5 md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(Encoding.Default.GetBytes(input));
                id = new Guid(hash);
            }
            return id;
        }
        public static void DeleteFolder(string directory)
        {
            if (Directory.Exists(directory))
            {
                try
                {
                    var dir = new DirectoryInfo(directory);
                    SetAttributesNormal(dir);
                    dir.Delete(true);
                }
                catch
                {
                    try
                    {
                        Directory.Delete(directory, false);
                    }
                    catch
                    {

                    }
                }
            }
        }
        public static void SetAttributesNormal(DirectoryInfo dir)
        {
            foreach (var subDir in dir.GetDirectories())
                SetAttributesNormal(subDir);
            dir.Attributes &= ~FileAttributes.Normal;
            foreach (var file in dir.GetFiles())
            {
                file.Attributes = FileAttributes.Normal;
            }
        }
        public static bool CreateFolder(string nameFolder)
        {
            if (!Directory.Exists(nameFolder))
            {
                Directory.CreateDirectory(nameFolder);
                return true;
            }
            return false;
        }
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        public static long ConvertDateTimeToTimestamp(DateTime value)
        {
            TimeSpan elapsedTime = value - Epoch;
            return (long)elapsedTime.TotalSeconds;
        }

        public static string StripHTML(string input)
        {
            return Regex.Replace(input, "<.*?>", String.Empty);
        }
        public static System.Drawing.Image Base64ToImage(string base64String)
        {
            var split = base64String.Split(',');
            var code = base64String;
            if (split.Length > 1)
            {
                code = split[split.Length - 1];
            }
            byte[] imageBytes = Convert.FromBase64String(code);
            MemoryStream ms = new MemoryStream(imageBytes, 0, imageBytes.Length);
            ms.Write(imageBytes, 0, imageBytes.Length);
            var image = System.Drawing.Image.FromStream(ms, true);
            return image;
        }
        public static async Task<string> ImageToBase64(string path, string type)
        {
            if (type == "file")
            {
                using (System.Drawing.Image image = System.Drawing.Image.FromFile(path))
                {
                    using (MemoryStream m = new MemoryStream())
                    {
                        image.Save(m, image.RawFormat);
                        byte[] imageBytes = m.ToArray();
                        string base64String = Convert.ToBase64String(imageBytes);
                        return base64String;
                    }
                }
            }
            else if (type == "url" && path.StartsWith("http"))
            {
                HttpClient client = new HttpClient();
                var response = await client.GetAsync(path);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var _bytes = await response.Content.ReadAsByteArrayAsync();
                    string base64String = Convert.ToBase64String(_bytes);
                    return base64String;
                }

            }
            return "";
        }

        public static DateTime Curtime()
        {
            //return DateTime..Now;

            //<option value="Morocco Standard Time">(GMT) Casablanca</option>
            //<option value="GMT Standard Time">(GMT) Greenwich Mean Time : Dublin, Edinburgh, Lisbon, London</option>
            //<option value="Greenwich Standard Time">(GMT) Monrovia, Reykjavik</option>
            //<option value="W. Europe Standard Time">(GMT+01:00) Amsterdam, Berlin, Bern, Rome, Stockholm, Vienna</option>
            //<option value="Central Europe Standard Time">(GMT+01:00) Belgrade, Bratislava, Budapest, Ljubljana, Prague</option>
            //<option value="Romance Standard Time">(GMT+01:00) Brussels, Copenhagen, Madrid, Paris</option>
            //<option value="Central European Standard Time">(GMT+01:00) Sarajevo, Skopje, Warsaw, Zagreb</option>
            //<option value="W. Central Africa Standard Time">(GMT+01:00) West Central Africa</option>
            //<option value="Jordan Standard Time">(GMT+02:00) Amman</option>
            //<option value="GTB Standard Time">(GMT+02:00) Athens, Bucharest, Istanbul</option>
            //<option value="Middle East Standard Time">(GMT+02:00) Beirut</option>
            //<option value="Egypt Standard Time">(GMT+02:00) Cairo</option>
            //<option value="South Africa Standard Time">(GMT+02:00) Harare, Pretoria</option>
            //<option value="FLE Standard Time">(GMT+02:00) Helsinki, Kyiv, Riga, Sofia, Tallinn, Vilnius</option>
            //<option value="Israel Standard Time">(GMT+02:00) Jerusalem</option>
            //<option value="E. Europe Standard Time">(GMT+02:00) Minsk</option>
            //<option value="Namibia Standard Time">(GMT+02:00) Windhoek</option>
            //<option value="Arabic Standard Time">(GMT+03:00) Baghdad</option>
            //<option value="Arab Standard Time">(GMT+03:00) Kuwait, Riyadh</option>
            //<option value="Russian Standard Time">(GMT+03:00) Moscow, St. Petersburg, Volgograd</option>
            //<option value="E. Africa Standard Time">(GMT+03:00) Nairobi</option>
            //<option value="Georgian Standard Time">(GMT+03:00) Tbilisi</option>
            //<option value="Iran Standard Time">(GMT+03:30) Tehran</option>
            //<option value="Arabian Standard Time">(GMT+04:00) Abu Dhabi, Muscat</option>
            //<option value="Azerbaijan Standard Time">(GMT+04:00) Baku</option>
            //<option value="Mauritius Standard Time">(GMT+04:00) Port Louis</option>
            //<option value="Caucasus Standard Time">(GMT+04:00) Yerevan</option>
            //<option value="Afghanistan Standard Time">(GMT+04:30) Kabul</option>
            //<option value="Ekaterinburg Standard Time">(GMT+05:00) Ekaterinburg</option>
            //<option value="Pakistan Standard Time">(GMT+05:00) Islamabad, Karachi</option>
            //<option value="West Asia Standard Time">(GMT+05:00) Tashkent</option>
            //<option value="India Standard Time">(GMT+05:30) Chennai, Kolkata, Mumbai, New Delhi</option>
            //<option value="Sri Lanka Standard Time">(GMT+05:30) Sri Jayawardenepura</option>
            //<option value="Nepal Standard Time">(GMT+05:45) Kathmandu</option>
            //<option value="N. Central Asia Standard Time">(GMT+06:00) Almaty, Novosibirsk</option>
            //<option value="Central Asia Standard Time">(GMT+06:00) Astana, Dhaka</option>
            //<option value="Myanmar Standard Time">(GMT+06:30) Yangon (Rangoon)</option>
            //<option value="SE Asia Standard Time">(GMT+07:00) Bangkok, Hanoi, Jakarta</option>
            //<option value="North Asia Standard Time">(GMT+07:00) Krasnoyarsk</option>
            //<option value="China Standard Time">(GMT+08:00) Beijing, Chongqing, Hong Kong, Urumqi</option>
            //<option value="North Asia East Standard Time">(GMT+08:00) Irkutsk, Ulaan Bataar</option>
            //<option value="Singapore Standard Time">(GMT+08:00) Kuala Lumpur, Singapore</option>
            //<option value="W. Australia Standard Time">(GMT+08:00) Perth</option>
            //<option value="Taipei Standard Time">(GMT+08:00) Taipei</option>
            //<option value="Tokyo Standard Time">(GMT+09:00) Osaka, Sapporo, Tokyo</option>
            //<option value="Korea Standard Time">(GMT+09:00) Seoul</option>
            //<option value="Yakutsk Standard Time">(GMT+09:00) Yakutsk</option>
            //<option value="Cen. Australia Standard Time">(GMT+09:30) Adelaide</option>
            //<option value="AUS Central Standard Time">(GMT+09:30) Darwin</option>
            //<option value="E. Australia Standard Time">(GMT+10:00) Brisbane</option>
            //<option value="AUS Eastern Standard Time">(GMT+10:00) Canberra, Melbourne, Sydney</option>
            //<option value="West Pacific Standard Time">(GMT+10:00) Guam, Port Moresby</option>
            //<option value="Tasmania Standard Time">(GMT+10:00) Hobart</option>
            //<option value="Vladivostok Standard Time">(GMT+10:00) Vladivostok</option>
            //<option value="Central Pacific Standard Time">(GMT+11:00) Magadan, Solomon Is., New Caledonia</option>
            //<option value="New Zealand Standard Time">(GMT+12:00) Auckland, Wellington</option>
            //<option value="Fiji Standard Time">(GMT+12:00) Fiji, Kamchatka, Marshall Is.</option>
            //<option value="Tonga Standard Time">(GMT+13:00) Nuku'alofa</option>
            //<option value="Azores Standard Time">(GMT-01:00) Azores</option>
            //<option value="Cape Verde Standard Time">(GMT-01:00) Cape Verde Is.</option>
            //<option value="Mid-Atlantic Standard Time">(GMT-02:00) Mid-Atlantic</option>
            //<option value="E. South America Standard Time">(GMT-03:00) Brasilia</option>
            //<option value="Argentina Standard Time">(GMT-03:00) Buenos Aires</option>
            //<option value="SA Eastern Standard Time">(GMT-03:00) Georgetown</option>
            //<option value="Greenland Standard Time">(GMT-03:00) Greenland</option>
            //<option value="Montevideo Standard Time">(GMT-03:00) Montevideo</option>
            //<option value="Newfoundland Standard Time">(GMT-03:30) Newfoundland</option>
            //<option value="Atlantic Standard Time">(GMT-04:00) Atlantic Time (Canada)</option>
            //<option value="SA Western Standard Time">(GMT-04:00) La Paz</option>
            //<option value="Central Brazilian Standard Time">(GMT-04:00) Manaus</option>
            //<option value="Pacific SA Standard Time">(GMT-04:00) Santiago</option>
            //<option value="Venezuela Standard Time">(GMT-04:30) Caracas</option>
            //<option value="SA Pacific Standard Time">(GMT-05:00) Bogota, Lima, Quito, Rio Branco</option>
            //<option value="Eastern Standard Time">(GMT-05:00) Eastern Time (US & Canada)</option>
            //<option value="US Eastern Standard Time">(GMT-05:00) Indiana (East)</option>
            //<option value="Central America Standard Time">(GMT-06:00) Central America</option>
            //<option value="Central Standard Time">(GMT-06:00) Central Time (US & Canada)</option>
            //<option value="Central Standard Time (Mexico)">(GMT-06:00) Guadalajara, Mexico City, Monterrey</option>
            //<option value="Canada Central Standard Time">(GMT-06:00) Saskatchewan</option>
            //<option value="US Mountain Standard Time">(GMT-07:00) Arizona</option>
            //<option value="Mountain Standard Time (Mexico)">(GMT-07:00) Chihuahua, La Paz, Mazatlan</option>
            //<option value="Mountain Standard Time">(GMT-07:00) Mountain Time (US & Canada)</option>
            //<option value="Pacific Standard Time">(GMT-08:00) Pacific Time (US & Canada)</option>
            //<option value="Pacific Standard Time (Mexico)">(GMT-08:00) Tijuana, Baja California</option>
            //<option value="Alaskan Standard Time">(GMT-09:00) Alaska</option>
            //<option value="Hawaiian Standard Time">(GMT-10:00) Hawaii</option>
            //<option value="Samoa Standard Time">(GMT-11:00) Midway Island, Samoa</option>
            //<option value="Dateline Standard Time">(GMT-12:00) International Date Line West</option>

            string InputConcatenate = "North Asia East Standard Time"; // ("SE Asia Standard Time");
            DateTime thisTime = DateTime.Now;
            TimeZoneInfo tst = TimeZoneInfo.FindSystemTimeZoneById(InputConcatenate);
            DateTime tstTime = TimeZoneInfo.ConvertTime(thisTime, TimeZoneInfo.Local, tst);
            return tstTime;
        }
        public static string GetRandomString(int maxSize)
        {
            var chars = "ABCDEFGHKMNPQRSTUVWXYZ123456789".ToCharArray();
            byte[] data = new byte[1];
            using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
            {
                crypto.GetNonZeroBytes(data);
                data = new byte[maxSize];
                crypto.GetNonZeroBytes(data);
            }
            StringBuilder result = new StringBuilder(maxSize);
            foreach (byte b in data)
            {
                result.Append(chars[b % (chars.Length)]);
            }
            return result.ToString();
        }
        public static string GetRandomNumber(int maxSize)
        {
            var chars = "0123456789".ToCharArray();
            byte[] data = new byte[1];
            using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
            {
                crypto.GetNonZeroBytes(data);
                data = new byte[maxSize];
                crypto.GetNonZeroBytes(data);
            }
            StringBuilder result = new StringBuilder(maxSize);
            foreach (byte b in data)
            {
                result.Append(chars[b % (chars.Length)]);
            }
            return result.ToString();
        }

        public static string SplitString(string s, int length)
        {
            if (String.IsNullOrEmpty(s))
            {
                //throw new ArgumentNullException(s);
                return "";
            }

            var words = s.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (words[0].Length > length)
                throw new ArgumentException("The first word is longer than the string to be cut");
            var sb = new StringBuilder();
            foreach (var word in words)
            {
                if ((sb + word).Length > length)
                    return string.Format("{0}...", sb.ToString().TrimEnd(' '));
                sb.Append(word + " ");
            }
            return string.Format("{0}", sb.ToString().TrimEnd(' '));
        }

        public static void RemoveFileIfExist(string filePath)
        {
            if (!filePath.Contains("http") && System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }
        public static void RemoveFileIfExist(string image, int afterSecond)
        {
            System.Threading.Thread.Sleep(afterSecond * 1000);
            RemoveFileIfExist(image);
        }

        public static string DeleteTag(string kq, string chuoi)
        {
            var list = kq.Split(',').ToList();
            var abc = "";
            foreach (var item in list)
            {
                if (!item.Contains(chuoi))
                {
                    //list.Remove(item);
                    abc += item + ",";
                }
            }
            abc = abc.Substring(0, (abc.Length - 1));
            //kq = kq.Replace(chuoi, "").Replace(" ","-").Replace(","," ").Trim().Replace(" ",",").Replace("-"," ");
            return abc;
        }

        private static readonly string[] VietNamChar = new string[]
        {
            " ",
            "`~!@#$%^&*()?{}[]+=-–<>;“”‘’'_|/,\"\\"
        };
        public static string LocDau(string str)
        {

            if (string.IsNullOrEmpty(str)) return "";

            str = str.Normalize(NormalizationForm.FormD);
            StringBuilder sb = new StringBuilder();
            for (int ich = 0; ich < str.Length; ich++)
            {
                System.Globalization.UnicodeCategory uc = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(str[ich]);
                if (uc != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(str[ich]);
                }
            }
            sb = sb.Replace('Đ', 'D');
            sb = sb.Replace('đ', 'd');
            sb = sb.Replace(".", "");

            for (int i = 1; i < VietNamChar.Length; i++)
            {
                for (int j = 0; j < VietNamChar[i].Length; j++)
                {
                    sb = sb.Replace(VietNamChar[i][j], VietNamChar[0][i - 1]);
                }

            }
            return sb.ToString().ToLower().Trim();
        }
        public static string CharToUnicode(string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;
            var splitted = Regex.Split(str, @"\\u([a-fA-F\d]{4})");
            string outString = "";
            foreach (var s in splitted)
            {
                try
                {
                    if (s.Length == 4)
                    {
                        var decoded = ((char)Convert.ToUInt16(s, 16)).ToString();
                        outString += decoded;
                    }
                    else
                    {
                        outString += s;
                    }
                }
                catch
                {
                    outString += s;
                }
            }
            return outString;
        }

        public static DateTime StringToDateTime(string ngay, char split)
        {
            var chuoi = ngay.Split(split);
            return new DateTime(Convert.ToInt32(chuoi[2]), Convert.ToInt32(chuoi[0]), Convert.ToInt32(chuoi[1]));
        }

        public static DateTime StringToDateTime_End(string ngay, char split)
        {
            var chuoi = ngay.Split(split);
            return new DateTime(Convert.ToInt32(chuoi[2]), Convert.ToInt32(chuoi[0]), Convert.ToInt32(chuoi[1]), 23, 59, 59);
        }

        public static IEnumerable<string> CheckLinkImage(string path)
        {
            var pattern = Regex.Matches(path, "<img.+?src=[\"'](.+?)[\"'].+?>", RegexOptions.IgnoreCase).Cast<Match>().Select(x => x.Groups[0].Value).ToArray();
            foreach (var item in pattern)
            {
                var kq = item.Split('"');
                yield return kq[1];
            }
        }
        public static bool CheckEmailFormat(string email)
        {
            if (string.IsNullOrEmpty(email))
                return false;
            else
            {
                var regex = new Regex(@"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*");
                return regex.IsMatch(email) && !email.EndsWith(".");
            }
        }

        public static void CreatNewFolder(string filePath)
        {
            if (!Directory.Exists(filePath))
                Directory.CreateDirectory(filePath);
        }
        public static void CreatNewFile(string filePath, string content)
        {
            if (!System.IO.File.Exists(filePath))
            {
                using (FileStream fs = System.IO.File.Create(filePath))
                {
                    byte[] info = new UTF8Encoding(true).GetBytes(content);
                    fs.Write(info, 0, info.Length);
                }
            }
        }
        public static string LocDauUrl(string str)
        {
            if (!string.IsNullOrEmpty(str))
                str = str.Replace(" - ", " ").Replace(" & ", " ").Replace(" : ", " ").Replace("Đ", "D").Replace("đ", "d");
            else return "";
            // Chuyển UNICODE
            str = str.Normalize(NormalizationForm.FormD);
            StringBuilder sb = new StringBuilder();
            for (int ich = 0; ich < str.Length; ich++)
            {
                System.Globalization.UnicodeCategory uc = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(str[ich]);
                if (uc != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(str[ich]);
                }
            }

            //Thay thế và lọc dấu từng char      
            for (int i = 1; i < VietNamChar.Length; i++)
            {
                for (int j = 0; j < VietNamChar[i].Length; j++)
                {
                    sb = sb.Replace(VietNamChar[i][j], VietNamChar[0][i - 1]);
                }

            }
            return sb.ToString().ToLower().Trim()
                .Replace(" ", "-")
                .Replace(" ", "-") // có khoảng trắng đặc biệt
                .Replace("--", "-")
                .Replace("ð", "d");
        }
        public static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
        // https://gist.github.com/odan/138dbd41a0c5ef43cbf529b03d814d7c
        public static string passEncrypt = "uto!%)(";
        public static string EncryptString(string plainText, string pass = null)
        {
            string password = passEncrypt;
            if (!string.IsNullOrEmpty(pass))
                password = pass;
            // Create sha256 hash
            SHA256 mySHA256 = SHA256.Create();
            byte[] key = mySHA256.ComputeHash(Encoding.ASCII.GetBytes(password));
            // Create secret IV
            byte[] iv = new byte[16] { 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0 };

            // Instantiate a new Aes object to perform string symmetric encryption
            Aes encryptor = Aes.Create();

            encryptor.Mode = CipherMode.CBC;
            //encryptor.KeySize = 256;
            //encryptor.BlockSize = 128;
            //encryptor.Padding = PaddingMode.Zeros;

            // Set key and IV
            encryptor.Key = key;
            encryptor.IV = iv;

            // Instantiate a new MemoryStream object to contain the encrypted bytes
            MemoryStream memoryStream = new MemoryStream();

            // Instantiate a new encryptor from our Aes object
            ICryptoTransform aesEncryptor = encryptor.CreateEncryptor();

            // Instantiate a new CryptoStream object to process the data and write it to the 
            // memory stream
            CryptoStream cryptoStream = new CryptoStream(memoryStream, aesEncryptor, CryptoStreamMode.Write);

            // Convert the plainText string into a byte array
            byte[] plainBytes = Encoding.ASCII.GetBytes(plainText);

            // Encrypt the input plaintext string
            cryptoStream.Write(plainBytes, 0, plainBytes.Length);

            // Complete the encryption process
            cryptoStream.FlushFinalBlock();

            // Convert the encrypted data from a MemoryStream to a byte array
            byte[] cipherBytes = memoryStream.ToArray();

            // Close both the MemoryStream and the CryptoStream
            memoryStream.Close();
            cryptoStream.Close();

            // Convert the encrypted byte array to a base64 encoded string
            string cipherText = Convert.ToBase64String(cipherBytes, 0, cipherBytes.Length);

            // Return the encrypted data as a string
            return cipherText;
        }
        public static string DecryptString(string cipherText, string pass = null)
        {
            string password = passEncrypt;
            if (!string.IsNullOrEmpty(pass))
                password = pass;

            // Create sha256 hash
            SHA256 mySHA256 = SHA256.Create();
            byte[] key = mySHA256.ComputeHash(Encoding.ASCII.GetBytes(password));
            // Create secret IV
            byte[] iv = new byte[16] { 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0 };

            // Instantiate a new Aes object to perform string symmetric encryption
            Aes encryptor = Aes.Create();

            encryptor.Mode = CipherMode.CBC;
            //encryptor.KeySize = 256;
            //encryptor.BlockSize = 128;
            //encryptor.Padding = PaddingMode.Zeros;

            // Set key and IV
            encryptor.Key = key;
            encryptor.IV = iv;

            // Instantiate a new MemoryStream object to contain the encrypted bytes
            MemoryStream memoryStream = new MemoryStream();

            // Instantiate a new encryptor from our Aes object
            ICryptoTransform aesDecryptor = encryptor.CreateDecryptor();

            // Instantiate a new CryptoStream object to process the data and write it to the 
            // memory stream
            CryptoStream cryptoStream = new CryptoStream(memoryStream, aesDecryptor, CryptoStreamMode.Write);

            // Will contain decrypted plaintext
            string plainText = String.Empty;

            try
            {
                // Convert the ciphertext string into a byte array
                byte[] cipherBytes = Convert.FromBase64String(cipherText);

                // Decrypt the input ciphertext string
                cryptoStream.Write(cipherBytes, 0, cipherBytes.Length);

                // Complete the decryption process
                cryptoStream.FlushFinalBlock();

                // Convert the decrypted data from a MemoryStream to a byte array
                byte[] plainBytes = memoryStream.ToArray();

                // Convert the encrypted byte array to a base64 encoded string
                plainText = Encoding.ASCII.GetString(plainBytes, 0, plainBytes.Length);
            }
            finally
            {
                // Close both the MemoryStream and the CryptoStream
                try
                {
                    memoryStream.Close();
                    cryptoStream.Close();
                }
                catch
                {
                }
            }

            // Return the encrypted data as a string
            return plainText;
        }
        public static string EnHEX(string text)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(text);
            string hex = BitConverter.ToString(bytes);
            return hex.Replace("-", "");
        }
        public static string DeHEX(string hex)
        {
            hex = hex.Replace("-", "");
            byte[] raw = new byte[hex.Length / 2];
            for (int i = 0; i < raw.Length; i++)
            {
                raw[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
            string s = Encoding.ASCII.GetString(raw, 0, raw.Length);
            return s;
        }
        public static string DoubleToHex(double value)
        {
            if (value == 0 || double.IsInfinity(value) || double.IsNaN(value))
                return value.ToString();

            StringBuilder hex = new StringBuilder();

            long bytes = BitConverter.ToInt64(BitConverter.GetBytes(value), 0);

            bool negative = bytes < 0;
            bytes &= long.MaxValue;

            int exp = ((int)(bytes >> 52) & 0x7FF) - 0x3FF;
            bytes = (bytes & 0xFFFFFFFFFFFFF) | 0x10000000000000;

            if (exp < 0)
            {
                exp = -exp - 1;
                hex.Append("0.").Append('0', exp / 4);
                bytes <<= 3 - exp % 4;
                hex.Append(bytes.ToString("x").TrimEnd('0'));
            }
            else
            {
                bytes <<= (exp % 4);
                exp &= ~3;
                hex.Append(bytes.ToString("x"));
                if (exp >= 52)
                    hex.Append('0', (exp - 52) / 4);
                else
                {
                    hex.Insert(exp / 4 + 1, '.');
                    hex = new StringBuilder(hex.ToString().TrimEnd('0').TrimEnd('.'));
                }
            }
            if (negative) hex.Insert(0, '-');
            return hex.ToString();
        }
        public static double HexToDouble(string hex)
        {
            uint num = uint.Parse(hex, System.Globalization.NumberStyles.AllowHexSpecifier);
            byte[] floatVals = BitConverter.GetBytes(num);
            double f = BitConverter.ToSingle(floatVals, 0);
            return f;
        }
        public static string StringToHex(string value)
        {
            byte[] ba = Encoding.UTF8.GetBytes(value);
            var hexString = BitConverter.ToString(ba);
            hexString = hexString.Replace("-", "");
            return hexString;
        }
        public static DateTime ConvertTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }
        public static string FormatNumber(double num)
        {
            if (num >= 1000 || num <= -1000)
            {
                return string.Format("{0:n0}", num);
            }
            else
            {
                var split = Math.Round(num, 10).ToString().Split('.');
                if (split.Length > 1)
                {
                    var str = split[1].Length;
                    return string.Format("{0:n" + str + "}", num);
                }
                return split[0];
            }
        }
        public static string FormatMoney(double num)
        {
            string text;
            if (num >= 1000 || num <= -1000)
            {
                if (num > 0)
                {
                    text = "+ " + string.Format("{0:n0}", num);
                }
                else
                {
                    text = "- " + string.Format("{0:n0}", num * -1);
                }
            }
            else
            {
                if (num.ToString().Contains("E"))
                {
                    var split2 = num.ToString().Split('E');
                    var dec = (int.Parse(split2[1]) * -1) - 1;
                    var stringDec = "0.";
                    var first = double.Parse(split2[0]);
                    if (split2[0].Contains("."))
                    {
                        first *= 10;
                    }
                    if (first < 0)
                    {
                        first *= -1;
                    }
                    for (int i = 0; i < dec; i++)
                    {
                        stringDec += "0";
                    }
                    text = stringDec + first;
                    if (num > 0)
                    {
                        text = "+ " + text;
                    }
                    else
                    {
                        text = "- " + text;
                    }
                }
                else
                {
                    if (num > 0)
                    {
                        var split = Math.Round(num, 8).ToString().Split('.');
                        if (split.Length > 1)
                        {
                            var str = split[1].Length;
                            text = "+ " + string.Format("{0:n" + str + "}", num);
                        }
                        else
                        {
                            text = "+ " + split[0];
                        }
                    }
                    else if (num < 0)
                    {
                        num = Math.Round(num * -1, 8);
                        var split = num.ToString().Split('.');
                        if (split.Length > 1)
                        {
                            var str = split[1].Length;
                            text = "- " + string.Format("{0:n" + str + "}", num);
                        }
                        else
                        {
                            text = "- " + split[0];
                        }
                    }
                    else
                    {
                        text = "0";
                    }
                }

            }
            return text;
        }
        //public static StatusRequest SendMail(string title, string detail, string email, string fullName = "", string userName = "", string link = "", string amount = "", string walletAddress = "", string ip = "", string time = "")
        //{
        //    // check expiry email
        //    //var exp = C_Service.Get_Service_By_Type(EnumService.Mail);
        //    //if (exp.DateCreate == null || exp.ExpiryDate < Tool.Curtime())
        //    //    return new TempResultJson
        //    //    {
        //    //        check = false,
        //    //        ms = "Email service is under maintenance"
        //    //    };
        //    detail = detail
        //    .Replace("{fullname}", fullName)
        //    .Replace("{user}", userName)
        //    .Replace("{ip}", ip)
        //    .Replace("{time}", time)
        //    .Replace("{amount}", amount)
        //    .Replace("{walletaddress}", walletAddress)
        //    .Replace("{link}", link);

        //    try
        //    {
        //        var de = DataEntities.Create(host);
        //        var config = de.Configs.FirstOrDefault();
        //        SmtpClient smtp = new SmtpClient
        //        {
        //            Host = "smtp.gmail.com",
        //            Port = 587,
        //            EnableSsl = true,
        //            DeliveryMethod = SmtpDeliveryMethod.Network,
        //            UseDefaultCredentials = false,
        //            Credentials = new NetworkCredential(config.GmailAccount, config.GmailPassword)
        //        };

        //        MailMessage msg = new MailMessage(config.GmailAccount, email)
        //        {
        //            Subject = title,
        //            Body = detail,
        //            BodyEncoding = Encoding.UTF8,
        //            SubjectEncoding = Encoding.UTF8
        //        };
        //        AlternateView htmlView = AlternateView.CreateAlternateViewFromString(detail);
        //        htmlView.ContentType = new System.Net.Mime.ContentType("text/html");
        //        msg.AlternateViews.Add(htmlView);
        //        smtp.EnableSsl = true;
        //        smtp.Send(msg);
        //        return new StatusRequest
        //        {
        //            Status = true,
        //            Message = "Success"
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        return new StatusRequest
        //        {
        //            Status = false,
        //            Message = ex.Message
        //        };
        //    }
        //}
        private static bool RemoteServerCertificateValidationCallback(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            //Console.WriteLine(certificate);
            return true;
        }
        public static DefaultResponse SendMail(string title, string detail, string email)
        {
            //return null;
            try
            {
                using (var de = new DataEntities())
                {
                    var config = de.Configs.AsNoTracking().FirstOrDefault();
                    if (config == null || string.IsNullOrEmpty(config.EmailAccount))
                        return new DefaultResponse { Status = false, Message = "The system has not configured Email" };

                    MailMessage EmailMsg = new MailMessage
                    {
                        From = new MailAddress(config.EmailAccount, config.CompanyName),
                        Subject = title,
                        Body = detail,
                        IsBodyHtml = true,
                        Priority = MailPriority.Normal
                    };
                    EmailMsg.To.Add(new MailAddress(email));
                    //EmailMsg.ReplyToList.Add("info@domain.com");
                    ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(RemoteServerCertificateValidationCallback);
                    //var pass = DecryptString(config.GmailPassword);
                    SmtpClient SMTP = new SmtpClient
                    {
                        Host = config.SMTPServer,
                        Port = (int)config.SMTPPort,
                        EnableSsl = true,
                        DeliveryMethod = SmtpDeliveryMethod.Network,
                        UseDefaultCredentials = false,
                        Credentials = new NetworkCredential(config.EmailAccount, config.EmailPassword)
                    };

                    SMTP.Send(EmailMsg);

                    return new DefaultResponse
                    {
                        Status = true,
                        Message = "Success"
                    };
                }
            }
            catch (Exception ex)
            {
                //Task.Run(() => SendTelegram("Lỗi gửi mail: " + email + "\nTiêu đề: " + title + "\nNội dung: " + detail + "\n" + ex.Message + "\n" + ex.InnerException?.Message + "\n" + ex.InnerException?.InnerException?.Message, "error"));
                return new DefaultResponse
                {
                    Status = false,
                    Message = ex.Message
                };
            }
        }
        private static string Chu(string gNumber)
        {
            string result = "";
            switch (gNumber)
            {
                case "0":
                    result = "không";
                    break;
                case "1":
                    result = "một";
                    break;
                case "2":
                    result = "hai";
                    break;
                case "3":
                    result = "ba";
                    break;
                case "4":
                    result = "bốn";
                    break;
                case "5":
                    result = "năm";
                    break;
                case "6":
                    result = "sáu";
                    break;
                case "7":
                    result = "bảy";
                    break;
                case "8":
                    result = "tám";
                    break;
                case "9":
                    result = "chín";
                    break;
            }
            return result;
        }
        private static string Donvi(string so)
        {
            string Kdonvi = "";

            if (so.Equals("1"))
                Kdonvi = "";
            if (so.Equals("2"))
                Kdonvi = "nghìn";
            if (so.Equals("3"))
                Kdonvi = "triệu";
            if (so.Equals("4"))
                Kdonvi = "tỷ";
            if (so.Equals("5"))
                Kdonvi = "nghìn tỷ";
            if (so.Equals("6"))
                Kdonvi = "triệu tỷ";
            if (so.Equals("7"))
                Kdonvi = "tỷ tỷ";

            return Kdonvi;
        }
        private static string Tach(string tach3)
        {
            string Ktach = "";
            if (tach3.Equals("000"))
                return "";
            if (tach3.Length == 3)
            {
                string tr = tach3.Trim().Substring(0, 1).ToString().Trim();
                string ch = tach3.Trim().Substring(1, 1).ToString().Trim();
                string dv = tach3.Trim().Substring(2, 1).ToString().Trim();
                if (tr.Equals("0") && ch.Equals("0"))
                    Ktach = " không trăm lẻ " + Chu(dv.ToString().Trim()) + " ";
                if (!tr.Equals("0") && ch.Equals("0") && dv.Equals("0"))
                    Ktach = Chu(tr.ToString().Trim()).Trim() + " trăm ";
                if (!tr.Equals("0") && ch.Equals("0") && !dv.Equals("0"))
                    Ktach = Chu(tr.ToString().Trim()).Trim() + " trăm lẻ " + Chu(dv.Trim()).Trim() + " ";
                if (tr.Equals("0") && Convert.ToInt32(ch) > 1 && Convert.ToInt32(dv) > 0 && !dv.Equals("5"))
                    Ktach = " không trăm " + Chu(ch.Trim()).Trim() + " mươi " + Chu(dv.Trim()).Trim() + " ";
                if (tr.Equals("0") && Convert.ToInt32(ch) > 1 && dv.Equals("0"))
                    Ktach = " không trăm " + Chu(ch.Trim()).Trim() + " mươi ";
                if (tr.Equals("0") && Convert.ToInt32(ch) > 1 && dv.Equals("5"))
                    Ktach = " không trăm " + Chu(ch.Trim()).Trim() + " mươi lăm ";
                if (tr.Equals("0") && ch.Equals("1") && Convert.ToInt32(dv) > 0 && !dv.Equals("5"))
                    Ktach = " không trăm mười " + Chu(dv.Trim()).Trim() + " ";
                if (tr.Equals("0") && ch.Equals("1") && dv.Equals("0"))
                    Ktach = " không trăm mười ";
                if (tr.Equals("0") && ch.Equals("1") && dv.Equals("5"))
                    Ktach = " không trăm mười lăm ";
                if (Convert.ToInt32(tr) > 0 && Convert.ToInt32(ch) > 1 && Convert.ToInt32(dv) > 0 && !dv.Equals("5"))
                    Ktach = Chu(tr.Trim()).Trim() + " trăm " + Chu(ch.Trim()).Trim() + " mươi " + Chu(dv.Trim()).Trim() + " ";
                if (Convert.ToInt32(tr) > 0 && Convert.ToInt32(ch) > 1 && dv.Equals("0"))
                    Ktach = Chu(tr.Trim()).Trim() + " trăm " + Chu(ch.Trim()).Trim() + " mươi ";
                if (Convert.ToInt32(tr) > 0 && Convert.ToInt32(ch) > 1 && dv.Equals("5"))
                    Ktach = Chu(tr.Trim()).Trim() + " trăm " + Chu(ch.Trim()).Trim() + " mươi lăm ";
                if (Convert.ToInt32(tr) > 0 && ch.Equals("1") && Convert.ToInt32(dv) > 0 && !dv.Equals("5"))
                    Ktach = Chu(tr.Trim()).Trim() + " trăm mười " + Chu(dv.Trim()).Trim() + " ";

                if (Convert.ToInt32(tr) > 0 && ch.Equals("1") && dv.Equals("0"))
                    Ktach = Chu(tr.Trim()).Trim() + " trăm mười ";
                if (Convert.ToInt32(tr) > 0 && ch.Equals("1") && dv.Equals("5"))
                    Ktach = Chu(tr.Trim()).Trim() + " trăm mười lăm ";

            }


            return Ktach;

        }
        private static string Replace_special_word(string chuoi)
        {
            chuoi = chuoi.Replace("không mươi không ", "");
            chuoi = chuoi.Replace("không mươi", "lẻ");
            chuoi = chuoi.Replace("i không", "i");
            chuoi = chuoi.Replace("i năm", "i lăm");
            chuoi = chuoi.Replace("một mươi", "mười");
            chuoi = chuoi.Replace("mươi một", "mươi mốt");
            return chuoi;
        }
        public static string So_chu(double gNum)
        {
            if (gNum == 0)
                return "Không";

            string lso_chu = "";
            string tach_mod = "";
            string tach_conlai = "";
            double Num = Math.Round(gNum, 0);
            string gN = Convert.ToString(Num);
            int m = Convert.ToInt32(gN.Length / 3);
            int mod = gN.Length - m * 3;
            //string dau = "[+]";

            //// Dau [+ , - ]
            //if (gNum < 0)
            //    dau = "[-]";
            //dau = "";

            // Tach hang lon nhat
            if (mod.Equals(1))
                tach_mod = "00" + Convert.ToString(Num.ToString().Trim().Substring(0, 1)).Trim();
            if (mod.Equals(2))
                tach_mod = "0" + Convert.ToString(Num.ToString().Trim().Substring(0, 2)).Trim();
            if (mod.Equals(0))
                tach_mod = "000";
            // Tach hang con lai sau mod :
            if (Num.ToString().Length > 2)
                tach_conlai = Convert.ToString(Num.ToString().Trim().Substring(mod, Num.ToString().Length - mod)).Trim();

            ///don vi hang mod
            int im = m + 1;
            if (mod > 0)
                lso_chu = Tach(tach_mod).ToString().Trim() + " " + Donvi(im.ToString().Trim());
            /// Tach 3 trong tach_conlai

            int i = m;
            int _m = m;
            int j = 1;
            string tach3;
            string tach3_;

            while (i > 0)
            {
                tach3 = tach_conlai.Trim().Substring(0, 3).Trim();
                tach3_ = tach3;
                lso_chu = lso_chu.Trim() + " " + Tach(tach3.Trim()).Trim();
                m = _m + 1 - j;
                if (!tach3_.Equals("000"))
                    lso_chu = lso_chu.Trim() + " " + Donvi(m.ToString().Trim()).Trim();
                tach_conlai = tach_conlai.Trim().Substring(3, tach_conlai.Trim().Length - 3);

                i--;
                j++;
            }
            if (lso_chu.Trim().Substring(0, 1).Equals("k"))
                lso_chu = lso_chu.Trim().Substring(10, lso_chu.Trim().Length - 10).Trim();
            if (lso_chu.Trim().Substring(0, 1).Equals("l"))
                lso_chu = lso_chu.Trim().Substring(2, lso_chu.Trim().Length - 2).Trim();
            if (lso_chu.Trim().Length > 0)
                lso_chu = lso_chu.Trim().Substring(0, 1).Trim().ToUpper() + lso_chu.Trim().Substring(1, lso_chu.Trim().Length - 1).Trim();

            return Replace_special_word(lso_chu.ToString().Trim());

        }
        public static System.Drawing.Image ResizeByWidth(System.Drawing.Image img, int width)
        {
            // lấy chiều rộng và chiều cao ban đầu của ảnh
            int originalW = img.Width;
            int originalH = img.Height;

            // lấy chiều rộng và chiều cao mới tương ứng với chiều rộng truyền vào của ảnh (nó sẽ giúp ảnh của chúng ta sau khi resize vần giứ được độ cân đối của tấm ảnh
            int resizedW = width;
            int resizedH = (originalH * resizedW) / originalW;

            // tạo một Bitmap có kích thước tương ứng với chiều rộng và chiều cao mới
            Bitmap bmp = new Bitmap(resizedW, resizedH);

            // tạo mới một đối tượng từ Bitmap
            Graphics graphic = Graphics.FromImage(bmp);
            graphic.InterpolationMode = InterpolationMode.High;

            // vẽ lại ảnh với kích thước mới
            graphic.DrawImage(img, 0, 0, resizedW, resizedH);

            // gải phóng resource cho đối tượng graphic
            graphic.Dispose();

            // trả về anh sau khi đã resize
            return bmp;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0059:Unnecessary assignment of a value", Justification = "<Pending>")]
        public static DateTime MondayOfWeek(DateTime date)
        {
            var dayOfWeek = date.DayOfWeek;
            DateTime a = Curtime();
            if (dayOfWeek == DayOfWeek.Sunday)
            {
                a = date.AddDays(-6);
            }
            else
            {
                int offset = dayOfWeek - DayOfWeek.Monday;
                a = date.AddDays(-offset);
            }
            return new DateTime(a.Year, a.Month, a.Day);
        }
        //public static async Task<string> SendTelegram(string ms, string type)
        //{
        //    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        //    var token = C_Config.Get_TelegramToken().FirstOrDefault(p => p.Type == type);
        //    var api = "https://api.telegram.org/bot" + token.Token + "/sendmessage";
        //    if (token != null)
        //    {
        //        try
        //        {
        //            var content = new FormUrlEncodedContent(new[]
        //            {
        //                new KeyValuePair<string, string>("parse_mode", "html"),
        //                new KeyValuePair<string, string>("chat_id",token.Room),
        //                new KeyValuePair<string, string>("text","["+C_Config.ProjectName+"] " + ms),
        //                new KeyValuePair<string, string>("disable_web_page_preview", "1")
        //            });

        //            var res = await C_Request.PostDataHttpClient(api, content);
        //            return res;
        //        }
        //        catch (Exception ex)
        //        {
        //        }

        //    }
        //    return "Token don't exist";
        //}
        public static string CatChuoi(string s, int length)
        {
            if (String.IsNullOrEmpty(s))
            {
                //throw new ArgumentNullException(s);
                return "";
            }

            var words = s.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (words[0].Length > length)
                throw new ArgumentException("The first word is longer than the string to be cut");
            var sb = new StringBuilder();
            foreach (var word in words)
            {
                if ((sb + word).Length > length)
                    return string.Format("{0}...", sb.ToString().TrimEnd(' '));
                sb.Append(word + " ");
            }
            return string.Format("{0}", sb.ToString().TrimEnd(' '));
        }
        public static long CountTable(string tableName, string host)
        {
            using (var de = new DataEntities())
            {
                if (tableName.ToLower().Contains("delete") || tableName.ToLower().Contains("--") || tableName.ToLower().Contains("update")) return 0;
                var qr = "SELECT sum(p.rows)  FROM sys.partitions AS p INNER JOIN sys.tables AS t ON p.[object_id] = t.[object_id] INNER JOIN sys.schemas AS s ON s.[schema_id] = t.[schema_id] WHERE t.name = N'" + tableName + "' AND s.name = N'dbo' AND p.index_id IN (0,1);";
                var c = de.Database.SqlQuery<long>(qr);
                return c.FirstOrDefault();
            }
        }
        public static string GetSubDomain(Uri url)
        {

            if (url.HostNameType == UriHostNameType.Dns)
            {
                string host = url.Host;
                var nodes = host.Split('.');
                int startNode = 0;
                if (nodes[0] == "www") startNode = 1;

                return nodes[startNode];
            }

            return null;
        }
        public static string GetIPServer()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }
        public static string GetIPAddressPublic()
        {
            try
            {
                string address = "";
                WebRequest request = WebRequest.Create("http://checkip.dyndns.org/");
                using (WebResponse response = request.GetResponse())
                using (StreamReader stream = new StreamReader(response.GetResponseStream()))
                {
                    address = stream.ReadToEnd();
                }

                int first = address.IndexOf("Address: ") + 9;
                int last = address.LastIndexOf("</body>");
                address = address.Substring(first, last - first);

                return address;
            }
            catch
            {
                return "42.117.2.212";
            }
        }
        public static bool RenameFile(string pathFrom, string pathTo)
        {
            try
            {
                // Create a FileInfo  
                FileInfo fi = new FileInfo(pathFrom);
                // Check if file is there  
                if (fi.Exists)
                {
                    // Move file with a new name. Hence renamed.  
                    fi.MoveTo(pathTo);
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
        public static bool RenameFolder(string pathFrom, string pathTo)
        {
            try
            {
                Directory.Move(pathFrom, pathTo);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public static string CalcCRC16(string strInput)
        {
            ushort crc = 0xFFFF;
            byte[] data = Encoding.ASCII.GetBytes(strInput);
            for (int i = 0; i < data.Length; i++)
            {
                crc ^= (ushort)(data[i] << 8);
                for (int j = 0; j < 8; j++)
                {
                    if ((crc & 0x8000) > 0)
                        crc = (ushort)((crc << 1) ^ 0x1021);
                    else
                        crc <<= 1;
                }
            }
            return crc.ToString("X4");
        }
        public static string TaoMaQRNganHang(string bankCode, string bankNumber, string amount = "", string note = "")
        {
            if (string.IsNullOrEmpty(bankCode) || string.IsNullOrEmpty(bankNumber)) return "";
            var phienBanDuLieu = "000201";          // Phiên bản dữ liệu
            var phuongThucKhoiTao = "010212";       // Phương thức khởi tạo
            var thongTinDinhDanhNguoiHuongThu = "38";
            {
                var dinhDanhToanCau = "0010A000000727";
                var soTaiKhoan = bankNumber;
                soTaiKhoan = "01" + string.Format("{0:D2}", soTaiKhoan.Length) + soTaiKhoan;
                string maBIN;
                switch (bankCode.ToLower())
                {
                    case "sgicb": maBIN = "970400"; break;          // Ngân hàng TMCP Sài Gòn Công Thương
                    case "stb": maBIN = "970403"; break;            // Ngân hàng TMCP Sài Gòn Thương Tín
                    case "vba": maBIN = "970405"; break;            // Ngân hàng Nông nghiệp và Phát triển Nông thôn Việt Nam
                    case "dob": maBIN = "970406"; break;            // Ngân hàng TMCP Đông Á
                    case "tcb": maBIN = "970407"; break;            // Ngân hàng TMCP Kỹ Thương
                    case "gpb": maBIN = "970408"; break;            // Ngân hàng Thương mại TNHH Một Thành Viên Dầu Khí Toàn Cầu
                    case "bab": maBIN = "970409"; break;            // Ngân hàng TMCP Bắc Á
                    case "scvn": maBIN = "970410"; break;           // Ngân hàng TNHH Một Thành Viên Standard Chartered
                    case "kbank": maBIN = "970412"; break;          // Ngân hàng Đại chúng TNHH Kasikornbank
                    case "oceanbank": maBIN = "970414"; break;      // Ngân hàng TNHH Một Thành Viên Đại Dương
                    case "icb": maBIN = "970415"; break;            // Ngân hàng TMCP Công Thương Việt Nam
                    case "acb": maBIN = "970416"; break;            // Ngân hàng TMCP Á Châu
                    case "bidv": maBIN = "970418"; break;           // Ngân hàng TMCP Đầu tư và Phát triển Việt Nam
                    case "ncb": maBIN = "970419"; break;            // Ngân hàng TMCP Quốc Dân
                    case "vrb": maBIN = "970421"; break;            // Ngân hàng liên doanh Việt Nga
                    case "mb": maBIN = "970422"; break;             // Ngân hàng TMCP Quân Đội
                    case "tpb": maBIN = "970423"; break;            // Ngân hàng TMCP Tiên Phong
                    case "shbvn": maBIN = "970424"; break;          // Ngân hàng TNHH Một Thành Viên Shinhan Việt Nam
                    case "abb": maBIN = "970425"; break;            // Ngân hàng TMCP An Bình
                    case "msb": maBIN = "970426"; break;            // Ngân hàng TMCP Hàng Hải
                    case "vab": maBIN = "970427"; break;            // Ngân hàng TMCP Việt Á
                    case "nab": maBIN = "970428"; break;            // Ngân hàng TMCP Nam Á
                    case "scb": maBIN = "970429"; break;            // Ngân hàng TMCP Sài Gòn
                    case "pgb": maBIN = "970430"; break;            // Ngân hàng TMCP Xăng dầu Petrolimex
                    case "eib": maBIN = "970431"; break;            // Ngân hàng TMCP Xuất Nhập khẩu Việt Nam
                    case "vpb": maBIN = "970432"; break;            // Ngân hàng TMCP Việt Nam Thịnh Vượng
                    case "vbb": maBIN = "970433"; break;            // Ngân hàng TMCP Việt Nam Thương Tín
                    case "ivb": maBIN = "970434"; break;            // Ngân hàng TNHH Indovina
                    case "vcb": maBIN = "970436"; break;            // Ngân hàng TMCP Ngoại Thương Việt Nam
                    case "hdb": maBIN = "970437"; break;            // Ngân hàng TMCP Phát triển TP.HCM
                    case "bvb": maBIN = "970438"; break;            // Ngân hàng TMCP Bảo Việt
                    case "pbvn": maBIN = "970439"; break;           // Ngân hàng TNHH MTV Public Việt Nam
                    case "ssb": maBIN = "970440"; break;            // Ngân hàng TMCP Đông Nam Á
                    case "vib": maBIN = "970441"; break;            // Ngân hàng TMCP Quốc Tế Việt Nam
                    case "hlbvn": maBIN = "970442"; break;          // Ngân hàng TNHH Một Thành Viên Hong Leong Việt Nam
                    case "shb": maBIN = "970443"; break;            // Ngân hàng TMCP Sài Gòn - Hà Nội
                    case "cbb": maBIN = "970444"; break;            // Ngân hàng Thương mại TNHH MTV Xây Dựng Việt Nam
                    case "coopbank": maBIN = "970446"; break;       // Ngân hàng Hợp Tác Xã Việt Nam
                    case "ocb": maBIN = "970448"; break;            // Ngân hàng TMCP Phương Đông
                    case "lpb": maBIN = "970449"; break;            // Ngân hàng TMCP Bưu Điện Liên Việt
                    case "klb": maBIN = "970452"; break;            // Ngân hàng TMCP Kiên Long
                    //case "bvb": maBIN = "970454"; break;          // Ngân hàng TMCP Bản Việt
                    case "ibkhn": maBIN = "970455"; break;          // Ngân hàng Công nghiệp Hàn Quốc - Chi nhánh Hà Nội
                    case "ibkhcm": maBIN = "970456"; break;         // Ngân hàng Công nghiệp Hàn Quốc - Chi nhánh Hồ Chí Minh
                    case "wvn": maBIN = "970457"; break;            // Ngân hàng Woori Bank Việt Nam
                    case "uob": maBIN = "970458"; break;            // Ngân hàng UOB Việt Nam
                    case "cimb": maBIN = "970459"; break;           // Ngân hàng TNHH Một Thành Viên CIMB Việt Nam
                    case "cfc": maBIN = "970460"; break;            // Công ty tài chính cổ phần Xi Măng
                    default:
                        return "";
                }
                maBIN = "00" + string.Format("{0:D2}", maBIN.Length) + maBIN;
                var dinhDanhACQ = maBIN + soTaiKhoan;     // Định danh toàn cầu
                dinhDanhACQ = "01" + string.Format("{0:D2}", dinhDanhACQ.Length) + dinhDanhACQ;
                var loaiChuyen = "0208QRIBFTTA";                // TA: Chuyển qua tài khoản, TC: Chuyển qua thẻ
                var meg01 = dinhDanhToanCau + dinhDanhACQ + loaiChuyen;
                thongTinDinhDanhNguoiHuongThu += string.Format("{0:D2}", meg01.Length) + meg01;
            }
            var maTienTe = "5303704"; // Mã tiền tệ
            var soTienGiaoDich = amount;
            if (soTienGiaoDich != "")
                soTienGiaoDich = "54" + string.Format("{0:D2}", soTienGiaoDich.Length) + soTienGiaoDich;
            var maQuocGia = "5802VN"; // Mã quốc gia                

            var a = "08" + string.Format("{0:D2}", note.Length) + note;
            var ghiChuGiaoDich = "62" + string.Format("{0:D2}", a.Length) + a;
            var maKiemThu = "6304";
            var meg02 = phienBanDuLieu + phuongThucKhoiTao + thongTinDinhDanhNguoiHuongThu + maTienTe + soTienGiaoDich + maQuocGia + ghiChuGiaoDich + maKiemThu;
            var layMaKiemThu = CalcCRC16(meg02);
            var kq = meg02 + layMaKiemThu;
            return kq;
        }
        //public static async Task AdminLog(string adminId, string log, Guid parentId)
        //{
        //    try
        //    {
        //        using (var dbLog = DataEntities.Create("", Enum_Database.BBank))
        //        {
        //            dbLog.AdminLogs.Add(new AdminLog
        //            {
        //                Id = Guid.NewGuid(),
        //                AdminId = adminId,
        //                DateCreate = Curtime(),
        //                ParentId = parentId,
        //                Note = log
        //            });
        //            dbLog.SaveChanges();
        //            await SendTelegram("AdminLog\nLog: " + log.Replace(", ", "\n"), "alert");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        await SendTelegram("Lỗi AdminLog\nLog: " + log + "\n" + ex.Message + "\n" + ex.InnerException?.Message + "\n" + ex.InnerException?.InnerException?.Message, "error");
        //    }
        //}
        public static string GetFileExtension(string base64String)
        {
            var data = base64String.Substring(0, 5);

            switch (data.ToUpper())
            {
                case "IVBOR":
                    return ".png";
                case "/9J/4":
                    return ".jpg";
                case "AAAAF":
                    return ".mp4";
                case "JVBER":
                    return ".pdf";
                case "AAABA":
                    return ".ico";
                case "UMFYI":
                    return ".rar";
                case "E1XYD":
                    return ".rtf";
                case "U1PKC":
                    return ".txt";
                case "PHN2Z":
                    return ".svg";
                case "PD94B":
                    return ".svg";
                case "77U/M":
                    return ".srt";
                default:
                    return string.Empty;
            }
        }
        public static TempFileSize GetFileSize(long bytes)
        {
            var size = bytes / 1024;
            var unit = "KB";
            if (size > 1000)
            {
                size /= 1024;
                unit = "MB";
            }

            if (size > 1000)
            {
                size /= 1024;
                unit = "GB";
            }
            if (size > 1000)
            {
                size /= 1024;
                unit = "TB";
            }
            return new TempFileSize
            {
                Size = size,
                Unit = unit
            };
        }
        public static string ReadTextFromFile(string path)
        {
            return System.IO.File.ReadAllText(path);
        }
        public static int TotalMonths(this DateTime start, DateTime end)
        {
            return Math.Abs((start.Year * 12 + start.Month) - (end.Year * 12 + end.Month));
        }
        public static JwtSecurityToken DeJwtToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(C_Config.KeyJWTAPI)),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                return jwtToken;
            }
            catch
            {
                return null;
            }
        }
        public static string EnJwtToken(IEnumerable<Claim> claim, DateTime exp)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var keySign = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(C_Config.KeyJWTAPI)), SecurityAlgorithms.HmacSha256Signature);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                //new[] { new Claim("email", "test@gmail.com") }
                Subject = new ClaimsIdentity(claim),
                Expires = exp,
                SigningCredentials = keySign
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var kq = tokenHandler.WriteToken(token);
            return kq;
        }
    }
    public class TempFileSize
    {
        public long Size { get; set; }
        public string Unit { get; set; }
    }
    public class XSMB
    {
        public DateTime ThoiGian { get; set; }
        public List<string> GiaiDacBiet { get; set; }
        public List<string> GiaiNhat { get; set; }
        public List<string> GiaiNhi { get; set; }
        public List<string> GiaiBa { get; set; }
        public List<string> GiaiTu { get; set; }
        public List<string> GiaiNam { get; set; }
        public List<string> GiaiSau { get; set; }
        public List<string> GiaiBay { get; set; }
    }
    public class ScoreSearch
    {
        public int Score { get; set; }
        public double ScorePercent { get; set; }
        public Product Product { get; set; }
    }
}


