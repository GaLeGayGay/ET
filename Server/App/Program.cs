﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using ETHotfix;
using ETModel;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using NLog;

namespace App
{
	internal static class Program
	{

		
		private static void Main(string[] args)
		{
			// 异步方法全部会回掉到主线程
			OneThreadSynchronizationContext contex = new OneThreadSynchronizationContext();
			SynchronizationContext.SetSynchronizationContext(contex);
			
			try
			{
				Game.EventSystem.Add(DLLType.Model, typeof(Game).Assembly);
				Game.EventSystem.Add(DLLType.Hotfix, DllHelper.GetHotfixAssembly());

				Options options = Game.Scene.AddComponent<OptionComponent, string[]>(args).Options;
				StartConfig startConfig = Game.Scene.AddComponent<StartConfigComponent, string, int>(options.Config, options.AppId).StartConfig;

				if (!options.AppType.Is(startConfig.AppType))
				{
					Log.Error("命令行参数apptype与配置不一致");
					return;
				}

				IdGenerater.AppId = options.AppId;

				LogManager.Configuration.Variables["appType"] = startConfig.AppType.ToString();
				LogManager.Configuration.Variables["appId"] = startConfig.AppId.ToString();
				LogManager.Configuration.Variables["appTypeFormat"] = $"{startConfig.AppType,-8}";
				LogManager.Configuration.Variables["appIdFormat"] = $"{startConfig.AppId:D3}";

				Log.Info($"server start........................ {startConfig.AppId} {startConfig.AppType}");

				Game.Scene.AddComponent<OpcodeTypeComponent>();
				Game.Scene.AddComponent<MessageDispatherComponent>();

				// 根据不同的AppType添加不同的组件
				OuterConfig outerConfig = startConfig.GetComponent<OuterConfig>();
				InnerConfig innerConfig = startConfig.GetComponent<InnerConfig>();
				ClientConfig clientConfig = startConfig.GetComponent<ClientConfig>();
				
				switch (startConfig.AppType)
				{
					case AppType.Manager:
						Game.Scene.AddComponent<NetInnerComponent, IPEndPoint>(innerConfig.IPEndPoint);
						Game.Scene.AddComponent<NetOuterComponent, IPEndPoint>(outerConfig.IPEndPoint);
						Game.Scene.AddComponent<AppManagerComponent>();
						break;
					case AppType.Realm:
						Game.Scene.AddComponent<ActorMessageDispatherComponent>();
						Game.Scene.AddComponent<NetInnerComponent, IPEndPoint>(innerConfig.IPEndPoint);
						Game.Scene.AddComponent<NetOuterComponent, IPEndPoint>(outerConfig.IPEndPoint);
						Game.Scene.AddComponent<LocationProxyComponent>();
						Game.Scene.AddComponent<RealmGateAddressComponent>();
						break;
					case AppType.Gate:
						Game.Scene.AddComponent<PlayerComponent>();
						Game.Scene.AddComponent<ActorMessageDispatherComponent>();
						Game.Scene.AddComponent<NetInnerComponent, IPEndPoint>(innerConfig.IPEndPoint);
						Game.Scene.AddComponent<NetOuterComponent, IPEndPoint>(outerConfig.IPEndPoint);
						Game.Scene.AddComponent<LocationProxyComponent>();
						Game.Scene.AddComponent<ActorMessageSenderComponent>();
						Game.Scene.AddComponent<GateSessionKeyComponent>();
						break;
					case AppType.Location:
						Game.Scene.AddComponent<NetInnerComponent, IPEndPoint>(innerConfig.IPEndPoint);
						Game.Scene.AddComponent<LocationComponent>();
						break;
					case AppType.Map:
						Game.Scene.AddComponent<NetInnerComponent, IPEndPoint>(innerConfig.IPEndPoint);
						Game.Scene.AddComponent<UnitComponent>();
						Game.Scene.AddComponent<LocationProxyComponent>();
						Game.Scene.AddComponent<ActorMessageSenderComponent>();
						Game.Scene.AddComponent<ActorMessageDispatherComponent>();
						Game.Scene.AddComponent<ServerFrameComponent>();
						break;
					case AppType.AllServer:
						Game.Scene.AddComponent<ActorMessageSenderComponent>();
						Game.Scene.AddComponent<PlayerComponent>();
						Game.Scene.AddComponent<UnitComponent>();
						Game.Scene.AddComponent<DBComponent>();
						Game.Scene.AddComponent<DBProxyComponent>();
						Game.Scene.AddComponent<DBCacheComponent>();
						Game.Scene.AddComponent<LocationComponent>();
						Game.Scene.AddComponent<ActorMessageDispatherComponent>();
						Game.Scene.AddComponent<NetInnerComponent, IPEndPoint>(innerConfig.IPEndPoint);
						Game.Scene.AddComponent<NetOuterComponent, IPEndPoint>(outerConfig.IPEndPoint);
						Game.Scene.AddComponent<LocationProxyComponent>();
						Game.Scene.AddComponent<AppManagerComponent>();
						Game.Scene.AddComponent<RealmGateAddressComponent>();
						Game.Scene.AddComponent<GateSessionKeyComponent>();
						Game.Scene.AddComponent<ConfigComponent>();
						Game.Scene.AddComponent<ServerFrameComponent>();
						Game.Scene.AddComponent<TestComponent>();
						// Game.Scene.AddComponent<HttpComponent>();
						break;
					case AppType.Benchmark:
						Game.Scene.AddComponent<NetOuterComponent>();
						Game.Scene.AddComponent<BenchmarkComponent, IPEndPoint>(clientConfig.IPEndPoint);
						break;
					default:
						throw new Exception($"命令行参数没有设置正确的AppType: {startConfig.AppType}");
				}

				Test();

				while (true)
				{
					try
					{
						Thread.Sleep(1);
						contex.Update();
						Game.EventSystem.Update();
					}
					catch (Exception e)
					{
						Log.Error(e);
					}
				}
			}
			catch (Exception e)
			{
				Log.Error(e);
			}

			Console.Read();
		}

		static async void Test()
		{
			/*BsonClassMap.RegisterClassMap<XXX>();
			var cache = Game.Scene.GetComponent<DBProxyComponent>();
			//List<ComponentWithId> xx = await cache.Get<XXX>("XXX", (t) =>t.Fucker == 10000 && t.Lover >= 0);
			int kaka = 10000;
			//List<ComponentWithId> xx = await cache.Get<XXX>("XXX", (t) =>t.Fucker == 10000 && t.Lover >= 0);
			List<ComponentWithId> xx = await cache<XXX>("XXX", (t) => t.Id == 123);
			Log.Info(xx.Count.ToString());*/
		}
	}
}
