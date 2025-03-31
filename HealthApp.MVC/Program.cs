// Packages
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using HealthApp.MVC.Data;
using HealthApp.MVC.Models.Entities;
using HealthApp.MVC.Models.Domain;

var builder = WebApplication.CreateBuilder(args);

// Add database context with SQLite
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(connectionString));   

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Add Identity with roles
builder.Services.AddDefaultIdentity<ApplicationUser>(options => {
    options.SignIn.RequireConfirmedAccount = true;
    options.User.RequireUniqueEmail = true;
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Create the roles at the start of the application
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    string[] roles = { "Admin", "Doctor", "Patient" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
}

using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    string adminEmail = "admin@hospital.com";
    string adminPassword = "Admin@123";

    if (!await roleManager.RoleExistsAsync("Admin"))
    {
        await roleManager.CreateAsync(new IdentityRole("Admin"));
    }

    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true,
            FirstName = "Admin",
            LastName = "Admin",
            RoleType = "Admin"
        };

        var result = await userManager.CreateAsync(adminUser, adminPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }
}

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    if (!context.Specializations.Any())
    {
        var specializations = new List<Specialization>
        {
            new Specialization { Type = SpecializationType.GeneralPractice, Name = "General Practice", Description = "General Practice provides comprehensive primary care, including routine check-ups, preventive care, and management of common illnesses." },
            new Specialization { Type = SpecializationType.Cardiology, Name = "Cardiology", Description = "Cardiology focuses on diagnosing and treating diseases of the heart and blood vessels, including conditions such as heart attacks and arrhythmias." },
            new Specialization { Type = SpecializationType.Dermatology, Name = "Dermatology", Description = "Dermatology is dedicated to the diagnosis and treatment of skin, hair, and nail disorders, ranging from acne to skin cancer." },
            new Specialization { Type = SpecializationType.Neurology, Name = "Neurology", Description = "Neurology deals with disorders of the nervous system, including the brain, spinal cord, and peripheral nerves, such as stroke and epilepsy." },
            new Specialization { Type = SpecializationType.Pediatrics, Name = "Dermatology", Description = "Pediatrics is the branch of medicine that manages the health and medical care of infants, children, and adolescents." },
            new Specialization { Type = SpecializationType.Gynecology, Name = "Gynecology", Description = "Gynecology focuses on the health of the female reproductive system, including menstrual issues, fertility, and hormone-related conditions." },
            new Specialization { Type = SpecializationType.Orthopedics, Name = "Orthopedics", Description = "Orthopedics specializes in the treatment of conditions related to the musculoskeletal system, including bones, joints, muscles, and ligaments." },
            new Specialization { Type = SpecializationType.Ophthalmology, Name = "Ophthalmology", Description = "Ophthalmology deals with eye diseases and vision care, including the diagnosis and surgical treatment of conditions affecting the eyes." },
            new Specialization { Type = SpecializationType.Gastroenterology, Name = "Gastroenterology", Description = "Gastroenterology focuses on the digestive system and its disorders, such as acid reflux, inflammatory bowel disease, and liver disease." },
            new Specialization { Type = SpecializationType.Pulmonology, Name = "Pulmonology", Description = "Pulmonology is dedicated to the treatment of respiratory system disorders, including asthma, COPD, and lung infections." },
            new Specialization { Type = SpecializationType.Endocrinology, Name = "Endocrinology", Description = "Endocrinology deals with hormone-related disorders and the endocrine system, such as diabetes, thyroid dysfunction, and metabolic disorders." },
            new Specialization { Type = SpecializationType.Oncology, Name = "Oncology", Description = "Oncology focuses on the diagnosis, treatment, and management of cancer, using various therapies like chemotherapy, radiation, and surgery." },
            new Specialization { Type = SpecializationType.Urology, Name = "Urology", Description = "Urology specializes in diseases of the urinary tract and the male reproductive system, including kidney stones, urinary incontinence, and prostate issues." },
            new Specialization { Type = SpecializationType.Psychiatry, Name = "Psychiatry", Description = "Psychiatry is the medical specialty that deals with the diagnosis, treatment, and prevention of mental, emotional, and behavioral disorders." },
            new Specialization { Type = SpecializationType.GeneralSurgery, Name = "General Surgery", Description = "General Surgery involves a broad range of surgical procedures, primarily dealing with the abdominal organs, as well as other tissues and areas of the body." },
            new Specialization { Type = SpecializationType.Radiology, Name = "Radiology", Description = "Radiology uses imaging technologies such as X-rays, CT scans, MRI, and ultrasound to diagnose and sometimes treat diseases." },
            new Specialization { Type = SpecializationType.Rheumatology, Name = "Rheumatology", Description = "Rheumatology is concerned with the diagnosis and treatment of rheumatic diseases and disorders of the joints, muscles, and bones, such as arthritis." },
            new Specialization { Type = SpecializationType.Nephrology, Name = "Nephrology", Description = "Nephrology focuses on kidney care and the treatment of kidney diseases, including chronic kidney disease and electrolyte imbalances." },
            new Specialization { Type = SpecializationType.Anesthesiology, Name = "Anesthesiology", Description = "Anesthesiology specializes in anesthesia and perioperative medicine, ensuring patients undergo surgery with minimal pain and risk." },
            new Specialization { Type = SpecializationType.InfectiousDiseases, Name = "Infectious Diseases", Description = "Infectious Diseases deals with the diagnosis and treatment of infections caused by bacteria, viruses, fungi, or parasites, including emerging and antibiotic-resistant infections." },
            new Specialization { Type = SpecializationType.InternalMedicine, Name = "Internal Medicine", Description = "Internal Medicine focuses on the prevention, diagnosis, and treatment of a wide range of adult diseases, often managing complex, multi-system conditions." },
        };

        context.Specializations.AddRange(specializations);
        context.SaveChanges();
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapAreaControllerRoute(
	name: "Admin",
	areaName: "Admin",
	pattern: "Admin/{controller=Admin}/{action=Index}/{id?}");
app.MapAreaControllerRoute(
    name: "Doctors",
    areaName: "Doctors",
    pattern: "Doctors/{controller=Doctors}/{action=Index}/{id?}");
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
