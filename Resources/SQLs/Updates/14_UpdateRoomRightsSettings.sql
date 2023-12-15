ALTER TABLE `rooms` 
    MODIFY COLUMN `mute_settings` int NOT NULL DEFAULT 1 AFTER `group_id`,
    MODIFY COLUMN `ban_settings` int NOT NULL DEFAULT 1 AFTER `mute_settings`,
    MODIFY COLUMN `kick_settings` int NOT NULL DEFAULT 1 AFTER `ban_settings`;