CREATE TABLE `Tasks` (
 `Id` varchar(38) NOT NULL,
 `Type` text NOT NULL,
 `CreationTime` bigint(20) NOT NULL,
 `UpdateTime` bigint(20) NOT NULL,
 `Data` longtext NOT NULL,
 `Status` text NOT NULL,
 `LinkedObject` text NOT NULL,
 PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4