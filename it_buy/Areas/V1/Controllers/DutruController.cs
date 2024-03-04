

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NuGet.Packaging.Signing;
using Org.BouncyCastle.Utilities;
using Spire.Xls;
using System;
using System.Collections;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Vue.Data;
using Vue.Models;
using static it_template.Areas.V1.Controllers.UserController;

namespace it_template.Areas.V1.Controllers
{

    public class DutruController : BaseController
    {
        private readonly IConfiguration _configuration;
        private UserManager<UserModel> UserManager;
        public DutruController(ItContext context, IConfiguration configuration, UserManager<UserModel> UserMgr) : base(context)
        {
            _configuration = configuration;
            UserManager = UserMgr;
        }
        public JsonResult Get(int id)
        {
            var data = _context.DutruModel.Where(d => d.id == id).Include(d => d.chitiet).FirstOrDefault();
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
            }
            return Json(data);
        }

        [HttpPost]
        public async Task<JsonResult> Delete(int id)
        {
            var Model = _context.DutruModel.Where(d => d.id == id).FirstOrDefault();
            Model.deleted_at = DateTime.Now;
            _context.Update(Model);
            _context.SaveChanges();
            return Json(Model);
        }
        [HttpPost]
        public async Task<JsonResult> Save(DutruModel DutruModel, List<DutruChitietModel> list_add, List<DutruChitietModel>? list_update, List<DutruChitietModel>? list_delete)
        {
            try
            {
                System.Security.Claims.ClaimsPrincipal currentUser = this.User;
                var user_id = UserManager.GetUserId(currentUser);
                var user = await UserManager.GetUserAsync(currentUser);
                DutruModel? DutruModel_old;
                if (DutruModel.date != null && DutruModel.date.Value.Kind == DateTimeKind.Utc)
                {
                    DutruModel.date = DutruModel.date.Value.ToLocalTime();
                }
                if (DutruModel.id == 0)
                {
                    DutruModel.created_at = DateTime.Now;
                    DutruModel.status_id = (int)Status.Draft;
                    DutruModel.created_by = user_id;
                    DutruModel.activeStep = 0;

                    _context.DutruModel.Add(DutruModel);

                    _context.SaveChanges();

                    DutruModel_old = DutruModel;

                }
                else
                {

                    DutruModel_old = _context.DutruModel.Where(d => d.id == DutruModel.id).FirstOrDefault();
                    CopyValues<DutruModel>(DutruModel_old, DutruModel);
                    DutruModel_old.updated_at = DateTime.Now;

                    _context.Update(DutruModel_old);
                    _context.SaveChanges();
                }

                if (list_delete != null)
                    _context.RemoveRange(list_delete);
                if (list_add != null)
                {
                    foreach (var item in list_add)
                    {
                        item.dutru_id = DutruModel_old.id;
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

                return Json(new { success = true, id = DutruModel_old.id });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }

        }

        [HttpPost]
        public async Task<JsonResult> xuatpdf(int id)
        {
            var RawDetails = new List<RawDetails>();
            var data = _context.DutruModel.Where(d => d.id == id).Include(d => d.chitiet).FirstOrDefault();
            if (data != null)
            {
                var stt = 1;
                foreach (var item in data.chitiet)
                {
                    var material = _context.MaterialModel.Where(d => item.hh_id == "m-" + d.id).FirstOrDefault();
                    if (material != null)
                    {
                        //stt { get; set; }

                        //public string tennvl { get; set; }
                        //public string manvl { get; set; }
                        //public string artwork { get; set; }
                        //public string dvt { get; set; }
                        //public string soluong { get; set; }
                        //public string date { get; set; }
                        //public string note { get; set; }
                        RawDetails.Add(new RawDetails
                        {
                            stt = stt++,
                            tennvl = material.tenhh,
                            manvl = material.mahh,
                            dvt = item.dvt,
                            soluong = item.soluong.Value.ToString("#,##0.00"),
                            note = item.note,
                            artwork = material.masothietke,
                            date = data.date.Value.ToString("yyyy-MM-dd")
                        });
                    }
                }
            }
            else
            {
                return Json(new { success = false, message = "Dự trù không tồn tại!" });
            }
            System.Data.DataTable datatable_details = new System.Data.DataTable("details");
            PropertyInfo[] Props = typeof(RawDetails).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                //Setting column names as Property names
                datatable_details.Columns.Add(prop.Name);
            }
            foreach (RawDetails item in RawDetails)
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
            document.LoadFromFile(_configuration["Source:Path_Private"] + "/buy/templates/dutru_nvl.docx", Spire.Doc.FileFormat.Docx);


            var now = DateTime.Now;
            var raw = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"ngay",now.ToString("dd") },
                {"thang",now.ToString("MM") },
                {"nam",now.ToString("yyyy") },
            };


            string[] fieldName = raw.Keys.ToArray();
            string[] fieldValue = raw.Values.ToArray();

            string[] MergeFieldNames = document.MailMerge.GetMergeFieldNames();
            string[] GroupNames = document.MailMerge.GetMergeGroupNames();

