using X975.Radar.GameObjects.GatedWisps;
using X975.Radar.GameObjects.FishNodes;
using X975.Radar.GameObjects.LootChests;
using X975.Radar.GameObjects.LocalPlayer;
using X975.Radar.GameObjects.Dungeons;
using X975.Radar.GameObjects.Harvestables;
using X975.Radar.GameObjects.Mobs;
using X975.Radar.GameObjects.Players;
using X975.Radar.Packets.Handlers;
using Albion.Network;
using System.Threading;
using X975.Radar.Sniffer;
using X975.Radar.Drawing.Overlays;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using X975.Protocol.Connect.Messages.ResponseObj;

namespace X975.Radar
{
    public class Init
    {
        private readonly LocalPlayerHandler localPlayerHandler;
        private readonly PlayersHandler playersHandler;
        private readonly HarvestablesHandler harvestablesHandler;
        private readonly MobsHandler mobsHandler;
        private readonly DungeonsHandler dungeonsHandler;
        private readonly FishNodesHandler fishNodesHandler;
        private readonly GatedWispsHandler gatedWispsHandler;
        private readonly LootChestsHandler lootChestsHandler;

        private readonly PacketDeviceSelector packetSniffer;
        private readonly Thread globalTimer, radarOverlay, itemsOverlay, infoOverlay;

        private IPhotonReceiver photonReceiver;

        public static PacketIndexes PacketIndexes;
        public static PacketOffsets PacketOffsets;

        public Init()
        {
            PacketIndexes = ReadJson<PacketIndexes>("jsons/indexes.json");
            PacketOffsets = ReadJson<PacketOffsets>("jsons/offsets.json");

            #region HANDLERS

            localPlayerHandler = new LocalPlayerHandler(ReadJson<Dictionary<string, Cluster>>("jsons/clusters.json"));
            playersHandler = new PlayersHandler(ReadJson<List<PlayerItems>>("jsons/items.json"));
            harvestablesHandler = new HarvestablesHandler(ReadJson<Dictionary<int, string>>("jsons/harvestableTypes.json"), localPlayerHandler);
            mobsHandler = new MobsHandler(ReadJson<List<MobInfo>>("jsons/mobs.json"));
            dungeonsHandler = new DungeonsHandler();
            fishNodesHandler = new FishNodesHandler();
            gatedWispsHandler = new GatedWispsHandler();
            lootChestsHandler = new LootChestsHandler();

            #endregion

            #region PHOTON

            ReceiverBuilder builder = ReceiverBuilder.Create();

            builder.AddEventHandler(new LeaveEventHandler(playersHandler, mobsHandler, dungeonsHandler, fishNodesHandler, gatedWispsHandler, lootChestsHandler));
            builder.AddResponseHandler(new ChangeClusterEventHandler(localPlayerHandler, playersHandler, harvestablesHandler, mobsHandler, dungeonsHandler, fishNodesHandler, gatedWispsHandler, lootChestsHandler));
            builder.AddResponseHandler(new JoinResponseOperationHandler(localPlayerHandler, playersHandler, harvestablesHandler, mobsHandler, dungeonsHandler, fishNodesHandler, gatedWispsHandler, lootChestsHandler));
            builder.AddRequestHandler(new MoveRequestOperationHandler(localPlayerHandler, harvestablesHandler));
            builder.AddEventHandler(new MistsPlayerJoinedInfoEventHandler(localPlayerHandler));
            builder.AddEventHandler(new LoadClusterObjectsEventHandler(localPlayerHandler));
            builder.AddEventHandler(new NewCharacterEventHandler(playersHandler, localPlayerHandler));
            builder.AddEventHandler(new MountedEventHandler(playersHandler));
            builder.AddEventHandler(new ChangeFlaggingFinishedEventHandler(localPlayerHandler, playersHandler));
            builder.AddEventHandler(new CharacterEquipmentChangedEventHandler(playersHandler));
            builder.AddEventHandler(new MoveEventHandler(playersHandler, mobsHandler));
            builder.AddEventHandler(new HealthUpdateEventHandler(playersHandler, mobsHandler));
            builder.AddEventHandler(new RegenerationChangedEventHandler(playersHandler));//Разобраться с этим дерьмом
            builder.AddEventHandler(new NewHarvestableEventHandler(harvestablesHandler));
            builder.AddEventHandler(new NewHarvestablesListEventHandler(harvestablesHandler));
            builder.AddEventHandler(new HarvestableChangeStateEventHandler(harvestablesHandler));
            builder.AddEventHandler(new MobChangeStateEventHandler(mobsHandler));
            builder.AddEventHandler(new NewMobEventHandler(mobsHandler));
            builder.AddEventHandler(new NewFishingZoneEventHandler(fishNodesHandler));
            builder.AddEventHandler(new NewDungeonEventHandler(dungeonsHandler));
            builder.AddEventHandler(new NewGatedWispEventHandler(gatedWispsHandler));
            builder.AddEventHandler(new WispGateOpenedEventHandler(gatedWispsHandler));
            builder.AddEventHandler(new NewLootChestEventHandler(lootChestsHandler));

            photonReceiver = builder.Build();

            #endregion

            #region THREADS

            packetSniffer = new PacketDeviceSelector(photonReceiver);

            globalTimer = new Thread(() => new GlobalTimer(localPlayerHandler, playersHandler, mobsHandler).Start());

            radarOverlay = new Thread(() =>
            {
                GameOverlay.TimerService.EnableHighPrecisionTimers();

                using (RadarOverlay overlay = new RadarOverlay(localPlayerHandler, playersHandler, harvestablesHandler, mobsHandler, dungeonsHandler, fishNodesHandler, gatedWispsHandler, lootChestsHandler))
                {
                    overlay.Create();
                    overlay.Join();
                }
            });

            itemsOverlay = new Thread(() =>
            {
                GameOverlay.TimerService.EnableHighPrecisionTimers();

                using (ItemsOverlay overlay = new ItemsOverlay(localPlayerHandler, playersHandler))
                {
                    overlay.Create();
                    overlay.Join();
                }
            });

            infoOverlay = new Thread(() =>
            {
                GameOverlay.TimerService.EnableHighPrecisionTimers();

                using (InfoOverlay overlay = new InfoOverlay(localPlayerHandler))
                {
                    overlay.Create();
                    overlay.Join();
                }
            });

            #endregion
        }

        public void Start()
        {
            packetSniffer.Start();
            globalTimer.Start();
            radarOverlay.Start();
            itemsOverlay.Start();
            infoOverlay.Start();
        }

        private T ReadJson<T>(string file)
        {
            return JsonConvert.DeserializeObject<T>(File.ReadAllText(file));
        }
    }
}
