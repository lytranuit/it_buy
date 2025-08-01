﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using Spire.Xls;
using System.Collections;
using System.Data;
using System.Linq.Expressions;
using System.Text.Json.Serialization;
using Vue.Data;
using Vue.Models;
using static iText.Svg.SvgConstants;

namespace it_template.Areas.V1.Controllers
{

    public class VattuController : BaseController
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<UserModel> UserManager;
        private QLSXContext _QLSXcontext;
        public VattuController(ItContext context, QLSXContext QLSXContext, UserManager<UserModel> UserMgr, IConfiguration configuration) : base(context)
        {
            _configuration = configuration;
            UserManager = UserMgr;
            _QLSXcontext = QLSXContext;
        }

        [HttpPost]
        public async Task<JsonResult> Table()
        {
            System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            var user_id = UserManager.GetUserId(currentUser);
            var user = await UserManager.GetUserAsync(currentUser);
            var warehouse = user.warehouses_vt;

            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            var mahh = Request.Form["filters[mahh]"].FirstOrDefault();
            var tenhh = Request.Form["filters[tenhh]"].FirstOrDefault();
            var tenncc = Request.Form["filters[tenncc]"].FirstOrDefault();
            var mancc = Request.Form["filters[mancc]"].FirstOrDefault();
            var tennsx = Request.Form["filters[tennsx]"].FirstOrDefault();
            var mansx = Request.Form["filters[mansx]"].FirstOrDefault();
            var malo = Request.Form["filters[malo]"].FirstOrDefault();
            var makho = Request.Form["filters[makho]"].FirstOrDefault();
            var ngay = Request.Form["filters[ngay]"].FirstOrDefault();
            int skip = start != null ? Convert.ToInt32(start) : 0;
            DateTime start_date = ngay != null ? DateTime.Parse(ngay).AddDays(1).AddTicks(-1) : DateTime.Now.Date.AddDays(1).AddTicks(-1);
            //var hanghoa = _context.MaterialModel.Where(d => d.deleted_at == null).ToList();
            var nhap = _QLSXcontext.VattuNhapChiTietModel.Include(d => d.hoadon).Where(d => d.hoadon != null && d.hoadon.deleted_at == null && d.hoadon.ngaylap <= start_date && warehouse.Contains(d.hoadon.makho)).AsNoTracking().ToList();
            var dieuchuyen = _QLSXcontext.VattuDieuchuyenChiTietModel.Include(d => d.hoadon).Where(d => d.hoadon != null && d.hoadon.deleted_at == null && d.hoadon.ngaylap <= start_date && warehouse.Contains(d.hoadon.noidi)).AsNoTracking().ToList();
            var customerData = nhap.Select(d => new VattuInventory()
            {
                mahh = d.mahh,
                //tenhh = hanghoa.Where(e => e.mahh == d.mahh).FirstOrDefault()?.tenhh,
                //dvt = hanghoa.Where(e => e.mahh == d.mahh).FirstOrDefault()?.dvt,
                malo = d.malo,
                handung = d.handung,
                mancc = d.mancc,
                makho = d.hoadon.makho,
                soluong = d.soluong,
            }).ToList();
            customerData = customerData.Concat(dieuchuyen.Where(d => d.kt_xuat == true).Select(d => new VattuInventory() /// Xuất
            {
                mahh = d.mahh,
                //tenhh = d.package.tenhh,
                //dvt = d.package.dvt,
                malo = d.malo,
                handung = d.handung,
                mancc = d.mancc,
                makho = d.hoadon.noidi,
                soluong = -d.soluong,
            }).ToList()).ToList();
            var list_hh = _context.MaterialModel.Where(d => d.deleted_at == null).ToList();
            var list_ncc = _context.NhacungcapModel.ToList();
            var list_nsx = _context.NsxModel.ToList();
            var datalist = customerData.GroupBy(d => new { d.mahh, d.mancc, d.makho, d.malo, d.handung }).Select(d => new VattuInventory()
            {
                mahh = d.Key.mahh,
                makho = d.Key.makho,
                mancc = d.Key.mancc,
                malo = d.Key.malo,
                handung = d.Key.handung,
                //tenhh = list_hh.Where(e => e.mahh == d.Key.mahh).FirstOrDefault()?.tenhh,
                //dvt = list_hh.Where(e => e.mahh == d.Key.mahh).FirstOrDefault()?.dvt,
                //tenncc = list_ncc.Where(e => e.mancc == d.Key.mancc).FirstOrDefault()?.tenncc,
                //mansx = list_hh.Where(e => e.mahh == d.Key.mahh).FirstOrDefault()?.mansx,
                //tennsx = list_nsx.Where(f => f.mansx == list_hh.Where(e => e.mahh == d.Key.mahh).FirstOrDefault()?.mansx).FirstOrDefault()?.tennsx,
                soluong = d.Sum(e => e.soluong),
            }).Where(d => d.soluong != 0 && warehouse.Contains(d.makho)).ToList();


            foreach (var d in datalist)
            {
                var ncc = list_ncc.Where(e => e.mancc == d.mancc).FirstOrDefault();
                if (ncc != null)
                {
                    d.tenncc = ncc.tenncc;
                }

                var hh = list_hh.Where(e => e.mahh == d.mahh).FirstOrDefault();
                if (hh != null)
                {
                    d.mansx = hh.mansx;
                    d.tenhh = hh.tenhh;
                    d.dvt = hh.dvt;
                    var nsx = list_nsx.Where(f => f.mansx == hh.mansx).FirstOrDefault();
                    d.tennsx = nsx != null ? nsx.tennsx : null;
                }
            }

            int recordsTotal = datalist.Count();
            if (mahh != null && mahh != "")
            {
                datalist = datalist.Where(d => d.mahh.Contains(mahh)).ToList();
            }

            if (tenhh != null && tenhh != "")
            {
                datalist = datalist.Where(d => d.tenhh != null && d.tenhh.Contains(tenhh)).ToList();
            }
            if (mancc != null && mancc != "")
            {
                datalist = datalist.Where(d => d.mancc == mancc).ToList();
            }
            if (tenncc != null && tenncc != "")
            {
                datalist = datalist.Where(d => d.tenncc != null && d.tenncc.Contains(tenncc)).ToList();
            }
            if (mansx != null && mansx != "")
            {
                datalist = datalist.Where(d => d.mansx == mansx).ToList();
            }
            if (tennsx != null && tennsx != "")
            {
                datalist = datalist.Where(d => d.tennsx != null && d.tennsx.Contains(tennsx)).ToList();
            }
            if (malo != null && malo != "")
            {
                datalist = datalist.Where(d => d.malo == malo).ToList();
            }
            if (makho != null && makho != "")
            {
                datalist = datalist.Where(d => d.makho == makho).ToList();
            }
            int recordsFiltered = datalist.Count();
            var datapost = datalist.OrderBy(d => d.mahh).Skip(skip).Take(pageSize).ToList();
            //var data = new ArrayList();
            //foreach (var record in datapost)
            //{
            //    //var hh =  _context.MaterialModel.Where(d => d.mahh == ).ToList();
            //}
            var jsonData = new { draw = draw, recordsFiltered = recordsFiltered, recordsTotal = recordsTotal, data = datapost };
            return Json(jsonData, new System.Text.Json.JsonSerializerOptions()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
            });
        }
        [HttpPost]
        public async Task<JsonResult> TableNhap()
        {
            System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            var user_id = UserManager.GetUserId(currentUser);
            var user = await UserManager.GetUserAsync(currentUser);
            var warehouse = user.warehouses_vt;

            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            var sohd = Request.Form["filters[sohd]"].FirstOrDefault();
            var id_string = Request.Form["filters[id]"].FirstOrDefault();
            var user_created = Request.Form["filters[user_created]"].FirstOrDefault();
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int id = id_string != null ? Convert.ToInt32(id_string) : 0;
            //var hanghoa = _context.MaterialModel.Where(d => d.deleted_at == null).ToList();
            var datalist = _QLSXcontext.VattuNhapModel.Where(d => d.deleted_at == null && warehouse.Contains(d.makho));


            int recordsTotal = datalist.Count();
            if (sohd != null && sohd != "")
            {
                datalist = datalist.Where(d => d.sohd.Contains(sohd));
            }

            if (id != null && id > 0)
            {
                datalist = datalist.Where(d => d.id == id);
            }
            if (user_created != null && user_created != "")
            {
                var list_user_id = _context.UserModel.Where(d => d.FullName.Contains(user_created)).Select(d => d.Id).ToList();
                datalist = datalist.Where(d => list_user_id.Contains(d.created_by));
            }
            int recordsFiltered = datalist.Count();
            var datapost = datalist.OrderByDescending(d => d.created_at).Skip(skip).Take(pageSize).ToList();
            //var data = new ArrayList();
            foreach (var record in datapost)
            {
                record.user_created = _context.UserModel.Where(d => d.Email == record.created_by).FirstOrDefault();
            }
            var jsonData = new { draw = draw, recordsFiltered = recordsFiltered, recordsTotal = recordsTotal, data = datapost };
            return Json(jsonData, new System.Text.Json.JsonSerializerOptions()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
            });
        }
        [HttpPost]
        public async Task<JsonResult> TableNhapChitiet()
        {
            System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            var user_id = UserManager.GetUserId(currentUser);
            var user = await UserManager.GetUserAsync(currentUser);
            var warehouse = user.warehouses_vt;

            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            var sohd = Request.Form["filters[sohd]"].FirstOrDefault();
            var mahh = Request.Form["filters[mahh]"].FirstOrDefault();
            var malo = Request.Form["filters[malo]"].FirstOrDefault();
            var makho = Request.Form["filters[makho]"].FirstOrDefault();
            int skip = start != null ? Convert.ToInt32(start) : 0;
            //var hanghoa = _context.MaterialModel.Where(d => d.deleted_at == null).ToList();
            var datalist = _QLSXcontext.VattuNhapChiTietModel.Include(d => d.hoadon).Where(d => d.hoadon.deleted_at == null && warehouse.Contains(d.hoadon.makho));


            int recordsTotal = datalist.Count();
            if (sohd != null && sohd != "")
            {
                datalist = datalist.Where(d => d.sohd.Contains(sohd));
            }
            if (mahh != null && mahh != "")
            {
                datalist = datalist.Where(d => d.mahh == mahh);
            }
            if (malo != null && malo != "")
            {
                datalist = datalist.Where(d => d.malo == malo);
            }
            if (makho != null && makho != "")
            {
                datalist = datalist.Where(d => d.hoadon.makho == makho);
            }
            int recordsFiltered = datalist.Count();
            var datapost = datalist.OrderByDescending(d => d.hoadon.ngaylap).ThenByDescending(d => d.mahh).Skip(skip).Take(pageSize).ToList();
            //var data = new ArrayList();
            foreach (var record in datapost)
            {
                var hh = _context.MaterialModel.Where(d => d.mahh == record.mahh).FirstOrDefault();
                record.tenhh = hh != null ? hh.tenhh : null;
                record.dvt = hh != null ? hh.dvt : null;
            }
            var jsonData = new { draw = draw, recordsFiltered = recordsFiltered, recordsTotal = recordsTotal, data = datapost };
            return Json(jsonData, new System.Text.Json.JsonSerializerOptions()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
            });
        }
        [HttpPost]
        public async Task<JsonResult> saveNhap(VattuNhapModel VattuNhapModel, List<VattuNhapChiTietModel>? list_add, List<VattuNhapChiTietModel>? list_update, List<VattuNhapChiTietModel>? list_delete)
        {
            System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            var user = await UserManager.GetUserAsync(currentUser); // Get user
            var Timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
            //var list_hdnhaptrave = new List<NHAPModel>();
            var properties = typeof(VattuNhapModel).GetProperties().Where(prop => prop.CanRead && prop.CanWrite && prop.PropertyType == typeof(DateTime?));

            foreach (var prop in properties)
            {
                DateTime? value = (DateTime?)prop.GetValue(VattuNhapModel, null);
                if (value != null && value.Value.Kind == DateTimeKind.Utc)
                {
                    value = value.Value.ToLocalTime();
                    prop.SetValue(VattuNhapModel, value, null);
                }
            }
            if (VattuNhapModel.id > 0)
            {
                var VattuNhapModel_old = _QLSXcontext.VattuNhapModel.Where(d => d.id == VattuNhapModel.id).FirstOrDefault();
                CopyValues<VattuNhapModel>(VattuNhapModel_old, VattuNhapModel);

                _QLSXcontext.Update(VattuNhapModel_old);
                _QLSXcontext.SaveChanges();
            }
            else
            {
                int count = _QLSXcontext.VattuNhapModel.Where(d => d.created_at.Value.Date == DateTime.Now.Date).Count();
                count = count + 1;
                var sohd = "NHAP_" + DateTime.Now.Date.ToString("ddMMyy") + "_" + count.ToString("D3");
                var DTA_NHAPXUAT_old = _QLSXcontext.VattuNhapModel.Where(d => d.sohd == sohd).FirstOrDefault();
                if (DTA_NHAPXUAT_old != null)
                {
                    return Json(new { success = false, message = "Số HD đã tồn tại!" });

                }
                VattuNhapModel.ngaylap = VattuNhapModel.ngaylap.Value.Date;
                VattuNhapModel.sohd = sohd;
                VattuNhapModel.created_by = user.Email;
                VattuNhapModel.created_at = DateTime.Now;
                _QLSXcontext.Add(VattuNhapModel);

                _QLSXcontext.SaveChanges();

            }
            var list = new List<VattuNhapChiTietModel>();
            if (list_delete != null)
                _QLSXcontext.RemoveRange(list_delete);
            if (list_add != null)
            {
                foreach (var item in list_add)
                {
                    item.sohd = VattuNhapModel.sohd;
                    //item.ngaylaphd = VattuNhapModel.ngaylap;
                    //item.created_by = VattuNhapModel.created_by;
                    if (item.handung != null && item.handung.Value.Kind == DateTimeKind.Utc)
                    {
                        item.handung = item.handung.Value.ToLocalTime();
                    }

                    _QLSXcontext.Add(item);
                    //_QLSXcontext.SaveChanges();
                    list.Add(item);
                }
            }
            if (list_update != null)
            {
                foreach (var item in list_update)
                {
                    if (item.handung != null && item.handung.Value.Kind == DateTimeKind.Utc)
                    {
                        item.handung = item.handung.Value.ToLocalTime();
                    }
                    _QLSXcontext.Update(item);
                    list.Add(item);
                }
            }


            _QLSXcontext.SaveChanges();

            return Json(new { success = true, id = VattuNhapModel.id });
        }
        [HttpPost]
        public async Task<JsonResult> removeNhap(int id)
        {
            var nhap = _QLSXcontext.VattuNhapModel.Where(d => d.id == id).FirstOrDefault();
            nhap.deleted_at = DateTime.Now;
            _QLSXcontext.Update(nhap);
            _QLSXcontext.SaveChanges();
            return Json(new { success = true });
        }


        public async Task<JsonResult> getNhap(int id)
        {
            var data = _QLSXcontext.VattuNhapModel.Where(d => d.id == id).Include(d => d.chitiet).FirstOrDefault();
            return Json(data);
        }
        [HttpPost]
        public async Task<JsonResult> TableXuat()
        {
            System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            var user_id = UserManager.GetUserId(currentUser);
            var user = await UserManager.GetUserAsync(currentUser);
            var warehouse = user.warehouses_vt;

            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            var sohd = Request.Form["filters[sohd]"].FirstOrDefault();
            var id_string = Request.Form["filters[id]"].FirstOrDefault();
            var user_created = Request.Form["filters[user_created]"].FirstOrDefault();
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int id = id_string != null ? Convert.ToInt32(id_string) : 0;
            //var hanghoa = _context.MaterialModel.Where(d => d.deleted_at == null).ToList();
            var datalist = _QLSXcontext.VattuDieuchuyenModel.Where(d => d.deleted_at == null && warehouse.Contains(d.noidi));


            int recordsTotal = datalist.Count();
            if (sohd != null && sohd != "")
            {
                datalist = datalist.Where(d => d.sohd.Contains(sohd));
            }

            if (id != null && id > 0)
            {
                datalist = datalist.Where(d => d.id == id);
            }
            if (user_created != null && user_created != "")
            {
                var list_user_id = _context.UserModel.Where(d => d.FullName.Contains(user_created)).Select(d => d.Id).ToList();
                datalist = datalist.Where(d => list_user_id.Contains(d.created_by));
            }
            int recordsFiltered = datalist.Count();
            var datapost = datalist.OrderByDescending(d => d.created_at).Skip(skip).Take(pageSize).ToList();
            //var data = new ArrayList();
            foreach (var record in datapost)
            {
                record.user_created = _context.UserModel.Where(d => d.Email == record.created_by).FirstOrDefault();
            }
            var jsonData = new { draw = draw, recordsFiltered = recordsFiltered, recordsTotal = recordsTotal, data = datapost };
            return Json(jsonData, new System.Text.Json.JsonSerializerOptions()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
            });
        }

        [HttpPost]
        public async Task<JsonResult> TableXuatChitiet()
        {
            System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            var user_id = UserManager.GetUserId(currentUser);
            var user = await UserManager.GetUserAsync(currentUser);
            var warehouse = user.warehouses_vt;

            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            var sohd = Request.Form["filters[sohd]"].FirstOrDefault();
            var mahh = Request.Form["filters[mahh]"].FirstOrDefault();
            var noidi = Request.Form["filters[noidi]"].FirstOrDefault();
            var noiden = Request.Form["filters[noiden]"].FirstOrDefault();
            var malo = Request.Form["filters[malo]"].FirstOrDefault();
            int skip = start != null ? Convert.ToInt32(start) : 0;
            //var hanghoa = _context.MaterialModel.Where(d => d.deleted_at == null).ToList();
            var datalist = _QLSXcontext.VattuDieuchuyenChiTietModel.Include(d => d.hoadon).Where(d => d.hoadon.deleted_at == null && warehouse.Contains(d.hoadon.noidi));


            int recordsTotal = datalist.Count();
            if (sohd != null && sohd != "")
            {
                datalist = datalist.Where(d => d.sohd.Contains(sohd));
            }
            if (mahh != null && mahh != "")
            {
                datalist = datalist.Where(d => d.mahh == mahh);
            }
            if (noidi != null && noidi != "")
            {
                datalist = datalist.Where(d => d.hoadon.noidi == noidi);
            }
            if (malo != null && malo != "")
            {
                datalist = datalist.Where(d => d.malo == malo);
            }
            if (noiden != null && noiden != "")
            {
                datalist = datalist.Where(d => d.hoadon.noiden == noiden);
            }
            int recordsFiltered = datalist.Count();
            var datapost = datalist.OrderByDescending(d => d.hoadon.ngaylap).ThenByDescending(d => d.mahh).Skip(skip).Take(pageSize).ToList();
            //var data = new ArrayList();
            foreach (var record in datapost)
            {
                var hh = _context.MaterialModel.Where(d => d.mahh == record.mahh).FirstOrDefault();
                record.tenhh = hh != null ? hh.tenhh : null;
                record.dvt = hh != null ? hh.dvt : null;
            }
            var jsonData = new { draw = draw, recordsFiltered = recordsFiltered, recordsTotal = recordsTotal, data = datapost };
            return Json(jsonData, new System.Text.Json.JsonSerializerOptions()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
            });
        }
        [HttpPost]
        public async Task<JsonResult> saveXuat(VattuDieuchuyenModel VattuDieuchuyenModel, List<VattuDieuchuyenChiTietModel>? list_add, List<VattuDieuchuyenChiTietModel>? list_update, List<VattuDieuchuyenChiTietModel>? list_delete)
        {
            System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            var user = await UserManager.GetUserAsync(currentUser); // Get user
            var Timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
            //var list_hdXuattrave = new List<XuatModel>();
            var properties = typeof(VattuDieuchuyenModel).GetProperties().Where(prop => prop.CanRead && prop.CanWrite && prop.PropertyType == typeof(DateTime?));

            foreach (var prop in properties)
            {
                DateTime? value = (DateTime?)prop.GetValue(VattuDieuchuyenModel, null);
                if (value != null && value.Value.Kind == DateTimeKind.Utc)
                {
                    value = value.Value.ToLocalTime();
                    prop.SetValue(VattuDieuchuyenModel, value, null);
                }
            }
            if (VattuDieuchuyenModel.id > 0)
            {
                var VattuDieuchuyenModel_old = _QLSXcontext.VattuDieuchuyenModel.Where(d => d.id == VattuDieuchuyenModel.id).FirstOrDefault();
                CopyValues<VattuDieuchuyenModel>(VattuDieuchuyenModel_old, VattuDieuchuyenModel);

                _QLSXcontext.Update(VattuDieuchuyenModel_old);
                _QLSXcontext.SaveChanges();
            }
            else
            {
                int count = _QLSXcontext.VattuDieuchuyenModel.Where(d => d.created_at.Value.Date == DateTime.Now.Date).Count();
                count = count + 1;
                var sohd = "XUAT_" + DateTime.Now.Date.ToString("ddMMyy") + "_" + count.ToString("D3");
                var DTA_XuatXUAT_old = _QLSXcontext.VattuDieuchuyenModel.Where(d => d.sohd == sohd).FirstOrDefault();
                if (DTA_XuatXUAT_old != null)
                {
                    return Json(new { success = false, message = "Số HD đã tồn tại!" });

                }
                VattuDieuchuyenModel.ngaylap = VattuDieuchuyenModel.ngaylap.Value.Date;
                VattuDieuchuyenModel.sohd = sohd;
                VattuDieuchuyenModel.created_by = user.Email;
                VattuDieuchuyenModel.created_at = DateTime.Now;
                _QLSXcontext.Add(VattuDieuchuyenModel);

                _QLSXcontext.SaveChanges();

            }
            var list = new List<VattuDieuchuyenChiTietModel>();
            if (list_delete != null)
                _QLSXcontext.RemoveRange(list_delete);
            if (list_add != null)
            {
                foreach (var item in list_add)
                {
                    item.sohd = VattuDieuchuyenModel.sohd;
                    //item.ngaylaphd = VattuDieuchuyenModel.ngaylap;
                    //item.created_by = VattuDieuchuyenModel.created_by;
                    if (item.kt_xuat == true)
                        item.user_xuat = user.Email;
                    if (item.handung != null && item.handung.Value.Kind == DateTimeKind.Utc)
                    {
                        item.handung = item.handung.Value.ToLocalTime();
                    }
                    _QLSXcontext.Add(item);
                    //_QLSXcontext.SaveChanges();
                    list.Add(item);
                }
            }
            if (list_update != null)
            {
                foreach (var item in list_update)
                {
                    if (item.handung != null && item.handung.Value.Kind == DateTimeKind.Utc)
                    {
                        item.handung = item.handung.Value.ToLocalTime();
                    }
                    _QLSXcontext.Update(item);
                    list.Add(item);
                }
            }


            _QLSXcontext.SaveChanges();

            ///Xóa phiếu cũ
            //var DTA_HOADON_XUAT = _QLSXcontext.DTA_HOADON_XUAT.Where(d => d.sohd.StartsWith(VattuDieuchuyenModel.sohd)).ToList();
            //var DTA_CT_HOADON_XUAT = _QLSXcontext.DTA_CT_HOADON_XUAT.Where(d => d.sohd == VattuDieuchuyenModel.sohd).ToList();
            //var HOADON_KHOA = DTA_HOADON_XUAT.Where(d => d.khoa == true).Count();
            //if (HOADON_KHOA == 0)
            //{
            //    _QLSXcontext.RemoveRange(DTA_CT_HOADON_XUAT);
            //    _QLSXcontext.RemoveRange(DTA_HOADON_XUAT);
            //    _QLSXcontext.SaveChanges();

            //    ////Thêm phiếu mới
            //    var khoden = _QLSXcontext.KhoModel.Where(d => d.makho == VattuDieuchuyenModel.noiden).FirstOrDefault();
            //    var DTA_HOADON_XUAT_NEW = new DTA_HOADON_XUAT()
            //    {
            //        sohd = VattuDieuchuyenModel.sohd,
            //        ngaylaphd = VattuDieuchuyenModel.ngaylap,
            //        mapl = VattuDieuchuyenModel.noidi,
            //        donvi = khoden != null ? khoden.tenkho : "",
            //        makh = VattuDieuchuyenModel.noiden,
            //        mach = VattuDieuchuyenModel.created_by,
            //        tennguoigd = VattuDieuchuyenModel.ghichu,
            //        pl = VattuDieuchuyenModel.pl,
            //        noixuat = VattuDieuchuyenModel.noidi,
            //    };
            //    _QLSXcontext.Add(DTA_HOADON_XUAT_NEW);

            //    var stt = 1;
            //    foreach (var item in list)
            //    {
            //        var hh = _context.MaterialModel.Where(d => d.mahh == item.mahh).FirstOrDefault();
            //        var DTA_CT_HOADON_XUAT_NEW = new DTA_CT_HOADON_XUAT()
            //        {
            //            kt = true,
            //            sohd = DTA_HOADON_XUAT_NEW.sohd,
            //            ngaylaphd = DTA_HOADON_XUAT_NEW.ngaylaphd,
            //            mach = DTA_HOADON_XUAT_NEW.mach,
            //            mahh = item.mahh,
            //            tenhh = hh.tenhh,
            //            malo = item.malo,
            //            handung = item.malo,
            //            dvt = hh.dvt,
            //            soluong = item.soluong,
            //            stt = stt++,
            //            ghichu = item.ghichu,

            //        };
            //        _QLSXcontext.Add(DTA_CT_HOADON_XUAT_NEW);
            //    }

            //    _QLSXcontext.SaveChanges();
            //}


            return Json(new { success = true, id = VattuDieuchuyenModel.id });
        }
        [HttpPost]
        public async Task<JsonResult> removeXuat(int id)
        {
            var Xuat = _QLSXcontext.VattuDieuchuyenModel.Where(d => d.id == id).FirstOrDefault();
            Xuat.deleted_at = DateTime.Now;
            _QLSXcontext.Update(Xuat);
            _QLSXcontext.SaveChanges();

            ///Xóa phiếu cũ
            //var DTA_HOADON_XUAT = _QLSXcontext.DTA_HOADON_XUAT.Where(d => d.sohd.StartsWith(Xuat.sohd)).ToList();
            //var DTA_CT_HOADON_XUAT = _QLSXcontext.DTA_CT_HOADON_XUAT.Where(d => d.sohd == Xuat.sohd).ToList();
            //_QLSXcontext.RemoveRange(DTA_CT_HOADON_XUAT);
            //_QLSXcontext.RemoveRange(DTA_HOADON_XUAT);
            //_QLSXcontext.SaveChanges();



            return Json(new { success = true });
        }
        [HttpPost]
        public async Task<JsonResult> xacnhanNhap(int id, bool kt_nhap)
        {
            System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            var user_id = UserManager.GetUserId(currentUser);
            var user = await UserManager.GetUserAsync(currentUser);

            var Xuat = _QLSXcontext.VattuDieuchuyenChiTietModel.Where(d => d.id == id).FirstOrDefault();
            Xuat.kt_nhap = kt_nhap;
            if (kt_nhap == true)
                Xuat.user_nhap = user.Email;
            _QLSXcontext.Update(Xuat);
            _QLSXcontext.SaveChanges();
            return Json(new { success = true });
        }
        [HttpPost]
        public async Task<JsonResult> excel()
        {
            System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            var user_id = UserManager.GetUserId(currentUser);
            var user = await UserManager.GetUserAsync(currentUser);
            var warehouse = user.warehouses_vt;

            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            var mahh = Request.Form["filters[mahh]"].FirstOrDefault();
            var tenhh = Request.Form["filters[tenhh]"].FirstOrDefault();
            var tenncc = Request.Form["filters[tenncc]"].FirstOrDefault();
            var mancc = Request.Form["filters[mancc]"].FirstOrDefault();
            var makho = Request.Form["filters[makho]"].FirstOrDefault();
            var malo = Request.Form["filters[malo]"].FirstOrDefault();
            var ngay = Request.Form["filters[ngay]"].FirstOrDefault();
            int skip = start != null ? Convert.ToInt32(start) : 0;
            DateTime start_date = ngay != null ? DateTime.Parse(ngay).AddDays(1).AddTicks(-1) : DateTime.Now.Date.AddDays(1).AddTicks(-1);
            //var hanghoa = _context.MaterialModel.Where(d => d.deleted_at == null).ToList();
            var nhap = _QLSXcontext.VattuNhapChiTietModel.Include(d => d.hoadon).Where(d => d.hoadon != null && d.hoadon.deleted_at == null && d.hoadon.ngaylap <= start_date && warehouse.Contains(d.hoadon.makho)).AsNoTracking().ToList();
            var dieuchuyen = _QLSXcontext.VattuDieuchuyenChiTietModel.Include(d => d.hoadon).Where(d => d.hoadon != null && d.hoadon.deleted_at == null && d.hoadon.ngaylap <= start_date && warehouse.Contains(d.hoadon.noidi)).AsNoTracking().ToList();
            var customerData = nhap.Select(d => new VattuInventory()
            {
                mahh = d.mahh,
                //tenhh = hanghoa.Where(e => e.mahh == d.mahh).FirstOrDefault()?.tenhh,
                //dvt = hanghoa.Where(e => e.mahh == d.mahh).FirstOrDefault()?.dvt,
                malo = d.malo,
                handung = d.handung,
                mancc = d.mancc,
                makho = d.hoadon.makho,
                soluong = d.soluong,
            }).ToList();
            customerData = customerData.Concat(dieuchuyen.Where(d => d.kt_xuat == true).Select(d => new VattuInventory() /// Xuất
            {
                mahh = d.mahh,
                //tenhh = d.package.tenhh,
                //dvt = d.package.dvt,
                malo = d.malo,
                handung = d.handung,
                mancc = d.mancc,
                makho = d.hoadon.noidi,
                soluong = -d.soluong,
            }).ToList()).ToList();
            var list_hh = _context.MaterialModel.Where(d => d.deleted_at == null).ToList();
            var list_ncc = _context.NhacungcapModel.ToList();
            var list_nsx = _context.NsxModel.ToList();
            var datalist = customerData.GroupBy(d => new { d.mahh, d.mancc, d.makho, d.malo, d.handung }).Select(d => new VattuInventory()
            {
                mahh = d.Key.mahh,
                makho = d.Key.makho,
                mancc = d.Key.mancc,
                malo = d.Key.malo,
                handung = d.Key.handung,
                //tenhh = list_hh.Where(e => e.mahh == d.Key.mahh).FirstOrDefault()?.tenhh,
                //dvt = list_hh.Where(e => e.mahh == d.Key.mahh).FirstOrDefault()?.dvt,
                //tenncc = list_ncc.Where(e => e.mancc == d.Key.mancc).FirstOrDefault()?.tenncc,
                //mansx = list_hh.Where(e => e.mahh == d.Key.mahh).FirstOrDefault()?.mansx,
                //tennsx = list_nsx.Where(f => f.mansx == list_hh.Where(e => e.mahh == d.Key.mahh).FirstOrDefault()?.mansx).FirstOrDefault()?.tennsx,
                soluong = d.Sum(e => e.soluong),
            }).Where(d => d.soluong != 0 && warehouse.Contains(d.makho)).ToList();
            foreach (var d in datalist)
            {
                var ncc = list_ncc.Where(e => e.mancc == d.mancc).FirstOrDefault();
                if (ncc != null)
                {
                    d.tenncc = ncc.tenncc;
                }

                var hh = list_hh.Where(e => e.mahh == d.mahh).FirstOrDefault();
                if (hh != null)
                {
                    d.mansx = hh.mansx;
                    d.tenhh = hh.tenhh;
                    d.dvt = hh.dvt;
                    var nsx = list_nsx.Where(f => f.mansx == hh.mansx).FirstOrDefault();
                    d.tennsx = nsx != null ? nsx.tennsx : null;
                }
            }

            int recordsTotal = datalist.Count();
            if (mahh != null && mahh != "")
            {
                datalist = datalist.Where(d => d.mahh.Contains(mahh)).ToList();
            }

            if (tenhh != null && tenhh != "")
            {
                datalist = datalist.Where(d => d.tenhh.Contains(tenhh)).ToList();
            }
            if (mancc != null && mancc != "")
            {
                datalist = datalist.Where(d => d.mancc.Contains(mancc)).ToList();
            }
            if (tenncc != null && tenncc != "")
            {
                datalist = datalist.Where(d => d.tenncc.Contains(tenncc)).ToList();
            }
            if (malo != null && malo != "")
            {
                datalist = datalist.Where(d => d.malo == malo).ToList();
            }
            if (makho != null && makho != "")
            {
                datalist = datalist.Where(d => d.makho == makho).ToList();
            }
            int recordsFiltered = datalist.Count();
            var datapost = datalist.OrderBy(d => d.mahh).ToList();
            var data = new ArrayList();

            var viewPath = _configuration["Source:Path_Private"] + "\\buy\\templates\\Vattu.xlsx";
            var documentPath = "/tmp/Rawdata_" + DateTime.Now.ToFileTimeUtc() + ".xlsx";
            string Domain = (HttpContext.Request.IsHttps ? "https://" : "http://") + HttpContext.Request.Host.Value;
            Workbook workbook = new Workbook();
            workbook.LoadFromFile(viewPath);
            Worksheet sheet = workbook.Worksheets[0];
            int stt = 0;
            var start_r = 2;

            DataTable dt = new DataTable();
            //dt.Columns.Add("stt", typeof(int));
            dt.Columns.Add("stt", typeof(int));
            dt.Columns.Add("mahh", typeof(string));
            dt.Columns.Add("tenhh", typeof(string));
            dt.Columns.Add("mancc", typeof(string));
            dt.Columns.Add("tenncc", typeof(string));
            dt.Columns.Add("malo", typeof(string));
            dt.Columns.Add("handung", typeof(string));
            dt.Columns.Add("soluong", typeof(decimal));
            dt.Columns.Add("makho", typeof(string));


            var stt_cell = 2;



            sheet.InsertRow(start_r, datapost.Count(), InsertOptionsType.FormatAsAfter);
            foreach (var record in datapost)
            {
                DataRow dr1 = dt.NewRow();
                dr1["stt"] = (++stt);
                dr1["mahh"] = record.mahh;
                dr1["tenhh"] = record.tenhh;
                dr1["mancc"] = record.mancc;
                dr1["tenncc"] = record.tenncc;
                dr1["malo"] = record.malo;
                dr1["handung"] = record.handung != null ? record.handung.Value.ToString("dd/MM/yyyy") : "";
                dr1["soluong"] = record.soluong;
                dr1["makho"] = record.makho;

                dt.Rows.Add(dr1);
                start_r++;

            }
            sheet.InsertDataTable(dt, false, 2, 1);

            workbook.SaveToFile("./wwwroot" + documentPath, ExcelVersion.Version2013);

            return Json(new { success = true, link = Domain + documentPath });
        }
        public async Task<JsonResult> getXuat(int id)
        {
            var data = _QLSXcontext.VattuDieuchuyenModel.Where(d => d.id == id).Include(d => d.chitiet).FirstOrDefault();
            return Json(data);
        }
        public async Task<JsonResult> getTonkho(string mahh, string makho, string mancc)
        {
            //System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            //var user_id = UserManager.GetUserId(currentUser);
            //var user = await UserManager.GetUserAsync(currentUser);
            //var warehouse = user.warehouses_vt;
            var start_date = DateTime.Now;

            var nhap = _QLSXcontext.VattuNhapChiTietModel.Include(d => d.hoadon).Where(d => d.hoadon != null && d.hoadon.deleted_at == null && d.hoadon.ngaylap <= start_date && d.mahh == mahh && d.mancc == mancc && d.hoadon.makho == makho).AsNoTracking().ToList();
            var dieuchuyen = _QLSXcontext.VattuDieuchuyenChiTietModel.Include(d => d.hoadon).Where(d => d.hoadon != null && d.hoadon.deleted_at == null && d.hoadon.ngaylap <= start_date && d.mahh == mahh && d.mancc == mancc).AsNoTracking().ToList();

            var customerData = nhap.Select(d => new VattuInventory()
            {
                mahh = d.mahh,
                //tenhh = hanghoa.Where(e => e.mahh == d.mahh).FirstOrDefault()?.tenhh,
                //dvt = hanghoa.Where(e => e.mahh == d.mahh).FirstOrDefault()?.dvt,
                malo = d.malo,
                handung = d.handung,
                mancc = d.mancc,
                makho = d.hoadon.makho,
                soluong = d.soluong,
            }).ToList();
            customerData = customerData.Concat(dieuchuyen.Where(d => d.kt_xuat == true).Select(d => new VattuInventory() /// Xuất
            {
                mahh = d.mahh,
                //tenhh = d.package.tenhh,
                //dvt = d.package.dvt,
                malo = d.malo,
                handung = d.handung,
                mancc = d.mancc,
                makho = d.hoadon.noidi,
                soluong = -d.soluong,
            }).ToList()).ToList();
            var list_hh = _context.MaterialModel.Where(d => d.deleted_at == null).ToList();
            var list_ncc = _context.NhacungcapModel.ToList();
            var list_nsx = _context.NsxModel.ToList();
            var datalist = customerData.GroupBy(d => new { d.mahh, d.mancc, d.makho, d.malo }).Select(d => new VattuInventory()
            {
                mahh = d.Key.mahh,
                makho = d.Key.makho,
                mancc = d.Key.mancc,
                malo = d.Key.malo,
                handung = d.FirstOrDefault().handung,
                //tenhh = list_hh.Where(e => e.mahh == d.Key.mahh).FirstOrDefault()?.tenhh,
                //dvt = list_hh.Where(e => e.mahh == d.Key.mahh).FirstOrDefault()?.dvt,
                //tenncc = list_ncc.Where(e => e.mancc == d.Key.mancc).FirstOrDefault()?.tenncc,
                //mansx = list_hh.Where(e => e.mahh == d.Key.mahh).FirstOrDefault()?.mansx,
                //tennsx = list_nsx.Where(f => f.mansx == list_hh.Where(e => e.mahh == d.Key.mahh).FirstOrDefault()?.mansx).FirstOrDefault()?.tennsx,
                soluong = d.Sum(e => e.soluong),
            }).Where(d => d.soluong != 0).ToList();
            foreach (var d in datalist)
            {
                var ncc = list_ncc.Where(e => e.mancc == d.mancc).FirstOrDefault();
                if (ncc != null)
                {
                    d.tenncc = ncc.tenncc;
                }
                var hh = list_hh.Where(e => e.mahh == d.mahh).FirstOrDefault();
                if (hh != null)
                {
                    d.mansx = hh.mansx;
                    d.tenhh = hh.tenhh;
                    d.dvt = hh.dvt;
                    var nsx = list_nsx.Where(f => f.mansx == hh.mansx).FirstOrDefault();
                    d.tennsx = nsx != null ? nsx.tennsx : null;
                }
            }
            var end = datalist.FirstOrDefault();
            return Json(end);
        }
        public void CopyValues<T>(T target, T source)
        {
            Type t = typeof(T);

            var properties = t.GetProperties().Where(prop => prop.CanRead && prop.CanWrite);

            foreach (var prop in properties)
            {
                var value = prop.GetValue(source, null);
                //if (value != null)
                if (ModelState.ContainsKey(prop.Name))
                {
                    prop.SetValue(target, value, null);
                }
            }
        }


        //[HttpPost]
        public async Task<JsonResult> importbandau()
        {
            return Json(new { });
            System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            var user_id = UserManager.GetUserId(currentUser);
            var user = await UserManager.GetUserAsync(currentUser);
            var list_excel = new List<dynamic>()
            {
                //new
                //{
                //    file = "TỒN KHO RD 31.05.2025.xlsx",
                //    makho = "13",

                //},
                new
                {
                    file = "TỒN KHO QC 30.06.2025.xlsx",
                    makho = "02",

                },
                //new
                //{
                //    file = "mã 0706.xlsx",
                //    makho = "12",

                //}
            };
            var list_hh = _context.MaterialModel.ToList();
            var error = new List<string>();
            foreach (var excel in list_excel)
            {
                //// Add tồn kho đầu kì
                string sohd = "NHAP_BANDAU_" + excel.makho;
                var DTA_NHAPXUAT_old = _QLSXcontext.VattuNhapModel.Where(d => d.sohd == sohd).FirstOrDefault();
                //if (DTA_NHAPXUAT_old != null)
                //{
                //    ///Xóa phiếu cũ
                //    var list = _QLSXcontext.VattuNhapChiTietModel.Where(d => d.sohd == sohd).ToList();
                //    _QLSXcontext.RemoveRange(list);
                //    _QLSXcontext.Remove(DTA_NHAPXUAT_old);
                //    _QLSXcontext.SaveChanges();

                //}
                if (DTA_NHAPXUAT_old == null)
                {
                    var VattuNhapModel = new VattuNhapModel();
                    VattuNhapModel.ngaylap = DateTime.Now.Date;
                    VattuNhapModel.sohd = sohd;
                    VattuNhapModel.makho = excel.makho;
                    VattuNhapModel.created_by = user.Email;
                    VattuNhapModel.created_at = DateTime.Now;
                    _QLSXcontext.Add(VattuNhapModel);
                    _QLSXcontext.SaveChanges();
                }


                Spire.Xls.Workbook book = new Spire.Xls.Workbook();
                book.LoadFromFile("./wwwroot/report/excel/" + excel.file, ExcelVersion.Version2013);

                Spire.Xls.Worksheet sheet = book.Worksheets[0];
                var lastrow = sheet.LastDataRow;
                var lastcol = sheet.LastDataColumn;
                // nếu vẫn chưa gặp end thì vẫn lấy data
                Console.WriteLine(lastrow);
                //var list_Result = new List<ResultModel>();
                var newlist = new List<string>();
                var list_add = new List<MaterialModel>();
                for (int rowIndex = 1; rowIndex < lastrow; rowIndex++)
                {
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
                    var mahh = nowRow.Cells[1] != null ? nowRow.Cells[1].DisplayedText : null;
                    if (mahh == null || mahh == "")
                        continue;
                    var hh = list_hh.Where(d => d.mahh == mahh).FirstOrDefault();
                    if (hh == null)
                    {
                        error.Add(mahh);
                        continue;///Lỗi
                    }

                    decimal? soluong = nowRow.Cells[4] != null ? (decimal)nowRow.Cells[4].NumberValue : null;


                    var VattuNhapChiTietModel = new VattuNhapChiTietModel();
                    VattuNhapChiTietModel.sohd = sohd;
                    VattuNhapChiTietModel.mahh = mahh;
                    //VattuNhapChiTietModel.mancc = item.mancc;
                    VattuNhapChiTietModel.soluong = soluong;

                    _QLSXcontext.Add(VattuNhapChiTietModel);
                }

                _QLSXcontext.SaveChanges();
            }


            return Json(new
            {
                error
            }, new System.Text.Json.JsonSerializerOptions()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
            });

        }

    }
    public class VattuInventory
    {
        public string mahh { get; set; }
        public string tenhh { get; set; }
        public string dvt { get; set; }
        public decimal? soluong { get; set; }
        public string makho { get; set; }
        public string mancc { get; set; }
        public string tenncc { get; set; }
        public string mansx { get; set; }
        public string tennsx { get; set; }
        public string malo { get; set; }
        public DateTime? handung { get; set; }
    }
}
