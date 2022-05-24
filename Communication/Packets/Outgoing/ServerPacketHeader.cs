namespace Plus.Communication.Packets.Outgoing;

public static class ServerPacketHeader
{
    // Handshake
    public const int InitCryptoMessageComposer = 1347; //675
    public const int SecretKeyMessageComposer = 3885; //3179
    public const int AuthenticationOkMessageComposer = 2491; //1442
    public const int UserObjectMessageComposer = 2725; //1823
    public const int UserPerksMessageComposer = 2586; //2807
    public const int UserRightsMessageComposer = 411; //1862
    public const int GenericErrorMessageComposer = 1600; //169
    public const int SetUniqueIdMessageComposer = 1488; //2935
    public const int AvailabilityStatusMessageComposer = 2033; //2468

    // Avatar
    public const int WardrobeMessageComposer = 3315; //2760

    // Catalog
    public const int CatalogIndexMessageComposer = 2347; //2018
    public const int CatalogItemDiscountMessageComposer = 2347; //3322
    public const int PurchaseOkMessageComposer = 869; //2843
    public const int CatalogOfferMessageComposer = 3388; //3848
    public const int CatalogPageMessageComposer = 804; //3477
    public const int CatalogUpdatedMessageComposer = 1866; //885
    public const int SellablePetBreedsMessageComposer = 3331; //1871
    public const int GroupFurniConfigMessageComposer = 420; //418
    public const int PresentDeliverErrorMessageComposer = 3914; //934

    // Quests
    public const int QuestListMessageComposer = 3625; //664
    public const int QuestCompletedMessageComposer = 949; //3692
    public const int QuestAbortedMessageComposer = 3027; //3581
    public const int QuestStartedMessageComposer = 230; //1477

    // Room Avatar
    public const int ActionMessageComposer = 1631; //179
    public const int SleepMessageComposer = 1797; //3852
    public const int DanceMessageComposer = 2233; //845
    public const int CarryObjectMessageComposer = 1474; //2623
    public const int AvatarEffectMessageComposer = 1167; //2662

    // Room Chat
    public const int ChatMessageComposer = 1446; //3821
    public const int ShoutMessageComposer = 1036; //909
    public const int WhisperMessageComposer = 2704; //2280
    public const int FloodControlMessageComposer = 566; //1197
    public const int UserTypingMessageComposer = 1717; //2854

    // Room Engine
    public const int UsersMessageComposer = 374; //2422
    public const int FurnitureAliasesMessageComposer = 2856; //81
    public const int ObjectAddMessageComposer = 3399; //505
    public const int ObjectsMessageComposer = 646; //3521
    public const int ObjectUpdateMessageComposer = 2286; //273
    public const int ObjectRemoveMessageComposer = 2411; //85
    public const int SlideObjectBundleMessageComposer = 3639; //11437
    public const int ItemsMessageComposer = 2779; //2335
    public const int ItemAddMessageComposer = 2448; //1841
    public const int ItemUpdateMessageComposer = 2034; //2933
    public const int ItemRemoveMessageComposer = 2514; //762

    // Room Session
    public const int RoomForwardMessageComposer = 1954; //1963
    public const int RoomReadyMessageComposer = 3221; //2029
    public const int OpenConnectionMessageComposer = 1856; //1329
    public const int CloseConnectionMessageComposer = 168; //1898
    public const int FlatAccessibleMessageComposer = 499; //1179
    public const int CantConnectMessageComposer = 2039; //1864

    // Room Permissions
    public const int YouAreControllerMessageComposer = 780; //1425
    public const int YouAreNotControllerMessageComposer = 2392; //1202
    public const int YouAreOwnerMessageComposer = 339; //495

    // Room Settings
    public const int RoomSettingsDataMessageComposer = 1498; //633
    public const int RoomSettingsSavedMessageComposer = 948; //3737
    public const int FlatControllerRemovedMessageComposer = 1327; //1205
    public const int FlatControllerAddedMessageComposer = 2088; //1056
    public const int RoomRightsListMessageComposer = 1284; //2410

    // Room Furniture
    public const int HideWiredConfigMessageComposer = 1155; //3715
    public const int WiredEffectConfigMessageComposer = 1434; //1469
    public const int WiredConditionConfigMessageComposer = 1108; //1456
    public const int WiredTriggerConfirmMessageComposer = 383; //1618
    public const int MoodlightConfigMessageComposer = 2710; //1964
    public const int GroupFurniSettingsMessageComposer = 3293; //613
    public const int OpenGiftMessageComposer = 56; //1375

