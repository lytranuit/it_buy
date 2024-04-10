

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
        public ApiController(ItContext context, IConfiguration configuration, UserManager<UserModel> UserMgr) : base(context)
        {
            _configuration = configuration;
            UserManager = UserMgr;
        }

        public async Task<JsonResult> materials()
        {
            var All = _context.MaterialModel.ToList();
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
            //var jsonData = new { data = ProcessModel };
            return Json(new { sodutru = sodutru, somuahang = somuahang, success = success, failed = 0 }, new System.Text.Json.JsonSerializerOptions()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
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
                if (chonmua != null)
                {
                    tonggiatri = chonmua.tonggiatri;
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
                    tonggiatri = tonggiatri
                };
                data.Add(data1);
            }
            var jsonData = new { draw = draw, recordsFiltered = recordsFiltered, recordsTotal = recordsTotal, data = data };
            return Json(jsonData);
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
            return Json(jsonData);
        }
        [HttpPost]
        public async Task<JsonResult> chiphi(DateTime? tungay, DateTime? denngay, string timetype = "Month")
        {
            var cusdata = _context.MuahangModel.Where(d => d.deleted_at == null && d.date_finish != null);
            if (tungay != null && tungay.HasValue)
            {
                if (tungay.Value.Kind == DateTimeKind.Utc)
                {
                    tungay = tungay.Value.ToLocalTime();
                }
                cusdata = cusdata.Where(d => d.date_finish >= tungay.Value);
            }
            if (denngay != null && denngay.HasValue)
            {
                if (denngay.Value.Kind == DateTimeKind.Utc)
                {
                    denngay = denngay.Value.ToLocalTime();
                }
                cusdata = cusdata.Where(d => d.date_finish <= denngay.Value);
            }
            var data = cusdata.Include(d => d.muahang_chonmua)
                .ThenInclude(d => d.chitiet)
                .ThenInclude(d => d.muahang_chitiet)
                .ThenInclude(d => d.dutru_chitiet).ThenInclude(d => d.dutru)
                .ToList();
            var list = new List<Chart1>();
            if (timetype == "Year")
            {
                list = data.GroupBy(d => new { d.date_finish.Value.Year }).Select(group => new Chart1
                {
                    sort = group.Key.Year.ToString(),
                    label = group.Key.Year.ToString(),
                    data = group.Sum(d => d.muahang_chonmua.tonggiatri),
                    data_nvl = group.Sum(d => d.muahang_chonmua.chitiet.Where(e => e.muahang_chitiet.dutru_chitiet.dutru.type_id == 1).Select(e => e.thanhtien).Sum()),
                    data_hoachat = group.Sum(d => d.muahang_chonmua.chitiet.Where(e => e.muahang_chitiet.dutru_chitiet.dutru.type_id == 3).Select(e => e.thanhtien).Sum()),
                    data_giantiep = group.Sum(d => d.muahang_chonmua.chitiet.Where(e => e.muahang_chitiet.dutru_chitiet.dutru.type_id == 2).Select(e => e.thanhtien).Sum()),
                    group = group.ToList()
                }).OrderBy(d => d.sort).ToList();

            }
            else if (timetype == "Month")
            {
                list = data.GroupBy(d => new { year = d.date_finish.Value.Year, month = d.date_finish.Value.Month }).Select(group => new Chart1
                {
                    sort = group.Key.year + "-" + group.Key.month.ToString("d2"),
                    label = group.Key.month.ToString("d2") + "/" + group.Key.year,
                    data = group.Sum(d => d.muahang_chonmua.tonggiatri),
                    data_nvl = group.Sum(d => d.muahang_chonmua.chitiet.Where(e => e.muahang_chitiet.dutru_chitiet.dutru.type_id == 1).Select(e => e.thanhtien).Sum()),
                    data_hoachat = group.Sum(d => d.muahang_chonmua.chitiet.Where(e => e.muahang_chitiet.dutru_chitiet.dutru.type_id == 3).Select(e => e.thanhtien).Sum()),
                    data_giantiep = group.Sum(d => d.muahang_chonmua.chitiet.Where(e => e.muahang_chitiet.dutru_chitiet.dutru.type_id == 2).Select(e => e.thanhtien).Sum()),
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
                list = data.GroupBy(d => new { date = d.date_finish.Value.Date }).Select(group => new Chart1
                {
                    sort = group.Key.date.ToString("yyyy-MM-dd"),
                    label = group.Key.date.ToString("yyyy-MM-dd"),
                    data = group.Sum(d => d.muahang_chonmua.tonggiatri),
                    data_nvl = group.Sum(d => d.muahang_chonmua.chitiet.Where(e => e.muahang_chitiet.dutru_chitiet.dutru.type_id == 1).Select(e => e.thanhtien).Sum()),
                    data_hoachat = group.Sum(d => d.muahang_chonmua.chitiet.Where(e => e.muahang_chitiet.dutru_chitiet.dutru.type_id == 3).Select(e => e.thanhtien).Sum()),
                    data_giantiep = group.Sum(d => d.muahang_chonmua.chitiet.Where(e => e.muahang_chitiet.dutru_chitiet.dutru.type_id == 2).Select(e => e.thanhtien).Sum()),
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
                list = list
            });
        }
        [HttpPost]
        public async Task<JsonResult> chiphibophan(DateTime? tungay, DateTime? denngay)
        {
            var cusdata = _context.MuahangModel.Where(d => d.deleted_at == null && d.date_finish != null);
            if (tungay != null && tungay.HasValue)
            {
                if (tungay.Value.Kind == DateTimeKind.Utc)
                {
                    tungay = tungay.Value.ToLocalTime();
                }
                cusdata = cusdata.Where(d => d.date_finish >= tungay.Value);
            }
            if (denngay != null && denngay.HasValue)
            {
                if (denngay.Value.Kind == DateTimeKind.Utc)
                {
                    denngay = denngay.Value.ToLocalTime();
                }
                cusdata = cusdata.Where(d => d.date_finish <= denngay.Value);
            }
            var data = cusdata.Include(d => d.muahang_chonmua)
                .ThenInclude(d => d.chitiet)
                .ThenInclude(d => d.muahang_chitiet)
                .ThenInclude(d => d.dutru_chitiet).ThenInclude(d => d.dutru).ThenInclude(d => d.bophan)
                .ToList();
            var listbophan = new List<Chartbophan>();
            foreach (var d in data)
            {
                foreach (var e in d.muahang_chonmua.chitiet)
                {
                    var dutru = e.muahang_chitiet.dutru_chitiet.dutru;
                    if (dutru.type_id == 2)
                    {
                        var bophan = dutru.bophan;
                        var thanhtien = e.thanhtien;
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
}
