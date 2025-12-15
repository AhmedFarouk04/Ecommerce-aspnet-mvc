using Microsoft.Extensions.Caching.Memory;

namespace ECommerce.Web.Services
{
    public class LoginSecurityService
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<LoginSecurityService> _logger;

        private readonly int _maxAttemptsPerWindow = 8;

        private readonly TimeSpan _ipWindow = TimeSpan.FromMinutes(1);

        private readonly TimeSpan _ipBlockDuration = TimeSpan.FromMinutes(5);

        public LoginSecurityService(IMemoryCache cache, ILogger<LoginSecurityService> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        private string GetIpAttemptsKey(string ip) => $"login:ip:attempts:{ip}";
        private string GetIpBlockKey(string ip) => $"login:ip:block:{ip}";

        public Task<bool> IsIpBlockedAsync(string ip)
        {
            if (string.IsNullOrWhiteSpace(ip))
                return Task.FromResult(false);

            bool blocked = _cache.TryGetValue(GetIpBlockKey(ip), out _);
            return Task.FromResult(blocked);
        }

        public Task RegisterFailedAttemptAsync(string ip)
        {
            if (string.IsNullOrWhiteSpace(ip))
                return Task.CompletedTask;

            var key = GetIpAttemptsKey(ip);

            int attempts = _cache.TryGetValue(key, out int existing)
                ? existing
                : 0;

            attempts++;

            _cache.Set(key, attempts, _ipWindow);

            if (attempts >= _maxAttemptsPerWindow)
            {
                _cache.Set(GetIpBlockKey(ip), true, _ipBlockDuration);
                _logger.LogWarning("IP {IP} has been temporarily blocked due to too many login attempts.", ip);
            }

            return Task.CompletedTask;
        }

        public Task ClearIpStateAsync(string ip)
        {
            if (string.IsNullOrWhiteSpace(ip))
                return Task.CompletedTask;

            _cache.Remove(GetIpAttemptsKey(ip));
            return Task.CompletedTask;
        }

        
        public async Task ApplyDynamicDelayAsync(string ip, int failedAttemptsForUser)
        {
            int delaySeconds = failedAttemptsForUser switch
            {
                <= 1 => 0,
                2 => 1,
                3 => 2,
                4 => 3,
                _ => 5
            };

            if (delaySeconds > 0)
            {
                _logger.LogInformation("Applying {Delay}s delay for login from IP {IP} due to {Attempts} failed attempts.",
                    delaySeconds, ip, failedAttemptsForUser);

                await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
            }
        }
    }
}
