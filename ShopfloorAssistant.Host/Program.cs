using AutoMapper;
using Microsoft.Agents.AI.Hosting.AGUI.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;
using ShopfloorAssistant.AppService;
using ShopfloorAssistant.Core;
using ShopfloorAssistant.Core.AgentsConfig;
using ShopfloorAssistant.Core.AiSearch;
using ShopfloorAssistant.Core.ChatStore;
using ShopfloorAssistant.Core.Email;
using ShopfloorAssistant.Core.Repository;
using ShopfloorAssistant.Core.Sql;
using ShopfloorAssistant.Core.Workflows;
using ShopfloorAssistant.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(options =>
    {
        builder.Configuration.Bind("AzureAd", options);
        options.TokenValidationParameters.ValidateIssuer = true;
    },
    options =>
    {
        builder.Configuration.Bind("AzureAd", options);
    });
builder.Services.AddAuthorization();


builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          // PERMITE peticiones desde tu frontend de Angular
                          policy.AllowAnyOrigin()
                                .AllowAnyHeader() // Permite cualquier encabezado HTTP
                                .AllowAnyMethod(); // Permite GET, POST, PUT, DELETE, etc.

                          // Alternativa temporal y menos segura para desarrollo: 
                          // policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
                      });
});

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Shopfloor Assistant API",
        Version = "v1",
        Description = "API for managing AI Agents and Azure OpenAI workflows."
    });

    // Definición de seguridad tipo Bearer JWT
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Ingresa un token JWT en el campo. Ejemplo: Bearer {tu_token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    // Requerimiento global del token
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

    c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
});

var appServiceAssembly = AppDomain.CurrentDomain
    .GetAssemblies()
    .First(a => a.GetName().Name == "ShopfloorAssistant.AppService");

builder.Services.AddAutoMapper(cfg => cfg.AddMaps(appServiceAssembly));


builder.Services.AddControllers();
var sqlOptions = builder.Configuration.GetSection(SqlQueryOptions.Sql);
builder.Services.Configure<OpenAiOptions>(builder.Configuration.GetSection(OpenAiOptions.OpenAI)); 
builder.Services.Configure<SqlQueryOptions>(builder.Configuration.GetSection(SqlQueryOptions.Sql));
builder.Services.Configure<AiSearchOptions>(builder.Configuration.GetSection(AiSearchOptions.AzureAISearch));
builder.Services.Configure<McpOptions>(builder.Configuration.GetSection(McpOptions.Mcp));

builder.Services.AddTransient<ShopfloorAssistant.AppService.IAgentAppService, ShopfloorAssistant.AppService.AgentAppService>();
builder.Services.AddTransient<ShopfloorAssistant.AppService.IThreadAppService, ThreadAppService>();
builder.Services.AddTransient<ShopfloorAssistant.AppService.IPromptSuggestionAppService, PromptSuggestionAppService>();
builder.Services.AddTransient<ISqlQueryService, SqlQueryService>();
builder.Services.AddTransient<SqlQueryExecutor>();
builder.Services.AddTransient<ToolExecutor>();
builder.Services.AddTransient<ShopfloorChatMessageStore>();
builder.Services.AddTransient<IAiSearchService, AiSearchService>();
builder.Services.AddTransient<ShopfloorSession>();


builder.Services.AddSingleton<IAgentProvider, AgentProvider>();
builder.Services.AddTransient<IUserRoleService, UserRoleService>();
builder.Services.AddHttpClient<IEmailService, PowerAutomateMailService>().SetHandlerLifetime(TimeSpan.FromMinutes(10));
builder.Services.AddSingleton<IAgentPromptProvider>(provider =>
{
    var roleService = provider.GetRequiredService<IUserRoleService>();
    var baseDir = Path.Combine(AppContext.BaseDirectory, "prompts");
    return new FileAgentPromptProvider(baseDir, roleService);
});
builder.Services.AddEntityFrameworkCoreServices(sqlOptions.Get<SqlQueryOptions>()?.DefaultConnection);

builder.Services.AddAGUI();
var app = builder.Build();


// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Shopfloor Assistant API v1");
    c.RoutePrefix = string.Empty;
});

//}
app.UseCors(MyAllowSpecificOrigins);

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.ApplyMigrations();

var agentProvider = app.Services.GetRequiredService<IAgentProvider>();
app.MapAGUI("/", new MyAgent(await agentProvider.GetShopfloorAgent(), app.Services.GetRequiredService<IThreadRepository>())).RequireAuthorization();

await app.RunAsync();
