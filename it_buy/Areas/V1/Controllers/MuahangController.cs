

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using System.Collections;
using System.Data;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Net.WebSockets;
using System.Reflection;
using System.Security.Policy;
using Vue.Data;
using Vue.Models;
using Vue.Services;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;

namespace it_template.Areas.V1.Controllers
{

    public class MuahangController : BaseController
    {
        private readonly IConfiguration _configuration;
        private UserManager<UserModel> UserManager;
        private readonly ViewRender _view;
        public MuahangController(ItContext context, IConfiguration configuration, UserManager<UserModel> UserMgr, ViewRender view) : base(context)
        {
            _configuration = configuration;
            UserManager = UserMgr;
            _view = view;
        }
        public JsonResult GetFiles(int id)
        {
            var data = new List<RawFile>();
            ///Lấy dự trù
            var dutru = _context.MuahangChitietModel.Where(d => d.muahang_id == id).Include(d => d.dutru_chitiet).ThenInclude(d => d.dutru).Select(d => d.dutru_chitiet.dutru).ToList();
            dutru = dutru.Distinct().ToList();
            foreach (var d in dutru)
            {
                var file = new RawFile()
                {
                    note = "Dự trù mã số " + d.code,
                    link = "/dutru/edit/" + d.id,
                    list_file = new List<MuahangDinhkemModel>()
                    {
                        new MuahangDinhkemModel()
                        {
                            name = d.name,
                            ext = ".pdf",
                            url = d.pdf,
                        }
                    },
                    is_user_upload = false,
                    created_at = d.created_at
                };
                data.Add(file);
            }

            ///Lấy báo giá
            var baogia = _context.MuahangNccModel.Where(d => d.muahang_id == id).Include(d => d.dinhkem).Include(d => d.ncc).ToList();
            foreach (var d in baogia)
            {
                var list_file = new List<MuahangDinhkemModel>();
                DateTime? created_at = null;
                foreach (var item in d.dinhkem)
                {
                    created_at = item.created_at;
                    list_file.Add(new MuahangDinhkemModel()
                    {
                        name = item.name,
                        ext = item.ext,
                        url = item.url,
                    });
                }
                if (created_at != null)
                {
                    var file = new RawFile()
                    {
                        note = "Báo giá của " + d.ncc.tenncc,
                        list_file = list_file,
                        is_user_upload = false,
                        created_at = created_at
                    };
                    data.Add(file);

                }
            }
            ///Lấy đề nghị mua hàng
            var muahang = _context.MuahangModel.Where(d => d.id == id).FirstOrDefault();
            data.Add(new RawFile()
            {
                note = "Đề nghị mua hàng mã số " + muahang.code,
                link = "/muahang/edit/" + muahang.id,
                list_file = new List<MuahangDinhkemModel>()
                    {
                        new MuahangDinhkemModel()
                        {
                            name = muahang.name,
                            ext = ".pdf",
                            url = muahang.pdf,
                        }
                    },
                is_user_upload = false,
                created_at = muahang.created_at
            });
            ///Lấy ủy nhiệm chi
            var uynhiemchi = _context.MuahangUynhiemchiModel.Where(d => d.muahang_id == id).ToList();
            if (uynhiemchi.Count() > 0)
            {
                data.Add(new RawFile()
                {
                    note = "Ủy nhiệm chi",
                    list_file = uynhiemchi.Select(d => new MuahangDinhkemModel()
                    {
                        name = d.name,
                        ext = d.ext,
                        url = d.url,
                    }).ToList(),
                    is_user_upload = false,
                    created_at = uynhiemchi.FirstOrDefault().created_at
                });
            }
            ///File up
            var files_up = _context.MuahangDinhkemModel.Where(d => d.muahang_id == id && d.deleted_at == null).ToList();
            if (files_up.Count > 0)
            {
                data.AddRange(files_up.GroupBy(d => new { d.note, d.created_at }).Select(d => new RawFile
                {
                    note = d.First().note,
                    list_file = d.ToList(),
                    is_user_upload = true,
                    created_at = d.Key.created_at
                }).ToList());
            }

            ///Sort
            data = data.OrderBy(d => d.created_at).ToList();
            return Json(data);
        }
        public JsonResult Get(int id)
        {
            var data = _context.MuahangModel.Where(d => d.id == id)
                .Include(d => d.user_created_by)
                .Include(d => d.uynhiemchi)
                .Include(d => d.chitiet)
                .Include(d => d.nccs).ThenInclude(d => d.chitiet)
                .Include(d => d.nccs).ThenInclude(d => d.dinhkem.Where(d => d.deleted_at == null))
                .Include(d => d.nccs).ThenInclude(d => d.ncc).FirstOrDefault();
            if (data != null)
            {
                var stt = 1;
                foreach (var item in data.chitiet)
                {
                    //var material = _context.MaterialModel.Where(d => item.hh_id == "m-" + d.id).FirstOrDefault();
                    //if (material != null)
                    //{
                    //    item.tenhh = material.tenhh;
                    //    item.mahh = material.mahh;
                    //    item.stt = stt++;
                    //}
                    item.soluong_nhanhang = item.soluong;
                    item.stt = stt++;
                }
                foreach (var ncc in data.nccs)
                {
                    stt = 1;
                    //if (data.muahang_chonmua_id == ncc.id)
                    //{
                    //    data.muahang_chonmua = ncc;
                    //}
                    foreach (var item in ncc.chitiet)
                    {
                        //var material = _context.MaterialModel.Where(d => item.hh_id == "m-" + d.id).FirstOrDefault();
                        //if (material != null)
                        //{
                        //    item.tenhh = material.tenhh;
                        //    item.mahh = material.mahh;
                        //    item.stt = stt++;
                        //}
                        item.stt = stt++;
                    }
                }
            }
            return Json(data);
        }
        public JsonResult GetUserNhanhang(int muahang_id)
        {
            var data = new List<string>();
            var dutru = _context.MuahangChitietModel.Where(d => d.muahang_id == muahang_id).Include(d => d.dutru_chitiet).ThenInclude(d => d.dutru).Select(d => d.dutru_chitiet.dutru).Distinct().ToList();
            foreach (var d in dutru)
            {
                data.Add(d.created_by);
            }
            data = data.Distinct().ToList();
            return Json(data);
        }
        public JsonResult getHistory(string hh_id)
        {
            var hh = _context.MaterialModel.Where(d => "m-" + d.id == hh_id).FirstOrDefault();
            var muahang_chitiet = _context.MuahangChitietModel.Include(d => d.muahang).ThenInclude(d => d.muahang_chonmua).ThenInclude(d => d.chitiet)
                .Where(d => d.muahang.deleted_at == null && d.muahang.is_dathang == true && d.hh_id == hh_id)
                .ToList();
            var to = hh != null ? hh.mahh + "-" + hh.tenhh : "";

            var data = new ArrayList();
            foreach (var d in muahang_chitiet)
            {
                var muahang = d.muahang;
                var chonmua = muahang.muahang_chonmua.chitiet.Where(e => e.muahang_chitiet_id == d.id).FirstOrDefault();
                var data1 = new
                {
                    muahang = muahang,
                    tenhh = chonmua.tenhh,
                    soluong = chonmua.soluong,
                    dongia = chonmua.dongia,
                    thanhtien = chonmua.thanhtien,
                };
                data.Add(data1);
            }
            return Json(new { to = to, data = data });
        }