            document.MailMerge.Execute(fieldName, fieldValue);
            document.MailMerge.ExecuteWidthRegion(datatable_details);


            var timeStamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
            if (Directory.Exists(_configuration["Source:Path_Private"] + "\\buy\\dutru\\" + id))
            {
                Directory.CreateDirectory(_configuration["Source:Path_Private"] + "\\buy\\dutru\\" + id);
            }
            string url = "/private/buy/dutru/" + id + "/" + timeStamp + ".docx";
            string url_pdf = "/private/buy/dutru/" + id + "/" + timeStamp + ".pdf";
            document.SaveToFile(_configuration["Source:Path_Private"] + url.Replace("/private", "").Replace("/", "\\"), Spire.Doc.FileFormat.Docx);

            var url_return = url;
            var convert_to_pdf = true;
            if (convert_to_pdf == true)
            {
                var output = ConvertWordFile(_configuration["Source:Path_Private"] + url.Replace("/private", "").Replace("/", "\\"), _configuration["Source:Path_Private"] + "\\buy\\dutru\\" + id);
                //if (process != "Success")
                if (output == false)
                {
                    return Json(new { success = false });
                }
                url_return = url_pdf;

            }
            ////Trình ký
            data.pdf = url_return;
            data.status_id = (int)Status.PDF;
            //_context.SaveChanges();
            System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            var user_id = UserManager.GetUserId(currentUser);
            var queue = new QueueModel()
            {
                status_id = 1,
                created_at = DateTime.Now,
                created_by = user_id,
                type = "create_esign_dutru",
                valueQ = new QueueValue()
                {
                    dutru = data
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
            var customerData = _context.DutruModel.Where(d => d.deleted_at == null);
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
        public async Task<JsonResult> TableChitiet()
        {
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            var type = Request.Form["type"].FirstOrDefault();
            var mahh = Request.Form["filters[mahh]"].FirstOrDefault();
            var tenhh = Request.Form["filters[tenhh]"].FirstOrDefault();
            var list_dutru = Request.Form["filters[list_dutru]"].FirstOrDefault();
            //var tenhh = Request.Form["filters[tenhh]"].FirstOrDefault();
            int skip = start != null ? Convert.ToInt32(start) : 0;
            var customerData = _context.DutruChitietModel
                .Include(d => d.muahang_chitiet).ThenInclude(d => d.muahang)
                .Include(d => d.dutru).Where(d => d.dutru.status_id == (int)Status.EsignSuccess);
            int recordsTotal = customerData.Count();
            if (mahh != null && mahh != "")
            {
                var listhh = _context.MaterialModel.Where(d => d.mahh.Contains(mahh)).Select(d => "m-" + d.id).ToList();
                customerData = customerData.Where(d => listhh.Contains(d.hh_id));
            }
            if (tenhh != null && tenhh != "")
            {
                var listhh = _context.MaterialModel.Where(d => d.tenhh.Contains(tenhh)).Select(d => "m-" + d.id).ToList();
                customerData = customerData.Where(d => listhh.Contains(d.hh_id));
            }
            if (list_dutru != null && list_dutru != "")
            {
                var listdutru = _context.DutruModel.Where(d => d.code.Contains(list_dutru)).Select(d => d.id).ToList();
                customerData = customerData.Where(d => listdutru.Contains(d.dutru_id));
            }

            var datapost = customerData.ToList();
            var data = new ArrayList();
            int recordsFiltered = datapost.Count();
            foreach (var record in datapost)
            {
                var dutru = record.dutru;
                var tenhh1 = "";
                var mahh1 = "";
                if (dutru.type_id == 1)
                {
                    var material = _context.MaterialModel.Where(d => record.hh_id == "m-" + d.id).FirstOrDefault();
                    if (material != null)
                    {
                        tenhh1 = material.tenhh;
                        mahh1 = material.mahh;
                    }
                }
                var soluong_dutru = record.soluong;
                var soluong_mua = record.muahang_chitiet.Sum(d => d.soluong);

                var soluong = soluong_mua < soluong_dutru ? soluong_dutru - soluong_mua : 0;
                data.Add(new
                {
                    hh_id = record.hh_id,
                    soluong_dutru = soluong_dutru,
                    soluong_mua = soluong_mua,
                    mahh = mahh1,
                    tenhh = tenhh1,
                    dvt = record.dvt,
                    soluong = soluong,
                    dutru_chitiet_id = record.id,
                    list_dutru = new List<DutruModel>() { dutru },
                    list_muahang = record.muahang_chitiet.Select(d => d.muahang).ToList(),
                });
            }

            var jsonData = new { draw = draw, recordsFiltered = recordsFiltered, recordsTotal = recordsTotal, data = data };
            return Json(jsonData);
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


    }
    public class RawDetails
    {
        public int stt { get; set; }

        public string tennvl { get; set; }
        public string manvl { get; set; }
        public string? artwork { get; set; }
        public string? dvt { get; set; }
        public string soluong { get; set; }
        public string? ngaysx { get; set; }
        public string? date { get; set; }
        public string? note { get; set; }
    }

}
