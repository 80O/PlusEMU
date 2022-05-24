namespace Plus.Communication.Packets.Incoming;

public static class ClientPacketHeader
{
    // Handshake
    public const int InitCryptoEvent = 3110; //316
    public const int GenerateSecretKeyEvent = 773; //3847
    public const int UniqueIdEvent = 2490; //1471
    public const int SsoTicketEvent = 2419; //1778
    public const int InfoRetrieveEvent = 357; //186

    // Avatar
    public const int GetWardrobeEvent = 2742; //765
    public const int SaveWardrobeOutfitEvent = 800; //55

    // Catalog
    public const int GetCatalogIndexEvent = 223; //1294
    public const int GetCatalogPageEvent = 412; //39
    public const int PurchaseFromCatalogEvent = 3492; //2830
    public const int PurchaseFromCatalogAsGiftEvent = 1411; //21

    // Navigator

    // Messenger
    public const int GetBuddyRequestsEvent = 2448; //2485

    // Quests
    public const int GetQuestListEvent = 3333; //2305
    public const int StartQuestEvent = 3604; //1282
    public const int GetCurrentQuestEvent = 2750; //90
    public const int CancelQuestEvent = 2397; //3879

    // Room Avatar
    // TODO: Add Nitro Support
    public const int ActionEvent = 3268; //3639
    public const int ApplySignEvent = 1975; //2966
    public const int DanceEvent = 2080; //645
    public const int SitEvent = 2235; //1565
    public const int ChangeMottoEvent = 2228; //3515
    public const int LookToEvent = 3301; //3744
    public const int DropHandItemEvent = 2814; //1751

    // Room Connection
    public const int OpenFlatConnectionEvent = 2312; //407
    public const int GoToFlatEvent = 685; //1601

    // Room Chat
    public const int ChatEvent = 1314; //670
    public const int ShoutEvent = 2085; //2101
    public const int WhisperEvent = 1543; //878

    // Room Engine

    // Room Furniture

    public const int OneWayGateEvent = 2765;

    // Room Settings

    // Room Action

    // Users
    public const int GetIgnoredUsersEvent = 3878;

    // Moderation
    public const int OpenHelpToolEvent = 3267; //1839
    public const int CallForHelpPendingCallsDeletedEvent = 3605;
    public const int ModeratorActionEvent = 3842; //781
    public const int ModerationMsgEvent = 1840; //2375
    public const int ModerationMuteEvent = 1945; //1940
    public const int ModerationTradeLockEvent = 3742; //1160
    public const int GetModeratorUserRoomVisitsEvent = 3526; //730
    public const int ModerationKickEvent = 2582; //3589
    public const int GetModeratorRoomInfoEvent = 707; //182
    public const int GetModeratorUserInfoEvent = 3295; //2984
    public const int GetModeratorRoomChatlogEvent = 2587; //2312
    public const int ModerateRoomEvent = 3260; //3458
    public const int GetModeratorUserChatlogEvent = 1391; //695
    public const int GetModeratorTicketChatlogsEvent = 211; //3484
    public const int ModerationCautionEvent = 229; //505
    public const int ModerationBanEvent = 2766; //2595
    public const int SubmitNewTicketEvent = 1691; //963
    public const int CloseIssueDefaultActionEvent = 2717;

    // Inventory
    public const int GetCreditsInfoEvent = 273; //3697
    public const int GetAchievementsEvent = 219; //2931
    public const int GetBadgesEvent = 2769; //166
    public const int RequestFurniInventoryEvent = 3150; //352
    public const int SetActivatedBadgesEvent = 644; //2752
    public const int AvatarEffectActivatedEvent = 2959; //129
    public const int AvatarEffectSelectedEvent = 1752; //628

    public const int InitTradeEvent = 1481; //3313
    public const int TradingCancelConfirmEvent = 2341; //2264
    public const int TradingModifyEvent = 1444; //1153
    public const int TradingOfferItemEvent = 1263; //114
    public const int TradingCancelEvent = 2551; //2967
    public const int TradingConfirmCancelEvent = 2341; //2399
    public const int TradingOfferItemsEvent = 1263; //2996
    public const int TradingRemoveItemEvent = 3845; //1033
    public const int TradingAcceptEvent = 3863; //3374

