CREATE TABLE `Users` (
 `Id` varchar(38) NOT NULL,
 `TelegramId` bigint(20) NOT NULL,
 `CreationTime` bigint(20) NOT NULL,
 `LastAccessTime` bigint(20) NOT NULL,
 `Claims` longtext NOT NULL,
 `Data` longtext NOT NULL,
 `Status` text NOT NULL,
 PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4