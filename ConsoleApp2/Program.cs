using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;
using ShopfloorAssistant.Core;
using ShopfloorAssistant.Core.AgentsConfig;
using ShopfloorAssistant.Core.AiSearch;
using ShopfloorAssistant.Core.Sql;
using ShopfloorAssistant.Core.Workflows;

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

    var azureAd = builder.Configuration.GetSection("AzureAd");
    var tenantId = azureAd["TenantId"];
    var clientId = azureAd["ClientId"];

    c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows
        {
            AuthorizationCode = new OpenApiOAuthFlow
            {
                AuthorizationUrl = new Uri($"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/authorize"),
                TokenUrl = new Uri($"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token"),
                Scopes = new Dictionary<string, string>
                {
                    { $"api://{clientId}/.default", "Access the API" }
                }
            }
        }
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "oauth2"
                }
            },
            new[] { $"api://{clientId}/.default" }
        }
    });
}); 
builder.Services.AddControllers();

builder.Services.Configure<OpenAiOptions>(builder.Configuration.GetSection(OpenAiOptions.OpenAI));
builder.Services.Configure<SqlQueryOptions>(builder.Configuration.GetSection(SqlQueryOptions.Sql));
builder.Services.Configure<AiSearchOptions>(builder.Configuration.GetSection(AiSearchOptions.AzureAISearch));
builder.Services.Configure<McpOptions>(builder.Configuration.GetSection(McpOptions.Mcp));

builder.Services.AddTransient<ShopfloorAssistant.AppService.IAgentAppService, ShopfloorAssistant.AppService.AgentAppService>();
builder.Services.AddTransient<ISqlQueryService, SqlQueryService>();
builder.Services.AddTransient<SqlQueryExecutor>();
builder.Services.AddTransient<IAiSearchService, AiSearchService>();

builder.Services.AddSingleton<IAgentProvider, AgentProvider>();
builder.Services.AddTransient<IUserRoleService, UserRoleService>();
builder.Services.AddSingleton<IAgentPromptProvider>(provider =>
{
    var roleService = provider.GetRequiredService<IUserRoleService>();
    var baseDir = Path.Combine(AppContext.BaseDirectory, "prompts");
    return new FileAgentPromptProvider(baseDir, roleService);
});

var app = builder.Build();


// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Shopfloor Assistant API v1");
    c.RoutePrefix = string.Empty;
    c.OAuthClientId(builder.Configuration["AzureAd:ClientId"]);
    c.OAuthUsePkce(); // Obligatorio si no usas client secret
    c.OAuthScopeSeparator(" ");
    c.OAuthAppName("Shopfloor Assistant Swagger UI");
});
//}
app.UseCors(MyAllowSpecificOrigins);

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
