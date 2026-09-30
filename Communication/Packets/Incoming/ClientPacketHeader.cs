namespace Plus.Communication.Packets.Incoming;

public static class ClientPacketHeader
{
    // Handshake
    public const uint InitDiffieHandshakeEvent = 3392; //316
    public const uint GenerateSecretKeyEvent = 3622; //3847
    public const uint UniqueIdEvent = 3521; //1471
    public const uint SsoTicketEvent = 1989; //1778
    public const uint InfoRetrieveEvent = 2629; //186

    // Avatar
    public const uint CheckUserNameEvent = 2507; //8
    public const uint ChangeUserNameEvent = 2709; //1067
    public const uint GetWardrobeEvent = 3901; //765
    public const uint SaveWardrobeOutfitEvent = 1777; //55

    //Preferences
    public const uint SetChatStylePreferenceEvent = 1656;
    public const uint SetChatPreferenceEvent = 1045; //2006

    // Catalog
    public const uint GetCatalogIndexEvent = 3226; //1294
    public const uint GetCatalogPageEvent = 60; //39
    public const uint GetClubOffersEvent = 362; //2594
    public const uint GetClubGiftInfoEvent = 3127; //3302
    public const uint PurchaseFromCatalogEvent = 3492; //2830
    public const uint PurchaseFromCatalogAsGiftEvent = 1555; //21

    // Navigator

    // Messenger
    public const uint GetFriendRequestsEvent = 1646; //2485

    // Quests
    public const uint GetQuestListEvent = 2198; //2305
    public const uint StartQuestEvent = 2457; //1282
    public const uint GetCurrentQuestEvent = 651; //90
    public const uint CancelQuestEvent = 104; //3879

    // Room Avatar
    public const uint ActionEvent = 3268; //3639
    public const uint ApplySignEvent = 3555; //2966
    public const uint DanceEvent = 1225; //645
    public const uint SitEvent = 3735; //1565
    public const uint ChangeMottoEvent = 674; //3515
    public const uint LookToEvent = 1142; //3744
    public const uint DropHandItemEvent = 3296; //1751

    // Room Connection
    public const uint OpenFlatConnectionEvent = 189; //407
    public const uint GoToFlatEvent = 2947; //1601

    // Room Chat
    public const uint ChatEvent = 744; //670
    public const uint ShoutEvent = 697; //2101
    public const uint WhisperEvent = 3003; //878

    // Room Engine

    // Room Settings

    // Room Action

    // Users
    public const uint GetIgnoredUsersEvent = 198;

    // Moderation
    public const uint OpenHelpToolEvent = 1282; //1839
    public const uint CallForHelpPendingCallsDeletedEvent = 3643;
    public const uint ModeratorActionEvent = 760; //781
    public const uint ModerationMsgEvent = 2348; //2375
    public const uint ModerationMuteEvent = 2474; //1940
    public const uint ModerationTradeLockEvent = 3955; //1160
    public const uint GetModeratorUserRoomVisitsEvent = 3848; //730
    public const uint ModerationKickEvent = 1011; //3589
    public const uint GetModeratorRoomInfoEvent = 1997; //182
    public const uint GetModeratorUserInfoEvent = 2677; //2984
    public const uint GetModeratorRoomChatlogEvent = 3216; //2312
    public const uint ModerateRoomEvent = 500; //3458
    public const uint GetModeratorUserChatlogEvent = 63; //695
    public const uint GetModeratorTicketChatlogsEvent = 1449; //3484
    public const uint ModerationCautionEvent = 2223; //505
    public const uint ModerationBanEvent = 2473; //2595
    public const uint SubmitNewTicketEvent = 1046; //963
    public const uint CloseIssueDefaultActionEvent = 1921;

    // Inventory
    public const uint GetCreditsInfoEvent = 1051; //3697
    public const uint GetAchievementsEvent = 2249; //2931
    public const uint GetBadgesEvent = 2954; //166
    public const uint RequestFurniInventoryEvent = 2395; //352
    public const uint SetActivatedBadgesEvent = 2355; //2752
    public const uint AvatarEffectActivatedEvent = 2658; //129
    public const uint AvatarEffectSelectedEvent = 1816; //628

