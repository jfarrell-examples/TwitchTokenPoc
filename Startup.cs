using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using TwitchTokenPoc.Services;

namespace TwitchTokenPoc
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            var keyVaultService = new KeyVaultService(new GetCredentialService(Configuration), Configuration);
            var tokenSecurityValidator = new JwtSecurityTokenValidator(Configuration, keyVaultService);
            services.AddTransient<CryptoService>()
                .AddTransient<JwtTokenService>()
                .AddSingleton<TwitchAuthService>()
                .AddSingleton(p => keyVaultService)
                .AddTransient(p => tokenSecurityValidator)
                .AddSingleton<GetCredentialService>()
                .AddTransient<TwitchApiService>()
                .AddTransient<GetTokensFromHttpRequestService>();

            services.AddHttpContextAccessor();            
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "TwitchTokenPoc", Version = "v1" });
            });

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.SecurityTokenValidators.Add(tokenSecurityValidator);
                });
        }
        
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "TwitchTokenPoc v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
