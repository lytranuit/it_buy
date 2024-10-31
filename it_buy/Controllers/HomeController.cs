
using Vue.Models;
using Microsoft.AspNetCore.Mvc;
using Vue.Data;
using System.Net.Mail;
using Vue.Services;
using System.Net.Mime;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Spire.Xls;
using System.Data;

namespace Vue.Controllers
{

    public class HomeController : Controller
    {
        protected readonly ItContext _context;
        private readonly ViewRender _view;


        public HomeController(ItContext context, ViewRender view)
        {
            _context = context;
            _view = view;
            var listener = _context.GetService<DiagnosticSource>();
            (listener as DiagnosticListener).SubscribeWithAdapter(new CommandInterceptor());
        }

        public JsonResult Index()
        {
            return Json(new { test = 1, message = DateTime.Now });

        }

        private SuccesMail SendMail(string to, string subject, string body, List<string> attachments)
        {
            try
            {

                string[] list_to = to.Split(",");

                MailMessage message = new MailMessage();
                message.From = new MailAddress("pymepharco.mail@gmail.com", "Pymepharco System");
                //message.From = new MailAddress("daolytran@pymepharco.com", "Pymepharco System");
                foreach (string str in list_to)
                {
                    message.To.Add(new MailAddress(str));
                }
                message.Subject = subject;
                message.Body = body;
                message.BodyEncoding = System.Text.Encoding.UTF8;
                message.SubjectEncoding = System.Text.Encoding.UTF8;
                message.IsBodyHtml = true;
                foreach (var attach in attachments)
                {
                    // Create  the file attachment for this email message.
                    Attachment data = new Attachment("." + attach, MediaTypeNames.Application.Pdf);
                    // Add time stamp information for the file.
                    ContentDisposition disposition = data.ContentDisposition;
                    disposition.CreationDate = System.IO.File.GetCreationTime(attach);
                    disposition.ModificationDate = System.IO.File.GetLastWriteTime(attach);
                    disposition.ReadDate = System.IO.File.GetLastAccessTime(attach);
                    // Add the file attachment to this email message.

                    message.Attachments.Add(data);
                }

                SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
                //SmtpClient client = new SmtpClient("mail.pymepharco.com", 993);
                client.EnableSsl = true;
                client.UseDefaultCredentials = false;
                client.Credentials = new System.Net.NetworkCredential("pymepharco.mail@gmail.com", "xenrezrhmvueqmvw");
                //client.Credentials = new System.Net.NetworkCredential("daolytran@pymepharco.com", "Asd12345");
                client.Send(message);
            }
            catch (Exception ex)
            {
                return new SuccesMail { ex = ex, success = 0 };
            }
            return new SuccesMail { success = 1 };
        }


