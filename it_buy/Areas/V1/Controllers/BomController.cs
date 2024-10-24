using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Text.Json.Serialization;
using Vue.Data;
using Vue.Models;

namespace it_template.Areas.V1.Controllers
{

    [Authorize(Roles = "Administrator,Manager BOM")]
    public class BomController : BaseController
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<UserModel> UserManager;
        public BomController(ItContext context, UserManager<UserModel> UserMgr, IConfiguration configuration) : base(context)
        {
            _configuration = configuration;
            UserManager = UserMgr;
        }
        public async Task<JsonResult> Get(string mahh, string colo)
        {

            decimal? colo_d = colo != null ? Convert.ToDecimal(colo) : null;
            var bom = _context.BomModel.Where(d => d.mahh == mahh && d.colo == colo_d).GroupBy(d => new { d.mahh, d.colo }).Select(d => new BOM()
            {
                mahh = d.Key.mahh,
                colo = d.Key.colo.Value,
                tenhh = d.FirstOrDefault().tenhh,
                dvt = d.FirstOrDefault().dvt,
                mahh_goc = d.FirstOrDefault().mahh_goc,
                tenhh_goc = d.FirstOrDefault().tenhh_goc,
                details = d.Select(e => new BOM_DETAILS()
                {
                    stt = e.stt.Value,
                    manvl = e.manvl,
                    tennvl = e.tennvl,
                    dvtnvl = e.dvtnvl,
                    me = e.me.Value,
                    soluong = e.soluong.Value,
                    thaythe = new List<BOM_DETAILS>()
                }).OrderBy(d => d.stt).ToList()
            }).FirstOrDefault();
            foreach (var item in bom.details)
            {

                item.thaythe = _context.BomThaytheModel.Where(d => d.manvl == item.manvl && d.mahh == bom.mahh && d.colo == bom.colo).Select(e => new BOM_DETAILS()
                {
                    stt = e.stt_thaythe,
                    manvl = e.manvl_thaythe,
                    tennvl = e.tennvl_thaythe,
                    dvtnvl = e.dvtnvl_thaythe,
                    me = e.me_thaythe.Value,
                    soluong = e.soluong_thaythe.Value
                }).ToList();
            }

            return Json(bom, new System.Text.Json.JsonSerializerOptions()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
            });
        }
        [HttpPost]
        public async Task<JsonResult> Remove(string mahh, string colo)
        {
            var jsonData = new { success = true, message = "" };
            try
            {
                decimal? colo_d = colo != null ? Convert.ToDecimal(colo) : null;
                var list = _context.BomModel.Where(d => d.mahh == mahh && d.colo == colo_d).ToList();
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
        public async Task<JsonResult> Save(List<BomModel> list_add, List<BomModel>? list_update, List<BomModel>? list_delete)
        {
            try
            {
                System.Security.Claims.ClaimsPrincipal currentUser = this.User;
                var user_id = UserManager.GetUserId(currentUser);
                var user = await UserManager.GetUserAsync(currentUser);


                var list = new List<BomModel>();
                if (list_delete != null)
                    _context.RemoveRange(list_delete);
                if (list_add != null)
                {
                    foreach (var item in list_add)
                    {
                        _context.Add(item);
                        //_context.SaveChanges();
                        list.Add(item);
                    }
                }
                if (list_update != null)
                {
                    foreach (var item in list_update)
                    {
                        _context.Update(item);
                        list.Add(item);
                    }
                }


                _context.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.InnerException.Message });
            }

        }
        [HttpPost]
        public async Task<JsonResult> SaveThaythe(List<BomThaytheModel> list_add, List<BomThaytheModel>? list_update, List<BomThaytheModel>? list_delete)
        {
            try
            {
                System.Security.Claims.ClaimsPrincipal currentUser = this.User;
                var user_id = UserManager.GetUserId(currentUser);
                var user = await UserManager.GetUserAsync(currentUser);


                var list = new List<BomThaytheModel>();
                if (list_delete != null)
                    _context.RemoveRange(list_delete);
                if (list_add != null)
                {
                    foreach (var item in list_add)
                    {
                        _context.Add(item);
                        //_context.SaveChanges();
                        list.Add(item);
                    }
                }
                if (list_update != null)
                {
                    foreach (var item in list_update)
                    {
                        _context.Update(item);
                        list.Add(item);
                    }
                }


                _context.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.InnerException.Message });
            }

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
            var mahh_goc = Request.Form["filters[mahh_goc]"].FirstOrDefault();
            var tenhh_goc = Request.Form["filters[tenhh_goc]"].FirstOrDefault();
            var colo = Request.Form["filters[colo]"].FirstOrDefault();
            var mapl = Request.Form["filters[mapl]"].FirstOrDefault();
            var date_from = Request.Form["filters[dates][0]"].FirstOrDefault();
            var date_to = Request.Form["filters[dates][1]"].FirstOrDefault();
            var date0 = DateTime.Now;
            var date1 = DateTime.Now;
            if (date_from != null)
            {

                date0 = DateTime.Parse(date_from);
                if (date0.Kind == DateTimeKind.Utc)
                {
                    date0 = date0.ToLocalTime();
                }
            }
            if (date_to != null)
            {

                date1 = DateTime.Parse(date_to);
                if (date1.Kind == DateTimeKind.Utc)
                {
                    date1 = date1.ToLocalTime();
                }
            }

            int skip = start != null ? Convert.ToInt32(start) : 0;
            decimal? colo_d = colo != null ? Convert.ToDecimal(colo) : null;

            var list_tonkho = _context.PackageModel.Where(d => d.soluong > 0 && d.deleted_at == null).GroupBy(d => new { d.mahh }).Select(d => new PackageInventory()
            {
                mahh = d.Key.mahh,
                soluong = d.Sum(e => e.soluong)
            }).ToList();

            var customerData = _context.BomModel.Where(d => 1 == 1).GroupBy(d => new { d.mahh, d.colo }).Select(d => new BOM()
            {
                mahh = d.Key.mahh,
                colo = d.Key.colo.Value,
                tenhh = d.FirstOrDefault().tenhh,
                dvt = d.FirstOrDefault().dvt,
                mahh_goc = d.FirstOrDefault().mahh_goc,
                tenhh_goc = d.FirstOrDefault().tenhh_goc,
                details = d.Select(e => new BOM_DETAILS()
                {
                    stt = e.stt.Value,
                    manvl = e.manvl,
                    tennvl = e.tennvl,
                    dvtnvl = e.dvtnvl,
                    me = e.me.Value,
                    soluong = e.soluong.Value,
                    thaythe = new List<BOM_DETAILS>()
                }).ToList()
            }).ToList();



            int recordsTotal = customerData.Count();
            if (mahh != null && mahh != "")
            {
                customerData = customerData.Where(d => d.mahh.Contains(mahh)).ToList();
            }

            if (tenhh != null && tenhh != "")
            {
                customerData = customerData.Where(d => d.tenhh.Contains(tenhh)).ToList();
            }
            if (mahh_goc != null && mahh_goc != "")
            {
                customerData = customerData.Where(d => d.mahh_goc.Contains(mahh_goc)).ToList();
            }
            if (tenhh_goc != null && tenhh_goc != "")
            {
                customerData = customerData.Where(d => d.tenhh_goc.Contains(tenhh_goc)).ToList();
            }
            if (colo_d != null)
            {
                customerData = customerData.Where(d => d.colo == colo_d).ToList();
            }
            if (mapl != null && mapl != "")
            {
                var find_mahh = _context.ProductModel.Where(d => d.mapl == mapl).Select(d => d.mahh).ToList();

                customerData = customerData.Where(d => find_mahh.Contains(d.mahh)).ToList();
            }
            int recordsFiltered = customerData.Count();
            var datapost = customerData.OrderBy(d => d.mahh).Skip(skip).Take(pageSize).ToList();
            //var data = new ArrayList();
            foreach (var record in datapost)
            {
                foreach (var item in record.details)
                {
                    ///Ton
                    var ton_mahh = list_tonkho.Where(d => d.mahh == item.manvl).FirstOrDefault();
                    if (ton_mahh != null)
                    {
                        item.tonkho = ton_mahh.soluong;
                    }
                    ///
                    item.status = item.tonkho >= item.soluong;

                    ///Thay the
                    item.thaythe = _context.BomThaytheModel.Where(d => d.manvl == item.manvl && d.mahh == record.mahh && d.colo == record.colo).Select(e => new BOM_DETAILS()
                    {
                        stt = e.stt_thaythe,
                        manvl = e.manvl_thaythe,
                        tennvl = e.tennvl_thaythe,
                        dvtnvl = e.dvtnvl_thaythe,
                        me = e.me_thaythe.Value,
                        soluong = e.soluong_thaythe.Value
                    }).ToList();

                    foreach (var item1 in item.thaythe)
                    {
                        var ton_mahh_thaythe = list_tonkho.Where(d => d.mahh == item1.manvl).FirstOrDefault();
                        if (ton_mahh_thaythe != null)
                        {
                            item1.tonkho = ton_mahh_thaythe.soluong;
                        }
                        item.status = item1.tonkho >= item1.soluong;
                    }


                    ////Du tru
                    var dutru = _context.DutruChitietModel.Where(d => d.mahh == item.manvl).Include(d => d.dutru).Where(d => d.dutru.deleted_at == null && d.dutru.created_at >= date0 && d.dutru.created_at <= date1).Select(d => d.dutru).ToList();

                    item.dutru = dutru.OrderBy(d => d.created_at).Select(d => new DUTRU_DETAILS()
                    {
                        id = d.id,
                        code = d.code,
                        status_id = d.status_id.Value
                    }).ToList();


                    ////Muahang
                    var muahang = _context.MuahangChitietModel.Where(d => d.mahh == item.manvl).Include(d => d.muahang).Where(d => d.muahang.deleted_at == null && d.muahang.created_at >= date0 && d.muahang.created_at <= date1).Select(d => d.muahang).ToList();

                    item.muahang = muahang.OrderBy(d => d.created_at).Select(d => new MUAHANG_DETAILS()
                    {
                        id = d.id,
                        code = d.code,
                        status_id = d.status_id.Value,
                        is_dathang = d.is_dathang,
                        is_nhanhang = d.is_nhanhang,
                        is_thanhtoan = d.is_thanhtoan,
                        date_finish = d.date_finish,
                        loaithanhtoan = d.loaithanhtoan
                    }).ToList(); ;
                }
            }
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

        //private int Checkstatus(DutruModel d)
        //{
        //    return "Đang thực hiện"
        //}

    }
    public class BOM
    {
        public string mahh { get; set; }
        public decimal colo { get; set; }
        public string tenhh { get; set; }
        public string dvt { get; set; }
        public string mahh_goc { get; set; }
        public string tenhh_goc { get; set; }
        public List<BOM_DETAILS> details { get; set; }
    }
    public class BOM_DETAILS
    {
        public int stt { get; set; }
        public string manvl { get; set; }
        public string tennvl { get; set; }
        public string dvtnvl { get; set; }
        public double me { get; set; }
        public decimal soluong { get; set; }
        public decimal tonkho { get; set; }
        public bool status { get; set; }
        public List<BOM_DETAILS> thaythe { get; set; }

        public List<DUTRU_DETAILS> dutru { get; set; }

        public List<MUAHANG_DETAILS> muahang { get; set; }
    }

    public class PackageInventory
    {
        public string mahh { get; set; }
        public decimal soluong { get; set; }
    }


    public class DUTRU_DETAILS
    {
        public int id { get; set; }
        public string code { get; set; }

        public int status_id { get; set; }
    }
    public class MUAHANG_DETAILS
    {
        public int id { get; set; }
        public string code { get; set; }

        public int status_id { get; set; }

        public DateTime? date_finish { get; set; }

        public bool? is_dathang { get; set; }

        public bool? is_thanhtoan { get; set; }
        public bool? is_nhanhang { get; set; }
        public string? loaithanhtoan { get; set; }
    }
}
