namespace MultiTenantApp.Web.Services
{
    public class TokenProvider : ITokenProvider
    {
        private readonly object _lock = new();
        private TaskCompletionSource<bool> _tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private string _token;

        public string Token
        {
            get
            {
                lock (_lock) { return _token; }
            }
            private set
            {
                bool changed;
                lock (_lock)
                {
                    changed = _token != value;
                    _token = value;
                    if (changed)
                    {
                        // replace TCS so new awaiters wait for next set (or you can keep same behavior)
                        var prev = _tcs;
                        _tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                        // sinaliza que existe token agora (ou que foi atualizado)
                        prev.TrySetResult(true);
                    }
                }
                if (changed)
                    TokenChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler TokenChanged;

        public Task SetTokenAsync(string token)
        {
            Token = token;
            return Task.CompletedTask;
        }

        public Task ClearTokenAsync()
        {
            Token = null;
            return Task.CompletedTask;
        }

        public async Task WaitForTokenAsync(CancellationToken cancellationToken = default)
        {
            // Se já tem token, retorna rápido
            if (!string.IsNullOrEmpty(Token)) return;

            // aguarda até que alguém chame SetTokenAsync (ou token seja limpo)
            using (cancellationToken.Register(() => _tcs.TrySetCanceled()))
            {
                await _tcs.Task.ConfigureAwait(false);
            }
        }
    }

}
