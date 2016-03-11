using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Web;
using System.Web.Http;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Nancy;
using Owin;
using Pluralsight.Owin.Demo.Middleware;
using Nancy.Owin;

namespace Pluralsight.Owin.Demo
{
    public class Startup
    {
        public static void Configuration(IAppBuilder app)
        {
            app.UseDebugMiddleware(new DebugMiddlewareOptions
            {
                OnIncomingRequest = (ctx) =>
                {
                    var watch = new Stopwatch();
                    watch.Start();
                    ctx.Environment["DebugStopwatch"] = watch;
                },
                OnOutgoingRequest = (ctx) =>
                {
                    var watch = (Stopwatch)ctx.Environment["DebugStopwatch"];
                    watch.Stop();
                    Debug.WriteLine("Request took: " + watch.ElapsedMilliseconds + " ms");
                }
            });

            app.UseCookieAuthentication(new CookieAuthenticationOptions()
            {
                AuthenticationType = "ApplicationCookie",
                LoginPath = new PathString("/Auth/Login")
            });

            app.Use(async (ctx, next) =>
            {
                if (ctx.Authentication.User.Identity.IsAuthenticated)
                {
                    Debug.WriteLine("User: " + ctx.Authentication.User.Identity.Name);
                } else {
                    Debug.WriteLine("User Not Authenticated");
                }
                await next();
            });

            var config = new HttpConfiguration();
            config.MapHttpAttributeRoutes();
            app.UseWebApi(config);

            app.Map("/nancy", mappedApp => { mappedApp.UseNancy(); });

            // Nancy does something to MVC making the call below fail
            //app.UseNancy(conf =>
            //{
            //    conf.PassThroughWhenStatusCodesAre(HttpStatusCode.NotFound);
            //});

            //app.Use(async (ctx, next) =>
            //{
            //    await ctx.Response.WriteAsync("<html><head></head><body>Hello World</body></html>");
            //});
        }
    }
}