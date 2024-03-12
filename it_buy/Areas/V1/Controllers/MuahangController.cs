

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using System.Security.Policy;
using Vue.Data;
using Vue.Models;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;

namespace it_template.Areas.V1.Controllers
{

    public class MuahangController : BaseController
    {
        private readonly IConfiguration _configuration;
        private UserManager<UserModel> UserManager;
        public MuahangController(ItContext context, IConfiguration configuration, UserManager<UserModel> UserMgr) : base(context)
        {
            _configuration = configuration;
            UserManager = UserMgr;
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
                var file = new RawFile()
                {
                    note = "Báo giá của " + d.ncc.tenncc,
                    list_file = list_file,
                    is_user_upload = false,
                    created_at = created_at
                };
                data.Add(file);

            }
            return Json(data);
        }
        public JsonResult Get(int id)
        {
            var data = _context.MuahangModel.Where(d => d.id == id)
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
                    item.stt = stt++;
                }
                foreach (var ncc in data.nccs)
                {
                    stt = 1;
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

        [HttpPost]
        public async Task<JsonResult> xoadinhkem(int id)
        {
            var Model = _context.MuahangNccDinhkemModel.Where(d => d.id == id).FirstOrDefault();
            Model.deleted_at = DateTime.Now;
            _context.Update(Model);
            _context.SaveChanges();
            return Json(new { success = true });
        }
        [HttpPost]
        public async Task<JsonResult> xoadondathang(int id)
        {
            var Model = _context.MuahangDondathangModel.Where(d => d.id == id).FirstOrDefault();
            Model.deleted_at = DateTime.Now;
            _context.Update(Model);
            _context.SaveChanges();
            return Json(new { success = true });
        }
        [HttpPost]
        public async Task<JsonResult> xoathanhtoan(int id)
        {
            var Model = _context.MuahangThanhtoanModel.Where(d => d.id == id).FirstOrDefault();
            Model.deleted_at = DateTime.Now;
            _context.Update(Model);
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
            try
            {
                System.Security.Claims.ClaimsPrincipal currentUser = this.User;
                var user_id = UserManager.GetUserId(currentUser);
                var user = await UserManager.GetUserAsync(currentUser);
                MuahangModel? MuahangModel_old;
                if (MuahangModel.date != null && MuahangModel.date.Value.Kind == DateTimeKind.Utc)
                {
                    MuahangModel.date = MuahangModel.date.Value.ToLocalTime();
                }
                if (MuahangModel.id == 0)
                {
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

                    MuahangModel_old = _context.MuahangModel.Where(d => d.id == MuahangModel.id).FirstOrDefault();
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

                _context.SaveChanges();

                return Json(new { success = true, id = MuahangModel_old.id });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }

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
                            created_at = DateTime.Now
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
                        //var material = _context.MaterialModel.Where(d => item.hh_id == "m-" + d.id).FirstOrDefault();
                        //if (material != null)
                        //{
                        RawDetails.Add(new RawMuahangDetails
                        {
                            stt = stt++,
                            tenhh = item.tenhh,
                            mahh = item.mahh,
                            dvt = item.dvt,
                            soluong = item.soluong.Value.ToString("#,##0"),
                            dongia = item.dongia.Value.ToString("#,##0"),
                            thanhtien = item.thanhtien.Value.ToString("#,##0"),
                            //note = item.note,
                            tggh = data.date.Value.ToString("dd/MM/yyyy")
                            //artwork = material.masothietke,
                            //date = data.date.Value.ToString("yyyy-MM-dd")
                        }); ;
                        //}
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
        //[HttpPost]

        //public async Task<JsonResult> SaveThanhtoan(MuahangModel MuahangModel)
        //{
        //    System.Security.Claims.ClaimsPrincipal currentUser = this.User;
        //    var user_id = UserManager.GetUserId(currentUser);
        //    var user = await UserManager.GetUserAsync(currentUser);
        //    MuahangModel? MuahangModel_old;
        //    if (MuahangModel.date != null && MuahangModel.date.Value.Kind == DateTimeKind.Utc)
        //    {
        //        MuahangModel.date = MuahangModel.date.Value.ToLocalTime();
        //    }

        //    MuahangModel_old = _context.MuahangModel.Where(d => d.id == MuahangModel.id).FirstOrDefault();
        //    CopyValues<MuahangModel>(MuahangModel_old, MuahangModel);
        //    MuahangModel_old.updated_at = DateTime.Now;

        //    _context.Update(MuahangModel_old);
        //    _context.SaveChanges();

        //    var files = Request.Form.Files;

        //    var items = new List<MuahangThanhtoanModel>();
        //    if (files != null && files.Count > 0)
        //    {

        //        foreach (var file in files)
        //        {
        //            var timeStamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
        //            string name = file.FileName;
        //            string type = file.Name;
        //            string ext = Path.GetExtension(name);
        //            string mimeType = file.ContentType;
        //            //var fileName = Path.GetFileName(name);
        //            var newName = timeStamp + "-" + MuahangModel_old.id + "-" + name;
        //            var muahang_id = MuahangModel_old.id;
        //            newName = newName.Replace("+", "_");
        //            newName = newName.Replace("%", "_");
        //            var dir = _configuration["Source:Path_Private"] + "\\buy\\muahang\\" + muahang_id;
        //            bool exists = Directory.Exists(dir);

        //            if (!exists)
        //                Directory.CreateDirectory(dir);


        //            var filePath = dir + "\\" + newName;

        //            string url = "/private/buy/muahang/" + muahang_id + "/" + newName;
        //            items.Add(new MuahangThanhtoanModel
        //            {
        //                ext = ext,
        //                url = url,
        //                name = name,
        //                mimeType = mimeType,
        //                muahang_id = muahang_id,
        //                created_at = DateTime.Now
        //            });

        //            using (var fileSrteam = new FileStream(filePath, FileMode.Create))
        //            {
        //                await file.CopyToAsync(fileSrteam);
        //            }
        //        }
        //        _context.AddRange(items);
        //        _context.SaveChanges();
        //    }


        //    /////Update Finish
        //    var muahang = _context.MuahangModel.Where(d => d.id == MuahangModel_old.id).Include(d => d.chitiet).FirstOrDefault();
        //    var list_nhanhang = muahang.chitiet.Where(d => d.status_nhanhang == 1).Count();

        //    if (list_nhanhang == muahang.chitiet.Count() && muahang.is_thanhtoan == true)
        //    {
        //        muahang.date_finish = DateTime.Now;
        //        _context.Update(muahang);
        //    }
        //    _context.SaveChanges();
        //    return Json(new { success = true });
        //}
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
            //var tenhh = Request.Form["filters[tenhh]"].FirstOrDefault();
            int skip = start != null ? Convert.ToInt32(start) : 0;
            var customerData = _context.MuahangModel.Where(d => d.deleted_at == null);
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
            var datapost = customerData.OrderByDescending(d => d.id).Skip(skip).Take(pageSize).Include(d => d.user_created_by).ToList();
            //var data = new ArrayList();
            //foreach (var record in datapost)
            //{
            //	var ngaythietke = record.ngaythietke != null ? record.ngaythietke.Value.ToString("yyyy-MM-dd") : null;
            //	var ngaysodk = record.ngaysodk != null ? record.ngaysodk.Value.ToString("yyyy-MM-dd") : null;
            //	var ngayhethanthietke = record.ngayhethanthietke != null ? record.ngayhethanthietke.Value.ToString("yyyy-MM-dd") : null;
            //	var data1 = new
            //	{
            //		mahh = record.mahh,
            //		tenhh = record.tenhh,
            //		dvt = record.dvt,
            //		mansx = record.mansx,
            //		mancc = record.mancc,
            //		tennvlgoc = record.tennvlgoc,
            //		masothietke = record.masothietke,
            //		ghichu_thietke = record.ghichu_thietke,
            //		masodk = record.masodk,
            //		ghichu_sodk = record.ghichu_sodk,
            //		nhuongquyen = record.nhuongquyen,
            //		ngaythietke = ngaythietke,
            //		ngaysodk = ngaysodk,
            //		ngayhethanthietke = ngayhethanthietke
            //	};
            //	data.Add(data1);
            //}
            var jsonData = new { draw = draw, recordsFiltered = recordsFiltered, recordsTotal = recordsTotal, data = datapost };
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
            System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            var user_id = UserManager.GetUserId(currentUser);
            var queue = new QueueModel()
            {
                status_id = 1,
                created_at = DateTime.Now,
                created_by = user_id,
                type = "create_esign_muahang",
                valueQ = new QueueValue()
                {
                    muahang = data
                }
            };
            _context.Add(queue);
            _context.Update(data);
            _context.SaveChanges();

            data.esign_id = queue.id;

            _context.SaveChanges();

            return Json(new { success = true, queue = queue.id });
        }

        [HttpPost]
        public async Task<IActionResult> AddComment(MuahangCommentModel CommentModel)
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

            ///create unread
            //var ExecutionModel = _context.ExecutionModel.Where(d => d.id == CommentModel.execution_id).FirstOrDefault();
            //var Activities = _context.ActivityModel
            //            .Where(d => d.execution_id == CommentModel.execution_id)
            //            .ToList();
            //var users_related = new List<string>();
            //foreach (var activity in Activities)
            //{
            //    if (activity.blocking == true)
            //    {
            //        var list_reciever = _workflow.getListReciever(activity).Select(d => d.Id).ToList();
            //        users_related.AddRange(list_reciever);
            //    }
            //    else
            //    {
            //        users_related.Add(activity.created_by);
            //    }
            //}
            //users_related = users_related.Distinct().ToList();
            //var itemToRemove = users_related.SingleOrDefault(r => r == user_id);
            //users_related.Remove(itemToRemove);
            ////SEND MAIL
            //if (users_related != null)
            //{
            //    var users_related_obj = _context.UserModel.Where(d => users_related.Contains(d.Id)).Select(d => d.Email).ToList();
            //    var mail_string = string.Join(",", users_related_obj.ToArray());
            //    string Domain = (HttpContext.Request.IsHttps ? "https://" : "http://") + HttpContext.Request.Host.Value;
            //    var attach = items_comment.Select(d => d.url).ToList();
            //    var text = CommentModel.comment;
            //    if (attach.Count() > 0 && CommentModel.comment == null)
            //    {
            //        text = $"{user.FullName} gửi đính kèm";
            //    }
            //    var body = _view.Render("Emails/NewComment",
            //        new
            //        {
            //            link_logo = Domain + "/images/clientlogo_astahealthcare.com_f1800.png",
            //            link = Domain + "/execution/details/" + ExecutionModel.process_version_id + "?execution_id=" + ExecutionModel.id,
            //            text = text,
            //            name = user.FullName
            //        });

            //    var email = new EmailModel
            //    {
            //        email_to = mail_string,
            //        subject = "[Tin nhắn mới] " + ExecutionModel.title,
            //        body = body,
            //        email_type = "new_comment_document",
            //        status = 1,
            //        data_attachments = attach
            //    };
            //    _context.Add(email);
            //}
            ////await _context.SaveChangesAsync();

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
