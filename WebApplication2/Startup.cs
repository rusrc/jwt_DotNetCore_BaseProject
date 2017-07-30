using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WebApplication2.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace WebApplication2
{
    public class Startup
    {
        //1. FIRST ADD CONSTANTS
        public const string CONNECTION_STRING = "Data Source=localhost;Initial Catalog=CoreDbContext;Integrated Security=True";
        public const string JWT_ISSUER = "MyAuthServer";
        public const string JWT_AUDIENCE = "http://localhost:60719";
        public const string JWT_KEY = "b802bec8fd52b0a75f201d8b37274e1081c39b740293f765eae731f5a65ed1"; //Secret key
        public const int JWT_LIFETIME = 30; //JWT lifetime in minutes
        public static SymmetricSecurityKey JwtSymmetricSecurityKey
            => new SymmetricSecurityKey(Encoding.ASCII.GetBytes(JWT_KEY));

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            #region SQL Server 
            services.AddDbContext<CoreDbContext>(options => options.UseSqlServer(CONNECTION_STRING));
            #endregion

            #region Identity
            services.AddIdentity<User, IdentityRole>()
                    .AddEntityFrameworkStores<CoreDbContext>()
                    .AddDefaultTokenProviders(); 
            #endregion

            #region IOptions
            services.AddOptions();
            //services.Configure<ApplicationSetting>(Configuration.GetSection(nameof(ApplicationSetting)));
            #endregion
            // Add framework services.
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            //https://msdn.microsoft.com/en-us/library/system.identitymodel.tokens.tokenvalidationparameters(v=vs.114).aspx
            app.UseJwtBearerAuthentication(new JwtBearerOptions
            {
                AutomaticAuthenticate = true,
                AutomaticChallenge = true,
                TokenValidationParameters = new TokenValidationParameters
                {
                    // Gets or sets a value indicating whether the Issuer should be validated.
                    // укзывает, будет ли валидироваться издатель при валидации токена
                    ValidateIssuer = true,
                    // Gets or sets an issuer that is considered valid.
                    // строка, представляющая издателя
                    ValidIssuer = JWT_ISSUER,
                    // Gets or sets an audience that is considered valid.
                    // будет ли валидироваться потребитель токена
                    ValidateAudience = true,
                    // Set audience token
                    // установка потребителя токена
                    ValidAudience = JWT_AUDIENCE,
                    // JWT lifetime
                    // будет ли валидироваться время существования
                    ValidateLifetime = true,
                    // Set security key
                    // установка ключа безопасности
                    IssuerSigningKey = JwtSymmetricSecurityKey,
                    // Key validation
                    // валидация ключа безопасности
                    ValidateIssuerSigningKey = true,
                }
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                //app.UseWebpackDevMiddleware();
            }
            else
            {
                app.UseStatusCodePagesWithReExecute("/error/{0}");
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseMvc();

            //Default Index.html url if empty
            app.Use(async (context, next) =>
            {
                await next();

                if
                (
                    context.Response.StatusCode == StatusCodes.Status404NotFound &&
                    !Path.HasExtension(context.Request.Path.Value) &&
                    !context.Request.Path.Value.StartsWith("api")
                )
                {
                    context.Request.Path = "/Index.html";
                    context.Response.StatusCode = StatusCodes.Status200OK;
                    await next();
                }
            });
        }
    }
}
