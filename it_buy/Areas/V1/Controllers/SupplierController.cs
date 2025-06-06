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
        public async Task<JsonResult> Save(NhacungcapModel NhacungcapModel)
        {
            var jsonData = new { success = true, message = "" };
            try
            {
                if (NhacungcapModel.id > 0)
                {
                    var NhacungcapModel_old = _context.NhacungcapModel.Where(d => d.id == NhacungcapModel.id).FirstOrDefault();
                    CopyValues<NhacungcapModel>(NhacungcapModel_old, NhacungcapModel);
                    _context.Update(NhacungcapModel_old);
                    _context.SaveChanges();
                }
                else
                {
                    NhacungcapModel.nhom = "Hanghoa";
                    _context.Add(NhacungcapModel);
                    _context.SaveChanges();

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
                var list = _context.NhacungcapModel.Where(d => item.Contains(d.id)).ToList();
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
            var tenncc = Request.Form["filters[tenncc]"].FirstOrDefault();

            var sort_mancc = Request.Form["sorts[mancc]"].FirstOrDefault();
            var sort_tenncc = Request.Form["sorts[tenncc]"].FirstOrDefault();

            int skip = start != null ? Convert.ToInt32(start) : 0;
            var customerData = _context.NhacungcapModel.Where(d => 1 == 1);
            int recordsTotal = customerData.Count();
            if (mancc != null && mancc != "")
            {
                customerData = customerData.Where(d => d.mancc.Contains(mancc));
            }

            if (tenncc != null && tenncc != "")
            {
                customerData = customerData.Where(d => d.tenncc.Contains(tenncc));
            }
            int recordsFiltered = customerData.Count();

            if (sort_mancc != null)
            {
                if (sort_mancc == "1")
                {
                    customerData = customerData.OrderBy(d => d.mancc);
                }
                else if (sort_mancc == "-1")
                {
                    customerData = customerData.OrderByDescending(d => d.mancc);
                }
            }
            else if (sort_tenncc != null)
            {
                if (sort_tenncc == "1")
                {
                    customerData = customerData.OrderBy(d => d.tenncc);
                }
                else if (sort_tenncc == "-1")
                {
                    customerData = customerData.OrderByDescending(d => d.tenncc);
                }
            }
            else
            {
                customerData = customerData.OrderBy(d => d.mancc);
            }
            var datapost = customerData.Skip(skip).Take(pageSize).ToList();
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
            return Json(jsonData, new System.Text.Json.JsonSerializerOptions()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
            });
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
