var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.SynoAI_API>("synoai-api");

builder.Build().Run();
