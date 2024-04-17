

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Text.Json.Serialization;
using Vue.Data;
using Vue.Models;

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
                    if (surfix != null)
                    {
                        HangHoaModel.mahh = surfix + new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
                    }
                    _context.Add(HangHoaModel);
                    _context.SaveChanges();
                    if (surfix != null)
                    {
                        HangHoaModel.mahh = surfix + HangHoaModel.id;
                        _context.Update(HangHoaModel);
                        _context.SaveChanges();
                    }
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
            var mahh = Request.Form["filters[mahh]"].FirstOrDefault();
            var tenhh = Request.Form["filters[tenhh]"].FirstOrDefault();
            int skip = start != null ? Convert.ToInt32(start) : 0;
            var customerData = _context.MaterialModel.Where(d => d.deleted_at == null);
            int recordsTotal = customerData.Count();
            if (mahh != null && mahh != "")
            {
                customerData = customerData.Where(d => d.mahh.Contains(mahh));
            }

            if (tenhh != null && tenhh != "")
            {
                customerData = customerData.Where(d => d.tenhh.Contains(tenhh));
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
