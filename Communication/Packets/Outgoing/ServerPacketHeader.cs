namespace Plus.Communication.Packets.Outgoing;

public static class ServerPacketHeader
{
    // NextLimitedComposer
    public const int NextLimitedComposer = 2910;

    // Handshake
    public const int InitCryptoMessageComposer = 3531; //675
    public const int SecretKeyMessageComposer = 696; //3179
    public const int AuthenticationOkMessageComposer = 1079; //1442
    public const int UserObjectMessageComposer = 845; //1823
    public const int UserPerksMessageComposer = 1790; //2807
    public const int UserRightsMessageComposer = 3315; //1862
    public const int GenericErrorMessageComposer = 905; //169
    public const int SetUniqueIdMessageComposer = 3731; //2935
    public const int AvailabilityStatusMessageComposer = 3690; //2468

    // Avatar
    public const int WardrobeMessageComposer = 2959; //2760

    // Catalog
    public const int CatalogIndexMessageComposer = 2140; //2018
    public const int CatalogItemDiscountMessageComposer = 796; //3322
    public const int PurchaseOkMessageComposer = 1450; //2843
    public const int CatalogOfferMessageComposer = 1757; //3848
    public const int CatalogPageMessageComposer = 3277; //3477
    public const int CatalogUpdatedMessageComposer = 1411; //885
    public const int SellablePetBreedsMessageComposer = 2333; //1871
    public const int GroupFurniConfigMessageComposer = 3388; //418
    public const int PresentDeliverErrorMessageComposer = 1971; //934

    // Quests
    public const int QuestListMessageComposer = 3436; //664
    public const int QuestCompletedMessageComposer = 3715; //3692
    public const int QuestAbortedMessageComposer = 182; //3581
    public const int QuestStartedMessageComposer = 3281; //1477

    // Room Avatar
    public const int ActionMessageComposer = 3349; //179
    public const int SleepMessageComposer = 2306; //3852
    public const int DanceMessageComposer = 130; //845
    public const int CarryObjectMessageComposer = 2106; //2623
    public const int AvatarEffectMessageComposer = 2062; //2662

    // Room Chat
    public const int ChatMessageComposer = 2785; //3821
    public const int ShoutMessageComposer = 2888; //909
    public const int WhisperMessageComposer = 1400; //2280
    public const int FloodControlMessageComposer = 803; //1197
    public const int UserTypingMessageComposer = 1727; //2854

    // Room Engine
    public const int UsersMessageComposer = 3857; //2422
    public const int FurnitureAliasesMessageComposer = 2159; //81
    public const int ObjectAddMessageComposer = 2076; //505
    public const int ObjectsMessageComposer = 2783; //3521
    public const int ObjectUpdateMessageComposer = 1104; //273
    public const int ObjectRemoveMessageComposer = 2362; //85
    public const int SlideObjectBundleMessageComposer = 330; //11437
    public const int ItemsMessageComposer = 580; //2335
    public const int ItemAddMessageComposer = 2236; //1841
    public const int ItemUpdateMessageComposer = 3408; //2933
    public const int ItemRemoveMessageComposer = 209; //762

    // Room Session
    public const int RoomForwardMessageComposer = 3289; //1963
    public const int RoomReadyMessageComposer = 768; //2029
    public const int OpenConnectionMessageComposer = 3566; //1329
    public const int CloseConnectionMessageComposer = 726; //1898
    public const int FlatAccessibleMessageComposer = 735; //1179
    public const int CantConnectMessageComposer = 200; //1864

    // Room Permissions
    public const int YouAreControllerMessageComposer = 680; //1425
    public const int YouAreNotControllerMessageComposer = 1068; //1202
    public const int YouAreOwnerMessageComposer = 1932; //495

    // Room Settings
    public const int RoomSettingsDataMessageComposer = 3361; //633
    public const int RoomSettingsSavedMessageComposer = 3865; //3737
    public const int FlatControllerRemovedMessageComposer = 1501; //1205
    public const int FlatControllerAddedMessageComposer = 3493; //1056
    public const int RoomRightsListMessageComposer = 225; //2410

    // Room Furniture
    public const int HideWiredConfigMessageComposer = 2430; //3715
    public const int WiredEffectConfigMessageComposer = 1428; //1469
    public const int WiredConditionConfigMessageComposer = 1775; //1456
    public const int WiredTriggeRconfigMessageComposer = 21; //1618
    public const int MoodlightConfigMessageComposer = 1540; //1964
    public const int GroupFurniSettingsMessageComposer = 3755; //613
    public const int OpenGiftMessageComposer = 862; //1375

