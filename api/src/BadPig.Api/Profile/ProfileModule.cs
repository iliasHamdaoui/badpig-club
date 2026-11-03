using BadPig.Profile.Core;
using BadPig.Profile.Infrastructure;
using BadPig.Shared.Core;
using Microsoft.Extensions.DependencyInjection;

namespace BadPig.Profile;

/// <summary>What a person does for themselves (profile, licence, profile picture, deleting the account) and how the other members see them (club directory).</summary>
public static class ProfileModule
{
    public static IServiceCollection AddProfileModule(this IServiceCollection services)
    {
        services.AddScoped<PhotoAccess>();
        services.AddScoped<IMemberPhotos, MemberPhotos>();
        services.AddScoped<IAccountErasure, ProfileEraser>();
        return services;
    }
}
