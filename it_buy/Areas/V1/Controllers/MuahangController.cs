

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using Vue.Data;
using Vue.Models;

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
        public JsonResult Get(int id)
        {
            var data = _context.MuahangModel.Where(d => d.id == id).
                Include(d => d.chitiet)
                .Include(d => d.nccs).ThenInclude(d => d.chitiet)
                .Include(d => d.nccs).ThenInclude(d => d.dinhkem.Where(d => d.deleted_at == null))
                .Include(d => d.nccs).ThenInclude(d => d.ncc).FirstOrDefault();
            if (data != null)
            {
                var stt = 1;
                foreach (var item in data.chitiet)
                {
                    var material = _context.MaterialModel.Where(d => item.hh_id == "m-" + d.id).FirstOrDefault();
                    if (material != null)
                    {
                        item.tenhh = material.tenhh;
                        item.mahh = material.mahh;
                        item.stt = stt++;
                    }

                }
                foreach (var ncc in data.nccs)
                {
                    stt = 1;
                    foreach (var item in ncc.chitiet)
                    {
                        var material = _context.MaterialModel.Where(d => item.hh_id == "m-" + d.id).FirstOrDefault();
                        if (material != null)
                        {
                            item.tenhh = material.tenhh;
                            item.mahh = material.mahh;
                            item.stt = stt++;
                        }

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
                        var material = _context.MaterialModel.Where(d => item.hh_id == "m-" + d.id).FirstOrDefault();
                        if (material != null)
                        {
                            RawDetails.Add(new RawMuahangDetails
                            {
                                stt = stt++,
                                tenhh = material.tenhh,
                                mahh = material.mahh,
                                dvt = item.dvt,
                                soluong = item.soluong.Value.ToString("#,##0.00"),
                                dongia = item.dongia.Value.ToString("#,##0") + " VND",
                                thanhtien = item.thanhtien.Value.ToString("#,##0") + " VND",
                                //note = item.note,
                                //artwork = material.masothietke,
                                //date = data.date.Value.ToString("yyyy-MM-dd")
                            });
                        }
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
        }
    }
}