    public const uint InitTradeEvent = 3399; //3313
    public const uint TradingCancelConfirmEvent = 3738; //2264
    public const uint TradingModifyEvent = 644; //1153
    public const uint TradingOfferItemEvent = 842; //114
    public const uint TradingCancelEvent = 2934; //2967
    public const uint TradingConfirmEvent = 1394; //2399
    public const uint TradingOfferItemsEvent = 1607; //2996
    public const uint TradingRemoveItemEvent = 3313; //1033
    public const uint TradingAcceptEvent = 247; //3374

    // Register
    public const uint UpdateFigureDataEvent = 498; //2560

    // Groups
    public const uint GetBadgeEditorPartsEvent = 3706; //1670
    public const uint GetGroupCreationWindowEvent = 365; //468
    public const uint GetGroupFurniSettingsEvent = 1062; //41
    public const uint DeclineGroupMembershipEvent = 1571; //403
    public const uint JoinGroupEvent = 748; //2615
    public const uint UpdateGroupColoursEvent = 3469; //1443
    public const uint SetGroupFavouriteEvent = 77; //2625
    public const uint GetGroupMembersEvent = 3181; //205

    // Group Forums
    public const uint PostGroupContentEvent = 1499; //477
    public const uint GetForumStatsEvent = 1126; //872

    // Sound


    // Ambassador

    public const uint AmbassadorAlertEvent = 560;

