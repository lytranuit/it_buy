

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Text.Json.Serialization;
using Vue.Data;
using Vue.Models;
using static it_template.Areas.V1.Controllers.MuahangController;

namespace it_template.Areas.V1.Controllers
{

    public class MaterialController : BaseController
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<UserModel> UserManager;
        public MaterialController(ItContext context, UserManager<UserModel> UserMgr, IConfiguration configuration) : base(context)
        {
            _configuration = configuration;
            UserManager = UserMgr;
        }
        public JsonResult Get(int id)
        {
            var data = _context.MaterialModel.Where(d => d.id == id).Include(d => d.nhacungcap).FirstOrDefault();

            return Json(data, new System.Text.Json.JsonSerializerOptions()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
            });
        }
        [HttpPost]
        public async Task<JsonResult> Save(MaterialModel HangHoaModel, string surfix)
        {
            var jsonData = new { success = true, message = "" };
            try
            {
                HangHoaModel.tenhh = HangHoaModel.tenhh.Trim();
                //HangHoaModel.dvt = HangHoaModel.dvt.Trim();
                if (HangHoaModel.id > 0)
                {
                    var HangHoaModel_old = _context.MaterialModel.Where(d => d.id == HangHoaModel.id).FirstOrDefault();
                    CopyValues<MaterialModel>(HangHoaModel_old, HangHoaModel);
                    _context.Update(HangHoaModel_old);
                    _context.SaveChanges();

                }
                else
                {
                    HangHoaModel.nhom = "Khac";
                    _context.Add(HangHoaModel);
                    _context.SaveChanges();
                    return Json(new { success = true, data = HangHoaModel });
                }
            }
            catch (Exception ex)
            {
                jsonData = new { success = false, message = ex.Message };
            }


            return Json(jsonData, new System.Text.Json.JsonSerializerOptions()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
            });
        }

        [HttpPost]
        public async Task<JsonResult> Remove(List<int> item)
        {
            var jsonData = new { success = true, message = "" };
            try
            {
                var list = _context.MaterialModel.Where(d => item.Contains(d.id)).ToList();
                foreach (var i in list)
                {
                    i.deleted_at = DateTime.Now;
                }
                _context.UpdateRange(list);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                jsonData = new { success = false, message = ex.Message };
            }


            return Json(jsonData, new System.Text.Json.JsonSerializerOptions()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
            });
        }
        [HttpPost]
        public async Task<JsonResult> Table()
        {
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            var mancc = Request.Form["filters[mancc]"].FirstOrDefault();
            var mansx = Request.Form["filters[mansx]"].FirstOrDefault();
            var mahh = Request.Form["filters[mahh]"].FirstOrDefault();
            var tenhh = Request.Form["filters[tenhh]"].FirstOrDefault();
            var nhom = Request.Form["filters[nhom]"].FirstOrDefault();
            int skip = start != null ? Convert.ToInt32(start) : 0;
            var customerData = _context.MaterialModel.Where(d => d.deleted_at == null);
            int recordsTotal = customerData.Count();
            if (mancc != null && mancc != "")
            {
                customerData = customerData.Where(d => d.mancc == mancc);
            }
            if (mansx != null && mansx != "")
            {
                customerData = customerData.Where(d => d.mansx == mansx);
            }
            if (mahh != null && mahh != "")
            {
                customerData = customerData.Where(d => d.mahh == mahh);
            }

            if (tenhh != null && tenhh != "")
            {
                customerData = customerData.Where(d => d.tenhh.Contains(tenhh));
            }

            if (nhom != null && nhom != "")
            {
                customerData = customerData.Where(d => d.nhom == nhom);
            }
            int recordsFiltered = customerData.Count();
            var datapost = customerData.Include(d => d.nhasanxuat).Include(d => d.nhacungcap).OrderBy(d => d.mahh).Skip(skip).Take(pageSize).ToList();
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
            return Json(jsonData, new System.Text.Json.JsonSerializerOptions()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
            });
        }
        //public async Task<JsonResult> nhasx()
        //{
        //    var data = _QLSXcontext.NhasanxuatModel.ToList();
        //    var list = new List<SelectResponse>();
        //    foreach (var kv in data)
        //    {
        //        var label = kv.TenNSX != "" ? kv.TenNSX : kv.TenNSX_VN;
        //        var Select = new SelectResponse
        //        {

        //            id = kv.MaNSX,
        //            label = kv.MaNSX + " - " + label,
        //        };

        //        list.Add(Select);
        //    }
        //    //var All = GetChild(0);
        //    //var jsonData = new { data = ProcessModel };
        //    return Json(list);
        //}

        //public async Task<JsonResult> nhacc()
        //{

        //    var data = _QLSXcontext.NhacungcapModel.ToList();
        //    var list = new List<SelectResponse>();
        //    foreach (var kv in data)
        //    {
        //        var label = kv.TenNCC != "" ? kv.TenNCC : kv.TenNCC_VN;
        //        var Select = new SelectResponse
        //        {

        //            id = kv.MaNCC,
        //            label = kv.MaNCC + " - " + label,
        //        };

        //        list.Add(Select);
        //    }
        //    //var All = GetChild(0);
        //    //var jsonData = new { data = ProcessModel };
        //    return Json(list);
        //}

        public JsonResult GetFiles(string mahh)
        {
            var data = new List<RawFileMaterial>();

            ///File up
            var files_up = _context.MaterialDinhkemModel.Where(d => d.mahh == mahh && d.deleted_at == null).Include(d => d.user_created_by).ToList();
            if (files_up.Count > 0)
            {
                data.AddRange(files_up.GroupBy(d => new { d.note, d.created_at }).Select(d => new RawFileMaterial
                {
                    note = d.First().note,
                    list_file = d.ToList(),
                    is_user_upload = true,
                    created_at = d.Key.created_at
                }).ToList());
            }

            ///Sort
            data = data.OrderBy(d => d.created_at).ToList();
            return Json(data, new System.Text.Json.JsonSerializerOptions()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
            });
        }
        [HttpPost]
        public async Task<JsonResult> xoadinhkem(List<int> list_id)
        {
            var Model = _context.MaterialDinhkemModel.Where(d => list_id.Contains(d.id)).ToList();
            foreach (var item in Model)
            {
                item.deleted_at = DateTime.Now;
            }
            _context.UpdateRange(Model);
            _context.SaveChanges();
            return Json(new { success = true });
        }

        [HttpPost]

        public async Task<JsonResult> SaveDinhkem(string note, string mahh)
        {
            System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            var user_id = UserManager.GetUserId(currentUser);
            var user = await UserManager.GetUserAsync(currentUser);
            //MuahangModel? MuahangModel_old;
            //MuahangModel_old = _context.MuahangModel.Where(d => d.id == muahang_id).FirstOrDefault();

            var files = Request.Form.Files;
            var now = DateTime.Now;
            var items = new List<MaterialDinhkemModel>();
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
                    var newName = timeStamp + "-" + mahh + "-" + name;
                    //var muahang_id = MuahangModel_old.id;
                    newName = newName.Replace("+", "_");
                    newName = newName.Replace("%", "_");
                    var dir = _configuration["Source:Path_Private"] + "\\materials\\" + mahh;
                    bool exists = Directory.Exists(dir);

                    if (!exists)
                        Directory.CreateDirectory(dir);


                    var filePath = dir + "\\" + newName;

                    string url = "/private/materials/" + mahh + "/" + newName;
                    items.Add(new MaterialDinhkemModel
                    {
                        note = note,
                        ext = ext,
                        url = url,
                        name = name,
                        mimeType = mimeType,
                        mahh = mahh,
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


        public class RawFileMaterial
        {
            public string link { get; set; }
            public string note { get; set; }
            public List<MaterialDinhkemModel>? list_file { get; set; }
            //public string file_url { get; set; }

            public bool is_user_upload { get; set; }

            public DateTime? created_at { get; set; }

        }
        public class SelectResponse
        {
            public string id { get; set; }
            public string label { get; set; }

            public string name { get; set; }
            public virtual List<SelectResponse>? children { get; set; }
        }
        public class SelectResponseNVL
        {
            public string id { get; set; }
            public string label { get; set; }

            public string tenhh { get; set; }
            public string dvt
            {
                get; set;
            }
            public string mansx { get; set; }
            public string mancc { get; set; }
            public string masothietke { get; set; }
            public string nhom { get; set; }
            public virtual List<SelectResponse>? children { get; set; }
        }
        public void CopyValues<T>(T target, T source)
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
    }
}
