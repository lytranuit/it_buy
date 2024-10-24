using Microsoft.AspNetCore.Authorization;
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

    [Authorize(Roles = "Administrator,RD,KHSX")]
    public class ProductController : BaseController
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<UserModel> UserManager;
        public ProductController(ItContext context, UserManager<UserModel> UserMgr, IConfiguration configuration) : base(context)
        {
            _configuration = configuration;
            UserManager = UserMgr;
        }
        [HttpPost]
        public async Task<JsonResult> Save(ProductModel ProductModel)
        {
            var jsonData = new { success = true, message = "" };
            var properties = typeof(ProductModel).GetProperties().Where(prop => prop.CanRead && prop.CanWrite && prop.PropertyType == typeof(DateTime?));

            foreach (var prop in properties)
            {
                DateTime? value = (DateTime?)prop.GetValue(ProductModel, null);
                if (value != null && value.Value.Kind == DateTimeKind.Utc)
                {
                    value = value.Value.ToLocalTime();
                    prop.SetValue(ProductModel, value, null);
                }
            }
            try
            {
                if (ProductModel.id > 0)
                {
                    var ProductModel_old = _context.ProductModel.Where(d => d.id == ProductModel.id).FirstOrDefault();
                    CopyValues<ProductModel>(ProductModel_old, ProductModel);
                    _context.Update(ProductModel_old);
                    _context.SaveChanges();
                }
                else
                {
                    _context.Add(ProductModel);
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
                var list = _context.ProductModel.Where(d => item.Contains(d.id)).ToList();
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
            var mahh = Request.Form["filters[mahh]"].FirstOrDefault();
            var tenhh = Request.Form["filters[tenhh]"].FirstOrDefault();
            int skip = start != null ? Convert.ToInt32(start) : 0;
            var customerData = _context.ProductModel.Where(d => 1 == 1);
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
            var datapost = customerData.OrderBy(d => d.mahh).Skip(skip).Take(pageSize).ToList();
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