    // Register
    public const int UpdateFigureDataEvent = 2730; //2560

    // Groups
    public const int GetBadgeEditorPartsEvent = 813; //1670
    public const int GetGroupCreationWindowEvent = 798; //468
    public const int GetGroupFurniSettingsEvent = 367; //41
    public const int DeclineGroupMembershipEvent = 1894; //403
    public const int JoinGroupEvent = 998; //2615
    public const int UpdateGroupColoursEvent = 1764; //1443
    public const int SetGroupFavouriteEvent = 3549; //2625
    public const int GetGroupMembersEvent = 312; //205

    // Group Forums
    public const int PostGroupContentEvent = 3529; //477
    public const int GetForumStatsEvent = 3149; //872

    // Sound


    // Ambassador

    public const int AmbassadorSendAlertEvent = 2996;

    public const int RemoveMyRightsEvent = 3182; //879
    public const int GiveHandItemEvent = 2941; //3315
    public const int GetClubGiftsEvent = 487; //3302
    public const int GoToHotelViewEvent = 105; //3576
    public const int GetRoomFilterListEvent = 1911; //1348
    public const int GetPromoArticlesEvent = 1827; //3895
    public const int ModifyWhoCanRideHorseEvent = 1472; //1993
    public const int RemoveBuddyEvent = 1689; //698
    public const int RefreshCampaignEvent = 2912; //3544
    public const int AcceptBuddyEvent = 137; //45
    public const int YouTubeVideoInformationEvent = 790; //2395
    public const int FollowFriendEvent = 3997; //2280
    public const int SaveBotActionEvent = 2624; //678g
    public const int LetUserInEvent = 1644; //2356
    public const int GetMarketplaceItemStatsEvent = 3288; //1203
    public const int GetSellablePetBreedsEvent = 1756; //2505
    public const int ForceOpenCalendarBoxEvent = 3889; //2879
    public const int SetFriendBarStateEvent = 2313; //716
    public const int DeleteRoomEvent = 532; //722
    public const int SetSoundSettingsEvent = 1367; //3820
    public const int InitializeGameCenterEvent = 2399; //751
    public const int RedeemOfferCreditsEvent = 2650; //1207
    public const int FriendListUpdateEvent = 1419; //2664
    public const int ConfirmLoveLockEvent = 3775; //2082
    public const int UseHabboWheelEvent = 2144; //2651
    public const int SaveRoomSettingsEvent = 1969; //2074
    public const int ToggleMoodlightEvent = 2296; //1826
    public const int GetDailyQuestEvent = 1343; //484
    public const int SetMannequinNameEvent = 2850; //2406
    public const int UseOneWayGateEvent = 2765; //2816
    public const int EventTrackerEvent = 3457; //2386
    public const int FloorPlanEditorRoomPropertiesEvent = 1687; //24
    public const int PickUpPetEvent = 1581; //2342
    public const int GetPetInventoryEvent = 3095; //263
    public const int InitializeFloorPlanSessionEvent = 3559; //2623
    public const int GetOwnOffersEvent = 2105; //3829
    public const int CheckPetNameEvent = 2109; //159
    public const int SetUserFocusPreferenceEvent = 1461; //526
    public const int SubmitBullyReportEvent = 3060; //1803
    public const int RemoveRightsEvent = 2064; //40
    public const int MakeOfferEvent = 848; //255
    public const int KickUserEvent = 1320; //3929
    public const int GetRoomSettingsEvent = 3129; //1014
    public const int GetThreadsListDataEvent = 436; //1606
    public const int GetForumUserProfileEvent = 2249; //2639
    public const int SaveWiredEffectConfigEvent = 2281; //3431
    public const int GetRoomEntryDataEvent = 2300; //2768
    public const int JoinPlayerQueueEvent = 1458; //951
    public const int CanCreateRoomEvent = 2128; //361
    public const int SetTonerEvent = 2880; //1061
    public const int SaveWiredTriggerConfigEvent = 1520; //1897
    public const int PlaceBotEvent = 1592; //2321
    public const int GetRelationshipsEvent = 2138; //866
    public const int SetMessengerInviteStatusEvent = 1086; //1379
    public const int UseFurnitureEvent = 99; //3846
    public const int GetUserFlatCatsEvent = 3027; //3672
    public const int AssignRightsEvent = 808; //3574
    public const int GetRoomBannedUsersEvent = 2267; //581
    public const int ReleaseTicketEvent = 1572; //3800
    public const int OpenPlayerProfileEvent = 3265; //3591
    public const int GetSanctionStatusEvent = 2746; //2883
    public const int CreditFurniRedeemEvent = 3115; //1676
    public const int DisconnectEvent = 2445; //2391
    public const int PickupObjectEvent = 2445; //636
    public const int FindRandomFriendingRoomEvent = 1703; //1874
    public const int UseSellableClothingEvent = 3374; //818
    public const int MoveObjectEvent = 248; //1781
    public const int GetFurnitureAliasesEvent = 3898; //2125
    public const int TakeAdminRightsEvent = 722; //2725
    public const int ModifyRoomFilterListEvent = 3001; //256
    public const int MoodlightUpdateEvent = 1648; //856
    public const int GetPetTrainingPanelEvent = 2161; //2088
    public const int GetSongInfoEvent = 3082; //3418
    public const int UseWallItemEvent = 210; //3396
    public const int GetTalentTrackEvent = 196; //1284
    public const int GiveAdminRightsEvent = 2894; //465
    public const int GetCatalogModeEvent = 1195; //2267
    public const int SendBullyReportEvent = 3786; //2973
    public const int CancelOfferEvent = 434; //1862
    public const int SaveWiredConditionConfigEvent = 3203; //488
    public const int RedeemVoucherEvent = 339; //489
    public const int ThrowDiceEvent = 1990; //1182
    public const int CraftSecretEvent = 1251; //1622
    public const int GetGameListingEvent = 741; //2993
    public const int SetRelationshipEvent = 3768; //2112
    public const int RequestBuddyEvent = 3157; //3775
    public const int MemoryPerformanceEvent = 3230; //731
    public const int ToggleYouTubeVideoEvent = 2069; //890
    public const int SetMannequinFigureEvent = 2209; //3936
    public const int GetEventCategoriesEvent = 1782; //1086
    public const int DeleteGroupThreadEvent = 1397; //3299
    public const int PurchaseGroupEvent = 230; //2546
    public const int MessengerInitEvent = 2781; //2151
    public const int CancelTypingEvent = 1474; //1114
    public const int GetMoodlightConfigEvent = 2813; //3472
    public const int GetGroupInfoEvent = 2991; //3211
    public const int CreateFlatEvent = 2752; //3077
    public const int LatencyTestEvent = 295; //1789
    public const int GetSelectedBadgesEvent = 2091; //2226
    public const int AddStickyNoteEvent = 2248; //425
    public const int ChangeNameEvent = 2977; //1067
    public const int RideHorseEvent = 1036; //1440
    public const int InitializeNewNavigatorEvent = 2110; //882
    public const int SetChatPreferenceEvent = 1262; //2006
    public const int GetForumsListDataEvent = 873; //3912
    public const int ToggleMuteToolEvent = 3637; //2462
    public const int UpdateGroupIdentityEvent = 3137; //1062
    public const int UpdateStickyNoteEvent = 3666; //342
    public const int UnbanUserFromRoomEvent = 992; //3060
    public const int UnIgnoreUserEvent = 2061; //3023
    public const int OpenGiftEvent = 3558; //1515
    public const int ApplyDecorationEvent = 711; //728
    public const int GetRecipeConfigEvent = 633; //3654
    public const int ScrGetUserInfoEvent = 3166; //12
    public const int RemoveGroupMemberEvent = 3593; //649
    public const int DiceOffEvent = 1533; //191
    public const int YouTubeGetNextVideo = 3337; //1843
    public const int RemoveFavouriteRoomEvent = 309; //855
    public const int RespectUserEvent = 2694; //1955
    public const int AddFavouriteRoomEvent = 3817; //3092
    public const int DeclineBuddyEvent = 2890; //835
    public const int StartTypingEvent = 1597; //3362
    public const int GetGroupFurniConfigEvent = 367; //3046
    public const int SendRoomInviteEvent = 1276; //2694
    public const int RemoveAllRightsEvent = 2683; //1404
    public const int GetYouTubeTelevisionEvent = 336; //3517
    public const int FindNewFriendsEvent = 516; //1264
    public const int GetPromotableRoomsEvent = 2283; //276
    public const int GetBotInventoryEvent = 3848; //363
    public const int GetRentableSpaceEvent = 872; //793
    public const int OpenBotActionEvent = 1986; //2544
    public const int OpenCalendarBoxEvent = 2257; //724
    public const int DeleteGroupPostEvent = 286; //317
    public const int CheckValidNameEvent = 3950; //8
    public const int UpdateGroupBadgeEvent = 1991; //2959
    public const int PlaceObjectEvent = 1258; //579
    public const int RemoveGroupFavouriteEvent = 1820; //1412
    public const int UpdateNavigatorSettingsEvent = 1740; //2501
    public const int CheckGnomeNameEvent = 3698; //2281
    public const int NavigatorSearchEvent = 249; //2722
    public const int GetPetInformationEvent = 2934; //2853
    public const int GetGuestRoomEvent = 2230; //1164
    public const int UpdateThreadEvent = 3045; //1522
    public const int AcceptGroupMembershipEvent = 3386; //2259
    public const int GetMarketplaceConfigurationEvent = 2597; //1604
    public const int Game2GetWeeklyLeaderboardEvent = 2565; //2106
    public const int BuyOfferEvent = 1603; //3699
    public const int RemoveSaddleFromHorseEvent = 186; //1892
    public const int GiveRoomScoreEvent = 3582; //336
    public const int GetHabboClubWindowEvent = 3285; //715
    public const int DeleteStickyNoteEvent = 3336; //2777
    public const int MuteUserEvent = 3485; //2997
    public const int ApplyHorseEffectEvent = 1328; //870
    public const int GetClientVersionEvent = 4000; //4000
    public const int OnBullyClickEvent = 2455; //1932
    public const int HabboSearchEvent = 1210; //3375
    public const int PickTicketEvent = 15; //3973
    public const int GetGiftWrappingConfigurationEvent = 418; //1928
    public const int GetCraftingRecipesAvailableEvent = 3086; //1653
    public const int GetThreadDataEvent = 2324; //1559
    public const int ManageGroupEvent = 1004; //2547
    public const int PlacePetEvent = 2647; //223
    public const int EditRoomPromotionEvent = 3991; //3707
    public const int GetCatalogOfferEvent = 2594; //2180
    public const int SaveFloorPlanModelEvent = 875; //1287
    public const int MoveWallItemEvent = 168; //609
    public const int ClientVariablesEvent = 1053; //1600
    // TODO: Add support for Nitro
    public const int PingEvent = 509; //2584
    public const int DeleteGroupEvent = 1134; //747
    public const int UpdateGroupSettingsEvent = 3435; //3180
    public const int GetRecyclerRewardsEvent = 398; //3258
    public const int PurchaseRoomPromotionEvent = 777; //3078
    public const int PickUpBotEvent = 3323; //644
    public const int GetOffersEvent = 2407; //442
    public const int GetHabboGroupBadgesEvent = 21; //301
    public const int GetUserTagsEvent = 17; //1722
    public const int GetPlayableGamesEvent = 389; //482
    public const int GetCatalogRoomPromotionEvent = 957; //538
    public const int MoveAvatarEvent = 3320; //1737
    public const int SaveBrandingItemEvent = 3608; //3156
    public const int SaveEnforcedCategorySettingsEvent = 1265; //3413
    public const int RespectPetEvent = 3202; //1618
    public const int GetMarketplaceCanMakeOfferEvent = 848; //1647
    public const int UpdateMagicTileEvent = 3839; //1248
    public const int GetStickyNoteEvent = 3964; //2796
    public const int IgnoreUserEvent = 1117; //2394
    public const int BanUserEvent = 1477; //3940
    public const int UpdateForumSettingsEvent = 2214; //931
    public const int GetRoomRightsEvent = 3385; //2734
    public const int SendMsgEvent = 3567; //1981
    public const int CloseTicketEvent = 2067; //50
}