    // Navigator
    public const int UpdateFavouriteRoomMessageComposer = 2524; //854
    public const int NavigatorLiftedRoomsMessageComposer = 3104; //761
    public const int NavigatorPreferencesMessageComposer = 518; //1430
    public const int NavigatorFlatCatsMessageComposer = 3244; //1109
    public const int NavigatorMetaDataParserMessageComposer = 3052; //371
    public const int NavigatorCollapsedCategoriesMessageComposer = 1543; //1263

    // Messenger
    public const int BuddyListMessageComposer = 3130; //3394
    public const int BuddyRequestsMessageComposer = 2219; //2757
    public const int NewBuddyRequestMessageComposer = 2219; //2981

    // Moderation
    public const int ModeratorInitMessageComposer = 2696; //2545
    public const int ModeratorUserRoomVisitsMessageComposer = 1752; //1101
    public const int ModeratorRoomChatlogMessageComposer = 3434; //1362
    public const int ModeratorUserInfoMessageComposer = 2866; //289
    public const int ModeratorSupportTicketResponseMessageComposer = 934; //3927
    public const int ModeratorUserChatlogMessageComposer = 3377; //3308
    public const int ModeratorRoomInfoMessageComposer = 1333; //13
    public const int ModeratorSupportTicketMessageComposer = 3609; //1275
    public const int ModeratorTicketChatlogMessageComposer = 607; //766
    public const int CallForHelpPendingCallsMessageComposer = 1121;
    public const int CfhTopicsInitMessageComposer = 325;

    // Inventory
    public const int CreditBalanceMessageComposer = 3475; //3604
    public const int BadgesMessageComposer = 717; //154
    public const int FurniListAddMessageComposer = 104; //176
    public const int FurniListNotificationMessageComposer = 2103; //2725
    public const int FurniListRemoveMessageComposer = 159; //1903
    public const int FurniListMessageComposer = 994; //2183
    public const int FurniListUpdateMessageComposer = 3151; //506
    public const int AvatarEffectsMessageComposer = 340; //3310
    public const int AvatarEffectActivatedMessageComposer = 1959; //1710
    public const int AvatarEffectExpiredMessageComposer = 2228; //68
    public const int AvatarEffectAddedMessageComposer = 3315; //2760
    public const int TradingErrorMessageComposer = 217; //2876
    public const int TradingAcceptMessageComposer = 2568; //1367
    public const int TradingStartMessageComposer = 2505; //2290
    public const int TradingUpdateMessageComposer = 2024; //2277
    public const int TradingClosedMessageComposer = 1373; //2068
    public const int TradingCompleteMessageComposer = 2720; //1959
    public const int TradingConfirmedMessageComposer = 2568; //1367
    public const int TradingFinishMessageComposer = 1001; //2369

    // Inventory Achievements
    public const int AchievementsMessageComposer = 305; //509
    public const int AchievementScoreMessageComposer = 1968; //3710
    public const int AchievementUnlockedMessageComposer = 806; //1887
    public const int AchievementProgressedMessageComposer = 2107; //305

    // Notifications
    public const int ActivityPointsMessageComposer = 2018; //1911
    public const int HabboActivityPointNotificationMessageComposer = 2275; //606

    // Users
    public const int ScrSendUserInfoMessageComposer = 954; //2811
    public const int IgnoredUsersMessageComposer = 126;

    // Groups
    public const int UnknownGroupMessageComposer = 2445; //1T981
    public const int GroupMembershipRequestedMessageComposer = 1180; //423
    public const int ManageGroupMessageComposer = 3965; //2653
    public const int HabboGroupBadgesMessageComposer = 2402; //2487
    public const int NewGroupInfoMessageComposer = 2808; //1095
    public const int GroupInfoMessageComposer = 1702; //3160
    public const int GroupCreationWindowMessageComposer = 2159; //1232
    public const int SetGroupIdMessageComposer = 1459; //3197
    public const int GroupMembersMessageComposer = 1200; //2297
    public const int UpdateFavouriteGroupMessageComposer = 3403; //3685
    public const int GroupMemberUpdatedMessageComposer = 265; //2954
    public const int RefreshFavouriteGroupMessageComposer = 876; //382

    // Group Forums
    public const int ForumsListDataMessageComposer = 3001; //3596
    public const int ForumDataMessageComposer = 3011; //254
    public const int ThreadCreatedMessageComposer = 1862; //3683
    public const int ThreadDataMessageComposer = 509; //879
    public const int ThreadsListDataMessageComposer = 1073; //1538
    public const int ThreadUpdatedMessageComposer = 2528; //3226
    public const int ThreadReplyMessageComposer = 2049; //1936

