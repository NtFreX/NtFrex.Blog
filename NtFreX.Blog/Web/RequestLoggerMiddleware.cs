﻿using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using NtFreX.Blog.Data;
using NtFreX.Blog.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace NtFreX.Blog.Web
{
    public class RequestLoggerMiddleware
    {
        private readonly RequestDelegate next;

        public RequestLoggerMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task InvokeAsync(HttpContext context, Database database)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            await next.Invoke(context);
            stopwatch.Stop();

            var request = database.Monitoring.GetCollection<RequestModel>("request");
            await request.InsertOneAsync(new RequestModel
            {
                Date = DateTime.UtcNow,
                RemoteIp = context.Connection.RemoteIpAddress.ToString(),
                RequestHeader = ToJson(context.Request.Headers),
                StatusCode = context.Response.StatusCode,
                Success = IsSuccesStatusCode(context.Response.StatusCode.ToString()),
                IsAttack = Vulnerabilities.Any(x => x == context.Request.Path),
                LatencyInMs = stopwatch.ElapsedMilliseconds,
                Host = Environment.MachineName,
                System = "NtFrex.Blog",
                Path = context.Request.Path
            });
        }

        private string ToJson(IHeaderDictionary headers)
        {
            var jsonHeaders = new List<string>();
            foreach(var header in headers)
            {
                jsonHeaders.Add(header.Key.ToJson() + ":" + string.Join("; ", header.Value.ToArray()).ToJson());
            }
            return "{" + string.Join(", " + Environment.NewLine, jsonHeaders);
        }

        private bool IsSuccesStatusCode(string statusCode)
            => (statusCode.StartsWith("2") && statusCode.Length == 3)        // success
               || (statusCode.StartsWith("1") && statusCode.Length == 3)     // informational response
               || (statusCode.StartsWith("3") && statusCode.Length == 3);    // redirection



        private static string[] Vulnerabilities = new string[]
        {
                "/owa/auth/logon.aspx",
                "/owa/auth/signin.aspx",
                "/owa/auth/login.aspx",
                "//admin/config.php",
                "//xmlrpc.php",
                "/SQLite/main.php",
                "/SQLiteManager-1.2.4/main.php",
                "/SQLiteManager/main.php",
                "/admin.php",
                "/admin//config.php",
                "/administrator/index.php",
                "/agSearch/SQlite/main.php",
                "/blog/wp-login.php",
                "/composer.json",
                "/dup-installer/main.installer.php",
                "/index.php",
                "/main.php",
                "/sqlite/main.php",
                "/test/sqlite/SQLiteManager-1.2.0/SQLiteManager-1.2.0/main.php",
                "/vendor/phpunit/phpunit/src/Util/PHP/eval-stdin.php",
                "/vtigercrm/vtigerservice.php",
                "/wordpress/wp-login.php",
                "/wp-content/plugins/super-interactive-maps/sim-wp-admin/pages/import.php",
                "/wp-content/plugins/superlogoshowcase-wp/sls-wp-admin/pages/import.php",
                "/wp-content/plugins/superstorefinder-wp/ssf-wp-admin/pages/import.php",
                "/wp-login.php",
                "/wp/wp-login.php",
                "//2018/wp-includes/wlwmanifest.xml",
                "//2019/wp-includes/wlwmanifest.xml",
                "//2020/wp-includes/wlwmanifest.xml",
                "//blog/wp-includes/wlwmanifest.xml",
                "//cms/wp-includes/wlwmanifest.xml",
                "//media/wp-includes/wlwmanifest.xml",
                "//news/wp-includes/wlwmanifest.xml",
                "//shop/wp-includes/wlwmanifest.xml",
                "//site/wp-includes/wlwmanifest.xml",
                "//sito/wp-includes/wlwmanifest.xml",
                "//test/wp-includes/wlwmanifest.xml",
                "//web/wp-includes/wlwmanifest.xml",
                "//website/wp-includes/wlwmanifest.xml",
                "//wordpress/wp-includes/wlwmanifest.xml",
                "//wp-includes/wlwmanifest.xml",
                "//wp/wp-includes/wlwmanifest.xml",
                "//wp1/wp-includes/wlwmanifest.xml",
                "//wp2/wp-includes/wlwmanifest.xml",
                "/wp-content/",
                "/wp-content/plugins/wp-file-manager/readme.txt",
                "/.env",
                "//login_sid.lua",
                "/Autodiscover/Autodiscover.xml",
                "/mifs/.;/services/LogService",
                "/HNAP1/",
                "/Telerik.Web.UI.WebResource.axd",
                "/administrator/",
                "/admin",
                "/admin/",
                "/login",
                "/console/",
                "/api/jsonws/invoke",
                "/asset-manifest.json"
        };
    }
}
