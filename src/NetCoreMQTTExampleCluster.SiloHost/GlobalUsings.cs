#pragma warning disable IDE0065 // Die using-Anweisung wurde falsch platziert.
global using System.Net;
global using System.Reflection;

global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Hosting;

global using NetCoreMQTTExampleCluster.Grains;
global using NetCoreMQTTExampleCluster.SiloHost.Configuration;
global using NetCoreMQTTExampleCluster.SiloHost.Extensions;
global using NetCoreMQTTExampleCluster.Storage;
global using NetCoreMQTTExampleCluster.Storage.Repositories.Implementation;
global using NetCoreMQTTExampleCluster.Storage.Repositories.Interfaces;
global using NetCoreMQTTExampleCluster.Validation;

global using Orleans;
global using Orleans.Configuration;
global using Orleans.Hosting;

global using OrleansDashboard;

global using Serilog;
global using Serilog.Events;
global using Serilog.Exceptions;

global using ILogger = Serilog.ILogger;
#pragma warning restore IDE0065 // Die using-Anweisung wurde falsch platziert.