    // Navigator
    public const int UpdateFavouriteRoomMessageComposer = 3016; //854
    public const int NavigatorLiftedRoomsMessageComposer = 1568; //761
    public const int NavigatorPreferencesMessageComposer = 3617; //1430
    public const int NavigatorFlatCatsMessageComposer = 1265; //1109
    public const int NavigatorMetaDataParserMessageComposer = 1071; //371
    public const int NavigatorCollapsedCategoriesMessageComposer = 232; //1263

    // Messenger
    public const int BuddyListMessageComposer = 2900; //3394
    public const int BuddyRequestsMessageComposer = 177; //2757
    public const int NewBuddyRequestMessageComposer = 1525; //2981

    // Moderation
    public const int ModeratorInitMessageComposer = 2120; //2545
    public const int ModeratorUserRoomVisitsMessageComposer = 1282; //1101
    public const int ModeratorRoomChatlogMessageComposer = 3561; //1362
    public const int ModeratorUserInfoMessageComposer = 3234; //289
    public const int ModeratorSupportTicketResponseMessageComposer = 2651; //3927
    public const int ModeratorUserChatlogMessageComposer = 2812; //3308
    public const int ModeratorRoomInfoMessageComposer = 2318; //13
    public const int ModeratorSupportTicketMessageComposer = 1258; //1275
    public const int ModeratorTicketChatlogMessageComposer = 3637; //766
    public const int CallForHelpPendingCallsMessageComposer = 2460;
    public const int CfhTopicsInitMessageComposer = 1094;

    // Inventory
    public const int CreditBalanceMessageComposer = 1958; //3604
    public const int BadgesMessageComposer = 2943; //154
    public const int FurniListAddMessageComposer = 2020; //176
    public const int FurniListNotificationMessageComposer = 439; //2725
    public const int FurniListRemoveMessageComposer = 3968; //1903
    public const int FurniListMessageComposer = 3640; //2183
    public const int FurniListUpdateMessageComposer = 1619; //506
    public const int AvatarEffectsMessageComposer = 1684; //3310
    public const int AvatarEffectActivatedMessageComposer = 545; //1710
    public const int AvatarEffectExpiredMessageComposer = 2673; //68
    public const int AvatarEffectAddedMessageComposer = 2959; //2760
    public const int TradingErrorMessageComposer = 2484; //2876
    public const int TradingAcceptMessageComposer = 969; //1367
    public const int TradingStartMessageComposer = 2527; //2290
    public const int TradingUpdateMessageComposer = 2088; //2277
    public const int TradingClosedMessageComposer = 1436; //2068
    public const int TradingCompleteMessageComposer = 2288; //1959
    public const int TradingConfirmedMessageComposer = 969; //1367
    public const int TradingFinishMessageComposer = 3443; //2369

    // Inventory Achievements
    public const int AchievementsMessageComposer = 1801; //509
    public const int AchievementScoreMessageComposer = 1115; //3710
    public const int AchievementUnlockedMessageComposer = 3385; //1887
    public const int AchievementProgressedMessageComposer = 2749; //305

    // Notifications
    public const int ActivityPointsMessageComposer = 3318; //1911
    public const int HabboActivityPointNotificationMessageComposer = 543; //606

    // Users
    public const int ScrSendUserInfoMessageComposer = 826; //2811
    public const int IgnoredUsersMessageComposer = 2157;

    // Groups
    public const int UnknownGroupMessageComposer = 1136; //1T981
    public const int GroupMembershipRequestedMessageComposer = 2472; //423
    public const int ManageGroupMessageComposer = 230; //2653
    public const int HabboGroupBadgesMessageComposer = 711; //2487
    public const int NewGroupInfoMessageComposer = 815; //1095
    public const int GroupInfoMessageComposer = 3712; //3160
    public const int GroupCreationWindowMessageComposer = 1062; //1232
    public const int SetGroupIdMessageComposer = 364; //3197
    public const int GroupMembersMessageComposer = 1401; //2297
    public const int UpdateFavouriteGroupMessageComposer = 2000; //3685
    public const int GroupMemberUpdatedMessageComposer = 3911; //2954
    public const int RefreshFavouriteGroupMessageComposer = 149; //382

