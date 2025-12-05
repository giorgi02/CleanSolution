//using Microsoft.AspNetCore.Authentication.JwtBearer;
//using Microsoft.OpenApi;

//namespace Presentation.WebApi.Extensions.Configurations;

//public static class SwaggerConfiguration
//{
//    private readonly static string[] Options = ["CleanSolution v1"];

//    // services
//    public static void AddSwaggerServices(this IServiceCollection services)
//    {
//        services.AddEndpointsApiExplorer();

//        // Swagger-ის გენერატორის რეგისტრაცია, 1 ან მეტი Swagger დოკუმენტის განსაზღვრა
//        services.AddSwaggerGen(options =>
//        {
//            // nullable ველების აღწერა swagger ის დოკუმნტაციაში
//            options.UseAllOfToExtendReferenceSchemas();

//            // DTO კლასის სახელების დაგენერირების წესის განსაზღვრა
//            options.CustomSchemaIds(x => x.FullName?.Replace("+", "").Replace("`", ""));

//            // ავტორიზაციის წესების განსაზღვრა
//            var securityScheme = new OpenApiSecurityScheme
//            {
//                Scheme = "bearer",
//                BearerFormat = "JWT",
//                Name = "JWT Authentication",
//                In = ParameterLocation.Header,
//                Type = SecuritySchemeType.Http,
//                Description = "ქვედა ტექსტბოქსში ჩაწერეთ **_მხოლოდ_** თქვენი JWT Bearer token !",

//                //Reference = new OpenApiReference
//                //{
//                //    Id = JwtBearerDefaults.AuthenticationScheme,
//                //    Type = ReferenceType.SecurityScheme
//                //}
//            };
//            //options.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);
//            //options.AddSecurityRequirement(new OpenApiSecurityRequirement
//            //{
//            //    { securityScheme, Array.Empty<string>() }
//            //});
//            // მეთოდების დახარისხება სხვადასხვა სექციებად
//            foreach (var name in Options)
//            {
//                options.SwaggerDoc(name: name, new OpenApiInfo
//                {
//                    Title = "CleanSolution.Presentation.WebApi",
//                    Version = "v1.0",
//                    Description = "ASP.NET Core Web API - CleanSolution",
//                    Contact = new OpenApiContact
//                    {
//                        Name = "test",
//                        Email = "test@mail.com",
//                        Url = new Uri("http://test.com/")
//                    }
//                });
//            }

//            // კომენტარების დაყენება Swagger JSON და UI–თვის.
//            var xmlPath = Path.Combine(AppContext.BaseDirectory, "SwaggerDescription.xml");
//            options.IncludeXmlComments(xmlPath);
//        });
//    }

//    // middleware
//    public static IApplicationBuilder UseSwaggerMiddleware(this IApplicationBuilder app)
//    {
//        app.UseSwaggerUI(c =>
//        {
//            c.InjectStylesheet("/SwaggerDark.css"); // შავი ფონის დაყენება

//            foreach (var name in Options)
//            {
//                c.SwaggerEndpoint(
//                    url: $"{name}/swagger.json",
//                    name: name);
//            }
//        });

//        return app;
//    }
//}