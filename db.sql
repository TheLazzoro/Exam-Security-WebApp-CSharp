create schema `exam-security`;
use `exam-security`;

create table db_user (
	id int NOT NULL AUTO_INCREMENT,
    username varchar(255) NOT NULL,
    passwd varchar(255) NOT NULL,
	PRIMARY KEY (id)
);