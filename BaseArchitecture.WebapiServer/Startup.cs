using Autofac;
using BaseArchitecture.IServices;
using BaseArchitecture.Services;
using ConsulExtensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Linq;
using System.Text;

namespace BaseArchitecture.WebapiServer
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
            services.AddControllers();
            services.AddConsul(Configuration);
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
            });

            #region jwtУ��
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,//�Ƿ���֤Issuer
                    ValidateAudience = false,//�Ƿ���֤Audience
                    ValidateLifetime = true,//�Ƿ���֤ʧЧʱ��
                    ValidateIssuerSigningKey = true,//�Ƿ���֤SecurityKey
                    ClockSkew = TimeSpan.Zero,//
                    //ValidAudience = this.Configuration["audience"],//Audience
                    //ValidIssuer = this.Configuration["issuer"],//Issuer���������ǰ��ǩ��jwt������һ��
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this.Configuration["JWT:SecurityKey"])),//�õ�SecurityKey
                    //AudienceValidator = (m, n, z) =>
                    //{
                    //    return m != null && m.FirstOrDefault().Equals(this.Configuration["audience"]);
                    //},//�Զ���У����򣬿����µ�¼��֮ǰ����Ч
                };
            });
            #endregion

        }

        public void ConfigureContainer(ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterType<JWTService>().As<IJWTService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            int port;
            if (env.IsDevelopment())
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine($"IsDevelopment");
                app.UseDeveloperExceptionPage();
                port = Convert.ToInt32(this.Configuration["port"]);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine($"IsNotDevelopment");
                var server = app.ApplicationServices.GetRequiredService<IServer>();
                port = new Uri(server.Features.Get<IServerAddressesFeature>().Addresses.FirstOrDefault()).Port;
            }

            app.UseHttpsRedirection();

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });

            app.UseAuthentication();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
            app.UseConsul(port);
        }
    }
}