    public const uint RemoveMyRightsEvent = 111; //879
    public const uint GiveHandItemEvent = 2523; //3315
    public const uint GoToHotelViewEvent = 1429; //3576
    public const uint GetRoomFilterListEvent = 179; //1348
    public const uint GetPromoArticlesEvent = 2782; //3895
    public const uint ModifyWhoCanRideHorseEvent = 3604; //1993
    public const uint RemoveFriendEvent = 1636; //698
    public const uint RefreshCampaignEvent = 3960; //3544
    public const uint AcceptFriendEvent = 2067; //45
    public const uint YouTubeVideoInformationEvent = 1295; //2395
    public const uint FollowFriendEvent = 848; //2280
    public const uint SaveBotActionEvent = 2921; //678g
    public const uint LetUserInEvent = 1781; //2356
    public const uint GetMarketplaceItemStatsEvent = 1561; //1203
    public const uint GetSellablePetPalettesEvent = 599; //2505
    public const uint ForceOpenCalendarBoxEvent = 1275; //2879
    public const uint SetUIFlagsEvent = 3841; //716
    public const uint DeleteRoomEvent = 439; //722
    public const uint SetSoundSettingsEvent = 608; //3820
    public const uint InitializeGameCenterEvent = 1825; //751
    public const uint RedeemOfferCreditsEvent = 2879; //1207
    public const uint FriendListUpdateEvent = 1166; //2664
    public const uint FriendFurniConfirmLockEvent = 3873; //2082
    public const uint UseHabboWheelEvent = 2148; //2651
    public const uint SaveRoomSettingsEvent = 3023; //2074
    public const uint ToggleMoodlightEvent = 14; //1826
    public const uint GetDailyQuestEvent = 3441; //484
    public const uint SetMannequinNameEvent = 3262; //2406
    public const uint OneWayGateEvent = 1970; //2816
    public const uint EventTrackerEvent = 143; //2386
    public const uint FloorPlanEditorRoomPropertiesEvent = 2478; //24
    public const uint PickUpPetEvent = 3975; //2342
    public const uint GetPetInventoryEvent = 3646; //263
    public const uint InitializeFloorPlanSessionEvent = 3069; //2623
    public const uint GetOwnOffersEvent = 360; //3829
    public const uint CheckPetNameEvent = 3733; //159
    public const uint SetUserFocusPreferenceEvent = 799; //526
    public const uint SubmitBullyReportEvent = 3971; //1803
    public const uint RemoveRightsEvent = 877; //40
    public const uint MakeOfferEvent = 2308; //255
    public const uint KickUserEvent = 1336; //3929
    public const uint GetRoomSettingsEvent = 581; //1014
    public const uint GetThreadsListDataEvent = 2568; //1606
    public const uint GetForumUserProfileEvent = 3515; //2639
    public const uint SaveWiredEffectConfigEvent = 2234; //3431
    public const uint GetRoomEntryDataEvent = 1747; //2768
    public const uint JoinQueueEvent = 167; //951
    public const uint CanCreateRoomEvent = 2411; //361
    public const uint SetTonerEvent = 1389; //1061
    public const uint SaveWiredTriggerConfigEvent = 3877; //1897
    public const uint PlaceBotEvent = 3770; //2321
    public const uint GetRelationshipsEvent = 3046; //866
    public const uint SetMessengerInviteStatusEvent = 1663; //1379
    public const uint UseFurnitureEvent = 3249; //3846
    public const uint GetUserFlatCatsEvent = 493; //3672
    public const uint AssignRightsEvent = 3843; //3574
    public const uint GetRoomBannedUsersEvent = 2009; //581
    public const uint ReleaseTicketEvent = 3931; //3800
    public const uint OpenPlayerProfileEvent = 3053; //3591
    public const uint GetSanctionStatusEvent = 3209; //2883
    public const uint CreditFurniRedeemEvent = 3945; //1676
    public const uint DisconnectEvent = 1474; //2391
    public const uint PickupObjectEvent = 1766; //636
    public const uint FindRandomFriendingRoomEvent = 2189; //1874
    public const uint UseSellableClothingEvent = 2849; //818
    public const uint MoveObjectEvent = 3583; //1781
    public const uint GetFurnitureAliasesEvent = 3116; //2125
    public const uint TakeAdminRightsEvent = 1661; //2725
    public const uint ModifyRoomFilterListEvent = 87; //256
    public const uint MoodlightUpdateEvent = 2913; //856
    public const uint GetPetTrainingPanelEvent = 3915; //2088
    public const uint GetSongInfoEvent = 3916; //3418
    public const uint UseWallItemEvent = 3674; //3396
    public const uint GetTalentTrackEvent = 680; //1284
    public const uint GiveAdminRightsEvent = 404; //465
    public const uint GetCatalogModeEvent = 951; //2267
    public const uint SendBullyReportEvent = 3540; //2973
    public const uint CancelOfferEvent = 195; //1862
    public const uint SaveWiredConditionConfigEvent = 2370; //488
    public const uint RedeemVoucherEvent = 1384; //489
    public const uint ThrowDiceEvent = 3427; //1182
    public const uint CraftSecretEvent = 3623; //1622
    public const uint GetGameListEvent = 705; //2993
    public const uint SetRelationshipEvent = 1514; //2112
    public const uint RequestFriendEvent = 1706; //3775
    public const uint MemoryPerformanceEvent = 124; //731
    public const uint ToggleYouTubeVideoEvent = 1956; //890
    public const uint SetMannequinFigureEvent = 1909; //3936
    public const uint GetEventCategoriesEvent = 597; //1086
    public const uint DeleteGroupThreadEvent = 50; //3299
    public const uint PurchaseGroupEvent = 2959; //2546
    public const uint MessengerInitEvent = 2825; //2151
    public const uint CancelTypingEvent = 1329; //1114
    public const uint GetMoodlightConfigEvent = 2906; //3472
    public const uint GetGroupInfoEvent = 681; //3211
    public const uint CreateFlatEvent = 92; //3077
    public const uint LatencyTestEvent = 878; //1789
    public const uint GetSelectedBadgesEvent = 2735; //2226
    public const uint AddStickyNoteEvent = 3891; //425
    public const uint RideHorseEvent = 3387; //1440
    public const uint InitializeNewNavigatorEvent = 3375; //882
    public const uint GetForumsListDataEvent = 3802; //3912
    public const uint ToggleMuteToolEvent = 1301; //2462
    public const uint UpdateGroupIdentityEvent = 1375; //1062
    public const uint UpdateStickyNoteEvent = 3120; //342
    public const uint UnbanUserFromRoomEvent = 2050; //3060
    public const uint UnIgnoreUserEvent = 981; //3023
    public const uint OpenGiftEvent = 349; //1515
    public const uint ApplyDecorationEvent = 2729; //728
    public const uint GetRecipeConfigEvent = 2428; //3654
    public const uint ScrGetUserInfoEvent = 2749; //12
    public const uint RemoveGroupMemberEvent = 1590; //649
    public const uint DiceOffEvent = 1124; //191
    public const uint YouTubeGetNextVideo = 2618; //1843
    public const uint RemoveFavouriteRoomEvent = 3223; //855
    public const uint RespectUserEvent = 3812; //1955
    public const uint AddFavouriteRoomEvent = 3251; //3092
    public const uint DeclineFriendEvent = 3484; //835
    public const uint StartTypingEvent = 2826; //3362
    public const uint GetGroupFurniConfigEvent = 3902; //3046
    public const uint SendRoomInviteEvent = 1806; //2694
    public const uint RemoveAllRightsEvent = 884; //1404
    public const uint GetYouTubeTelevisionEvent = 1326; //3517
    public const uint FindNewFriendsEvent = 3889; //1264
    public const uint GetPromotableRoomsEvent = 2306; //276
    public const uint GetBotInventoryEvent = 775; //363
    public const uint GetRentableSpaceEvent = 2035; //793
    public const uint OpenBotActionEvent = 3236; //2544
    public const uint OpenCalendarBoxEvent = 1229; //724
    public const uint DeleteGroupPostEvent = 1991; //317
    public const uint UpdateGroupBadgeEvent = 1589; //2959
    public const uint PlaceObjectEvent = 1809; //579
    public const uint RemoveGroupFavouriteEvent = 226; //1412
    public const uint UpdateNavigatorSettingsEvent = 1824; //2501
    public const uint CheckGnomeNameEvent = 1179; //2281
    public const uint NavigatorSearchEvent = 618; //2722
    public const uint GetPetInformationEvent = 2986; //2853
    public const uint GetGuestRoomEvent = 2247; //1164
    public const uint UpdateThreadEvent = 2980; //1522
    public const uint AcceptGroupMembershipEvent = 2996; //2259
    public const uint GetMarketplaceConfigurationEvent = 2811; //1604
    public const uint Game2GetWeeklyLeaderboardEvent = 285; //2106
    public const uint BuyOfferEvent = 904; //3699
    public const uint RemoveSaddleFromHorseEvent = 844; //1892
    public const uint GiveRoomScoreEvent = 3261; //336
    public const uint GetHabboClubWindowEvent = 3530; //715
    public const uint DeleteStickyNoteEvent = 3885; //2777
    public const uint MuteUserEvent = 2101; //2997
    public const uint ApplyHorseEffectEvent = 3364; //870
    public const uint ClientHelloEvent = 4000; //4000
    public const uint OnBullyClickEvent = 254; //1932
    public const uint HabboSearchEvent = 1194; //3375
    public const uint PickTicketEvent = 1807; //3973
    public const uint GetGiftWrappingConfigurationEvent = 1570; //1928
    public const uint GetCraftingRecipesAvailableEvent = 1869; //1653
    public const uint GetThreadDataEvent = 2324; //1559
    public const uint ManageGroupEvent = 737; //2547
    public const uint PlacePetEvent = 1495; //223
    public const uint EditRoomPromotionEvent = 816; //3707
    public const uint SaveFloorPlanModelEvent = 1936; //1287
    public const uint MoveWallItemEvent = 1778; //609
    public const uint VersionCheckEvent = 1220; //1600
    public const uint PongEvent = 509; //2584
    public const uint DeleteGroupEvent = 114; //747
    public const uint UpdateGroupSettingsEvent = 2435; //3180
    public const uint GetRecyclerPrizesEvent = 2152; //3258
    public const uint PurchaseRoomAdEvent = 1542; //3078
    public const uint PickUpBotEvent = 3058; //644
    public const uint GetOffersEvent = 2817; //442
    public const uint GetHabboGroupBadgesEvent = 3925; //301
    public const uint GetUserTagsEvent = 84; //1722
    public const uint GetGameAchievementsEvent = 1418; //482
    public const uint GetCatalogRoomPromotionEvent = 2757; //538
    public const uint MoveAvatarEvent = 2121; //1737
    public const uint SaveBrandingItemEvent = 2208; //3156
    public const uint SaveEnforcedCategorySettingsEvent = 531; //3413
    public const uint RespectPetEvent = 1967; //1618
    public const uint GetMarketplaceCanMakeOfferEvent = 1552; //1647
    public const uint UpdateMagicTileEvent = 2997; //1248
    public const uint GetStickyNoteEvent = 2469; //2796
    public const uint IgnoreUserEvent = 2374; //2394
    public const uint BanUserEvent = 3009; //3940
    public const uint UpdateForumSettingsEvent = 3295; //931
    public const uint GetRoomRightsEvent = 3937; //2734
    public const uint SendMsgEvent = 2409; //1981
    public const uint CloseTicketEvent = 1080; //50