        public async Task<JsonResult> QrNhanhang(int muahang_id)
        {
            //var data = _context.MuahangChitietModel.Where(d => d.muahang_id == muahang_id).Include(d => d.dutru_chitiet).GroupBy(d => d.dutru_chitiet.dutru_id).Select(d => d.Key).ToList();
            List<string> ret = new List<string>();
            string Domain = (HttpContext.Request.IsHttps ? "https://" : "http://") + HttpContext.Request.Host.Value;
            //foreach (var item in data)
            //{
            QRCoder.PayloadGenerator.Url generator = new QRCoder.PayloadGenerator.Url(Domain + "/muahang/nhanhang/" + muahang_id.ToString());
            string payload = generator.ToString();

            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            var qrCodeAsBitmap = qrCode.GetGraphic(20);

            System.IO.MemoryStream ms = new MemoryStream();
            qrCodeAsBitmap.Save(ms, ImageFormat.Png);
            byte[] byteImage = ms.ToArray();
            var SigBase64 = Convert.ToBase64String(byteImage); // Get Base64

            var timeStamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
            //qrCodeAsBitmap.Save("wwwroot/temp/" + timeStamp + ".png", System.Drawing.Imaging.ImageFormat.Png);
            ret.Add("data:image/png;base64, " + SigBase64);
            //}

            return Json(new
            {
                success = true,
                list = ret
            });
        }
        [HttpPost]
        public async Task<JsonResult> xoadinhkemncc(int id)
        {
            var Model = _context.MuahangNccDinhkemModel.Where(d => d.id == id).FirstOrDefault();
            Model.deleted_at = DateTime.Now;
            _context.Update(Model);
            _context.SaveChanges();
            return Json(new { success = true });
        }
        [HttpPost]
        public async Task<JsonResult> xoadinhkem(List<int> list_id)
        {
            var Model = _context.MuahangDinhkemModel.Where(d => list_id.Contains(d.id)).ToList();
            foreach (var item in Model)
            {
                item.deleted_at = DateTime.Now;
            }
            _context.UpdateRange(Model);
            _context.SaveChanges();
            return Json(new { success = true });
        }
        [HttpPost]
        public async Task<JsonResult> Delete(int id)
        {
            var Model = _context.MuahangModel.Where(d => d.id == id).FirstOrDefault();
            Model.deleted_at = DateTime.Now;
            _context.Update(Model);

            var Model1 = _context.MuahangChitietModel.Where(d => d.muahang_id == id).ToList();
            _context.RemoveRange(Model1);

            _context.SaveChanges();
            return Json(Model);
        }

