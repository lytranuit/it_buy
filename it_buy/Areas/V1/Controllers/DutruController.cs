

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json.Serialization;
using Vue.Data;
using Vue.Models;
using Vue.Services;
using Microsoft.CodeAnalysis;
using LinqKit;
using System.Linq.Expressions;
using System.Linq;
using Spire.Xls;
using static iText.Svg.SvgConstants;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace it_template.Areas.V1.Controllers
{

    public class DutruController : BaseController
    {
        private readonly IConfiguration _configuration;
        private UserManager<UserModel> UserManager;
        private readonly ViewRender _view;
        private QLSXContext _QLSXContext;
        public DutruController(ItContext context, QLSXContext qlsxcontext, IConfiguration configuration, UserManager<UserModel> UserMgr, ViewRender view) : base(context)
        {
            _configuration = configuration;
            UserManager = UserMgr;
            _view = view;
            _QLSXContext = qlsxcontext;
        }
        public JsonResult Get(int id)
        {
            var data = _context.DutruModel.Where(d => d.id == id)
                .Include(d => d.chitiet.OrderBy(e => e.stt))
                .ThenInclude(d => d.dinhkem.Where(d => d.deleted_at == null))
                .Include(d => d.user_created_by).FirstOrDefault();
            if (data != null)
            {
                var stt = 1;
                foreach (var item in data.chitiet)
                {
                    item.stt = stt++;
                    item.can_huy = true;
                    if (item.date_huy != null)
                    {
                        item.can_huy = false;

                    }
                    else
                    {
                        var count_muahang = _context.MuahangChitietModel.Include(d => d.muahang).Where(d => d.dutru_chitiet_id == item.id && (d.muahang.deleted_at == null && d.muahang.status_id != (int)Status.MuahangEsignError)).Count();
                        if (count_muahang > 0)
                        {
                            item.can_huy = false;
                        }
                    }
                }
            }
            return Json(data, new System.Text.Json.JsonSerializerOptions()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
            });
        }
        public JsonResult GetFiles(int id)
        {
            var data = new List<RawFileDutru>();
            ///File up
            var files_up = _context.DutruDinhkemModel.Where(d => d.dutru_id == id && d.deleted_at == null).Include(d => d.user_created_by).ToList();
            if (files_up.Count > 0)
            {
                data.AddRange(files_up.GroupBy(d => new { d.note, d.created_at }).Select(d => new RawFileDutru
                {
                    note = d.First().note,
                    list_file = d.ToList(),
                    is_user_upload = true,
                    created_at = d.Key.created_at
                }).ToList());
            }

            ///Sort
            data = data.OrderBy(d => d.created_at).ToList();
            return Json(data, new System.Text.Json.JsonSerializerOptions()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
            });
        }
        public JsonResult GetNhanhang(int id)
        {
            var list_items = _context.DutruChitietModel.Where(d => d.dutru_id == id).Select(d => d.id).ToList();
            var data = _context.MuahangChitietModel.Include(d => d.muahang).ThenInclude(d => d.muahang_chonmua).ThenInclude(d => d.ncc)
                .Where(d => list_items.Contains(d.dutru_chitiet_id) && d.muahang.is_dathang == true && d.muahang.deleted_at == null).Include(d => d.user_nhanhang).ToList();
            if (data != null)
            {
                //var stt = 1;
                foreach (var item in data)
                {
                    //var material = _context.MaterialModel.Where(d => item.hh_id == "m-" + d.id).FirstOrDefault();
                    //if (material != null)
                    //{
                    //    item.tenhh = material.tenhh;
                    //    item.mahh = material.mahh;
                    //}
                    //var chonmua = _context.MuahangNccModel.Where(d => d.id == item.muahang.muahang_chonmua_id).Include(d => d.ncc).FirstOrDefault();
                    //item.muahang.muahang_chonmua = chonmua;
                    item.soluong_nhanhang = item.soluong;
                }
            }
            var list = data.GroupBy(d => new { d.muahang }).Select(d => new
            {
                muahang = d.Key.muahang,
                items = d.ToList()
            });

            return Json(list, new System.Text.Json.JsonSerializerOptions()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
            });
        }
        public JsonResult GetMuahang(int id)
        {
            var list_items = _context.DutruChitietModel.Where(d => d.dutru_id == id).Select(d => d.id).ToList();
            var data = _context.MuahangChitietModel
                .Include(d => d.muahang).ThenInclude(d => d.muahang_chonmua).ThenInclude(d => d.ncc)
                .Where(d => list_items.Contains(d.dutru_chitiet_id) && d.muahang.deleted_at == null && d.muahang.status_id != (int)Status.MuahangEsignError)
                .ToList();

            var list = data.GroupBy(d => new { d.muahang }).Select(d => d.Key.muahang).ToList();

            return Json(list, new System.Text.Json.JsonSerializerOptions()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
            });
        }

        [HttpPost]
        public async Task<JsonResult> Delete(int id)
        {
            var Model = _context.DutruModel.Where(d => d.id == id).FirstOrDefault();
            Model.deleted_at = DateTime.Now;
            _context.Update(Model);
            _context.SaveChanges();
            return Json(Model, new System.Text.Json.JsonSerializerOptions()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
            });
        }

        [HttpPost]
        public async Task<JsonResult> xoadinhkem(List<int> list_id)
        {
            var Model = _context.DutruDinhkemModel.Where(d => list_id.Contains(d.id)).ToList();
            foreach (var item in Model)
            {
                item.deleted_at = DateTime.Now;
            }
            _context.UpdateRange(Model);
            _context.SaveChanges();
            return Json(new { success = true });
        }
        [HttpPost]
        public async Task<JsonResult> xoachitietdinhkem(int id)
        {
            var Model = _context.DutruChitietDinhkemModel.Where(d => d.id == id).FirstOrDefault();
            Model.deleted_at = DateTime.Now;
            _context.Update(Model);
            _context.SaveChanges();
            return Json(new { success = true });
        }
        [HttpPost]
        public async Task<JsonResult> huychitiet(int id, string note_huy)
        {
            var Model = _context.DutruChitietModel.Where(d => d.id == id).FirstOrDefault();
            Model.note_huy = note_huy;
            Model.date_huy = DateTime.Now;
            _context.Update(Model);
            _context.SaveChanges();
            return Json(new { success = true });
        }
        [HttpPost]
        public async Task<JsonResult> phancong(int id, string user_id)
        {
            var Model = _context.DutruChitietModel.Where(d => d.id == id).FirstOrDefault();
            Model.user_id = user_id;
            _context.Update(Model);
            _context.SaveChanges();
            return Json(new { success = true });
        }
        [HttpPost]
        public async Task<JsonResult> tag(int id, List<string>? list_tag)
        {
            var Model = _context.DutruChitietModel.Where(d => d.id == id).FirstOrDefault();
            Model.tags = list_tag != null ? string.Join(",", list_tag) : null;
            _context.Update(Model);
            _context.SaveChanges();
            return Json(new { success = true });
        }
        [HttpPost]

        public async Task<JsonResult> SaveDinhkem(string note, int dutru_id)
        {
            System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            var user_id = UserManager.GetUserId(currentUser);
            var user = await UserManager.GetUserAsync(currentUser);
            //MuahangModel? MuahangModel_old;
            //MuahangModel_old = _context.MuahangModel.Where(d => d.id == muahang_id).FirstOrDefault();

            var files = Request.Form.Files;
            var now = DateTime.Now;
            var items = new List<DutruDinhkemModel>();
            if (files != null && files.Count > 0)
            {

                foreach (var file in files)
                {
                    var timeStamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
                    string name = file.FileName;
                    string type = file.Name;
                    string ext = Path.GetExtension(name);
                    string mimeType = file.ContentType;
                    //var fileName = Path.GetFileName(name);
                    var newName = timeStamp + "-" + dutru_id + "-" + name;
                    //var muahang_id = MuahangModel_old.id;
                    newName = newName.Replace("+", "_");
                    newName = newName.Replace("%", "_");
                    var dir = _configuration["Source:Path_Private"] + "\\buy\\dutru\\" + dutru_id;
                    bool exists = Directory.Exists(dir);

                    if (!exists)
                        Directory.CreateDirectory(dir);


                    var filePath = dir + "\\" + newName;

                    string url = "/private/buy/dutru/" + dutru_id + "/" + newName;
                    items.Add(new DutruDinhkemModel
                    {
                        note = note,
                        ext = ext,
                        url = url,
                        name = name,
                        mimeType = mimeType,
                        dutru_id = dutru_id,
                        created_at = now,
                        created_by = user_id
                    });

                    using (var fileSrteam = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(fileSrteam);
                    }
                }
                _context.AddRange(items);
                _context.SaveChanges();
            }

            return Json(new { success = true });
        }
        [HttpPost]
        public async Task<JsonResult> SaveNhanhang(int dutru_id, List<MuahangChitietModel> list)
        {
            System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            var user_id = UserManager.GetUserId(currentUser);
            var list_check_muahang = new List<int>();

            if (list != null)
            {
                foreach (var item in list)
                {
                    item.user_nhanhang = null;
                    if (item.status_nhanhang == 1)
                    {
                        list_check_muahang.Add(item.muahang_id);
                    }
                    _context.Update(item);
                }
            }
            _context.SaveChanges();

            ///check du tru
            var dutru = _context.DutruModel.Where(d => d.id == dutru_id).Include(d => d.chitiet).FirstOrDefault();
            var soluong_ht = 0;
            var chitiet = dutru.chitiet.Where(d => d.date_huy == null).ToList();
            foreach (var item in chitiet)
            {
                var soluong_dutru = item.soluong;
                var muahang_chitiet = _context.MuahangChitietModel.Where(d => d.dutru_chitiet_id == item.id && d.status_nhanhang == 1).ToList();
                var soluong_mua = muahang_chitiet.Sum(d => d.soluong * d.quidoi);
                if (soluong_dutru == soluong_mua)
                {
                    soluong_ht++;
                }
            }
            if (soluong_ht == chitiet.Count())
            {
                dutru.date_finish = DateTime.Now;
                _context.Update(dutru);
                _context.SaveChanges();
            }
            /////Update Finish Mua hàng
            list_check_muahang = list_check_muahang.Distinct().ToList();
            foreach (var item in list_check_muahang)
            {
                var muahang = _context.MuahangModel.Where(d => d.id == item).Include(d => d.chitiet).FirstOrDefault();
                var list_nhanhang = muahang.chitiet.Where(d => d.status_nhanhang == 1).Count();

                if (list_nhanhang == muahang.chitiet.Count())
                {
                    muahang.is_nhanhang = true;
                    _context.Update(muahang);
                }
                //if (muahang.is_thanhtoan == true && muahang.is_nhanhang == true)
                //{
                //    muahang.date_finish = DateTime.Now;
                //    _context.Update(muahang);
                //}
            }
            _context.SaveChanges();

            return Json(new { success = true });
        }
        [HttpPost]
        public async Task<JsonResult> Save(DutruModel DutruModel, List<DutruChitietModel> list_add, List<DutruChitietModel>? list_update, List<DutruChitietModel>? list_delete)
        {
            try
            {
                System.Security.Claims.ClaimsPrincipal currentUser = this.User;
                var user_id = UserManager.GetUserId(currentUser);
                var user = await UserManager.GetUserAsync(currentUser);
                DutruModel? DutruModel_old;
                if (DutruModel.date != null && DutruModel.date.Value.Kind == DateTimeKind.Utc)
                {
                    DutruModel.date = DutruModel.date.Value.ToLocalTime();
                }
                if (DutruModel.id == 0)
                {
                    DutruModel.created_at = DateTime.Now;
                    DutruModel.status_id = (int)Status.Draft;
                    DutruModel.created_by = user_id;
                    DutruModel.activeStep = 0;

                    _context.DutruModel.Add(DutruModel);

                    _context.SaveChanges();

                    DutruModel_old = DutruModel;

                }
                else
                {

                    DutruModel_old = _context.DutruModel.Where(d => d.id == DutruModel.id).Include(d => d.user_created_by).FirstOrDefault();

                    DutruModel.user_created_by = DutruModel_old.user_created_by;
                    CopyValues<DutruModel>(DutruModel_old, DutruModel);
                    DutruModel_old.updated_at = DateTime.Now;

                    _context.Update(DutruModel_old);
                    _context.SaveChanges();
                }



                var list = new List<DutruChitietModel>();
                if (list_delete != null)
                    _context.RemoveRange(list_delete);
                if (list_add != null)
                {

                    foreach (var item in list_add)
                    {
                        if (item.date != null && item.date.Value.Kind == DateTimeKind.Utc)
                        {
                            item.date = item.date.Value.ToLocalTime();
                        }
                        item.dutru_id = DutruModel_old.id;
                        _context.Add(item);
                        //_context.SaveChanges();
                        list.Add(item);
                    }
                }
                if (list_update != null)
                {
                    foreach (var item in list_update)
                    {
                        if (item.date != null && item.date.Value.Kind == DateTimeKind.Utc)
                        {
                            item.date = item.date.Value.ToLocalTime();
                        }
                        item.dinhkem = null;
                        _context.Update(item);
                        list.Add(item);
                    }
                }

                _context.SaveChanges();
                Console.WriteLine(list);


                var files = Request.Form.Files;

                var items_attachment = new List<DutruChitietDinhkemModel>();
                if (files != null && files.Count > 0)
                {

                    foreach (var file in files)
                    {
                        var timeStamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
                        string name = file.FileName;
                        string type = file.Name;
                        string ext = Path.GetExtension(name);
                        string mimeType = file.ContentType;

                        var list1 = type.Split("_");
                        var stt = Int32.Parse(list1[1]);
                        var dutru_chitiet = list.Where(d => d.stt == stt).First();
                        var dutru_chitiet_id = dutru_chitiet.id;
                        var dutru_id = dutru_chitiet.dutru_id;

                        //var fileName = Path.GetFileName(name);
                        var newName = timeStamp + "-" + dutru_chitiet_id + "-" + name;

                        newName = newName.Replace("+", "_");
                        newName = newName.Replace("%", "_");
                        var dir = _configuration["Source:Path_Private"] + "\\buy\\dutru\\" + dutru_id;
                        bool exists = Directory.Exists(dir);

                        if (!exists)
                            Directory.CreateDirectory(dir);


                        var filePath = dir + "\\" + newName;

                        string url = "/private/buy/dutru/" + dutru_id + "/" + newName;
                        items_attachment.Add(new DutruChitietDinhkemModel
                        {
                            ext = ext,
                            url = url,
                            name = name,
                            mimeType = mimeType,
                            dutru_chitiet_id = dutru_chitiet_id,
                            created_at = DateTime.Now,
                            created_by = user_id
                        });

                        using (var fileSrteam = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(fileSrteam);
                        }
                    }
                    _context.AddRange(items_attachment);
                    _context.SaveChanges();
                }
                return Json(new { success = true, id = DutruModel_old.id });
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
            var data = _context.DutruModel.Where(d => d.id == id).Include(d => d.chitiet.OrderBy(e => e.stt)).Include(d => d.bophan).FirstOrDefault();
            if (data != null)
            {
                var stt = 1;
                foreach (var item in data.chitiet)
                {

                    //stt { get; set; }

                    //public string tennvl { get; set; }
                    //public string manvl { get; set; }
                    //public string artwork { get; set; }
                    //public string dvt { get; set; }
                    //public string soluong { get; set; }
                    //public string date { get; set; }
                    //public string note { get; set; }
                    //if (item.mahh == null && data.type_id != 1)
                    //{
                    //    //tao ma
                    //    var hh = new MaterialModel()
                    //    {
                    //        nhom = "Khac",
                    //        mahh = "HH-" + new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds(),
                    //        tenhh = item.tenhh,
                    //        dvt = item.dvt,
                    //    };
                    //    _context.Add(hh);
                    //    _context.SaveChanges();
                    //    hh.mahh = "HH-" + hh.id;
                    //    _context.Update(hh);
                    //    item.hh_id = "m-" + hh.mahh;
                    //    item.mahh = hh.mahh;
                    //    _context.Update(item);
                    //    _context.SaveChanges();

                    //}
                    var date = item.date != null ? item.date : data.date;
                    var date_string = date.Value.ToString("yyyy-MM-dd");
                    RawDetails.Add(new RawDetails
                    {
                        stt = stt++,
                        tennvl = item.tenhh,
                        manvl = item.mahh,
                        dvt = item.dvt,
                        soluong = item.soluong.Value.ToString("#,##0.##", vietnamCulture),
                        note = item.note,
                        artwork = item.masothietke,
                        date = date_string,
                        grade = item.grade,
                        nhasx = item.nhasx,
                        dangbaoche = item.dangbaoche,
                        tensp = item.tensp,
                    });

                }
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
            //Loads the word document
            if (data.type_id == 1)
            {

                document.LoadFromFile(_configuration["Source:Path_Private"] + "/buy/templates/dutru_nvl.docx", Spire.Doc.FileFormat.Docx);
            }
            else
            {
                document.LoadFromFile(_configuration["Source:Path_Private"] + "/buy/templates/dutru_giantiep.docx", Spire.Doc.FileFormat.Docx);
            }


            var now = DateTime.Now;
            var raw = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"ngay",now.ToString("dd") },
                {"thang",now.ToString("MM") },
                {"nam",now.ToString("yyyy") },
                {"bophan",data.bophan.name },
                {"code",data.code },
                {"tonggiatri",data.tonggiatri },
                {"note",data.note },
            };


            string[] fieldName = raw.Keys.ToArray();
            string[] fieldValue = raw.Values.ToArray();

            string[] MergeFieldNames = document.MailMerge.GetMergeFieldNames();
            string[] GroupNames = document.MailMerge.GetMergeGroupNames();

            document.MailMerge.Execute(fieldName, fieldValue);
            document.MailMerge.ExecuteWidthRegion(datatable_details);


            var timeStamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
            if (Directory.Exists(_configuration["Source:Path_Private"] + "\\buy\\dutru\\" + id))
            {
                Directory.CreateDirectory(_configuration["Source:Path_Private"] + "\\buy\\dutru\\" + id);
            }
            string url = "/private/buy/dutru/" + id + "/" + timeStamp + ".docx";
            string url_pdf = "/private/buy/dutru/" + id + "/" + timeStamp + ".pdf";
            document.SaveToFile(_configuration["Source:Path_Private"] + url.Replace("/private", "").Replace("/", "\\"), Spire.Doc.FileFormat.Docx);

            var url_return = url;
            var convert_to_pdf = true;
            if (convert_to_pdf == true)
            {
                var output = ConvertWordFile(_configuration["Source:Path_Private"] + url.Replace("/private", "").Replace("/", "\\"), _configuration["Source:Path_Private"] + "\\buy\\dutru\\" + id);
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
                //_context.SaveChanges();


                ///UPLOAD ESIGN
                System.Security.Claims.ClaimsPrincipal currentUser = this.User;
                var user_id = UserManager.GetUserId(currentUser);
                var user = await UserManager.GetUserAsync(currentUser);

                ///Document
                var type_id = 72;
                if (data.type_id == 1)
                {
                    type_id = 78;
                }
                else if (data.type_id == 3)
                {
                    type_id = 79;
                }
                var DocumentModel = new DocumentModel()
                {
                    name_vi = data.name,
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
                    name = data.name,
                    mimeType = "application/pdf",
                    created_at = DateTime.Now
                };
                _context.Add(DocumentFileModel);
                ////Đính kèm
                var list_attachment = new List<DocumentAttachmentModel>();
                var dinhkem = _context.DutruDinhkemModel.Where(d => d.dutru_id == id).ToList();
                foreach (var d in dinhkem)
                {
                    list_attachment.Add(new DocumentAttachmentModel()
                    {
                        document_id = DocumentModel.id,
                        name = d.name,
                        ext = d.ext,
                        mimeType = d.mimeType,
                        url = d.url,
                        created_at = d.created_at
                    });
                }
                _context.AddRange(list_attachment);

                ///Đính kèm từng item
                var list_chitiet = data.chitiet.Select(d => d.id).ToList();
                var list_attachment2 = new List<DocumentAttachmentModel>();
                var dinhkemchitiet = _context.DutruChitietDinhkemModel.Where(d => list_chitiet.Contains(d.dutru_chitiet_id)).ToList();
                foreach (var d in dinhkemchitiet)
                {
                    list_attachment2.Add(new DocumentAttachmentModel()
                    {
                        document_id = DocumentModel.id,
                        name = d.name,
                        ext = d.ext,
                        mimeType = d.mimeType,
                        url = d.url,
                        created_at = d.created_at
                    });
                }
                _context.AddRange(list_attachment2);

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
                    type = "dutru",
                    created_at = DateTime.Now
                };
                _context.Add(RelatedEsignModel);

                //_context.SaveChanges();
                data.status_id = (int)Status.Esign;
                data.activeStep = 1;
                data.esign_id = DocumentModel.id;
                data.code = DocumentModel.code;
                _context.Update(data);

                _context.SaveChanges();

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
            var department_id_string = Request.Form["filters[bophan_id]"].FirstOrDefault();
            int department_id = department_id_string != null ? Convert.ToInt32(department_id_string) : 0;
            var type_id_string = Request.Form["type_id"].FirstOrDefault();
            var type1_string = Request.Form["type1"].FirstOrDefault();
            int type_id = type_id_string != null ? Convert.ToInt32(type_id_string) : 0;
            int type1 = type1_string != null ? Convert.ToInt32(type1_string) : 0;
            //var tenhh = Request.Form["filters[tenhh]"].FirstOrDefault();
            int skip = start != null ? Convert.ToInt32(start) : 0;
            var customerData = _context.DutruModel.Where(d => d.deleted_at == null);

            var departments = _context.UserDepartmentModel.Where(d => d.user_id == user_id).Select(d => (int)d.department_id).ToList();
            var is_CungungNVL = departments.Contains(29) == true;
            var is_CungungGiantiep = departments.Contains(14) == true;
            var is_CungungHCTT = departments.Contains(30) == true;

            Expression<Func<DutruModel, bool>> filter = p => p.created_by == user_id; // Biểu thức ban đầu là true (chấp nhận tất cả)

            filter = filter.Or(d => departments.Contains((int)d.bophan_id));
            if (is_CungungNVL)
            {
                filter = filter.Or(d => d.type_id == 1);
            }
            if (is_CungungGiantiep)
            {
                filter = filter.Or(d => d.type_id == 2);
            }
            if (is_CungungHCTT)
            {
                filter = filter.Or(d => d.type_id == 3);
            }
            customerData = customerData.Where(filter);



            if (type_id > 0)
            {
                customerData = customerData.Where(d => d.type_id == type_id);
            }

            if (type1 == 1)
            {
                customerData = customerData.Where(d => d.created_by == user_id);
            }
            else if (type1 == 2)
            {
                //Phan cho toi
                var list_dutru_1 = _context.DutruChitietModel.Where(d => d.user_id == user_id).Select(d => d.dutru_id).ToList();
                customerData = customerData.Where(d => list_dutru_1.Contains(d.id));
            }




            int recordsTotal = customerData.Count();

            if (department_id > 0)
            {
                customerData = customerData.Where(d => d.bophan_id == department_id);
            }
            if (code != null && code != "")
            {
                customerData = customerData.Where(d => d.code.Contains(code));
            }
            if (name != null && name != "")
            {
                customerData = customerData.Where(d => d.name.Contains(name));
            }
            if (id != 0)
            {
                customerData = customerData.Where(d => d.id == id);
            }
            if (priority_id != 0)
            {
                customerData = customerData.Where(d => d.priority_id == priority_id);
            }
            if (status_id != 0)
            {
                customerData = customerData.Where(d => d.status_id == status_id);
            }
            int recordsFiltered = customerData.Count();
            var datapost = customerData.OrderByDescending(d => d.id).Skip(skip).Take(pageSize)
                .Include(d => d.user_created_by).Include(d => d.chitiet)
                .ThenInclude(d => d.muahang_chitiet).ThenInclude(d => d.muahang).ToList();
            var data = new ArrayList();
            foreach (var record in datapost)
            {
                var muahang = record.chitiet
                    .SelectMany(d =>
                    d.muahang_chitiet.Where(d => d.muahang.deleted_at == null && d.muahang.status_id != (int)Status.MuahangEsignError).Select(d => d.muahang).ToList())
                    .Distinct()
                    .Select(d => new MuahangModel()
                    {
                        id = d.id,
                        name = d.name,
                        code = d.code,
                        date_finish = d.date_finish,
                        is_dathang = d.is_dathang,
                        is_thanhtoan = d.is_thanhtoan,
                        is_nhanhang = d.is_nhanhang,
                        loaithanhtoan = d.loaithanhtoan,
                        status_id = d.status_id

                    })
                    .ToList();
                var data1 = new
                {
                    id = record.id,
                    code = record.code,
                    name = record.name,
                    created_by = record.created_by,
                    user_created_by = record.user_created_by,
                    status_id = record.status_id,
                    created_at = record.created_at,
                    priority_id = record.priority_id,
                    bophan_id = record.bophan_id,
                    list_muahang = muahang
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
        public async Task<JsonResult> TableChitiet()
        {
            System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            var user_id = UserManager.GetUserId(currentUser);
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            var type_id_string = Request.Form["type_id"].FirstOrDefault();
            var filter_DNMH_string = Request.Form["filter_DNMH"].FirstOrDefault();
            int filter_DNMH = filter_DNMH_string != null ? Convert.ToInt32(filter_DNMH_string) : 0;
            int type_id = type_id_string != null ? Convert.ToInt32(type_id_string) : 0;
            var mahh = Request.Form["filters[mahh]"].FirstOrDefault();
            var tenhh = Request.Form["filters[tenhh]"].FirstOrDefault();
            var filter_user_id = Request.Form["filters[user_id]"].FirstOrDefault();
            var filter_tags = Request.Form["filters[tags]"].FirstOrDefault();
            var list_dutru = Request.Form["filters[list_dutru]"].FirstOrDefault();
            var id_string = Request.Form["filters[id]"].FirstOrDefault();
            var priority_id_string = Request.Form["filters[priority_id]"].FirstOrDefault();
            int priority_id = priority_id_string != null ? Convert.ToInt32(priority_id_string) : 0;
            var tensp = Request.Form["filters[tensp]"].FirstOrDefault();

            var filterTable = Request.Form["filterTable"].FirstOrDefault();
            var dutru_id_string = Request.Form["dutru_id"].FirstOrDefault();
            var department_id_string = Request.Form["filters[bophan]"].FirstOrDefault();
            int id = id_string != null ? Convert.ToInt32(id_string) : 0;
            int dutru_id = dutru_id_string != null ? Convert.ToInt32(dutru_id_string) : 0;
            int department_id = department_id_string != null ? Convert.ToInt32(department_id_string) : 0;

            var departments = _context.UserDepartmentModel.Where(d => d.user_id == user_id).Select(d => (int)d.department_id).ToList();
            var is_CungungNVL = departments.Contains(29) == true;
            var is_CungungGiantiep = departments.Contains(14) == true;
            var is_CungungHCTT = departments.Contains(30) == true;

            var sort_id = Request.Form["sorts[id]"].FirstOrDefault();
            var sort_mahh = Request.Form["sorts[mahh]"].FirstOrDefault();
            var sort_tenhh = Request.Form["sorts[tenhh]"].FirstOrDefault();
            var sort_tensp = Request.Form["sorts[tensp]"].FirstOrDefault();
            var sort_bophan = Request.Form["sorts[bophan]"].FirstOrDefault();
            var sort_priority = Request.Form["sorts[priority_id]"].FirstOrDefault();
            var sort_dutru = Request.Form["sorts[list_dutru]"].FirstOrDefault();
            var sort_ngayhethan = Request.Form["sorts[ngayhethan]"].FirstOrDefault();
            //var 

            //var tenhh = Request.Form["filters[tenhh]"].FirstOrDefault();
            int skip = start != null ? Convert.ToInt32(start) : 0;
            var customerData = _context.DutruChitietModel
                .Include(d => d.muahang_chitiet).ThenInclude(d => d.muahang).ThenInclude(d => d.muahang_chonmua).ThenInclude(d => d.ncc)
                .Include(d => d.dutru).Where(d => d.dutru.status_id == (int)Status.EsignSuccess && d.date_huy == null);

            Expression<Func<DutruChitietModel, bool>> filter = p => p.dutru.created_by == user_id; // Biểu thức ban đầu là true (chấp nhận tất cả)

            filter = filter.Or(d => departments.Contains((int)d.dutru.bophan_id));
            if (is_CungungNVL)
            {
                filter = filter.Or(d => d.dutru.type_id == 1);
            }
            if (is_CungungGiantiep)
            {
                filter = filter.Or(d => d.dutru.type_id == 2);
            }
            if (is_CungungHCTT)
            {
                filter = filter.Or(d => d.dutru.type_id == 3);
            }
            customerData = customerData.Where(filter);

            var list_muahang_id = _QLSXContext.VattuNhapChiTietModel.Select(d => d.muahang_id).ToList();

            if (filterTable != null && filterTable == "1")
            {
                customerData = customerData.Where(d => d.muahang_chitiet.Count() == 0);
            }
            else if (filterTable != null && filterTable == "2")
            {
                customerData = customerData.Where(d => d.muahang_chitiet.Count() > 0);
            }
            else if (filterTable != null && filterTable == "3")
            {
                customerData = customerData.Where(d => d.mahh != null && d.muahang_chitiet.Any(m => !list_muahang_id.Contains(m.muahang_id)));
            }
            else if (filterTable != null && filterTable == "4")
            {
                customerData = customerData.Where(d => d.mahh != null && d.muahang_chitiet.Any(m => list_muahang_id.Contains(m.muahang_id)));
            }








            if (dutru_id != null && dutru_id != 0)
            {
                customerData = customerData.Where(d => d.dutru_id == dutru_id);
            }

            int recordsTotal = customerData.Count();
            if (filter_DNMH == 1)
            {
                customerData = customerData.Where(d => d.muahang_chitiet.Any(m => m.muahang != null && (m.muahang.status_id == 1 || m.muahang.status_id == 6 || m.muahang.status_id == 7)));
            }
            else if (filter_DNMH == 2)
            {
                customerData = customerData.Where(d => d.muahang_chitiet.Any(m => m.muahang != null && m.muahang.status_id == 9));
            }
            else if (filter_DNMH == 3)
            {
                customerData = customerData.Where(d => d.muahang_chitiet.Any(m => m.muahang != null && m.muahang.status_id == 11));
            }
            else if (filter_DNMH == 4)
            {
                customerData = customerData.Where(d => d.muahang_chitiet.Any(m => m.muahang != null && m.muahang.status_id == 10 && m.muahang.is_dathang != true));
            }
            else if (filter_DNMH == 5)
            {
                customerData = customerData.Where(d => d.muahang_chitiet.Any(m => m.muahang != null && m.muahang.date_finish == null && (m.muahang.is_dathang == true && ((m.muahang.loaithanhtoan == "tra_sau" && m.muahang.is_nhanhang == false) || (m.muahang.loaithanhtoan == "tra_truoc" && m.muahang.is_thanhtoan == true)))));
            }
            else if (filter_DNMH == 6)
            {
                customerData = customerData.Where(d => d.muahang_chitiet.Any(m => m.muahang != null && m.muahang.date_finish == null && (m.muahang.is_dathang == true && ((m.muahang.loaithanhtoan == "tra_truoc" && m.muahang.is_thanhtoan == false) || (m.muahang.loaithanhtoan == "tra_sau" && m.muahang.is_nhanhang == true)))));
            }
            else if (filter_DNMH == 7)
            {
                customerData = customerData.Where(d => d.muahang_chitiet.Any(m => m.muahang != null && m.muahang.date_finish != null));
            }

            if (id != null && id != 0)
            {
                customerData = customerData.Where(d => d.id == id);
            }
            if (type_id != null && type_id != 0)
            {
                customerData = customerData.Where(d => d.dutru.type_id == type_id);
            }
            if (priority_id != null && priority_id != 0)
            {
                customerData = customerData.Where(d => d.dutru.priority_id == priority_id);
            }
            if (department_id != null && department_id != 0)
            {
                customerData = customerData.Where(d => d.dutru.bophan_id == department_id);
            }
            if (mahh != null && mahh != "")
            {
                customerData = customerData.Where(d => d.mahh.Contains(mahh));
            }
            if (tenhh != null && tenhh != "")
            {
                customerData = customerData.Where(d => d.tenhh.Contains(tenhh));
            }
            if (filter_user_id != null && filter_user_id != "")
            {
                customerData = customerData.Where(d => d.user_id == filter_user_id);
            }
            if (filter_tags != null && filter_tags != "")
            {
                customerData = customerData.Where(d => d.tags.Contains(filter_tags));
            }
            if (tensp != null && tensp != "")
            {
                customerData = customerData.Where(d => d.tensp.Contains(tensp));
            }
            if (list_dutru != null && list_dutru != "")
            {
                var list_dt = list_dutru.Split(",").ToList();
                var listdutru = _context.DutruModel.Where(d => list_dt.Contains(d.code)).Select(d => d.id).ToList();
                customerData = customerData.Where(d => listdutru.Contains(d.dutru_id));
            }
            int recordsFiltered = customerData.Count();

            if (sort_id != null)
            {
                if (sort_id == "1")
                {
                    customerData = customerData.OrderBy(d => d.id);
                }
                else if (sort_id == "-1")
                {
                    customerData = customerData.OrderByDescending(d => d.id);
                }
            }
            else if (sort_mahh != null)
            {
                if (sort_mahh == "1")
                {
                    customerData = customerData.OrderBy(d => d.mahh);
                }
                else if (sort_mahh == "-1")
                {
                    customerData = customerData.OrderByDescending(d => d.mahh);
                }
            }
            else if (sort_tenhh != null)
            {
                if (sort_tenhh == "1")
                {
                    customerData = customerData.OrderBy(d => d.tenhh);
                }
                else if (sort_tenhh == "-1")
                {
                    customerData = customerData.OrderByDescending(d => d.tenhh);
                }
            }
            else if (sort_tensp != null)
            {
                if (sort_tensp == "1")
                {
                    customerData = customerData.OrderBy(d => d.tensp);
                }
                else if (sort_tensp == "-1")
                {
                    customerData = customerData.OrderByDescending(d => d.tensp);
                }
            }
            else if (sort_bophan != null)
            {
                if (sort_bophan == "1")
                {
                    customerData = customerData.OrderBy(d => d.dutru.bophan_id);
                }
                else if (sort_bophan == "-1")
                {
                    customerData = customerData.OrderByDescending(d => d.dutru.bophan_id);
                }
            }
            else if (sort_priority != null)
            {
                if (sort_priority == "1")
                {
                    customerData = customerData.OrderBy(d => d.dutru.priority_id);
                }
                else if (sort_priority == "-1")
                {
                    customerData = customerData.OrderByDescending(d => d.dutru.priority_id);
                }
            }
            else if (sort_dutru != null)
            {
                if (sort_dutru == "1")
                {
                    customerData = customerData.OrderBy(d => d.dutru.code);
                }
                else if (sort_dutru == "-1")
                {
                    customerData = customerData.OrderByDescending(d => d.dutru.code);
                }
            }
            else if (sort_ngayhethan != null)
            {
                if (sort_ngayhethan == "1")
                {
                    customerData = customerData.OrderBy(d => d.date).ThenBy(d => d.dutru.date);
                }
                else if (sort_ngayhethan == "-1")
                {
                    customerData = customerData.OrderByDescending(d => d.date).ThenByDescending(d => d.dutru.date);
                }
            }
            else
            {
                customerData = customerData.OrderByDescending(d => d.id);
            }


            var datapost = customerData.Skip(skip).Take(pageSize)
                .Include(d => d.user)
                .ToList();
            var data = new ArrayList();
            foreach (var record in datapost)
            {
                var dutru = record.dutru;
                var tenhh1 = record.tenhh;
                var mahh1 = record.mahh;
                //if (dutru.type_id == 1)
                //{
                var nhasx = record.nhasx;
                var nhacc = "";
                var material = _context.MaterialModel.Where(d => record.mahh == d.mahh).Include(d => d.nhacungcap).FirstOrDefault();
                if (material != null)
                {
                    nhacc = material.nhacungcap != null ? material.nhacungcap.tenncc : "";
                }
                //}
                var list_muahang_ncc_id = record.muahang_chitiet.Select(d => d.muahang.muahang_chonmua_id).ToList();
                var list_muahang_chitiet_id = record.muahang_chitiet.Select(d => d.id).ToList();

                var MuahangNccChitietModel = _context.MuahangNccChitietModel.Include(d => d.muahang_ncc).ThenInclude(d => d.muahang)
                    .Where(d => d.muahang_ncc.muahang.deleted_at == null && d.muahang_ncc.muahang.status_id != (int)Status.MuahangEsignError && list_muahang_ncc_id.Contains(d.muahang_ncc_id) && list_muahang_chitiet_id.Contains(d.muahang_chitiet_id))
                    .ToList();
                var thanhtien = MuahangNccChitietModel.Sum(d => d.thanhtien_vat);
                var muahang_chitiet = record.muahang_chitiet.Where(d => d.muahang.deleted_at == null && d.muahang.status_id != (int)Status.MuahangEsignError).ToList();
                var soluong_dutru = record.soluong;
                var soluong_mua = muahang_chitiet.Where(d => d.muahang.parent_id == null).Sum(d => d.soluong * d.quidoi);

                var soluong = soluong_mua < soluong_dutru ? soluong_dutru - soluong_mua : 0;
                var list_muahang = new List<dynamic>();
                foreach (var m in muahang_chitiet)
                {

                    var muahang = m.muahang;
                    if (muahang.is_multiple_ncc == true) /// Xét trường hợp mua nhiều nhà cc
                    {
                        if (muahang.pay_at != null) /// Đã ký xong
                            continue;
                        //if (muahang.parent_id > 0)
                        //{
                        //    continue;
                        //}
                    }

                    var soluong_mua_chitiet = m.soluong * m.quidoi;
                    bool is_nhap = _QLSXContext.VattuNhapChiTietModel.Where(d => d.muahang_id == muahang.id).Count() > 0;
                    list_muahang.Add(new
                    {
                        id = muahang.id,
                        name = muahang.name,
                        code = muahang.code,
                        date_finish = muahang.date_finish,
                        is_dathang = muahang.is_dathang,
                        is_thanhtoan = muahang.is_thanhtoan,
                        is_nhanhang = muahang.is_nhanhang,
                        loaithanhtoan = muahang.loaithanhtoan,
                        status_id = muahang.status_id,
                        mancc = muahang.muahang_chonmua?.ncc?.mancc,
                        soluong = soluong_mua_chitiet,
                        pay_at = muahang.pay_at, //// Ngày ký xong
                        is_nhap = is_nhap
                    });
                }
                data.Add(new
                {
                    id = record.id,
                    //hh_id = record.hh_id,
                    soluong_dutru = soluong_dutru,
                    soluong_mua = soluong_mua,
                    thanhtien = thanhtien,
                    nhacc = nhacc,
                    nhasx = nhasx,
                    mahh = mahh1,
                    tenhh = tenhh1,
                    is_new = record.is_new,
                    grade = record.grade,
                    masothietke = record.masothietke,
                    dvt = record.dvt,
                    soluong = soluong,
                    user = record.user,
                    list_tag = record.list_tag,
                    tensp = record.tensp,
                    dutru_chitiet_id = record.id,
                    note = record.note,
                    date = record.date != null ? record.date : dutru.date,
                    dutru = new DutruModel()
                    {
                        id = dutru.id,
                        name = dutru.name,
                        code = dutru.code,
                        type_id = dutru.type_id,
                        priority_id = dutru.priority_id,
                        bophan_id = dutru.bophan_id,
                        date = dutru.date,

                    },
                    user_nhanhang_id = dutru.created_by,
                    list_muahang = list_muahang
                });
            }

            var jsonData = new { draw = draw, recordsFiltered = recordsFiltered, recordsTotal = recordsTotal, data = data };
            return Json(jsonData, new System.Text.Json.JsonSerializerOptions()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
            });
        }
        [HttpPost]
        public async Task<JsonResult> xuatexcel()
        {
            System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            var user_id = UserManager.GetUserId(currentUser);
            var type_id_string = Request.Form["type_id"].FirstOrDefault();
            int type_id = type_id_string != null ? Convert.ToInt32(type_id_string) : 0;
            var mahh = Request.Form["filters[mahh]"].FirstOrDefault();
            var tenhh = Request.Form["filters[tenhh]"].FirstOrDefault();
            var filter_user_id = Request.Form["filters[user_id]"].FirstOrDefault();
            var filter_tags = Request.Form["filters[tags]"].FirstOrDefault();
            var list_dutru = Request.Form["filters[list_dutru]"].FirstOrDefault();
            var tensp = Request.Form["filters[tensp]"].FirstOrDefault();

            var sort_id = Request.Form["sorts[id]"].FirstOrDefault();
            var sort_mahh = Request.Form["sorts[mahh]"].FirstOrDefault();
            var sort_tenhh = Request.Form["sorts[tenhh]"].FirstOrDefault();
            var sort_tensp = Request.Form["sorts[tensp]"].FirstOrDefault();
            var sort_bophan = Request.Form["sorts[bophan]"].FirstOrDefault();
            var sort_priority = Request.Form["sorts[priority_id]"].FirstOrDefault();
            var sort_dutru = Request.Form["sorts[list_dutru]"].FirstOrDefault();
            var sort_ngayhethan = Request.Form["sorts[ngayhethan]"].FirstOrDefault();
            //var 

            var filterTable = Request.Form["filterTable"].FirstOrDefault();
            var dutru_id_string = Request.Form["dutru_id"].FirstOrDefault();
            var department_id_string = Request.Form["filters[bophan]"].FirstOrDefault();
            int dutru_id = dutru_id_string != null ? Convert.ToInt32(dutru_id_string) : 0;
            int department_id = department_id_string != null ? Convert.ToInt32(department_id_string) : 0;

            var departments = _context.UserDepartmentModel.Where(d => d.user_id == user_id).Select(d => (int)d.department_id).ToList();
            var is_CungungNVL = departments.Contains(29) == true;
            var is_CungungGiantiep = departments.Contains(14) == true;
            var is_CungungHCTT = departments.Contains(30) == true;

            //var 

            var customerData = _context.DutruChitietModel
                .Include(d => d.muahang_chitiet).ThenInclude(d => d.muahang)
                .Include(d => d.dutru).ThenInclude(d => d.bophan)
                .Include(d => d.dutru).ThenInclude(d => d.user_created_by).Where(d => d.dutru.status_id == (int)Status.EsignSuccess && d.date_huy == null);

            Expression<Func<DutruChitietModel, bool>> filter = p => p.dutru.created_by == user_id; // Biểu thức ban đầu là true (chấp nhận tất cả)

            filter = filter.Or(d => departments.Contains((int)d.dutru.bophan_id));
            if (is_CungungNVL)
            {
                filter = filter.Or(d => d.dutru.type_id == 1);
            }
            if (is_CungungGiantiep)
            {
                filter = filter.Or(d => d.dutru.type_id == 2);
            }
            if (is_CungungHCTT)
            {
                filter = filter.Or(d => d.dutru.type_id == 3);
            }
            customerData = customerData.Where(filter);



            if (filterTable != null && filterTable == "Chưa xử lý")
            {
                customerData = customerData.Where(d => d.muahang_chitiet.Count() == 0);
            }
            else if (filterTable != null && filterTable == "Đã xử lý")
            {
                customerData = customerData.Where(d => d.muahang_chitiet.Count() > 0);
            }






            if (dutru_id != null && dutru_id != 0)
            {
                customerData = customerData.Where(d => d.dutru_id == dutru_id);
            }

            int recordsTotal = customerData.Count();
            if (type_id != null && type_id != 0)
            {
                customerData = customerData.Where(d => d.dutru.type_id == type_id);
            }
            if (department_id != null && department_id != 0)
            {
                customerData = customerData.Where(d => d.dutru.bophan_id == department_id);
            }
            if (mahh != null && mahh != "")
            {
                customerData = customerData.Where(d => d.mahh.Contains(mahh));
            }
            if (tenhh != null && tenhh != "")
            {
                customerData = customerData.Where(d => d.tenhh.Contains(tenhh));
            }
            if (filter_user_id != null && filter_user_id != "")
            {
                customerData = customerData.Where(d => d.user_id == filter_user_id);
            }
            if (filter_tags != null && filter_tags != "")
            {
                customerData = customerData.Where(d => d.tags.Contains(filter_tags));
            }
            if (tensp != null && tensp != "")
            {
                customerData = customerData.Where(d => d.tensp.Contains(tensp));
            }
            if (list_dutru != null && list_dutru != "")
            {
                var list_dt = list_dutru.Split(",").ToList();
                var listdutru = _context.DutruModel.Where(d => list_dt.Contains(d.code)).Select(d => d.id).ToList();
                customerData = customerData.Where(d => listdutru.Contains(d.dutru_id));
            }
            int recordsFiltered = customerData.Count();

            if (sort_id != null)
            {
                if (sort_id == "1")
                {
                    customerData = customerData.OrderBy(d => d.id);
                }
                else if (sort_id == "-1")
                {
                    customerData = customerData.OrderByDescending(d => d.id);
                }
            }
            else if (sort_mahh != null)
            {
                if (sort_mahh == "1")
                {
                    customerData = customerData.OrderBy(d => d.mahh);
                }
                else if (sort_mahh == "-1")
                {
                    customerData = customerData.OrderByDescending(d => d.mahh);
                }
            }
            else if (sort_tenhh != null)
            {
                if (sort_tenhh == "1")
                {
                    customerData = customerData.OrderBy(d => d.tenhh);
                }
                else if (sort_tenhh == "-1")
                {
                    customerData = customerData.OrderByDescending(d => d.tenhh);
                }
            }
            else if (sort_tensp != null)
            {
                if (sort_tensp == "1")
                {
                    customerData = customerData.OrderBy(d => d.tensp);
                }
                else if (sort_tensp == "-1")
                {
                    customerData = customerData.OrderByDescending(d => d.tensp);
                }
            }
            else if (sort_bophan != null)
            {
                if (sort_bophan == "1")
                {
                    customerData = customerData.OrderBy(d => d.dutru.bophan_id);
                }
                else if (sort_bophan == "-1")
                {
                    customerData = customerData.OrderByDescending(d => d.dutru.bophan_id);
                }
            }
            else if (sort_priority != null)
            {
                if (sort_priority == "1")
                {
                    customerData = customerData.OrderBy(d => d.dutru.priority_id);
                }
                else if (sort_priority == "-1")
                {
                    customerData = customerData.OrderByDescending(d => d.dutru.priority_id);
                }
            }
            else if (sort_dutru != null)
            {
                if (sort_dutru == "1")
                {
                    customerData = customerData.OrderBy(d => d.dutru.code);
                }
                else if (sort_dutru == "-1")
                {
                    customerData = customerData.OrderByDescending(d => d.dutru.code);
                }
            }
            else if (sort_ngayhethan != null)
            {
                if (sort_ngayhethan == "1")
                {
                    customerData = customerData.OrderBy(d => d.date).ThenBy(d => d.dutru.date);
                }
                else if (sort_ngayhethan == "-1")
                {
                    customerData = customerData.OrderByDescending(d => d.date).ThenByDescending(d => d.dutru.date);
                }
            }
            else
            {
                customerData = customerData.OrderByDescending(d => d.id);
            }
            var datapost = customerData
                .Include(d => d.user)
                .ToList();
            var data = new ArrayList();

            var viewPath = _configuration["Source:Path_Private"] + "\\buy\\templates\\Dutru.xlsx";
            var documentPath = "/tmp/Rawdata_" + DateTime.Now.ToFileTimeUtc() + ".xlsx";
            string Domain = (HttpContext.Request.IsHttps ? "https://" : "http://") + HttpContext.Request.Host.Value;
            Workbook workbook = new Workbook();
            workbook.LoadFromFile(viewPath);
            Worksheet sheet = workbook.Worksheets[0];
            int stt = 0;
            var start_r = 2;

            DataTable dt = new DataTable();
            //dt.Columns.Add("stt", typeof(int));
            dt.Columns.Add("code", typeof(string));
            dt.Columns.Add("name", typeof(string));
            dt.Columns.Add("id", typeof(string));
            dt.Columns.Add("mahh", typeof(string));
            dt.Columns.Add("tenhh", typeof(string));
            dt.Columns.Add("tensp", typeof(string));
            dt.Columns.Add("bophan", typeof(string));
            dt.Columns.Add("soluong", typeof(decimal));
            dt.Columns.Add("dvt", typeof(string));
            dt.Columns.Add("is_new", typeof(string));
            dt.Columns.Add("grade", typeof(string));
            dt.Columns.Add("dangbaoche", typeof(string));
            dt.Columns.Add("masothietke", typeof(string));
            dt.Columns.Add("mansx", typeof(string));
            dt.Columns.Add("tennsx", typeof(string));
            dt.Columns.Add("mota", typeof(string));
            dt.Columns.Add("priority", typeof(string));
            dt.Columns.Add("created_at", typeof(DateTime));
            dt.Columns.Add("created_by", typeof(string));
            dt.Columns.Add("date", typeof(DateTime));
            dt.Columns.Add("status", typeof(string));

            var stt_cell = 2;



            sheet.InsertRow(start_r, datapost.Count(), InsertOptionsType.FormatAsAfter);
            foreach (var record in datapost)
            {
                var dutru = record.dutru;
                var tenhh1 = record.tenhh;
                var mahh1 = record.mahh;
                //if (dutru.type_id == 1)
                //{
                //var nhasx = record.nhasx;
                //var nhacc = "";
                //var material = _context.MaterialModel.Where(d => record.mahh == d.mahh).Include(d => d.nhacungcap).FirstOrDefault();
                //if (material != null)
                //{
                //    nhacc = material.nhacungcap != null ? material.nhacungcap.tenncc : "";
                //}
                //}
                var list_muahang_ncc_id = record.muahang_chitiet.Select(d => d.muahang.muahang_chonmua_id).ToList();
                var list_muahang_chitiet_id = record.muahang_chitiet.Select(d => d.id).ToList();

                var thanhtien1 = _context.MuahangNccChitietModel.Include(d => d.muahang_ncc).ThenInclude(d => d.muahang)
                    .Where(d => d.muahang_ncc.muahang.deleted_at == null && d.muahang_ncc.muahang.status_id != (int)Status.MuahangEsignError && list_muahang_ncc_id.Contains(d.muahang_ncc_id) && list_muahang_chitiet_id.Contains(d.muahang_chitiet_id))
                    .ToList();
                var thanhtien = thanhtien1.Sum(d => d.thanhtien_vat);
                var muahang_chitiet = record.muahang_chitiet.Where(d => d.muahang.deleted_at == null && d.muahang.status_id != (int)Status.MuahangEsignError).ToList();
                var dulieu_muahang = "";
                foreach (var item3 in muahang_chitiet.Select(d => d.muahang).ToList())
                {
                    var status = "";
                    if (item3.date_finish != null)
                    {
                        status = "Hoàn thành";

                    }
                    else if (item3.is_dathang == true && ((item3.loaithanhtoan == "tra_sau" && item3.is_nhanhang == true) ||
                                    (item3.loaithanhtoan == "tra_truoc" && item3.is_thanhtoan == true)))
                    {
                        status = "Chờ nhận hàng";
                    }
                    else if (item3.is_dathang == true && ((item3.loaithanhtoan == "tra_sau" && item3.is_nhanhang == false) ||
                                    (item3.loaithanhtoan == "tra_truoc" && item3.is_thanhtoan == false)))
                    {
                        status = "Chờ thanh toán";
                    }
                    else if (item3.status_id == 1 || item3.status_id == 6 || item3.status_id == 7)
                    {
                        status = "Đang thực hiện";

                    }
                    else if (item3.status_id == 9)
                    {
                        status = "Đang trình ký";

                    }
                    else if (item3.status_id == 10)
                    {
                        status = "Đang đặt hàng";

                    }
                    else if (item3.status_id == 11)
                    {
                        status = "Không duyệt";

                    }
                    dulieu_muahang += $"{item3.id} - {item3.code} - {status}\n";
                }
                DataRow dr1 = dt.NewRow();
                //dr1["stt"] = (++stt);
                dr1["code"] = dutru.code;
                dr1["name"] = dutru.name;

                dr1["id"] = record.id;
                dr1["mahh"] = record.mahh;
                dr1["tenhh"] = record.tenhh;
                dr1["tensp"] = record.tensp;
                dr1["bophan"] = dutru.bophan.name;
                dr1["soluong"] = record.soluong;
                dr1["dvt"] = record.dvt;
                dr1["is_new"] = record.is_new == true ? "Unactive" : "Active";
                dr1["grade"] = record.grade;
                dr1["dangbaoche"] = record.dangbaoche;
                dr1["masothietke"] = record.masothietke;
                dr1["mansx"] = record.mansx;
                dr1["tennsx"] = record.nhasx;
                dr1["mota"] = record.note;
                dr1["priority"] = dutru.priority_id != null ? GetEnumDisplayName((Priority)dutru.priority_id) : "";
                dr1["created_at"] = dutru.created_at;
                dr1["created_by"] = dutru.user_created_by.FullName;
                dr1["date"] = record.date != null ? record.date : dutru.date;

                dr1["status"] = dulieu_muahang;
                dt.Rows.Add(dr1);
                start_r++;

            }
            sheet.InsertDataTable(dt, false, 2, 1);

            workbook.SaveToFile("./wwwroot" + documentPath, ExcelVersion.Version2013);

            return Json(new { success = true, link = Domain + documentPath });
        }
        [HttpPost]
        public async Task<IActionResult> AddComment(DutruCommentModel CommentModel, List<string> users_related)
        {
            System.Security.Claims.ClaimsPrincipal currentUser = User;
            string user_id = UserManager.GetUserId(currentUser); // Get user id:
            var user = await UserManager.GetUserAsync(currentUser); // Get user id:
            CommentModel.user_id = user_id;
            CommentModel.created_at = DateTime.Now;
            _context.Add(CommentModel);
            _context.SaveChanges();
            var files = Request.Form.Files;

            var items_comment = new List<DutruCommentFileModel>();
            if (files != null && files.Count > 0)
            {
                var pathroot = _configuration["Source:Path_Private"] + "\\buy\\dutru\\" + CommentModel.dutru_id + "\\";
                bool exists = Directory.Exists(pathroot);

                if (!exists)
                    Directory.CreateDirectory(pathroot);

                foreach (var file in files)
                {
                    var timeStamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
                    string name = file.FileName;
                    string ext = Path.GetExtension(name);
                    string mimeType = file.ContentType;

                    //var fileName = Path.GetFileName(name);
                    var newName = timeStamp + " - " + name;

                    newName = newName.Replace("+", "_");
                    newName = newName.Replace("%", "_");
                    newName = newName.Replace(",", "_");
                    var filePath = _configuration["Source:Path_Private"] + "\\buy\\dutru\\" + CommentModel.dutru_id + "\\" + newName;
                    string url = "/private/buy/dutru/" + CommentModel.dutru_id + "/" + newName;
                    items_comment.Add(new DutruCommentFileModel
                    {
                        ext = ext,
                        url = url,
                        name = name,
                        mimeType = mimeType,
                        comment_id = CommentModel.id,
                        created_at = DateTime.Now
                    });

                    using (var fileSrteam = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(fileSrteam);
                    }
                }
                _context.AddRange(items_comment);
                _context.SaveChanges();
            }
            foreach (var item in users_related)
            {
                _context.Add(new DutruCommentUserModel
                {
                    dutru_comment_id = CommentModel.id,
                    user_id = item
                });
                _context.SaveChanges();
            }
            ///lây user liên quan
            var dutru = _context.DutruModel.Where(d => d.id == CommentModel.dutru_id).FirstOrDefault();
            var comments = _context.DutruCommentModel.Where(d => d.dutru_id == CommentModel.dutru_id).Include(d => d.users_related).ToList();

            var users_related_id = new List<string>();
            users_related_id.Add(dutru.created_by);
            foreach (var activity in comments)
            {
                users_related_id.Add(activity.user_id);
                users_related_id.AddRange(activity.users_related.Select(d => d.user_id).ToList());
            }
            users_related_id = users_related_id.Distinct().ToList();
            var itemToRemove = users_related_id.SingleOrDefault(r => r == user_id);
            users_related_id.Remove(itemToRemove);
            //SEND MAIL
            if (users_related_id != null && users_related_id.Count() > 0)
            {
                var users_related_obj = _context.UserModel.Where(d => users_related_id.Contains(d.Id)).Select(d => d.Email).ToList();
                var mail_string = string.Join(",", users_related_obj.ToArray());
                string Domain = (HttpContext.Request.IsHttps ? "https://" : "http://") + HttpContext.Request.Host.Value;
                var attach = items_comment.Select(d => d.url).ToList();
                var text = CommentModel.comment;
                if (attach.Count() > 0 && CommentModel.comment == null)
                {
                    text = $"{user.FullName} gửi đính kèm";
                }
                var body = _view.Render("Emails/NewComment",
                    new
                    {
                        link_logo = Domain + "/images/clientlogo_astahealthcare.com_f1800.png",
                        link = Domain + "/dutru/edit/" + CommentModel.dutru_id,
                        text = text,
                        name = user.FullName
                    });

                var email = new EmailModel
                {
                    email_to = mail_string,
                    subject = "[Tin nhắn mới] " + dutru.name,
                    body = body,
                    email_type = "new_comment_purchase",
                    status = 1,
                    data_attachments = attach
                };
                _context.Add(email);
            }
            //await _context.SaveChangesAsync();

            /// Audittrail
            var audit = new AuditTrailsModel();
            audit.UserId = user.Id;
            audit.Type = AuditType.Update.ToString();
            audit.DateTime = DateTime.Now;
            audit.description = $"Tài khoản {user.FullName} đã thêm bình luận.";
            _context.Add(audit);
            await _context.SaveChangesAsync();

            CommentModel.user = user;
            CommentModel.is_read = true;

            return Json(new
            {
                success = 1,
                comment = CommentModel
            }, new System.Text.Json.JsonSerializerOptions()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
            });
        }

        public async Task<IActionResult> MoreComment(int id, int? from_id)
        {
            int limit = 10;
            var comments_ctx = _context.DutruCommentModel
                .Where(d => d.dutru_id == id);
            if (from_id != null)
            {
                comments_ctx = comments_ctx.Where(d => d.id < from_id);
            }
            List<DutruCommentModel> comments = comments_ctx.OrderByDescending(d => d.id)
                .Take(limit).Include(d => d.files).Include(d => d.user).ToList();
            //System.Security.Claims.ClaimsPrincipal currentUser = User;
            //string current_user_id = UserManager.GetUserId(currentUser); // Get user id:


            return Json(new { success = 1, comments }, new System.Text.Json.JsonSerializerOptions()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
            });
        }
        [HttpPost]
        public async Task<IActionResult> thongbaodoima(int dutru_chitiet_id, string mahh)
        {
            System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            var user_id = UserManager.GetUserId(currentUser);
            var user_doima = await UserManager.GetUserAsync(currentUser);

            var data = _context.DutruChitietModel.Where(d => d.id == dutru_chitiet_id).Include(d => d.dutru).ThenInclude(d => d.user_created_by).FirstOrDefault();
            var hh = _context.MaterialModel.Where(d => d.mahh == mahh).FirstOrDefault();
            //SEND MAIL
            if (hh != null && data != null)
            {
                var from_mahh = data.mahh;
                var user = data.dutru.user_created_by;
                var mail_string = user.Email;
                string Domain = (HttpContext.Request.IsHttps ? "https://" : "http://") + HttpContext.Request.Host.Value;
                var body = _view.Render("Emails/Doima",
                    new
                    {
                        link_logo = Domain + "/images/clientlogo_astahealthcare.com_f1800.png",
                        link = Domain + "/dutru/edit/" + data.dutru.id,
                        from_mahh = from_mahh,
                        user_doima = user_doima,
                        hh = hh
                    });

                var email = new EmailModel
                {
                    email_to = mail_string,
                    subject = "[Thông báo đổi mã] " + data.dutru.name,
                    body = body,
                    email_type = "doima_purchase",
                    status = 1,
                };
                _context.Add(email);
                _context.SaveChanges();
            }
            return Json(new
            {
                success = true,
            });
        }
        [HttpPost]
        public async Task<IActionResult> savedoima(int dutru_chitiet_id, string mahh)
        {
            System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            var user_id = UserManager.GetUserId(currentUser);
            var user_doima = await UserManager.GetUserAsync(currentUser);

            var data = _context.DutruChitietModel.Where(d => d.id == dutru_chitiet_id)
                .Include(d => d.dutru).ThenInclude(d => d.user_created_by)
                .Include(d => d.muahang_chitiet).ThenInclude(d => d.muahang_ncc_chitiet)
                .FirstOrDefault();
            var hh = _context.MaterialModel.Where(d => d.mahh == mahh).FirstOrDefault();

            if (hh != null && data != null)
            {
                //data.hh_id = hh_id;
                data.mahh = hh.mahh;

                _context.Update(data);
                _context.SaveChanges();
                foreach (var d in data.muahang_chitiet)
                {

                    //d.hh_id = hh_id;
                    d.mahh = hh.mahh;

                    _context.Update(d);
                    _context.SaveChanges();

                    foreach (var c in d.muahang_ncc_chitiet)
                    {
                        //c.hh_id = hh_id;
                        c.mahh = hh.mahh;
                        _context.Update(c);
                        _context.SaveChanges();
                    }
                }
            }
            return Json(new
            {
                success = true,
            });
        }
        private static string GetEnumDisplayName(Priority status)
        {
            // Lấy thông tin thuộc tính của enum
            var enumType = status.GetType();
            var enumValue = enumType.GetMember(status.ToString()).FirstOrDefault();

            if (enumValue != null)
            {
                // Lấy attribute Display
                var displayAttribute = enumValue.GetCustomAttribute<DisplayAttribute>();
                if (displayAttribute != null)
                {
                    return displayAttribute.Name; // Trả về giá trị của Display(Name)
                }
            }
            // Nếu không có Display(Name) thì trả về tên của enum
            return status.ToString();
        }
        private void CopyValues<T>(T target, T source)
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
    public class RawDetails
    {
        public int stt { get; set; }

        public string tennvl { get; set; }
        public string? manvl { get; set; }
        public string? artwork { get; set; }
        public string? dvt { get; set; }
        public string soluong { get; set; }
        public string? ngaysx { get; set; }
        public string? date { get; set; }
        public string? note { get; set; }
        public string? grade { get; set; }
        public string? nhasx { get; set; }
        public string? tensp { get; set; }
        public string? dangbaoche { get; set; }
    }

    public class RawFileDutru
    {
        public string link { get; set; }
        public string note { get; set; }
        public List<DutruDinhkemModel>? list_file { get; set; }
        //public string file_url { get; set; }

        public bool is_user_upload { get; set; }

        public DateTime? created_at { get; set; }

    }
}
