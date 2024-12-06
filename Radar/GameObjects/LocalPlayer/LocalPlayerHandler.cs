using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using X975.Radar.Utility;

namespace X975.Radar.GameObjects.LocalPlayer
{
    [Obfuscation(Feature = "mutation", Exclude = false)]
    public class LocalPlayerHandler
    {
        public LocalPlayer localPlayer = new LocalPlayer();

        private readonly Dictionary<string, Cluster> clusterList = new Dictionary<string, Cluster>();

        public LocalPlayerHandler(Dictionary<string, Cluster> clusterList)
        {
            this.clusterList = clusterList;
        }

        public bool ChangeCluster(string id)
        {
            lock (localPlayer)
            {
                if (localPlayer.CurrentCluster.Id == id)
                    return false;

                if (clusterList != null && clusterList.ContainsKey(id))
                {
                    Cluster cluster = clusterList[id];

                    localPlayer.CurrentCluster.Id = id;
                    localPlayer.CurrentCluster.DisplayName = cluster.DisplayName;
                    localPlayer.CurrentCluster.ClusterColor = cluster.ClusterColor;
                    
                }
                else
                {
                    localPlayer.CurrentCluster.ClusterColor = ClusterColor.Unknown;
                    localPlayer.CurrentCluster.DisplayName = "Unknown";
                }

                if (id.Contains("@"))
                {
                    string[] temp = id.Split('@');

                    if (string.IsNullOrEmpty(temp[1]) || string.IsNullOrEmpty(temp[2]))
                    {
                        localPlayer.CurrentCluster.Subtype = ClusterSubtype.Unknown;
                        localPlayer.CurrentCluster.LobbyID = string.Empty;
                    }

                    switch (temp[1])
                    {
                        case "MISTS":
                            localPlayer.CurrentCluster.DisplayName = "Mists Dungeon";
                            localPlayer.CurrentCluster.Subtype = ClusterSubtype.Mist;
                            break;

                        case "MISTSDUNGEON":
                            localPlayer.CurrentCluster.DisplayName = "Knightfall Abbey";
                            localPlayer.CurrentCluster.Subtype = ClusterSubtype.Abbey;
                            break;

                        default:
                            localPlayer.CurrentCluster.Subtype = ClusterSubtype.Unknown;
                            break;
                    }

                    localPlayer.CurrentCluster.LobbyID = temp[2];
                }
                else
                {
                    localPlayer.CurrentCluster.Subtype = ClusterSubtype.Unknown;
                    localPlayer.CurrentCluster.LobbyID = string.Empty;
                }

                return true;
            }
        }

        public void UpdateClusterObjectives(Dictionary<int, ClusterObjective> clusterObjectives)
        {
            lock (localPlayer)
            {
                localPlayer.CurrentCluster.ClusterObjectives = clusterObjectives;
            }
        }

        public void UpdateClusterTimeCycle(DateTime timeCycle)
        {
            lock (localPlayer)
            {
                localPlayer.CurrentCluster.TimeCycle = timeCycle;
            }
        }

        public void UpdateInfo(int id, string nickname, string guild, string alliance, Faction faction, Vector2 position)
        {
            lock (localPlayer)
            {
                localPlayer.Id = id;
                localPlayer.Nickname = nickname;
                localPlayer.Guild = guild.Length > 1 ? guild : "!!!!";
                localPlayer.Alliance = alliance.Length > 1 ? alliance : "!!!!";

                localPlayer.Faction = faction;

                localPlayer.IsStanding = true;
                localPlayer.Position = position;
            }
        }

        public void Move(Vector2 position, Vector2 newPosition, float speed, DateTime time)
        {
            lock (localPlayer)
            {
                localPlayer.IsStanding = (localPlayer.Position - position).Magnitude() <= 0.05;
                localPlayer.Position = position;
                localPlayer.Speed = speed;
                localPlayer.Time = time;
                localPlayer.NewPosition = newPosition;
            }
        }

        public void SetFaction(int id, Faction faction)
        {
            lock (localPlayer)
            {
                if (localPlayer.Id == id)
                    localPlayer.Faction = faction;
            }
        }

        public void SyncPosition()
        {
            lock (localPlayer)
            {
                if (localPlayer.IsStanding) return;

                Vector2 posDiff = localPlayer.Position - localPlayer.NewPosition;

                if (posDiff == Vector2.Zero) return;

                localPlayer.Position -= posDiff * (float)((DateTime.UtcNow - localPlayer.Time).TotalSeconds / (posDiff.Magnitude() / (localPlayer.Speed / 10)));
            } 
        }
    }
}
