﻿using Autofac;
using Autofac.Extensions.DependencyInjection;
using Common.Log;
using Lykke.Job.LykkeJob.Domain.Services;
using Lykke.Job.LykkeJob.Services;
using Lykke.Job.LykkeJob.Settings.JobSettings;
#if azurequeuesub
using Lykke.JobTriggers.Extenstions;
#endif
#if timeperiod
using Lykke.Job.LykkeJob.PeriodicalHandlers;
#endif
#if rabbitsub
using Lykke.Job.LykkeJob.RabbitSubscribers;
#endif
#if rabbitpub
using AzureStorage.Blob;
using Lykke.Job.LykkeJob.Contract;
using Lykke.Job.LykkeJob.RabbitPublishers;
using Lykke.RabbitMq.Azure;
using Lykke.RabbitMqBroker.Publisher;
#endif
using Lykke.SettingsReader;
using Microsoft.Extensions.DependencyInjection;

namespace Lykke.Job.LykkeJob.Modules
{
    public class JobModule : Module
    {
        private readonly LykkeJobSettings _settings;
        private readonly IReloadingManager<LykkeJobSettings> _settingsManager;
        // NOTE: you can remove it if you don't need to use IServiceCollection extensions to register service specific dependencies
        private readonly IServiceCollection _services;

        public JobModule(LykkeJobSettings settings, IReloadingManager<LykkeJobSettings> settingsManager)
        {
            _settings = settings;
            _settingsManager = settingsManager;

            _services = new ServiceCollection();
        }

        protected override void Load(ContainerBuilder builder)
        {
            // NOTE: Do not register entire settings in container, pass necessary settings to services which requires them
            // ex:
            // builder.RegisterType<QuotesPublisher>()
            //  .As<IQuotesPublisher>()
            //  .WithParameter(TypedParameter.From(_settings.Rabbit.ConnectionString))

            builder.RegisterType<HealthService>()
                .As<IHealthService>()
                .SingleInstance();

            builder.RegisterType<StartupManager>()
                .As<IStartupManager>()
                .SingleInstance();

            builder.RegisterType<ShutdownManager>()
                .As<IShutdownManager>()
                .AutoActivate()
                .SingleInstance();
#if azurequeuesub

            RegisterAzureQueueHandlers(builder);
#endif
#if timeperiod

            RegisterPeriodicalHandlers(builder);
#endif
#if rabbitsub

            RegisterRabbitMqSubscribers(builder);
#endif
#if rabbitpub

            RegisterRabbitMqPublishers(builder);
#endif

            // TODO: Add your dependencies here

            builder.Populate(_services);
        }
#if azurequeuesub

        private void RegisterAzureQueueHandlers(ContainerBuilder builder)
        {
            // NOTE: You can implement your own poison queue notifier for azure queue subscription.
            // See https://github.com/LykkeCity/JobTriggers/blob/master/readme.md
            // builder.Register<PoisionQueueNotifierImplementation>().As<IPoisionQueueNotifier>();

            builder.AddTriggers(
                pool =>
                {
                    pool.AddDefaultConnection(_settingsManager.Nested(s => s.AzureQueue.ConnectionString));
                });
        }
#endif
#if timeperiod

        private void RegisterPeriodicalHandlers(ContainerBuilder builder)
        {
            // TODO: You should register each periodical handler in DI container as IStartable singleton and autoactivate it

            builder.RegisterType<MyPeriodicalHandler>()
                .As<IStartable>()
                .SingleInstance();
        }
#endif
#if rabbitsub

        private void RegisterRabbitMqSubscribers(ContainerBuilder builder)
        {
            // TODO: You should register each subscriber in DI container as IStartable singleton and autoactivate it

            builder.RegisterType<MyRabbitSubscriber>()
                .As<IStartable>()
                .SingleInstance()
                .WithParameter("connectionString", _settings.Rabbit.ConnectionString)
                .WithParameter("exchangeName", _settings.Rabbit.ExchangeName);
        }
#endif
#if rabbitpub

        private void RegisterRabbitMqPublishers(ContainerBuilder builder)
        {
            // TODO: You should register each publisher in DI container as publisher specific interface and as IStartable,
            // as singleton and do not autoactivate it

            builder.RegisterType<MyRabbitPublisher>()
                .As<IMyRabbitPublisher>()
                .As<IStartable>()
                .SingleInstance()
                .WithParameter(TypedParameter.From(_settings.Rabbit.ConnectionString));
        }
#endif
    }
}
