namespace IdentitySample.Authentication.Entities;

public class Tenant
{
    public Guid Id { get; set; }

    public string ConnectionString { get; set; }

    public string StorageConnectionString { get; set; }

    public string ContainerName { get; set; }
}