        public async Task<JsonResult> cronjob()
        {
            var queues = _context.QueueModel.Where(d => d.status_id == 1).OrderBy(d => d.created_at).ToList();
            foreach (var queue in queues)
            {
                var type = queue.type;
                if (type == "create_esign_dutru_return")
                {
                    var dutru_tmp = queue.valueQ.dutru;
                    var model = _context.DutruModel.Where(d => d.id == dutru_tmp.id).FirstOrDefault();
                    if (model != null)
                    {
                        model.status_id = (int)Status.Esign;
                        model.activeStep = 1;
                        model.esign_id = dutru_tmp.esign_id;
                        model.code = dutru_tmp.code;
                        _context.Update(model);
                        queue.status_id = 2;
                        _context.Update(queue);
                        await _context.SaveChangesAsync();
                    }
                }
                else if (type == "esign_dutru_failed")
                {
                    var dutru_tmp = queue.valueQ.dutru;
                    var model = _context.DutruModel.Where(d => d.id == dutru_tmp.id).FirstOrDefault();
                    if (model != null)
                    {
                        model.status_id = (int)Status.EsignError;
                        _context.Update(model);
                        queue.status_id = 2;
                        _context.Update(queue);
                        await _context.SaveChangesAsync();
                    }
                }
                else if (type == "esign_dutru_success")
                {
                    var dutru_tmp = queue.valueQ.dutru;
                    var model = _context.DutruModel.Where(d => d.id == dutru_tmp.id).Include(d => d.chitiet).FirstOrDefault();
                    if (model != null)
                    {
                        model.status_id = (int)Status.EsignSuccess;
                        model.pdf = dutru_tmp.pdf;
                        _context.Update(model);
                        //var chitiet = model.chitiet;
                        //foreach (var item in chitiet)
                        //{
                        //    item.status_id = 2;
                        //}
                        //_context.UpdateRange(chitiet);
                        queue.status_id = 2;
                        _context.Update(queue);
                        await _context.SaveChangesAsync();
                    }
                }
                else if (type == "create_esign_muahang_return")
                {
                    var muahang_tmp = queue.valueQ.muahang;
                    var model = _context.MuahangModel.Where(d => d.id == muahang_tmp.id).FirstOrDefault();
                    if (model != null)
                    {
                        model.status_id = (int)Status.MuahangEsign;
                        model.activeStep = 1;
                        model.esign_id = muahang_tmp.esign_id;
                        model.code = muahang_tmp.code;
                        _context.Update(model);
                        queue.status_id = 2;
                        _context.Update(queue);
                        await _context.SaveChangesAsync();
                    }
                }
                else if (type == "esign_muahang_failed")
                {
                    var muahang_tmp = queue.valueQ.muahang;
                    var model = _context.MuahangModel.Where(d => d.id == muahang_tmp.id).FirstOrDefault();
                    if (model != null)
                    {
                        model.status_id = (int)Status.MuahangEsignError;
                        _context.Update(model);
                        queue.status_id = 2;
                        _context.Update(queue);
                        await _context.SaveChangesAsync();
                    }
                }
                else if (type == "esign_muahang_success")
                {
                    var muahang_tmp = queue.valueQ.muahang;
                    var model = _context.MuahangModel.Where(d => d.id == muahang_tmp.id).FirstOrDefault();
                    if (model != null)
                    {
                        model.status_id = (int)Status.MuahangEsignSuccess;
                        model.pdf = muahang_tmp.pdf;
                        model.pay_at = DateTime.Now;
                        _context.Update(model);
                        queue.status_id = 2;
                        _context.Update(queue);
                        await _context.SaveChangesAsync();

                        ////Tạo DNMH con
                        if (model.is_multiple_ncc == true)
                        {

                            var nccs = _context.MuahangNccModel.Where(d => d.muahang_id == model.id && d.chonmua == true).Include(d => d.chitiet).Include(d => d.dinhkem).ToList();
                            foreach (var ncc in nccs)
                            {
                                ///MUAHANG
                                var muahang_data = new MuahangModel()
                                {
                                    name = model.name,
                                    status_id = (int)Status.MuahangEsignSuccess,
                                    code = model.code,
                                    type_id = model.type_id,
                                    created_by = model.created_by,
                                    created_at = DateTime.Now,
                                    note = model.note,
                                    note_chonmua = model.note_chonmua,
                                    pdf = model.pdf,
                                    esign_id = model.esign_id,
                                    date = model.date,
                                    parent_id = model.id
                                };
                                _context.Add(muahang_data);
                                await _context.SaveChangesAsync();
                                var list_chitiet = new Dictionary<int, int>();
                                var model_chitiet = _context.MuahangChitietModel.Where(d => d.muahang_id == model.id).ToList();
                                foreach (var item in model_chitiet)
                                {
                                    list_chitiet.Add(item.id, item.dutru_chitiet_id);
                                }
                                ////MUAHANG DINHKEM
                                var model_dinhkem = _context.MuahangDinhkemModel.Where(d => d.muahang_id == model.id && d.deleted_at == null).ToList();
                                foreach (var item in model_dinhkem)
                                {

                                    var muahang_dinhkem_data = new MuahangDinhkemModel()
                                    {
                                        muahang_id = muahang_data.id,
                                        name = item.name,
                                        ext = item.ext,
                                        url = item.url,
                                        mimeType = item.mimeType,
                                        created_at = DateTime.Now,
                                        created_by = item.created_by,
                                    };
                                    _context.Add(muahang_dinhkem_data);
                                    await _context.SaveChangesAsync();
                                }
                                ////NCC
                                var muahang_ncc = new MuahangNccModel()
                                {
                                    muahang_id = muahang_data.id,
                                    ncc_id = ncc.ncc_id,
                                    chonmua = true,
                                    thoigiangiaohang = ncc.thoigiangiaohang,
                                    dapung = ncc.dapung,
                                    baohanh = ncc.baohanh,
                                    thanhtoan = ncc.thanhtoan,
                                    tonggiatri = ncc.tonggiatri,
                                    thanhtien = ncc.thanhtien,
                                    thanhtien_vat = ncc.thanhtien_vat,
                                    tienvat = ncc.tienvat,
                                    phigiaohang = ncc.phigiaohang,
                                    vat = ncc.vat,
                                    tiente = ncc.tiente,
                                    quidoi = ncc.quidoi,
                                    is_vat = ncc.is_vat,
                                    ck = ncc.ck,
                                };
                                _context.Add(muahang_ncc);
                                await _context.SaveChangesAsync();


                                muahang_data.muahang_chonmua_id = muahang_ncc.id;
                                _context.Update(muahang_data);
                                await _context.SaveChangesAsync();

                                var chitiet = ncc.chitiet;
                                foreach (var item in chitiet)
                                {
                                    var dutru_chitiet_id = list_chitiet[item.muahang_chitiet_id];
                                    var muahang_chitiet_data1 = new MuahangChitietModel()
                                    {
                                        muahang_id = muahang_data.id,
                                        dutru_chitiet_id = dutru_chitiet_id,
                                        //hh_id = item.hh_id,
                                        quidoi = item.quidoi,
                                        soluong = item.soluong,
                                        note = item.note,
                                        mahh = item.mahh,
                                        tenhh = item.tenhh,
                                        dvt = item.dvt,
                                        dvt_dutru = item.dvt_dutru,
                                    };
                                    _context.Add(muahang_chitiet_data1);
                                    await _context.SaveChangesAsync();

                                    var muahang_chitiet_data = new MuahangNccChitietModel()
                                    {
                                        muahang_ncc_id = muahang_ncc.id,
                                        muahang_chitiet_id = muahang_chitiet_data1.id,
                                        //hh_id = item.hh_id,
                                        quidoi = item.quidoi,
                                        dvt_dutru = item.dvt_dutru,
                                        soluong = item.soluong,
                                        dongia = item.dongia,
                                        thanhtien = item.thanhtien,
                                        thanhtien_vat = item.thanhtien_vat,
                                        vat = item.vat,
                                        note = item.note,
                                        mahh = item.mahh,
                                        tenhh = item.tenhh,
                                        dvt = item.dvt,
                                    };
                                    _context.Add(muahang_chitiet_data);
                                    await _context.SaveChangesAsync();
                                }
                                var dinhkems = ncc.dinhkem.Where(d => d.deleted_at == null).ToList();
                                foreach (var item in dinhkems)
                                {
                                    var muahang_ncc_dinhkem_data = new MuahangNccDinhkemModel()
                                    {
                                        muahang_ncc_id = muahang_ncc.id,
                                        name = item.name,
                                        url = item.url,
                                        ext = item.ext,
                                        mimeType = item.mimeType,
                                        created_at = DateTime.Now,
                                        created_by = item.created_by,
                                    };
                                    _context.Add(muahang_ncc_dinhkem_data);
                                    await _context.SaveChangesAsync();
                                }

                            }
                        }
                    }
                }
            }
            ////finish mua hàng
            var muahang_list = _context.MuahangModel.Where(d => d.deleted_at == null && d.date_finish == null && d.is_nhanhang == true && d.is_thanhtoan == true)
                .Include(d => d.chitiet).ThenInclude(d => d.dutru_chitiet)
                .Include(d => d.chitiet).ThenInclude(d => d.muahang_ncc_chitiet)
                .ToList();
            foreach (var item in muahang_list)
            {
                item.date_finish = DateTime.Now;
                _context.Update(item);
                _context.SaveChanges();
                /////
                //var chitiet_no_mahh = item.chitiet.Where(d => d.mahh == null).ToList();
                //foreach (var c in chitiet_no_mahh)
                //{
                //    //////tao ma
                //    var hh = new MaterialModel()
                //    {
                //        nhom = "Khac",
                //        mahh = "HH-" + new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds(),
                //        tenhh = c.tenhh,
                //        dvt = c.dvt,
                //    };
                //    _context.Add(hh);
                //    _context.SaveChanges();
                //    hh.mahh = "HH-" + hh.id;
                //    _context.Update(hh);
                //    _context.SaveChanges();
                //    /////update chitiet muahang
                //    c.hh_id = "m-" + hh.id;
                //    c.mahh = hh.mahh;
                //    _context.Update(c);
                //    _context.SaveChanges();
                //    ///Update chitiet ncc
                //    foreach (var ncc_c in c.muahang_ncc_chitiet)
                //    {
                //        ncc_c.mahh = hh.mahh;
                //        ncc_c.hh_id = "m-" + hh.id;

                //        _context.Update(ncc_c);
                //        _context.SaveChanges();
                //    }

                //    ///// update chitiet dutru
                //    c.dutru_chitiet.mahh = hh.mahh;
                //    c.dutru_chitiet.hh_id = "m-" + hh.id;

                //    _context.Update(c.dutru_chitiet);
                //    _context.SaveChanges();


                //}

            }



            return Json(new { success = true });
        }
        public async Task<JsonResult> cronjobDaily()
        {
            ///Check chờ thanh toán gửi mail
            var customerData = _context.MuahangModel
                .Where(d => d.deleted_at == null && d.date_finish == null && d.is_dathang == true && (d.is_thanhtoan == null || d.is_thanhtoan == false) && (d.loaithanhtoan == "tra_truoc" || (d.loaithanhtoan == "tra_sau" && d.is_nhanhang == true)))
                .OrderByDescending(d => d.id)
                .Include(d => d.muahang_chonmua)
                .ToList();
            var mail_string = "tram.nth@astahealthcare.com";
            string Domain = (HttpContext.Request.IsHttps ? "https://" : "http://") + HttpContext.Request.Host.Value;
            var body = _view.Render("Emails/RemindThanhtoan",
                new
                {
                    link_logo = Domain + "/images/clientlogo_astahealthcare.com_f1800.png",
                    link = Domain + "/muahang/thanhtoan",
                    Domain = Domain,
                    data = customerData
                });

            var email = new EmailModel
            {
                email_to = mail_string,
                subject = "[Nhắc nhở] Các đề nghị đang chờ thanh toán",
                body = body,
                email_type = "thanhtoan_purchase",
                status = 1,
            };
            _context.Add(email);
            _context.SaveChanges();

            ///Check chờ nhận hàng gửi mail
            var customerData1 = _context.MuahangModel
                .Where(d => d.deleted_at == null && d.is_dathang == true && d.date < DateTime.Now && d.date_finish == null && (d.is_nhanhang == null || d.is_nhanhang == false)
                && (d.loaithanhtoan == "tra_sau" || (d.loaithanhtoan == "tra_truoc" && d.is_thanhtoan == true)))
                .Select(d => d.id)
                .ToList();
            var chitiet = _context.MuahangChitietModel.Where(d => customerData1.Contains(d.muahang_id) && d.date_nhanhang == null)
                .Include(d => d.muahang)
                .Include(d => d.dutru_chitiet)
                .ThenInclude(d => d.dutru)
                .ThenInclude(d => d.user_created_by).ToList();
            var data_nhanhang = chitiet.GroupBy(d => d.dutru_chitiet.dutru.user_created_by).Select(d => new
            {
                user = d.Key,
                list = d.Select(e => new
                {
                    hanghoa = e.mahh + "-" + e.tenhh,
                    soluong = e.soluong.Value.ToString("#,##0.##") + " " + e.dvt,
                    dutru = e.dutru_chitiet.dutru.code + "-" + e.dutru_chitiet.dutru.name,
                    dnmh = e.muahang.code + "-" + e.muahang.name,
                    ngaygiaohang = e.muahang.date.Value.ToString("dd/MM/yyyy"),
                    muahang_id = e.muahang_id
                }).ToList(),
            }).ToList();
            foreach (var d in data_nhanhang)
            {
                var user = d.user;
                if (user.deleted_at != null || (user.LockoutEnd != null && user.LockoutEnd >= DateTime.Now))
                    continue;
                mail_string = user.Email;
                Domain = (HttpContext.Request.IsHttps ? "https://" : "http://") + HttpContext.Request.Host.Value;
                body = _view.Render("Emails/RemindNhanhang",
                    new
                    {
                        link_logo = Domain + "/images/clientlogo_astahealthcare.com_f1800.png",
                        Domain = Domain,
                        data = d.list
                    });

                email = new EmailModel
                {
                    email_to = mail_string,
                    subject = "[Nhắc nhở] Nhận hàng hóa",
                    body = body,
                    email_type = "remind_nhanhang_purchase",
                    status = 1,
                };
                _context.Add(email);
                _context.SaveChanges();
            }

            return Json(new { success = true });
        }

