

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Spire.Xls;
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
            data.list_sp = _context.MaterialModel.Where(d => d.group != null && d.group != "" && d.group == data.group).Select(d => d.mahh).ToList();
            return Json(data, new System.Text.Json.JsonSerializerOptions()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
            });
        }
        [HttpPost]
        public async Task<JsonResult> Save(MaterialModel HangHoaModel, string surfix, List<string>? list_sp)
        {
            System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            var user_id = UserManager.GetUserId(currentUser);
            var jsonData = new { success = true, message = "" };
            try
            {
                HangHoaModel.tenhh = HangHoaModel.tenhh.Trim();
                MaterialModel? HangHoaModel_old;
                //HangHoaModel.dvt = HangHoaModel.dvt.Trim();
                if (HangHoaModel.id > 0)
                {
                    HangHoaModel_old = _context.MaterialModel.Where(d => d.id == HangHoaModel.id).FirstOrDefault();
                    CopyValues<MaterialModel>(HangHoaModel_old, HangHoaModel);
                    _context.Update(HangHoaModel_old);
                    _context.SaveChanges();

                }
                else
                {
                    HangHoaModel.nhom = HangHoaModel.nhom != null ? HangHoaModel.nhom : "Khac";
                    HangHoaModel.created_at = DateTime.Now;
                    HangHoaModel.created_by = user_id;
                    _context.Add(HangHoaModel);
                    _context.SaveChanges();
                    HangHoaModel_old = HangHoaModel;
                }
                var new_group = Guid.NewGuid().ToString();
                HangHoaModel_old.group = new_group;
                _context.Update(HangHoaModel_old);
                if (list_sp.Count() > 0)
                {
                    foreach (var item in list_sp)
                    {
                        var hh = _context.MaterialModel.Where(d => d.mahh == item).FirstOrDefault();
                        hh.group = new_group;
                        _context.Update(hh);
                    }
                }
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
        public async Task<JsonResult> uploadImage(string mahh)
        {
            System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            var user_id = UserManager.GetUserId(currentUser);
            var hh = _context.MaterialModel.Where(d => d.mahh == mahh).FirstOrDefault();
            var files = Request.Form.Files;
            //return Json(files);
            if (files != null && files.Count > 0)
            {
                var file = files[0];
                var timeStamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
                string name = file.FileName;
                string ext = Path.GetExtension(name);
                string mimeType = file.ContentType;

                //var fileName = Path.GetFileName(name);
                var newName = timeStamp + " - " + name;
                newName = newName.Replace("+", "_");
                newName = newName.Replace("%", "_");
                newName = newName.Replace("#", "_");
                var filePath = _configuration["Source:Path_Private"] + "\\materials\\" + newName;
                string url = "/private/materials/" + newName;


                using (var fileSrteam = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileSrteam);
                }
                hh.image_url = url;
                _context.Update(hh);
                _context.SaveChanges();
            }

            var jsonData = new { success = true };
            return Json(jsonData, new System.Text.Json.JsonSerializerOptions()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
            });
        }

        [HttpPost]
        public async Task<JsonResult> Remove(List<string> item)
        {
            var jsonData = new { success = true, message = "" };
            try
            {
                var list = _context.MaterialModel.Where(d => item.Contains(d.mahh)).ToList();

                _context.RemoveRange(list);
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

            var sort_mahh = Request.Form["sorts[mahh]"].FirstOrDefault();
            var sort_tenhh = Request.Form["sorts[tenhh]"].FirstOrDefault();

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
                customerData = customerData.Where(d => d.mahh.Contains(mahh));
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


            if (sort_mahh != null)
            {
                if (sort_mahh == "1")
                {
                    customerData = customerData.OrderBy(d => d.mahh);
                }
                else if (sort_mahh == "-1")
                {
                    customerData = customerData.OrderByDescending(d => d.mahh);
                }
            }
            else if (sort_tenhh != null)
            {
                if (sort_tenhh == "1")
                {
                    customerData = customerData.OrderBy(d => d.tenhh);
                }
                else if (sort_tenhh == "-1")
                {
                    customerData = customerData.OrderByDescending(d => d.tenhh);
                }
            }
            else
            {
                customerData = customerData.OrderByDescending(d => d.created_at);
            }


            var datapost = customerData.Include(d => d.nhasanxuat).Include(d => d.nhacungcap).Skip(skip).Take(pageSize).ToList();
            //var data = new ArrayList();
            //foreach (var record in datapost)
            //{
            //    record.list_sp = _context.MaterialModel.Where(d => d.group != null && d.group == record.group).Select(d => d.mahh).ToList();

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

        public async Task<IActionResult> importmahh()
        {
            return Ok();
            // Khởi tạo workbook để đọc
            Spire.Xls.Workbook book = new Spire.Xls.Workbook();
            book.LoadFromFile("./wwwroot/report/excel/DS HH.xlsx", ExcelVersion.Version2013);

            Spire.Xls.Worksheet sheet = book.Worksheets[0];
            var lastrow = sheet.LastDataRow;
            var lastcol = sheet.LastDataColumn;
            // nếu vẫn chưa gặp end thì vẫn lấy data
            Console.WriteLine(lastrow);
            //var list_Result = new List<ResultModel>();
            var newlist = new List<string>();
            var error = new List<string>();
            var list_add = new List<MaterialModel>();
            for (int rowIndex = 5; rowIndex < lastrow; rowIndex++)
            {
                // lấy row hiện tại
                var nowRow = sheet.Rows[rowIndex];
                if (nowRow == null)
                    continue;
                // vì ta dùng 3 cột A, B, C => data của ta sẽ như sau
                //int numcount = nowRow.Cells.Count;
                //for(int y = 0;y<numcount - 1 ;y++)
                //var nowRowVitri = sheet.Rows[8];

                //var code_vitri = nowRow.Cells[15] != null ? nowRow.Cells[15].Value : null;
                //if (code_vitri == null || code_vitri == "")
                //    continue;
                var mahh = nowRow.Cells[2] != null ? nowRow.Cells[2].DisplayedText : null;
                if (mahh == null || mahh == "")
                    continue;
                var hh = _context.MaterialModel.Where(d => d.mahh == mahh).FirstOrDefault();
                if (hh != null)
                {
                    error.Add(mahh);
                    continue;///Lỗi
				}
                string manhom = mahh.Length >= 4 ? mahh.Substring(0, 4) : mahh;
                var tenhh = nowRow.Cells[3] != null ? nowRow.Cells[3].Value : null;
                var dvt = nowRow.Cells[4] != null ? nowRow.Cells[4].Value : null;


                var new_hh = new MaterialModel()
                {
                    mahh = mahh,
                    tenhh = tenhh,
                    dvt = dvt,
                    nhom = manhom,
                    created_at = DateTime.Now
                };

                list_add.Add(new_hh);

            }
            _context.AddRange(list_add);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, error });
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