    // Group Forums
    public const int ForumsListDataMessageComposer = 1539; //3596
    public const int ForumDataMessageComposer = 91; //254
    public const int ThreadCreatedMessageComposer = 2675; //3683
    public const int ThreadDataMessageComposer = 2526; //879
    public const int ThreadsListDataMessageComposer = 1056; //1538
    public const int ThreadUpdatedMessageComposer = 951; //3226
    public const int ThreadReplyMessageComposer = 1003; //1936

    // Sound
    public const int SoundSettingsMessageComposer = 1949; //2921

    public const int QuestionParserMessageComposer = 1163; //1719
    public const int AvatarAspectUpdateMessageComposer = 884;
    public const int HelperToolMessageComposer = 3610; //224
    public const int RoomErrorNotifMessageComposer = 2355; //444
    public const int FollowFriendFailedMessageComposer = 3469; //1170

    public const int FindFriendsProcessResultMessageComposer = 2921; //3763
    public const int UserChangeMessageComposer = 2248; //32
    public const int FloorHeightMapMessageComposer = 1819; //1112
    public const int RoomInfoUpdatedMessageComposer = 3743; //3833
    public const int MessengerErrorMessageComposer = 880; //915
    public const int MarketplaceCanMakeOfferResultMessageComposer = 2452; //1874
    public const int GameAccountStatusMessageComposer = 3750; //139
    public const int GuestRoomSearchResultMessageComposer = 1634; //43
    public const int NewUserExperienceGiftOfferMessageComposer = 2029; //1904
    public const int UpdateUsernameMessageComposer = 3461; //3801
    public const int VoucherRedeemOkMessageComposer = 2809; //3432
    public const int FigureSetIdsMessageComposer = 1811; //3469
    public const int StickyNoteMessageComposer = 344; //2338
    public const int UserRemoveMessageComposer = 3839; //2841
    public const int GetGuestRoomResultMessageComposer = 306; //2224
    public const int DoorbellMessageComposer = 2068; //162

    public const int GiftWrappingConfigurationMessageComposer = 766; //3348
    public const int GetRelationshipsMessageComposer = 112; //1589
    public const int FriendNotificationMessageComposer = 3024; //1211
    public const int BadgeEditorPartsMessageComposer = 2839; //2519
    public const int TraxSongInfoMessageComposer = 1159; //523
    public const int PostUpdatedMessageComposer = 1180; //1752
    public const int UserUpdateMessageComposer = 3559; //3153
    public const int MutedMessageComposer = 2246; //229
    public const int MarketplaceConfigurationMessageComposer = 1817; //3702
    public const int CheckGnomeNameMessageComposer = 3228; //2491
    public const int OpenBotActionMessageComposer = 464; //895
    public const int FavouritesMessageComposer = 3267; //604
    public const int TalentLevelUpMessageComposer = 3150; //3538

    public const int BcBorrowedItemsMessageComposer = 1043; //3424
    public const int UserTagsMessageComposer = 940; //774
    public const int CampaignMessageComposer = 2394; //3234
    public const int RoomEventMessageComposer = 1587; //2274
    public const int MarketplaceItemStatsMessageComposer = 480; //2909
    public const int HabboSearchResultMessageComposer = 2823; //214
    public const int PetHorseFigureInformationMessageComposer = 2926; //560
    public const int PetInventoryMessageComposer = 1988; //3528
    public const int PongMessageComposer = 1240; //624
    public const int RentableSpaceMessageComposer = 2323; //2660
    public const int GetYouTubePlaylistMessageComposer = 1354; //763
    public const int RespectNotificationMessageComposer = 1818; //474
    public const int RecyclerRewardsMessageComposer = 1604; //2457
    public const int GetRoomBannedUsersMessageComposer = 1810; //3580
    public const int RoomRatingMessageComposer = 2454; //3464
    public const int PlayableGamesMessageComposer = 3076; //549
    public const int TalentTrackLevelMessageComposer = 700; //2382
    public const int JoinQueueMessageComposer = 3139; //749
    public const int MarketPlaceOwnOffersMessageComposer = 1892; //2806
    public const int PetBreedingMessageComposer = 528; //616
    public const int SubmitBullyReportMessageComposer = 47; //453
    public const int UserNameChangeMessageComposer = 574; //2587
    public const int LoveLockDialogueMessageComposer = 1157; //173
    public const int SendBullyReportMessageComposer = 39; //2094
    public const int VoucherRedeemErrorMessageComposer = 2279; //3670
    public const int PurchaseErrorMessageComposer = 1331; //3016
    public const int UnknownCalendarMessageComposer = 128; //1799
    public const int FriendListUpdateMessageComposer = 1190; //1611