        [HttpPost]
        public async Task<JsonResult> Save(MuahangModel MuahangModel, List<MuahangChitietModel> list_add, List<MuahangChitietModel>? list_update, List<MuahangChitietModel>? list_delete)
        {

            MuahangModel? MuahangModel_old;
            if (MuahangModel.date != null && MuahangModel.date.Value.Kind == DateTimeKind.Utc)
            {
                MuahangModel.date = MuahangModel.date.Value.ToLocalTime();
            }
            bool is_check_state = false;
            if (MuahangModel.id == 0)
            {
                System.Security.Claims.ClaimsPrincipal currentUser = this.User;

                var user_id = UserManager.GetUserId(currentUser);
                //var user = await UserManager.GetUserAsync(currentUser);
                MuahangModel.created_at = DateTime.Now;
                MuahangModel.status_id = (int)Status.Baogia;
                MuahangModel.created_by = user_id;
                MuahangModel.activeStep = 0;

                _context.MuahangModel.Add(MuahangModel);

                _context.SaveChanges();

                MuahangModel_old = MuahangModel;

            }
            else
            {

                MuahangModel_old = _context.MuahangModel.Where(d => d.id == MuahangModel.id).Include(d => d.user_created_by).FirstOrDefault();
                if (MuahangModel_old.is_nhanhang == false || MuahangModel_old.is_thanhtoan == false)
                {
                    is_check_state = true;
                }
                MuahangModel.user_created_by = MuahangModel_old.user_created_by;
                MuahangModel.muahang_chonmua = null;
                CopyValues<MuahangModel>(MuahangModel_old, MuahangModel);
                MuahangModel_old.updated_at = DateTime.Now;

                _context.Update(MuahangModel_old);
                _context.SaveChanges();
            }

            if (list_delete != null)
                _context.RemoveRange(list_delete);
            if (list_add != null)
            {
                foreach (var item in list_add)
                {
                    item.muahang_id = MuahangModel_old.id;
                    _context.Add(item);
                }
            }
            if (list_update != null)
            {
                foreach (var item in list_update)
                {
                    _context.Update(item);
                }
            }
            ///// CHECK FINISH
            //if (is_check_state == true && MuahangModel_old.is_nhanhang == true && MuahangModel_old.is_thanhtoan == true)
            //{
            //    MuahangModel_old.date_finish = DateTime.Now;
            //    _context.Update(MuahangModel_old);
            //}
            _context.SaveChanges();

            return Json(new { success = true, id = MuahangModel_old.id });

        }
        [HttpPost]
        public async Task<JsonResult> saveNcc(List<MuahangNccModel> nccs)
        {
            try
            {
                System.Security.Claims.ClaimsPrincipal currentUser = this.User;

                var user_id = UserManager.GetUserId(currentUser);
                var user = await UserManager.GetUserAsync(currentUser);
                var muahang_id = 0;
                foreach (var item in nccs)
                {
                    muahang_id = item.muahang_id.Value;
                    if (item.id > 0)
                    {
                        _context.Update(item);

                        _context.SaveChanges();

                    }
                    else
                    {
                        _context.Add(item);

                        _context.SaveChanges();
                    }
                }


                var files = Request.Form.Files;

                var items_attachment = new List<MuahangNccDinhkemModel>();
                if (files != null && files.Count > 0)
                {

                    foreach (var file in files)
                    {
                        var timeStamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
                        string name = file.FileName;
                        string type = file.Name;
                        string ext = Path.GetExtension(name);
                        string mimeType = file.ContentType;

                        var list = type.Split("_");
                        var key_muahang = Int32.Parse(list[1]);
                        var muahang_ncc_id = nccs[key_muahang].id;
                        //var fileName = Path.GetFileName(name);
                        var newName = timeStamp + "-" + muahang_ncc_id + "-" + name;

                        newName = newName.Replace("+", "_");
                        newName = newName.Replace("%", "_");
                        var dir = _configuration["Source:Path_Private"] + "\\buy\\muahang\\" + muahang_id;
                        bool exists = Directory.Exists(dir);

                        if (!exists)
                            Directory.CreateDirectory(dir);


                        var filePath = dir + "\\" + newName;

                        string url = "/private/buy/muahang/" + muahang_id + "/" + newName;
                        items_attachment.Add(new MuahangNccDinhkemModel
                        {
                            ext = ext,
                            url = url,
                            name = name,
                            mimeType = mimeType,
                            muahang_ncc_id = muahang_ncc_id,
                            created_at = DateTime.Now,
                            created_by = user_id
                        });

                        using (var fileSrteam = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(fileSrteam);
                        }
                    }
                    _context.AddRange(items_attachment);
                    _context.SaveChanges();
                }


                //_context.SaveChanges();
                if (muahang_id > 0)
                {
                    var muahang = _context.MuahangModel.Where(d => d.id == muahang_id).FirstOrDefault();
                    muahang.status_id = (int)Status.sosanhgia;
                    _context.SaveChanges();
                }
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }

        }
        [HttpPost]
        public async Task<JsonResult> saveNhanhang(int muahang_id, List<MuahangChitietModel> list)
        {
            try
            {
                System.Security.Claims.ClaimsPrincipal currentUser = this.User;

                var user_id = UserManager.GetUserId(currentUser);
                var user = await UserManager.GetUserAsync(currentUser);
                var list_dutru_chitiet_id = new List<int>();
                if (list != null)
                {
                    foreach (var item in list)
                    {
                        item.user_nhanhang = null;
                        list_dutru_chitiet_id.Add(item.dutru_chitiet_id);
                        _context.Update(item);
                    }
                }
                _context.SaveChanges();
                ////Check nhận hàng mua hàng
                var muahang = _context.MuahangModel.Where(d => d.id == muahang_id).FirstOrDefault();

                var list_nhanhang = list.Where(d => d.status_nhanhang == 1).Count();
                if (list_nhanhang == list.Count())
                {
                    muahang.is_nhanhang = true;
                    //if (muahang.is_thanhtoan == true && muahang.is_nhanhang == true)
                    //{
                    //    muahang.date_finish = DateTime.Now;
                    //    _context.Update(muahang);
                    //}
                    _context.Update(muahang);
                    _context.SaveChanges();

                }
                //// Check du trù finish
                var list_dutru_id = _context.DutruChitietModel.Where(d => list_dutru_chitiet_id.Contains(d.id)).Select(d => d.dutru_id).Distinct().ToList();
                var list_dutru = _context.DutruModel.Where(d => list_dutru_id.Contains(d.id)).Include(d => d.chitiet).ToList();
                foreach (var dutru in list_dutru)
                {
                    var soluong_ht = 0;
                    foreach (var item in dutru.chitiet)
                    {
                        var soluong_dutru = item.soluong;
                        var muahang_chitiet = _context.MuahangChitietModel.Where(d => d.dutru_chitiet_id == item.id && d.status_nhanhang == 1).ToList();
                        var soluong_mua = muahang_chitiet.Sum(d => d.soluong);
                        if (soluong_dutru == soluong_mua)
                        {
                            soluong_ht++;
                        }
                    }
                    if (soluong_ht == dutru.chitiet.Count())
                    {
                        dutru.date_finish = DateTime.Now;
                        _context.Update(dutru);
                        _context.SaveChanges();
                    }
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }

        }
        [HttpPost]

        public async Task<JsonResult> baogia(int id)
        {
            var data = _context.MuahangModel.Where(d => d.id == id).Include(d => d.chitiet).FirstOrDefault();
            if (data == null)
            {
                return Json(new { success = false, message = "Dự trù không tồn tại!" });
            }

            ////Nhận báo giá
            data.status_id = (int)Status.Baogia;
            //_context.SaveChanges();
            System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            var user_id = UserManager.GetUserId(currentUser);
            _context.Update(data);
            _context.SaveChanges();

            return Json(new { success = true });
        }
        [HttpPost]

        public async Task<JsonResult> XuatDondathang(int id)
        {
            var now = DateTime.Now;
            var raw = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"date",now.ToString("dd/MM/yyyy") },
            };