    // Sound
    public const int SoundSettingsMessageComposer = 513; //2921

    public const int QuestionParserMessageComposer = 2665; //1719
    public const int AvatarAspectUpdateMessageComposer = 2429;
    public const int HelperToolMessageComposer = 1548; //224
    public const int RoomErrorNotifMessageComposer = 2913; //444
    public const int FollowFriendFailedMessageComposer = 3048; //1170

    public const int FindFriendsProcessResultMessageComposer = 1210; //3763
    public const int UserChangeMessageComposer = 3920; //32
    public const int FloorHeightMapMessageComposer = 607; //1112
    public const int RoomInfoUpdatedMessageComposer = 3297; //3833
    public const int MessengerErrorMessageComposer = 892; //915
    public const int MarketplaceCanMakeOfferResultMessageComposer = 54; //1874
    public const int GameAccountStatusMessageComposer = 2893; //139
    public const int GuestRoomSearchResultMessageComposer = 52; //43
    public const int NewUserExperienceGiftOfferMessageComposer = 91; //1904
    public const int UpdateUsernameMessageComposer = 118; //3801
    public const int VoucherRedeemOkMessageComposer = 3336; //3432
    public const int FigureSetIdsMessageComposer = 1450; //3469
    public const int StickyNoteMessageComposer = 2202; //2338
    public const int UserRemoveMessageComposer = 2661; //2841
    public const int GetGuestRoomResultMessageComposer = 687; //2224
    public const int DoorbellMessageComposer = 2309; //162

    public const int GiftWrappingConfigurationMessageComposer = 2234; //3348
    public const int GetRelationshipsMessageComposer = 2016; //1589
    public const int FriendNotificationMessageComposer = 3082; //1211
    public const int BadgeEditorPartsMessageComposer = 2238; //2519
    public const int TraxSongInfoMessageComposer = 3365; //523
    public const int PostUpdatedMessageComposer = 324; //1752
    public const int UserUpdateMessageComposer = 1640; //3153
    public const int MutedMessageComposer = 826; //229
    public const int MarketplaceConfigurationMessageComposer = 1823; //3702
    public const int CheckGnomeNameMessageComposer = 546; //2491
    public const int OpenBotActionMessageComposer = 1618; //895
    public const int FavouritesMessageComposer = 151; //604
    public const int TalentLevelUpMessageComposer = 638; //3538

    public const int BcBorrowedItemsMessageComposer = 3828; //3424
    public const int UserTagsMessageComposer = 1255; //774
    public const int CampaignMessageComposer = 1745; //3234
    public const int RoomEventMessageComposer = 1840; //2274
    public const int MarketplaceItemStatsMessageComposer = 725; //2909
    public const int HabboSearchResultMessageComposer = 973; //214
    public const int PetHorseFigureInformationMessageComposer = 1924; //560
    public const int PetInventoryMessageComposer = 3522; //3528
    public const int PongMessageComposer = 3928; //624
    public const int RentableSpaceMessageComposer = 3559; //2660
    public const int GetYouTubePlaylistMessageComposer = 1112; //763
    public const int RespectNotificationMessageComposer = 2815; //474
    public const int RecyclerRewardsMessageComposer = 3164; //2457
    public const int GetRoomBannedUsersMessageComposer = 1869; //3580
    public const int RoomRatingMessageComposer = 482; //3464
    public const int PlayableGamesMessageComposer = 3805; //549
    public const int TalentTrackLevelMessageComposer = 1203; //2382
    public const int JoinQueueMessageComposer = 2260; //749
    public const int MarketPlaceOwnOffersMessageComposer = 3884; //2806
    public const int PetBreedingMessageComposer = 634; //616
    public const int SubmitBullyReportMessageComposer = 2674; //453
    public const int UserNameChangeMessageComposer = 2182; //2587
    public const int LoveLockDialogueMessageComposer = 3753; //173
    public const int SendBullyReportMessageComposer = 3463; //2094
    public const int VoucherRedeemErrorMessageComposer = 714; //3670
    public const int PurchaseErrorMessageComposer = 1404; //3016
    public const int UnknownCalendarMessageComposer = 2551; //1799
    public const int FriendListUpdateMessageComposer = 2800; //1611

