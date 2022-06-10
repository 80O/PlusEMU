namespace Plus.Communication.Packets.Incoming;

public static class ClientPacketHeader
{
    // Handshake
    public const int InitCryptoEvent = 3392; //316
    public const int GenerateSecretKeyEvent = 3622; //3847
    public const int UniqueIdEvent = 3521; //1471
    public const int SsoTicketEvent = 1989; //1778
    public const int InfoRetrieveEvent = 2629; //186

    // Avatar
    public const int GetWardrobeEvent = 3901; //765
    public const int SaveWardrobeOutfitEvent = 1777; //55
    public const int SetChatStylePreferenceEvent = 1656;

    // Catalog
    public const int GetCatalogIndexEvent = 3226; //1294
    public const int GetCatalogPageEvent = 60; //39
    public const int PurchaseFromCatalogEvent = 3492; //2830
    public const int PurchaseFromCatalogAsGiftEvent = 1555; //21

    // Navigator

    // Messenger
    public const int GetBuddyRequestsEvent = 1646; //2485

    // Quests
    public const int GetQuestListEvent = 2198; //2305
    public const int StartQuestEvent = 2457; //1282
    public const int GetCurrentQuestEvent = 651; //90
    public const int CancelQuestEvent = 104; //3879

    // Room Avatar
    public const int ActionEvent = 3268; //3639
    public const int ApplySignEvent = 3555; //2966
    public const int DanceEvent = 1225; //645
    public const int SitEvent = 3735; //1565
    public const int ChangeMottoEvent = 674; //3515
    public const int LookToEvent = 1142; //3744
    public const int DropHandItemEvent = 3296; //1751

    // Room Connection
    public const int OpenFlatConnectionEvent = 189; //407
    public const int GoToFlatEvent = 2947; //1601

    // Room Chat
    public const int ChatEvent = 744; //670
    public const int ShoutEvent = 697; //2101
    public const int WhisperEvent = 3003; //878

    // Room Engine

    // Room Furniture

    public const int OneWayGateEvent = 1970;

    // Room Settings

    // Room Action

    // Users
    public const int GetIgnoredUsersEvent = 198;

    // Moderation
    public const int OpenHelpToolEvent = 1282; //1839
    public const int CallForHelpPendingCallsDeletedEvent = 3643;
    public const int ModeratorActionEvent = 760; //781
    public const int ModerationMsgEvent = 2348; //2375
    public const int ModerationMuteEvent = 2474; //1940
    public const int ModerationTradeLockEvent = 3955; //1160
    public const int GetModeratorUserRoomVisitsEvent = 3848; //730
    public const int ModerationKickEvent = 1011; //3589
    public const int GetModeratorRoomInfoEvent = 1997; //182
    public const int GetModeratorUserInfoEvent = 2677; //2984
    public const int GetModeratorRoomChatlogEvent = 3216; //2312
    public const int ModerateRoomEvent = 500; //3458
    public const int GetModeratorUserChatlogEvent = 63; //695
    public const int GetModeratorTicketChatlogsEvent = 1449; //3484
    public const int ModerationCautionEvent = 2223; //505
    public const int ModerationBanEvent = 2473; //2595
    public const int SubmitNewTicketEvent = 1046; //963
    public const int CloseIssueDefaultActionEvent = 1921;

    // Inventory
    public const int GetCreditsInfoEvent = 1051; //3697
    public const int GetAchievementsEvent = 2249; //2931
    public const int GetBadgesEvent = 2954; //166
    public const int RequestFurniInventoryEvent = 2395; //352
    public const int SetActivatedBadgesEvent = 2355; //2752
    public const int AvatarEffectActivatedEvent = 2658; //129
    public const int AvatarEffectSelectedEvent = 1816; //628

    public const int InitTradeEvent = 3399; //3313
    public const int TradingCancelConfirmEvent = 3738; //2264
    public const int TradingModifyEvent = 644; //1153
    public const int TradingOfferItemEvent = 842; //114
    public const int TradingCancelEvent = 2934; //2967
    public const int TradingConfirmEvent = 1394; //2399
    public const int TradingOfferItemsEvent = 1607; //2996
    public const int TradingRemoveItemEvent = 3313; //1033
    public const int TradingAcceptEvent = 247; //3374