    //NotImplemented

    //Camera
    //public const uint InitCameraEvent =;
    //public const uint PhotoCompetitionEvent =;
    //public const uint PublishPhotoEvent =;
    //public const uint PurchasePhotoEvent =;
    //public const uint RenderRoomEvent =;
    //public const uint RenderRoomThumbnailEvent =;

    //campaign
    //public const uint OpenCampaignCalendarDoorAsStaffEvent =;
    //public const uint OpenCampaignCalendarDoorEvent =;

    //catalog
    //public const uint GetBonusRareInfoEvent =;
    //public const uint BuildersClubQueryFurniCountEvent =;
    //public const uint BuildersClubPlaceWallItemEvent =;
    //public const uint BuildersClubPlaceRoomItemEvent =;
    //public const uint GetBundleDiscountRulesetEvent =;
    //public const uint GetCatalogPageExpirationEvent =;
    //public const uint GetCatalogPageWithEarliestExpiryEvent =;
    //public const uint GetDirectClubBuyAvailableEvent =;
    //public const uint GetHabboBasicMembershipExtendOfferEvent =;
    //public const uint GetLimitedOfferAppearingNextEvent =;
    //public const uint GetHabboClubExtendOfferEvent =;
    //public const uint GetIsOfferGiftableEvent =;
    //public const uint GetNextTargetedOfferEvent =;
    //public const uint GetSeasonalCalendarDailyOfferEvent =;
    //public const uint MarkCatalogNewAdditionsPageOpenedEvent =;
    //public const uint PurchaseBasicMembershipExtensionEvent =;
    //public const uint PurchaseTargetedOfferEvent =;
    //public const uint PurchaseVipMembershipExtensionEvent =;
    //public const uint SelectClubGiftEvent =;
    //public const uint SetTargetedOfferStateEvent =;
    //public const uint ShopTargetedOfferViewedEvent =;


