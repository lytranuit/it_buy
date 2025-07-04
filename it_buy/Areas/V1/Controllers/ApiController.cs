﻿

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Spire.Xls;
using System.Collections;
using System.Data;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Vue.Data;
using Vue.Models;
using static it_template.Areas.V1.Controllers.UserController;

namespace it_template.Areas.V1.Controllers
{

    public class ApiController : BaseController
    {
        private readonly IConfiguration _configuration;
        private UserManager<UserModel> UserManager;
        private QLSXContext _QLSXContext;
        private KTContext _KTContext;
        public ApiController(ItContext context, QLSXContext QLSXContext, KTContext KTContext, IConfiguration configuration, UserManager<UserModel> UserMgr) : base(context)

        {
            _configuration = configuration;
            UserManager = UserMgr;
            _QLSXContext = QLSXContext;
            _KTContext = KTContext;
        }

        public async Task<JsonResult> DutruChitiet(int type_id)
        {
            var query = _context.DutruChitietModel
                .Include(d => d.dutru).Where(d => d.dutru.status_id == (int)Status.EsignSuccess && d.date_huy == null);
            if (type_id > 0)
            {
                query = query.Where(d => d.dutru.type_id == type_id);
            }
            var All = query.Select(d => new
            {
                id = d.id,
                mahh = d.mahh,
                tenhh = d.tenhh,
                is_danhgia = d.danhgianhacungcap_id > 0 ? true : false,
            }).ToList();
            //var jsonData = new { data = ProcessModel };
            return Json(All, new System.Text.Json.JsonSerializerOptions()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            });
        }

