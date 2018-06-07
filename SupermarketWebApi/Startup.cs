using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SupermarketWebApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Newtonsoft.Json.Serialization;

namespace SupermarketWebApi
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
            services.AddDbContext<SupermarketContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddMvc(setupAction =>
            {
                setupAction.ReturnHttpNotAcceptable = true;
                setupAction.OutputFormatters.Add(new XmlDataContractSerializerOutputFormatter());
                setupAction.InputFormatters.Add(new XmlDataContractSerializerInputFormatter());

                var jsonOutputFormatter = setupAction.OutputFormatters
                .OfType<JsonOutputFormatter>().FirstOrDefault();

                if (jsonOutputFormatter != null)
                {
                    jsonOutputFormatter.SupportedMediaTypes.Add("application/vnd.idp.hateoas+json");
                }
            })
            .AddJsonOptions(options =>
            {
                options.SerializerSettings.ContractResolver =
                new CamelCasePropertyNamesContractResolver();
            });
            services.AddScoped<ISupermarketRepository, SupermarketRepository>();

            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddScoped<IUrlHelper, UrlHelper>(implementationFactory =>
            {
                var actionContext =
                implementationFactory.GetService<IActionContextAccessor>().ActionContext;
                return new UrlHelper(actionContext);
            });

            services.AddTransient<IPropertyMappingService, SupermarketPropertyMappingService>();
            services.AddTransient<IProductPropertyMappingService, ProductPropertyMappingService>();
            services.AddTransient<IStaffMemberPropertyMappingService, StaffMemberPropertyMappingService>();
            services.AddTransient<IStockPropertyMappingService, StockPropertyMappingService>();

            services.AddTransient<ITypeHelperService, TypeHelperService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env )
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            // set a global 500 error for server errors
            else
            {
                app.UseExceptionHandler(appBuilder =>
                {
                    appBuilder.Run(async context =>
                    {
                        var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();

                        context.Response.StatusCode = 500;
                        await context.Response.WriteAsync("An Unexpected fault occured. Try again later");
                    });
                });
            }

            AutoMapper.Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<Models.Supermarket, DTO.SupermarketDTO>();
                cfg.CreateMap<Models.Product, DTO.ProductDTO>();
                cfg.CreateMap<Models.StaffMember, DTO.StaffMemberDTO>();
                cfg.CreateMap<Models.SupermarketStock, DTO.SupermarketStockDTO>();
                cfg.CreateMap<DTO.SupermarketForCreationDTO, Models.Supermarket>();
                cfg.CreateMap<DTO.ProductForCreationDTO, Models.Product>();
                cfg.CreateMap<DTO.StaffMemberForCreationDTO, Models.StaffMember>();
                cfg.CreateMap<DTO.SupermarketStockForCreationDTO, Models.SupermarketStock>();
                cfg.CreateMap<DTO.SupermarketForUpdateDTO, Models.Supermarket>();
                cfg.CreateMap<DTO.ProductForUpdateDTO, Models.Product>();
                cfg.CreateMap<DTO.StaffMemberForUpdateDTO, Models.StaffMember>();
                cfg.CreateMap<DTO.SupermarketStockForUpdateDTO, Models.SupermarketStock>();
            });

            app.UseMvc();
        }
    }
}
