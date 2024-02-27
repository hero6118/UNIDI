 using AspNetCoreRateLimit;
using Core;
using Core.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PayPalCheckoutSdk.Core;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Text;

namespace UNIONTEK.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Inject Application Options
            services.Configure<ApplicationOptions>(Configuration.GetSection("ApplicationOptions"));
            services.AddCors();
            services.AddHttpClient();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "API",
                    Version = "v1",
                    Description = "API Swagger Surface",
                });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = @"JWT Authorization header using the Bearer scheme. <br />
                      Enter 'Bearer' [space] and then your token in the text input below.<br />
                      Example: 'Bearer 12345abcdef'",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header,

                        },
                        new List<string>()
                    }
                });
            });

            services.AddAuthentication(o =>
            {
                o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(o =>
            {
                o.RequireHttpsMetadata = false;
                o.SaveToken = true;
                o.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(C_Config.KeyJWTAPI)),
                    ClockSkew = TimeSpan.Zero
                };
                o.Events = new JwtBearerEvents
                {
                    OnChallenge = async (context) =>
                    {
                        //context.Response.StatusCode = 200;
                        //context.HttpContext.Response.Redirect("/users/notauthorized");
                        var content = "{ \"status\": false, \"message\": \"Login session has expired, please login again\", \"result\": null }";
                        context.HttpContext.Response.ContentType = "application/json";
                        await context.HttpContext.Response.WriteAsync(content);
                    }
                };
            });
            services.AddControllersWithViews()
                .AddNewtonsoftJson(options =>
                options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
            );

            services.AddSwaggerGen(c => { c.EnableAnnotations(); });
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });

            // needed to load configuration from appsettings.json
            services.AddOptions();

            // needed to store rate limit counters and ip rules
            services.AddMemoryCache();

            //load general configuration from appsettings.json
            services.Configure<IpRateLimitOptions>(Configuration.GetSection("IpRateLimiting"));

            //load ip rules from appsettings.json
            //services.Configure<IpRateLimitPolicies>(Configuration.GetSection("IpRateLimitPolicies"));

            // inject counter and rules stores
            services.AddInMemoryRateLimiting();
            //services.AddDistributedRateLimiting<AsyncKeyLockProcessingStrategy>();
            //services.AddDistributedRateLimiting<RedisProcessingStrategy>();
            //services.AddRedisRateLimiting();

            // configuration (resolvers, counter key builders)
            services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
            // Add framework services.
            //services.AddMvc();
            services.AddMvc(options => { options.InputFormatters.Insert(0, new RawJsonBodyInputFormatter()); });
            services.Configure<FormOptions>(o =>
            {
                o.MultipartBodyLengthLimit = int.MaxValue;
                o.MemoryBufferThreshold = int.MaxValue;
            });
            services.Configure<KestrelServerOptions>(options =>
             {
                 options.AllowSynchronousIO = true;
                 options.Limits.MaxRequestBodySize = int.MaxValue;
             });
            services.Configure<IISServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
                options.MaxRequestBodySize = int.MaxValue;
            });


            services.AddDistributedMemoryCache();
            services.AddSession(options => {
                options.IdleTimeout = TimeSpan.FromMinutes(20);//You can set Time  
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // test IP ADDRESS
            services.AddHttpContextAccessor();

            // test IDENtity
            services.AddAuthorization(options =>
            {
                options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("AdminAll"));
            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseCors(builder => builder.WithOrigins("*").AllowAnyHeader().AllowAnyMethod());
            app.UseSwagger();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseHttpsRedirection();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
                    c.ConfigObject.AdditionalItems.Add("syntaxHighlight", false);
                    c.ConfigObject.AdditionalItems.Add("theme", "agate");
                    c.DocumentTitle = "API";
                    c.DocExpansion(DocExpansion.None);
                });
            }
            else
            {
                app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(env.ContentRootPath),
                    RequestPath = new PathString("")
                });

                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
                    c.ConfigObject.AdditionalItems.Add("syntaxHighlight", false);
                    c.ConfigObject.AdditionalItems.Add("theme", "agate");
                    c.DocumentTitle = "API";
                    c.DocExpansion(DocExpansion.None);
                    c.RoutePrefix = string.Empty;
                });
            }
            app.UseIPFilter();
            app.UseIpRateLimiting();
            app.UseSession();
            //app.UseHttpsRedirection(); // Cloudflare

            //app.Use(async (context, next) =>
            //{
            //    context.Response.Headers.Add("X-Developed-By", "UNIONTEK TEAM");
            //    context.Response.Headers.Add("Server", "UNIONTEK");
            //    context.Response.Headers["X-Powered-By"] = "UNIONTEK COMPANY";

            //    if (!context.Request.IsHttps && context.Request.Host.ToString().EndsWith("uto.vn"))
            //    {
            //        var withHttps = "https://" + context.Request.Host + context.Request.Path;
            //        context.Response.Redirect(withHttps);
            //        await context.Response.CompleteAsync();
            //    } 
            //    else
            //    {
            //        await next();
            //        await context.Response.CompleteAsync();
            //    }
            //});
            app.UseAuthentication();
            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            //app.UseCors(builder =>
            //    builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()
            //);
        }
    }
}
