using DropdownTests.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace DropdownTests
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            // Sample in-memory data. Replace with your own data access (e.g. an EF Core DbContext).
            services.AddSingleton<RestrictionStore>();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();
            else
                app.UseExceptionHandler("/Error");

            app.UseStaticFiles();
            app.UseMvc();
        }
    }
}
