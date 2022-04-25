#pragma warning disable IDE0065 // Die using-Anweisung wurde falsch platziert.
global using System;
global using System.Collections.Concurrent;
global using System.Collections.Generic;
global using System.Linq;
global using System.Text;
global using System.Threading;
global using System.Threading.Tasks;

global using Microsoft.AspNetCore.Identity;
global using Microsoft.Extensions.Caching.Memory;

global using MQTTnet;
global using MQTTnet.Client.Options;
global using MQTTnet.Server;

global using NetCoreMQTTExampleCluster.Grains.Interfaces;
global using NetCoreMQTTExampleCluster.Models.Interfaces;
global using NetCoreMQTTExampleCluster.Storage.Data;
global using NetCoreMQTTExampleCluster.Storage.Repositories.Interfaces;
global using NetCoreMQTTExampleCluster.Validation;

global using Orleans;
global using Orleans.Concurrency;

global using Serilog;
#pragma warning restore IDE0065 // Die using-Anweisung wurde falsch platziert.