    //recycler
    //public const uint GetRecyclerStatusEvent =;
    //public const uint RecyclerRecycleEvent =;


    //FriendList
    //public const uint FriendFurniConfirmLockEvent =;
    //public const uint SetRelationshipStatusEvent =;
    //public const uint VisitUserEvent =;

    //competition
    //public const uint ForwardToACompetitionRoomEvent =;
    //public const uint ForwardToASubmittableRoomEvent =;
    //public const uint ForwardToRandomCompetitionRoomEvent =;
    //public const uint GetCurrentTimingCodeEvent =;
    //public const uint GetIsUserPartOfCompetitionEvent =;
    //public const uint GetSecondsUntilEvent =;
    //public const uint RoomCompetitionInitEvent =;
    //public const uint SubmitRoomToCompetitionEvent =;
    //public const uint VoteForRoomEvent =;

    //crafting
    //public const uint CraftEvent =;
    //public const uint CraftSecretEvent =;
    //public const uint GetCraftableProductsEvent =;
    //public const uint GetCraftingRecipeEvent =;
    //public const uint GetCraftingRecipesAvailableEvent =;

    //game
    //public const uint Game2ExitGameEvent =;
    //public const uint Game2GameChatEvent =;
    //public const uint Game2LoadStageReadyEvent =;
    //public const uint Game2PlayAgainEvent =;
    //public const uint Game2CheckGameDirectoryStatusEvent =;
    //public const uint Game2GetAccountGameStatusEvent =;
    //public const uint Game2RequestFullStatusUpdateEvent =;




}
