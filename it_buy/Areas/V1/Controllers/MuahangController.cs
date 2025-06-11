

using iText.Commons.Actions.Contexts;
using iText.Layout.Element;
using iText.StyledXmlParser.Jsoup.Nodes;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using NuGet.Packaging;
using QRCoder;
using Spire.Doc.Reporting;
using Spire.Xls;
using System;
using System.Collections;
using System.Data;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Globalization;
using System.Linq.Expressions;
using System.Net.WebSockets;
using System.Reflection;
using System.Security.Policy;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Vue.Data;
using Vue.Models;
using Vue.Services;

namespace it_template.Areas.V1.Controllers
{

    public class MuahangController : BaseController
    {
        private readonly IConfiguration _configuration;
        private UserManager<UserModel> UserManager;
        private readonly ViewRender _view;
        public MuahangController(ItContext context, IConfiguration configuration, UserManager<UserModel> UserMgr, ViewRender view) : base(context)
        {
            _configuration = configuration;
            UserManager = UserMgr;
            _view = view;
        }
        public JsonResult GetFiles(int id)
        {
            var data = new List<RawFile>();
            ///Lấy dự trù
            var dutru = _context.MuahangChitietModel.Where(d => d.muahang_id == id).Include(d => d.dutru_chitiet).ThenInclude(d => d.dutru).ThenInclude(d => d.user_created_by).Select(d => d.dutru_chitiet.dutru).ToList();
            dutru = dutru.Distinct().ToList();
            foreach (var d in dutru)
            {
                var file = new RawFile()
                {
                    note = "Dự trù mã số " + d.code,
                    link = "/dutru/edit/" + d.id,
                    list_file = new List<MuahangDinhkemModel>()
                    {
                        new MuahangDinhkemModel()
                        {
                            name = d.name,
                            ext = ".pdf",
                            url = d.pdf,
                            created_by = d.created_by,
                            user_created_by = d.user_created_by,
                        }
                    },
                    is_user_upload = false,
                    created_at = d.created_at
                };
                data.Add(file);
            }
            ///Lấy Dinhkem hang hoa
            var list_hh = _context.MuahangChitietModel.Where(d => d.muahang_id == id).ToList();
            foreach (var hh in list_hh)
            {
                var files_up1 = _context.MaterialDinhkemModel.Where(d => d.mahh == hh.mahh && d.deleted_at == null).Include(d => d.user_created_by).ToList();
                if (files_up1.Count > 0)
                {
                    data.AddRange(files_up1.GroupBy(d => new { d.note, d.created_at }).Select(d => new RawFile
                    {
                        note = d.First().note + "-" + hh.tenhh,
                        list_file = d.Select(e => new MuahangDinhkemModel()
                        {
                            name = e.name,
                            ext = e.ext,
                            url = e.url,
                            created_by = e.created_by,
                            user_created_by = e.user_created_by
                        }).ToList(),
                        is_user_upload = false,
                        created_at = d.Key.created_at
                    }).ToList());
                }

            }
            ///Lấy báo giá
            var baogia = _context.MuahangNccModel.Where(d => d.muahang_id == id).Include(d => d.dinhkem).ThenInclude(d => d.user_created_by).Include(d => d.ncc).ToList();
            foreach (var d in baogia)
            {
                var list_file = new List<MuahangDinhkemModel>();
                DateTime? created_at = null;
                foreach (var item in d.dinhkem)
                {
                    created_at = item.created_at;
                    list_file.Add(new MuahangDinhkemModel()
                    {
                        name = item.name,
                        ext = item.ext,
                        url = item.url,
                        created_by = item.created_by,
                        user_created_by = item.user_created_by
                    });
                }
                if (created_at != null)
                {
                    var file = new RawFile()
                    {
                        note = "Báo giá của " + d.ncc?.tenncc,
                        list_file = list_file,
                        is_user_upload = false,
                        created_at = created_at
                    };
                    data.Add(file);

                }
            }
            ///Lấy đề nghị mua hàng
            var muahang = _context.MuahangModel.Where(d => d.id == id).Include(d => d.user_created_by).FirstOrDefault();
            data.Add(new RawFile()
            {
                note = "Đề nghị mua hàng mã số " + muahang.code,
                link = "/muahang/edit/" + muahang.id,
                list_file = new List<MuahangDinhkemModel>()
                    {
                        new MuahangDinhkemModel()
                        {
                            name = muahang.name,
                            ext = ".pdf",
                            url = muahang.pdf,
                            created_by = muahang.created_by,
                            user_created_by = muahang.user_created_by
                        }
                    },
                is_user_upload = false,
                created_at = muahang.created_at
            });
            ///Lấy ủy nhiệm chi
            var uynhiemchi = _context.MuahangUynhiemchiModel.Where(d => d.muahang_id == id).ToList();
            if (uynhiemchi.Count() > 0)
            {
                data.Add(new RawFile()
                {
                    note = "Ủy nhiệm chi",
                    list_file = uynhiemchi.Select(d => new MuahangDinhkemModel()
                    {
                        name = d.name,
                        ext = d.ext,
                        url = d.url,
                        created_by = d.created_by,
                        user_created_by = d.user_created_by
                    }).ToList(),
                    is_user_upload = false,
                    created_at = uynhiemchi.FirstOrDefault().created_at
                });
            }
            ///File up
            var files_up = _context.MuahangDinhkemModel.Where(d => d.muahang_id == id && d.deleted_at == null).Include(d => d.user_created_by).ToList();
            if (files_up.Count > 0)
            {
                data.AddRange(files_up.GroupBy(d => new { d.note, d.created_at }).Select(d => new RawFile
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
        public JsonResult Get(int id)
        {
            var data = _context.MuahangModel.Where(d => d.id == id)
                .Include(d => d.user_created_by)
                .Include(d => d.uynhiemchi)
                .Include(d => d.muahang_chonmua)
                .Include(d => d.chitiet).ThenInclude(d => d.user_nhanhang)
                //.Include(d => d.nccs).ThenInclude(d => d.chitiet)
                //.Include(d => d.nccs).ThenInclude(d => d.dinhkem.Where(d => d.deleted_at == null))
                //.Include(d => d.nccs).ThenInclude(d => d.ncc)
                .FirstOrDefault();
            if (data != null)
            {
                var stt = 1;
                foreach (var item in data.chitiet)
                {
                    //var material = _context.MaterialModel.Where(d => item.hh_id == "m-" + d.id).FirstOrDefault();
                    //if (material != null)
                    //{
                    //    item.tenhh = material.tenhh;
                    //    item.mahh = material.mahh;
                    //    item.stt = stt++;
                    //}
                    item.soluong_nhanhang = item.soluong;
                    item.stt = stt++;
                }
            }
            return Json(data, new System.Text.Json.JsonSerializerOptions()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
            });
        }
        public JsonResult Getnccs(int id)
        {
            var data = _context.MuahangNccModel
                .Where(d => d.muahang_id == id)
                .Include(d => d.chitiet)
                .Include(d => d.dinhkem.Where(d => d.deleted_at == null))
                .Include(d => d.ncc)
                .ToList();
            if (data != null)
            {
                foreach (var ncc in data)
                {
                    var stt = 1;
                    //if (data.muahang_chonmua_id == ncc.id)
                    //{
                    //    data.muahang_chonmua = ncc;
                    //}
                    //ncc.chitiet = _context.MuahangNccChitietModel.Where(d => d.muahang_ncc_id == ncc.id).ToList();
                    if (ncc.chitiet != null)
                    {
                        foreach (var item in ncc.chitiet)
                        {
                            //var material = _context.MaterialModel.Where(d => item.hh_id == "m-" + d.id).FirstOrDefault();
                            //if (material != null)
                            //{
                            //    item.tenhh = material.tenhh;
                            //    item.mahh = material.mahh;
                            //    item.stt = stt++;
                            //}
                            item.stt = stt++;
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
        public JsonResult getDonhang(int id)
        {

            var data = _context.MuahangModel.Where(d => d.deleted_at == null && d.parent_id == id).Include(d => d.muahang_chonmua).ThenInclude(d => d.ncc).ToList();


            return Json(data, new System.Text.Json.JsonSerializerOptions()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
            });
        }
        public async Task<JsonResult> GetNhanhang(int id)
        {
            System.Security.Claims.ClaimsPrincipal currentUser = this.User;

            var user = await UserManager.GetUserAsync(currentUser);
            var user_id = UserManager.GetUserId(currentUser);

            var is_admin = await UserManager.IsInRoleAsync(user, "Administrator");
            var my_item = _context.DutruChitietModel.Include(d => d.dutru).Where(d => d.dutru.deleted_at == null && d.dutru.created_by == user_id).Select(d => d.id).ToList();

            var data = _context.MuahangModel.Where(d => d.id == id)
                .Include(d => d.muahang_chonmua).ThenInclude(d => d.ncc)
                .Include(d => d.chitiet)
                .ThenInclude(d => d.user_nhanhang).FirstOrDefault();
            if (data != null)
            {
                var stt = 1;
                foreach (var item in data.chitiet)
                {
                    //var material = _context.MaterialModel.Where(d => item.hh_id == "m-" + d.id).FirstOrDefault();
                    //if (material != null)
                    //{
                    //    item.tenhh = material.tenhh;
                    //    item.mahh = material.mahh;
                    //    item.stt = stt++;
                    //}
                    item.soluong_nhanhang = item.soluong;
                    item.stt = stt++;
                }
            }
            return Json(data, new System.Text.Json.JsonSerializerOptions()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
            });
        }
        public JsonResult GetUserNhanhang(int muahang_id)
        {
            var data = new List<string>();
            var dutru = _context.MuahangChitietModel.Where(d => d.muahang_id == muahang_id).Include(d => d.dutru_chitiet).ThenInclude(d => d.dutru).Select(d => d.dutru_chitiet.dutru).Distinct().ToList();
            foreach (var d in dutru)
            {
                data.Add(d.created_by);
            }
            data = data.Distinct().ToList();
            return Json(data, new System.Text.Json.JsonSerializerOptions()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
            });
        }
        public JsonResult getHistory(string mahh, bool? is_sametype)
        {
            var hh = _context.MaterialModel.Where(d => d.mahh == mahh).FirstOrDefault();
            var list_mahh = new List<string>() { mahh };
            if (is_sametype == true)
            {
                list_mahh = _context.MaterialModel.Where(d => d.mahh == mahh || (d.group != null && d.group == hh.group)).Select(d => d.mahh).ToList();
            }
            var muahang_chitiet = _context.MuahangChitietModel
                .Include(d => d.muahang).ThenInclude(d => d.muahang_chonmua).ThenInclude(d => d.chitiet)
                 .Include(d => d.muahang).ThenInclude(d => d.muahang_chonmua).ThenInclude(d => d.ncc)
                .Where(d => d.muahang.deleted_at == null && d.muahang.muahang_chonmua_id != null && d.muahang.pay_at != null && list_mahh.Contains(d.mahh))
                .ToList();
            var to = hh != null ? hh.mahh + "-" + hh.tenhh : "";

            var data = new ArrayList();
            foreach (var d in muahang_chitiet)
            {
                var muahang = d.muahang;
                var muahang_chonmua = muahang.muahang_chonmua;
                var chonmua = muahang_chonmua.chitiet.Where(e => e.muahang_chitiet_id == d.id).FirstOrDefault();
                var data1 = new
                {
                    muahang = muahang,
                    tenhh = chonmua.tenhh,
                    soluong = chonmua.soluong,
                    dongia = chonmua.dongia,
                    thanhtien = chonmua.thanhtien_vat,
                    ncc = muahang_chonmua.ncc,
                    tiente = muahang.muahang_chonmua.tiente,
                    note = d.note
                };
                data.Add(data1);
            }
            return Json(new { to = to, data = data }, new System.Text.Json.JsonSerializerOptions()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
            });
        }


        public async Task<JsonResult> laydongiahoachat(int muahang_id, string mancc)
        {
            System.Security.Claims.ClaimsPrincipal currentUser = this.User;

            var user = await UserManager.GetUserAsync(currentUser);
            var user_id = UserManager.GetUserId(currentUser);

            var is_admin = await UserManager.IsInRoleAsync(user, "Administrator");

            var viewPath = _configuration["Source:DONGIA_HOACHAT"];
            var data = _context.MuahangModel
                .Where(d => d.id == muahang_id)
                .Include(d => d.chitiet)
                .FirstOrDefault();

            var dic = new Dictionary<int, double>();

            if (data != null && data.chitiet.Any())
            {
                // Load file Excel 1 lần duy nhất
                Workbook workbook = new Workbook();
                workbook.LoadFromFile(viewPath);

                foreach (var item in data.chitiet)
                {
                    var dutru_chitiet_id = item.dutru_chitiet_id;
                    var muahang_chitiet_id = item.id;
                    double dongia = 0;
                    bool found = false;

                    foreach (Worksheet sheet in workbook.Worksheets)
                    {
                        var lastrow = sheet.LastDataRow;
                        var lastcol = sheet.LastDataColumn;
                        var nRowFirst = sheet.Rows[0]; // Dòng tiêu đề

                        // Tìm cột của nhà cung cấp (NCC)
                        int col_ncc = -1;
                        for (int colIndex = 1; colIndex < lastcol; colIndex++)
                        {
                            var check_cell = nRowFirst.Cells[colIndex].Value?.Trim();
                            if (string.IsNullOrEmpty(check_cell)) continue;

                            var split = check_cell.Split("_");
                            if (split.Length > 0 && split[0] == mancc)
                            {
                                col_ncc = colIndex;
                                break;
                            }
                        }

                        if (col_ncc == -1) continue;

                        // Tìm dòng chứa dữ liệu đúng dự trù
                        for (int rowIndex = 1; rowIndex < lastrow; rowIndex++)
                        {
                            var nowRow = sheet.Rows[rowIndex];
                            if (nowRow?.Cells[1] == null) continue;

                            int? sheet_dutru_chitiet_id = nowRow.Cells[1].Value != null && nowRow.Cells[1].Value != "" ? int.Parse(nowRow.Cells[1].Value) : null;
                            if (sheet_dutru_chitiet_id > 0)
                            {
                                Console.WriteLine(sheet_dutru_chitiet_id);
                            }
                            if (sheet_dutru_chitiet_id == dutru_chitiet_id)
                            {
                                var value = nowRow.Cells[col_ncc + 4].NumberValue;
                                dongia = double.IsNaN(value) ? 0 : value;
                                found = true;
                                break;
                            }
                        }

                        if (found) break;
                    }

                    dic[muahang_chitiet_id] = dongia;
                }
            }
            return Json(dic, new System.Text.Json.JsonSerializerOptions()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
                NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals
            });
        }
        public async Task<JsonResult> QrNhanhang(int muahang_id)
        {
            //var data = _context.MuahangChitietModel.Where(d => d.muahang_id == muahang_id).Include(d => d.dutru_chitiet).GroupBy(d => d.dutru_chitiet.dutru_id).Select(d => d.Key).ToList();

            string Domain = (HttpContext.Request.IsHttps ? "https://" : "http://") + HttpContext.Request.Host.Value;
            //foreach (var item in data)
            //{
            QRCoder.PayloadGenerator.Url generator = new QRCoder.PayloadGenerator.Url(Domain + "/muahang/nhanhang/" + muahang_id.ToString());
            string payload = generator.ToString();

            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            var qrCodeAsBitmap = qrCode.GetGraphic(20);

            System.IO.MemoryStream ms = new MemoryStream();
            qrCodeAsBitmap.Save(ms, ImageFormat.Png);
            byte[] byteImage = ms.ToArray();
            var SigBase64 = Convert.ToBase64String(byteImage); // Get Base64
            var link = "data:image/png;base64, " + SigBase64;
            //}

            return Json(new
            {
                success = true,
                link = link
            }, new System.Text.Json.JsonSerializerOptions()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
            });
        }
        [HttpPost]
        public async Task<JsonResult> xoancc(int id)
        {
            var Model = _context.MuahangNccModel.Where(d => d.id == id).FirstOrDefault();

            _context.Remove(Model);
            //_context.SaveChanges();
            var muahang_ncc_chitiet = _context.MuahangNccChitietModel.Where(d => d.muahang_ncc_id == id).ToList();

            _context.RemoveRange(muahang_ncc_chitiet);
            var muahang_ncc_dinhkem = _context.MuahangNccDinhkemModel.Where(d => d.muahang_ncc_id == id).ToList();

            _context.RemoveRange(muahang_ncc_dinhkem);
            _context.SaveChanges();
            return Json(new { success = true });
        }
        [HttpPost]
        public async Task<JsonResult> xoadinhkemncc(int id)
        {
            var Model = _context.MuahangNccDinhkemModel.Where(d => d.id == id).FirstOrDefault();
            Model.deleted_at = DateTime.Now;
            _context.Update(Model);
            _context.SaveChanges();
            return Json(new { success = true });
        }
        [HttpPost]
        public async Task<JsonResult> xoadinhkem(List<int> list_id)
        {
            var Model = _context.MuahangDinhkemModel.Where(d => list_id.Contains(d.id)).ToList();
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
            var Model = _context.MuahangModel.Where(d => d.id == id).FirstOrDefault();
            Model.deleted_at = DateTime.Now;
            _context.Update(Model);

            var Model1 = _context.MuahangChitietModel.Where(d => d.muahang_id == id).ToList();
            _context.RemoveRange(Model1);

            ///Child
            var Model2 = _context.MuahangModel.Where(d => d.parent_id == id).ToList();
            foreach (var item in Model2)
            {
                item.deleted_at = DateTime.Now;
            }
            _context.UpdateRange(Model2);

            foreach (var item in Model2)
            {
                var muahangchitiet = _context.MuahangChitietModel.Where(d => d.muahang_id == item.id).ToList();
                _context.RemoveRange(muahangchitiet);
            }

            _context.SaveChanges();
            return Json(Model, new System.Text.Json.JsonSerializerOptions()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
            });
        }

        [HttpPost]
        public async Task<JsonResult> Save(MuahangModel MuahangModel, List<MuahangChitietModel> list_add, List<MuahangChitietModel>? list_update, List<MuahangChitietModel>? list_delete)
        {

            MuahangModel? MuahangModel_old;
            if (MuahangModel.date != null && MuahangModel.date.Value.Kind == DateTimeKind.Utc)
            {
                MuahangModel.date = MuahangModel.date.Value.ToLocalTime();
            }
            bool is_check_state = false;
            if (MuahangModel.id == 0)
            {
                System.Security.Claims.ClaimsPrincipal currentUser = this.User;

                var user_id = UserManager.GetUserId(currentUser);
                //var user = await UserManager.GetUserAsync(currentUser);
                MuahangModel.created_at = DateTime.Now;
                MuahangModel.status_id = (int)Status.Baogia;
                MuahangModel.created_by = user_id;
                MuahangModel.activeStep = 0;

                _context.MuahangModel.Add(MuahangModel);

                _context.SaveChanges();

                MuahangModel_old = MuahangModel;


            }
            else
            {

                MuahangModel_old = _context.MuahangModel.Where(d => d.id == MuahangModel.id).Include(d => d.user_created_by).FirstOrDefault();
                if (MuahangModel_old.is_nhanhang == false || MuahangModel_old.is_thanhtoan == false)
                {
                    is_check_state = true;
                }
                MuahangModel.user_created_by = MuahangModel_old.user_created_by;
                MuahangModel.muahang_chonmua = null;
                CopyValues<MuahangModel>(MuahangModel_old, MuahangModel);
                MuahangModel_old.updated_at = DateTime.Now;

                _context.Update(MuahangModel_old);
                _context.SaveChanges();
            }

            if (list_delete != null)
                _context.RemoveRange(list_delete);
            if (list_add != null)
            {
                foreach (var item in list_add)
                {
                    item.muahang_id = MuahangModel_old.id;
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
            ///// CHECK FINISH
            //if (is_check_state == true && MuahangModel_old.is_nhanhang == true && MuahangModel_old.is_thanhtoan == true)
            //{
            //    MuahangModel_old.date_finish = DateTime.Now;
            //    _context.Update(MuahangModel_old);
            //}
            _context.SaveChanges();


            return Json(new { success = true, id = MuahangModel_old.id });
        }
        [HttpPost]
        public async Task<JsonResult> saveHangmau(int muahang_id, int nhacungcap_id)
        {
            var muahang = _context.MuahangModel.Where(d => d.id == muahang_id).Include(d => d.muahang_chonmua).Include(d => d.chitiet).FirstOrDefault();
            if (muahang.muahang_chonmua == null || muahang.muahang_chonmua.ncc_id != nhacungcap_id)
            {
                var date_now = DateTime.Now;
                var count_type_in_day = _context.DocumentModel.Where(d => d.type_id == 73 && d.created_at.Value.DayOfYear == date_now.DayOfYear).Count();
                var type = _context.DocumentTypeModel.Where(d => d.id == 73).Include(d => d.users_receive).FirstOrDefault();
                muahang.code = type.symbol + date_now.ToString("ddMMyy") + (count_type_in_day < 9 ? "0" : "") + (count_type_in_day + 1);

                muahang.is_dathang = true;
                muahang.is_thanhtoan = true;
                muahang.status_id = (int)Status.MuahangEsignSuccess;
                _context.Update(muahang);

                var chitiet = new List<MuahangNccChitietModel>();

                var MuahangNccModel = new MuahangNccModel()
                {
                    muahang_id = muahang.id,
                    ncc_id = nhacungcap_id,
                    chonmua = true,
                    thanhtien = 0,
                    thanhtien_vat = 0,
                    tonggiatri = 0,
                    phigiaohang = 0,
                    ck = 0,
                    tienvat = 0,
                    vat = 0,
                    tiente = "VND",
                    quidoi = 1,
                };
                _context.Add(MuahangNccModel);
                _context.SaveChanges();


                muahang.muahang_chonmua_id = MuahangNccModel.id;
                _context.Update(muahang);


                foreach (var item in muahang.chitiet)
                {
                    chitiet.Add(new MuahangNccChitietModel()
                    {
                        id = 0,
                        muahang_ncc_id = MuahangNccModel.id,
                        muahang_chitiet_id = item.id,
                        //hh_id = item.hh_id,
                        soluong = item.soluong,
                        dongia = 0,
                        thanhtien = 0,
                        thanhtien_vat = 0,
                        vat = 0,
                        mahh = item.mahh,
                        tenhh = item.tenhh,
                        dvt = item.dvt,
                    });
                }
                _context.AddRange(chitiet);
                _context.SaveChanges();
            }

            return Json(new { success = true });
        }
        [HttpPost]
        public async Task<JsonResult> saveNcc(List<MuahangNccModel> nccs)
        {

            System.Security.Claims.ClaimsPrincipal currentUser = this.User;

            var user_id = UserManager.GetUserId(currentUser);
            var user = await UserManager.GetUserAsync(currentUser);
            var muahang_id = 0;

            foreach (var item in nccs)
            {
                muahang_id = item.muahang_id.Value;
                //item.chitiet;
                if (item.id > 0)
                {
                    _context.Update(item);

                    _context.SaveChanges();

                }
                else
                {
                    _context.Add(item);

                    _context.SaveChanges();
                }
            }


            var files = Request.Form.Files;

            var items_attachment = new List<MuahangNccDinhkemModel>();
            if (files != null && files.Count > 0)
            {

                foreach (var file in files)
                {
                    var timeStamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
                    string name = file.FileName;
                    string type = file.Name;
                    string ext = Path.GetExtension(name);
                    string mimeType = file.ContentType;

                    var list = type.Split("_");
                    var key_muahang = Int32.Parse(list[1]);
                    var muahang_ncc_id = nccs[key_muahang].id;
                    //var fileName = Path.GetFileName(name);
                    var newName = timeStamp + "-" + muahang_ncc_id + "-" + name;

                    newName = newName.Replace("+", "_");
                    newName = newName.Replace("%", "_");
                    var dir = _configuration["Source:Path_Private"] + "\\buy\\muahang\\" + muahang_id;
                    bool exists = Directory.Exists(dir);

                    if (!exists)
                        Directory.CreateDirectory(dir);


                    var filePath = dir + "\\" + newName;

                    string url = "/private/buy/muahang/" + muahang_id + "/" + newName;
                    items_attachment.Add(new MuahangNccDinhkemModel
                    {
                        ext = ext,
                        url = url,
                        name = name,
                        mimeType = mimeType,
                        muahang_ncc_id = muahang_ncc_id,
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


            //_context.SaveChanges();
            if (muahang_id > 0)
            {
                var muahang = _context.MuahangModel.Where(d => d.id == muahang_id).FirstOrDefault();
                muahang.status_id = (int)Status.sosanhgia;
                _context.SaveChanges();
            }
            return Json(new { success = true });


        }
        [HttpPost]
        public async Task<JsonResult> saveNhanhang(int muahang_id, List<MuahangChitietModel> list)
        {

            System.Security.Claims.ClaimsPrincipal currentUser = this.User;

            var user_id = UserManager.GetUserId(currentUser);
            var user = await UserManager.GetUserAsync(currentUser);
            var list_dutru_chitiet_id = new List<int>();
            if (list != null)
            {
                foreach (var item in list)
                {
                    item.user_nhanhang = null;
                    list_dutru_chitiet_id.Add(item.dutru_chitiet_id);
                    _context.Update(item);
                }

                _context.SaveChanges();
                ////Check nhận hàng mua hàng
                var muahang = _context.MuahangModel.Where(d => d.id == muahang_id).FirstOrDefault();

                var list_nhanhang = list.Where(d => d.status_nhanhang == 1).Count();
                if (list_nhanhang == list.Count())
                {
                    muahang.is_nhanhang = true;
                    //if (muahang.is_thanhtoan == true && muahang.is_nhanhang == true)
                    //{
                    //    muahang.date_finish = DateTime.Now;
                    //    _context.Update(muahang);
                    //}
                    _context.Update(muahang);
                    _context.SaveChanges();

                }
                //// Check du trù finish
                var list_dutru_id = _context.DutruChitietModel.Where(d => list_dutru_chitiet_id.Contains(d.id)).Select(d => d.dutru_id).Distinct().ToList();
                var list_dutru = _context.DutruModel.Where(d => list_dutru_id.Contains(d.id)).Include(d => d.chitiet).ToList();
                foreach (var dutru in list_dutru)
                {
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
                }

            }
            return Json(new { success = true });

        }
        [HttpPost]
        public async Task<JsonResult> saveChitiet(List<MuahangChitietModel> list)
        {

            System.Security.Claims.ClaimsPrincipal currentUser = this.User;

            if (list != null)
            {
                foreach (var item in list)
                {
                    item.user_nhanhang = null;
                    _context.Update(item);
                }
            }
            _context.SaveChanges();

            return Json(new { success = true });

        }
        [HttpPost]

        public async Task<JsonResult> baogia(int id)
        {
            var data = _context.MuahangModel.Where(d => d.id == id).Include(d => d.chitiet).FirstOrDefault();
            if (data == null)
            {
                return Json(new { success = false, message = "Dự trù không tồn tại!" });
            }

            ////Nhận báo giá
            data.status_id = (int)Status.Baogia;
            //_context.SaveChanges();
            System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            var user_id = UserManager.GetUserId(currentUser);
            _context.Update(data);
            _context.SaveChanges();

            return Json(new { success = true });
        }
        [HttpPost]

        public async Task<JsonResult> XuatDondathang(int id)
        {
            CultureInfo vietnamCulture = new CultureInfo("vi-VN");
            var now = DateTime.Now;
            var raw = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"date",now.ToString("dd/MM/yyyy") },
            };
            bool? is_vat = false;
            var RawDetails = new List<RawMuahangDetails>();
            var data = _context.MuahangModel.Where(d => d.id == id).FirstOrDefault();
            if (data != null)
            {
                var muahang_chonmua = _context.MuahangNccModel.Where(d => d.id == data.muahang_chonmua_id).Include(d => d.chitiet).Include(d => d.ncc).FirstOrDefault();
                if (muahang_chonmua != null)
                {
                    var ncc_chon = muahang_chonmua;
                    is_vat = ncc_chon.is_vat;
                    raw.Add("tonggiatri", ncc_chon.tonggiatri.Value.ToString("#,##0.##", vietnamCulture));
                    raw.Add("tggh", data.date.Value.ToString("dd/MM/yyyy"));
                    raw.Add("thanhtien", ncc_chon.thanhtien.Value.ToString("#,##0.##", vietnamCulture));
                    raw.Add("thanhtien_vat", ncc_chon.thanhtien_vat.Value.ToString("#,##0.##", vietnamCulture));
                    raw.Add("phigiaohang", ncc_chon.phigiaohang.Value.ToString("#,##0.##", vietnamCulture));
                    raw.Add("ck", ncc_chon.ck.Value.ToString("#,##0.##", vietnamCulture));
                    raw.Add("tienvat", ncc_chon.tienvat.Value.ToString("#,##0.##", vietnamCulture));
                    //raw.Add("vat", ncc_chon.vat.Value.ToString());
                    raw.Add("tiente", ncc_chon.tiente.ToString());
                    var stt = 1;
                    foreach (var item in ncc_chon.chitiet)
                    {


                        var material = _context.MaterialModel.Where(d => item.mahh == d.mahh).Include(d => d.nhasanxuat).FirstOrDefault();
                        var tieuchuan = "";
                        var nhasx = "";
                        if (material != null)
                        {
                            nhasx = material.nhasanxuat?.tennsx;
                            tieuchuan = material.tieuchuan;
                        }
                        RawDetails.Add(new RawMuahangDetails
                        {
                            stt = stt++,
                            tenhh = item.tenhh,
                            mahh = item.mahh,
                            dvt = item.dvt,
                            soluong = item.soluong.Value.ToString("#,##0.##", vietnamCulture),
                            dongia = item.dongia.Value.ToString("#,##0.##", vietnamCulture),
                            thanhtien = item.thanhtien.Value.ToString("#,##0.##", vietnamCulture),
                            nhasx = nhasx,
                            tieuchuan = tieuchuan,
                            vat = item.vat,
                            //note = item.note,
                            tggh = data.date.Value.ToString("dd/MM/yyyy")
                            //artwork = material.masothietke,
                            //date = data.date.Value.ToString("yyyy-MM-dd")
                        });
                        //}
                    }
                    ///Nhà cung cấp
                    raw.Add("tenncc", ncc_chon.ncc.tenncc);
                    raw.Add("diachincc", ncc_chon.ncc.diachincc);
                    raw.Add("dienthoaincc", ncc_chon.ncc.dienthoaincc);
                    raw.Add("emailncc", ncc_chon.ncc.emailncc);
                    raw.Add("taikhoannh", ncc_chon.ncc.taikhoannh);
                    raw.Add("masothue", ncc_chon.ncc.masothue);
                    raw.Add("nguoilienhe", ncc_chon.ncc.nguoilienhe);
                    raw.Add("chucvu", ncc_chon.ncc.chucvu);

                }
                var loaithanhtoan = "Trả sau";
                if (data.loaithanhtoan == "tra_truoc")
                {
                    loaithanhtoan = "Trả trước";
                }
                raw.Add("loaithanhtoan", loaithanhtoan);
                raw.Add("ptthanhtoan", data.ptthanhtoan);
                raw.Add("diachigiaohang", data.diachigiaohang);

            }
            else
            {
                return Json(new { success = false, message = "Đề nghị mua hàng này không tồn tại!" });
            }

            System.Data.DataTable datatable_details = new System.Data.DataTable("details");
            PropertyInfo[] Props = typeof(RawMuahangDetails).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                //Setting column names as Property names
                datatable_details.Columns.Add(prop.Name);
            }
            foreach (RawMuahangDetails item in RawDetails)
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
            if (data.type_id == 1 && is_vat == false)
            {
                document.LoadFromFile(_configuration["Source:Path_Private"] + "/buy/templates/dondathangnvl.docx", Spire.Doc.FileFormat.Docx);
            }
            else if (data.type_id == 1 && is_vat == true)
            {
                document.LoadFromFile(_configuration["Source:Path_Private"] + "/buy/templates/dondathangnvl_vat.docx", Spire.Doc.FileFormat.Docx);
            }
            else if (is_vat == false)
            {
                document.LoadFromFile(_configuration["Source:Path_Private"] + "/buy/templates/dondathang.docx", Spire.Doc.FileFormat.Docx);
            }
            else
            {
                document.LoadFromFile(_configuration["Source:Path_Private"] + "/buy/templates/dondathang_vat.docx", Spire.Doc.FileFormat.Docx);
            }



            string[] fieldName = raw.Keys.ToArray();
            string[] fieldValue = raw.Values.ToArray();

            string[] MergeFieldNames = document.MailMerge.GetMergeFieldNames();
            string[] GroupNames = document.MailMerge.GetMergeGroupNames();

            document.MailMerge.Execute(fieldName, fieldValue);
            document.MailMerge.ExecuteWidthRegion(datatable_details);


            var timeStamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
            if (Directory.Exists(_configuration["Source:Path_Private"] + "\\buy\\muahang\\" + id))
            {
                Directory.CreateDirectory(_configuration["Source:Path_Private"] + "\\buy\\muahang\\" + id);
            }
            string url = "/private/buy/muahang/" + id + "/" + timeStamp + "-dondathang.docx";
            string url_pdf = "/private/buy/muahang/" + id + "/" + timeStamp + "-dondathang.pdf";
            document.SaveToFile(_configuration["Source:Path_Private"] + url.Replace("/private", "").Replace("/", "\\"), Spire.Doc.FileFormat.Docx);

            var url_return = url;
            //var convert_to_pdf = true;
            //if (convert_to_pdf == true)
            //{
            //    var output = ConvertWordFile(_configuration["Source:Path_Private"] + url.Replace("/private", "").Replace("/", "\\"), _configuration["Source:Path_Private"] + "\\buy\\muahang\\" + id);
            //    //if (process != "Success")
            //    if (output == false)
            //    {
            //        return Json(new { success = false });
            //    }
            //    url_return = url_pdf;

            //}

            data.dondathang = url_return;

            _context.SaveChanges();

            return Json(new { success = true });
        }
        [HttpPost]

        public async Task<JsonResult> SaveDinhkem(string note, int muahang_id)
        {
            System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            var user_id = UserManager.GetUserId(currentUser);
            var user = await UserManager.GetUserAsync(currentUser);
            //MuahangModel? MuahangModel_old;
            //MuahangModel_old = _context.MuahangModel.Where(d => d.id == muahang_id).FirstOrDefault();

            var files = Request.Form.Files;
            var now = DateTime.Now;
            var items = new List<MuahangDinhkemModel>();
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
                    var newName = timeStamp + "-" + muahang_id + "-" + name;
                    //var muahang_id = MuahangModel_old.id;
                    newName = newName.Replace("+", "_");
                    newName = newName.Replace("%", "_");
                    var dir = _configuration["Source:Path_Private"] + "\\buy\\muahang\\" + muahang_id;
                    bool exists = Directory.Exists(dir);

                    if (!exists)
                        Directory.CreateDirectory(dir);


                    var filePath = dir + "\\" + newName;

                    string url = "/private/buy/muahang/" + muahang_id + "/" + newName;
                    items.Add(new MuahangDinhkemModel
                    {
                        note = note,
                        ext = ext,
                        url = url,
                        name = name,
                        mimeType = mimeType,
                        muahang_id = muahang_id,
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

        public async Task<JsonResult> SaveUynhiemchi(int muahang_id)
        {
            System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            var user_id = UserManager.GetUserId(currentUser);
            //MuahangModel? MuahangModel_old;
            //MuahangModel_old = _context.MuahangModel.Where(d => d.id == muahang_id).FirstOrDefault();

            var files = Request.Form.Files;
            var now = DateTime.Now;
            var items = new List<MuahangUynhiemchiModel>();
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
                    var newName = timeStamp + "-" + muahang_id + "-" + name;
                    //var muahang_id = MuahangModel_old.id;
                    newName = newName.Replace("+", "_");
                    newName = newName.Replace("%", "_");
                    var dir = _configuration["Source:Path_Private"] + "\\buy\\muahang\\" + muahang_id;
                    bool exists = Directory.Exists(dir);

                    if (!exists)
                        Directory.CreateDirectory(dir);


                    var filePath = dir + "\\" + newName;

                    string url = "/private/buy/muahang/" + muahang_id + "/" + newName;
                    items.Add(new MuahangUynhiemchiModel
                    {
                        ext = ext,
                        url = url,
                        name = name,
                        mimeType = mimeType,
                        muahang_id = muahang_id,
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
        public async Task<JsonResult> saveQuidoi(int chonmua_id, decimal quidoi)
        {
            var chonmua = _context.MuahangNccModel.Where(d => d.id == chonmua_id).FirstOrDefault();
            if (chonmua != null)
            {
                chonmua.quidoi = quidoi;
                _context.Update(chonmua);
                _context.SaveChanges();
            }
            return Json(new { success = true });
        }
        [HttpPost]
        public async Task<JsonResult> Table(bool filter_thanhtoan)
        {
            System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            var user_id = UserManager.GetUserId(currentUser);
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            var code = Request.Form["filters[code]"].FirstOrDefault();
            var name = Request.Form["filters[name]"].FirstOrDefault();
            var id_text = Request.Form["filters[id]"].FirstOrDefault();
            int id = id_text != null ? Convert.ToInt32(id_text) : 0;
            var type_id_string = Request.Form["type_id"].FirstOrDefault();
            int type_id = type_id_string != null ? Convert.ToInt32(type_id_string) : 0;
            var status_id_string = Request.Form["filters[status_id]"].FirstOrDefault();
            int status_id = status_id_string != null ? Convert.ToInt32(status_id_string) : 0;
            //var filter_thanhtoan = Request.Form["filter_thanhtoan"].FirstOrDefault();
            //var tenhh = Request.Form["filters[tenhh]"].FirstOrDefault();
            int skip = start != null ? Convert.ToInt32(start) : 0;
            var customerData = _context.MuahangModel.Where(d => d.deleted_at == null && d.parent_id == null);
            if (status_id == 1)
            {
                customerData = customerData.Where(m => m.status_id == 1 || m.status_id == 6 || m.status_id == 7);
            }
            else if (status_id == 2)
            {
                customerData = customerData.Where(m => m.status_id == 9);
            }
            else if (status_id == 3)
            {
                customerData = customerData.Where(m => m.status_id == 11);
            }
            else if (status_id == 4)
            {
                customerData = customerData.Where(m => m.status_id == 10 && m.is_dathang != true);
            }
            else if (status_id == 5)
            {
                customerData = customerData.Where(m => m.date_finish == null && (m.is_dathang == true && ((m.loaithanhtoan == "tra_sau" && m.is_nhanhang == false) || (m.loaithanhtoan == "tra_truoc" && m.is_thanhtoan == true))));
            }
            else if (status_id == 6)
            {
                customerData = customerData.Where(m => m.date_finish == null && (m.is_dathang == true && ((m.loaithanhtoan == "tra_truoc" && m.is_thanhtoan == false) || (m.loaithanhtoan == "tra_sau" && m.is_nhanhang == true))));
            }
            else if (status_id == 7)
            {
                customerData = customerData.Where(m => m.date_finish != null);
            }

            if (filter_thanhtoan == true)
            {
                customerData = customerData.Where(d => d.is_dathang == true && (d.loaithanhtoan == "tra_truoc" || (d.loaithanhtoan == "tra_sau" && d.is_nhanhang == true)));
            }
            if (type_id != null && type_id != 0)
            {
                customerData = customerData.Where(d => d.type_id == type_id);
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
                var thoigiangiaohang = "";
                var thanhtoan = "";
                if (chonmua != null)
                {
                    tonggiatri = chonmua.tonggiatri;
                    tiente = chonmua.tiente;

                    thoigiangiaohang = chonmua.thoigiangiaohang;
                    thanhtoan = chonmua.thanhtoan;
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
                    date_pay_at = record.date_pay_at,
                    tiente = tiente,
                    thoigiangiaohang = thoigiangiaohang,
                    thanhtoan = thanhtoan,
                    date_finish = record.date_finish,
                    tonggiatri = tonggiatri,
                    created_at = record.created_at
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
        public async Task<JsonResult> xuatpdf(int id, bool is_view = false, int loaimau = 0)
        {
            CultureInfo vietnamCulture = new CultureInfo("vi-VN");
            System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            var user_id = UserManager.GetUserId(currentUser);
            var now = DateTime.Now;

            var date_now = DateTime.Now;
            var count_type_in_day = _context.DocumentModel.Where(d => d.type_id == 73 && d.created_at.Value.DayOfYear == date_now.DayOfYear).Count();
            var type = _context.DocumentTypeModel.Where(d => d.id == 73).Include(d => d.users_receive).FirstOrDefault();
            var code = type.symbol + date_now.ToString("ddMMyy") + (count_type_in_day < 9 ? "0" : "") + (count_type_in_day + 1);

            //var is_nvl_moi = false;
            bool? is_vat = false;
            var raw = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"ngay",now.ToString("dd") },
                {"thang",now.ToString("MM") },
                {"nam",now.ToString("yyyy") },
            };
            var bophan = "P. HR&GA";
            if (user_id == "28dc61a5-001d-4c1f-9325-9b9318dc3c59" || user_id == "e9ca633b-ac5c-4f64-9485-10cfe5388d16" || user_id == "d2104cc4-10b1-408c-8161-b4df8d6df1b5")
            {
                bophan = "Cung ứng";
            }
            raw.Add("bophan", bophan);
            var RawDetails = new List<RawMuahangDetails>();
            var RawTonghop = new List<RawMuahangTonghop>();
            var data = _context.MuahangModel.Where(d => d.id == id)
                .Include(d => d.nccs).ThenInclude(d => d.ncc)
                .Include(d => d.nccs).ThenInclude(d => d.chitiet)
                .ThenInclude(d => d.muahang_chitiet)
                    .ThenInclude(d => d.dutru_chitiet).ThenInclude(d => d.dutru).ThenInclude(d => d.bophan)
                    .FirstOrDefault();
            if (data != null)
            {
                raw.Add("code", code);
                raw.Add("note", data.note);
                raw.Add("note_chonmua", data.note_chonmua);

                if (data.is_multiple_ncc != true)
                {
                    var muahang_chonmua = _context.MuahangNccModel.Where(d => d.id == data.muahang_chonmua_id)
                    .Include(d => d.chitiet)
                    .ThenInclude(d => d.muahang_chitiet)
                    .ThenInclude(d => d.dutru_chitiet).ThenInclude(d => d.dutru).ThenInclude(d => d.bophan)
                    .FirstOrDefault();
                    if (muahang_chonmua != null)
                    {
                        is_vat = muahang_chonmua.is_vat;
                        var ncc_chon = muahang_chonmua;
                        var tonggiatri_string = ncc_chon.tonggiatri.Value.ToString("#,##0.##", vietnamCulture);
                        var thanhtien_vat_string = ncc_chon.thanhtien_vat.Value.ToString("#,##0.##", vietnamCulture);
                        var thanhtien_string = ncc_chon.thanhtien.Value.ToString("#,##0.##", vietnamCulture);
                        if (ncc_chon.tiente == "VND" || ncc_chon.tiente == "VNĐ" || ncc_chon.tiente == null)
                        {
                            tonggiatri_string = ncc_chon.tonggiatri.Value.ToString("#,##0", vietnamCulture);
                            thanhtien_vat_string = ncc_chon.thanhtien_vat.Value.ToString("#,##0", vietnamCulture);
                            thanhtien_string = ncc_chon.thanhtien.Value.ToString("#,##0", vietnamCulture);
                        }

                        raw.Add("tonggiatri", tonggiatri_string);
                        raw.Add("thanhtien_vat", thanhtien_vat_string);
                        raw.Add("thanhtien", thanhtien_string);
                        raw.Add("phigiaohang", ncc_chon.phigiaohang.Value.ToString("#,##0.##", vietnamCulture));
                        raw.Add("ck", ncc_chon.ck.Value.ToString("#,##0.##", vietnamCulture));
                        raw.Add("tienvat", ncc_chon.tienvat.Value.ToString("#,##0.##", vietnamCulture));
                        //raw.Add("vat", ncc_chon.vat.Value.ToString());
                        raw.Add("tiente", ncc_chon.tiente.ToString());
                        var stt = 1;
                        foreach (var item in ncc_chon.chitiet)
                        {
                            //if (item.hh_id != null && data.type_id == 1) /// Check có phải mua nguyên liệu mới hay ko?
                            //    is_nvl_moi = true;
                            //var material = _context.MaterialModel.Where(d => item.hh_id == "m-" + d.id).FirstOrDefault();
                            //if (material != null)
                            //{
                            var dutru = item.muahang_chitiet.dutru_chitiet.dutru;
                            if (dutru.type_id == 2 || dutru.type_id == 3)
                            {
                                var bp = dutru.bophan;
                                var thanhtien = item.thanhtien;
                                RawTonghop.Add(new RawMuahangTonghop
                                {
                                    bophan = bp.name,
                                    thanhtien = item.thanhtien.Value,
                                    tiente = ncc_chon.tiente
                                });
                            }
                            RawDetails.Add(new RawMuahangDetails
                            {
                                stt = stt++,
                                tenhh = item.tenhh,
                                mahh = item.mahh,
                                dvt = item.dvt,
                                soluong = item.soluong.Value.ToString("#,##0.##", vietnamCulture),
                                dongia = item.dongia.Value.ToString("#,##0.#####", vietnamCulture),
                                thanhtien = item.thanhtien.Value.ToString("#,##0.##", vietnamCulture),
                                vat = item.vat,
                                note = item.note ?? "",
                                //artwork = material.masothietke,
                                //date = data.date.Value.ToString("yyyy-MM-dd")
                            });


                            //}
                        }
                        RawTonghop = RawTonghop.GroupBy(d => d.bophan).Select(d => new RawMuahangTonghop
                        {
                            bophan = d.Key,
                            thanhtien = d.Sum(e => e.thanhtien),
                            tiente = d.FirstOrDefault().tiente
                        }).ToList();
                    }
                }
                var key = 0;
                foreach (var ncc in data.nccs)
                {
                    if (data.muahang_chonmua_id == ncc.id)
                    {
                        ncc.chonmua = true;
                    }
                    var list_nsx = ncc.chitiet.Select(d => d.nhasx).ToList();
                    var nhasx = string.Join("/", list_nsx);
                    raw.Add("bang_ncc_ten_" + key, ncc.ncc.tenncc);
                    raw.Add("bang_ncc_nsx_" + key, nhasx);
                    raw.Add("bang_ncc_tong_" + key, ncc.tonggiatri.Value.ToString("#,##0.##", vietnamCulture));
                    raw.Add("bang_ncc_dap_ung_" + key, ncc.dapung == true ? "X" : "");
                    raw.Add("bang_ncc_time_delivery_" + key, ncc.thoigiangiaohang);
                    raw.Add("bang_ncc_policy_" + key, ncc.baohanh);
                    raw.Add("bang_ncc_payment_" + key, ncc.thanhtoan);
                    raw.Add("bang_ncc_select_" + key, ncc.chonmua == true ? "X" : "");

                    key++;
                }


                if (data.is_multiple_ncc == true)
                {
                    var nccs = data.nccs.Where(d => d.chonmua == true).ToList();
                    var tonggiatri = nccs.Sum(d => d.tonggiatri);
                    var ncc = nccs.FirstOrDefault();
                    var tonggiatri_string = tonggiatri.Value.ToString("#,##0.##", vietnamCulture);
                    if (ncc.tiente == "VND" || ncc.tiente == "VNĐ" || ncc.tiente == null)
                    {
                        tonggiatri_string = tonggiatri.Value.ToString("#,##0", vietnamCulture);
                    }
                    raw.Add("tonggiatri", tonggiatri_string);
                    raw.Add("tiente", ncc != null ? ncc.tiente : "");

                    var stt = 1;
                    foreach (var ncc_chon in nccs)
                    {
                        foreach (var item in ncc_chon.chitiet)
                        {
                            //if (item.hh_id != null && data.type_id == 1) /// Check có phải mua nguyên liệu mới hay ko?
                            //    is_nvl_moi = true;
                            //var material = _context.MaterialModel.Where(d => item.hh_id == "m-" + d.id).FirstOrDefault();
                            //if (material != null)
                            //{
                            var dutru = item.muahang_chitiet.dutru_chitiet.dutru;
                            if (dutru.type_id == 2 || dutru.type_id == 3)
                            {
                                var bp = dutru.bophan;
                                var thanhtien = item.thanhtien;
                                RawTonghop.Add(new RawMuahangTonghop
                                {
                                    bophan = bp.name,
                                    thanhtien = item.thanhtien.Value,
                                    tiente = ncc_chon.tiente
                                });
                            }
                        }
                    }

                    RawTonghop = RawTonghop.GroupBy(d => d.bophan).Select(d => new RawMuahangTonghop
                    {
                        bophan = d.Key,
                        thanhtien = d.Sum(e => e.thanhtien),
                        tiente = d.FirstOrDefault().tiente
                    }).ToList();
                }
            }
            else
            {
                return Json(new { success = false, message = "Đề nghị mua hàng này không tồn tại!" });
            }

            ///DETAILS
            System.Data.DataTable datatable_details = new System.Data.DataTable("details");
            PropertyInfo[] Props = typeof(RawMuahangDetails).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                //Setting column names as Property names
                datatable_details.Columns.Add(prop.Name);
            }
            foreach (RawMuahangDetails item in RawDetails)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    //inserting property values to datatable rows
                    values[i] = Props[i].GetValue(item, null);
                }
                datatable_details.Rows.Add(values);
            }

            ///Tong hop
            System.Data.DataTable datatable_tonghop = new System.Data.DataTable("tonghop");
            PropertyInfo[] Props_tonghop = typeof(RawMuahangTonghop).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props_tonghop)
            {
                //Setting column names as Property names
                datatable_tonghop.Columns.Add(prop.Name);
            }
            foreach (RawMuahangTonghop item in RawTonghop)
            {
                var values = new object[Props_tonghop.Length];
                for (int i = 0; i < Props_tonghop.Length; i++)
                {
                    //inserting property values to datatable rows
                    values[i] = Props_tonghop[i].GetValue(item, null);
                }
                datatable_tonghop.Rows.Add(values);
            }
            ///XUẤT PDF

            //Creates Document instance
            Spire.Doc.Document document = new Spire.Doc.Document();
            //Loads the word document
            if (data.is_multiple_ncc == true && data.type_id == 1)
            {
                document.LoadFromFile(_configuration["Source:Path_Private"] + "/buy/templates/denghimuahangnvlmoi_multiple_ncc.docx", Spire.Doc.FileFormat.Docx);

            }
            else if (data.is_multiple_ncc == true)
            {
                document.LoadFromFile(_configuration["Source:Path_Private"] + "/buy/templates/denghimuahang_multiple_ncc.docx", Spire.Doc.FileFormat.Docx);

            }
            else if (loaimau == 1 && is_vat == false)
            {
                document.LoadFromFile(_configuration["Source:Path_Private"] + "/buy/templates/denghimuahangnvlmoi.docx", Spire.Doc.FileFormat.Docx);
            }
            else if (data.type_id == 1 && is_vat == false)
            {
                document.LoadFromFile(_configuration["Source:Path_Private"] + "/buy/templates/denghimuahangnvl.docx", Spire.Doc.FileFormat.Docx);
            }
            else if (loaimau == 1 && is_vat == true)
            {
                document.LoadFromFile(_configuration["Source:Path_Private"] + "/buy/templates/denghimuahangnvlmoi_vat.docx", Spire.Doc.FileFormat.Docx);
            }
            else if (data.type_id == 1 && is_vat == true)
            {
                document.LoadFromFile(_configuration["Source:Path_Private"] + "/buy/templates/denghimuahangnvl_vat.docx", Spire.Doc.FileFormat.Docx);
            }
            else if (is_vat == false)
            {
                document.LoadFromFile(_configuration["Source:Path_Private"] + "/buy/templates/denghimuahang.docx", Spire.Doc.FileFormat.Docx);
            }
            else
            {
                document.LoadFromFile(_configuration["Source:Path_Private"] + "/buy/templates/denghimuahang_vat.docx", Spire.Doc.FileFormat.Docx);
            }



            string[] fieldName = raw.Keys.ToArray();
            string[] fieldValue = raw.Values.ToArray();

            string[] MergeFieldNames = document.MailMerge.GetMergeFieldNames();
            string[] GroupNames = document.MailMerge.GetMergeGroupNames();

            document.MailMerge.Execute(fieldName, fieldValue);

            if (data.is_multiple_ncc == true)
            {
                //raw.Add("tonggiatri", ncc_chon.tonggiatri.Value.ToString("#,##0.##"));
                //raw.Add("thanhtien", ncc_chon.thanhtien.Value.ToString("#,##0.##"));
                //raw.Add("thanhtien_vat", ncc_chon.thanhtien_vat.Value.ToString("#,##0.##"));
                //raw.Add("phigiaohang", ncc_chon.phigiaohang.Value.ToString("#,##0.##"));
                //raw.Add("ck", ncc_chon.ck.Value.ToString("#,##0.##"));
                //raw.Add("tienvat", ncc_chon.tienvat.Value.ToString("#,##0.##"));

                // Tạo dữ liệu mẫu cho nhà cung cấp (nccs - bảng cha)
                var nccs = data.nccs.Where(d => d.chonmua == true).ToList();
                //var nccs_id = nccs.Select(d => d.id).ToList();
                //var parent = nccs.Select(d => new
                //{
                //    id = d.id,
                //    tiente = d.tiente,
                //    thanhtien = d.thanhtien.Value.ToString("#,##0.##"),
                //    thanhtien_vat = d.thanhtien_vat.Value.ToString("#,##0.##"),
                //    phigiaohang = d.phigiaohang.Value.ToString("#,##0.##"),
                //    tonggiatri = d.tonggiatri.Value.ToString("#,##0.##"),
                //    tienvat = d.tienvat.Value.ToString("#,##0.##"),
                //    ck = d.ck.Value.ToString("#,##0.##"),
                //    tenncc = d.ncc.tenncc
                //}).ToList();
                var parent = new List<object>();
                var details = new List<object>();
                foreach (var item in nccs)
                {
                    var stt = 1;
                    //var chitiet = _context.MuahangNccChitietModel.Where(d => d.muahang_ncc_id == item.id).ToList();
                    foreach (var e in item.chitiet)
                    {
                        details.Add(new
                        {
                            stt = stt++,
                            muahang_ncc_id = e.muahang_ncc_id,
                            tenhh = e.tenhh,
                            mahh = e.mahh,
                            dvt = e.dvt,
                            soluong = e.soluong.Value.ToString("#,##0.##", vietnamCulture),
                            dongia = e.dongia.Value.ToString("#,##0.#####", vietnamCulture),
                            thanhtien = e.thanhtien.Value.ToString("#,##0.##", vietnamCulture),
                            vat = e.vat,
                            note = e.note ?? "",
                        });
                    }
                    parent.Add(new
                    {
                        id = item.id,
                        tiente = item.tiente,
                        thanhtien = item.thanhtien.Value.ToString("#,##0.##", vietnamCulture),
                        thanhtien_vat = item.thanhtien_vat.Value.ToString("#,##0.##", vietnamCulture),
                        phigiaohang = item.phigiaohang.Value.ToString("#,##0.##", vietnamCulture),
                        tonggiatri = item.tonggiatri.Value.ToString("#,##0.##", vietnamCulture),
                        tienvat = item.tienvat.Value.ToString("#,##0.##", vietnamCulture),
                        ck = item.ck.Value.ToString("#,##0.##", vietnamCulture),
                        tenncc = item.ncc.tenncc
                    });
                }



                // Tạo MailMergeDataSet cho nccs và details
                MailMergeDataSet mailMergeDataSet = new MailMergeDataSet();
                mailMergeDataSet.Add(new MailMergeDataTable("nccs", parent));
                mailMergeDataSet.Add(new MailMergeDataTable("details", details));

                // Thiết lập quan hệ giữa bảng cha (nccs) và bảng con (details)
                List<DictionaryEntry> relationsList = new List<DictionaryEntry>();
                relationsList.Add(new DictionaryEntry("nccs", string.Empty));
                relationsList.Add(new DictionaryEntry("details", "muahang_ncc_id = %nccs.id%"));

                // Thực hiện MailMerge với vùng dữ liệu lồng nhau


                document.MailMerge.ExecuteWidthNestedRegion(mailMergeDataSet, relationsList);
                if (data.type_id != 1)
                {

                    document.MailMerge.ExecuteWidthRegion(datatable_tonghop);
                }
            }
            else
            {

                document.MailMerge.ExecuteWidthRegion(datatable_details);
                document.MailMerge.ExecuteWidthRegion(datatable_tonghop);
            }
            var timeStamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
            if (Directory.Exists(_configuration["Source:Path_Private"] + "\\buy\\muahang\\" + id))
            {
                Directory.CreateDirectory(_configuration["Source:Path_Private"] + "\\buy\\muahang\\" + id);
            }
            string url = "/private/buy/muahang/" + id + "/" + timeStamp + ".docx";
            string url_pdf = "/private/buy/muahang/" + id + "/" + timeStamp + ".pdf";
            document.SaveToFile(_configuration["Source:Path_Private"] + url.Replace("/private", "").Replace("/", "\\"), Spire.Doc.FileFormat.Docx);

            var url_return = url;
            var convert_to_pdf = true;
            if (convert_to_pdf == true)
            {
                var output = ConvertWordFile(_configuration["Source:Path_Private"] + url.Replace("/private", "").Replace("/", "\\"), _configuration["Source:Path_Private"] + "\\buy\\muahang\\" + id);
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
                data.status_id = (int)Status.MuahangPDF;
                //_context.SaveChanges();


                ///UPLOAD ESIGN
                var user = await UserManager.GetUserAsync(currentUser);
                ///Document
                var DocumentModel = new DocumentModel()
                {
                    name_vi = data.name,
                    description_vi = data.note,
                    priority = 2,
                    status_id = (int)DocumentStatus.Draft,
                    type_id = 73,
                    user_id = user_id,
                    user_next_signature_id = user_id,
                    is_sign_parellel = false,
                    created_at = DateTime.Now,
                };

                DocumentModel.code = code;
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
                ///Lấy dự trù
                var list_attachment = new List<DocumentAttachmentModel>();
                var dutru = _context.MuahangChitietModel.Where(d => d.muahang_id == id).Include(d => d.dutru_chitiet).ThenInclude(d => d.dutru).Select(d => d.dutru_chitiet.dutru).ToList();
                dutru = dutru.Distinct().ToList();
                foreach (var d in dutru)
                {
                    list_attachment.Add(new DocumentAttachmentModel()
                    {
                        document_id = DocumentModel.id,
                        name = "Dự trù " + d.code + "- " + d.name,
                        ext = ".pdf",
                        mimeType = "application/pdf",
                        url = d.pdf,
                        created_at = DateTime.Now
                    });
                }

                ///Lấy báo giá
                var baogia = _context.MuahangNccModel.Where(d => d.muahang_id == id).Include(d => d.dinhkem).Include(d => d.ncc).ToList();
                foreach (var d in baogia)
                {
                    foreach (var item in d.dinhkem)
                    {
                        list_attachment.Add(new DocumentAttachmentModel()
                        {
                            document_id = DocumentModel.id,
                            name = item.name,
                            ext = item.ext,
                            mimeType = item.mimeType,
                            url = item.url,
                            created_at = item.created_at
                        });
                    }
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
                    type = "muahang",
                    created_at = DateTime.Now
                };
                _context.Add(RelatedEsignModel);

                //_context.SaveChanges();
                data.status_id = (int)Status.MuahangEsign;
                data.activeStep = 1;
                data.esign_id = DocumentModel.id;
                data.code = DocumentModel.code;
                _context.Update(data);
                _context.SaveChanges();

                return Json(new { success = true });
            }
        }
        [HttpPost]
        public async Task<JsonResult> xuatexcel(bool filter_thanhtoan)
        {
            System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            var user_id = UserManager.GetUserId(currentUser);
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            var code = Request.Form["filters[code]"].FirstOrDefault();
            var name = Request.Form["filters[name]"].FirstOrDefault();
            var id_text = Request.Form["filters[id]"].FirstOrDefault();
            int id = id_text != null ? Convert.ToInt32(id_text) : 0;
            var type_id_string = Request.Form["type_id"].FirstOrDefault();
            int type_id = type_id_string != null ? Convert.ToInt32(type_id_string) : 0;
            var status_id_string = Request.Form["filters[status_id]"].FirstOrDefault();
            int status_id = status_id_string != null ? Convert.ToInt32(status_id_string) : 0;
            //var filter_thanhtoan = Request.Form["filter_thanhtoan"].FirstOrDefault();
            //var tenhh = Request.Form["filters[tenhh]"].FirstOrDefault();
            int skip = start != null ? Convert.ToInt32(start) : 0;
            var customerData = _context.MuahangModel.Where(d => d.deleted_at == null && d.parent_id == null);
            if (status_id == 1)
            {
                customerData = customerData.Where(m => m.status_id == 1 || m.status_id == 6 || m.status_id == 7);
            }
            else if (status_id == 2)
            {
                customerData = customerData.Where(m => m.status_id == 9);
            }
            else if (status_id == 3)
            {
                customerData = customerData.Where(m => m.status_id == 11);
            }
            else if (status_id == 4)
            {
                customerData = customerData.Where(m => m.status_id == 10 && m.is_dathang != true);
            }
            else if (status_id == 5)
            {
                customerData = customerData.Where(m => m.date_finish == null && (m.is_dathang == true && ((m.loaithanhtoan == "tra_sau" && m.is_nhanhang == false) || (m.loaithanhtoan == "tra_truoc" && m.is_thanhtoan == true))));
            }
            else if (status_id == 6)
            {
                customerData = customerData.Where(m => m.date_finish == null && (m.is_dathang == true && ((m.loaithanhtoan == "tra_truoc" && m.is_thanhtoan == false) || (m.loaithanhtoan == "tra_sau" && m.is_nhanhang == true))));
            }
            else if (status_id == 7)
            {
                customerData = customerData.Where(m => m.date_finish != null);
            }

            if (filter_thanhtoan == true)
            {
                customerData = customerData.Where(d => d.is_dathang == true && (d.loaithanhtoan == "tra_truoc" || (d.loaithanhtoan == "tra_sau" && d.is_nhanhang == true)));
            }
            if (type_id != null && type_id != 0)
            {
                customerData = customerData.Where(d => d.type_id == type_id);
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
            var datapost = customerData
                 .Include(d => d.user_created_by)
                 .Include(d => d.muahang_chonmua)
                 .ThenInclude(d => d.chitiet)
                 .Include(d => d.muahang_chonmua)
                 .ThenInclude(d => d.ncc)
                 .ToList();
            var data = new ArrayList();

            var viewPath = _configuration["Source:Path_Private"] + "\\buy\\templates\\Muahang.xlsx";
            var documentPath = "/tmp/Rawdata_" + DateTime.Now.ToFileTimeUtc() + ".xlsx";
            string Domain = (HttpContext.Request.IsHttps ? "https://" : "http://") + HttpContext.Request.Host.Value;
            Workbook workbook = new Workbook();
            workbook.LoadFromFile(viewPath);
            Worksheet sheet = workbook.Worksheets[0];
            int stt = 0;
            var start_r = 2;

            DataTable dt = new DataTable();
            //dt.Columns.Add("stt", typeof(int));
            dt.Columns.Add("id", typeof(int));
            dt.Columns.Add("code", typeof(string));
            dt.Columns.Add("name", typeof(string));
            dt.Columns.Add("status", typeof(string));
            dt.Columns.Add("giaohang", typeof(string));
            dt.Columns.Add("thanhtoan", typeof(string));
            dt.Columns.Add("date_dukien", typeof(string));
            dt.Columns.Add("tonggiatri", typeof(decimal));
            dt.Columns.Add("created_at", typeof(DateTime));
            dt.Columns.Add("created_by", typeof(string));

            var stt_cell = 2;



            sheet.InsertRow(start_r, datapost.Count(), InsertOptionsType.FormatAsAfter);
            foreach (var record in datapost)
            {
                var chonmua = record.muahang_chonmua;
                decimal? tonggiatri = null;
                var tiente = "VND";
                var thoigiangiaohang = "";
                var thanhtoan = "";
                if (chonmua != null)
                {
                    tonggiatri = chonmua.tonggiatri;
                    tiente = chonmua.tiente;

                    thoigiangiaohang = chonmua.thoigiangiaohang;
                    thanhtoan = chonmua.thanhtoan;



                }
                var status = "";
                if (record.date_finish != null)
                {
                    status = "Hoàn thành";

                }
                else if (record.is_dathang == true && ((record.loaithanhtoan == "tra_sau" && record.is_nhanhang == true) ||
                                (record.loaithanhtoan == "tra_truoc" && record.is_thanhtoan == true)))
                {
                    status = "Chờ nhận hàng";
                }
                else if (record.is_dathang == true && ((record.loaithanhtoan == "tra_sau" && record.is_nhanhang == false) ||
                                (record.loaithanhtoan == "tra_truoc" && record.is_thanhtoan == false)))
                {
                    status = "Chờ thanh toán";
                }
                else if (record.status_id == 1 || record.status_id == 6 || record.status_id == 7)
                {
                    status = "Đang thực hiện";

                }
                else if (record.status_id == 9)
                {
                    status = "Đang trình ký";

                }
                else if (record.status_id == 10)
                {
                    status = "Đang đặt hàng";

                }
                else if (record.status_id == 11)
                {
                    status = "Không duyệt";

                }
                DataRow dr1 = dt.NewRow();
                //dr1["stt"] = (++stt);
                dr1["id"] = record.id;
                dr1["code"] = record.code;
                dr1["name"] = record.name;
                dr1["status"] = status;
                //dr1["baohanh"] = chonmua.baohanh;
                dr1["giaohang"] = thoigiangiaohang;
                dr1["thanhtoan"] = thanhtoan;
                dr1["date_dukien"] = record.date_pay_at != null ? record.date_pay_at.Value.ToString("dd/MM/YYYY") : "";
                dr1["tonggiatri"] = tonggiatri ?? 0;
                dr1["created_at"] = record.created_at;
                dr1["created_by"] = record.user_created_by.FullName;
                dt.Rows.Add(dr1);
                start_r++;

            }
            sheet.InsertDataTable(dt, false, 2, 1);

            workbook.SaveToFile("./wwwroot" + documentPath, ExcelVersion.Version2013);

            return Json(new { success = true, link = Domain + documentPath });
        }
        [HttpPost]
        public async Task<JsonResult> xuatexcelchitiet(bool filter_thanhtoan)
        {
            System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            var user_id = UserManager.GetUserId(currentUser);
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            var code = Request.Form["filters[code]"].FirstOrDefault();
            var name = Request.Form["filters[name]"].FirstOrDefault();
            var id_text = Request.Form["filters[id]"].FirstOrDefault();
            int id = id_text != null ? Convert.ToInt32(id_text) : 0;
            var type_id_string = Request.Form["type_id"].FirstOrDefault();
            int type_id = type_id_string != null ? Convert.ToInt32(type_id_string) : 0;
            var status_id_string = Request.Form["filters[status_id]"].FirstOrDefault();
            int status_id = status_id_string != null ? Convert.ToInt32(status_id_string) : 0;
            //var filter_thanhtoan = Request.Form["filter_thanhtoan"].FirstOrDefault();
            //var tenhh = Request.Form["filters[tenhh]"].FirstOrDefault();
            int skip = start != null ? Convert.ToInt32(start) : 0;
            var customerData = _context.MuahangModel.Where(d => d.deleted_at == null && d.parent_id == null);
            if (status_id == 1)
            {
                customerData = customerData.Where(m => m.status_id == 1 || m.status_id == 6 || m.status_id == 7);
            }
            else if (status_id == 2)
            {
                customerData = customerData.Where(m => m.status_id == 9);
            }
            else if (status_id == 3)
            {
                customerData = customerData.Where(m => m.status_id == 11);
            }
            else if (status_id == 4)
            {
                customerData = customerData.Where(m => m.status_id == 10 && m.is_dathang != true);
            }
            else if (status_id == 5)
            {
                customerData = customerData.Where(m => m.date_finish == null && (m.is_dathang == true && ((m.loaithanhtoan == "tra_sau" && m.is_nhanhang == false) || (m.loaithanhtoan == "tra_truoc" && m.is_thanhtoan == true))));
            }
            else if (status_id == 6)
            {
                customerData = customerData.Where(m => m.date_finish == null && (m.is_dathang == true && ((m.loaithanhtoan == "tra_truoc" && m.is_thanhtoan == false) || (m.loaithanhtoan == "tra_sau" && m.is_nhanhang == true))));
            }
            else if (status_id == 7)
            {
                customerData = customerData.Where(m => m.date_finish != null);
            }

            if (filter_thanhtoan == true)
            {
                customerData = customerData.Where(d => d.is_dathang == true && (d.loaithanhtoan == "tra_truoc" || (d.loaithanhtoan == "tra_sau" && d.is_nhanhang == true)));
            }
            if (type_id != null && type_id != 0)
            {
                customerData = customerData.Where(d => d.type_id == type_id);
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
            var datapost = customerData
                 .Include(d => d.user_created_by)
                 .Include(d => d.muahang_chonmua)
                 .ThenInclude(d => d.chitiet)
                 .Include(d => d.muahang_chonmua)
                 .ThenInclude(d => d.ncc)
                 .ToList();
            var data = new ArrayList();

            var viewPath = _configuration["Source:Path_Private"] + "\\buy\\templates\\Muahangchitiet.xlsx";
            var documentPath = "/tmp/Rawdata_" + DateTime.Now.ToFileTimeUtc() + ".xlsx";
            string Domain = (HttpContext.Request.IsHttps ? "https://" : "http://") + HttpContext.Request.Host.Value;
            Workbook workbook = new Workbook();
            workbook.LoadFromFile(viewPath);
            Worksheet sheet = workbook.Worksheets[0];
            int stt = 0;
            var start_r = 2;

            DataTable dt = new DataTable();
            //dt.Columns.Add("stt", typeof(int));
            dt.Columns.Add("name", typeof(string));
            dt.Columns.Add("mahh", typeof(string));
            dt.Columns.Add("tenhh", typeof(string));
            dt.Columns.Add("soluong", typeof(decimal));
            dt.Columns.Add("dvt", typeof(string));
            dt.Columns.Add("dongia", typeof(decimal));
            dt.Columns.Add("thanhtien", typeof(decimal));
            dt.Columns.Add("tiente", typeof(string));
            dt.Columns.Add("vat", typeof(int));
            dt.Columns.Add("ncc", typeof(string));
            dt.Columns.Add("baohanh", typeof(string));
            dt.Columns.Add("giaohang", typeof(string));
            dt.Columns.Add("thanhtoan", typeof(string));
            dt.Columns.Add("mota", typeof(string));
            dt.Columns.Add("created_at", typeof(DateTime));
            dt.Columns.Add("created_by", typeof(string));

            var stt_cell = 2;



            sheet.InsertRow(start_r, datapost.Count(), InsertOptionsType.FormatAsAfter);
            foreach (var record in datapost)
            {
                var chonmua = record.muahang_chonmua;
                decimal? tonggiatri = null;
                var tiente = "VND";
                var thoigiangiaohang = "";
                var thanhtoan = "";
                if (chonmua != null)
                {
                    tonggiatri = chonmua.tonggiatri;
                    tiente = chonmua.tiente;

                    thoigiangiaohang = chonmua.thoigiangiaohang;
                    thanhtoan = chonmua.thanhtoan;

                    var chitiet = chonmua.chitiet;

                    sheet.InsertRow(start_r, chitiet.Count(), InsertOptionsType.FormatAsAfter);
                    foreach (var item in chitiet)
                    {


                        DataRow dr1 = dt.NewRow();
                        //dr1["stt"] = (++stt);
                        dr1["name"] = record.name;
                        dr1["mahh"] = item.mahh;
                        dr1["tenhh"] = item.tenhh;
                        dr1["soluong"] = item.soluong;
                        dr1["dvt"] = item.dvt;
                        dr1["dongia"] = item.dongia ?? 0;
                        dr1["thanhtien"] = item.thanhtien_vat ?? 0;
                        dr1["tiente"] = tiente;
                        dr1["vat"] = item.vat ?? 0;
                        dr1["ncc"] = chonmua.ncc != null ? chonmua.ncc.tenncc : "";
                        dr1["baohanh"] = chonmua.baohanh;
                        dr1["giaohang"] = chonmua.thoigiangiaohang;
                        dr1["thanhtoan"] = chonmua.thanhtoan;
                        dr1["mota"] = record.note;
                        dr1["created_at"] = record.created_at;
                        dr1["created_by"] = record.user_created_by.FullName;
                        dt.Rows.Add(dr1);
                        start_r++;
                    }
                }



            }
            sheet.InsertDataTable(dt, false, 2, 1);

            workbook.SaveToFile("./wwwroot" + documentPath, ExcelVersion.Version2013);

            return Json(new { success = true, link = Domain + documentPath });
        }

        [HttpPost]
        public async Task<IActionResult> AddComment(MuahangCommentModel CommentModel, List<string> users_related)
        {
            System.Security.Claims.ClaimsPrincipal currentUser = User;
            string user_id = UserManager.GetUserId(currentUser); // Get user id:
            var user = await UserManager.GetUserAsync(currentUser); // Get user id:
            CommentModel.user_id = user_id;
            CommentModel.created_at = DateTime.Now;
            _context.Add(CommentModel);
            _context.SaveChanges();
            var files = Request.Form.Files;

            var items_comment = new List<MuahangCommentFileModel>();
            if (files != null && files.Count > 0)
            {
                var pathroot = _configuration["Source:Path_Private"] + "\\buy\\muahang\\" + CommentModel.muahang_id + "\\";
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
                    var filePath = _configuration["Source:Path_Private"] + "\\buy\\muahang\\" + CommentModel.muahang_id + "\\" + newName;
                    string url = "/private/buy/muahang/" + CommentModel.muahang_id + "/" + newName;
                    items_comment.Add(new MuahangCommentFileModel
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
                _context.Add(new MuahangCommentUserModel
                {
                    muahang_comment_id = CommentModel.id,
                    user_id = item
                });
                _context.SaveChanges();
            }
            ///lây user liên quan
            var muahang = _context.MuahangModel.Where(d => d.id == CommentModel.muahang_id).FirstOrDefault();
            var comments = _context.MuahangCommentModel.Where(d => d.muahang_id == CommentModel.muahang_id).Include(d => d.users_related).ToList();

            var users_related_id = new List<string>();
            users_related_id.Add(muahang.created_by);
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
                        link = Domain + "/muahang/edit/" + CommentModel.muahang_id,
                        text = text,
                        name = user.FullName
                    });

                var email = new EmailModel
                {
                    email_to = mail_string,
                    subject = "[Tin nhắn mới] " + muahang.name,
                    body = body,
                    email_type = "new_comment_purchase",
                    status = 1,
                    data_attachments = attach
                };
                _context.Add(email);
            }
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
            var comments_ctx = _context.MuahangCommentModel
                .Where(d => d.muahang_id == id);
            if (from_id != null)
            {
                comments_ctx = comments_ctx.Where(d => d.id < from_id);
            }
            List<MuahangCommentModel> comments = comments_ctx.OrderByDescending(d => d.id)
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
        public async Task<IActionResult> thongbao(int muahang_id, List<string> list_user, string note)
        {

            var data = _context.MuahangModel.Where(d => d.id == muahang_id).Include(d => d.chitiet).FirstOrDefault();
            //SEND MAIL
            if (list_user != null && list_user.Count() > 0 && data != null)
            {
                var stt = 1;
                foreach (var item in data.chitiet)
                {
                    item.stt = stt++;
                }
                var users_related_obj = _context.UserModel.Where(d => list_user.Contains(d.Id)).Select(d => d.Email).ToList();
                var mail_string = string.Join(",", users_related_obj.ToArray());
                string Domain = (HttpContext.Request.IsHttps ? "https://" : "http://") + HttpContext.Request.Host.Value;
                var body = _view.Render("Emails/Nhanhang",
                    new
                    {
                        link_logo = Domain + "/images/clientlogo_astahealthcare.com_f1800.png",
                        link = Domain + "/muahang/nhanhang/" + muahang_id,
                        date = data.date,
                        data = data.chitiet,
                        note = note
                    });

                var email = new EmailModel
                {
                    email_to = mail_string,
                    subject = "[Thông báo nhận hàng] " + data.name,
                    body = body,
                    email_type = "nhanhang_purchase",
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
        public async Task<IActionResult> thongbaothanhtoan(int muahang_id)
        {

            var data = _context.MuahangModel.Where(d => d.id == muahang_id).Include(d => d.chitiet).FirstOrDefault();
            //SEND MAIL
            if (data != null)
            {
                var stt = 1;
                foreach (var item in data.chitiet)
                {
                    item.stt = stt++;
                }

                //var users_related_obj = _context.UserModel.Where(d => list_user.Contains(d.Id)).Select(d => d.Email).ToList();
                var mail_string = "tram.nth@astahealthcare.com";
                string Domain = (HttpContext.Request.IsHttps ? "https://" : "http://") + HttpContext.Request.Host.Value;
                var body = _view.Render("Emails/Thanhtoan",
                    new
                    {
                        link_logo = Domain + "/images/clientlogo_astahealthcare.com_f1800.png",
                        link = Domain + "/muahang/edit/" + muahang_id,
                        date = data.date,
                        data = data.chitiet
                    });

                var email = new EmailModel
                {
                    email_to = mail_string,
                    subject = "[Thông báo thanh toán] " + data.name,
                    body = body,
                    email_type = "thanhtoan_purchase",
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

        public async Task<JsonResult> updatethanhtien()
        {
            return Json(new { });
            var muahang_ncc = _context.MuahangNccModel.Include(d => d.chitiet).ToList();
            foreach (var item in muahang_ncc)
            {
                var vat = item.vat;
                var chitiet = item.chitiet;
                item.thanhtien_vat = item.thanhtien + (item.thanhtien * vat / 100);
                _context.Update(item);
                _context.SaveChanges();
                foreach (var item2 in chitiet)
                {
                    item2.vat = vat;
                    item2.thanhtien_vat = item2.thanhtien + (item2.thanhtien * vat / 100);
                    _context.Update(item2);
                    _context.SaveChanges();
                }
            }
            return Json(muahang_ncc, new System.Text.Json.JsonSerializerOptions()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
            });

        }
        public async Task<JsonResult> updateUserNhanhang()
        {
            //return Json(new { });
            var muahang_chitiet = _context.MuahangChitietModel.Where(d => d.user_nhanhang_id == null).Include(d => d.dutru_chitiet).ThenInclude(d => d.dutru).ToList();
            foreach (var item in muahang_chitiet)
            {
                item.user_nhanhang_id = item.dutru_chitiet.dutru.created_by;

                _context.Update(item);
                _context.SaveChanges();

            }
            return Json(new { success = true }, new System.Text.Json.JsonSerializerOptions()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
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


        public class RawMuahangTonghop
        {
            public string bophan { get; set; }
            public decimal thanhtien { get; set; }
            public string tiente { get; set; }
            public string thanhtien_string
            {
                get
                {
                    CultureInfo vietnamCulture = new CultureInfo("vi-VN");
                    return thanhtien.ToString("#,##0.##", vietnamCulture) + " " + tiente;
                }
            }
        }
        public class RawMuahangDetails
        {
            public int stt { get; set; }

            public string tenhh { get; set; }
            public string mahh { get; set; }
            public string? dvt { get; set; }
            public string soluong { get; set; }
            public string? dongia { get; set; }
            public string? thanhtien { get; set; }
            public string? note { get; set; }
            public string? nhasx { get; set; }
            public string? tggh { get; set; }
            public string? tieuchuan { get; set; }
            public int? vat { get; set; }
        }
        public class RawFile
        {
            public string link { get; set; }
            public string note { get; set; }
            public List<MuahangDinhkemModel>? list_file { get; set; }
            //public string file_url { get; set; }

            public bool is_user_upload { get; set; }

            public DateTime? created_at { get; set; }

        }
    }
}
