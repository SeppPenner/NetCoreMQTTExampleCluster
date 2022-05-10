#pragma warning disable IDE0065 // Die using-Anweisung wurde falsch platziert.
global using System.Reflection;

global using Microsoft.AspNetCore.Builder;
global using Microsoft.AspNetCore.Hosting;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Hosting;

global using NetCoreMQTTExampleCluster.Grains;
global using NetCoreMQTTExampleCluster.Models.Configuration;
global using NetCoreMQTTExampleCluster.Models.Constants;
global using NetCoreMQTTExampleCluster.Models.Extensions;
global using NetCoreMQTTExampleCluster.Models.Service;
global using NetCoreMQTTExampleCluster.Storage.Repositories.Implementation;
global using NetCoreMQTTExampleCluster.Storage.Repositories.Interfaces;
global using NetCoreMQTTExampleCluster.Validation;

global using Orleans;
global using Orleans.Configuration;
global using Orleans.Hosting;

global using Serilog;
global using Serilog.Events;
global using Serilog.Exceptions;

global using ILogger = Serilog.ILogger;
#pragma warning restore IDE0065 // Die using-Anweisung wurde falsch platziert.