        public async Task<JsonResult> materials()
        {
            var All = _context.MaterialModel.Include(d => d.nhasanxuat).ToList();
            //var jsonData = new { data = ProcessModel };
            return Json(All, new System.Text.Json.JsonSerializerOptions()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            });
        }

        public async Task<JsonResult> group_materials()
        {
            var All = _context.MaterialGroupModel.Include(d => d.items).ToList();
            //var jsonData = new { data = ProcessModel };
            return Json(All, new System.Text.Json.JsonSerializerOptions()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                ReferenceHandler = ReferenceHandler.IgnoreCycles
            });
        }
        public async Task<JsonResult> Kho()
        {
            var All = _QLSXContext.KhoModel.OrderBy(d => d.makho).ToList();
            //var jsonData = new { data = ProcessModel };
            return Json(All, new System.Text.Json.JsonSerializerOptions()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            });
        }
        public async Task<JsonResult> nhom()
        {
            var data = _context.MaterialGroupModel.OrderBy(d => d.manhom).ToList();

            return Json(data);
        }
        public async Task<JsonResult> nhacc()
        {
            var All = _context.NhacungcapModel.ToList();
            //var jsonData = new { data = ProcessModel };
            return Json(All, new System.Text.Json.JsonSerializerOptions()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            });
        }
        public async Task<JsonResult> nhasx()
        {
            var All = _context.NsxModel.ToList();
            //var jsonData = new { data = ProcessModel };
            return Json(All, new System.Text.Json.JsonSerializerOptions()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            });
        }
        public async Task<JsonResult> PLHH()
        {
            var All = _KTContext.TBL_DANHMUCPLHANGHOA.ToList();
            //var jsonData = new { data = ProcessModel };
            return Json(All, new System.Text.Json.JsonSerializerOptions()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            });
        }
        public async Task<JsonResult> khuvuc()
        {
            var All = _context.KhuvucModel.ToList();
            //var jsonData = new { data = ProcessModel };
            return Json(All, new System.Text.Json.JsonSerializerOptions()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            });
        }
        public async Task<JsonResult> products()
        {
            var All = _context.ProductModel.ToList();
            //var jsonData = new { data = ProcessModel };
            return Json(All, new System.Text.Json.JsonSerializerOptions()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            });
        }
        public async Task<JsonResult> tonkhoNVL()
        {
            var All = _context.PackageModel.Where(d => d.soluong > 0 && d.deleted_at == null).GroupBy(d => new { d.mahh }).Select(d => new PackageInventory()
            {
                mahh = d.Key.mahh,
                tenhh = d.FirstOrDefault().tenhh,
                dvt = d.FirstOrDefault().dvt,
                soluong = d.Sum(e => e.soluong)
            }).ToList();
            //var jsonData = new { data = ProcessModel };
            return Json(All, new System.Text.Json.JsonSerializerOptions()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            });
        }
        public async Task<JsonResult> tonkhoVattu()
        {
            var nhap = _QLSXContext.VattuNhapChiTietModel.Include(d => d.hoadon).Where(d => d.hoadon != null && d.hoadon.deleted_at == null).AsNoTracking().ToList();
            var dieuchuyen = _QLSXContext.VattuDieuchuyenChiTietModel.Include(d => d.hoadon).Where(d => d.hoadon != null && d.hoadon.deleted_at == null).AsNoTracking().ToList();
            
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
                malo = d.malo,
                handung = d.handung,
                mancc = d.mancc,
                makho = d.hoadon.noidi,
                soluong = -d.soluong,
            }).ToList()).ToList();

            var list_hh = _context.MaterialModel.Where(d => d.deleted_at == null).ToList();
            var datalist = customerData.GroupBy(d => new { d.mahh }).Select(d => new VattuInventory()
            {
                mahh = d.Key.mahh,
                soluong = d.Sum(e => e.soluong),
            }).Where(d => d.soluong != 0).ToList();

            foreach (var d in datalist)
            {
                var hh = list_hh.Where(e => e.mahh == d.mahh).FirstOrDefault();
                if (hh != null)
                {
                    d.mansx = hh.mansx;
                    d.tenhh = hh.tenhh;
                    d.dvt = hh.dvt;
                }
            }

            return Json(datalist, new System.Text.Json.JsonSerializerOptions()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            });
        }

        public async Task<JsonResult> departmentsofuser()
        {
            System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            var user_id = UserManager.GetUserId(currentUser);

            var list_departments = _context.UserDepartmentModel.Where(d => d.user_id == user_id).Select(d => (int)d.department_id).ToList();
            var is_CungungNVL = list_departments.Contains(29) == true;
            var is_CungungGiantiep = list_departments.Contains(14) == true;
            var is_CungungHCTT = list_departments.Contains(30) == true;


            var departments = _context.DepartmentModel.Where(d => d.deleted_at == null && d.parent == 0).ToList();
            if (!is_CungungGiantiep && !is_CungungHCTT && !is_CungungHCTT)
            {
                departments = departments.Where(d => list_departments.Contains(d.id)).ToList();
            }
            return Json(departments, new System.Text.Json.JsonSerializerOptions()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
            });
        }
        public async Task<JsonResult> departments()
        {
            var All = GetChild(0);
            //var jsonData = new { data = ProcessModel };
            return Json(All, new System.Text.Json.JsonSerializerOptions()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            });
        }
        private List<SelectDepartmentResponse> GetChild(int parent)
        {
            var DepartmentModel = _context.DepartmentModel.Where(d => d.deleted_at == null && d.parent == parent).OrderBy(d => d.stt).ToList();
            var list = new List<SelectDepartmentResponse>();
            if (DepartmentModel.Count() > 0)
            {
                foreach (var department in DepartmentModel)
                {
                    //if (users.Count == 0)
                    //    continue;
                    var DepartmentResponse = new SelectDepartmentResponse
                    {

                        id = department.id.ToString(),
                        label = department.name,
                        name = department.name,
                        is_department = true,
                    };
                    //var count_child = _context.DepartmentModel.Where(d => d.deleted_at == null && d.parent == department.id).Count();
                    //if (count_child > 0)
                    //{
                    var child = GetChild(department.id);
                    var users = _context.UserDepartmentModel.Where(d => d.department_id == department.id).Include(d => d.user).ToList();
                    if (users.Count() == 0 && child.Count() == 0)
                        continue;
                    foreach (var item in users)
                    {
                        var user = item.user;
                        child.Add(new SelectDepartmentResponse
                        {

                            id = user.Id.ToString(),
                            label = user.FullName + "<" + user.Email + ">",
                            name = user.FullName,
                        });
                    }
                    if (child.Count() > 0)
                        DepartmentResponse.children = child;
                    //}
                    list.Add(DepartmentResponse);



                }
            }
            return list;
        }


        public async Task<JsonResult> HomeBadge()
        {
            var sodutru = _context.DutruModel.Where(d => d.deleted_at == null).Count();
            var somuahang = _context.MuahangModel.Where(d => d.deleted_at == null).Count();
            var success = _context.MuahangModel.Where(d => d.deleted_at == null && d.date_finish != null).Count();
            var failed = _context.MuahangChitietModel.Include(d => d.muahang).Where(d => d.muahang.deleted_at == null && d.status_nhanhang == 2).Count();
            //var jsonData = new { data = ProcessModel };
            return Json(new { sodutru = sodutru, somuahang = somuahang, success = success, failed = failed }, new System.Text.Json.JsonSerializerOptions()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
            });
        }

        [HttpPost]
        public async Task<JsonResult> TableMuahang()
        {
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            var name = Request.Form["filters[name]"].FirstOrDefault();
            var id_text = Request.Form["filters[id]"].FirstOrDefault();
            int id = id_text != null ? Convert.ToInt32(id_text) : 0;
            var type = Request.Form["type"].FirstOrDefault();
            //var tenhh = Request.Form["filters[tenhh]"].FirstOrDefault();
            int skip = start != null ? Convert.ToInt32(start) : 0;
            var customerData = _context.MuahangModel.Where(d => d.deleted_at == null);

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
            var datapost = customerData.OrderByDescending(d => d.id).Skip(skip).Take(pageSize)
                .Include(d => d.user_created_by)
                .Include(d => d.muahang_chonmua)
                .ToList();
            var data = new ArrayList();
            foreach (var record in datapost)
            {
                var chonmua = record.muahang_chonmua;
                decimal? tonggiatri = null;
                var tiente = "VND";
                if (chonmua != null)
                {
                    tonggiatri = chonmua.tonggiatri;
                    tiente = chonmua.tiente;
                }
                var data1 = new
                {
                    id = record.id,
                    code = record.code,
                    name = record.name,
                    status_id = record.status_id,
                    created_by = record.created_by,
                    user_created_by = record.user_created_by,
                    is_dathang = record.is_dathang,
                    loaithanhtoan = record.loaithanhtoan,
                    is_nhanhang = record.is_nhanhang,
                    is_thanhtoan = record.is_thanhtoan,
                    date_finish = record.date_finish,
                    tonggiatri = tonggiatri,
                    created_at = record.created_at,
                    tiente = tiente,
                };
                data.Add(data1);
            }
            var jsonData = new { draw = draw, recordsFiltered = recordsFiltered, recordsTotal = recordsTotal, data = data };
            return Json(jsonData, new System.Text.Json.JsonSerializerOptions()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
            });
        }
        [HttpPost]
        public async Task<JsonResult> TableDutru()
        {
            System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            var user_id = UserManager.GetUserId(currentUser);
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

            //customerData = customerData.Where(d => d.created_by == user_id);


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
            return Json(jsonData, new System.Text.Json.JsonSerializerOptions()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
            });
        }
        [HttpPost]
        public async Task<JsonResult> chiphi(DateTime? tungay, DateTime? denngay, string timetype = "Month")
        {
            var cusdata = _context.MuahangModel.Where(d => d.deleted_at == null && d.is_multiple_ncc != true && d.pay_at != null);
            if (tungay != null && tungay.HasValue)
            {
                if (tungay.Value.Kind == DateTimeKind.Utc)
                {
                    tungay = tungay.Value.ToLocalTime();
                }
                cusdata = cusdata.Where(d => d.pay_at >= tungay.Value);
            }
            if (denngay != null && denngay.HasValue)
            {
                if (denngay.Value.Kind == DateTimeKind.Utc)
                {
                    denngay = denngay.Value.ToLocalTime();
                }
                cusdata = cusdata.Where(d => d.pay_at <= denngay.Value);
            }
            var data = cusdata.Include(d => d.muahang_chonmua)
                .ThenInclude(d => d.chitiet)
                .ThenInclude(d => d.muahang_chitiet)
                .ThenInclude(d => d.dutru_chitiet).ThenInclude(d => d.dutru)
                .ToList();
            data = data.Where(d => d.muahang_chonmua != null).ToList();
            var list = new List<Chart1>();
            if (timetype == "Year")
            {
                list = data.GroupBy(d => new { d.pay_at.Value.Year }).Select(group => new Chart1
                {
                    sort = group.Key.Year.ToString(),
                    label = group.Key.Year.ToString(),
                    data = group.Sum(d => d.muahang_chonmua.tonggiatri),
                    data_nvl = group.Sum(d => d.muahang_chonmua.chitiet.Where(e => e.muahang_chitiet.dutru_chitiet.dutru.type_id == 1).Select(e => e.thanhtien_vat).Sum()),
                    data_hoachat = group.Sum(d => d.muahang_chonmua.chitiet.Where(e => e.muahang_chitiet.dutru_chitiet.dutru.type_id == 3).Select(e => e.thanhtien_vat).Sum()),
                    data_giantiep = group.Sum(d => d.muahang_chonmua.chitiet.Where(e => e.muahang_chitiet.dutru_chitiet.dutru.type_id == 2).Select(e => e.thanhtien_vat).Sum()),
                    group = group.ToList()
                }).OrderBy(d => d.sort).ToList();

            }
            else if (timetype == "Month")
            {
                list = data.GroupBy(d => new { year = d.pay_at.Value.Year, month = d.pay_at.Value.Month }).Select(group => new Chart1
                {
                    sort = group.Key.year + "-" + group.Key.month.ToString("d2"),
                    label = group.Key.month.ToString("d2") + "/" + group.Key.year,
                    data = group.Sum(d => d.muahang_chonmua.tonggiatri),
                    data_nvl = group.Sum(d => d.muahang_chonmua.chitiet.Where(e => e.muahang_chitiet.dutru_chitiet.dutru.type_id == 1).Select(e => e.thanhtien_vat).Sum()),
                    data_hoachat = group.Sum(d => d.muahang_chonmua.chitiet.Where(e => e.muahang_chitiet.dutru_chitiet.dutru.type_id == 3).Select(e => e.thanhtien_vat).Sum()),
                    data_giantiep = group.Sum(d => d.muahang_chonmua.chitiet.Where(e => e.muahang_chitiet.dutru_chitiet.dutru.type_id == 2).Select(e => e.thanhtien_vat).Sum()),
                    group = group.ToList()
                }).OrderBy(d => d.sort).ToList();
            }
            //else if (timetype == "Week")
            //{
            //    list = data.GroupBy(d => new { year = d.date_finish.Value.Year, month = d.date_finish.Value.Month }).Select(group => new Chart1
            //    {
            //        sort = group.Key.year + "-" + group.Key.month.ToString("d2"),
            //        label = group.Key.month.ToString("d2") + "/" + group.Key.year,
            //        data = group.Sum(d => d.muahang_chonmua.tonggiatri)
            //    }).OrderBy(d => d.sort).ToList();
            //}
            else
            {
                list = data.GroupBy(d => new { date = d.pay_at.Value.Date }).Select(group => new Chart1
                {
                    sort = group.Key.date.ToString("yyyy-MM-dd"),
                    label = group.Key.date.ToString("yyyy-MM-dd"),
                    data = group.Sum(d => d.muahang_chonmua.tonggiatri),
                    data_nvl = group.Sum(d => d.muahang_chonmua.chitiet.Where(e => e.muahang_chitiet.dutru_chitiet.dutru.type_id == 1).Select(e => e.thanhtien_vat).Sum()),
                    data_hoachat = group.Sum(d => d.muahang_chonmua.chitiet.Where(e => e.muahang_chitiet.dutru_chitiet.dutru.type_id == 3).Select(e => e.thanhtien_vat).Sum()),
                    data_giantiep = group.Sum(d => d.muahang_chonmua.chitiet.Where(e => e.muahang_chitiet.dutru_chitiet.dutru.type_id == 2).Select(e => e.thanhtien_vat).Sum()),
                    group = group.ToList()
                }).OrderBy(d => d.sort).ToList();
            }
            var dulieu = new
            {
                labels = list.Select(d => d.label).ToList(),
                datasets = new List<Chart>()
                {
                    new Chart { type="line",fill=false,borderWidth=2,label = "Tổng", data = list.Select(d => d.data).ToList() },

                    new Chart { type="bar",label = "Nguyên liệu", data = list.Select(d => d.data_nvl).ToList() },


                    new Chart { type="bar",label = "Hóa chất", data = list.Select(d => d.data_hoachat).ToList() },


                    new Chart { type="bar",label = "Khác", data = list.Select(d => d.data_giantiep).ToList() }
                }
            };

            return Json(new
            {
                data = dulieu,
                //list = list
            }, new System.Text.Json.JsonSerializerOptions()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
            });


        }
        [HttpPost]
        public async Task<JsonResult> chiphibophan(DateTime? tungay, DateTime? denngay)
        {
            var cusdata = _context.MuahangModel.Where(d => d.deleted_at == null && d.is_multiple_ncc != true && d.pay_at != null);
            if (tungay != null && tungay.HasValue)
            {
                if (tungay.Value.Kind == DateTimeKind.Utc)
                {
                    tungay = tungay.Value.ToLocalTime();
                }
                cusdata = cusdata.Where(d => d.pay_at >= tungay.Value);
            }
            if (denngay != null && denngay.HasValue)
            {
                if (denngay.Value.Kind == DateTimeKind.Utc)
                {
                    denngay = denngay.Value.ToLocalTime();
                }
                cusdata = cusdata.Where(d => d.pay_at <= denngay.Value);
            }
            var data = cusdata.Include(d => d.muahang_chonmua)
                .ThenInclude(d => d.chitiet)
                .ThenInclude(d => d.muahang_chitiet)
                .ThenInclude(d => d.dutru_chitiet).ThenInclude(d => d.dutru).ThenInclude(d => d.bophan)
                .ToList();
            data = data.Where(d => d.muahang_chonmua != null).ToList();
            var listbophan = new List<Chartbophan>();
            foreach (var d in data)
            {
                foreach (var e in d.muahang_chonmua.chitiet)
                {
                    var dutru = e.muahang_chitiet.dutru_chitiet.dutru;
                    if (dutru.type_id == 2)
                    {
                        var bophan = dutru.bophan;
                        var thanhtien = e.thanhtien_vat;
                        listbophan.Add(new Chartbophan()
                        {
                            bophan = bophan,
                            thanhtien = thanhtien
                        });
                    }
                }
            }
            var list = new List<Chart1>();

            list = listbophan.GroupBy(d => new { d.bophan }).Select(group => new Chart1
            {
                label = group.Key.bophan.name,
                data = group.Sum(d => d.thanhtien),
            }).OrderByDescending(d => d.data).ToList();

            var dulieu = new
            {
                labels = list.Select(d => d.label).ToList(),
                datasets = new List<Chart>()
                {
                    new Chart { label = "Chi phí gián tiếp", data = list.Select(d => d.data).ToList() },
                }
            };

            return Json(new
            {
                data = dulieu,
            }, new System.Text.Json.JsonSerializerOptions()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
            });
        }
        [HttpPost]
        public async Task<JsonResult> Comments()
        {
            System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            var user_id = UserManager.GetUserId(currentUser);
            // Du tru
            var list_dutru = _context.DutruModel.Where(d => d.created_by == user_id).Select(d => d.id).ToList(); /// dự trù tạo
            var list_comment = _context.DutruCommentModel.Where(d => d.user_id == user_id).Select(d => d.dutru_id).ToList(); /// dự trù có comment
            var list_comment_releated = _context.DutruCommentUserModel.Include(d => d.comment).Where(d => d.user_id == user_id).Select(d => d.comment.dutru_id).ToList(); /// dự trù có comment

            var list_related_dutru = new List<int>();
            list_related_dutru.AddRange(list_dutru);
            list_related_dutru.AddRange(list_comment);
            list_related_dutru.AddRange(list_comment_releated);
            list_related_dutru = list_related_dutru.Distinct().ToList();
            var new_comment = _context.DutruCommentModel.Where(d => d.deleted_at == null && list_related_dutru.Contains(d.dutru_id)).Include(d => d.files).OrderByDescending(d => d.created_at).ToList();
            foreach (var item in new_comment)
            {

            }
            ///
            return Json(new
            {
                success = true,
                new_comment = new_comment,
            }, new System.Text.Json.JsonSerializerOptions()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
            });
        }
    }
    public class Chart
    {
        public string type { get; set; }
        public string label { get; set; }
        public List<decimal?> data { get; set; }
        public bool fill { get; set; }
        public int borderWidth { get; set; }
        public string borderColor { get; set; }
        public string backgroundColor { get; set; }
    }
    public class Chart1
    {
        public string sort { get; set; }
        public string label { get; set; }
        public decimal? data { get; set; }
        public decimal? data_nvl { get; set; }
        public decimal? data_hoachat { get; set; }
        public decimal? data_giantiep { get; set; }
        public List<MuahangModel> group { get; set; }
    }
    public class Chartbophan
    {
        public string sort { get; set; }
        public DepartmentModel bophan { get; set; }
        public decimal? thanhtien { get; set; }
    }
    public class Comments
    {
        public string title { get; set; }
        public string comment { get; set; }
        public UserModel user { get; set; }
        public DateTime? created_at { get; set; }
        public string link { get; set; }
    }
}
