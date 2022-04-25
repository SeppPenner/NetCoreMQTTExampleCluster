#pragma warning disable IDE0065 // Die using-Anweisung wurde falsch platziert.
global using System.Reflection;
global using System.Security.Authentication;
global using System.Security.Cryptography.X509Certificates;

global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Hosting;

global using MQTTnet;
global using MQTTnet.Protocol;
global using MQTTnet.Server;

global using NetCoreMQTTExampleCluster.Models.Configuration;
global using NetCoreMQTTExampleCluster.Models.Constants;
global using NetCoreMQTTExampleCluster.Grains.Interfaces;

global using Orleans;
global using Orleans.Configuration;
global using Orleans.Hosting;
global using Orleans.Runtime;

global using Polly;

global using Serilog;
global using Serilog.Events;
global using Serilog.Exceptions;

global using ILogger = Serilog.ILogger;
#pragma warning restore IDE0065 // Die using-Anweisung wurde falsch platziert.
