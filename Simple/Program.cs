using FluentDataWrapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Simple
{
    class Program
    {

        static void Main(string[] args)
        {

           
            Console.ReadKey();
        }

    }

    public class NewPartnerException
    {
        public NewPartnerException(DateTime dateTimeBefore, DateTime dateTime, string newpartner_ConnectionString = null, string mj_log_ConnectionString = null, string mj_cardshop_ConnectionString = null)
        {
            _dateTimeBefore = dateTimeBefore;
            _dateTime = dateTime;

            if (newpartner_ConnectionString != null) { _newpartner_ConnectionString = newpartner_ConnectionString; }
            if (mj_log_ConnectionString != null) { _mj_log_ConnectionString = mj_log_ConnectionString; }
            if (mj_cardshop_ConnectionString != null) { _mj_cardshop_ConnectionString = mj_cardshop_ConnectionString; }

        }

        public NewPartnerException(DateTime dateTime, string newpartner_ConnectionString = null, string mj_log_ConnectionString = null, string mj_cardshop_ConnectionString = null)
        {
            _dateTime = dateTime;
            _dateTimeBefore = dateTime.AddDays(-15);

            if (newpartner_ConnectionString != null) { _newpartner_ConnectionString = newpartner_ConnectionString; }
            if (mj_log_ConnectionString != null) { _mj_log_ConnectionString = mj_log_ConnectionString; }
            if (mj_cardshop_ConnectionString != null) { _mj_cardshop_ConnectionString = mj_cardshop_ConnectionString; }
        }

        private static string _newpartner_ConnectionString = "newpartner_statistics";
        private static string _mj_log_ConnectionString = "mySql_dataMenu";
        private static string _mj_cardshop_ConnectionString = "mj_cardshop_statistics";

        private static object obj = new object();
        private DateTime _dateTimeBefore { get; set; }
        private DateTime _dateTime { get; set; }

        #region Data

        private static p_usersumcashData<p_usersumcashModel> p_UsersumcashData
            = new p_usersumcashData<p_usersumcashModel>();
        private static p_gameRelationsData<p_gameRelationsModle> p_GameRelationsData
            = new p_gameRelationsData<p_gameRelationsModle>();
        private static game_gameStartUser_newData<game_gameStartUser_newModel> game_GameStartUser_NewData
            = new game_gameStartUser_newData<game_gameStartUser_newModel>();
        private static p_shareAwardsData<p_shareAwardsModel> p_ShareAwardsData
            = new p_shareAwardsData<p_shareAwardsModel>();
        private static game_gameCategoryData<game_gameCategoryModel> game_GameCategoryData
            = new game_gameCategoryData<game_gameCategoryModel>();
        private static p_inviteAwardsnewData<p_inviteAwardsnewModel> p_InviteAwardsnewData
            = new p_inviteAwardsnewData<p_inviteAwardsnewModel>();
        private static shop_agentData<shop_agentModle> shop_AgentData
            = new shop_agentData<shop_agentModle>();

        #endregion

        #region singleton

        private static List<p_usersumcashModel> _p_UsersumcashDatas;
        public List<p_usersumcashModel> p_UsersumcashDatas
        {
            get
            {
                lock (obj)
                {
                    if (_p_UsersumcashDatas == null)
                    {
                        _p_UsersumcashDatas = p_UsersumcashData.SelectList();
                    }
                    return _p_UsersumcashDatas;
                }
            }
        }

        private static List<shop_agentModle> _shop_AgentDatas;
        public List<shop_agentModle> shop_AgentDatas
        {
            get
            {
                lock (obj)
                {
                    if (_shop_AgentDatas == null)
                    {
                        _shop_AgentDatas = shop_AgentData.SelectList();
                    }
                    return _shop_AgentDatas;
                }
            }
        }

        private static List<game_gameCategoryModel> _game_GameCategoryDatas;
        public List<game_gameCategoryModel> game_GameCategoryDatas
        {
            get
            {
                lock (obj)
                {
                    if (_game_GameCategoryDatas == null)
                    {
                        _game_GameCategoryDatas = game_GameCategoryData.SelectList();
                    }
                    return _game_GameCategoryDatas;
                }
            }
        }

        private static List<p_gameRelationsModle> _p_GameRelationsDatas;
        public List<p_gameRelationsModle> p_GameRelationsDatas
        {
            get
            {
                lock (obj)
                {
                    if (_p_GameRelationsDatas == null)
                    {
                        _p_GameRelationsDatas = p_GameRelationsData
    .SelectList(String.Format("select SysNo,UID,ParentUID,CreateTime from p_gameRelations where CreateTime between '{0}' and '{1}'", _dateTimeBefore.ToString("yyyy-MM-dd HH:mm:ss.fff"), _dateTime.ToString("yyyy-MM-dd HH:mm:ss.fff")));
                    }
                    return _p_GameRelationsDatas;
                }
            }
        }

        private static List<p_inviteAwardsnewModel> _p_InviteAwardsnewDatas;
        public List<p_inviteAwardsnewModel> p_InviteAwardsnewDatas
        {
            get
            {
                lock (obj)
                {
                    if (_p_InviteAwardsnewDatas == null)
                    {
                        _p_InviteAwardsnewDatas = p_InviteAwardsnewData
    .SelectList(String.Format("select id,UID,Cash,UID,CreateTime from p_inviteAwardsnew where CreateTime between '{0}' and '{1}'", _dateTimeBefore.ToString("yyyy-MM-dd HH:mm:ss.fff"), _dateTime.ToString("yyyy-MM-dd HH:mm:ss.fff")));

                    }
                    return _p_InviteAwardsnewDatas;
                }
            }
        }

        private static List<p_shareAwardsModel> _p_ShareAwardsDatas;
        public List<p_shareAwardsModel> p_ShareAwardsDatas
        {
            get
            {
                lock (obj)
                {
                    if (_p_ShareAwardsDatas == null)
                    {
                        _p_ShareAwardsDatas = p_ShareAwardsData
    .SelectList(String.Format("select SysNo,GameID,PF,UID,CreateTime from p_shareAwards where CreateTime between '{0}' and '{1}'", _dateTimeBefore.ToString("yyyy-MM-dd HH:mm:ss.fff"), _dateTime.ToString("yyyy-MM-dd HH:mm:ss.fff")));
                    }
                    return _p_ShareAwardsDatas;
                }
            }
        }

        private static List<game_gameStartUser_newModel> _game_GameStartUser_NewDatas;
        public List<game_gameStartUser_newModel> game_GameStartUser_NewDatas
        {
            get
            {
                if (_game_GameStartUser_NewDatas == null)
                {
                    _game_GameStartUser_NewDatas = game_GameStartUser_NewData
    .SelectList(String.Format("select gameid,uid,endtime from game_gameStartUser_new where endtime between '{0}' and '{1}'", _dateTimeBefore.ToString("yyyy-MM-dd HH:mm:ss.fff"), _dateTime.ToString("yyyy-MM-dd HH:mm:ss.fff")));

                }
                return _game_GameStartUser_NewDatas;
            }
        }

        #endregion

        #region properties
        public List<p_shareAwardsException> ResultOfRuleNo1_1
        {
            get
            {
                return RuleNo1_1(p_ShareAwardsDatas);
            }
        }
        public List<p_shareAwardsModel> ResultOfRuleNo1_2
        {
            get
            {
                return RuleNo1_2(game_GameStartUser_NewDatas, game_GameCategoryDatas, p_ShareAwardsDatas, _dateTimeBefore);
            }
        }
        public List<userException> ResultOfRuleNo2_1
        {
            get { return RuleNo2_1(p_ShareAwardsDatas, p_InviteAwardsnewDatas); }
        }
        public List<InvitersException> ResultOfRuleNo2_2
        {
            get { return RuleNo2_2(p_UsersumcashDatas, shop_AgentDatas, p_ShareAwardsDatas, p_GameRelationsDatas, game_GameStartUser_NewDatas); }
        }
        #endregion

        #region static Methords

        public static List<p_shareAwardsException> RuleNo1_1(List<p_shareAwardsModel> p_ShareAwardsDatas)
        {
            #region 15天内的总分享次数大于等于15

            var p_ShareAwardsDataUnion = (from a in p_ShareAwardsDatas
                                          group a by a.UID into g
                                          select new
                                          {
                                              g.Key,
                                              count = g.Count()
                                          })
                                          .Where(c => c.count >= 15)
                                          .Select(c => c.Key)
                                          .ToList()
                                          ;

            #endregion

            #region 同一天、同一人在2个及以上APP分享

            return SameDayPersonTwoAppShare(p_ShareAwardsDatas.Where(c => p_ShareAwardsDataUnion.Contains(c.UID)).ToList());

            #endregion

        }
        public static List<p_shareAwardsModel> RuleNo1_2(List<game_gameStartUser_newModel> game_GameStartUser_NewDatas, List<game_gameCategoryModel> game_GameCategoryDatas, List<p_shareAwardsModel> p_ShareAwardsDatas, DateTime dateTimeBefore)
        {
            List<p_shareAwardsModel> ungameSharers = new List<p_shareAwardsModel>();

            #region 装配

            var GameCategoryMappings = (from b in (from a in game_GameCategoryDatas
                                                   where (a.parentCodeNo != "0")
                                                   select new
                                                   {
                                                       a.codeNo,
                                                       a.parentCodeNo
                                                   })
                                        from c in game_GameCategoryDatas
                                        where (b.parentCodeNo == c.codeNo)
                                        select new
                                        {
                                            b.codeNo,
                                            c.containsCode
                                        }).ToList();

            #endregion

            #region 组局
            List<GameOrRaceUnion> game_gameStartUser_newUnions = new List<GameOrRaceUnion>();
            foreach (var item in game_GameStartUser_NewDatas)
            {
                var temp = GameCategoryMappings.Find(c => c.codeNo == item.gameid.ToString());
                if (temp != null)
                {
                    game_gameStartUser_newUnions.Add(new GameOrRaceUnion
                    {
                        PF = temp.containsCode,
                        endtime = item.endtime,
                        uid = item.uid
                    });
                }
            }

            #endregion

            #region 比赛
            game_raceRankData<game_raceRankModel> game_RaceRankData = new game_raceRankData<game_raceRankModel>();
            var game_RaceRankDatas = game_RaceRankData.SelectList();
            List<GameOrRaceUnion> game_raceRankUnions = new List<GameOrRaceUnion>();
            foreach (var item in game_RaceRankDatas)
            {
                var temp = game_GameCategoryDatas.Find(c => c.codeNo == item.gameid.ToString());
                if (temp != null)
                {
                    game_raceRankUnions.Add(new GameOrRaceUnion
                    {
                        PF = temp.containsCode,
                        uid = item.uid,
                        endtime = item.overtime
                    });
                }
            }
            game_gameStartUser_newUnions.AddRange(game_raceRankUnions);
            #endregion

            #region 计算 导出找不到
            for (var i = 0; i < 15; i++)
            {
                ungameContinuousThreeDays(ungameSharers, dateTimeBefore.AddDays(i), p_ShareAwardsDatas, game_gameStartUser_newUnions);
            }
            #endregion

            return ungameSharers;
        }
        public static List<userException> RuleNo2_1(List<p_shareAwardsModel> p_ShareAwardsDatas, List<p_inviteAwardsnewModel> p_InviteAwardsnewDatas)
        {

            List<userException> userExceptions = new List<userException>();

            #region 用户有在大于等于2个APP进行邀请拉新

            // 按PF和UID分组，得到同一个PF与同一个UID的合计次数分组
            var groupBysamePFandUID = (from a in p_ShareAwardsDatas
                                       group a by new { a.PF, a.UID }
                                       into g
                                       select new
                                       {
                                           g.Key.PF,
                                           g.Key.UID,
                                           count = g.Count()
                                       }).ToList();

            //先装配，后过滤
            foreach (var item in groupBysamePFandUID)
            {
                var temp = userExceptions.Find(c => c.uid == item.UID);
                if (temp == null)
                {
                    userExceptions.Add(new userException
                    {
                        apps = new List<string>() {
                            item.PF
                        },
                        uid = item.UID,
                        cashSum = 0
                    });
                }
                else
                {
                    temp.apps.Add(item.PF);
                }
            }
            userExceptions = userExceptions.Where(c => c.apps.Count >= 2).ToList();

            #endregion

            #region 获得奖励大于等于100元

            //合计p_InviteAwardsnew

            foreach (var item in userExceptions)
            {
                int total = 0;
                p_InviteAwardsnewDatas.Where(c => c.UID == item.uid).ToList().ForEach(c => total += c.Cash);
                item.cashSum += total;
            }


            userExceptions = userExceptions.Where(c => c.cashSum >= 10000).ToList();

            #endregion

            return userExceptions;
        }
        public static List<InvitersException> RuleNo2_2(List<p_usersumcashModel> p_UsersumcashDatas, List<shop_agentModle> shop_AgentDatas, List<p_shareAwardsModel> p_ShareAwardsDatas, List<p_gameRelationsModle> p_GameRelationsDatas, List<game_gameStartUser_newModel> game_GameStartUser_NewDatas)
        {

            List<InvitersException> InvitersExceptions = new List<InvitersException>();

            #region 用户（非代理）

            var agentWithUid = (from a in p_UsersumcashDatas
                                from b in shop_AgentDatas
                                where a.mobile == b.businessmobile
                                select new
                                {
                                    a.uid
                                }).Distinct().Select(c => c.uid).ToList();

            var users = p_ShareAwardsDatas.Where(c => !agentWithUid.Contains(c.UID)).Select(c => c.UID).ToList();

            #endregion

            #region 邀请人数大于等于4个且被邀请人在24小时内（以绑定关系的时间开始计算）只完成了10场游戏

            //被邀请人数大于等于7个 的邀请人集合
            var Inviters = (from a in p_GameRelationsDatas
                            group a by a.ParentUID into p
                            select new
                            {
                                p.Key,
                                count = p.Count()
                            }).Where(c => c.count >= 7).Select(c => c.Key).ToList();


            var Invitees = p_GameRelationsDatas.Where(c => Inviters.Contains(c.ParentUID)).ToList();

            foreach (var item in Invitees)
            {
                //被邀请人在24小时内（以绑定关系的时间开始计算）只完成了10场游戏
                if (game_GameStartUser_NewDatas.Where(c => c.endtime.Day == item.CreateTime.Day && c.uid == item.UID).Count() == 10)
                {

                    var temp = new InvitersException
                    {
                        uid = item.ParentUID,//这里需要 邀请人的UID
                        gameStarts = new List<game_gameStartUser_newModel>()
                    };
                    foreach (var bottom in game_GameStartUser_NewDatas.Where(c => c.endtime.Day == item.CreateTime.Day && c.uid == item.UID).ToList())
                    {
                        temp.gameStarts.Add(bottom);
                    }
                    InvitersExceptions.Add(temp);

                }
            }

            #endregion

            //筛选出非代理的用户
            return InvitersExceptions.Where(c => users.Contains(c.uid)).ToList();

        }

        #endregion

        #region private Methords

        private static void ungameContinuousThreeDays(List<p_shareAwardsModel> ungameSharers, DateTime beginDay, List<p_shareAwardsModel> p_ShareAwardsDatas, List<GameOrRaceUnion> game_gameStartUser_newUnions)
        {
            var firstDay = beginDay;
            var secondDay = beginDay.AddDays(1);
            var thirdDay = beginDay.AddDays(2);
            foreach (var item in p_ShareAwardsDatas.Where(c => c.CreateTime.Day == beginDay.Day).ToList())
            {

                var temps = game_gameStartUser_newUnions.FindAll(c => c.PF == item.PF && c.uid == item.UID);
                if (temps.Count == 0)
                {
                    //15天内 未组局
                    ungameSharers.Add(item);
                }
                else
                {
                    if (temps.Find(c => c.endtime.Day == firstDay.Day) != null
                     || temps.Find(c => c.endtime.Day == secondDay.Day) != null
                     || temps.Find(c => c.endtime.Day == thirdDay.Day) != null)
                    {
                        //3天内 未组局
                        ungameSharers.Add(item);
                    }
                }
            }
        }
        private static List<p_shareAwardsException> SameDayPersonTwoAppShare(List<p_shareAwardsModel> p_shareAwardsModels)
        {
            var groups = (from a in p_shareAwardsModels
                          orderby a.UID, a.CreateTime
                          select a).ToList();

            long UIDbefore = 0;
            DateTime CreateTimeBefore = DateTime.MinValue;
            string PFBefore = null;
            int PFcount = 1;

            List<p_shareAwardsException> exceptioners = new List<p_shareAwardsException>();
            foreach (var item in groups)
            {
                //第一次
                if (UIDbefore == 0
                    && CreateTimeBefore == DateTime.MinValue
                    && PFBefore == null)
                {
                    UIDbefore = item.UID;
                    CreateTimeBefore = item.CreateTime;
                    PFBefore = item.PF;
                }
                else //第二次
                {
                    if (item.UID == UIDbefore
                     && item.CreateTime.Day == CreateTimeBefore.Day
                     && item.PF != PFBefore
                        )
                    {
                        PFcount++;
                    }

                    if (PFcount >= 2)
                    {
                        PFcount = 0;
                        exceptioners.Add(new p_shareAwardsException
                        {
                            day = item.CreateTime,
                            uid = item.UID,
                            apps = new List<string> {
                            item.PF,
                            PFBefore
                        }
                        });
                    }

                    //保留值
                    UIDbefore = item.UID;
                    PFBefore = item.PF;
                    CreateTimeBefore = item.CreateTime;
                }
            }

            return exceptioners;
        }

        #endregion

        #region Data Model and else


        #region Data

        public class p_usersumcashData<p_usersumcashModel> : FluentDataBase<p_usersumcashModel>
        {
            public override string connectionString => _newpartner_ConnectionString;
            public override string tableName => "p_usersumcash";
        }
        public class p_gameRelationsData<p_gameRelationsModle> : FluentDataBase<p_gameRelationsModle>
        {
            public override string connectionString => _newpartner_ConnectionString;
            public override string tableName => "p_gameRelations";
        }
        public class p_inviteAwardsnewData<p_inviteAwardsnewModel> : FluentDataBase<p_inviteAwardsnewModel>
        {
            public override string connectionString => _newpartner_ConnectionString;
            public override string tableName => "p_inviteAwardsnew";
        }
        public class p_shareAwardsData<p_shareAwardsModel> : FluentDataBase<p_shareAwardsModel>
        {
            public override string connectionString => _newpartner_ConnectionString;
            public override string tableName => "p_shareAwards";
        }
        public class game_gameStartUser_newData<game_gameStartUser_newModel> : FluentDataBase<game_gameStartUser_newModel>
        {
            public override string connectionString => _mj_log_ConnectionString;

            public override string tableName => "game_gameStartUser_new";
        }
        public class game_gameCategoryData<game_gameCategoryModel> : FluentDataBase<game_gameCategoryModel>
        {
            public override string connectionString => _mj_log_ConnectionString;

            public override string tableName => "game_gameCategory";
        }
        public class game_raceRankData<game_raceRankModel> : FluentDataBase<game_raceRankModel>
        {
            public override string connectionString => _mj_log_ConnectionString;
            public override string tableName => "game_raceRank";
        }
        public class shop_agentData<shop_agentModle> : FluentDataBase<shop_agentModle>
        {
            public override string connectionString => _mj_cardshop_ConnectionString;
            public override string tableName => "shop_agent";
        }

        #endregion

        #region Model

        public class p_usersumcashModel
        {
            [PrimaryKey]
            public int id { get; set; }

            public long uid { get; set; }

            public string mobile { get; set; }
        }
        public class p_gameRelationsModle
        {
            [PrimaryKey]
            public string SysNo { get; set; }
            public long UID { get; set; }
            public long ParentUID { get; set; }
            public DateTime CreateTime { get; set; }
        }
        public class p_inviteAwardsnewModel
        {
            [PrimaryKey]
            public int id { get; set; }
            public long UID { get; set; }
            public int Cash { get; set; }
            public DateTime CreateTime { get; set; }
        }
        public class p_shareAwardsModel
        {
            [PrimaryKey]
            public string SysNo { get; set; }

            public int GameID { get; set; }

            public string PF { get; set; }

            public long UID { get; set; }

            public DateTime CreateTime { get; set; }

        }
        public class game_gameStartUser_newModel
        {
            public int gameid { get; set; }
            public long uid { get; set; }
            public DateTime endtime { get; set; }
        }
        public class game_gameCategoryModel
        {

            public string codeNo { get; set; }

            public string parentCodeNo { get; set; }

            public string containsCode { get; set; }

        }
        public class game_raceRankModel
        {
            [PrimaryKey]
            public long id { get; set; }
            public int gameid { get; set; }
            public long uid { get; set; }
            public DateTime overtime { get; set; }
        }
        public class shop_agentModle
        {
            [PrimaryKey]
            public string sysNo { get; set; }

            public string businessmobile { get; set; }

        }

        #endregion

        public class p_shareAwardsException
        {
            public DateTime day { get; set; }
            public long uid { get; set; }
            public List<string> apps { get; set; }
        }
        public class userException
        {
            public long uid { get; set; }
            public List<string> apps { get; set; }
            public int cashSum { get; set; }
        }
        public class InvitersException
        {

            public long uid { get; set; } // 邀请人的UID
            public List<game_gameStartUser_newModel> gameStarts { get; set; }//被邀请人完成的局数信息

        }
        public class GameOrRaceUnion
        {

            public long uid { get; set; }
            public DateTime endtime { get; set; }
            public string PF { get; set; }

        }

        #endregion

    }

    #region How to use NewPartnerException

    //DateTime dateTime = new DateTime(2018, 2, 10);
    //DateTime dateTimeBefore = dateTime.AddDays(-15);

    //NewPartnerException newpartnerException = new NewPartnerException(dateTimeBefore, dateTime);

    //一、程序判断（15天内）
    //备注：以下条件满足一个，然后查询绑定的手机号是否归属于游戏地区，如果不属于，即初步判断为异常。

    // 1.每日分享异常判断：
    // 1)同一天、同一人在2个及以上APP分享，且15天内的总分享次数大于等于15
    //var a = newpartnerException.ResultOfRuleNo1_1;

    // 2)连续3天及以上仅分享不游戏（未组局或者未比赛）
    //var b = newpartnerException.ResultOfRuleNo1_2;

    // 2.有奖邀请异常判断：
    // 1)用户有在大于等于2个APP进行邀请拉新，且获得奖励大于等于100元
    //var c = newpartnerException.ResultOfRuleNo2_1;

    // 2)用户（非代理）邀请人数大于等于4个且被邀请人在24小时内（以绑定关系的时间开始计算）完成10场游戏，同时被邀请人全部都完成10场后未进行第11场游戏
    //var d = newpartnerException.ResultOfRuleNo2_2;

    #endregion


}
