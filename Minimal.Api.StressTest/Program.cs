// See https://aka.ms/new-console-template for more information

using NBomber.Contracts;
using NBomber.CSharp;
using NBomber.Plugins.Http.CSharp;

var httpFactory = HttpClientFactory.Create();

var minimalApiStep = Step.Create("Call Get All Users", httpFactory, async context =>
{
    var request =
        Http.CreateRequest("GET", "https://localhost:7196/User")
            .WithCheck(response =>
                Task.FromResult(response.IsSuccessStatusCode
                    ? Response.Ok()
                    : Response.Fail())
            );

    var response = await Http.Send(request, context);
    return response;
});

var controllerApiStep = Step.Create("Call Get All Users", httpFactory, async context =>
{
    var request =
        Http.CreateRequest("GET", "https://localhost:7188/User")
            .WithCheck(response =>
                Task.FromResult(response.IsSuccessStatusCode
                    ? Response.Ok()
                    : Response.Fail())
            );

    var response = await Http.Send(request, context);
    return response;
});

var minimalApiWithControllerApiStep = Step.Create("Call Get All Users", httpFactory, async context =>
{
    var request =
        Http.CreateRequest("GET", "https://localhost:7188/User1")
            .WithCheck(response =>
                Task.FromResult(response.IsSuccessStatusCode
                    ? Response.Ok()
                    : Response.Fail())
            );

    var response = await Http.Send(request, context);
    return response;
});

var minimalApiScenario = ScenarioBuilder
    .CreateScenario("nbomber-minimal-api", minimalApiStep)
    .WithWarmUpDuration(TimeSpan.FromSeconds(10))
    .WithLoadSimulations(Simulation.KeepConstant(16, TimeSpan.FromSeconds(60)));

var controllerScenario = ScenarioBuilder
    .CreateScenario("nbomber-controller-api", controllerApiStep)
    .WithWarmUpDuration(TimeSpan.FromSeconds(10))
    .WithLoadSimulations(Simulation.KeepConstant(16, TimeSpan.FromSeconds(60)));

var minimalApiWithControllerScenario = ScenarioBuilder
    .CreateScenario("nbomber-minimal-api-controller-api", minimalApiWithControllerApiStep)
    .WithWarmUpDuration(TimeSpan.FromSeconds(10))
    .WithLoadSimulations(Simulation.KeepConstant(16, TimeSpan.FromSeconds(60)));

//NBomberRunner
//    .RegisterScenarios(controllerScenario)
//    .WithTestName("nbomber-controller-api")
//    .Run();

//NBomberRunner
//    .RegisterScenarios(minimalApiScenario)
//    .WithTestName("nbomber-minimal-api")
//    .Run();

NBomberRunner
    .RegisterScenarios(minimalApiWithControllerScenario)
    .WithTestName("nbomber-minimal-api-controller-api")
    .Run();
