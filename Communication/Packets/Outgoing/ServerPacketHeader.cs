namespace Plus.Communication.Packets.Outgoing;

public static class ServerPacketHeader
{
    // Handshake
    public const uint InitCryptoComposer = 3531; //675
    public const uint SecretKeyComposer = 696; //3179
    public const uint AuthenticationOkComposer = 1079; //1442
    public const uint UserObjectComposer = 845; //1823
    public const uint UserPerksComposer = 1790; //2807
    public const uint KickbackInfoComposer = 4001; //3277
    public const uint UserRightsComposer = 3315; //1862
    public const uint GenericErrorComposer = 905; //169
    public const uint SetUniqueIdComposer = 3731; //2935
    public const uint AvailabilityStatusComposer = 3690; //2468

    // Avatar
    public const uint WardrobeComposer = 2959; //2760

    // Catalog
    public const uint CatalogIndexComposer = 2140; //2018
    public const uint CatalogItemDiscountComposer = 796; //3322
    public const uint PurchaseOkComposer = 1450; //2843
    public const uint CatalogOfferComposer = 1757; //3848
    public const uint CatalogPageComposer = 3277; //3477
    public const uint CatalogUpdatedComposer = 1411; //885
    public const uint SellablePetBreedsComposer = 2333; //1871
    public const uint GroupFurniConfigComposer = 3388; //418
    public const uint PresentDeliverErrorComposer = 1971; //934

    // Quests
    public const uint QuestListComposer = 3436; //664
    public const uint QuestCompletedComposer = 3715; //3692
    public const uint QuestAbortedComposer = 182; //3581
    public const uint QuestStartedComposer = 3281; //1477

    // Room Avatar
    public const uint ActionComposer = 3349; //179
    public const uint SleepComposer = 2306; //3852
    public const uint DanceComposer = 130; //845
    public const uint CarryObjectComposer = 2106; //2623
    public const uint AvatarEffectComposer = 2062; //2662

    // Room Chat
    public const uint ChatComposer = 2785; //3821
    public const uint ShoutComposer = 2888; //909
    public const uint WhisperComposer = 1400; //2280
    public const uint FloodControlComposer = 803; //1197
    public const uint UserTypingComposer = 1727; //2854

    // Room Engine
    public const uint UsersComposer = 3857; //2422
    public const uint FurnitureAliasesComposer = 2159; //81
    public const uint ObjectAddComposer = 2076; //505
    public const uint ObjectsComposer = 2783; //3521
    public const uint ObjectUpdateComposer = 1104; //273
    public const uint ObjectRemoveComposer = 2362; //85
    public const uint SlideObjectBundleComposer = 330; //11437
    public const uint ItemsComposer = 580; //2335
    public const uint ItemAddComposer = 2236; //1841
    public const uint ItemUpdateComposer = 3408; //2933
    public const uint ItemRemoveComposer = 209; //762

    // Room Session
    public const uint RoomForwardComposer = 3289; //1963
    public const uint RoomReadyComposer = 768; //2029
    public const uint OpenConnectionComposer = 3566; //1329
    public const uint CloseConnectionComposer = 726; //1898
    public const uint FlatAccessibleComposer = 735; //1179
    public const uint CantConnectComposer = 200; //1864

    // Room Permissions
    public const uint YouAreControllerComposer = 680; //1425
    public const uint YouAreNotControllerComposer = 1068; //1202
    public const uint YouAreOwnerComposer = 1932; //495

    // Room Settings
    public const uint RoomSettingsDataComposer = 3361; //633
    public const uint RoomSettingsSavedComposer = 3865; //3737
    public const uint FlatControllerRemovedComposer = 1501; //1205
    public const uint FlatControllerAddedComposer = 3493; //1056
    public const uint RoomRightsListComposer = 225; //2410

    // Room Furniture
    public const uint HideWiredConfigComposer = 2430; //3715
    public const uint WiredEffectConfigComposer = 1428; //1469
    public const uint WiredConditionConfigComposer = 1775; //1456
    public const uint WiredTriggeRconfigComposer = 21; //1618
    public const uint MoodlightConfigComposer = 1540; //1964
    public const uint GroupFurniSettingsComposer = 3755; //613
    public const uint OpenGiftComposer = 862; //1375

