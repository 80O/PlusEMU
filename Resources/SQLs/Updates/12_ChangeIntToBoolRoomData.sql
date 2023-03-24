-- Rename old columns
ALTER TABLE `rooms` 
    CHANGE `allow_pets` `allow_pets_old` ENUM('0','1') NOT NULL DEFAULT '0',
    CHANGE `allow_pets_eat` `allow_pets_eat_old` ENUM('0','1') NOT NULL DEFAULT '0',
    CHANGE `room_blocking_disabled` `room_blocking_disabled_old` ENUM('0','1') NOT NULL DEFAULT '0',
    CHANGE `allow_hidewall` `allow_hidewall_old` ENUM('0','1') NOT NULL DEFAULT '0';

-- Add new columns with boolean type
ALTER TABLE `rooms`
    ADD `allow_pets` BOOLEAN NOT NULL DEFAULT 0,
    ADD `allow_pets_eat` BOOLEAN NOT NULL DEFAULT 0,
    ADD `room_blocking_disabled` BOOLEAN NOT NULL DEFAULT 0,
    ADD `allow_hidewall` BOOLEAN NOT NULL DEFAULT 0;

-- Update the new columns with the data from the old columns
UPDATE `rooms`
SET
    `allow_pets` = `allow_pets_old` = '1',
    `allow_pets_eat` = `allow_pets_eat_old` = '1',
    `room_blocking_disabled` = `room_blocking_disabled_old` = '1',
    `allow_hidewall` = `allow_hidewall_old` = '1';

-- Delete old columns
ALTER TABLE `rooms`
    DROP `allow_pets_old`,
    DROP `allow_pets_eat_old`,
    DROP `room_blocking_disabled_old`,
    DROP `allow_hidewall_old`;
