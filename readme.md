
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