CREATE TABLE `ambassador_logs` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `user_id` int(11),
  `target` varchar(50) NOT NULL DEFAULT '',
  `sanctions_type` text NOT NULL,
  `timestamp` double NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  UNIQUE KEY `id` (`id`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=latin1;