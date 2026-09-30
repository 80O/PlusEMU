CREATE TABLE IF NOT EXISTS `catalog_club_offers` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(100) NOT NULL,
  `days` int(11) NOT NULL,
  `credits` int(11) NOT NULL DEFAULT 0,
  `points` int(11) NOT NULL DEFAULT 0,
  `points_type` int(11) NOT NULL DEFAULT 0,
  `type` enum('basic','vip') NOT NULL DEFAULT 'basic',
  `deal` enum('0','1') NOT NULL DEFAULT '0',
  `enabled` enum('0','1') NOT NULL DEFAULT '1',
  PRIMARY KEY (`id`),
  KEY `enabled_days` (`enabled`, `days`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