    // Navigator
    public const uint UpdateFavouriteRoomComposer = 3016; //854
    public const uint NavigatorLiftedRoomsComposer = 1568; //761
    public const uint NavigatorPreferencesComposer = 3617; //1430
    public const uint NavigatorFlatCatsComposer = 1265; //1109
    public const uint NavigatorMetaDataParserComposer = 1071; //371
    public const uint NavigatorCollapsedCategoriesComposer = 232; //1263

    // Messenger
    public const uint BuddyListComposer = 2900; //3394
    public const uint BuddyRequestsComposer = 177; //2757
    public const uint NewBuddyRequestComposer = 1525; //2981

    // Moderation
    public const uint ModeratorInitComposer = 2120; //2545
    public const uint ModeratorUserRoomVisitsComposer = 1282; //1101
    public const uint ModeratorRoomChatlogComposer = 3561; //1362
    public const uint ModeratorUserInfoComposer = 3234; //289
    public const uint ModeratorSupportTicketResponseComposer = 2651; //3927
    public const uint ModeratorUserChatlogComposer = 2812; //3308
    public const uint ModeratorRoomInfoComposer = 2318; //13
    public const uint ModeratorSupportTicketComposer = 1258; //1275
    public const uint ModeratorTicketChatlogComposer = 3637; //766
    public const uint CallForHelpPendingCallsComposer = 0; // TODO @80O: Seems to be duplicate of OpenHelpTool 2460;
    public const uint CfhTopicsInitComposer = 1094;

    // Inventory
    public const uint CreditBalanceComposer = 1958; //3604
    public const uint BadgesComposer = 2943; //154
    public const uint FurniListAddComposer = 2020; //176
    public const uint FurniListNotificationComposer = 439; //2725
    public const uint FurniListRemoveComposer = 3968; //1903
    public const uint FurniListComposer = 3640; //2183
    public const uint FurniListUpdateComposer = 1619; //506
    public const uint AvatarEffectsComposer = 1684; //3310
    public const uint AvatarEffectActivatedComposer = 545; //1710
    public const uint AvatarEffectExpiredComposer = 2673; //68
    public const uint AvatarEffectAddedComposer = 0; //2959; //2760 //TODO @80O: Seems to be incorrect
    public const uint TradingErrorComposer = 2484; //2876
    public const uint TradingAcceptComposer = 969; //1367
    public const uint TradingStartComposer = 2527; //2290
    public const uint TradingUpdateComposer = 2088; //2277
    public const uint TradingClosedComposer = 1436; //2068
    public const uint TradingCompleteComposer = 2288; //1959
    public const uint TradingConfirmedComposer = 0; // TODO @80O: Same as TradingAcceptComposer. Incorrect? 969; //1367
    public const uint TradingFinishComposer = 3443; //2369

    // Inventory Achievements
    public const uint AchievementsComposer = 1801; //509
    public const uint AchievementScoreComposer = 1115; //3710
    public const uint AchievementUnlockedComposer = 3385; //1887
    public const uint AchievementProgressedComposer = 2749; //305

    // Notifications
    public const uint ActivityPointsComposer = 3318; //1911
    public const uint HabboActivityPointNotificationComposer = 543; //606

    // Users
    public const uint ScrSendUserInfoComposer = 826; //2811
    public const uint IgnoredUsersComposer = 2157;

    // Groups
    public const uint UnknownGroupComposer = 1136; //1T981
    public const uint GroupMembershipRequestedComposer = 2472; //423
    public const uint ManageGroupComposer = 230; //2653
    public const uint HabboGroupBadgesComposer = 711; //2487
    public const uint NewGroupInfoComposer = 815; //1095
    public const uint GroupInfoComposer = 3712; //3160
    public const uint GroupCreationWindowComposer = 1062; //1232
    public const uint SetGroupIdComposer = 364; //3197
    public const uint GroupMembersComposer = 1401; //2297
    public const uint UpdateFavouriteGroupComposer = 2000; //3685
    public const uint GroupMemberUpdatedComposer = 3911; //2954
    public const uint RefreshFavouriteGroupComposer = 149; //382

    // Group Forums
    public const uint ForumsListDataComposer = 1539; //3596
    public const uint ForumDataComposer = 91; //254
    public const uint ThreadCreatedComposer = 2675; //3683
    public const uint ThreadDataComposer = 2526; //879
    public const uint ThreadsListDataComposer = 1056; //1538
    public const uint ThreadUpdatedComposer = 951; //3226
    public const uint ThreadReplyComposer = 1003; //1936