    public const int UserFlatCatsMessageComposer = 1562; //377
    public const int UpdateFreezeLivesMessageComposer = 2376; //1395
    public const int UnbanUserFromRoomMessageComposer = 3429; //3472
    public const int PetTrainingPanelMessageComposer = 1164; //1067
    public const int LoveLockDialogueCloseMessageComposer = 382; //1534
    public const int BuildersClubMembershipMessageComposer = 1452; //2357
    public const int FlatAccessDeniedMessageComposer = 878; //1582
    public const int LatencyResponseMessageComposer = 10; //3014
    public const int HabboUserBadgesMessageComposer = 1087; //1123
    public const int HeightMapMessageComposer = 1268; //207

    public const int CanCreateRoomMessageComposer = 378; //1237
    public const int InstantMessageErrorMessageComposer = 3359; //2964
    public const int GnomeBoxMessageComposer = 2380; //1778
    public const int IgnoreStatusMessageComposer = 207; //3882
    public const int PetInformationMessageComposer = 2901; //3913
    public const int NavigatorSearchResultSetMessageComposer = 2690; //815
    public const int ConcurrentUsersGoalProgressMessageComposer = 2737; //2955
    public const int VideoOffersRewardsMessageComposer = 2125; //1896
    public const int SanctionStatusMessageComposer = 2221; //193
    public const int GetYouTubeVideoMessageComposer = 1411; //2374
    public const int CheckPetNameMessageComposer = 1503; //3019
    public const int RespectPetNotificationMessageComposer = 2788; //3637
    public const int EnforceCategoryUpdateMessageComposer = 3896; //315
    public const int CommunityGoalHallOfFameMessageComposer = 2515; //690
    public const int FloorPlanFloorMapMessageComposer = 3990; //2337
    public const int SendGameInvitationMessageComposer = 904; //1165
    public const int GiftWrappingErrorMessageComposer = 1517; //2534
    public const int PromoArticlesMessageComposer = 286; //3565
    public const int GameWeeklyLeaderboardMessageComposer = 2196; //3124
    public const int RentableSpacesErrorMessageComposer = 1868; //838
    public const int AddExperiencePointsMessageComposer = 2156; //3779
    public const int OpenHelpToolMessageComposer = 1121; //3831
    public const int GetRoomFilterListMessageComposer = 2937; //2169
    public const int GameAchievementListMessageComposer = 2265; //1264
    public const int PromotableRoomsMessageComposer = 2468; //2166
    public const int FloorPlanSendDoorMessageComposer = 1664; //2180
    public const int RoomEntryInfoMessageComposer = 749; //3378
    public const int RoomNotificationMessageComposer = 1992; //2419
    public const int ClubGiftsMessageComposer = 619; //1549
    public const int MotdNotificationMessageComposer = 2035; //1829
    public const int PopularRoomTagsResultMessageComposer = 2012; //234
    public const int NewConsoleMessageMessageComposer = 1587; //2121
    public const int RoomPropertyMessageComposer = 2454; //1328
    public const int MarketPlaceOffersMessageComposer = 680; //2985
    public const int TalentTrackMessageComposer = 3406; //3614
    public const int ProfileInformationMessageComposer = 3898; //3872
    public const int BadgeDefinitionsMessageComposer = 2501; //2066
    public const int Game2WeeklyLeaderboardMessageComposer = 2196; //1127
    public const int NameChangeUpdateMessageComposer = 563; //2698
    public const int RoomVisualizationSettingsMessageComposer = 3547; //3786
    public const int MarketplaceMakeOfferResultMessageComposer = 1359; //3960
    public const int FlatCreatedMessageComposer = 1304; //1621
    public const int BotInventoryMessageComposer = 3086; //2620
    public const int LoadGameMessageComposer = 3654; //1403
    public const int UpdateMagicTileMessageComposer = 233; //2641
    public const int MaintenanceStatusMessageComposer = 1350; //3198
    public const int Game3WeeklyLeaderboardMessageComposer = 2196; //2194
    public const int GameListMessageComposer = 222; //2481
    public const int RoomMuteSettingsMessageComposer = 2533; //257
    public const int RoomInviteMessageComposer = 3870; //3942
    public const int LoveLockDialogueSetLockedMessageComposer = 382; //1534
    public const int BroadcastMessageAlertMessageComposer = 3801; //1279
    public const int MarketplaceCancelOfferResultMessageComposer = 3264; //202
    public const int NavigatorSettingsMessageComposer = 2875; //3175

    public const int MessengerInitMessageComposer = 1605; //391
}