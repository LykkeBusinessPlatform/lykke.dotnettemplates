﻿using System;
using Autofac;
using JetBrains.Annotations;
using Lykke.HttpClientGenerator;

namespace Lykke.Service.LykkeService.Client
{
    [PublicAPI]
    public static class AutofacExtension
    {
        /// <summary>
        /// Registers <see cref="ILykkeServiceClient"/> in Autofac container using <see cref="LykkeServiceServiceClientSettings"/>.
        /// </summary>
        /// <param name="builder">Autofac container builder.</param>
        /// <param name="settings">LykkeService client settings.</param>
        /// <param name="builderConfigure">Optional <see cref="HttpClientGeneratorBuilder"/> configure handler.</param>
        public static void RegisterLykkeServiceClient(
            [NotNull] this ContainerBuilder builder,
            [NotNull] LykkeServiceServiceClientSettings settings,
            [CanBeNull] Func<HttpClientGeneratorBuilder, HttpClientGeneratorBuilder> builderConfigure)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));
            if (string.IsNullOrWhiteSpace(settings.ServiceUrl))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(LykkeServiceServiceClientSettings.ServiceUrl));

            builder.RegisterClient<ILykkeServiceClient>(settings?.ServiceUrl, builderConfigure);
        }
    }
}
