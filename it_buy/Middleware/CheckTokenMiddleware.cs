﻿using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Diagnostics;
using Vue.Data;
using Vue.Models;

namespace Vue.Middleware
{
    public class CheckTokenMiddleware
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        // Lưu middlewware tiếp theo trong Pipeline
        private readonly RequestDelegate _next;
        public CheckTokenMiddleware(RequestDelegate next, IHttpContextAccessor httpContextAccessor)
        {
            _next = next;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task Invoke(HttpContext httpContext, ItContext _context, SignInManager<UserModel> _signInManager)
        {
            var listener = _context.GetService<DiagnosticSource>();
            (listener as DiagnosticListener).SubscribeWithAdapter(new CommandInterceptor());

            bool islogin = httpContext.User.Identity.IsAuthenticated;
            string Token = _httpContextAccessor.HttpContext.Request.Cookies["Auth-Token"];
            var path = (string)_httpContextAccessor.HttpContext.Request.Path;
            var except = new List<string>()
            {
                "/Identity/Account/AccessDenied",
                "/Identity/Account/Logout",
                "/Home",
                "/Api"
            };
            foreach (var item in except)
            {
                if (path.ToLower().StartsWith(item.ToLower()))
                {
                    islogin = true;
                    break;
                }
            }
            Console.WriteLine("Path: " + path);
            Console.WriteLine("CheckLogin: " + islogin);
            if (islogin)
            {
                await _next(httpContext);
            }
            else
            {
                //Console.WriteLine("CheckTokebMiddleware: " + Token);
                if (Token != null)
                {
                    var find = _context.TokenModel.Where(d => d.token == Token && d.vaild_to > DateTime.Now && d.deleted_at == null).FirstOrDefault();
                    if (find != null)
                    {
                        var email = find.email;
                        var user = _context.UserModel.Where(d => d.Email.ToLower() == email.ToLower() && d.deleted_at == null).FirstOrDefault();
                        if (user != null)
                        {
                            await _signInManager.SignInAsync(user, true);
                        }
                        else
                        {

                            httpContext.Response.Redirect("/Identity/Account/AccessDenied");
                        }
                    }
                }
                await _next(httpContext);
            }

        }
    }
}