    public const int UserFlatCatsMessageComposer = 3379; //377
    public const int UpdateFreezeLivesMessageComposer = 2998; //1395
    public const int UnbanUserFromRoomMessageComposer = 3710; //3472
    public const int PetTrainingPanelMessageComposer = 546; //1067
    public const int LoveLockDialogueCloseMessageComposer = 1767; //1534
    public const int BuildersClubMembershipMessageComposer = 820; //2357
    public const int FlatAccessDeniedMessageComposer = 797; //1582
    public const int LatencyResponseMessageComposer = 942; //3014
    public const int HabboUserBadgesMessageComposer = 3269; //1123
    public const int HeightMapMessageComposer = 1232; //207

    public const int CanCreateRoomMessageComposer = 3568; //1237
    public const int InstantMessageErrorMessageComposer = 945; //2964
    public const int GnomeBoxMessageComposer = 1694; //1778
    public const int IgnoreStatusMessageComposer = 2485; //3882
    public const int PetInformationMessageComposer = 3380; //3913
    public const int NavigatorSearchResultSetMessageComposer = 1089; //815
    public const int ConcurrentUsersGoalProgressMessageComposer = 3782; //2955
    public const int VideoOffersRewardsMessageComposer = 1806; //1896
    public const int SanctionStatusMessageComposer = 3525; //193
    public const int GetYouTubeVideoMessageComposer = 1022; //2374
    public const int CheckPetNameMessageComposer = 1760; //3019
    public const int RespectPetNotificationMessageComposer = 540; //3637
    public const int EnforceCategoryUpdateMessageComposer = 3714; //315
    public const int CommunityGoalHallOfFameMessageComposer = 2629; //690
    public const int FloorPlanFloorMapMessageComposer = 1855; //2337
    public const int SendGameInvitationMessageComposer = 2071; //1165
    public const int GiftWrappingErrorMessageComposer = 1385; //2534
    public const int PromoArticlesMessageComposer = 3015; //3565
    public const int Game1WeeklyLeaderboardMessageComposer = 57; //3124
    public const int RentableSpacesErrorMessageComposer = 1255; //838
    public const int AddExperiencePointsMessageComposer = 3791; //3779
    public const int OpenHelpToolMessageComposer = 2460; //3831
    public const int GetRoomFilterListMessageComposer = 1100; //2169
    public const int GameAchievementListMessageComposer = 2141; //1264
    public const int PromotableRoomsMessageComposer = 442; //2166
    public const int FloorPlanSendDoorMessageComposer = 1685; //2180
    public const int RoomEntryInfoMessageComposer = 3675; //3378
    public const int RoomNotificationMessageComposer = 3152; //2419
    public const int ClubGiftsMessageComposer = 2992; //1549
    public const int MotdNotificationMessageComposer = 1368; //1829
    public const int PopularRoomTagsResultMessageComposer = 1002; //234
    public const int NewConsoleMessageMessageComposer = 984; //2121
    public const int RoomPropertyMessageComposer = 1897; //1328
    public const int MarketPlaceOffersMessageComposer = 291; //2985
    public const int TalentTrackMessageComposer = 382; //3614
    public const int ProfileInformationMessageComposer = 3263; //3872
    public const int BadgeDefinitionsMessageComposer = 1827; //2066
    public const int Game2WeeklyLeaderboardMessageComposer = 275; //1127
    public const int NameChangeUpdateMessageComposer = 1226; //2698
    public const int RoomVisualizationSettingsMessageComposer = 3003; //3786
    public const int MarketplaceMakeOfferResultMessageComposer = 480; //3960
    public const int FlatCreatedMessageComposer = 3001; //1621
    public const int BotInventoryMessageComposer = 3692; //2620
    public const int LoadGameMessageComposer = 652; //1403
    public const int UpdateMagicTileMessageComposer = 2811; //2641
    public const int MaintenanceStatusMessageComposer = 3465; //3198
    public const int Game3WeeklyLeaderboardMessageComposer = 1326; //2194
    public const int GameListMessageComposer = 1220; //2481
    public const int RoomMuteSettingsMessageComposer = 1117; //257
    public const int RoomInviteMessageComposer = 2138; //3942
    public const int LoveLockDialogueSetLockedMessageComposer = 1767; //1534
    public const int BroadcastMessageAlertMessageComposer = 1751; //1279
    public const int MarketplaceCancelOfferResultMessageComposer = 1892; //202
    public const int NavigatorSettingsMessageComposer = 2477; //3175

    public const int MessengerInitMessageComposer = 1329; //391
}