    // Sound
    public const uint SoundSettingsComposer = 1949; //2921

    public const uint QuestionParserComposer = 1163; //1719
    public const uint AvatarAspectUpdateComposer = 884;
    public const uint HelperToolComposer = 3610; //224
    public const uint RoomErrorNotifComposer = 2355; //444
    public const uint FollowFriendFailedComposer = 3469; //1170

    public const uint FindFriendsProcessResultComposer = 2921; //3763
    public const uint UserChangeComposer = 2248; //32
    public const uint FloorHeightMapComposer = 1819; //1112
    public const uint RoomInfoUpdatedComposer = 3743; //3833
    public const uint MessengerErrorComposer = 880; //915
    public const uint MarketplaceCanMakeOfferResultComposer = 2452; //1874
    public const uint GameAccountStatusComposer = 3750; //139
    public const uint GuestRoomSearchResultComposer = 1634; //43
    public const uint NewUserExperienceGiftOfferComposer = 2029; //1904
    public const uint UpdateUsernameComposer = 3461; //3801
    public const uint VoucherRedeemOkComposer = 2809; //3432
    public const uint FigureSetIdsComposer = 1811; //3469
    public const uint StickyNoteComposer = 344; //2338
    public const uint UserRemoveComposer = 3839; //2841
    public const uint GetGuestRoomResultComposer = 306; //2224
    public const uint DoorbellComposer = 2068; //162

    public const uint GiftWrappingConfigurationComposer = 766; //3348
    public const uint GetRelationshipsComposer = 112; //1589
    public const uint FriendNotificationComposer = 3024; //1211
    public const uint BadgeEditorPartsComposer = 2839; //2519
    public const uint TraxSongInfoComposer = 1159; //523
    public const uint PostUpdatedComposer = 1180; //1752
    public const uint UserUpdateComposer = 3559; //3153
    public const uint MutedComposer = 2246; //229
    public const uint MarketplaceConfigurationComposer = 1817; //3702
    public const uint CheckGnomeNameComposer = 3228; //2491
    public const uint OpenBotActionComposer = 464; //895
    public const uint FavouritesComposer = 3267; //604
    public const uint TalentLevelUpComposer = 3150; //3538

    public const uint BcBorrowedItemsComposer = 1043; //3424
    public const uint UserTagsComposer = 940; //774
    public const uint CampaignComposer = 2394; //3234
    public const uint RoomEventComposer = 1587; //2274
    public const uint MarketplaceItemStatsComposer = 0; // TODO @80O: Wrong ID listed same as MarketplaceMakeOfferResultComposer 480; //2909
    public const uint HabboSearchResultComposer = 2823; //214
    public const uint PetHorseFigureInformationComposer = 2926; //560
    public const uint PetInventoryComposer = 1988; //3528
    public const uint PongComposer = 1240; //624
    public const uint RentableSpaceComposer = 2323; //2660
    public const uint GetYouTubePlaylistComposer = 1354; //763
    public const uint RespectNotificationComposer = 1818; //474
    public const uint RecyclerRewardsComposer = 1604; //2457
    public const uint GetRoomBannedUsersComposer = 1810; //3580
    public const uint RoomRatingComposer = 2454; //3464
    public const uint PlayableGamesComposer = 3076; //549
    public const uint TalentTrackLevelComposer = 700; //2382
    public const uint JoinQueueComposer = 3139; //749
    public const uint MarketPlaceOwnOffersComposer = 1892; //2806
    public const uint PetBreedingComposer = 528; //616
    public const uint SubmitBullyReportComposer = 47; //453
    public const uint UserNameChangeComposer = 574; //2587
    public const uint LoveLockDialogueComposer = 1157; //173
    public const uint SendBullyReportComposer = 39; //2094
    public const uint VoucherRedeemErrorComposer = 2279; //3670
    public const uint PurchaseErrorComposer = 1331; //3016
    public const uint UnknownCalendarComposer = 128; //1799
    public const uint FriendListUpdateComposer = 1190; //1611

