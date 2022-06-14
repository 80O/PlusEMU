using Plus.Core;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Subscriptions;
using System.Data;

namespace Plus.HabboHotel.Users.UserData
{
    internal class LoadStatisticsLoginTask : IUserDataLoadingTask
    {
        public Task Load(Habbo habbo)
        {
            DataRow statRow = null;
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    "SELECT `id`,`roomvisits`,`onlinetime`,`respect`,`respectgiven`,`giftsgiven`,`giftsreceived`,`dailyrespectpoints`,`dailypetrespectpoints`,`achievementscore`,`quest_id`,`quest_progress`,`groupid`,`tickets_answered`,`respectstimestamp`,`forum_posts` FROM `user_statistics` WHERE `id` = @user_id LIMIT 1");
                dbClient.AddParameter("user_id", habbo.Id);
                statRow = dbClient.GetRow();
                if (statRow == null) //No row, add it yo
                {
                    dbClient.RunQuery("INSERT INTO `user_statistics` (`id`) VALUES ('" + habbo.Id + "')");
                    dbClient.SetQuery(
                        "SELECT `id`,`roomvisits`,`onlinetime`,`respect`,`respectgiven`,`giftsgiven`,`giftsreceived`,`dailyrespectpoints`,`dailypetrespectpoints`,`achievementscore`,`quest_id`,`quest_progress`,`groupid`,`tickets_answered`,`respectstimestamp`,`forum_posts` FROM `user_statistics` WHERE `id` = @user_id LIMIT 1");
                    dbClient.AddParameter("user_id", habbo.Id);
                    statRow = dbClient.GetRow();
                }
                try
                {
                    var stats = new HabboStats(Convert.ToInt32(statRow["roomvisits"]), Convert.ToDouble(statRow["onlineTime"]), Convert.ToInt32(statRow["respect"]),
                        Convert.ToInt32(statRow["respectGiven"]), Convert.ToInt32(statRow["giftsGiven"]),
                        Convert.ToInt32(statRow["giftsReceived"]), Convert.ToInt32(statRow["dailyRespectPoints"]), Convert.ToInt32(statRow["dailyPetRespectPoints"]),
                        Convert.ToInt32(statRow["AchievementScore"]),
                        Convert.ToInt32(statRow["quest_id"]), Convert.ToInt32(statRow["quest_progress"]), Convert.ToInt32(statRow["groupid"]), Convert.ToString(statRow["respectsTimestamp"]),
                        Convert.ToInt32(statRow["forum_posts"]));
                    if (Convert.ToString(statRow["respectsTimestamp"]) != DateTime.Today.ToString("MM/dd"))
                    {
                        stats.RespectsTimestamp = DateTime.Today.ToString("MM/dd");
                        SubscriptionData subData = null;
                        var dailyRespects = 10;
                        //if (_permissions.HasRight("mod_tool"))
                        //    dailyRespects = 20;
                        //else if (PlusEnvironment.GetGame().GetSubscriptionManager().TryGetSubscriptionData(vipRank, out subData))
                        //    dailyRespects = subData.Respects;
                        stats.DailyRespectPoints = dailyRespects;
                        stats.DailyPetRespectPoints = dailyRespects;
                        dbClient.RunQuery("UPDATE `user_statistics` SET `dailyRespectPoints` = '" + dailyRespects + "', `dailyPetRespectPoints` = '" + dailyRespects + "', `respectsTimestamp` = '" +
                                          DateTime.Today.ToString("MM/dd") + "' WHERE `id` = '" + habbo.Id + "' LIMIT 1");
                    }
                    Group g = null;
                    if (!PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(stats.FavouriteGroupId, out g))
                        stats.FavouriteGroupId = 0;

                    habbo.SetStats(stats);
                }
                catch (Exception e)
                {
                    ExceptionLogger.LogException(e);
                }


            }

            return Task.CompletedTask;
        }
    }
}
