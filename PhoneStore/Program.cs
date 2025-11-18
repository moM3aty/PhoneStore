using Microsoft.EntityFrameworkCore;
using PhoneStore.Data;
using PhoneStore.Filters;

var builder = WebApplication.CreateBuilder(args);

// 1. إضافة خدمة الـ Controllers والـ Views
builder.Services.AddControllersWithViews();

// 2. تسجيل IHttpContextAccessor (هام جداً لعمل الهيدر والسلة)
builder.Services.AddHttpContextAccessor();

// 3. إعداد قاعدة البيانات
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<StoreDbContext>(options =>
    options.UseSqlServer(connectionString));

// 4. إضافة خدمات الجلسات (Session)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromDays(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// 5. تسجيل فلتر الحماية
builder.Services.AddScoped<AdminAuthFilter>();


var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 6. تفعيل الجلسات
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();