# Entitiy Framework Core 5 Temportal Tables

Based on https://github.com/findulov/EntityFrameworkCore.TemporalTables and https://github.com/Adam-Langley/efcore-temporal-query

```
public void ConfigureServices(IServiceCollection services)
{
	services.AddDbContext<YourDbContext>((provider, options) =>
	{
		options.UseSqlServer(connectionString);
		options.UseInternalServiceProvider(provider);
	});
	services.AddEntityFrameworkSqlServer();
	services.RegisterTemporalTablesForDatabase<YourDbContext>();
}
```
