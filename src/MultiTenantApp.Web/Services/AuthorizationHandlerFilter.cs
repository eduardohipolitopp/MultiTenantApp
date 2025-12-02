using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using System;
using System.Net.Http;

public class AuthorizationHandlerFilter : IHttpMessageHandlerBuilderFilter
{
    public AuthorizationHandlerFilter() { }

    public Action<HttpMessageHandlerBuilder> Configure(Action<HttpMessageHandlerBuilder> next)
    {
        return builder =>
        {
            next(builder);

            // IMPORTANTE: use builder.Services (o provider contextual) — não o provider root injetado.
            var handler = builder.Services.GetService<TokenAuthorizationMessageHandler>();
            if (handler != null)
            {
                builder.AdditionalHandlers.Add(handler);
            }
        };
    }
}
