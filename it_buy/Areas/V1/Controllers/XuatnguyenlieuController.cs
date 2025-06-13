

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json.Serialization;
using Vue.Data;
using Vue.Models;
using Vue.Services;
using Microsoft.CodeAnalysis;
using System.Globalization;

namespace it_template.Areas.V1.Controllers
{

    public class XuatnguyenlieuController : BaseController
    {
        private readonly IConfiguration _configuration;
        private UserManager<UserModel> UserManager;
        private readonly ViewRender _view;
        private QLSXContext _QLSXContext;
        public XuatnguyenlieuController(ItContext context, QLSXContext qlsxcontext, IConfiguration configuration, UserManager<UserModel> UserMgr, ViewRender view) : base(context)
        {
            _configuration = configuration;
            UserManager = UserMgr;
            _view = view;
            _QLSXContext = qlsxcontext;
        }
        public JsonResult Get(int id)
        {
            var data = _QLSXContext.XuatNVLModel.Where(d => d.id == id)
                .Include(d => d.chitiet).FirstOrDefault();

            return Json(data, new System.Text.Json.JsonSerializerOptions()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
            });
        }

        [HttpPost]
        public async Task<JsonResult> Delete(int id)
        {
            var Model = _QLSXContext.XuatNVLModel.Where(d => d.id == id).FirstOrDefault();
            Model.deleted_at = DateTime.Now;
            _QLSXContext.Update(Model);
            _QLSXContext.SaveChanges();
            return Json(Model, new System.Text.Json.JsonSerializerOptions()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
            });
        }

        [HttpPost]
        public async Task<JsonResult> Save(XuatNVLModel XuatNVLModel, List<XuatNVLChitietModel> list_add, List<XuatNVLChitietModel>? list_update, List<XuatNVLChitietModel>? list_delete)
        {
            try
            {
                System.Security.Claims.ClaimsPrincipal currentUser = this.User;
                var user_id = UserManager.GetUserId(currentUser);
                var user = await UserManager.GetUserAsync(currentUser);
                XuatNVLModel? XuatNVLModel_old;
                if (XuatNVLModel.date != null && XuatNVLModel.date.Value.Kind == DateTimeKind.Utc)
                {
                    XuatNVLModel.date = XuatNVLModel.date.Value.ToLocalTime();
                }
                if (XuatNVLModel.id == 0)
                {
                    int count = _QLSXContext.XuatNVLModel.Where(d => d.created_at.Value.Date == DateTime.Now.Date).Count();
                    count = count + 1;
                    var code = "XUAT_NVL_" + DateTime.Now.Date.ToString("ddMMyy") + "_" + count.ToString("D3");
                    var check = _QLSXContext.XuatNVLModel.Where(d => d.code == code).FirstOrDefault();
                    if (check != null)
                    {
                        return Json(new { success = false, message = "Số HD đã tồn tại!" });
                    }
                    XuatNVLModel.code = code;
                    XuatNVLModel.created_at = DateTime.Now;
                    XuatNVLModel.status_id = (int)Status.Draft;
                    XuatNVLModel.created_by = user.Email;

                    _QLSXContext.XuatNVLModel.Add(XuatNVLModel);

                    _QLSXContext.SaveChanges();

                    XuatNVLModel_old = XuatNVLModel;

                }
                else
                {

                    XuatNVLModel_old = _QLSXContext.XuatNVLModel.Where(d => d.id == XuatNVLModel.id).FirstOrDefault();

                    CopyValues<XuatNVLModel>(XuatNVLModel_old, XuatNVLModel);
                    XuatNVLModel_old.updated_at = DateTime.Now;

                    _QLSXContext.Update(XuatNVLModel_old);
                    _QLSXContext.SaveChanges();
                }



                var list = new List<XuatNVLChitietModel>();
                if (list_delete != null)
                    _QLSXContext.RemoveRange(list_delete);
                if (list_add != null)
                {

                    foreach (var item in list_add)
                    {
                        item.xuat_id = XuatNVLModel_old.id;
                        _QLSXContext.Add(item);
                        //_QLSXContext.SaveChanges();
                        list.Add(item);
                    }
                }
                if (list_update != null)
                {
                    foreach (var item in list_update)
                    {
                        _QLSXContext.Update(item);
                        list.Add(item);
                    }
                }

                _QLSXContext.SaveChanges();

                return Json(new { success = true, id = XuatNVLModel_old.id });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.InnerException.Message });
            }

        }

        [HttpPost]
        public async Task<JsonResult> xuatpdf(int id, bool is_view = false)
        {
            CultureInfo vietnamCulture = new CultureInfo("vi-VN");
            var RawDetails = new List<RawDetails>();
            var data = _QLSXContext.XuatNVLModel.Where(d => d.id == id).Include(d => d.chitiet).FirstOrDefault();
            var bophan = new KhoModel();
            if (data != null)
            {
                var stt = 1;
                foreach (var item in data.chitiet)
                {
                    RawDetails.Add(new RawDetails
                    {
                        stt = stt++,
                        tennvl = item.tenhh,
                        manvl = item.mahh,
                        dvt = item.dvt,
                        note = item.note,
                        soluong = item.soluong.ToString("#,##0.##", vietnamCulture),
                    });

                }
                bophan = _QLSXContext.KhoModel.Where(d => d.makho == data.bophan_id).FirstOrDefault(); ;
            }
            else
            {
                return Json(new { success = false, message = "Dự trù không tồn tại!" });
            }


            System.Data.DataTable datatable_details = new System.Data.DataTable("details");
            PropertyInfo[] Props = typeof(RawDetails).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                //Setting column names as Property names
                datatable_details.Columns.Add(prop.Name);
            }
            foreach (RawDetails item in RawDetails)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    //inserting property values to datatable rows
                    values[i] = Props[i].GetValue(item, null);
                }
                datatable_details.Rows.Add(values);
            }
            ///XUẤT PDF

            //Creates Document instance
            Spire.Doc.Document document = new Spire.Doc.Document();

            document.LoadFromFile(_configuration["Source:Path_Private"] + "/buy/templates/Phieu de nghi xuat NVL.docx", Spire.Doc.FileFormat.Docx);



            var now = DateTime.Now;
            var raw = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"ngay",now.ToString("dd") },
                {"thang",now.ToString("MM") },
                {"nam",now.ToString("yyyy") },
                {"bophan",bophan != null ? bophan.tenkho : "" },
                {"code",data.code },
                {"note",data.note },
                {"date",data.date != null ? data.date.Value.ToString("dd/MM/yyyy") : "" },
            };


            string[] fieldName = raw.Keys.ToArray();
            string[] fieldValue = raw.Values.ToArray();

            string[] MergeFieldNames = document.MailMerge.GetMergeFieldNames();
            string[] GroupNames = document.MailMerge.GetMergeGroupNames();

            document.MailMerge.Execute(fieldName, fieldValue);
            document.MailMerge.ExecuteWidthRegion(datatable_details);


            var timeStamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
            if (Directory.Exists(_configuration["Source:Path_Private"] + "\\buy\\xuatnguyenlieu\\" + id))
            {
                Directory.CreateDirectory(_configuration["Source:Path_Private"] + "\\buy\\xuatnguyenlieu\\" + id);
            }
            string url = "/private/buy/xuatnguyenlieu/" + id + "/" + timeStamp + ".docx";
            string url_pdf = "/private/buy/xuatnguyenlieu/" + id + "/" + timeStamp + ".pdf";
            document.SaveToFile(_configuration["Source:Path_Private"] + url.Replace("/private", "").Replace("/", "\\"), Spire.Doc.FileFormat.Docx);

            var url_return = url;
            var convert_to_pdf = true;
            if (convert_to_pdf == true)
            {
                var output = ConvertWordFile(_configuration["Source:Path_Private"] + url.Replace("/private", "").Replace("/", "\\"), _configuration["Source:Path_Private"] + "\\buy\\xuatnguyenlieu\\" + id);
                //if (process != "Success")
                if (output == false)
                {
                    return Json(new { success = false });
                }
                url_return = url_pdf;

            }
            if (is_view)
            {
                return Json(new { success = true, link = url_return });
            }
            else
            {
                ////Trình ký
                data.pdf = url_return;
                data.status_id = (int)Status.PDF;
                //_QLSXContext.SaveChanges();


                ///UPLOAD ESIGN
                System.Security.Claims.ClaimsPrincipal currentUser = this.User;
                var user_id = UserManager.GetUserId(currentUser);
                var user = await UserManager.GetUserAsync(currentUser);

                ///Document
                var type_id = 77;

                var DocumentModel = new DocumentModel()
                {
                    name_vi = "Đề nghị xuất nguyên liệu " + data.code,
                    description_vi = data.note,
                    priority = 2,
                    status_id = (int)DocumentStatus.Draft,
                    type_id = type_id,
                    user_id = user_id,
                    user_next_signature_id = user_id,
                    is_sign_parellel = false,
                    created_at = DateTime.Now,
                };
                var date_now = DateTime.Now;
                var count_type_in_day = _context.DocumentModel.Where(d => d.type_id == DocumentModel.type_id && d.created_at.Value.DayOfYear == date_now.DayOfYear).Count();
                var type = _context.DocumentTypeModel.Where(d => d.id == DocumentModel.type_id).Include(d => d.users_receive).FirstOrDefault();
                DocumentModel.code = type.symbol + date_now.ToString("ddMMyy") + (count_type_in_day < 9 ? "0" : "") + (count_type_in_day + 1);
                _context.Add(DocumentModel);
                _context.SaveChanges();
                ///DocumentFile
                DocumentFileModel DocumentFileModel = new DocumentFileModel
                {
                    document_id = DocumentModel.id,
                    ext = ".pdf",
                    url = url_return,
                    name = data.code,
                    mimeType = "application/pdf",
                    created_at = DateTime.Now
                };
                _context.Add(DocumentFileModel);


                ////Signature
                for (int k = 0; k < 1; ++k)
                {
                    DocumentSignatureModel DocumentSignatureModel = new DocumentSignatureModel() { document_id = DocumentModel.id, user_id = user_id, stt = k };
                    _context.Add(DocumentSignatureModel);
                }
                ////Receive
                if (type.users_receive.Count() > 0)
                {
                    foreach (var receive in type.users_receive)
                    {
                        DocumentUserReceiveModel DocumentUserReceiveModel = new DocumentUserReceiveModel() { document_id = DocumentModel.id, user_id = receive.user_id };
                        _context.Add(DocumentUserReceiveModel);
                    }
                }
                /////create event
                DocumentEventModel DocumentEventModel = new DocumentEventModel
                {
                    document_id = DocumentModel.id,
                    event_content = "<b>" + user.FullName + "</b> tạo hồ sơ mới",
                    created_at = DateTime.Now,
                };
                _context.Add(DocumentEventModel);
                /////create Related 
                RelatedEsignModel RelatedEsignModel = new RelatedEsignModel()
                {
                    esign_id = DocumentModel.id,
                    related_id = data.id,
                    type = "xuatnguyenlieu",
                    created_at = DateTime.Now
                };
                _context.Add(RelatedEsignModel);

                //_QLSXContext.SaveChanges();
                data.status_id = (int)Status.Esign;
                data.esign_id = DocumentModel.id;
                data.code = DocumentModel.code;

                _QLSXContext.Update(data);
                _QLSXContext.SaveChanges();

                return Json(new { success = true });
            }
        }

        [HttpPost]
        public async Task<JsonResult> Table()
        {
            System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            var user_id = UserManager.GetUserId(currentUser);
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            var name = Request.Form["filters[name]"].FirstOrDefault();
            var code = Request.Form["filters[code]"].FirstOrDefault();
            var priority_id_string = Request.Form["filters[priority_id]"].FirstOrDefault();
            var status_id_string = Request.Form["filters[status_id]"].FirstOrDefault();
            var id_text = Request.Form["filters[id]"].FirstOrDefault();
            int id = id_text != null ? Convert.ToInt32(id_text) : 0;
            int priority_id = priority_id_string != null ? Convert.ToInt32(priority_id_string) : 0;
            int status_id = status_id_string != null ? Convert.ToInt32(status_id_string) : 0;
            var bophan_id = Request.Form["filters[bophan_id]"].FirstOrDefault();
            var type_id_string = Request.Form["type_id"].FirstOrDefault();
            var type1_string = Request.Form["type1"].FirstOrDefault();
            int type_id = type_id_string != null ? Convert.ToInt32(type_id_string) : 0;
            int type1 = type1_string != null ? Convert.ToInt32(type1_string) : 0;
            //var tenhh = Request.Form["filters[tenhh]"].FirstOrDefault();
            int skip = start != null ? Convert.ToInt32(start) : 0;
            var customerData = _QLSXContext.XuatNVLModel.Where(d => d.deleted_at == null);


            int recordsTotal = customerData.Count();

            if (bophan_id != null && code != "")
            {
                customerData = customerData.Where(d => d.bophan_id == bophan_id);
            }
            if (code != null && code != "")
            {
                customerData = customerData.Where(d => d.code.Contains(code));
            }
            if (id != 0)
            {
                customerData = customerData.Where(d => d.id == id);
            }
            if (status_id != 0)
            {
                customerData = customerData.Where(d => d.status_id == status_id);
            }
            int recordsFiltered = customerData.Count();
            var datapost = customerData.OrderByDescending(d => d.id).Skip(skip).Take(pageSize).Include(d => d.chitiet).ToList();
            var jsonData = new { draw = draw, recordsFiltered = recordsFiltered, recordsTotal = recordsTotal, data = datapost };
            return Json(jsonData, new System.Text.Json.JsonSerializerOptions()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
            });
        }
        //[HttpPost]
        //public async Task<JsonResult> TableChitiet()
        //{
        //    System.Security.Claims.ClaimsPrincipal currentUser = this.User;
        //    var user_id = UserManager.GetUserId(currentUser);
        //    var draw = Request.Form["draw"].FirstOrDefault();
        //    var start = Request.Form["start"].FirstOrDefault();
        //    var length = Request.Form["length"].FirstOrDefault();
        //    int pageSize = length != null ? Convert.ToInt32(length) : 0;
        //    var type_id_string = Request.Form["type_id"].FirstOrDefault();
        //    var filter_DNMH_string = Request.Form["filter_DNMH"].FirstOrDefault();
        //    int filter_DNMH = filter_DNMH_string != null ? Convert.ToInt32(filter_DNMH_string) : 0;
        //    int type_id = type_id_string != null ? Convert.ToInt32(type_id_string) : 0;
        //    var mahh = Request.Form["filters[mahh]"].FirstOrDefault();
        //    var tenhh = Request.Form["filters[tenhh]"].FirstOrDefault();
        //    var filter_user_id = Request.Form["filters[user_id]"].FirstOrDefault();
        //    var filter_tags = Request.Form["filters[tags]"].FirstOrDefault();
        //    var list_dutru = Request.Form["filters[list_dutru]"].FirstOrDefault();
        //    var id_string = Request.Form["filters[id]"].FirstOrDefault();
        //    var priority_id_string = Request.Form["filters[priority_id]"].FirstOrDefault();
        //    int priority_id = priority_id_string != null ? Convert.ToInt32(priority_id_string) : 0;
        //    var tensp = Request.Form["filters[tensp]"].FirstOrDefault();

        //    var filterTable = Request.Form["filterTable"].FirstOrDefault();
        //    var dutru_id_string = Request.Form["dutru_id"].FirstOrDefault();
        //    var department_id_string = Request.Form["filters[bophan]"].FirstOrDefault();
        //    int id = id_string != null ? Convert.ToInt32(id_string) : 0;
        //    int dutru_id = dutru_id_string != null ? Convert.ToInt32(dutru_id_string) : 0;
        //    int department_id = department_id_string != null ? Convert.ToInt32(department_id_string) : 0;

        //    var departments = _QLSXContext.UserDepartmentModel.Where(d => d.user_id == user_id).Select(d => (int)d.department_id).ToList();
        //    var is_CungungNVL = departments.Contains(29) == true;
        //    var is_CungungGiantiep = departments.Contains(14) == true;
        //    var is_CungungHCTT = departments.Contains(30) == true;

        //    var sort_id = Request.Form["sorts[id]"].FirstOrDefault();
        //    var sort_mahh = Request.Form["sorts[mahh]"].FirstOrDefault();
        //    var sort_tenhh = Request.Form["sorts[tenhh]"].FirstOrDefault();
        //    var sort_tensp = Request.Form["sorts[tensp]"].FirstOrDefault();
        //    var sort_bophan = Request.Form["sorts[bophan]"].FirstOrDefault();
        //    var sort_priority = Request.Form["sorts[priority_id]"].FirstOrDefault();
        //    var sort_dutru = Request.Form["sorts[list_dutru]"].FirstOrDefault();
        //    var sort_ngayhethan = Request.Form["sorts[ngayhethan]"].FirstOrDefault();
        //    //var 

        //    //var tenhh = Request.Form["filters[tenhh]"].FirstOrDefault();
        //    int skip = start != null ? Convert.ToInt32(start) : 0;
        //    var customerData = _QLSXContext.XuatNVLChitietModel
        //        .Include(d => d.muahang_chitiet).ThenInclude(d => d.muahang).ThenInclude(d => d.muahang_chonmua).ThenInclude(d => d.ncc)
        //        .Include(d => d.dutru).Where(d => d.dutru.status_id == (int)Status.EsignSuccess && d.date_huy == null);

        //    Expression<Func<XuatNVLChitietModel, bool>> filter = p => p.dutru.created_by == user_id; // Biểu thức ban đầu là true (chấp nhận tất cả)

        //    filter = filter.Or(d => departments.Contains((int)d.dutru.bophan_id));
        //    if (is_CungungNVL)
        //    {
        //        filter = filter.Or(d => d.dutru.type_id == 1);
        //    }
        //    if (is_CungungGiantiep)
        //    {
        //        filter = filter.Or(d => d.dutru.type_id == 2);
        //    }
        //    if (is_CungungHCTT)
        //    {
        //        filter = filter.Or(d => d.dutru.type_id == 3);
        //    }
        //    customerData = customerData.Where(filter);

        //    var list_muahang_id = _QLSXContext.VattuNhapChiTietModel.Select(d => d.muahang_id).ToList();

        //    if (filterTable != null && filterTable == "1")
        //    {
        //        customerData = customerData.Where(d => d.muahang_chitiet.Count() == 0);
        //    }
        //    else if (filterTable != null && filterTable == "2")
        //    {
        //        customerData = customerData.Where(d => d.muahang_chitiet.Count() > 0);
        //    }
        //    else if (filterTable != null && filterTable == "3")
        //    {
        //        customerData = customerData.Where(d => d.mahh != null && d.muahang_chitiet.Any(m => !list_muahang_id.Contains(m.muahang_id)));
        //    }
        //    else if (filterTable != null && filterTable == "4")
        //    {
        //        customerData = customerData.Where(d => d.mahh != null && d.muahang_chitiet.Any(m => list_muahang_id.Contains(m.muahang_id)));
        //    }








        //    if (dutru_id != null && dutru_id != 0)
        //    {
        //        customerData = customerData.Where(d => d.dutru_id == dutru_id);
        //    }

        //    int recordsTotal = customerData.Count();
        //    if (filter_DNMH == 1)
        //    {
        //        customerData = customerData.Where(d => d.muahang_chitiet.Any(m => m.muahang != null && (m.muahang.status_id == 1 || m.muahang.status_id == 6 || m.muahang.status_id == 7)));
        //    }
        //    else if (filter_DNMH == 2)
        //    {
        //        customerData = customerData.Where(d => d.muahang_chitiet.Any(m => m.muahang != null && m.muahang.status_id == 9));
        //    }
        //    else if (filter_DNMH == 3)
        //    {
        //        customerData = customerData.Where(d => d.muahang_chitiet.Any(m => m.muahang != null && m.muahang.status_id == 11));
        //    }
        //    else if (filter_DNMH == 4)
        //    {
        //        customerData = customerData.Where(d => d.muahang_chitiet.Any(m => m.muahang != null && m.muahang.status_id == 10 && m.muahang.is_dathang != true));
        //    }
        //    else if (filter_DNMH == 5)
        //    {
        //        customerData = customerData.Where(d => d.muahang_chitiet.Any(m => m.muahang != null && m.muahang.date_finish == null && (m.muahang.is_dathang == true && ((m.muahang.loaithanhtoan == "tra_sau" && m.muahang.is_nhanhang == false) || (m.muahang.loaithanhtoan == "tra_truoc" && m.muahang.is_thanhtoan == true)))));
        //    }
        //    else if (filter_DNMH == 6)
        //    {
        //        customerData = customerData.Where(d => d.muahang_chitiet.Any(m => m.muahang != null && m.muahang.date_finish == null && (m.muahang.is_dathang == true && ((m.muahang.loaithanhtoan == "tra_truoc" && m.muahang.is_thanhtoan == false) || (m.muahang.loaithanhtoan == "tra_sau" && m.muahang.is_nhanhang == true)))));
        //    }
        //    else if (filter_DNMH == 7)
        //    {
        //        customerData = customerData.Where(d => d.muahang_chitiet.Any(m => m.muahang != null && m.muahang.date_finish != null));
        //    }

        //    if (id != null && id != 0)
        //    {
        //        customerData = customerData.Where(d => d.id == id);
        //    }
        //    if (type_id != null && type_id != 0)
        //    {
        //        customerData = customerData.Where(d => d.dutru.type_id == type_id);
        //    }
        //    if (priority_id != null && priority_id != 0)
        //    {
        //        customerData = customerData.Where(d => d.dutru.priority_id == priority_id);
        //    }
        //    if (department_id != null && department_id != 0)
        //    {
        //        customerData = customerData.Where(d => d.dutru.bophan_id == department_id);
        //    }
        //    if (mahh != null && mahh != "")
        //    {
        //        customerData = customerData.Where(d => d.mahh.Contains(mahh));
        //    }
        //    if (tenhh != null && tenhh != "")
        //    {
        //        customerData = customerData.Where(d => d.tenhh.Contains(tenhh));
        //    }
        //    if (filter_user_id != null && filter_user_id != "")
        //    {
        //        customerData = customerData.Where(d => d.user_id == filter_user_id);
        //    }
        //    if (filter_tags != null && filter_tags != "")
        //    {
        //        customerData = customerData.Where(d => d.tags.Contains(filter_tags));
        //    }
        //    if (tensp != null && tensp != "")
        //    {
        //        customerData = customerData.Where(d => d.tensp.Contains(tensp));
        //    }
        //    if (list_dutru != null && list_dutru != "")
        //    {
        //        var list_dt = list_dutru.Split(",").ToList();
        //        var listdutru = _QLSXContext.XuatNVLModel.Where(d => list_dt.Contains(d.code)).Select(d => d.id).ToList();
        //        customerData = customerData.Where(d => listdutru.Contains(d.dutru_id));
        //    }
        //    int recordsFiltered = customerData.Count();

        //    if (sort_id != null)
        //    {
        //        if (sort_id == "1")
        //        {
        //            customerData = customerData.OrderBy(d => d.id);
        //        }
        //        else if (sort_id == "-1")
        //        {
        //            customerData = customerData.OrderByDescending(d => d.id);
        //        }
        //    }
        //    else if (sort_mahh != null)
        //    {
        //        if (sort_mahh == "1")
        //        {
        //            customerData = customerData.OrderBy(d => d.mahh);
        //        }
        //        else if (sort_mahh == "-1")
        //        {
        //            customerData = customerData.OrderByDescending(d => d.mahh);
        //        }
        //    }
        //    else if (sort_tenhh != null)
        //    {
        //        if (sort_tenhh == "1")
        //        {
        //            customerData = customerData.OrderBy(d => d.tenhh);
        //        }
        //        else if (sort_tenhh == "-1")
        //        {
        //            customerData = customerData.OrderByDescending(d => d.tenhh);
        //        }
        //    }
        //    else if (sort_tensp != null)
        //    {
        //        if (sort_tensp == "1")
        //        {
        //            customerData = customerData.OrderBy(d => d.tensp);
        //        }
        //        else if (sort_tensp == "-1")
        //        {
        //            customerData = customerData.OrderByDescending(d => d.tensp);
        //        }
        //    }
        //    else if (sort_bophan != null)
        //    {
        //        if (sort_bophan == "1")
        //        {
        //            customerData = customerData.OrderBy(d => d.dutru.bophan_id);
        //        }
        //        else if (sort_bophan == "-1")
        //        {
        //            customerData = customerData.OrderByDescending(d => d.dutru.bophan_id);
        //        }
        //    }
        //    else if (sort_priority != null)
        //    {
        //        if (sort_priority == "1")
        //        {
        //            customerData = customerData.OrderBy(d => d.dutru.priority_id);
        //        }
        //        else if (sort_priority == "-1")
        //        {
        //            customerData = customerData.OrderByDescending(d => d.dutru.priority_id);
        //        }
        //    }
        //    else if (sort_dutru != null)
        //    {
        //        if (sort_dutru == "1")
        //        {
        //            customerData = customerData.OrderBy(d => d.dutru.code);
        //        }
        //        else if (sort_dutru == "-1")
        //        {
        //            customerData = customerData.OrderByDescending(d => d.dutru.code);
        //        }
        //    }
        //    else if (sort_ngayhethan != null)
        //    {
        //        if (sort_ngayhethan == "1")
        //        {
        //            customerData = customerData.OrderBy(d => d.date).ThenBy(d => d.dutru.date);
        //        }
        //        else if (sort_ngayhethan == "-1")
        //        {
        //            customerData = customerData.OrderByDescending(d => d.date).ThenByDescending(d => d.dutru.date);
        //        }
        //    }
        //    else
        //    {
        //        customerData = customerData.OrderByDescending(d => d.id);
        //    }


        //    var datapost = customerData.Skip(skip).Take(pageSize)
        //        .Include(d => d.user)
        //        .ToList();
        //    var data = new ArrayList();
        //    foreach (var record in datapost)
        //    {
        //        var dutru = record.dutru;
        //        var tenhh1 = record.tenhh;
        //        var mahh1 = record.mahh;
        //        //if (dutru.type_id == 1)
        //        //{
        //        var nhasx = record.nhasx;
        //        var nhacc = "";
        //        var material = _QLSXContext.MaterialModel.Where(d => record.mahh == d.mahh).Include(d => d.nhacungcap).FirstOrDefault();
        //        if (material != null)
        //        {
        //            nhacc = material.nhacungcap != null ? material.nhacungcap.tenncc : "";
        //        }
        //        //}
        //        var list_muahang_ncc_id = record.muahang_chitiet.Select(d => d.muahang.muahang_chonmua_id).ToList();
        //        var list_muahang_chitiet_id = record.muahang_chitiet.Select(d => d.id).ToList();

        //        var MuahangNccChitietModel = _QLSXContext.MuahangNccChitietModel.Include(d => d.muahang_ncc).ThenInclude(d => d.muahang)
        //            .Where(d => d.muahang_ncc.muahang.deleted_at == null && d.muahang_ncc.muahang.status_id != (int)Status.MuahangEsignError && list_muahang_ncc_id.Contains(d.muahang_ncc_id) && list_muahang_chitiet_id.Contains(d.muahang_chitiet_id))
        //            .ToList();
        //        var thanhtien = MuahangNccChitietModel.Sum(d => d.thanhtien_vat);
        //        var muahang_chitiet = record.muahang_chitiet.Where(d => d.muahang.deleted_at == null && d.muahang.status_id != (int)Status.MuahangEsignError).ToList();
        //        var soluong_dutru = record.soluong;
        //        var soluong_mua = muahang_chitiet.Where(d => d.muahang.parent_id == null).Sum(d => d.soluong * d.quidoi);

        //        var soluong = soluong_mua < soluong_dutru ? soluong_dutru - soluong_mua : 0;
        //        var list_muahang = new List<dynamic>();
        //        foreach (var m in muahang_chitiet)
        //        {

        //            var muahang = m.muahang;
        //            if (muahang.is_multiple_ncc == true) /// Xét trường hợp mua nhiều nhà cc
        //            {
        //                if (muahang.pay_at != null) /// Đã ký xong
        //                    continue;
        //                //if (muahang.parent_id > 0)
        //                //{
        //                //    continue;
        //                //}
        //            }

        //            var soluong_mua_chitiet = m.soluong * m.quidoi;
        //            bool is_nhap = _QLSXContext.VattuNhapChiTietModel.Where(d => d.muahang_id == muahang.id).Count() > 0;
        //            list_muahang.Add(new
        //            {
        //                id = muahang.id,
        //                name = muahang.name,
        //                code = muahang.code,
        //                date_finish = muahang.date_finish,
        //                is_dathang = muahang.is_dathang,
        //                is_thanhtoan = muahang.is_thanhtoan,
        //                is_nhanhang = muahang.is_nhanhang,
        //                loaithanhtoan = muahang.loaithanhtoan,
        //                status_id = muahang.status_id,
        //                mancc = muahang.muahang_chonmua?.ncc?.mancc,
        //                soluong = soluong_mua_chitiet,
        //                pay_at = muahang.pay_at, //// Ngày ký xong
        //                is_nhap = is_nhap
        //            });
        //        }
        //        data.Add(new
        //        {
        //            id = record.id,
        //            //hh_id = record.hh_id,
        //            soluong_dutru = soluong_dutru,
        //            soluong_mua = soluong_mua,
        //            thanhtien = thanhtien,
        //            nhacc = nhacc,
        //            nhasx = nhasx,
        //            mahh = mahh1,
        //            tenhh = tenhh1,
        //            is_new = record.is_new,
        //            grade = record.grade,
        //            masothietke = record.masothietke,
        //            dvt = record.dvt,
        //            soluong = soluong,
        //            user = record.user,
        //            list_tag = record.list_tag,
        //            tensp = record.tensp,
        //            dutru_chitiet_id = record.id,
        //            note = record.note,
        //            date = record.date != null ? record.date : dutru.date,
        //            dutru = new XuatNVLModel()
        //            {
        //                id = dutru.id,
        //                name = dutru.name,
        //                code = dutru.code,
        //                type_id = dutru.type_id,
        //                priority_id = dutru.priority_id,
        //                bophan_id = dutru.bophan_id,
        //                date = dutru.date,

        //            },
        //            user_nhanhang_id = dutru.created_by,
        //            list_muahang = list_muahang
        //        });
        //    }

        //    var jsonData = new { draw = draw, recordsFiltered = recordsFiltered, recordsTotal = recordsTotal, data = data };
        //    return Json(jsonData, new System.Text.Json.JsonSerializerOptions()
        //    {
        //        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        //        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        //    });
        //}

        //[HttpPost]
        //public async Task<JsonResult> xuatexcel()
        //{
        //    System.Security.Claims.ClaimsPrincipal currentUser = this.User;
        //    var user_id = UserManager.GetUserId(currentUser);
        //    var type_id_string = Request.Form["type_id"].FirstOrDefault();
        //    int type_id = type_id_string != null ? Convert.ToInt32(type_id_string) : 0;
        //    var mahh = Request.Form["filters[mahh]"].FirstOrDefault();
        //    var tenhh = Request.Form["filters[tenhh]"].FirstOrDefault();
        //    var filter_user_id = Request.Form["filters[user_id]"].FirstOrDefault();
        //    var filter_tags = Request.Form["filters[tags]"].FirstOrDefault();
        //    var list_dutru = Request.Form["filters[list_dutru]"].FirstOrDefault();
        //    var tensp = Request.Form["filters[tensp]"].FirstOrDefault();

        //    var sort_id = Request.Form["sorts[id]"].FirstOrDefault();
        //    var sort_mahh = Request.Form["sorts[mahh]"].FirstOrDefault();
        //    var sort_tenhh = Request.Form["sorts[tenhh]"].FirstOrDefault();
        //    var sort_tensp = Request.Form["sorts[tensp]"].FirstOrDefault();
        //    var sort_bophan = Request.Form["sorts[bophan]"].FirstOrDefault();
        //    var sort_priority = Request.Form["sorts[priority_id]"].FirstOrDefault();
        //    var sort_dutru = Request.Form["sorts[list_dutru]"].FirstOrDefault();
        //    var sort_ngayhethan = Request.Form["sorts[ngayhethan]"].FirstOrDefault();
        //    //var 

        //    var filterTable = Request.Form["filterTable"].FirstOrDefault();
        //    var dutru_id_string = Request.Form["dutru_id"].FirstOrDefault();
        //    var department_id_string = Request.Form["filters[bophan]"].FirstOrDefault();
        //    int dutru_id = dutru_id_string != null ? Convert.ToInt32(dutru_id_string) : 0;
        //    int department_id = department_id_string != null ? Convert.ToInt32(department_id_string) : 0;

        //    var departments = _QLSXContext.UserDepartmentModel.Where(d => d.user_id == user_id).Select(d => (int)d.department_id).ToList();
        //    var is_CungungNVL = departments.Contains(29) == true;
        //    var is_CungungGiantiep = departments.Contains(14) == true;
        //    var is_CungungHCTT = departments.Contains(30) == true;

        //    //var 

        //    var customerData = _QLSXContext.XuatNVLChitietModel
        //        .Include(d => d.muahang_chitiet).ThenInclude(d => d.muahang)
        //        .Include(d => d.dutru).ThenInclude(d => d.bophan)
        //        .Include(d => d.dutru).ThenInclude(d => d.user_created_by).Where(d => d.dutru.status_id == (int)Status.EsignSuccess && d.date_huy == null);

        //    Expression<Func<XuatNVLChitietModel, bool>> filter = p => p.dutru.created_by == user_id; // Biểu thức ban đầu là true (chấp nhận tất cả)

        //    filter = filter.Or(d => departments.Contains((int)d.dutru.bophan_id));
        //    if (is_CungungNVL)
        //    {
        //        filter = filter.Or(d => d.dutru.type_id == 1);
        //    }
        //    if (is_CungungGiantiep)
        //    {
        //        filter = filter.Or(d => d.dutru.type_id == 2);
        //    }
        //    if (is_CungungHCTT)
        //    {
        //        filter = filter.Or(d => d.dutru.type_id == 3);
        //    }
        //    customerData = customerData.Where(filter);



        //    if (filterTable != null && filterTable == "Chưa xử lý")
        //    {
        //        customerData = customerData.Where(d => d.muahang_chitiet.Count() == 0);
        //    }
        //    else if (filterTable != null && filterTable == "Đã xử lý")
        //    {
        //        customerData = customerData.Where(d => d.muahang_chitiet.Count() > 0);
        //    }






        //    if (dutru_id != null && dutru_id != 0)
        //    {
        //        customerData = customerData.Where(d => d.dutru_id == dutru_id);
        //    }

        //    int recordsTotal = customerData.Count();
        //    if (type_id != null && type_id != 0)
        //    {
        //        customerData = customerData.Where(d => d.dutru.type_id == type_id);
        //    }
        //    if (department_id != null && department_id != 0)
        //    {
        //        customerData = customerData.Where(d => d.dutru.bophan_id == department_id);
        //    }
        //    if (mahh != null && mahh != "")
        //    {
        //        customerData = customerData.Where(d => d.mahh.Contains(mahh));
        //    }
        //    if (tenhh != null && tenhh != "")
        //    {
        //        customerData = customerData.Where(d => d.tenhh.Contains(tenhh));
        //    }
        //    if (filter_user_id != null && filter_user_id != "")
        //    {
        //        customerData = customerData.Where(d => d.user_id == filter_user_id);
        //    }
        //    if (filter_tags != null && filter_tags != "")
        //    {
        //        customerData = customerData.Where(d => d.tags.Contains(filter_tags));
        //    }
        //    if (tensp != null && tensp != "")
        //    {
        //        customerData = customerData.Where(d => d.tensp.Contains(tensp));
        //    }
        //    if (list_dutru != null && list_dutru != "")
        //    {
        //        var list_dt = list_dutru.Split(",").ToList();
        //        var listdutru = _QLSXContext.XuatNVLModel.Where(d => list_dt.Contains(d.code)).Select(d => d.id).ToList();
        //        customerData = customerData.Where(d => listdutru.Contains(d.dutru_id));
        //    }
        //    int recordsFiltered = customerData.Count();

        //    if (sort_id != null)
        //    {
        //        if (sort_id == "1")
        //        {
        //            customerData = customerData.OrderBy(d => d.id);
        //        }
        //        else if (sort_id == "-1")
        //        {
        //            customerData = customerData.OrderByDescending(d => d.id);
        //        }
        //    }
        //    else if (sort_mahh != null)
        //    {
        //        if (sort_mahh == "1")
        //        {
        //            customerData = customerData.OrderBy(d => d.mahh);
        //        }
        //        else if (sort_mahh == "-1")
        //        {
        //            customerData = customerData.OrderByDescending(d => d.mahh);
        //        }
        //    }
        //    else if (sort_tenhh != null)
        //    {
        //        if (sort_tenhh == "1")
        //        {
        //            customerData = customerData.OrderBy(d => d.tenhh);
        //        }
        //        else if (sort_tenhh == "-1")
        //        {
        //            customerData = customerData.OrderByDescending(d => d.tenhh);
        //        }
        //    }
        //    else if (sort_tensp != null)
        //    {
        //        if (sort_tensp == "1")
        //        {
        //            customerData = customerData.OrderBy(d => d.tensp);
        //        }
        //        else if (sort_tensp == "-1")
        //        {
        //            customerData = customerData.OrderByDescending(d => d.tensp);
        //        }
        //    }
        //    else if (sort_bophan != null)
        //    {
        //        if (sort_bophan == "1")
        //        {
        //            customerData = customerData.OrderBy(d => d.dutru.bophan_id);
        //        }
        //        else if (sort_bophan == "-1")
        //        {
        //            customerData = customerData.OrderByDescending(d => d.dutru.bophan_id);
        //        }
        //    }
        //    else if (sort_priority != null)
        //    {
        //        if (sort_priority == "1")
        //        {
        //            customerData = customerData.OrderBy(d => d.dutru.priority_id);
        //        }
        //        else if (sort_priority == "-1")
        //        {
        //            customerData = customerData.OrderByDescending(d => d.dutru.priority_id);
        //        }
        //    }
        //    else if (sort_dutru != null)
        //    {
        //        if (sort_dutru == "1")
        //        {
        //            customerData = customerData.OrderBy(d => d.dutru.code);
        //        }
        //        else if (sort_dutru == "-1")
        //        {
        //            customerData = customerData.OrderByDescending(d => d.dutru.code);
        //        }
        //    }
        //    else if (sort_ngayhethan != null)
        //    {
        //        if (sort_ngayhethan == "1")
        //        {
        //            customerData = customerData.OrderBy(d => d.date).ThenBy(d => d.dutru.date);
        //        }
        //        else if (sort_ngayhethan == "-1")
        //        {
        //            customerData = customerData.OrderByDescending(d => d.date).ThenByDescending(d => d.dutru.date);
        //        }
        //    }
        //    else
        //    {
        //        customerData = customerData.OrderByDescending(d => d.id);
        //    }
        //    var datapost = customerData
        //        .Include(d => d.user)
        //        .ToList();
        //    var data = new ArrayList();

        //    var viewPath = _configuration["Source:Path_Private"] + "\\buy\\templates\\Dutru.xlsx";
        //    var documentPath = "/tmp/Rawdata_" + DateTime.Now.ToFileTimeUtc() + ".xlsx";
        //    string Domain = (HttpContext.Request.IsHttps ? "https://" : "http://") + HttpContext.Request.Host.Value;
        //    Workbook workbook = new Workbook();
        //    workbook.LoadFromFile(viewPath);
        //    Worksheet sheet = workbook.Worksheets[0];
        //    int stt = 0;
        //    var start_r = 2;

        //    DataTable dt = new DataTable();
        //    //dt.Columns.Add("stt", typeof(int));
        //    dt.Columns.Add("code", typeof(string));
        //    dt.Columns.Add("name", typeof(string));
        //    dt.Columns.Add("id", typeof(string));
        //    dt.Columns.Add("mahh", typeof(string));
        //    dt.Columns.Add("tenhh", typeof(string));
        //    dt.Columns.Add("tensp", typeof(string));
        //    dt.Columns.Add("bophan", typeof(string));
        //    dt.Columns.Add("soluong", typeof(decimal));
        //    dt.Columns.Add("dvt", typeof(string));
        //    dt.Columns.Add("is_new", typeof(string));
        //    dt.Columns.Add("grade", typeof(string));
        //    dt.Columns.Add("dangbaoche", typeof(string));
        //    dt.Columns.Add("masothietke", typeof(string));
        //    dt.Columns.Add("mansx", typeof(string));
        //    dt.Columns.Add("tennsx", typeof(string));
        //    dt.Columns.Add("mota", typeof(string));
        //    dt.Columns.Add("priority", typeof(string));
        //    dt.Columns.Add("created_at", typeof(DateTime));
        //    dt.Columns.Add("created_by", typeof(string));
        //    dt.Columns.Add("date", typeof(DateTime));
        //    dt.Columns.Add("status", typeof(string));

        //    var stt_cell = 2;



        //    sheet.InsertRow(start_r, datapost.Count(), InsertOptionsType.FormatAsAfter);
        //    foreach (var record in datapost)
        //    {
        //        var dutru = record.dutru;
        //        var tenhh1 = record.tenhh;
        //        var mahh1 = record.mahh;
        //        //if (dutru.type_id == 1)
        //        //{
        //        //var nhasx = record.nhasx;
        //        //var nhacc = "";
        //        //var material = _QLSXContext.MaterialModel.Where(d => record.mahh == d.mahh).Include(d => d.nhacungcap).FirstOrDefault();
        //        //if (material != null)
        //        //{
        //        //    nhacc = material.nhacungcap != null ? material.nhacungcap.tenncc : "";
        //        //}
        //        //}
        //        var list_muahang_ncc_id = record.muahang_chitiet.Select(d => d.muahang.muahang_chonmua_id).ToList();
        //        var list_muahang_chitiet_id = record.muahang_chitiet.Select(d => d.id).ToList();

        //        var thanhtien1 = _QLSXContext.MuahangNccChitietModel.Include(d => d.muahang_ncc).ThenInclude(d => d.muahang)
        //            .Where(d => d.muahang_ncc.muahang.deleted_at == null && d.muahang_ncc.muahang.status_id != (int)Status.MuahangEsignError && list_muahang_ncc_id.Contains(d.muahang_ncc_id) && list_muahang_chitiet_id.Contains(d.muahang_chitiet_id))
        //            .ToList();
        //        var thanhtien = thanhtien1.Sum(d => d.thanhtien_vat);
        //        var muahang_chitiet = record.muahang_chitiet.Where(d => d.muahang.deleted_at == null && d.muahang.status_id != (int)Status.MuahangEsignError).ToList();
        //        var dulieu_muahang = "";
        //        foreach (var item3 in muahang_chitiet.Select(d => d.muahang).ToList())
        //        {
        //            var status = "";
        //            if (item3.date_finish != null)
        //            {
        //                status = "Hoàn thành";

        //            }
        //            else if (item3.is_dathang == true && ((item3.loaithanhtoan == "tra_sau" && item3.is_nhanhang == true) ||
        //                            (item3.loaithanhtoan == "tra_truoc" && item3.is_thanhtoan == true)))
        //            {
        //                status = "Chờ nhận hàng";
        //            }
        //            else if (item3.is_dathang == true && ((item3.loaithanhtoan == "tra_sau" && item3.is_nhanhang == false) ||
        //                            (item3.loaithanhtoan == "tra_truoc" && item3.is_thanhtoan == false)))
        //            {
        //                status = "Chờ thanh toán";
        //            }
        //            else if (item3.status_id == 1 || item3.status_id == 6 || item3.status_id == 7)
        //            {
        //                status = "Đang thực hiện";

        //            }
        //            else if (item3.status_id == 9)
        //            {
        //                status = "Đang trình ký";

        //            }
        //            else if (item3.status_id == 10)
        //            {
        //                status = "Đang đặt hàng";

        //            }
        //            else if (item3.status_id == 11)
        //            {
        //                status = "Không duyệt";

        //            }
        //            dulieu_muahang += $"{item3.id} - {item3.code} - {status}\n";
        //        }
        //        DataRow dr1 = dt.NewRow();
        //        //dr1["stt"] = (++stt);
        //        dr1["code"] = dutru.code;
        //        dr1["name"] = dutru.name;

        //        dr1["id"] = record.id;
        //        dr1["mahh"] = record.mahh;
        //        dr1["tenhh"] = record.tenhh;
        //        dr1["tensp"] = record.tensp;
        //        dr1["bophan"] = dutru.bophan.name;
        //        dr1["soluong"] = record.soluong;
        //        dr1["dvt"] = record.dvt;
        //        dr1["is_new"] = record.is_new == true ? "Unactive" : "Active";
        //        dr1["grade"] = record.grade;
        //        dr1["dangbaoche"] = record.dangbaoche;
        //        dr1["masothietke"] = record.masothietke;
        //        dr1["mansx"] = record.mansx;
        //        dr1["tennsx"] = record.nhasx;
        //        dr1["mota"] = record.note;
        //        dr1["created_at"] = dutru.created_at;
        //        dr1["created_by"] = dutru.user_created_by.FullName;
        //        dr1["date"] = record.date != null ? record.date : dutru.date;

        //        dr1["status"] = dulieu_muahang;
        //        dt.Rows.Add(dr1);
        //        start_r++;

        //    }
        //    sheet.InsertDataTable(dt, false, 2, 1);

        //    workbook.SaveToFile("./wwwroot" + documentPath, ExcelVersion.Version2013);

        //    return Json(new { success = true, link = Domain + documentPath });
        //}

        private void CopyValues<T>(T target, T source)
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
        private bool ConvertWordFile(string file, string outputDirectory)
        {
            string libreOfficePath = _configuration["LibreOffice:Path"];
            //// FIXME: file name escaping: I have not idea how to do it in .NET.
            ProcessStartInfo procStartInfo = new ProcessStartInfo(libreOfficePath, string.Format("--headless --convert-to pdf --nologo " + file + " --outdir " + outputDirectory));
            procStartInfo.RedirectStandardOutput = true;
            procStartInfo.UseShellExecute = false;
            procStartInfo.CreateNoWindow = true;
            procStartInfo.WorkingDirectory = Environment.CurrentDirectory;

            Process process = new Process() { StartInfo = procStartInfo, };
            process.Start();
            process.WaitForExit();

            if (process.ExitCode != 0)
                return false;
            return true;
        }


    }
}
