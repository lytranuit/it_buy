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

    public class ProducerController : BaseController
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<UserModel> UserManager;
        public ProducerController(ItContext context, UserManager<UserModel> UserMgr, IConfiguration configuration) : base(context)
        {
            _configuration = configuration;
            UserManager = UserMgr;
        }
        [HttpPost]
        public async Task<JsonResult> Save(NsxModel NsxModel)
        {
            var jsonData = new { success = true, message = "" };
            try
            {
                if (NsxModel.id > 0)
                {
                    var NsxModel_old = _context.NsxModel.Where(d => d.id == NsxModel.id).FirstOrDefault();
                    CopyValues<NsxModel>(NsxModel_old, NsxModel);
                    _context.Update(NsxModel_old);
                    _context.SaveChanges();
                }
                else
                {
                    _context.Add(NsxModel);
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
                var list = _context.NsxModel.Where(d => item.Contains(d.id)).ToList();
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
            var mansx = Request.Form["filters[mansx]"].FirstOrDefault();
            var tennsx = Request.Form["filters[tennsx]"].FirstOrDefault();
            int skip = start != null ? Convert.ToInt32(start) : 0;
            var customerData = _context.NsxModel.Where(d => 1 == 1);
            int recordsTotal = customerData.Count();
            if (mansx != null && mansx != "")
            {
                customerData = customerData.Where(d => d.mansx.Contains(mansx));
            }

            if (tennsx != null && tennsx != "")
            {
                customerData = customerData.Where(d => d.tennsx.Contains(tennsx));
            }
            int recordsFiltered = customerData.Count();
            var datapost = customerData.OrderBy(d => d.mansx).Skip(skip).Take(pageSize).ToList();
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
