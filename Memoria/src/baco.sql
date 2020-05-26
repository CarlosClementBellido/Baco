CREATE DATABASE baco;
USE baco;

CREATE TABLE Users (
    id INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
    nick VARCHAR(16) NOT NULL,
    mail VARCHAR(255) NOT NULL,
    pass_hash VARCHAR(60) NOT NULL
);

CREATE TABLE Friends (
	id INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
    id_petitioner INT NOT NULL,
    id_acceptor INT NOT NULL,
	accepted BIT DEFAULT NULL,
    FOREIGN KEY (id_petitioner) 
		REFERENCES Users(id),
	FOREIGN KEY (id_acceptor) 
		REFERENCES Users(id) 
);

CREATE TABLE RSSChannels (
    id INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
    rss VARCHAR(64) NOT NULL,
    name VARCHAR(24) NOT NULL
);

CREATE TABLE RSSChannelSubscriptions (
	id INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
    id_rsschannel INT NOT NULL,
    id_user INT NOT NULL,
    FOREIGN KEY (id_rsschannel) 
		REFERENCES RSSChannels(id),
	FOREIGN KEY (id_user) 
		REFERENCES Users(id) 
);

CREATE TABLE Groups (
    id INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
    name VARCHAR(24) NOT NULL
);

CREATE TABLE GroupsRelations (
	id INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
    id_group INT NOT NULL,
    id_user INT NOT NULL,
    FOREIGN KEY (id_group) 
		REFERENCES Groups(id),
	FOREIGN KEY (id_user) 
		REFERENCES Users(id) 
);

INSERT INTO Users (nick, mail, pass_hash)
VALUES 
('nick1', 'mail1', 'nIdNeuo3T99bvMKcHSdKOedQ9rPcNAptEErOt26PuxE='),
('nick2', 'mail2', 'nIdNeuo3T99bvMKcHSdKOedQ9rPcNAptEErOt26PuxE='),
('nick3', 'mail3', 'nIdNeuo3T99bvMKcHSdKOedQ9rPcNAptEErOt26PuxE='),
('nick4', 'mail4', 'nIdNeuo3T99bvMKcHSdKOedQ9rPcNAptEErOt26PuxE='),
('nick5', 'mail5', 'nIdNeuo3T99bvMKcHSdKOedQ9rPcNAptEErOt26PuxE='),
('nick6', 'mail6', 'nIdNeuo3T99bvMKcHSdKOedQ9rPcNAptEErOt26PuxE='),
('nick7', 'mail7', 'nIdNeuo3T99bvMKcHSdKOedQ9rPcNAptEErOt26PuxE=');

INSERT INTO Friends (id_petitioner, id_acceptor, accepted)
VALUES 
(1, 3, 1),
(2, 1, 1),
(1, 4, 1);

INSERT INTO RSSChannels (rss, name)
VALUES 
('http://feeds.feedburner.com/naukas/danielmarin', 'Eureka'),
('https://e00-elmundo.uecdn.es/elmundo/rss/portada.xml', 'Portada // elmundo');

INSERT INTO RSSChannelSubscriptions (id_rsschannel, id_user)
VALUES 
(1, 1),
(2, 1),
(2, 2);

INSERT INTO Groups (name)
VALUES 
('Group1'),
('Group2');

INSERT INTO GroupsRelations (id_group, id_user)
VALUES 
(1, 1),
(1, 2),
(1, 3),
(2, 2);
(2, 1),
(2, 4),
(2, 5);