    // Register
    public const int UpdateFigureDataEvent = 498; //2560

    // Groups
    public const int GetBadgeEditorPartsEvent = 3706; //1670
    public const int GetGroupCreationWindowEvent = 365; //468
    public const int GetGroupFurniSettingsEvent = 1062; //41
    public const int DeclineGroupMembershipEvent = 1571; //403
    public const int JoinGroupEvent = 748; //2615
    public const int UpdateGroupColoursEvent = 3469; //1443
    public const int SetGroupFavouriteEvent = 77; //2625
    public const int GetGroupMembersEvent = 3181; //205

    // Group Forums
    public const int PostGroupContentEvent = 1499; //477
    public const int GetForumStatsEvent = 1126; //872

    // Sound


    // Ambassador

    public const int AmbassadorSendAlertEvent = 560;

    public const int RemoveMyRightsEvent = 111; //879
    public const int GiveHandItemEvent = 2523; //3315
    public const int GetClubGiftsEvent = 3127; //3302
    public const int GoToHotelViewEvent = 1429; //3576
    public const int GetRoomFilterListEvent = 179; //1348
    public const int GetPromoArticlesEvent = 2782; //3895
    public const int ModifyWhoCanRideHorseEvent = 3604; //1993
    public const int RemoveBuddyEvent = 1636; //698
    public const int RefreshCampaignEvent = 3960; //3544
    public const int AcceptBuddyEvent = 2067; //45
    public const int YouTubeVideoInformationEvent = 1295; //2395
    public const int FollowFriendEvent = 848; //2280
    public const int SaveBotActionEvent = 2921; //678g
    public const int LetUserInEvent = 1781; //2356
    public const int GetMarketplaceItemStatsEvent = 1561; //1203
    public const int GetSellablePetBreedsEvent = 599; //2505
    public const int ForceOpenCalendarBoxEvent = 1275; //2879
    public const int SetFriendBarStateEvent = 3841; //716
    public const int DeleteRoomEvent = 439; //722
    public const int SetSoundSettingsEvent = 608; //3820
    public const int InitializeGameCenterEvent = 1825; //751
    public const int RedeemOfferCreditsEvent = 2879; //1207
    public const int FriendListUpdateEvent = 1166; //2664
    public const int ConfirmLoveLockEvent = 3873; //2082
    public const int UseHabboWheelEvent = 2148; //2651
    public const int SaveRoomSettingsEvent = 3023; //2074
    public const int ToggleMoodlightEvent = 14; //1826
    public const int GetDailyQuestEvent = 3441; //484
    public const int SetMannequinNameEvent = 3262; //2406
    public const int UseOneWayGateEvent = 1970; //2816
    public const int EventTrackerEvent = 143; //2386
    public const int FloorPlanEditorRoomPropertiesEvent = 2478; //24
    public const int PickUpPetEvent = 3975; //2342
    public const int GetPetInventoryEvent = 3646; //263
    public const int InitializeFloorPlanSessionEvent = 3069; //2623
    public const int GetOwnOffersEvent = 360; //3829
    public const int CheckPetNameEvent = 3733; //159
    public const int SetUserFocusPreferenceEvent = 799; //526
    public const int SubmitBullyReportEvent = 3971; //1803
    public const int RemoveRightsEvent = 877; //40
    public const int MakeOfferEvent = 2308; //255
    public const int KickUserEvent = 1336; //3929
    public const int GetRoomSettingsEvent = 581; //1014
    public const int GetThreadsListDataEvent = 2568; //1606
    public const int GetForumUserProfileEvent = 3515; //2639
    public const int SaveWiredEffectConfigEvent = 2234; //3431
    public const int GetRoomEntryDataEvent = 1747; //2768
    public const int JoinPlayerQueueEvent = 167; //951
    public const int CanCreateRoomEvent = 2411; //361
    public const int SetTonerEvent = 1389; //1061
    public const int SaveWiredTriggerConfigEvent = 3877; //1897
    public const int PlaceBotEvent = 3770; //2321
    public const int GetRelationshipsEvent = 3046; //866
    public const int SetMessengerInviteStatusEvent = 1663; //1379
    public const int UseFurnitureEvent = 3249; //3846
    public const int GetUserFlatCatsEvent = 493; //3672
    public const int AssignRightsEvent = 3843; //3574
    public const int GetRoomBannedUsersEvent = 2009; //581
    public const int ReleaseTicketEvent = 3931; //3800
    public const int OpenPlayerProfileEvent = 3053; //3591
    public const int GetSanctionStatusEvent = 3209; //2883
    public const int CreditFurniRedeemEvent = 3945; //1676
    public const int DisconnectEvent = 1474; //2391
    public const int PickupObjectEvent = 1766; //636
    public const int FindRandomFriendingRoomEvent = 2189; //1874
    public const int UseSellableClothingEvent = 2849; //818
    public const int MoveObjectEvent = 3583; //1781
    public const int GetFurnitureAliasesEvent = 3116; //2125
    public const int TakeAdminRightsEvent = 1661; //2725
    public const int ModifyRoomFilterListEvent = 87; //256
    public const int MoodlightUpdateEvent = 2913; //856
    public const int GetPetTrainingPanelEvent = 3915; //2088
    public const int GetSongInfoEvent = 3916; //3418
    public const int UseWallItemEvent = 3674; //3396
    public const int GetTalentTrackEvent = 680; //1284
    public const int GiveAdminRightsEvent = 404; //465
    public const int GetCatalogModeEvent = 951; //2267
    public const int SendBullyReportEvent = 3540; //2973
    public const int CancelOfferEvent = 195; //1862
    public const int SaveWiredConditionConfigEvent = 2370; //488
    public const int RedeemVoucherEvent = 1384; //489
    public const int ThrowDiceEvent = 3427; //1182
    public const int CraftSecretEvent = 3623; //1622
    public const int GetGameListingEvent = 705; //2993
    public const int SetRelationshipEvent = 1514; //2112
    public const int RequestBuddyEvent = 1706; //3775
    public const int MemoryPerformanceEvent = 124; //731
    public const int ToggleYouTubeVideoEvent = 1956; //890
    public const int SetMannequinFigureEvent = 1909; //3936
    public const int GetEventCategoriesEvent = 597; //1086
    public const int DeleteGroupThreadEvent = 50; //3299
    public const int PurchaseGroupEvent = 2959; //2546
    public const int MessengerInitEvent = 2825; //2151
    public const int CancelTypingEvent = 1329; //1114
    public const int GetMoodlightConfigEvent = 2906; //3472
    public const int GetGroupInfoEvent = 681; //3211
    public const int CreateFlatEvent = 92; //3077
    public const int LatencyTestEvent = 878; //1789
    public const int GetSelectedBadgesEvent = 2735; //2226
    public const int AddStickyNoteEvent = 3891; //425
    public const int ChangeNameEvent = 2709; //1067
    public const int RideHorseEvent = 3387; //1440
    public const int InitializeNewNavigatorEvent = 3375; //882
    public const int SetChatPreferenceEvent = 1045; //2006
    public const int GetForumsListDataEvent = 3802; //3912
    public const int ToggleMuteToolEvent = 1301; //2462
    public const int UpdateGroupIdentityEvent = 1375; //1062
    public const int UpdateStickyNoteEvent = 3120; //342
    public const int UnbanUserFromRoomEvent = 2050; //3060
    public const int UnIgnoreUserEvent = 981; //3023
    public const int OpenGiftEvent = 349; //1515
    public const int ApplyDecorationEvent = 2729; //728
    public const int GetRecipeConfigEvent = 2428; //3654
    public const int ScrGetUserInfoEvent = 2749; //12
    public const int RemoveGroupMemberEvent = 1590; //649
    public const int DiceOffEvent = 1124; //191
    public const int YouTubeGetNextVideo = 2618; //1843
    public const int RemoveFavouriteRoomEvent = 3223; //855
    public const int RespectUserEvent = 3812; //1955
    public const int AddFavouriteRoomEvent = 3251; //3092
    public const int DeclineBuddyEvent = 3484; //835
    public const int StartTypingEvent = 2826; //3362
    public const int GetGroupFurniConfigEvent = 3902; //3046
    public const int SendRoomInviteEvent = 1806; //2694
    public const int RemoveAllRightsEvent = 884; //1404
    public const int GetYouTubeTelevisionEvent = 1326; //3517
    public const int FindNewFriendsEvent = 3889; //1264
    public const int GetPromotableRoomsEvent = 2306; //276
    public const int GetBotInventoryEvent = 775; //363
    public const int GetRentableSpaceEvent = 2035; //793
    public const int OpenBotActionEvent = 3236; //2544
    public const int OpenCalendarBoxEvent = 1229; //724
    public const int DeleteGroupPostEvent = 1991; //317
    public const int CheckValidNameEvent = 2507; //8
    public const int UpdateGroupBadgeEvent = 1589; //2959
    public const int PlaceObjectEvent = 1809; //579
    public const int RemoveGroupFavouriteEvent = 226; //1412
    public const int UpdateNavigatorSettingsEvent = 1824; //2501
    public const int CheckGnomeNameEvent = 1179; //2281
    public const int NavigatorSearchEvent = 618; //2722
    public const int GetPetInformationEvent = 2986; //2853
    public const int GetGuestRoomEvent = 2247; //1164
    public const int UpdateThreadEvent = 2980; //1522
    public const int AcceptGroupMembershipEvent = 2996; //2259
    public const int GetMarketplaceConfigurationEvent = 2811; //1604
    public const int Game2GetWeeklyLeaderboardEvent = 285; //2106
    public const int BuyOfferEvent = 904; //3699
    public const int RemoveSaddleFromHorseEvent = 844; //1892
    public const int GiveRoomScoreEvent = 3261; //336
    public const int GetHabboClubWindowEvent = 3530; //715
    public const int DeleteStickyNoteEvent = 3885; //2777
    public const int MuteUserEvent = 2101; //2997
    public const int ApplyHorseEffectEvent = 3364; //870
    public const int GetClientVersionEvent = 4000; //4000
    public const int OnBullyClickEvent = 254; //1932
    public const int HabboSearchEvent = 1194; //3375
    public const int PickTicketEvent = 1807; //3973
    public const int GetGiftWrappingConfigurationEvent = 1570; //1928
    public const int GetCraftingRecipesAvailableEvent = 1869; //1653
    public const int GetThreadDataEvent = 2324; //1559
    public const int ManageGroupEvent = 737; //2547
    public const int PlacePetEvent = 1495; //223
    public const int EditRoomPromotionEvent = 816; //3707
    public const int GetCatalogOfferEvent = 362; //2180
    public const int SaveFloorPlanModelEvent = 1936; //1287
    public const int MoveWallItemEvent = 1778; //609
    public const int ClientVariablesEvent = 1220; //1600
    public const int PingEvent = 509; //2584
    public const int DeleteGroupEvent = 114; //747
    public const int UpdateGroupSettingsEvent = 2435; //3180
    public const int GetRecyclerRewardsEvent = 2152; //3258
    public const int PurchaseRoomPromotionEvent = 1542; //3078
    public const int PickUpBotEvent = 3058; //644
    public const int GetOffersEvent = 2817; //442
    public const int GetHabboGroupBadgesEvent = 3925; //301
    public const int GetUserTagsEvent = 84; //1722
    public const int GetPlayableGamesEvent = 1418; //482
    public const int GetCatalogRoomPromotionEvent = 2757; //538
    public const int MoveAvatarEvent = 2121; //1737
    public const int SaveBrandingItemEvent = 2208; //3156
    public const int SaveEnforcedCategorySettingsEvent = 531; //3413
    public const int RespectPetEvent = 1967; //1618
    public const int GetMarketplaceCanMakeOfferEvent = 1552; //1647
    public const int UpdateMagicTileEvent = 2997; //1248
    public const int GetStickyNoteEvent = 2469; //2796
    public const int IgnoreUserEvent = 2374; //2394
    public const int BanUserEvent = 3009; //3940
    public const int UpdateForumSettingsEvent = 3295; //931
    public const int GetRoomRightsEvent = 3937; //2734
    public const int SendMsgEvent = 2409; //1981
    public const int CloseTicketEvent = 1080; //50
}