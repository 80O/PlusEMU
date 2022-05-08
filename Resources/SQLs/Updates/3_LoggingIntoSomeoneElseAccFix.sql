DROP TABLE IF EXISTS `user_auth_ticket`;
CREATE TABLE `user_auth_ticket` (
`user_id` int(11) NOT NULL,
`auth_ticket` varchar(60) NOT NULL,
PRIMARY KEY (`user_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;