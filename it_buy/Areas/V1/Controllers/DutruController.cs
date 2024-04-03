

using iText.Barcodes.Dmcode;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NuGet.Packaging.Signing;
using Org.BouncyCastle.Utilities;
using Spire.Xls;
using System;
using System.Collections;
using System.Data;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Reflection;
using System.Security.Policy;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Vue.Data;
using Vue.Models;
using QRCoder;
using static QRCoder.PayloadGenerator;
using NuGet.Packaging;
using Vue.Services;
using Microsoft.CodeAnalysis;

namespace it_template.Areas.V1.Controllers
{

    public class DutruController : BaseController
    {
        private readonly IConfiguration _configuration;
        private UserManager<UserModel> UserManager;
        private readonly ViewRender _view;
        public DutruController(ItContext context, IConfiguration configuration, UserManager<UserModel> UserMgr, ViewRender view) : base(context)
        {
            _configuration = configuration;
            UserManager = UserMgr;
            _view = view;
        }
        public JsonResult Get(int id)
        {
            var data = _context.DutruModel.Where(d => d.id == id).Include(d => d.chitiet).Include(d => d.user_created_by).FirstOrDefault();
            if (data != null)
            {
                var stt = 1;
                foreach (var item in data.chitiet)
                {
                    item.stt = stt++;
                }
            }
            return Json(data);
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
            return Json(data);
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

            return Json(list);
        }
        public JsonResult GetMuahang(int id)
        {
            var list_items = _context.DutruChitietModel.Where(d => d.dutru_id == id).Select(d => d.id).ToList();
            var data = _context.MuahangChitietModel.Include(d => d.muahang).Where(d => list_items.Contains(d.dutru_chitiet_id) && d.muahang.deleted_at == null).ToList();

            var list = data.GroupBy(d => new { d.muahang }).Select(d => d.Key.muahang).ToList();

            return Json(list);
        }

