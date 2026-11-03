using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
namespace XmlCB1_2415053122235
{
    class SinhVien
    {
        public string Mssv { get; set; } = string.Empty;
        public string HoTen { get; set; } = string.Empty;
        public List<(string Mon, double Diem)> DanhSachDiem { get; set; } = new List<(string, double)>();

        public double DTB => DanhSachDiem.Count > 0 ? DanhSachDiem.Average(d => d.Diem) : 0.0;
        public double DiemToiThieu => DanhSachDiem.Count > 0 ? DanhSachDiem.Min(d => d.Diem) : 0.0;
    }
    internal class Program
    {
        static void Main(string[] args)
        {
            // 1. Tự động tạo file XML mẫu nếu chưa tồn tại
            TaoFileXmlMauNeuChuaCo("dulieu.xml");
            // 2. Đọc dòng lệnh đầu tiên từ Console
            string? lenhInput = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(lenhInput))
            {
                Console.WriteLine("Lenh khong hop le");
                return;
            }
            string lenhFull = lenhInput.Trim();

            // 3. Đọc nội dung XML từ stdin
            string xmlContent = Console.In.ReadToEnd();
            // Nếu stdin trống (ví dụ chạy trực tiếp bấm F5), tự động lấy nội dung từ file dulieu.xml vừa tạo
            if (string.IsNullOrWhiteSpace(xmlContent) && File.Exists("dulieu.xml"))
            {
                xmlContent = File.ReadAllText("dulieu.xml");
            }
            if (string.IsNullOrWhiteSpace(xmlContent))
            {
                Console.WriteLine("Lenh khong hop le");
                return;
            }
            // 4. Load XML bằng XmlDocument
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.LoadXml(xmlContent);
            }
            catch
            {
                Console.WriteLine("Lenh khong hop le");
                return;
            }
            // 5. Đọc danh sách sinh viên bằng DOM
            List<SinhVien> dsSinhVien = DocDanhSachSinhVien(xmlDoc);

            // 6. Xử lý các lệnh
            if (lenhFull == "DANHSACH")
            {
                XuLyDanhSach(dsSinhVien);
            }
            else if (lenhFull == "HOCBONG")
            {
                XuLyHocBong(dsSinhVien);
            }
            else if (lenhFull.StartsWith("MON "))
            {
                string tenMon = lenhFull.Substring(4).Trim();
                XuLyMon(dsSinhVien, tenMon);
            }
            else
            {
                Console.WriteLine("Lenh khong hop le");
            }
        }
        static void TaoFileXmlMauNeuChuaCo(string filePath)
        {
            if (File.Exists(filePath)) return;

            XmlDocument doc = new XmlDocument();
            XmlDeclaration xmlDec = doc.CreateXmlDeclaration("1.0", "utf-8", null);
            doc.AppendChild(xmlDec);
            XmlElement root = doc.CreateElement("lop");
            root.SetAttribute("ma", "22T1");
            doc.AppendChild(root);
            var listData = new[]
            {
                new { Mssv = "22001", Ten = "Nguyen Van An", Diems = new[] { ("XML", "8.5"), ("CSDL", "9.0"), ("Web", "8.0") } },
                new { Mssv = "22002", Ten = "Tran Thi Binh", Diems = new[] { ("XML", "9.5"), ("CSDL", "5.5") } },
                new { Mssv = "22003", Ten = "Le Van Cuong", Diems = new[] { ("XML", "7.0"), ("CSDL", "8.0"), ("Web", "9.0") } },
                new { Mssv = "22004", Ten = "Pham Thi Dung", Diems = new[] { ("XML", "9.0"), ("CSDL", "8.0") } },
                new { Mssv = "22005", Ten = "Hoang Van Em", Diems = new[] { ("XML", "6.0"), ("CSDL", "9.5"), ("Web", "8.5") } }
            };
            foreach (var data in listData)
            {
                XmlElement sv = doc.CreateElement("sinhvien");
                sv.SetAttribute("mssv", data.Mssv);

                XmlElement hoten = doc.CreateElement("hoten");
                hoten.InnerText = data.Ten;
                sv.AppendChild(hoten);
                foreach (var d in data.Diems)
                {
                    XmlElement diem = doc.CreateElement("diem");
                    diem.SetAttribute("mon", d.Item1);
                    diem.InnerText = d.Item2;
                    sv.AppendChild(diem);
                }
                root.AppendChild(sv);
            }

            doc.Save(filePath);
        }
        static List<SinhVien> DocDanhSachSinhVien(XmlDocument doc)
        {
            List<SinhVien> list = new List<SinhVien>();
            XmlNodeList svNodes = doc.GetElementsByTagName("sinhvien");
            foreach (XmlNode svNode in svNodes)
            {
                if (svNode.Attributes == null) continue;

                SinhVien sv = new SinhVien();
                XmlAttribute? attrMssv = svNode.Attributes["mssv"];
                sv.Mssv = attrMssv != null ? attrMssv.Value : string.Empty;
                foreach (XmlNode child in svNode.ChildNodes)
                {
                    if (child.Name == "hoten")
                    {
                        sv.HoTen = child.InnerText;
                    }
                    else if (child.Name == "diem")
                    {
                        if (child.Attributes != null && child.Attributes["mon"] != null)
                        {
                            string mon = child.Attributes["mon"]!.Value;
                            if (double.TryParse(child.InnerText, NumberStyles.Any, CultureInfo.InvariantCulture, out double val))
                            {
                                sv.DanhSachDiem.Add((mon, val));
                            }
                        }
                    }
                }
                list.Add(sv);
            }
            return list;
        }
        static void XuLyDanhSach(List<SinhVien> list)
        {
            var sortedList = list
                .OrderByDescending(s => s.DTB)
                .ThenBy(s => s.Mssv, StringComparer.Ordinal)
                .ToList();
            foreach (var sv in sortedList)
            {
                string dtbStr = sv.DTB.ToString("0.00", CultureInfo.InvariantCulture);
                Console.WriteLine($"{sv.Mssv} - {sv.HoTen} - DTB: {dtbStr}");
            }
            Console.WriteLine($"Tong: {sortedList.Count} sinh vien");
        }
