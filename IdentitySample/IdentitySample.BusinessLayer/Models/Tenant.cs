﻿namespace IdentitySample.BusinessLayer.Models;

public record Tenant(Guid Id, string ConnectionString, string StorageConnectionString, string ContainerName);
