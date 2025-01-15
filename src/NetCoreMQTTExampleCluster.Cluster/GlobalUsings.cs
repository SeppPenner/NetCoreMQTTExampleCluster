#pragma warning disable IDE0065 // Die using-Anweisung wurde falsch platziert.
global using System.Diagnostics.CodeAnalysis;
global using System.Reflection;
global using System.Net.Mime;
global using System.Security.Authentication;
global using System.Security.Cryptography.X509Certificates;
global using System.Text;

global using Microsoft.AspNetCore.Authentication.JwtBearer;
global using Microsoft.AspNetCore.Builder;
global using Microsoft.AspNetCore.Hosting;
global using Microsoft.AspNetCore.ResponseCompression;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Hosting;
global using Microsoft.IdentityModel.Tokens;

global using MQTTnet;
global using MQTTnet.Protocol;
global using MQTTnet.Server;

global using NetCoreMQTTExampleCluster.Models.Configuration;
global using NetCoreMQTTExampleCluster.Models.Constants;
global using NetCoreMQTTExampleCluster.Models.Service;
global using NetCoreMQTTExampleCluster.Grains.Interfaces;

global using Orleans;
global using Orleans.Configuration;
global using Orleans.Hosting;

global using Serilog;
global using Serilog.Events;
global using Serilog.Exceptions;

global using ILogger = Serilog.ILogger;
#pragma warning restore IDE0065 // Die using-Anweisung wurde falsch platziert.
