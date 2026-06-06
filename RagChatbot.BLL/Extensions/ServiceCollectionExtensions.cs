using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RagChatbot.DAL.Data;
using RagChatbot.DAL.Repositories.Interfaces;
using RagChatbot.DAL.Repositories.Implements;
using RagChatbot.BLL.Services.Interfaces;
using RagChatbot.BLL.Services.Implements;

namespace RagChatbot.BLL.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddProjectDependencies(this IServiceCollection services, IConfiguration configuration)
        {
            // 1. Đăng ký Database (DAL)
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"),
                npgsqlOptions => npgsqlOptions.UseVector()));

            // 2. Đăng ký Repositories (DAL)
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ISubjectRepository, SubjectRepository>();
            services.AddScoped<IDocumentRepository, DocumentRepository>();
            services.AddScoped<IDocumentChunkRepository, DocumentChunkRepository>();
            services.AddScoped<IUserSubjectRepository, UserSubjectRepository>();

            // 3. Đăng ký Services (BLL)
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ISubjectService, SubjectService>();
            services.AddScoped<IDocumentService, DocumentService>();
            services.AddScoped<IUserSubjectService, UserSubjectService>();
            services.AddScoped<IAIService, GeminiService>();
            services.AddScoped<IChatbotService, ChatbotService>();
            services.AddScoped<IDocumentProcessingService, DocumentProcessingService>();

            return services;
        }
    }
}
