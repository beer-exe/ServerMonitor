using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using ServerMonitorApp.API.Hubs;
using ServerMonitorApp.API.Middlewares;
using ServerMonitorApp.API.Services;
using ServerMonitorApp.Application;
using ServerMonitorApp.Application.Common.Interfaces;
using ServerMonitorApp.Infrastructure;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;

namespace ServerMonitorApp.API
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSignalR();

            services.AddTransient<IMonitorHubDispatcher, MonitorHubDispatcher>();

            services.AddInfrastructureServices(Configuration);
            services.AddApplicationServices(Configuration);

            services.AddControllers().AddJsonOptions(options => options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Description = "Enter JWT Token here",
                    Name = "Authorization",
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT"
                });

                options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
                {
                    {
                        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                        {
                            Reference = new Microsoft.OpenApi.Models.OpenApiReference
                            {
                                Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });
            });

            IConfigurationSection? jwtSettings = Configuration.GetSection("JwtSettings");
            byte[]? secretKey = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"] ?? "sGraw5@|K1aQFW+?fo.T*/fBI)4Jy8P60:wdRtncyO@KFme/2J&toDLz!U#/x$4kb6hIkq16Boo.wx(elXB>EySOik!^Vz%!%!L2URXr&8Ksmj*oWt&7As(b:jut9+|VUBM9OcJtfco[1Hzq;TsBY+kasYrzvu?Tm4FUcLvm9$EWW#A:Iv3fD{CE$f>uI4WKlA7zDrJJehF.f[|4CbA%k#e^v5A.[$J]vyo[wu%C=p1G[Q#%G{rrxJxCaD?c5}o}slmG1L1>&)xaRgGHUzU-)t,JtLzx?eMo=eqptS&{@OkQ=Z)PSorxKzaP=@I:w<0=U*d3lC+)plY,;$<pss)uvE1>jb8m?!$czGc]52sC,C{tmmRgd@)bQqybG&%GY).[e}8kGWk5-@86GA[WOy|7KmA}%Udbcv.X5)_3.-7xiq6,{=,4WVCrc#-:[8:/2&)Y;inTJDuqjgy@UNRN5/1zh;rA{$JGVPvOG7E<{nb*Gl%w,2K)ws7;Rp00:lNd-xC[");

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(secretKey),
                    ClockSkew = TimeSpan.Zero,

                    RoleClaimType = ClaimTypes.Role
                };
            });

            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder =>
                {
                    builder.SetIsOriginAllowed(_ => true)
                           .AllowAnyMethod()
                           .AllowAnyHeader()
                           .AllowCredentials();
                });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseRouting();
            app.UseCors("AllowAll");

            app.UseMiddleware<ErrorHandlerMiddleware>();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<MonitorHub>("/hubs/monitor");
            });
        }
    }
}
