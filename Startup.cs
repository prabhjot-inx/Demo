using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
// using Demo.GraphQLC.Users;
using Demo.Data.Entities;
using Demo.Logics;
using Demo.Resolvers;
using Demo.Shared;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Demo
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            _env = env;
        }

        private readonly IWebHostEnvironment _env;

        public IConfiguration Configuration { get; }
        public void ConfigureServices(IServiceCollection services)
        {
            

            services.AddGraphQLServer()
            .AddQueryType<QueryResolver>()
            .AddMutationType<MutationResolver>()
            .AddAuthorization()
            .ModifyRequestOptions(opt => opt.IncludeExceptionDetails = _env.IsDevelopment()); // for logging error
            
            services.AddDbContext<AuthContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
            services.AddScoped<IAuthLogic, AuthLogic>();
            services.Configure<TokenSettings>(Configuration.GetSection("TokenSettings"));
            
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options => 
            {
                var tokenSettings = Configuration.GetSection("TokenSettings").Get<TokenSettings>();
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = tokenSettings.Issuer,
                    ValidateIssuer = true,
                    ValidAudience = tokenSettings.Audience,
                    ValidateAudience = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenSettings.Key)),
                    ValidateIssuerSigningKey = true
                };
            });
            services.AddAuthorization();
            services.AddAuthorization(options => {
                options.AddPolicy("roles-policy", policy => {
                    policy.RequireRole(new string[]{"admin","super-admin"});
                });
                options.AddPolicy("claim-policy-1", policy => {
                    policy.RequireClaim("LastName");
                });
                options.AddPolicy("claim-policy-2", policy=>{
                    policy.RequireClaim("LastName",new string[]{"Prabh","Test"});
                });
            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGraphQL();
                // endpoints.MapControllers();
                // endpoints.MapGet("/", async context =>
                // {
                //     await context.Response.WriteAsync("Hello World!");
                // });
            });
        }
    }
}