        [HttpPost]
        public async Task<JsonResult> Delete(int id)
        {
            var Model = _context.DutruModel.Where(d => d.id == id).FirstOrDefault();
            Model.deleted_at = DateTime.Now;
            _context.Update(Model);
            _context.SaveChanges();
            return Json(Model);
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
            foreach (var item in dutru.chitiet)
            {
                var soluong_dutru = item.soluong;
                var muahang_chitiet = _context.MuahangChitietModel.Where(d => d.dutru_chitiet_id == item.id && d.status_nhanhang == 1).ToList();
                var soluong_mua = muahang_chitiet.Sum(d => d.soluong * d.quidoi);
                if (soluong_dutru == soluong_mua)
                {
                    soluong_ht++;
                }
            }
            if (soluong_ht == dutru.chitiet.Count())
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

                if (list_delete != null)
                    _context.RemoveRange(list_delete);
                if (list_add != null)
                {
                    foreach (var item in list_add)
                    {
                        item.dutru_id = DutruModel_old.id;
                        _context.Add(item);
                    }
                }
                if (list_update != null)
                {
                    foreach (var item in list_update)
                    {
                        _context.Update(item);
                    }
                }

                _context.SaveChanges();

                return Json(new { success = true, id = DutruModel_old.id });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.InnerException.Message });
            }

        }

        [HttpPost]
        public async Task<JsonResult> xuatpdf(int id)
        {
            var RawDetails = new List<RawDetails>();
            var data = _context.DutruModel.Where(d => d.id == id).Include(d => d.chitiet).Include(d => d.bophan).FirstOrDefault();
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
                    RawDetails.Add(new RawDetails
                    {
                        stt = stt++,
                        tennvl = item.tenhh,
                        manvl = item.mahh,
                        dvt = item.dvt,
                        soluong = item.soluong.Value.ToString("#,##0.##"),
                        note = item.note,
                        artwork = item.masothietke,
                        date = data.date.Value.ToString("yyyy-MM-dd"),
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
            var count_type = _context.DocumentModel.Where(d => d.type_id == DocumentModel.type_id).Count();
            var type = _context.DocumentTypeModel.Where(d => d.id == DocumentModel.type_id).Include(d => d.users_receive).FirstOrDefault();
            DocumentModel.code = type.symbol + "00" + (count_type + 1);
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

            _context.SaveChanges();

            return Json(new { success = true });
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
            var id_text = Request.Form["filters[id]"].FirstOrDefault();
            int id = id_text != null ? Convert.ToInt32(id_text) : 0;
            var type_id_string = Request.Form["type_id"].FirstOrDefault();
            int type_id = type_id_string != null ? Convert.ToInt32(type_id_string) : 0;
            //var tenhh = Request.Form["filters[tenhh]"].FirstOrDefault();
            int skip = start != null ? Convert.ToInt32(start) : 0;
            var customerData = _context.DutruModel.Where(d => d.deleted_at == null);

            if (type_id != null && type_id != 0)
            {
                customerData = customerData.Where(d => d.type_id == type_id);
            }
            else
            {
                customerData = customerData.Where(d => d.created_by == user_id);
            }



            int recordsTotal = customerData.Count();
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
        public async Task<JsonResult> TableChitiet()
        {
            System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            var user_id = UserManager.GetUserId(currentUser);
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            var type_id_string = Request.Form["type_id"].FirstOrDefault();
            int type_id = type_id_string != null ? Convert.ToInt32(type_id_string) : 0;
            var mahh = Request.Form["filters[mahh]"].FirstOrDefault();
            var tenhh = Request.Form["filters[tenhh]"].FirstOrDefault();
            var list_dutru = Request.Form["filters[list_dutru]"].FirstOrDefault();
            var orderById = Request.Form["orderBy[id]"].FirstOrDefault();

            var filterTable = Request.Form["filterTable"].FirstOrDefault();
            var dutru_id_string = Request.Form["dutru_id"].FirstOrDefault();
            int dutru_id = dutru_id_string != null ? Convert.ToInt32(dutru_id_string) : 0;

            var departments = _context.UserDepartmentModel.Where(d => d.user_id == user_id).Select(d => d.department_id).ToList();
            var is_CungungNVL = departments.Contains(29) == true;
            var is_CungungGiantiep = departments.Contains(14) == true;
            var is_CungungHCTT = departments.Contains(30) == true;


            //var tenhh = Request.Form["filters[tenhh]"].FirstOrDefault();
            int skip = start != null ? Convert.ToInt32(start) : 0;
            var customerData = _context.DutruChitietModel
                .Include(d => d.muahang_chitiet).ThenInclude(d => d.muahang)
                .Include(d => d.dutru).Where(d => d.dutru.status_id == (int)Status.EsignSuccess);

            //if (is_CungungNVL && is_CungungGiantiep)
            //{

            //}
            if (filterTable != null && filterTable == "Chưa xử lý")
            {
                customerData = customerData.Where(d => d.muahang_chitiet.Count() == 0);
            }
            else if (filterTable != null && filterTable == "Đã xử lý")
            {
                customerData = customerData.Where(d => d.muahang_chitiet.Count() > 0);
            }
            if (type_id != null && type_id != 0)
            {
                customerData = customerData.Where(d => d.dutru.type_id == type_id);
            }
            if (dutru_id != null && dutru_id != 0)
            {
                customerData = customerData.Where(d => d.dutru_id == dutru_id);
            }

            int recordsTotal = customerData.Count();
            if (mahh != null && mahh != "")
            {
                var listhh = _context.MaterialModel.Where(d => d.mahh.Contains(mahh)).Select(d => "m-" + d.id).ToList();
                customerData = customerData.Where(d => listhh.Contains(d.hh_id));
            }
            if (tenhh != null && tenhh != "")
            {
                var listhh = _context.MaterialModel.Where(d => d.tenhh.Contains(tenhh)).Select(d => "m-" + d.id).ToList();
                customerData = customerData.Where(d => listhh.Contains(d.hh_id));
            }
            if (list_dutru != null && list_dutru != "")
            {
                var list_dt = list_dutru.Split(",").ToList();
                var listdutru = _context.DutruModel.Where(d => list_dt.Contains(d.code)).Select(d => d.id).ToList();
                customerData = customerData.Where(d => listdutru.Contains(d.dutru_id));
            }
            int recordsFiltered = customerData.Count();

            if (orderById != null && orderById == "Asc")
            {
                customerData = customerData.OrderBy(d => d.id);
            }
            else
            {
                customerData = customerData.OrderByDescending(d => d.id);
            }
            var datapost = customerData.Skip(skip).Take(pageSize).ToList();
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
                var material = _context.MaterialModel.Where(d => record.hh_id == "m-" + d.id).Include(d => d.nhacungcap).FirstOrDefault();
                if (material != null)
                {
                    nhacc = material.nhacungcap != null ? material.nhacungcap.tenncc : "";
                }
                //}
                var list_muahang_ncc_id = record.muahang_chitiet.Select(d => d.muahang.muahang_chonmua_id).ToList();
                var list_muahang_chitiet_id = record.muahang_chitiet.Select(d => d.id).ToList();

                var thanhtien = _context.MuahangNccChitietModel.Where(d => list_muahang_ncc_id.Contains(d.muahang_ncc_id) && list_muahang_chitiet_id.Contains(d.muahang_chitiet_id)).Include(d => d.muahang_ncc).Sum(d => d.thanhtien + (d.thanhtien * d.muahang_ncc.vat / 100));
                var muahang_chitiet = record.muahang_chitiet.Where(d => d.muahang.status_id != 11).ToList();
                var soluong_dutru = record.soluong;
                var soluong_mua = muahang_chitiet.Sum(d => d.soluong * d.quidoi);

                var soluong = soluong_mua < soluong_dutru ? soluong_dutru - soluong_mua : 0;
                data.Add(new
                {
                    id = record.id,
                    hh_id = record.hh_id,
                    soluong_dutru = soluong_dutru,
                    soluong_mua = soluong_mua,
                    thanhtien = thanhtien,
                    nhacc = nhacc,
                    nhasx = nhasx,
                    mahh = mahh1,
                    tenhh = tenhh1,
                    grade = record.grade,
                    masothietke = record.masothietke,
                    dvt = record.dvt,
                    soluong = soluong,
                    dutru_chitiet_id = record.id,
                    list_dutru = new List<DutruModel>() { dutru },
                    list_muahang = muahang_chitiet.Select(d => d.muahang).ToList(),
                });
            }

            var jsonData = new { draw = draw, recordsFiltered = recordsFiltered, recordsTotal = recordsTotal, data = data };
            return Json(jsonData);
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


            return Json(new { success = 1, comments });
        }
        [HttpPost]
        public async Task<IActionResult> thongbaodoima(int dutru_chitiet_id, string hh_id)
        {
            System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            var user_id = UserManager.GetUserId(currentUser);
            var user_doima = await UserManager.GetUserAsync(currentUser);

            var data = _context.DutruChitietModel.Where(d => d.id == dutru_chitiet_id).Include(d => d.dutru).ThenInclude(d => d.user_created_by).FirstOrDefault();
            var hh = _context.MaterialModel.Where(d => "m-" + d.id == hh_id).FirstOrDefault();
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
        public async Task<IActionResult> savedoima(int dutru_chitiet_id, string hh_id)
        {
            System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            var user_id = UserManager.GetUserId(currentUser);
            var user_doima = await UserManager.GetUserAsync(currentUser);

            var data = _context.DutruChitietModel.Where(d => d.id == dutru_chitiet_id)
                .Include(d => d.dutru).ThenInclude(d => d.user_created_by)
                .Include(d => d.muahang_chitiet).ThenInclude(d => d.muahang_ncc_chitiet)
                .FirstOrDefault();
            var hh = _context.MaterialModel.Where(d => "m-" + d.id == hh_id).FirstOrDefault();

            if (hh != null && data != null)
            {
                data.hh_id = hh_id;
                data.mahh = hh.mahh;

                _context.Update(data);
                _context.SaveChanges();
                foreach (var d in data.muahang_chitiet)
                {

                    d.hh_id = hh_id;
                    d.mahh = hh.mahh;

                    _context.Update(d);
                    _context.SaveChanges();

                    foreach (var c in d.muahang_ncc_chitiet)
                    {
                        c.hh_id = hh_id;
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
