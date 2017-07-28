# Lykke.Job template

dotnet cli template for generating a solution for the job Lykke.Job.JobName

## How install and use template?

Clone repo to some directory

Install template:
```sh
$ dotnet new --install ${path}
```
where `${path}` is the path to the clonned directory (where folder .template.config placed) without trailing slash

Now new template can be used in dotnet cli:

```sh
dotnet new lkejob -n ${JobName} -o Lykke.Job.${JobName} [-e {true|false}]
```
This will create a solution in the current folder, where `${JobName}` is the job name without Lykke.Job. prefix. 
Switches:
* -n: JobName
* -o: Output directory name
* -e: Includes code snippets examples. Default value is `true`, specify `false` if you familar with project's structure

When temlate has changed, to update installed template run again command:

```sh
$ dotnet new --install ${path}
```

To remove all installed custom temlates run command:

```sh
$ dotnet new --debug:reinit 
```

## Developing notes

### Environment variables

To define your own environment variables, see [Working with multiple environments](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/environments)

* *ASPNETCORE_ENVIRONMENT* - defines environment name, the value can be: Development, Staging, Production. When value is Development, 
AppSettings will be loaded from appsettings.Development.json (which overrides appsettings.json), 
otherwise, `AppSettings` will be loaded from URL defined by `SettingsUrl` env variable.
* *SettingsUrl* - defines URL of remote settings. 

Default launchSettings.json is:

```json
{
  "profiles": {
    "LykkeJob Development": {
      "commandName": "Project",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    },
    "LykkeJob Remote Settings": {
      "commandName": "Project",
      "environmentVariables": {
        "SettingsUrl": ""
      }
    }
  }
}
```

### Job triggers

See [Job Triggers read me](https://github.com/LykkeCity/JobTriggers/blob/master/readme.md)

Consider trigger handler class as controller, it responsible to control execution flow, and cross-cutting concerns. Place all business logic in appropriate service classes. Typical trigger handler class looks like:

```cs
    public class MyHandlers
    {
        private readonly IMyFooService _myFooService;
        private readonly IMyBooService _myBooService;
        private readonly IHealthService _healthService;

        // NOTE: The object is instantiated using DI container, so registered dependencies are injects well
        public MyHandlers(IMyFooService myFooService, IMyBooService myBooService, IHealthService healthService)
        {
            _myFooService = myFooService;
            _myBooService = myBooService;
            _healthService = healthService;
        }

        [TimerTrigger("00:00:10")]
        public async Task TimeTriggeredHandler()
        {
            try
            {
                _healthService.TraceFooStarted();

                await _myFooService.FooAsync();

                _healthService.TraceFooCompleted();
            }
            catch
            {
                _healthService.TraceFooFailed();
            }
        }

        [QueueTrigger("queue-name")]
        public async Task QueueTriggeredHandler(MyMessage msg)
        {
            try
            {
                _healthService.TraceBooStarted();

                await _myBooService.BooAsync();

                _healthService.TraceBooCompleted();
            }
            catch
            {
                _healthService.TraceBooFailed();
            }
        }
    }
```

### Health monitoring

Job should provides it's health status by responding to HTTP `/api/isAlive` request. 
If job health is ok, it should respond IsAliveResponse with status code 200, if job health is bad, it should respond `ErrorResponse` with status code 500.

You should gathers health statistics in `IHealthService` by injecting it into your trigger handlers classes, 
and invoking specific `IHealthService` methods when key events has occures. 
Then you should implement `IHealthService.GetHealthViolationMessage()` method, 
to return job health violation message, if any.

You can extend `IsAliveResponse` to  provide all necessary job health information.

*Job shouldn't receive any other HTTP requests*

Typical IsAlive action looks like:

```cs
    public IActionResult Get()
    {
        var healthViloationMessage = _healthService.GetHealthViolationMessage();
        if (healthViloationMessage != null)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, new ErrorResponse
            {
                ErrorMessage = $"Job is unhealthy: {healthViloationMessage}"
            });
        }

        // NOTE: Feel free to extend IsAliveResponse, to display job-specific health status
        return Ok(new IsAliveResponse
        {
            Version = Microsoft.Extensions.PlatformAbstractions.PlatformServices.Default.Application.ApplicationVersion,
            Env = Environment.GetEnvironmentVariable("ENV_INFO"),

            // NOTE: Health status information example: 
            LastFooStartedMoment = _healthService.LastFooStartedMoment,
            LastFooDuration = _healthService.LastFooDuration,
            MaxHealthyFooDuration = _healthService.MaxHealthyFooDuration
        });
    }
```