    public const uint UserFlatCatsComposer = 3379; //377
    public const uint UpdateFreezeLivesComposer = 2998; //1395
    public const uint UnbanUserFromRoomComposer = 3710; //3472
    public const uint PetTrainingPanelComposer = 546; //1067
    public const uint BuildersClubMembershipComposer = 820; //2357
    public const uint FlatAccessDeniedComposer = 797; //1582
    public const uint LatencyResponseComposer = 942; //3014
    public const uint HabboUserBadgesComposer = 3269; //1123
    public const uint HeightMapComposer = 1232; //207

    public const uint CanCreateRoomComposer = 3568; //1237
    public const uint InstantMessageErrorComposer = 945; //2964
    public const uint GnomeBoxComposer = 1694; //1778
    public const uint IgnoreStatusComposer = 2485; //3882
    public const uint PetInformationComposer = 3380; //3913
    public const uint NavigatorSearchResultSetComposer = 1089; //815
    public const uint ConcurrentUsersGoalProgressComposer = 3782; //2955
    public const uint VideoOffersRewardsComposer = 1806; //1896
    public const uint SanctionStatusComposer = 3525; //193
    public const uint GetYouTubeVideoComposer = 1022; //2374
    public const uint CheckPetNameComposer = 1760; //3019
    public const uint RespectPetNotificationComposer = 540; //3637
    public const uint EnforceCategoryUpdateComposer = 3714; //315
    public const uint CommunityGoalHallOfFameComposer = 2629; //690
    public const uint FloorPlanFloorMapComposer = 1855; //2337
    public const uint SendGameInvitationComposer = 2071; //1165
    public const uint GiftWrappingErrorComposer = 1385; //2534
    public const uint PromoArticlesComposer = 3015; //3565
    public const uint Game1WeeklyLeaderboardComposer = 57; //3124
    public const uint RentableSpacesErrorComposer = 1255; //838
    public const uint AddExperiencePointsComposer = 3791; //3779
    public const uint OpenHelpToolComposer = 2460; //3831
    public const uint GetRoomFilterListComposer = 1100; //2169
    public const uint GameAchievementListComposer = 2141; //1264
    public const uint PromotableRoomsComposer = 442; //2166
    public const uint FloorPlanSendDoorComposer = 1685; //2180
    public const uint RoomEntryInfoComposer = 3675; //3378
    public const uint RoomNotificationComposer = 3152; //2419
    public const uint ClubGiftsComposer = 2992; //1549
    public const uint MotdNotificationComposer = 1368; //1829
    public const uint PopularRoomTagsResultComposer = 1002; //234
    public const uint NewConsoleMessageComposer = 984; //2121
    public const uint RoomPropertyComposer = 1897; //1328
    public const uint MarketPlaceOffersComposer = 291; //2985
    public const uint TalentTrackComposer = 382; //3614
    public const uint ProfileInformationComposer = 3263; //3872
    public const uint BadgeDefinitionsComposer = 1827; //2066
    public const uint Game2WeeklyLeaderboardComposer = 275; //1127
    public const uint NameChangeUpdateComposer = 1226; //2698
    public const uint RoomVisualizationSettingsComposer = 3003; //3786
    public const uint MarketplaceMakeOfferResultComposer = 480; //3960
    public const uint FlatCreatedComposer = 3001; //1621
    public const uint BotInventoryComposer = 3692; //2620
    public const uint LoadGameComposer = 652; //1403
    public const uint UpdateMagicTileComposer = 2811; //2641
    public const uint MaintenanceStatusComposer = 3465; //3198
    public const uint Game3WeeklyLeaderboardComposer = 1326; //2194
    public const uint GameListComposer = 1220; //2481
    public const uint RoomMuteSettingsComposer = 1117; //257
    public const uint RoomInviteComposer = 2138; //3942
    public const uint LoveLockDialogueSetLockedComposer = 1767; //1534
    public const uint LoveLockDialogueCloseComposer = 0; //TODO @80O: Same header defined as LoveLockDialogueSetLockedComposer 1767; //1534
    public const uint BroadcastMessageAlertComposer = 1751; //1279
    public const uint MarketplaceCancelOfferResultComposer = 0; // TODO @80O: Same header defined as MarketPlaceOwnOffersComposer 1892; //202
    public const uint NavigatorSettingsComposer = 2477; //3175

    public const uint MessengerInitComposer = 1329; //391
    public const uint PollContentsComposer = 3826;
    public const uint PollOfferComposer = 1074;
}
