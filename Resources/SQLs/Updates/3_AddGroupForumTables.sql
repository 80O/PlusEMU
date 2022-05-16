DROP TABLE IF EXISTS `group_forums_settings`;
CREATE TABLE IF NOT EXISTS `group_forums_settings` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `group_id` int(11) NOT NULL,
  `who_can_read` int(11) NOT NULL DEFAULT 0,
  `who_can_post` int(11) NOT NULL DEFAULT 0,
  `who_can_init_discussions` int(11) NOT NULL DEFAULT 0,
  `who_can_mod` int(11) NOT NULL DEFAULT 3,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;

DROP TABLE IF EXISTS `group_forums_threads`;
CREATE TABLE IF NOT EXISTS `group_forums_threads` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `user_id` int(11) NOT NULL,
  `timestamp` int(11) DEFAULT NULL,
  `caption` varchar(255) NOT NULL,
  `pinned` tinyint(4) NOT NULL DEFAULT 0,
  `locked` tinyint(4) NOT NULL DEFAULT 0,
  `deleted_level` int(11) NOT NULL DEFAULT 0,
  `deleter_user_id` int(11) NOT NULL DEFAULT 0,
  `forum_id` int(11) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;

DROP TABLE IF EXISTS `group_forums_thread_posts`;
CREATE TABLE IF NOT EXISTS `group_forums_thread_posts` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `user_id` int(11) NOT NULL,
  `timestamp` int(11) DEFAULT NULL,
  `message` varchar(255) NOT NULL,
  `deleted_level` int(11) NOT NULL DEFAULT 0,
  `deleter_user_id` int(11) NOT NULL DEFAULT 0,
  `thread_id` int(11) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;

DROP TABLE IF EXISTS `group_forums_thread_views`;
CREATE TABLE IF NOT EXISTS `group_forums_thread_views` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `thread_id` int(11) NOT NULL,
  `timestamp` int(11) DEFAULT 0,
  `user_id` int(11) NOT NULL,
  `count` int(11) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;