            var RawDetails = new List<RawMuahangDetails>();
            var data = _context.MuahangModel.Where(d => d.id == id).FirstOrDefault();
            if (data != null)
            {
                var muahang_chonmua = _context.MuahangNccModel.Where(d => d.id == data.muahang_chonmua_id).Include(d => d.chitiet).Include(d => d.ncc).FirstOrDefault();
                if (muahang_chonmua != null)
                {
                    var ncc_chon = muahang_chonmua;
                    raw.Add("tonggiatri", ncc_chon.tonggiatri.Value.ToString("#,##0"));
                    raw.Add("tggh", data.date.Value.ToString("dd/MM/yyyy"));
                    var stt = 1;
                    foreach (var item in ncc_chon.chitiet)
                    {
                        var material = _context.MaterialModel.Where(d => item.hh_id == "m-" + d.id).FirstOrDefault();
                        if (material != null)
                        {
                            RawDetails.Add(new RawMuahangDetails
                            {
                                stt = stt++,
                                tenhh = item.tenhh,
                                mahh = item.mahh,
                                dvt = item.dvt,
                                soluong = item.soluong.Value.ToString("#,##0"),
                                dongia = item.dongia.Value.ToString("#,##0"),
                                thanhtien = item.thanhtien.Value.ToString("#,##0"),
                                //nhasx = material.nhasx,
                                tieuchuan = material.tieuchuan,
                                //note = item.note,
                                tggh = data.date.Value.ToString("dd/MM/yyyy")
                                //artwork = material.masothietke,
                                //date = data.date.Value.ToString("yyyy-MM-dd")
                            });
                        }
                    }
                    ///Nhà cung cấp
                    raw.Add("tenncc", ncc_chon.ncc.tenncc);
                    raw.Add("diachincc", ncc_chon.ncc.diachincc);
                    raw.Add("dienthoaincc", ncc_chon.ncc.dienthoaincc);
                    raw.Add("emailncc", ncc_chon.ncc.emailncc);
                    raw.Add("taikhoannh", ncc_chon.ncc.taikhoannh);
                    raw.Add("masothue", ncc_chon.ncc.masothue);
                    raw.Add("nguoilienhe", ncc_chon.ncc.nguoilienhe);
                    raw.Add("chucvu", ncc_chon.ncc.chucvu);

                }
                var loaithanhtoan = "Trả sau";
                if (data.loaithanhtoan == "tra_truoc")
                {
                    loaithanhtoan = "Trả trước";
                }
                raw.Add("loaithanhtoan", loaithanhtoan);
                raw.Add("ptthanhtoan", data.ptthanhtoan);
                raw.Add("diachigiaohang", data.diachigiaohang);

            }
            else
            {
                return Json(new { success = false, message = "Đề nghị mua hàng này không tồn tại!" });
            }
            System.Data.DataTable datatable_details = new System.Data.DataTable("details");
            PropertyInfo[] Props = typeof(RawMuahangDetails).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                //Setting column names as Property names
                datatable_details.Columns.Add(prop.Name);
            }
            foreach (RawMuahangDetails item in RawDetails)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    //inserting property values to datatable rows
                    values[i] = Props[i].GetValue(item, null);
                }
                datatable_details.Rows.Add(values);
            }
            ///XUẤT PDF

            //Creates Document instance
            Spire.Doc.Document document = new Spire.Doc.Document();
            //Loads the word document
            document.LoadFromFile(_configuration["Source:Path_Private"] + "/buy/templates/dondathang.docx", Spire.Doc.FileFormat.Docx);



            string[] fieldName = raw.Keys.ToArray();
            string[] fieldValue = raw.Values.ToArray();

            string[] MergeFieldNames = document.MailMerge.GetMergeFieldNames();
            string[] GroupNames = document.MailMerge.GetMergeGroupNames();

            document.MailMerge.Execute(fieldName, fieldValue);
            document.MailMerge.ExecuteWidthRegion(datatable_details);


            var timeStamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
            if (Directory.Exists(_configuration["Source:Path_Private"] + "\\buy\\muahang\\" + id))
            {
                Directory.CreateDirectory(_configuration["Source:Path_Private"] + "\\buy\\muahang\\" + id);
            }
            string url = "/private/buy/muahang/" + id + "/" + timeStamp + "-dondathang.docx";
            string url_pdf = "/private/buy/muahang/" + id + "/" + timeStamp + "-dondathang.pdf";
            document.SaveToFile(_configuration["Source:Path_Private"] + url.Replace("/private", "").Replace("/", "\\"), Spire.Doc.FileFormat.Docx);

            var url_return = url;
            //var convert_to_pdf = true;
            //if (convert_to_pdf == true)
            //{
            //    var output = ConvertWordFile(_configuration["Source:Path_Private"] + url.Replace("/private", "").Replace("/", "\\"), _configuration["Source:Path_Private"] + "\\buy\\muahang\\" + id);
            //    //if (process != "Success")
            //    if (output == false)
            //    {
            //        return Json(new { success = false });
            //    }
            //    url_return = url_pdf;

            //}

            data.dondathang = url_return;

            _context.SaveChanges();

            return Json(new { success = true });
        }
        [HttpPost]

        public async Task<JsonResult> SaveDinhkem(string note, int muahang_id)
        {
            System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            var user_id = UserManager.GetUserId(currentUser);
            var user = await UserManager.GetUserAsync(currentUser);
            //MuahangModel? MuahangModel_old;
            //MuahangModel_old = _context.MuahangModel.Where(d => d.id == muahang_id).FirstOrDefault();

            var files = Request.Form.Files;
            var now = DateTime.Now;
            var items = new List<MuahangDinhkemModel>();
            if (files != null && files.Count > 0)
            {

                foreach (var file in files)
                {
                    var timeStamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
                    string name = file.FileName;
                    string type = file.Name;
                    string ext = Path.GetExtension(name);
                    string mimeType = file.ContentType;
                    //var fileName = Path.GetFileName(name);
                    var newName = timeStamp + "-" + muahang_id + "-" + name;
                    //var muahang_id = MuahangModel_old.id;
                    newName = newName.Replace("+", "_");
                    newName = newName.Replace("%", "_");
                    var dir = _configuration["Source:Path_Private"] + "\\buy\\muahang\\" + muahang_id;
                    bool exists = Directory.Exists(dir);

                    if (!exists)
                        Directory.CreateDirectory(dir);


                    var filePath = dir + "\\" + newName;

                    string url = "/private/buy/muahang/" + muahang_id + "/" + newName;
                    items.Add(new MuahangDinhkemModel
                    {
                        note = note,
                        ext = ext,
                        url = url,
                        name = name,
                        mimeType = mimeType,
                        muahang_id = muahang_id,
                        created_at = now,
                        created_by = user_id
                    });

                    using (var fileSrteam = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(fileSrteam);
                    }
                }
                _context.AddRange(items);
                _context.SaveChanges();
            }

            return Json(new { success = true });
        }
        [HttpPost]

        public async Task<JsonResult> SaveUynhiemchi(int muahang_id)
        {
            System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            var user_id = UserManager.GetUserId(currentUser);
            //MuahangModel? MuahangModel_old;
            //MuahangModel_old = _context.MuahangModel.Where(d => d.id == muahang_id).FirstOrDefault();

            var files = Request.Form.Files;
            var now = DateTime.Now;
            var items = new List<MuahangUynhiemchiModel>();
            if (files != null && files.Count > 0)
            {

                foreach (var file in files)
                {
                    var timeStamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
                    string name = file.FileName;
                    string type = file.Name;
                    string ext = Path.GetExtension(name);
                    string mimeType = file.ContentType;
                    //var fileName = Path.GetFileName(name);
                    var newName = timeStamp + "-" + muahang_id + "-" + name;
                    //var muahang_id = MuahangModel_old.id;
                    newName = newName.Replace("+", "_");
                    newName = newName.Replace("%", "_");
                    var dir = _configuration["Source:Path_Private"] + "\\buy\\muahang\\" + muahang_id;
                    bool exists = Directory.Exists(dir);

                    if (!exists)
                        Directory.CreateDirectory(dir);


                    var filePath = dir + "\\" + newName;

                    string url = "/private/buy/muahang/" + muahang_id + "/" + newName;
                    items.Add(new MuahangUynhiemchiModel
                    {
                        ext = ext,
                        url = url,
                        name = name,
                        mimeType = mimeType,
                        muahang_id = muahang_id,
                        created_at = now,
                        created_by = user_id
                    });

                    using (var fileSrteam = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(fileSrteam);
                    }
                }
                _context.AddRange(items);
                _context.SaveChanges();
            }

            return Json(new { success = true });
        }
        [HttpPost]
        public async Task<JsonResult> Table()
        {
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            var name = Request.Form["filters[name]"].FirstOrDefault();
            var id_text = Request.Form["filters[id]"].FirstOrDefault();
            int id = id_text != null ? Convert.ToInt32(id_text) : 0;
            var type = Request.Form["type"].FirstOrDefault();
            //var tenhh = Request.Form["filters[tenhh]"].FirstOrDefault();
            int skip = start != null ? Convert.ToInt32(start) : 0;
            var customerData = _context.MuahangModel.Where(d => d.deleted_at == null);
            if (type == "thanhtoan")
            {
                customerData = customerData.Where(d => d.is_dathang == true && (d.loaithanhtoan == "tra_truoc" || (d.loaithanhtoan == "tra_sau" && d.is_nhanhang == true)));

            }
            int recordsTotal = customerData.Count();

            if (name != null && name != "")
            {
                customerData = customerData.Where(d => d.name.Contains(name));
            }
            if (id != 0)
            {
                customerData = customerData.Where(d => d.id == id);
            }
            int recordsFiltered = customerData.Count();
            var datapost = customerData.OrderByDescending(d => d.id).Skip(skip).Take(pageSize)
                .Include(d => d.user_created_by)
                .Include(d => d.muahang_chonmua)
                .ToList();
            var data = new ArrayList();
            foreach (var record in datapost)
            {
                var chonmua = record.muahang_chonmua;
                decimal? tonggiatri = null;
                if (chonmua != null)
                {
                    tonggiatri = chonmua.tonggiatri;
                }
                var data1 = new
                {
                    id = record.id,
                    code = record.code,
                    name = record.name,
                    status_id = record.status_id,
                    created_by = record.created_by,
                    user_created_by = record.user_created_by,
                    is_dathang = record.is_dathang,
                    date_finish = record.date_finish,
                    tonggiatri = tonggiatri
                };
                data.Add(data1);
            }
            var jsonData = new { draw = draw, recordsFiltered = recordsFiltered, recordsTotal = recordsTotal, data = data };
            return Json(jsonData);
        }

        [HttpPost]
        public async Task<JsonResult> xuatpdf(int id)
        {
            var now = DateTime.Now;
            var raw = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"ngay",now.ToString("dd") },
                {"thang",now.ToString("MM") },
                {"nam",now.ToString("yyyy") },
            };

            var RawDetails = new List<RawMuahangDetails>();
            var data = _context.MuahangModel.Where(d => d.id == id)
                .Include(d => d.nccs).ThenInclude(d => d.ncc).FirstOrDefault();
            if (data != null)
            {
                raw.Add("note", data.note);
                raw.Add("note_chonmua", data.note_chonmua);
                var muahang_chonmua = _context.MuahangNccModel.Where(d => d.id == data.muahang_chonmua_id).Include(d => d.chitiet).FirstOrDefault();
                if (muahang_chonmua != null)
                {
                    var ncc_chon = muahang_chonmua;
                    raw.Add("tonggiatri", ncc_chon.tonggiatri.Value.ToString("#,##0"));
                    var stt = 1;
                    foreach (var item in ncc_chon.chitiet)
                    {
                        //var material = _context.MaterialModel.Where(d => item.hh_id == "m-" + d.id).FirstOrDefault();
                        //if (material != null)
                        //{
                        RawDetails.Add(new RawMuahangDetails
                        {
                            stt = stt++,
                            tenhh = item.tenhh,
                            mahh = item.mahh,
                            dvt = item.dvt,
                            soluong = item.soluong.Value.ToString("#,##0.00"),
                            dongia = item.dongia.Value.ToString("#,##0") + " VND",
                            thanhtien = item.thanhtien.Value.ToString("#,##0") + " VND",
                            //note = item.note,
                            //artwork = material.masothietke,
                            //date = data.date.Value.ToString("yyyy-MM-dd")
                        });
                        //}
                    }
                }
                var key = 0;
                foreach (var ncc in data.nccs)
                {
                    if (data.muahang_chonmua_id == ncc.id)
                    {
                        ncc.chonmua = true;
                    }
                    raw.Add("bang_ncc_ten_" + key, ncc.ncc.tenncc);
                    raw.Add("bang_ncc_tong_" + key, ncc.tonggiatri.Value.ToString("#,##0.00"));
                    raw.Add("bang_ncc_dap_ung_" + key, ncc.dapung == true ? "X" : "");
                    raw.Add("bang_ncc_time_delivery_" + key, ncc.thoigiangiaohang);
                    raw.Add("bang_ncc_policy_" + key, ncc.baohanh);
                    raw.Add("bang_ncc_payment_" + key, ncc.thanhtoan);
                    raw.Add("bang_ncc_select_" + key, ncc.chonmua == true ? "X" : "");

                    key++;
                }
            }
            else
            {
                return Json(new { success = false, message = "Đề nghị mua hàng này không tồn tại!" });
            }
            System.Data.DataTable datatable_details = new System.Data.DataTable("details");
            PropertyInfo[] Props = typeof(RawMuahangDetails).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                //Setting column names as Property names
                datatable_details.Columns.Add(prop.Name);
            }
            foreach (RawMuahangDetails item in RawDetails)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    //inserting property values to datatable rows
                    values[i] = Props[i].GetValue(item, null);
                }
                datatable_details.Rows.Add(values);
            }
            ///XUẤT PDF

            //Creates Document instance
            Spire.Doc.Document document = new Spire.Doc.Document();
            //Loads the word document
            document.LoadFromFile(_configuration["Source:Path_Private"] + "/buy/templates/denghimuahang.docx", Spire.Doc.FileFormat.Docx);



            string[] fieldName = raw.Keys.ToArray();
            string[] fieldValue = raw.Values.ToArray();

            string[] MergeFieldNames = document.MailMerge.GetMergeFieldNames();
            string[] GroupNames = document.MailMerge.GetMergeGroupNames();

            document.MailMerge.Execute(fieldName, fieldValue);
            document.MailMerge.ExecuteWidthRegion(datatable_details);


            var timeStamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
            if (Directory.Exists(_configuration["Source:Path_Private"] + "\\buy\\muahang\\" + id))
            {
                Directory.CreateDirectory(_configuration["Source:Path_Private"] + "\\buy\\muahang\\" + id);
            }
            string url = "/private/buy/muahang/" + id + "/" + timeStamp + ".docx";
            string url_pdf = "/private/buy/muahang/" + id + "/" + timeStamp + ".pdf";
            document.SaveToFile(_configuration["Source:Path_Private"] + url.Replace("/private", "").Replace("/", "\\"), Spire.Doc.FileFormat.Docx);

            var url_return = url;
            var convert_to_pdf = true;
            if (convert_to_pdf == true)
            {
                var output = ConvertWordFile(_configuration["Source:Path_Private"] + url.Replace("/private", "").Replace("/", "\\"), _configuration["Source:Path_Private"] + "\\buy\\muahang\\" + id);
                //if (process != "Success")
                if (output == false)
                {
                    return Json(new { success = false });
                }
                url_return = url_pdf;

            }
            ////Trình ký
            data.pdf = url_return;
            data.status_id = (int)Status.MuahangPDF;
            //_context.SaveChanges();


            ///UPLOAD ESIGN
            System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            var user_id = UserManager.GetUserId(currentUser);
            var user = await UserManager.GetUserAsync(currentUser);
            ///Document
            var DocumentModel = new DocumentModel()
            {
                name_vi = data.name,
                description_vi = data.note,
                priority = 2,
                status_id = (int)DocumentStatus.Draft,
                type_id = 73,
                user_id = user_id,
                user_next_signature_id = user_id,
                is_sign_parellel = false,
                created_at = DateTime.Now,
            };
            var count_type = _context.DocumentModel.Where(d => d.type_id == DocumentModel.type_id).Count();
            var type = _context.DocumentTypeModel.Where(d => d.id == DocumentModel.type_id).Include(d => d.users_receive).FirstOrDefault();
            DocumentModel.code = type.symbol + "00" + (count_type + 1);
            _context.Add(DocumentModel);
            _context.SaveChanges();
            ///DocumentFile
            DocumentFileModel DocumentFileModel = new DocumentFileModel
            {
                document_id = DocumentModel.id,
                ext = ".pdf",
                url = url_return,
                name = data.name,
                mimeType = "application/pdf",
                created_at = DateTime.Now
            };
            _context.Add(DocumentFileModel);
            ////Đính kèm
            ///Lấy dự trù
            var list_attachment = new List<DocumentAttachmentModel>();
            var dutru = _context.MuahangChitietModel.Where(d => d.muahang_id == id).Include(d => d.dutru_chitiet).ThenInclude(d => d.dutru).Select(d => d.dutru_chitiet.dutru).ToList();
            dutru = dutru.Distinct().ToList();
            foreach (var d in dutru)
            {
                list_attachment.Add(new DocumentAttachmentModel()
                {
                    document_id = DocumentModel.id,
                    name = "Dự trù " + d.code + "- " + d.name,
                    ext = ".pdf",
                    mimeType = "application/pdf",
                    url = d.pdf,
                    created_at = DateTime.Now
                });
            }

            ///Lấy báo giá
            var baogia = _context.MuahangNccModel.Where(d => d.muahang_id == id).Include(d => d.dinhkem).Include(d => d.ncc).ToList();
            foreach (var d in baogia)
            {
                foreach (var item in d.dinhkem)
                {
                    list_attachment.Add(new DocumentAttachmentModel()
                    {
                        document_id = DocumentModel.id,
                        name = item.name,
                        ext = item.ext,
                        mimeType = item.mimeType,
                        url = item.url,
                        created_at = item.created_at
                    });
                }
            }
            _context.AddRange(list_attachment);
            ////Signature
            for (int k = 0; k < 1; ++k)
            {
                DocumentSignatureModel DocumentSignatureModel = new DocumentSignatureModel() { document_id = DocumentModel.id, user_id = user_id, stt = k };
                _context.Add(DocumentSignatureModel);
            }
            ////Receive
            if (type.users_receive.Count() > 0)
            {
                foreach (var receive in type.users_receive)
                {
                    DocumentUserReceiveModel DocumentUserReceiveModel = new DocumentUserReceiveModel() { document_id = DocumentModel.id, user_id = receive.user_id };
                    _context.Add(DocumentUserReceiveModel);
                }
            }
            /////create event
            DocumentEventModel DocumentEventModel = new DocumentEventModel
            {
                document_id = DocumentModel.id,
                event_content = "<b>" + user.FullName + "</b> tạo hồ sơ mới",
                created_at = DateTime.Now,
            };
            _context.Add(DocumentEventModel);
            /////create Related 
            RelatedEsignModel RelatedEsignModel = new RelatedEsignModel()
            {
                esign_id = DocumentModel.id,
                related_id = data.id,
                type = "muahang",
                created_at = DateTime.Now
            };
            _context.Add(RelatedEsignModel);

            //_context.SaveChanges();
            data.status_id = (int)Status.MuahangEsign;
            data.activeStep = 1;
            data.esign_id = DocumentModel.id;
            data.code = DocumentModel.code;

            _context.SaveChanges();

            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> AddComment(MuahangCommentModel CommentModel, List<string> users_related)
        {
            System.Security.Claims.ClaimsPrincipal currentUser = User;
            string user_id = UserManager.GetUserId(currentUser); // Get user id:
            var user = await UserManager.GetUserAsync(currentUser); // Get user id:
            CommentModel.user_id = user_id;
            CommentModel.created_at = DateTime.Now;
            _context.Add(CommentModel);
            _context.SaveChanges();
            var files = Request.Form.Files;

            var items_comment = new List<MuahangCommentFileModel>();
            if (files != null && files.Count > 0)
            {
                var pathroot = _configuration["Source:Path_Private"] + "\\buy\\muahang\\" + CommentModel.muahang_id + "\\";
                bool exists = Directory.Exists(pathroot);

                if (!exists)
                    Directory.CreateDirectory(pathroot);

                foreach (var file in files)
                {
                    var timeStamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
                    string name = file.FileName;
                    string ext = Path.GetExtension(name);
                    string mimeType = file.ContentType;

                    //var fileName = Path.GetFileName(name);
                    var newName = timeStamp + " - " + name;

                    newName = newName.Replace("+", "_");
                    newName = newName.Replace("%", "_");
                    newName = newName.Replace(",", "_");
                    var filePath = _configuration["Source:Path_Private"] + "\\buy\\muahang\\" + CommentModel.muahang_id + "\\" + newName;
                    string url = "/private/buy/muahang/" + CommentModel.muahang_id + "/" + newName;
                    items_comment.Add(new MuahangCommentFileModel
                    {
                        ext = ext,
                        url = url,
                        name = name,
                        mimeType = mimeType,
                        comment_id = CommentModel.id,
                        created_at = DateTime.Now
                    });

                    using (var fileSrteam = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(fileSrteam);
                    }
                }
                _context.AddRange(items_comment);
                _context.SaveChanges();
            }
            foreach (var item in users_related)
            {
                _context.Add(new MuahangCommentUserModel
                {
                    muahang_comment_id = CommentModel.id,
                    user_id = item
                });
                _context.SaveChanges();
            }
            ///lây user liên quan
            var muahang = _context.MuahangModel.Where(d => d.id == CommentModel.muahang_id).FirstOrDefault();
            var comments = _context.MuahangCommentModel.Where(d => d.muahang_id == CommentModel.muahang_id).Include(d => d.users_related).ToList();

            var users_related_id = new List<string>();
            users_related_id.Add(muahang.created_by);
            foreach (var activity in comments)
            {
                users_related_id.Add(activity.user_id);
                users_related_id.AddRange(activity.users_related.Select(d => d.user_id).ToList());
            }
            users_related_id = users_related_id.Distinct().ToList();
            var itemToRemove = users_related_id.SingleOrDefault(r => r == user_id);
            users_related_id.Remove(itemToRemove);
            //SEND MAIL
            if (users_related_id != null && users_related_id.Count() > 0)
            {
                var users_related_obj = _context.UserModel.Where(d => users_related_id.Contains(d.Id)).Select(d => d.Email).ToList();
                var mail_string = string.Join(",", users_related_obj.ToArray());
                string Domain = (HttpContext.Request.IsHttps ? "https://" : "http://") + HttpContext.Request.Host.Value;
                var attach = items_comment.Select(d => d.url).ToList();
                var text = CommentModel.comment;
                if (attach.Count() > 0 && CommentModel.comment == null)
                {
                    text = $"{user.FullName} gửi đính kèm";
                }
                var body = _view.Render("Emails/NewComment",
                    new
                    {
                        link_logo = Domain + "/images/clientlogo_astahealthcare.com_f1800.png",
                        link = Domain + "/muahang/edit/" + CommentModel.muahang_id,
                        text = text,
                        name = user.FullName
                    });

                var email = new EmailModel
                {
                    email_to = mail_string,
                    subject = "[Tin nhắn mới] " + muahang.name,
                    body = body,
                    email_type = "new_comment_purchase",
                    status = 1,
                    data_attachments = attach
                };
                _context.Add(email);
            }
            /// Audittrail
            var audit = new AuditTrailsModel();
            audit.UserId = user.Id;
            audit.Type = AuditType.Update.ToString();
            audit.DateTime = DateTime.Now;
            audit.description = $"Tài khoản {user.FullName} đã thêm bình luận.";
            _context.Add(audit);
            await _context.SaveChangesAsync();

            CommentModel.user = user;
            CommentModel.is_read = true;

            return Json(new
            {
                success = 1,
                comment = CommentModel
            });
        }

        public async Task<IActionResult> MoreComment(int id, int? from_id)
        {
            int limit = 10;
            var comments_ctx = _context.MuahangCommentModel
                .Where(d => d.muahang_id == id);
            if (from_id != null)
            {
                comments_ctx = comments_ctx.Where(d => d.id < from_id);
            }
            List<MuahangCommentModel> comments = comments_ctx.OrderByDescending(d => d.id)
                .Take(limit).Include(d => d.files).Include(d => d.user).ToList();
            //System.Security.Claims.ClaimsPrincipal currentUser = User;
            //string current_user_id = UserManager.GetUserId(currentUser); // Get user id:


            return Json(new { success = 1, comments });
        }
        [HttpPost]
        public async Task<IActionResult> thongbao(int muahang_id, List<string> list_user)
        {

            var data = _context.MuahangModel.Where(d => d.id == muahang_id).Include(d => d.chitiet).FirstOrDefault();
            //SEND MAIL
            if (list_user != null && list_user.Count() > 0 && data != null)
            {
                var stt = 1;
                foreach (var item in data.chitiet)
                {
                    item.stt = stt++;
                }
                var users_related_obj = _context.UserModel.Where(d => list_user.Contains(d.Id)).Select(d => d.Email).ToList();
                var mail_string = string.Join(",", users_related_obj.ToArray());
                string Domain = (HttpContext.Request.IsHttps ? "https://" : "http://") + HttpContext.Request.Host.Value;
                var body = _view.Render("Emails/Nhanhang",
                    new
                    {
                        link_logo = Domain + "/images/clientlogo_astahealthcare.com_f1800.png",
                        link = Domain + "/muahang/nhanhang/" + muahang_id,
                        date = data.date,
                        data = data.chitiet
                    });

                var email = new EmailModel
                {
                    email_to = mail_string,
                    subject = "[Thông báo nhận hàng] " + data.name,
                    body = body,
                    email_type = "nhanhang_purchase",
                    status = 1,
                };
                _context.Add(email);
                _context.SaveChanges();
            }
            return Json(new
            {
                success = true,
            });
        }
        [HttpPost]
        public async Task<IActionResult> thongbaothanhtoan(int muahang_id)
        {

            var data = _context.MuahangModel.Where(d => d.id == muahang_id).Include(d => d.chitiet).FirstOrDefault();
            //SEND MAIL
            if (data != null)
            {
                var stt = 1;
                foreach (var item in data.chitiet)
                {
                    item.stt = stt++;
                }

                //var users_related_obj = _context.UserModel.Where(d => list_user.Contains(d.Id)).Select(d => d.Email).ToList();
                var mail_string = "tram.nth@astahealthcare.com";
                string Domain = (HttpContext.Request.IsHttps ? "https://" : "http://") + HttpContext.Request.Host.Value;
                var body = _view.Render("Emails/Thanhtoan",
                    new
                    {
                        link_logo = Domain + "/images/clientlogo_astahealthcare.com_f1800.png",
                        link = Domain + "/muahang/edit/" + muahang_id,
                        date = data.date,
                        data = data.chitiet
                    });

                var email = new EmailModel
                {
                    email_to = mail_string,
                    subject = "[Thông báo thanh toán] " + data.name,
                    body = body,
                    email_type = "thanhtoan_purchase",
                    status = 1,
                };
                _context.Add(email);
                _context.SaveChanges();
            }
            return Json(new
            {
                success = true,
            });
        }

        private void CopyValues<T>(T target, T source)
        {
            Type t = typeof(T);

            var properties = t.GetProperties().Where(prop => prop.CanRead && prop.CanWrite);

            foreach (var prop in properties)
            {
                var value = prop.GetValue(source, null);
                //if (value != null)
                prop.SetValue(target, value, null);
            }
        }
        private bool ConvertWordFile(string file, string outputDirectory)
        {
            string libreOfficePath = _configuration["LibreOffice:Path"];
            //// FIXME: file name escaping: I have not idea how to do it in .NET.
            ProcessStartInfo procStartInfo = new ProcessStartInfo(libreOfficePath, string.Format("--convert-to pdf --nologo " + file + " --outdir " + outputDirectory));
            procStartInfo.RedirectStandardOutput = true;
            procStartInfo.UseShellExecute = false;
            procStartInfo.CreateNoWindow = true;
            procStartInfo.WorkingDirectory = Environment.CurrentDirectory;

            Process process = new Process() { StartInfo = procStartInfo, };
            process.Start();
            process.WaitForExit();

            if (process.ExitCode != 0)
                return false;
            return true;
        }


        public class RawMuahangDetails
        {
            public int stt { get; set; }

            public string tenhh { get; set; }
            public string mahh { get; set; }
            public string? dvt { get; set; }
            public string soluong { get; set; }
            public string? dongia { get; set; }
            public string? thanhtien { get; set; }
            public string? note { get; set; }
            public string? nhasx { get; set; }
            public string? tggh { get; set; }
            public string? tieuchuan { get; set; }
        }
        public class RawFile
        {
            public string link { get; set; }
            public string note { get; set; }
            public List<MuahangDinhkemModel>? list_file { get; set; }
            //public string file_url { get; set; }

            public bool is_user_upload { get; set; }

            public DateTime? created_at { get; set; }

        }
    }
}
