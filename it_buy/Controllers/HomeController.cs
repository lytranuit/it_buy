
using Vue.Models;
using Microsoft.AspNetCore.Mvc;
using Vue.Data;
using System.Net.Mail;
using Vue.Services;
using System.Net.Mime;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System;
using iText.StyledXmlParser.Jsoup.Nodes;

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
                    var model = _context.MuahangModel.Where(d => d.id == muahang_tmp.id).Include(d => d.chitiet).FirstOrDefault();
                    if (model != null)
                    {
                        model.status_id = (int)Status.MuahangEsignSuccess;
                        model.pdf = muahang_tmp.pdf;
                        _context.Update(model);
                        queue.status_id = 2;
                        _context.Update(queue);
                        await _context.SaveChangesAsync();
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
                mail_string = d.user.Email;
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
    }
    class SuccesMail
    {
        public int success { get; set; }
        public Exception ex { get; set; }
    }
}
