﻿using IdentitySample.Shared.Models;

namespace IdentitySample.BusinessLayer.Services;

public interface IIdentityService
{
    Task<RegisterResponse> RegisterAsync(RegisterRequest request);

    Task<AuthResponse> LoginAsync(LoginRequest request);

    Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request);

    Task<AuthResponse> ImpersonateAsync(Guid userId);
}
