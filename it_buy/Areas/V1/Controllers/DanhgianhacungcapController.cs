

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
using Microsoft.CodeAnalysis.Differencing;
using static it_template.Areas.V1.Controllers.MuahangController;
using iText.StyledXmlParser.Jsoup.Nodes;

namespace it_template.Areas.V1.Controllers
{

    public class DanhgianhacungcapController : BaseController
    {
        private readonly IConfiguration _configuration;
        private UserManager<UserModel> UserManager;
        private readonly ViewRender _view;
        public DanhgianhacungcapController(ItContext context, IConfiguration configuration, UserManager<UserModel> UserMgr, ViewRender view) : base(context)
        {
            _configuration = configuration;
            UserManager = UserMgr;
            _view = view;
        }
        public JsonResult Get(int id)
        {
            var data = _context.DanhgianhacungcapModel
                .Where(d => d.id == id)
                .Include(d => d.user_created_by)
                .Include(d => d.user_chapnhan)
                .Include(d => d.DutruChitietModels)
                .FirstOrDefault();

            return Json(data, new System.Text.Json.JsonSerializerOptions()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
            });
        }
        public JsonResult GetFiles(int id)
        {
            var data = new List<RawFileDanhgianhacungcap>();
            ///File up
            var files_up = _context.DanhgianhacungcapDinhkemModel.Where(d => d.danhgianhacungcap_id == id && d.deleted_at == null).Include(d => d.user_created_by).ToList();
            if (files_up.Count > 0)
            {
                data.AddRange(files_up.GroupBy(d => new { d.note, d.created_at }).Select(d => new RawFileDanhgianhacungcap
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
        public JsonResult GetDanhgia(int id)
        {
            var data = _context.DanhgianhacungcapDanhgiaModel
                .Where(d => d.danhgianhacungcap_id == id)
                .Include(d => d.user)
                .OrderBy(d => d.id).ToList();
            return Json(data, new System.Text.Json.JsonSerializerOptions()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
            });
        }

        [HttpPost]
        public async Task<JsonResult> xoadinhkem(List<int> list_id)
        {
            var Model = _context.DanhgianhacungcapDinhkemModel.Where(d => list_id.Contains(d.id)).ToList();
            foreach (var item in Model)
            {
                item.deleted_at = DateTime.Now;
            }
            _context.UpdateRange(Model);
            _context.SaveChanges();
            return Json(new { success = true });
        }
        [HttpPost]
        public async Task<JsonResult> Delete(int id)
        {
            var Model = _context.DanhgianhacungcapModel.Where(d => d.id == id).FirstOrDefault();
            Model.deleted_at = DateTime.Now;
            _context.Update(Model);

            var DutruChitietModel = _context.DutruChitietModel.Where(d => d.danhgianhacungcap_id == id).ToList();
            foreach (var item in DutruChitietModel)
            {
                item.danhgianhacungcap_id = null;

            }
            _context.UpdateRange(DutruChitietModel);

            _context.SaveChanges();
            return Json(Model, new System.Text.Json.JsonSerializerOptions()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
            });
        }
        [HttpPost]

        public async Task<JsonResult> SaveDinhkem(string note, int danhgianhacungcap_id)
        {
            System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            var user_id = UserManager.GetUserId(currentUser);
            var user = await UserManager.GetUserAsync(currentUser);
            //MuahangModel? MuahangModel_old;
            //MuahangModel_old = _context.MuahangModel.Where(d => d.id == muahang_id).FirstOrDefault();

            var files = Request.Form.Files;
            var now = DateTime.Now;
            var items = new List<DanhgianhacungcapDinhkemModel>();
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
                    var newName = timeStamp + "-" + danhgianhacungcap_id + "-" + name;
                    //var muahang_id = MuahangModel_old.id;
                    newName = newName.Replace("+", "_");
                    newName = newName.Replace("%", "_");
                    var dir = _configuration["Source:Path_Private"] + "\\buy\\danhgianhacungcap\\" + danhgianhacungcap_id;
                    bool exists = Directory.Exists(dir);

                    if (!exists)
                        Directory.CreateDirectory(dir);


                    var filePath = dir + "\\" + newName;

                    string url = "/private/buy/danhgianhacungcap/" + danhgianhacungcap_id + "/" + newName;
                    items.Add(new DanhgianhacungcapDinhkemModel
                    {
                        note = note,
                        ext = ext,
                        url = url,
                        name = name,
                        mimeType = mimeType,
                        danhgianhacungcap_id = danhgianhacungcap_id,
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
        public async Task<JsonResult> Save(DanhgianhacungcapModel DanhgianhacungcapModel, List<DanhgianhacungcapDanhgiaModel> list_add, List<DanhgianhacungcapDanhgiaModel>? list_update, List<DanhgianhacungcapDanhgiaModel>? list_delete, List<int> list_dutru_chitiet)
        {
            try
            {
                System.Security.Claims.ClaimsPrincipal currentUser = this.User;
                var user_id = UserManager.GetUserId(currentUser);
                var user = await UserManager.GetUserAsync(currentUser);
                DanhgianhacungcapModel? DanhgianhacungcapModel_old;
                //if (DanhgianhacungcapModel.date != null && DanhgianhacungcapModel.date.Value.Kind == DateTimeKind.Utc)
                //{
                //    DanhgianhacungcapModel.date = DanhgianhacungcapModel.date.Value.ToLocalTime();
                //}
                if (DanhgianhacungcapModel.id == 0)
                {
                    DanhgianhacungcapModel.created_at = DateTime.Now;
                    DanhgianhacungcapModel.created_by = user_id;

                    _context.DanhgianhacungcapModel.Add(DanhgianhacungcapModel);

                    _context.SaveChanges();

                    DanhgianhacungcapModel_old = DanhgianhacungcapModel;

                }
                else
                {

                    DanhgianhacungcapModel_old = _context.DanhgianhacungcapModel.Where(d => d.id == DanhgianhacungcapModel.id).FirstOrDefault();
                    CopyValues<DanhgianhacungcapModel>(DanhgianhacungcapModel_old, DanhgianhacungcapModel);
                    DanhgianhacungcapModel_old.updated_at = DateTime.Now;

                    _context.Update(DanhgianhacungcapModel_old);
                    _context.SaveChanges();
                }

                //////

                if (list_delete != null)
                    _context.RemoveRange(list_delete);
                if (list_add != null)
                {
                    foreach (var item in list_add)
                    {
                        item.danhgianhacungcap_id = DanhgianhacungcapModel_old.id;
                        _context.Add(item);
                        //_context.SaveChanges();
                    }
                }
                if (list_update != null)
                {
                    foreach (var item in list_update)
                    {
                        item.user = null;
                        _context.Update(item);
                    }
                }

                _context.SaveChanges();

                /////
                var list_dutru_chitiet_old = _context.DutruChitietModel.Where(d => d.danhgianhacungcap_id == DanhgianhacungcapModel_old.id).Select(d => d.id).ToList();
                IEnumerable<int> list_delete_chitiet = list_dutru_chitiet_old.Except(list_dutru_chitiet);
                IEnumerable<int> list_add_chitiet = list_dutru_chitiet.Except(list_dutru_chitiet_old);

                if (list_add != null)
                {
                    foreach (int key in list_add_chitiet)
                    {

                        var DutruChitietModel = _context.DutruChitietModel.Where(d => d.id == key).FirstOrDefault();
                        DutruChitietModel.danhgianhacungcap_id = DanhgianhacungcapModel_old.id;
                        _context.Update(DutruChitietModel);
                    }
                    //await _context.SaveChangesAsync();
                }
                if (list_delete != null)
                {
                    foreach (int key in list_delete_chitiet)
                    {
                        var DutruChitietModel = _context.DutruChitietModel.Where(d => d.id == key).FirstOrDefault();
                        DutruChitietModel.danhgianhacungcap_id = null;
                        _context.Update(DutruChitietModel);
                    }
                    //await _context.SaveChangesAsync();
                }
                _context.SaveChanges();

                return Json(new { success = true, id = DanhgianhacungcapModel_old.id });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.InnerException.Message });
            }

        }
        [HttpPost]
        public async Task<JsonResult> chapnhan(int id)
        {

            System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            var user_id = UserManager.GetUserId(currentUser);
            var user = await UserManager.GetUserAsync(currentUser);

            var DanhgianhacungcapModel_old = _context.DanhgianhacungcapModel.Where(d => d.id == id).FirstOrDefault();
            if (DanhgianhacungcapModel_old == null)
            {
                return Json(new { success = false });
            }
            DanhgianhacungcapModel_old.updated_at = DateTime.Now;
            DanhgianhacungcapModel_old.date_chapnhan = DateTime.Now;
            DanhgianhacungcapModel_old.is_chapnhan = true;
            DanhgianhacungcapModel_old.user_chapnhan_id = user_id;

            _context.Update(DanhgianhacungcapModel_old);
            _context.SaveChanges();



            ///lây user liên quan
            var danhgianhacungcap = _context.DanhgianhacungcapModel.Where(d => d.id == DanhgianhacungcapModel_old.id).FirstOrDefault();
            List<string> user_danhgia = _context.DanhgianhacungcapDanhgiaModel.Where(d => d.danhgianhacungcap_id == DanhgianhacungcapModel_old.id).Select(d => d.user_id.ToString()).ToList();
            var comments = _context.DanhgianhacungcapCommentModel.Where(d => d.danhgianhacungcap_id == DanhgianhacungcapModel_old.id)
                .Include(d => d.users_related).ToList();

            var users_related_id = new List<string>();
            users_related_id.Add(danhgianhacungcap.created_by);
            foreach (var comment in comments)
            {
                users_related_id.Add(comment.user_id);
                users_related_id.AddRange(comment.users_related.Select(d => d.user_id).ToList());
            }
            users_related_id.AddRange(user_danhgia);
            users_related_id = users_related_id.Distinct().ToList();
            var itemToRemove = users_related_id.SingleOrDefault(r => r == user_id);
            users_related_id.Remove(itemToRemove);
            //SEND MAIL
            if (users_related_id != null && users_related_id.Count() > 0)
            {
                var users_related_obj = _context.UserModel.Where(d => users_related_id.Contains(d.Id)).Select(d => d.Email).ToList();
                var mail_string = string.Join(",", users_related_obj.ToArray());
                string Domain = (HttpContext.Request.IsHttps ? "https://" : "http://") + HttpContext.Request.Host.Value;
                //var text = CommentModel.comment;
                var body = _view.Render("Emails/Accept",
                    new
                    {
                        link_logo = Domain + "/images/clientlogo_astahealthcare.com_f1800.png",
                        link = Domain + "/danhgianhacungcap/details/" + DanhgianhacungcapModel_old.id,
                        note = "",
                        user = user.FullName
                    });

                var email = new EmailModel
                {
                    email_to = mail_string,
                    subject = "[Chấp nhận] Đánh giá " + danhgianhacungcap.nhacc + " cho nguyên liệu " + danhgianhacungcap.tenhh,
                    body = body,
                    email_type = "dgncc_accept",
                    status = 1,
                };
                _context.Add(email);
            }
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }
        [HttpPost]
        public async Task<JsonResult> chapnhanDanhgia(int id, string note)
        {

            System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            var user_id = UserManager.GetUserId(currentUser);
            var user = await UserManager.GetUserAsync(currentUser);

            var DanhgianhacungcapDanhgiaModel_old = _context.DanhgianhacungcapDanhgiaModel.Where(d => d.id == id).FirstOrDefault();
            if (DanhgianhacungcapDanhgiaModel_old == null)
            {
                return Json(new { success = false });
            }
            DanhgianhacungcapDanhgiaModel_old.user_id = user_id;
            DanhgianhacungcapDanhgiaModel_old.date_accept = DateTime.Now;
            DanhgianhacungcapDanhgiaModel_old.note = note;

            _context.Update(DanhgianhacungcapDanhgiaModel_old);
            _context.SaveChanges();

            ///lây user liên quan
            var danhgianhacungcap = _context.DanhgianhacungcapModel.Where(d => d.id == DanhgianhacungcapDanhgiaModel_old.danhgianhacungcap_id).FirstOrDefault();
            List<string> user_danhgia = _context.DanhgianhacungcapDanhgiaModel.Where(d => d.danhgianhacungcap_id == DanhgianhacungcapDanhgiaModel_old.danhgianhacungcap_id).Select(d => d.user_id.ToString()).ToList();
            var comments = _context.DanhgianhacungcapCommentModel.Where(d => d.danhgianhacungcap_id == DanhgianhacungcapDanhgiaModel_old.danhgianhacungcap_id)
                .Include(d => d.users_related).ToList();

            var users_related_id = new List<string>();
            users_related_id.Add(danhgianhacungcap.created_by);
            foreach (var c in comments)
            {
                users_related_id.Add(c.user_id);
                users_related_id.AddRange(c.users_related.Select(d => d.user_id).ToList());
            }
            users_related_id.AddRange(user_danhgia);

            ///CHECK ALL ĐÃ CHÂP NHẬN
            var count_not_done = _context.DanhgianhacungcapDanhgiaModel.Where(d => d.danhgianhacungcap_id == DanhgianhacungcapDanhgiaModel_old.danhgianhacungcap_id && d.date_accept == null).Count();
            if (count_not_done == 0)
            {
                users_related_id.Add(danhgianhacungcap.user_chapnhan_id);
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
                //var text = CommentModel.comment;
                var body = _view.Render("Emails/Accept",
                    new
                    {
                        link_logo = Domain + "/images/clientlogo_astahealthcare.com_f1800.png",
                        link = Domain + "/danhgianhacungcap/details/" + DanhgianhacungcapDanhgiaModel_old.danhgianhacungcap_id,
                        note = note,
                        user = user.FullName
                    });

                var email = new EmailModel
                {
                    email_to = mail_string,
                    subject = "[Chấp nhận] Đánh giá " + danhgianhacungcap.nhacc + " cho nguyên liệu " + danhgianhacungcap.tenhh,
                    body = body,
                    email_type = "dgncc_accept",
                    status = 1,
                };
                _context.Add(email);
            }
            await _context.SaveChangesAsync();




            return Json(new { success = true });
        }
        [HttpPost]
        public async Task<JsonResult> saveDanhgia(int id, string note, string user_id, string bophan)
        {


            var DanhgianhacungcapDanhgiaModel_old = _context.DanhgianhacungcapDanhgiaModel.Where(d => d.id == id).FirstOrDefault();
            if (DanhgianhacungcapDanhgiaModel_old == null)
            {
                return Json(new { success = false });
            }
            DanhgianhacungcapDanhgiaModel_old.note = note;
            DanhgianhacungcapDanhgiaModel_old.user_id = user_id;
            DanhgianhacungcapDanhgiaModel_old.bophan = bophan;

            _context.Update(DanhgianhacungcapDanhgiaModel_old);
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
            var id_text = Request.Form["filters[id]"].FirstOrDefault();
            var nhacc = Request.Form["filters[nhacc]"].FirstOrDefault();
            var nhasx = Request.Form["filters[nhasx]"].FirstOrDefault();
            var tenhh = Request.Form["filters[tenhh]"].FirstOrDefault();
            int id = id_text != null ? Convert.ToInt32(id_text) : 0;
            //var tenhh = Request.Form["filters[tenhh]"].FirstOrDefault();
            int skip = start != null ? Convert.ToInt32(start) : 0;
            var customerData = _context.DanhgianhacungcapModel.Where(d => d.deleted_at == null);

            //customerData = customerData.Where(d => d.created_by == user_id);


            int recordsTotal = customerData.Count();
            if (nhacc != null && nhacc != "")
            {
                customerData = customerData.Where(d => d.nhacc.Contains(nhacc));
            }
            if (nhasx != null && nhasx != "")
            {
                customerData = customerData.Where(d => d.nhasx.Contains(nhasx));
            }
            if (tenhh != null && tenhh != "")
            {
                customerData = customerData.Where(d => d.tenhh.Contains(tenhh));
            }
            if (id != 0)
            {
                customerData = customerData.Where(d => d.id == id);
            }
            int recordsFiltered = customerData.Count();
            var datapost = customerData.OrderByDescending(d => d.id).Skip(skip).Take(pageSize)
                .Include(d => d.user_created_by)
                .Include(d => d.DutruChitietModels).ThenInclude(d => d.dutru).ThenInclude(d => d.bophan)
                .ToList();
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
        public async Task<IActionResult> AddComment(DanhgianhacungcapCommentModel CommentModel, List<string> users_related)
        {
            System.Security.Claims.ClaimsPrincipal currentUser = User;
            string user_id = UserManager.GetUserId(currentUser); // Get user id:
            var user = await UserManager.GetUserAsync(currentUser); // Get user id:
            CommentModel.user_id = user_id;
            CommentModel.created_at = DateTime.Now;
            _context.Add(CommentModel);
            _context.SaveChanges();
            var files = Request.Form.Files;

            var items_comment = new List<DanhgianhacungcapCommentFileModel>();
            if (files != null && files.Count > 0)
            {
                var pathroot = _configuration["Source:Path_Private"] + "\\buy\\danhgianhacungcap\\" + CommentModel.danhgianhacungcap_id + "\\";
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
                    var filePath = _configuration["Source:Path_Private"] + "\\buy\\danhgianhacungcap\\" + CommentModel.danhgianhacungcap_id + "\\" + newName;
                    string url = "/private/buy/danhgianhacungcap/" + CommentModel.danhgianhacungcap_id + "/" + newName;
                    items_comment.Add(new DanhgianhacungcapCommentFileModel
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
                _context.Add(new DanhgianhacungcapCommentUserModel
                {
                    danhgianhacungcap_comment_id = CommentModel.id,
                    user_id = item
                });
                _context.SaveChanges();
            }
            ///lây user liên quan
            var danhgianhacungcap = _context.DanhgianhacungcapModel.Where(d => d.id == CommentModel.danhgianhacungcap_id).FirstOrDefault();
            var comments = _context.DanhgianhacungcapCommentModel.Where(d => d.danhgianhacungcap_id == CommentModel.danhgianhacungcap_id).Include(d => d.users_related).ToList();
            List<string> user_danhgia = _context.DanhgianhacungcapDanhgiaModel.Where(d => d.danhgianhacungcap_id == CommentModel.danhgianhacungcap_id).Select(d => d.user_id.ToString()).ToList();

            var users_related_id = new List<string>();
            users_related_id.Add(danhgianhacungcap.created_by);
            users_related_id.AddRange(user_danhgia);
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
                        link = Domain + "/danhgianhacungcap/details/" + CommentModel.danhgianhacungcap_id,
                        text = text,
                        name = user.FullName
                    });

                var email = new EmailModel
                {
                    email_to = mail_string,
                    subject = "[Tin nhắn mới] Đánh giá " + danhgianhacungcap.nhacc + " cho nguyên liệu " + danhgianhacungcap.tenhh,
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
            var comments_ctx = _context.DanhgianhacungcapCommentModel
                .Where(d => d.danhgianhacungcap_id == id);
            if (from_id != null)
            {
                comments_ctx = comments_ctx.Where(d => d.id < from_id);
            }
            List<DanhgianhacungcapCommentModel> comments = comments_ctx.OrderByDescending(d => d.id)
                .Take(limit).Include(d => d.files).Include(d => d.user).ToList();
            //System.Security.Claims.ClaimsPrincipal currentUser = User;
            //string current_user_id = UserManager.GetUserId(currentUser); // Get user id:


            return Json(new
            {
                success = 1,
                comments,

            }, new System.Text.Json.JsonSerializerOptions()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
            });
        }
        [HttpPost]
        public async Task<IActionResult> thongbao(int danhgianhacungcap_id, List<string> list_user)
        {

            var data = _context.DanhgianhacungcapModel.Where(d => d.id == danhgianhacungcap_id).FirstOrDefault();
            //SEND MAIL
            if (list_user != null && list_user.Count() > 0 && data != null)
            {
                var users_related_obj = _context.UserModel.Where(d => list_user.Contains(d.Id)).Select(d => d.Email).ToList();
                var mail_string = string.Join(",", users_related_obj.ToArray());
                string Domain = (HttpContext.Request.IsHttps ? "https://" : "http://") + HttpContext.Request.Host.Value;
                var body = _view.Render("Emails/Danhgianhacungcap",
                    new
                    {
                        link_logo = Domain + "/images/clientlogo_astahealthcare.com_f1800.png",
                        link = Domain + "/danhgianhacungcap/details/" + danhgianhacungcap_id,
                        data = data,
                    });
                var attach = _context.DanhgianhacungcapDinhkemModel.Where(d => d.danhgianhacungcap_id == danhgianhacungcap_id).Select(d => d.url).ToList();
                var email = new EmailModel
                {
                    email_to = mail_string,
                    subject = "[Đánh giá nhà cung cấp] " + data.tenhh,
                    body = body,
                    email_type = "danhgianhacungcap_purchase",
                    status = 1,
                    data_attachments = attach
                };
                _context.Add(email);

                data.is_thongbao = true;
                _context.Update(data);
                _context.SaveChanges();
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
            ProcessStartInfo procStartInfo = new ProcessStartInfo(libreOfficePath, string.Format("--convert-to pdf --nologo " + file + " --outdir " + outputDirectory));
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

    public class RawFileDanhgianhacungcap
    {
        public string link { get; set; }
        public string note { get; set; }
        public List<DanhgianhacungcapDinhkemModel>? list_file { get; set; }
        //public string file_url { get; set; }

        public bool is_user_upload { get; set; }

        public DateTime? created_at { get; set; }

    }
}
