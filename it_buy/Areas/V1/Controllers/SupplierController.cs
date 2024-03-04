using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using Vue.Data;
using Vue.Models;

namespace it_template.Areas.V1.Controllers
{

    public class SupplierController : BaseController
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<UserModel> UserManager;
        public SupplierController(ItContext context, UserManager<UserModel> UserMgr, IConfiguration configuration) : base(context)
        {
            _configuration = configuration;
            UserManager = UserMgr;
        }
        [HttpPost]
        public async Task<JsonResult> Save(NhacungcapModel NhacungcapModel, string old_key)
        {
            var jsonData = new { success = true, message = "" };
            try
            {
                if (old_key == null)
                {
                    _context.Add(NhacungcapModel);
                    _context.SaveChanges();
                }
                else
                {
                    var NhacungcapModel_old = _context.NhacungcapModel.Where(d => d.mancc == old_key).FirstOrDefault();
                    CopyValues<NhacungcapModel>(NhacungcapModel_old, NhacungcapModel);
                    _context.Update(NhacungcapModel_old);
                    _context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                jsonData = new { success = false, message = ex.Message };
            }


            return Json(jsonData);
        }

        [HttpPost]
        public async Task<JsonResult> Remove(List<string> item)
        {
            var jsonData = new { success = true, message = "" };
            try
            {
                var list = _context.NhacungcapModel.Where(d => item.Contains(d.mancc)).ToList();
                _context.RemoveRange(list);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                jsonData = new { success = false, message = ex.Message };
            }


            return Json(jsonData);
        }
        [HttpPost]
        public async Task<JsonResult> Table()
        {
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            var maNCC = Request.Form["filters[maNCC]"].FirstOrDefault();
            var tenNCC = Request.Form["filters[tenNCC]"].FirstOrDefault();
            var tenNCC_VN = Request.Form["filters[tenNCC_VN]"].FirstOrDefault();
            int skip = start != null ? Convert.ToInt32(start) : 0;
            var customerData = _context.NhacungcapModel.Where(d => 1 == 1);
            int recordsTotal = customerData.Count();
            if (maNCC != null && maNCC != "")
            {
                customerData = customerData.Where(d => d.mancc.Contains(maNCC));
            }

            if (tenNCC != null && tenNCC != "")
            {
                customerData = customerData.Where(d => d.tenncc.Contains(tenNCC));
            }
            if (tenNCC_VN != null && tenNCC_VN != "")
            {
                customerData = customerData.Where(d => d.tenncc_vn.Contains(tenNCC_VN));
            }
            int recordsFiltered = customerData.Count();
            var datapost = customerData.OrderBy(d => d.mancc).Skip(skip).Take(pageSize).ToList();
            //var data = new ArrayList();
            //foreach (var record in datapost)
            //{
            //	var data1 = new
            //	{
            //		MaNCC = record.MaNCC,
            //		TenNCC = record.TenNCC,
            //		TenNCC_VN = record.TenNCC_VN
            //	};
            //	data.Add(data1);
            //}
            var jsonData = new { draw = draw, recordsFiltered = recordsFiltered, recordsTotal = recordsTotal, data = datapost };
            return Json(jsonData);
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