        public async Task<JsonResult> export()
        {
            var muahang_ncc = _context.MuahangNccModel.Include(d => d.ncc).Include(d => d.muahang).Where(d => d.muahang.deleted_at == null && d.muahang.type_id == 3).ToList();
            var ncc = muahang_ncc.GroupBy(d => d.ncc).Select(d => d.Key).ToList();

            var chitiet_dutru = _context.DutruChitietModel.Include(d => d.dutru).Where(d => d.dutru.deleted_at == null && d.dutru.type_id == 3)
                .Include(d => d.muahang_chitiet).ThenInclude(d => d.muahang)
                .Include(d => d.muahang_chitiet).ThenInclude(d => d.muahang_ncc_chitiet).ThenInclude(d => d.muahang_ncc).ThenInclude(d => d.ncc).OrderBy(d => d.dutru.created_at).ToList();

            var viewPath = "wwwroot/report/excel/Book1.xlsx";
            var documentPath = "/tmp/" + DateTime.Now.ToFileTimeUtc() + ".xlsx";
            string Domain = (HttpContext.Request.IsHttps ? "https://" : "http://") + HttpContext.Request.Host.Value;

            Workbook workbook = new Workbook();
            workbook.LoadFromFile(viewPath);

            Worksheet sheet = workbook.Worksheets[0];
            sheet.InsertColumn(18, ncc.Count() * 7, InsertOptionsType.FormatAsAfter);
            var start_c = 18;
            var stt = 0;
            foreach (var item in ncc)
            {
                //if (stt >= (ncc.Count() - 5))
                //    continue;
                item.start_c = start_c;
                var nRow = sheet.Rows[0];
                CellRange range = sheet.Range[1, start_c, 1, start_c + 6];  // Hàng 1, cột 5 đến hàng cuối cùng, cột 7

                range.Merge();

                var cell = nRow.Columns[start_c - 1];
                cell.Value = item.tenncc;
                var nRow2 = sheet.Rows[1];
                nRow2.Columns[start_c - 1].Value = "Code";
                nRow2.Columns[start_c].Value = "Mô tả";
                nRow2.Columns[start_c + 1].Value = "NSX";
                nRow2.Columns[start_c + 2].Value = "Đơn vị tính";
                nRow2.Columns[start_c + 3].Value = "Đơn giá";
                nRow2.Columns[start_c + 4].Value = "Dự kiến thời gian giao hàng";
                nRow2.Columns[start_c + 5].Value = "Ghi chú";

                start_c = start_c + 7;
            }
            start_c = 18;
            var start_r = 2;
            sheet.InsertRow(start_r + 1, chitiet_dutru.Count(), InsertOptionsType.FormatAsAfter);
            foreach (var item in chitiet_dutru)
            {
                stt++;
                var nRow = sheet.Rows[start_r];
                nRow.Columns[0].NumberValue = stt;
                nRow.Columns[1].Value = item.tenhh;
                nRow.Columns[2].Value = item.note;
                nRow.Columns[3].NumberValue = (double)item.soluong;
                nRow.Columns[4].Value = item.dvt;
                nRow.Columns[6].Value = item.dutru.note;
                nRow.Columns[7].Value = "Chưa mua";
                var muahang_chitiet = item.muahang_chitiet;
                if (muahang_chitiet.Count() > 0)
                {
                    nRow.Columns[7].Value = "Đang làm đề nghị mua hàng";
                    var muahang = muahang_chitiet[0].muahang;
                    var is_dathang = muahang.is_dathang;
                    if (is_dathang == true)
                    {
                        nRow.Columns[7].Value = "Đang đặt hàng";
                    }
                    var is_nhanhang = muahang.is_nhanhang;
                    if (is_nhanhang == true)
                    {
                        nRow.Columns[7].Value = "Đã nhận hàng";
                    }
                }

                foreach (var item1 in muahang_chitiet)
                {
                    foreach (var item2 in item1.muahang_ncc_chitiet)
                    {
                        var dongia = item2.dongia;
                        var dvt = item2.dvt;
                        var muahang_ncc1 = item2.muahang_ncc;
                        var ncc_id = muahang_ncc1.ncc_id;
                        var giaohang = muahang_ncc1.thoigiangiaohang;
                        var findncc = ncc.Where(d => d.id == ncc_id).FirstOrDefault();
                        var start_c1 = findncc.start_c.Value;

                        nRow.Columns[start_c1 + 2].Value = dvt;
                        nRow.Columns[start_c1 + 3].NumberValue = (double)dongia;
                        nRow.Columns[start_c1 + 3].NumberFormat = "#,##0";
                        nRow.Columns[start_c1 + 4].Value = giaohang;

                    }
                }
                //nRow.Columns[start_c + 2].Value



                start_r++;
            }
            //sheet.InsertRow(5, 10, InsertOptionsType.FormatAsBefore);
            //var start_r = 5;
            //var stt = 0;
            //foreach (var d in muahang_ncc)
            //{

            //    var nRow = sheet.Rows[start_r];

            //    nRow.Cells[0].NumberValue = ++stt;
            //    //start_r++;
            //}

            workbook.SaveToFile("./wwwroot" + documentPath, ExcelVersion.Version2013);

            //var congthuc_ct = _QLSXcontext.Congthuc_CTModel.Where()
            var jsonData = new { success = true, link = documentPath };
            return Json(jsonData);
        }
    }
    class SuccesMail
    {
        public int success { get; set; }
        public Exception ex { get; set; }
    }
}
