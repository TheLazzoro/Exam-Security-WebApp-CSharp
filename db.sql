create schema `exam-security`;
use `exam-security`;

create table db_user (
	id int NOT NULL,
    username varchar(255) NOT NULL,
    password varchar(255) NOT NULL,
	PRIMARY KEY (id)
);